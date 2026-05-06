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
    public class ServiceStatusChangedConsumerTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<ILogger<ServiceStatusChangedConsumer>> _loggerMock;
        private readonly Mock<ConsumeContext<ServiceStatusChangedEvent>> _contextMock;
        private readonly IFixture _fixture;
        private readonly ServiceStatusChangedConsumer _sut;

        public ServiceStatusChangedConsumerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<ServiceStatusChangedConsumer>>();
            _contextMock = new Mock<ConsumeContext<ServiceStatusChangedEvent>>();

            _sut = new ServiceStatusChangedConsumer(_repositoryMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Consume_WhenServiceExists_ShouldUpdateStatusAndSaveChanges(bool newStatus)
        {
            // Arrange
            var message = new ServiceStatusChangedEvent(Guid.NewGuid(), newStatus);
            var existingService = _fixture.Build<Service>()
                .With(s => s.IsActive, !newStatus)
                .Create();

            _contextMock.Setup(x => x.Message).Returns(message);
            _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(r => r.GetServiceByIdSyncAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingService);

            // Act
            await _sut.Consume(_contextMock.Object);

            // Assert
            existingService.IsActive.Should().Be(newStatus);

            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenServiceDoesNotExist_ShouldNotSave()
        {
            // Arrange
            var message = _fixture.Create<ServiceStatusChangedEvent>();

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