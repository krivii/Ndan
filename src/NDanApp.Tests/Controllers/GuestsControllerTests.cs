using System;
using System.Collections.Generic;
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

public class GuestsControllerTests
{
    private readonly Mock<IGuestService> _guestServiceMock = new();
    private readonly Mock<ILogger<GuestsController>> _loggerMock = new();
    private readonly GuestsController _controller;

    public GuestsControllerTests()
    {
        _controller = new GuestsController(_guestServiceMock.Object, _loggerMock.Object);
    }

    // ---------------- CREATE GUEST ----------------

    [Fact]
    public async Task CreateGuest_Returns201Created()
    {
        // Arrange
        var request = new CreateGuestRequest
        {
            EventId = Guid.NewGuid(),
            Nickname = "John"
        };

        var created = new GuestCreated(
            Guid.NewGuid(),
            "John"
        );

        _guestServiceMock
            .Setup(s => s.CreateGuestAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateGuest(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(created);
    }

    // ---------------- GET GUEST BY ID ----------------

    [Fact]
    public async Task GetGuest_WhenNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();

        _guestServiceMock
            .Setup(s => s.GetGuestDetailAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GuestDetail?)null);

        // Act
        var result = await _controller.GetGuest(id, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetGuest_WhenFound_Returns200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var detail = new GuestDetail(
            id,
            Guid.NewGuid(),
            "Alice",
            DateTimeOffset.UtcNow,
            5,
            10
        );

        _guestServiceMock
            .Setup(s => s.GetGuestDetailAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _controller.GetGuest(id, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    // ---------------- GET GUESTS BY EVENT ----------------

    [Fact]
    public async Task GetGuestsByEvent_Returns200WithList()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var guests = new List<GuestListItem>
        {
            new GuestListItem(Guid.NewGuid(), "Alice", 3),
            new GuestListItem(Guid.NewGuid(), "Bob", 0)
        };

        _guestServiceMock
            .Setup(s => s.GetGuestsByEventAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guests);

        // Act
        var result = await _controller.GetGuestsByEvent(eventId, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(guests);
    }
}
