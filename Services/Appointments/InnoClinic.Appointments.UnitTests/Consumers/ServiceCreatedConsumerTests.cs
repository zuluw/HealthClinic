using AutoFixture;
using FluentAssertions;
using InnoClinic.Appointments.Application.Consumers;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace InnoClinic.Appointments.UnitTests.Consumers
{
    public class ServiceCreatedConsumerTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<ILogger<ServiceCreatedConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<ServiceCreatedEvent>> _contextMock;
        private readonly IFixture _fixture;
        private readonly ServiceCreatedConsumer _sut;

        public ServiceCreatedConsumerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<ServiceCreatedConsumer>>();
            _contextMock = new Mock<ConsumeContext<ServiceCreatedEvent>>();

            _sut = new ServiceCreatedConsumer(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_WhenSpecializationNotFound_ShouldAddSpecializationAndService()
        {
            // Arrange
            var message = _fixture.Create<ServiceCreatedEvent>();
            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetSpecializationByIdAsync(message.SpecializationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Specialization?)null);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            _repositoryMock.Verify(r => r.AddSpecialization(It.Is<Specialization>(s =>
                s.Id == message.SpecializationId &&
                s.Name == "New Specialization")), Times.Once);
            _repositoryMock.Verify(r => r.AddService(It.Is<Service>(s =>
                s.Id == message.Id &&
                s.Name == message.Name &&
                s.Price == message.Price &&
                s.SpecializationId == message.SpecializationId)), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenSpecializationExists_ShouldOnlyAddService()
        {
            // Arrange
            var message = _fixture.Create<ServiceCreatedEvent>();
            var existingSpec = _fixture.Create<Specialization>();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetSpecializationByIdAsync(message.SpecializationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSpec);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            _repositoryMock.Verify(r => r.AddSpecialization(It.IsAny<Specialization>()), Times.Never);
            _repositoryMock.Verify(r => r.AddService(It.IsAny<Service>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}