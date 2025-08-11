[Fact]
public async Task SetTripType_WithValidRequest_ReturnsCreated()
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var request = new TripTypeRequest 
    { 
        AdventureId = "adv-123", 
        Type = TripTypeOption.Section 
    };
    
    mockService.Setup(s => s.SetTripTypeAsync(It.IsAny<TripTypeRequest>()))
        .ReturnsAsync(true);
    
    var controller = new MobileAdventureController(mockService.Object);
    
    // Act
    var result = await controller.TripType(request);
    
    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    Assert.Equal(201, createdResult.StatusCode);
    mockService.Verify(s => s.SetTripTypeAsync(request), Times.Once);
}

[Fact]
public async Task SetTripType_WithInvalidAdventureId_ReturnsBadRequest()
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var request = new TripTypeRequest { AdventureId = "", Type = TripTypeOption.Circuit };
    
    mockService.Setup(s => s.SetTripTypeAsync(It.IsAny<TripTypeRequest>()))
        .ThrowsAsync(new ArgumentException("Invalid adventure ID"));
    
    var controller = new MobileAdventureController(mockService.Object);
    
    // Act
    var result = await controller.TripType(request);
    
    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.Equal(400, badRequestResult.StatusCode);
}

[Fact]
public async Task SetTripStartLocation_WithValidCoordinates_ReturnsCreated()
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var request = new LocationRequest 
    { 
        AdventureId = "adv-123", 
        Latitude = 47.6062, 
        Longitude = -122.3321 
    };
    
    mockService.Setup(s => s.SetTripStartLocationAsync(It.IsAny<LocationRequest>()))
        .ReturnsAsync(new LocationResponse { LocationId = "loc-123" });
    
    var controller = new MobileAdventureController(mockService.Object);
    
    // Act
    var result = await controller.TripStartLocation(request);
    
    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    Assert.Equal(201, createdResult.StatusCode);
    Assert.Equal("loc-123", ((LocationResponse)createdResult.Value).LocationId);
}

[Theory]
[InlineData(91, 0)]      // Invalid latitude (over 90)
[InlineData(-91, 0)]     // Invalid latitude (under -90)
[InlineData(0, 181)]     // Invalid longitude (over 180)
[InlineData(0, -181)]    // Invalid longitude (under -180)
public async Task SetTripStartLocation_WithInvalidCoordinates_ReturnsBadRequest(double latitude, double longitude)
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var request = new LocationRequest 
    { 
        AdventureId = "adv-123", 
        Latitude = latitude, 
        Longitude = longitude 
    };
    
    var controller = new MobileAdventureController(mockService.Object);
    controller.ModelState.AddModelError("Coordinates", "Invalid coordinates");
    
    // Act
    var result = await controller.TripStartLocation(request);
    
    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
}

[Fact]
public async Task SetTripEndLocation_WithValidRequest_ReturnsCreated()
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var request = new LocationRequest 
    { 
        AdventureId = "adv-123", 
        Latitude = 47.6062, 
        Longitude = -122.3321,
        LocationName = "Mountain Summit" 
    };
    
    mockService.Setup(s => s.SetTripEndLocationAsync(It.IsAny<LocationRequest>()))
        .ReturnsAsync(new LocationResponse { LocationId = "loc-456" });
    
    var controller = new MobileAdventureController(mockService.Object);
    
    // Act
    var result = await controller.TripEndLocation(request);
    
    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    Assert.Equal(201, createdResult.StatusCode);
}

[Fact]
public async Task SetTripEndLocation_WithCircuitTypeAdventure_ValidatesMatchingStartLocation()
{
    // Arrange
    var mockService = new Mock<IMobileAdventureService>();
    var mockTripService = new Mock<ITripService>();
    
    var request = new LocationRequest 
    { 
        AdventureId = "adv-circuit", 
        Latitude = 47.6062, 
        Longitude = -122.3321
    };
    
    mockTripService.Setup(s => s.GetTripTypeAsync("adv-circuit"))
        .ReturnsAsync(TripTypeOption.Circuit);
        
    mockTripService.Setup(s => s.ValidateCircuitEndpointsAsync(
        "adv-circuit", It.IsAny<double>(), It.IsAny<double>()))
        .ReturnsAsync(false); // Endpoints don't match for circuit
    
    var controller = new MobileAdventureController(mockService.Object, mockTripService.Object);
    
    // Act
    var result = await controller.TripEndLocation(request);
    
    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.Contains("must match start location for circuit", badRequestResult.Value.ToString());
}