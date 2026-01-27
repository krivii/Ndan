using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NDanApp.Backend.Controllers;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;
using Xunit;

public class EventsControllerTests
{
    private readonly Mock<IEventService> _eventServiceMock = new();
    private readonly Mock<ILogger<EventsController>> _loggerMock = new();
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _controller = new EventsController(_eventServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateEvent_Returns201Created()
    {
        // Arrange
        var request = new CreateEventRequest { Name = "Test Event" };
        var created = new EventCreated(
                        Guid.NewGuid(),
                        "Test Event",
                        "test-event"
                    );


        _eventServiceMock
            .Setup(s => s.CreateEventAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateEvent(request, CancellationToken.None);

        // Assert
        var action = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        action.StatusCode.Should().Be(201);
        action.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task ValidateInvite_WhenTokenInvalid_Returns401()
    {
        // Arrange
        var request = new ValidateInviteRequest { InviteToken = "bad" };

        _eventServiceMock
            .Setup(s => s.ValidateInviteAsync("bad", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventAccess?)null);

        // Act
        var result = await _controller.ValidateInvite(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ValidateInvite_WhenTokenValid_Returns200()
    {
        // Arrange
        var request = new ValidateInviteRequest { InviteToken = "good" };
        var access = new EventAccess (Guid.NewGuid(), "Test", true);

        _eventServiceMock
            .Setup(s => s.ValidateInviteAsync("good", It.IsAny<CancellationToken>()))
            .ReturnsAsync(access);

        // Act
        var result = await _controller.ValidateInvite(request, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(access);
    }

    [Fact]
    public async Task GetEvent_WhenNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();

        _eventServiceMock
            .Setup(s => s.GetEventDetailAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDetail?)null);

        // Act
        var result = await _controller.GetEvent(id, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetEvent_WhenFound_Returns200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var detail = new EventDetail(
                    id,
                    "Test Event",
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    DateTimeOffset.UtcNow,
                    0,
                    0,
                    0
                );

        _eventServiceMock
            .Setup(s => s.GetEventDetailAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _controller.GetEvent(id, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    [Fact]
    public async Task DeactivateEvent_WhenNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();

        _eventServiceMock
            .Setup(s => s.DeactivateEventAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeactivateEvent(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeactivateEvent_WhenSuccess_Returns200()
    {
        // Arrange
        var id = Guid.NewGuid();

        _eventServiceMock
            .Setup(s => s.DeactivateEventAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateEvent(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
