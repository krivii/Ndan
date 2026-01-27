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
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Services;
using Xunit;

public class MediaControllerTests
{
    private readonly Mock<IMediaService> _mediaServiceMock = new();
    private readonly Mock<ILogger<MediaController>> _loggerMock = new();
    private readonly MediaController _controller;

    public MediaControllerTests()
    {
        _controller = new MediaController(_mediaServiceMock.Object, _loggerMock.Object);
    }

    // ---------------- SAVE MEDIA METADATA ----------------
    [Fact]
    public async Task SaveMediaMetadata_Returns201Created()
    {
        // Arrange
        var request = new SaveMediaMetadataRequest
        {
            EventId = Guid.NewGuid(),
            FileName = "test.jpg",
            StorageKey = "media/test.jpg",
            FileUrl = "https://cdn.example.com/media/test.jpg",
            MediaType = MediaType.Image
        };

        var created = new MediaCreated(
            Guid.NewGuid(),
            request.StorageKey,
            request.FileUrl,
            request.MediaType
        );

        _mediaServiceMock
            .Setup(s => s.SaveMediaMetadataAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.SaveMediaMetadata(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(created);
    }

    // ---------------- GET MEDIA ----------------
    [Fact]
    public async Task GetMedia_Returns404_WhenNotFound()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mediaServiceMock
            .Setup(s => s.GetMediaDetailAsync(mediaId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaDetail?)null);

        // Act
        var result = await _controller.GetMedia(mediaId, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMedia_Returns200_WhenFound()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var detail = new MediaDetail(
            mediaId,
            MediaType.Image,
            "media/key.jpg",
            "thumb/key.jpg",
            "GuestName",
            DateTimeOffset.UtcNow,
            5,
            true
        );

        _mediaServiceMock
            .Setup(s => s.GetMediaDetailAsync(mediaId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _controller.GetMedia(mediaId, null, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(detail);
    }

    // ---------------- GET MEDIA BY EVENT ----------------
    [Fact]
    public async Task GetMediaByEvent_Returns200WithList()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var mediaList = new List<MediaListItem>
        {
            new MediaListItem(Guid.NewGuid(), "media/key1.jpg", 3),
            new MediaListItem(Guid.NewGuid(), "media/key2.jpg", 0)
        };

        _mediaServiceMock
            .Setup(s => s.GetMediaByEventAsync(eventId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediaList);

        // Act
        var result = await _controller.GetMediaByEvent(eventId, null, CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(mediaList);
    }

    // ---------------- DELETE MEDIA ----------------
    [Fact]
    public async Task DeleteMedia_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mediaServiceMock
            .Setup(s => s.DeleteMediaAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteMedia(mediaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteMedia_Returns404_WhenNotFound()
    {
        // Arrange
        var mediaId = Guid.NewGuid();

        _mediaServiceMock
            .Setup(s => s.DeleteMediaAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteMedia(mediaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
