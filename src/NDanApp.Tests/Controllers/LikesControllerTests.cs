using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NDanApp.API.Controllers;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;
using Xunit;

public class LikesControllerTests
{
    private readonly Mock<ILikeService> _likeServiceMock = new();
    private readonly Mock<ILogger<LikesController>> _loggerMock = new();
    private readonly LikesController _controller;

    public LikesControllerTests()
    {
        _controller = new LikesController(_likeServiceMock.Object, _loggerMock.Object);
    }

    // ---------------- ADD LIKE ----------------
    [Fact]
    public async Task AddLike_Returns201Created()
    {
        // Arrange
        var request = new AddLikeRequest
        {
            MediaId = Guid.NewGuid(),
            GuestId = Guid.NewGuid()
        };

        var created = new LikeCreated(Guid.NewGuid(), request.MediaId);

        _likeServiceMock
            .Setup(s => s.AddLikeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.AddLike(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(created);
        createdResult.StatusCode.Should().Be(201);
    }

    // ---------------- REMOVE LIKE ----------------
    [Fact]
    public async Task RemoveLike_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var guestId = Guid.NewGuid();

        _likeServiceMock
            .Setup(s => s.RemoveLikeAsync(mediaId, guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveLike(mediaId, guestId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveLike_Returns404_WhenNotFound()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var guestId = Guid.NewGuid();

        _likeServiceMock
            .Setup(s => s.RemoveLikeAsync(mediaId, guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveLike(mediaId, guestId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ---------------- GET MEDIA LIKES ----------------
    [Fact]
    public async Task GetMediaLikes_Returns200WithList()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        var likes = new List<LikeDetail>
        {
            new LikeDetail(Guid.NewGuid(), "Alice", DateTimeOffset.UtcNow),
            new LikeDetail(Guid.NewGuid(), "Bob", DateTimeOffset.UtcNow)
        };

        _likeServiceMock
            .Setup(s => s.GetMediaLikesAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(likes);

        // Act
        var result = await _controller.GetMediaLikes(mediaId, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(likes);
    }
    
}
