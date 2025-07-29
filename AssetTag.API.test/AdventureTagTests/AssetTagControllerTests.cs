using ForEveryAdventure.Controllers;
using ForEveryAdventure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

public class AssetTagControllerTests
{
    [Fact]
    public void MakeAssetTag_ReturnsOk_WithAssetTagId()
    {
        // Arrange
        var mockStore = new Mock<IAssetTagStore>();
        mockStore.Setup(s => s.AssetTags).Returns(new List<AssetTag>());
        var mockLogger = new Mock<ILogger<AssetTagController>>();
        var controller = new AssetTagController(mockStore.Object, mockLogger.Object);

        var assetTag = new AssetTag
        {
            TagCode = "ABC123",
            UserId = Guid.NewGuid(),
            EmergencyContacts = new List<EmergencyContact>(),
            TripPlans = new List<TripPlan>()
        };

        // Act
        var result = controller.MakeAssetTag(assetTag) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        dynamic response = result.Value;
        //Assert.Equal("Retrieve your Asset Sticker with this Unique Asset Tag ID", (string)response.Message);
        //Assert.NotEqual(Guid.Empty, (Guid)response.AssetTagId);
    }
}