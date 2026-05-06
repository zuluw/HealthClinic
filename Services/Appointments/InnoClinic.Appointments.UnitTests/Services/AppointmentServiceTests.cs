using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentValidation;
using ValidationResult = FluentValidation.Results.ValidationResult;
using FluentValidation.Results;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Application.Services;
using InnoClinic.Appointments.Domain.Exceptions;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace InnoClinic.Appointments.UnitTests.Services
{
    public class AppointmentServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<ILogger<AppointmentService>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly Mock<IValidator<AppointmentRequestDTO>> _validatorMock;
        private readonly Mock<IValidator<ResultRequestDTO>> _resultValidatorMock;
        private readonly Mock<IValidator<UpdateResultRequestDTO>> _updateResultValidatorMock;
        private readonly Mock<IDocumentsClient> _documentsClientMock;

        private readonly AppointmentService _sut;

        public AppointmentServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)));

            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<AppointmentService>>();
            _publishMock = new Mock<IPublishEndpoint>();

            _validatorMock = new Mock<IValidator<AppointmentRequestDTO>>();
            _resultValidatorMock = new Mock<IValidator<ResultRequestDTO>>();
            _updateResultValidatorMock = new Mock<IValidator<UpdateResultRequestDTO>>();
            _documentsClientMock = new Mock<IDocumentsClient>();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["AppointmentSettings:TimeFormat"]).Returns("HH:mm");
            configMock.Setup(c => c.GetSection("AppointmentSettings:DefaultDurationMinutes").Value).Returns("20");
            var mapper = new AppointmentMapper(configMock.Object);

            _sut = new AppointmentService(
                _repositoryMock.Object,
                _loggerMock.Object,
                mapper,
                _validatorMock.Object,
                _resultValidatorMock.Object,
                _updateResultValidatorMock.Object,
                _documentsClientMock.Object,
                _publishMock.Object
            );
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenIsValid_ShouldReturnResponseDto()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();
            var patient = _fixture.Create<Patient>();
            var doctor = _fixture.Create<Doctor>();
            var service = _fixture.Create<Service>();

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(request.DoctorId, request.DateTime, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.GetPatientByIdAsync(request.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);
            _repositoryMock.Setup(r => r.GetDoctorByIdAsync(request.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            _repositoryMock.Setup(r => r.GetServiceByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act
            var result = await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ServiceId.Should().Be(request.ServiceId);

            _repositoryMock.Verify(r => r.AddAppointment(It.IsAny<Appointment>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<AppointmentCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenValidationFails_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();
            var failures = new List<ValidationFailure> { new("Property", "Error") };

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            Func<Task> act = async () => await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenSlotIsBusy_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenPatientNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.GetPatientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenDoctorNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.GetPatientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Patient>());
            _repositoryMock.Setup(r => r.GetDoctorByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAppointmentAsync_WhenServiceNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = _fixture.Create<AppointmentRequestDTO>();

            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.GetPatientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Patient>());
            _repositoryMock.Setup(r => r.GetDoctorByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Doctor>());
            _repositoryMock.Setup(r => r.GetServiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateAppointmentAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ApproveAppointmentAsync_WhenAppointmentExists_ShouldSetConfirmed()
        {
            // Arrange
            var appointment = _fixture.Create<Appointment>();
            appointment.IsConfirmed = false;

            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act
            await _sut.ApproveAppointmentAsync(appointment.Id, CancellationToken.None);

            // Assert
            appointment.IsConfirmed.Should().BeTrue();
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApproveAppointmentAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act
            Func<Task> act = async () => await _sut.ApproveAppointmentAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task RescheduleAppointmentAsync_WhenValid_ShouldUpdateDateTime()
        {
            // Arrange
            var appointment = _fixture.Create<Appointment>();
            var newDateTime = DateTime.UtcNow.AddDays(2);

            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(appointment.DoctorId, newDateTime, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await _sut.RescheduleAppointmentAsync(appointment.Id, newDateTime, CancellationToken.None);

            // Assert
            appointment.DateTime.Should().BeCloseTo(newDateTime, TimeSpan.FromSeconds(1));
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RescheduleAppointmentAsync_WhenNewSlotIsBusy_ShouldThrowBadRequestException()
        {
            // Arrange
            var appointment = _fixture.Create<Appointment>();
            var newDateTime = DateTime.UtcNow.AddDays(1);

            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);
            _repositoryMock.Setup(r => r.IsSlotBusyAsync(appointment.DoctorId, newDateTime, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.RescheduleAppointmentAsync(appointment.Id, newDateTime, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task RescheduleAppointmentAsync_WhenAppointmentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act
            Func<Task> act = async () => await _sut.RescheduleAppointmentAsync(id, DateTime.UtcNow, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenExists_ShouldDeleteAndSave()
        {
            // Arrange
            var appointment = _fixture.Create<Appointment>();
        
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            // Act
            await _sut.CancelAppointmentAsync(appointment.Id, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAppointment(appointment), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act
            Func<Task> act = async () => await _sut.CancelAppointmentAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateResultAsync_WhenFileIsProvided_ShouldUploadFileAndSaveResult()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var request = _fixture.Build<ResultRequestDTO>()
                .With(x => x.File, fileMock.Object)
                .Create();

            var appointment = _fixture.Create<Appointment>();
            var documentId = Guid.NewGuid();

            _resultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);
            _repositoryMock.Setup(r => r.ResultExistsAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _documentsClientMock.Setup(d => d.UploadDocumentAsync(request.File!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(documentId);
            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Result>());

            // Act
            await _sut.CreateResultAsync(request, CancellationToken.None);

            // Assert
            _documentsClientMock.Verify(d => d.UploadDocumentAsync(request.File!, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.AddResult(It.Is<Result>(res => res.DocumentId == documentId)), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<ResultCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateResultAsync_WhenNoFile_ShouldSaveResultWithNullDocumentId()
        {
            // Arrange
            var request = new ResultRequestDTO(
                AppointmentId: Guid.NewGuid(),
                Complaints: "Complaints",
                Conclusion: "Conclusion",
                Diagnosis: "Diagnosis",
                Recommendations: "Recommendations",
                File: null
            );

            _resultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Appointment>());
            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Result>());

            // Act
            await _sut.CreateResultAsync(request, CancellationToken.None);

            // Assert
            _documentsClientMock.Verify(d => d.UploadDocumentAsync(It.IsAny<IFormFile>()!, It.IsAny<CancellationToken>()), Times.Never);
            _repositoryMock.Verify(r => r.AddResult(It.Is<Result>(res => res.DocumentId == null)), Times.Once);
        }

        [Fact]
        public async Task UpdateResultAsync_WhenValid_ShouldUpdateAndPublish()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = _fixture.Create<UpdateResultRequestDTO>();
            var resultEntity = _fixture.Create<Result>();

            _updateResultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetResultByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultEntity);
            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultEntity);

            // Act
            await _sut.UpdateResultAsync(id, request, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<ResultUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateResultAsync_WhenValidationFails_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = _fixture.Create<ResultRequestDTO>();
            var failures = new List<ValidationFailure> { new("Property", "Error") };
            
            _resultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            Func<Task> act = async () => await _sut.CreateResultAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CreateResultAsync_WhenAppointmentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = _fixture.Create<ResultRequestDTO>();
            
            _resultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateResultAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateResultAsync_WhenResultAlreadyExists_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = _fixture.Create<ResultRequestDTO>();

            _resultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetAppointmentByIdAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Appointment>());
            _repositoryMock.Setup(r => r.ResultExistsAsync(request.AppointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.CreateResultAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task UpdateResultAsync_WhenValidationFails_ShouldThrowBadRequestException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = _fixture.Create<UpdateResultRequestDTO>();
            var failures = new List<ValidationFailure> { new("Property", "Error") };
            
            _updateResultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            Func<Task> act = async () => await _sut.UpdateResultAsync(id, request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task UpdateResultAsync_WhenResultNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = _fixture.Create<UpdateResultRequestDTO>();
            
            _updateResultValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetResultByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdateResultAsync(id, request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GenerateResultPdfAsync_WhenResultNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result?)null);

            // Act
            Func<Task> act = async () => await _sut.GenerateResultPdfAsync(id, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GenerateResultPdfAsync_WhenResultExists_ShouldReturnByteArray()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var resultEntity = _fixture.Create<Result>();

            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultEntity);

            // Act
            var pdfBytes = await _sut.GenerateResultPdfAsync(appointmentId, CancellationToken.None);

            // Assert
            pdfBytes.Should().NotBeNull();
            pdfBytes.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetSpecializationsAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var officeId = "some-office-id";
            var data = _fixture.CreateMany<Specialization>(3).ToList();
            
            _repositoryMock.Setup(r => r.GetSpecializationsAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);

            // Act
            var result = await _sut.GetSpecializationsAsync(officeId, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.First().Name.Should().Be(data.First().Name);
        }

        [Fact]
        public async Task GetDoctorScheduleAsync_ShouldReturnFormattedSchedule()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;
            var appointments = _fixture.CreateMany<Appointment>(2).ToList();

            _repositoryMock.Setup(r => r.GetDoctorScheduleAsync(doctorId, date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            // Act
            var result = await _sut.GetDoctorScheduleAsync(doctorId, date, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            _repositoryMock.Verify(r => r.GetDoctorScheduleAsync(doctorId, date, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPatientHistoryAsync_ShouldReturnHistoryDtos()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var history = _fixture.CreateMany<Appointment>(5).ToList();
            
            _repositoryMock.Setup(r => r.GetPatientHistoryAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Act
            var result = await _sut.GetPatientHistoryAsync(patientId, CancellationToken.None);

            // Assert
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetResultByAppointmentIdAsync_WhenExists_ShouldReturnDto()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var resultEntity = _fixture.Create<Result>();
            
            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultEntity);

            // Act
            var result = await _sut.GetResultByAppointmentIdAsync(appointmentId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Complaints.Should().Be(resultEntity.Complaints);
        }

        [Fact]
        public async Task GetResultByAppointmentIdAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetResultAsync(It.IsAny<Expression<Func<Result, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result?)null);

            // Act
            Func<Task> act = async () => await _sut.GetResultByAppointmentIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetDoctorsAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var specId = Guid.NewGuid();
            var officeId = "office-1";
            var doctors = _fixture.CreateMany<Doctor>(3).ToList();

            _repositoryMock.Setup(r => r.GetDoctorsAsync(specId, officeId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctors);

            // Act
            var result = await _sut.GetDoctorsAsync(specId, officeId, null, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.First().FirstName.Should().Be(doctors.First().FirstName);
            _repositoryMock.Verify(r => r.GetDoctorsAsync(specId, officeId, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetServicesAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var specId = Guid.NewGuid();
            var services = _fixture.CreateMany<Service>(2).ToList();

            _repositoryMock.Setup(r => r.GetServicesAsync(specId, It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _sut.GetServicesAsync(specId, null, null, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be(services.First().Name);
            _repositoryMock.Verify(r => r.GetServicesAsync(specId, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetOfficesAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var specId = Guid.NewGuid();
            var offices = _fixture.CreateMany<Office>(4).ToList();

            _repositoryMock.Setup(r => r.GetOfficesAsync(specId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offices);

            // Act
            var result = await _sut.GetOfficesAsync(specId, CancellationToken.None);

            // Assert
            result.Should().HaveCount(4);
            _repositoryMock.Verify(r => r.GetOfficesAsync(specId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReceptionistAppointmentsAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var doctorName = "House";
            var appointments = _fixture.CreateMany<Appointment>(3).ToList();

            _repositoryMock.Setup(r => r.GetAppointmentsAsync(date, doctorName, It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointments);

            // Act
            var result = await _sut.GetReceptionistAppointmentsAsync(date, doctorName, null, null, null, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            _repositoryMock.Verify(r => r.GetAppointmentsAsync(date, doctorName, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}