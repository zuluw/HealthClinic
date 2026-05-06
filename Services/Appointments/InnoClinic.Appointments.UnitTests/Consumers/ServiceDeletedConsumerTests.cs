using AutoFixture;
using InnoClinic.Appointments.Application.Consumers;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace InnoClinic.Appointments.UnitTests.Consumers
{
    public class ServiceDeletedConsumerTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<ILogger<ServiceDeletedConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<ServiceDeletedEvent>> _contextMock;
        private readonly IFixture _fixture;
        private readonly ServiceDeletedConsumer _sut;

        public ServiceDeletedConsumerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<ServiceDeletedConsumer>>();
            _contextMock = new Mock<ConsumeContext<ServiceDeletedEvent>>();

            _sut = new ServiceDeletedConsumer(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_WhenServiceExists_ShouldDeleteServiceAndSaveChanges()
        {
            // Arrange
            var message = _fixture.Create<ServiceDeletedEvent>();
            var existingService = _fixture.Create<Service>();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetServiceByIdSyncAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingService);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            _repositoryMock.Verify(r => r.DeleteService(existingService), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenServiceDoesNotExist_ShouldDoNothing()
        {
            // Arrange
            var message = _fixture.Create<ServiceDeletedEvent>();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetServiceByIdSyncAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            _repositoryMock.Verify(r => r.DeleteService(It.IsAny<Service>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}