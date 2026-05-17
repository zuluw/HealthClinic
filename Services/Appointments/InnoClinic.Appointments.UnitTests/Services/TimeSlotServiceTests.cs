using AutoFixture;
using FluentAssertions;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Services;
using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Exceptions;
using InnoClinic.Appointments.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace InnoClinic.Appointments.UnitTests.Services
{
    public class TimeSlotServiceTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly TimeSlotService _sut;
        private readonly IFixture _fixture;

        public TimeSlotServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)));

            _repositoryMock = new Mock<IAppointmentRepository>();
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(c => c["AppointmentSettings:WorkDayStart"]).Returns("08:00:00");
            _configMock.Setup(c => c["AppointmentSettings:WorkDayEnd"]).Returns("18:00:00");
            _configMock.Setup(c => c.GetSection("AppointmentSettings:SlotSizeMinutes").Value).Returns("10");

            _sut = new TimeSlotService(
                _repositoryMock.Object,
                new Mock<ILogger<TimeSlotService>>().Object,
                _configMock.Object);
        }

        [Theory]
        [InlineData(ServiceCategory.Analyses, 60)]     
        [InlineData(ServiceCategory.Consultations, 59)] 
        [InlineData(ServiceCategory.Diagnostics, 58)]
        public async Task GetAvailableSlotsAsync_BasedOnCategory_ShouldReturnCorrectNumberOfSlots(ServiceCategory category, int expectedCount)
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var date = DateTime.UtcNow.AddDays(1).Date;
            var service = _fixture.Build<Service>().With(s => s.Category, category).Create();

            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);
            _repositoryMock.Setup(r => r.GetAppointmentsByDateAsync(date, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>()); 

            // Act
            var result = await _sut.GetAvailableSlotsAsync(serviceId, date, null, null, CancellationToken.None);

            // Assert
            result.Should().HaveCount(expectedCount);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_WhenSlotIsBusy_ShouldSkip()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var date = DateTime.UtcNow.AddDays(1).Date;
            var service = _fixture.Build<Service>().With(s => s.Category, ServiceCategory.Consultations).Create();

            var busyTime = DateTime.SpecifyKind(date.AddHours(8).AddMinutes(10), DateTimeKind.Utc);
            var existingApp = _fixture.Build<Appointment>()
                .With(a => a.DateTime, busyTime)
                .With(a => a.Service, _fixture.Build<Service>().With(s => s.Category, ServiceCategory.Analyses).Create()) 
                .Create();

            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);
            _repositoryMock.Setup(r => r.GetAppointmentsByDateAsync(date, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment> { existingApp });

            // Act
            var result = await _sut.GetAvailableSlotsAsync(serviceId, date, null, null, CancellationToken.None);

            // Assert
            result.First().TimeLabel.Should().Be("08:20");
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_WhenDateIsToday_ShouldNotReturnPastSlots()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            var service = _fixture.Create<Service>();

            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);
            _repositoryMock.Setup(r => r.GetAppointmentsByDateAsync(today, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            // Act
            var result = await _sut.GetAvailableSlotsAsync(serviceId, today, null, null, CancellationToken.None);

            // Assert
            foreach (var slot in result)
            {
                slot.StartTime.Should().BeAfter(DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_WhenServiceNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
           
            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            // Act
            Func<Task> act = async () => await _sut.GetAvailableSlotsAsync(serviceId, DateTime.UtcNow.AddDays(1), null, null, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Service not found");
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_WhenAllDayIsBusy_ShouldReturnEmptyList()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var date = DateTime.UtcNow.AddDays(1).Date;
            var service = _fixture.Build<Service>().With(s => s.Category, ServiceCategory.Analyses).Create();
            
            var busyAppointments = new List<Appointment>();
            var current = TimeSpan.Parse("08:00:00");
            var end = TimeSpan.Parse("18:00:00");

            while (current < end)
            {
                busyAppointments.Add(_fixture.Build<Appointment>()
                    .With(a => a.DateTime, DateTime.SpecifyKind(date.Add(current), DateTimeKind.Utc))
                    .With(a => a.Service, service)
                    .Create());
                current = current.Add(TimeSpan.FromMinutes(10));
            }

            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
            _repositoryMock.Setup(r => r.GetAppointmentsByDateAsync(date, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(busyAppointments);

            // Act
            var result = await _sut.GetAvailableSlotsAsync(serviceId, date, null, null, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ForDiagnostics_ShouldNotReturnSlotsTooCloseToWorkEnd()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var date = DateTime.UtcNow.AddDays(1).Date;
            
            var service = _fixture.Build<Service>().With(s => s.Category, ServiceCategory.Diagnostics).Create();

            _repositoryMock.Setup(r => r.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);
            _repositoryMock.Setup(r => r.GetAppointmentsByDateAsync(date, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            // Act
            var result = await _sut.GetAvailableSlotsAsync(serviceId, date, null, null, CancellationToken.None);

            // Assert
            result.Select(x => x.TimeLabel).Should().NotContain("17:40");
            result.Select(x => x.TimeLabel).Should().NotContain("17:50");
            result.Last().TimeLabel.Should().Be("17:30");
        }
    }
}