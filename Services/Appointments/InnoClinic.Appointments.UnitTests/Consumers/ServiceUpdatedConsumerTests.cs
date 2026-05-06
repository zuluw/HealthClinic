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
    public class ServiceUpdatedConsumerTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<ILogger<ServiceUpdatedConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<ServiceUpdatedEvent>> _contextMock;
        private readonly IFixture _fixture;
        private readonly ServiceUpdatedConsumer _sut;

        public ServiceUpdatedConsumerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<ServiceUpdatedConsumer>>();
            _contextMock = new Mock<ConsumeContext<ServiceUpdatedEvent>>();

            _sut = new ServiceUpdatedConsumer(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_WhenServiceExists_ShouldUpdateAllFieldsAndSaveChanges()
        {
            // Arrange
            var message = _fixture.Create<ServiceUpdatedEvent>();
            var existingService = _fixture.Create<Service>();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetServiceByIdSyncAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingService);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            existingService.Name.Should().Be(message.Name);
            existingService.Price.Should().Be(message.Price);
            existingService.IsActive.Should().Be(message.IsActive);

            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenServiceDoesNotExist_ShouldNotAttemptUpdate()
        {
            // Arrange
            var message = _fixture.Create<ServiceUpdatedEvent>();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetServiceByIdSyncAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}