# Testing Guide

This document provides comprehensive testing strategies, coverage analysis, and templates for the ForAdventure AssetTag API project.

## Testing Overview

The project uses modern .NET testing practices with xUnit, Moq, and Microsoft Test SDK to ensure code quality and reliability.

### Testing Framework Stack

| Tool | Purpose | Version |
|------|---------|---------|
| **xUnit** | Primary testing framework | 2.5.1 |
| **Moq** | Mocking framework | 4.20.72 |
| **Microsoft.NET.Test.SDK** | Test discovery and execution | 17.10.0 |
| **xunit.assert** | Assertion library | 2.9.3 |
| **xunit.extensibility.core** | xUnit extensions | 2.9.3 |

## Current Test Coverage

### Existing Test Structure

```
AssetTag.API.test/
└── AdventureTagTests/
    ├── AssetTagControllerTests.cs      # Controller unit tests
    ├── AdventureTagTests.csproj        # Test project configuration
    └── (Future test files)
```

### Current Test Coverage Analysis

**AssetTagController Coverage:**
- ✅ `MakeAssetTag()` - Basic functionality test
- ❌ Error handling scenarios
- ❌ Input validation edge cases
- ❌ Integration with IAssetTagStore

**Overall Coverage Statistics:**
- **Controllers**: ~30% (1 test method)
- **Services**: 0% (No tests)
- **Models**: 0% (No tests)
- **Overall**: ~10%

## Running Tests

### Command Line

```bash
# Navigate to test project
cd AssetTag.API.test/AdventureTagTests

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=AssetTagControllerTests"

# Run specific test method
dotnet test --filter "TestName=MakeAssetTag_ReturnsOk_WithAssetTagId"
```

### Visual Studio Integration

1. **Test Explorer**: View > Test Explorer
2. **Run Tests**: Right-click test → Run Test(s)
3. **Debug Tests**: Right-click test → Debug Test(s)
4. **Live Unit Testing**: Test > Live Unit Testing > Start

### Code Coverage

Generate code coverage reports:

```bash
# Install report generator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate HTML report
reportgenerator -reports:"coverage/*/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:Html
```

## Current Test Implementation

### AssetTagControllerTests.cs Analysis

```csharp
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
        // Note: Assertions are commented out in current implementation
    }
}
```

**Issues with Current Test:**
1. Incomplete assertions (commented out)
2. No verification of mock interactions
3. Missing edge case testing
4. No error scenario testing

## Comprehensive Testing Strategy

### Unit Testing Approach

#### 1. Controller Testing

**Test Categories:**
- Happy path scenarios
- Input validation failures
- Dependency failures
- Error handling

**Example: Enhanced AssetTagController Tests**

```csharp
public class AssetTagControllerTests
{
    private readonly Mock<IAssetTagStore> _mockStore;
    private readonly Mock<ILogger<AssetTagController>> _mockLogger;
    private readonly AssetTagController _controller;

    public AssetTagControllerTests()
    {
        _mockStore = new Mock<IAssetTagStore>();
        _mockLogger = new Mock<ILogger<AssetTagController>>();
        _controller = new AssetTagController(_mockStore.Object, _mockLogger.Object);
    }

    [Fact]
    public void MakeAssetTag_ValidInput_ReturnsOkWithAssetTagId()
    {
        // Arrange
        var assetTags = new List<AssetTag>();
        _mockStore.Setup(s => s.AssetTags).Returns(assetTags);
        
        var inputAssetTag = new AssetTag
        {
            TagCode = "TEST-001",
            UserId = Guid.NewGuid(),
            EmergencyContacts = new List<EmergencyContact>
            {
                new EmergencyContact 
                { 
                    Name = "John Doe", 
                    Phone = "+1-555-0123", 
                    Email = "john@example.com" 
                }
            },
            TripPlans = new List<TripPlan>()
        };

        // Act
        var result = _controller.MakeAssetTag(inputAssetTag) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        
        var response = result.Value;
        Assert.NotNull(response);
        
        // Verify asset tag was added to store
        Assert.Single(assetTags);
        var createdTag = assetTags.First();
        Assert.Equal(inputAssetTag.TagCode, createdTag.TagCode);
        Assert.Equal(inputAssetTag.UserId, createdTag.UserId);
        Assert.NotEqual(Guid.Empty, createdTag.Id);
    }

    [Fact]
    public void MakeAssetTag_NullStore_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockStore.Setup(s => s.AssetTags).Returns((List<AssetTag>)null);
        var assetTag = new AssetTag { TagCode = "TEST", UserId = Guid.NewGuid() };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => _controller.MakeAssetTag(assetTag));
        Assert.Equal("Asset tag store is not initialized.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MakeAssetTag_InvalidTagCode_HandlesGracefully(string tagCode)
    {
        // Arrange
        _mockStore.Setup(s => s.AssetTags).Returns(new List<AssetTag>());
        var assetTag = new AssetTag 
        { 
            TagCode = tagCode, 
            UserId = Guid.NewGuid() 
        };

        // Act
        var result = _controller.MakeAssetTag(assetTag) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        // The system should handle empty/null tag codes gracefully
    }

    [Fact]
    public void MakeAssetTag_EmptyUserId_HandlesGracefully()
    {
        // Arrange
        _mockStore.Setup(s => s.AssetTags).Returns(new List<AssetTag>());
        var assetTag = new AssetTag 
        { 
            TagCode = "TEST", 
            UserId = Guid.Empty 
        };

        // Act
        var result = _controller.MakeAssetTag(assetTag) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        // System should accept empty GUID (might be valid in some scenarios)
    }
}
```

#### 2. Service Testing

**ForAdventureLogic Tests**

```csharp
public class ForAdventureLogicTests
{
    [Fact]
    public void GenerateTripPlanNarrative_ValidTripPlan_ReturnsNarrative()
    {
        // Arrange
        var tripPlan = new TripPlan
        {
            TripLocationStart = new List<LocationCoordinates>
            {
                new LocationCoordinates { LocationName = "Mount Rainier" }
            },
            TripStartDate = new DateTime(2024, 7, 15),
            TripEndDate = new DateTime(2024, 7, 17),
            TripRoutePreference = "Scenic route preferred"
        };

        // Act
        var narrative = ForAdventureLogic.generateTripPlanNarrative(tripPlan);

        // Assert
        Assert.NotNull(narrative);
        Assert.Contains("Mount Rainier", narrative);
        Assert.Contains("July 15, 2024", narrative);
        Assert.Contains("July 17, 2024", narrative);
        Assert.Contains("2 days", narrative);
        Assert.Contains("Scenic route preferred", narrative);
    }

    [Fact]
    public void GenerateTripPlanNarrative_NullTripPlan_ReturnsDefaultMessage()
    {
        // Act
        var narrative = ForAdventureLogic.generateTripPlanNarrative(null);

        // Assert
        Assert.Equal("No trip plan provided.", narrative);
    }

    [Fact]
    public void GenerateTripPlanNarrative_EmptyRoutePreference_HandlesGracefully()
    {
        // Arrange
        var tripPlan = new TripPlan
        {
            TripLocationStart = new List<LocationCoordinates>(),
            TripStartDate = new DateTime(2024, 7, 15),
            TripEndDate = new DateTime(2024, 7, 17),
            TripRoutePreference = ""
        };

        // Act
        var narrative = ForAdventureLogic.generateTripPlanNarrative(tripPlan);

        // Assert
        Assert.Contains("No additional notes provided.", narrative);
    }
}
```

**AdventureAPIService Tests**

```csharp
public class AdventureAPIServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly AdventureAPIService _service;

    public AdventureAPIServiceTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object);
        _service = new AdventureAPIService();
        
        // Use reflection to set the private HttpClient field
        var clientField = typeof(AdventureAPIService).GetField("_client", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        clientField?.SetValue(_service, _httpClient);
    }

    [Fact]
    public async Task CreateAssetTagAsync_ValidUserId_ReturnsAssetTag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedAssetTag = new AssetTag { Id = Guid.NewGuid(), UserId = userId };
        var jsonResponse = JsonSerializer.Serialize(expectedAssetTag);

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.CreateAssetTagAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAssetTag.Id, result.Id);
        Assert.Equal(userId, result.UserId);
    }
}
```

#### 3. Model Testing

**AssetTag Model Tests**

```csharp
public class AssetTagTests
{
    [Fact]
    public void AssetTag_DefaultConstructor_InitializesCollections()
    {
        // Act
        var assetTag = new AssetTag();

        // Assert
        Assert.NotNull(assetTag.EmergencyContacts);
        Assert.NotNull(assetTag.TripPlans);
        Assert.Empty(assetTag.EmergencyContacts);
        Assert.Empty(assetTag.TripPlans);
    }

    [Fact]
    public void AssetTag_SetProperties_PropertiesSetCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tagCode = "TEST-001";

        // Act
        var assetTag = new AssetTag
        {
            Id = id,
            UserId = userId,
            TagCode = tagCode
        };

        // Assert
        Assert.Equal(id, assetTag.Id);
        Assert.Equal(userId, assetTag.UserId);
        Assert.Equal(tagCode, assetTag.TagCode);
    }
}
```

### Integration Testing

#### API Integration Tests

```csharp
public class AssetTagIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AssetTagIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task MakeAssetTag_ValidRequest_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        var assetTag = new AssetTag
        {
            TagCode = "INTEGRATION-001",
            UserId = Guid.NewGuid(),
            EmergencyContacts = new List<EmergencyContact>
            {
                new EmergencyContact 
                { 
                    Name = "Test Contact", 
                    Phone = "+1-555-0123" 
                }
            },
            TripPlans = new List<TripPlan>()
        };

        var json = JsonSerializer.Serialize(assetTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AssetTag/MakeAssetTag", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", 
            response.Content.Headers.ContentType?.ToString());

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<dynamic>(responseString);
        Assert.NotNull(responseObj);
    }

    [Fact]
    public async Task MakeAssetTag_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AssetTag/MakeAssetTag", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

### Test Data Builders

#### AssetTag Test Data Builder

```csharp
public class AssetTagBuilder
{
    private AssetTag _assetTag;

    public AssetTagBuilder()
    {
        _assetTag = new AssetTag
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TagCode = "DEFAULT-001",
            EmergencyContacts = new List<EmergencyContact>(),
            TripPlans = new List<TripPlan>()
        };
    }

    public AssetTagBuilder WithTagCode(string tagCode)
    {
        _assetTag.TagCode = tagCode;
        return this;
    }

    public AssetTagBuilder WithUserId(Guid userId)
    {
        _assetTag.UserId = userId;
        return this;
    }

    public AssetTagBuilder WithEmergencyContact(string name, string phone, string email = null)
    {
        _assetTag.EmergencyContacts.Add(new EmergencyContact
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            Email = email
        });
        return this;
    }

    public AssetTagBuilder WithTripPlan(string route, DateTime startDate, DateTime endDate)
    {
        _assetTag.TripPlans.Add(new TripPlan
        {
            TripIdentifier = Guid.NewGuid(),
            TripRoute = route,
            TripStartDate = startDate,
            TripEndDate = endDate,
            TripDurationDays = (endDate - startDate).Days
        });
        return this;
    }

    public AssetTag Build() => _assetTag;
}

// Usage example:
var assetTag = new AssetTagBuilder()
    .WithTagCode("TEST-001")
    .WithEmergencyContact("John Doe", "+1-555-0123", "john@example.com")
    .WithTripPlan("Mount Rainier", DateTime.Today, DateTime.Today.AddDays(3))
    .Build();
```

## Test Coverage Targets

### Minimum Coverage Goals

| Component | Target Coverage | Current Coverage | Priority |
|-----------|----------------|------------------|----------|
| Controllers | 90% | 30% | High |
| Services | 85% | 0% | High |
| Models | 70% | 0% | Medium |
| Extensions | 80% | 0% | Medium |
| Overall | 85% | 10% | High |

### Coverage Exclusions

The following code should be excluded from coverage requirements:
- Program.cs (startup configuration)
- Model properties (simple getters/setters)
- Exception constructors
- Generated code

## Performance Testing

### Load Testing with NBomber

```csharp
public class LoadTests
{
    [Fact]
    public void AssetTag_LoadTest_HandlesExpectedLoad()
    {
        var scenario = Scenario.Create("asset_tag_creation", async context =>
        {
            using var client = new HttpClient();
            
            var assetTag = new AssetTagBuilder()
                .WithTagCode($"LOAD-{context.ScenarioInfo.ThreadId}-{context.InvocationNumber}")
                .Build();
            
            var json = JsonSerializer.Serialize(assetTag);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync("http://localhost:5034/api/AssetTag/MakeAssetTag", content);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}
```

## Test Automation & CI/CD

### GitHub Actions Workflow

```yaml
name: Test and Coverage

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Generate coverage report
      run: |
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
        
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/coverage.cobertura.xml
```

### Local Test Automation

**PowerShell Script: run-tests.ps1**

```powershell
#!/usr/bin/env pwsh

param(
    [switch]$Coverage,
    [switch]$Watch,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Running ForAdventure AssetTag API Tests" -ForegroundColor Green

$testCommand = "dotnet test"

if ($Filter) {
    $testCommand += " --filter `"$Filter`""
}

if ($Coverage) {
    $testCommand += " --collect:`"XPlat Code Coverage`""
    Write-Host "Code coverage enabled" -ForegroundColor Yellow
}

if ($Watch) {
    $testCommand += " --watch"
    Write-Host "Watch mode enabled" -ForegroundColor Yellow
}

Write-Host "Executing: $testCommand" -ForegroundColor Cyan

Invoke-Expression $testCommand

if ($Coverage -and !$Watch) {
    Write-Host "Generating coverage report..." -ForegroundColor Yellow
    
    if (!(Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
        Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:Html
    
    Write-Host "Coverage report generated at: coverage/report/index.html" -ForegroundColor Green
}
```

## Testing Best Practices

### 1. Test Organization

- **Arrange, Act, Assert (AAA)**: Structure all tests with clear sections
- **One Assert Per Test**: Focus each test on a single concern
- **Descriptive Names**: Use method names that describe the scenario and expected outcome

### 2. Mocking Guidelines

- **Mock External Dependencies**: Mock IAssetTagStore, HttpClient, etc.
- **Verify Interactions**: Use `Mock.Verify()` to ensure expected calls were made
- **Setup Return Values**: Configure mocks to return expected data

### 3. Test Data Management

- **Builders Pattern**: Use builder classes for complex object creation
- **Test-Specific Data**: Create fresh data for each test to avoid coupling
- **Realistic Data**: Use data that represents real-world scenarios

### 4. Async Testing

```csharp
[Fact]
public async Task AsyncMethod_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var service = new AdventureAPIService();
    
    // Act
    var result = await service.CreateAssetTagAsync(Guid.NewGuid());
    
    // Assert
    Assert.NotNull(result);
}
```

### 5. Exception Testing

```csharp
[Fact]
public void Method_InvalidInput_ThrowsExpectedException()
{
    // Arrange
    var controller = new AssetTagController(null, null);
    
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() => 
        controller.MakeAssetTag(null));
    Assert.Equal("assetTag", exception.ParamName);
}
```

## Test Templates

### Controller Test Template

```csharp
public class [ControllerName]Tests
{
    private readonly Mock<[IDependency]> _mock[Dependency];
    private readonly [ControllerName] _controller;

    public [ControllerName]Tests()
    {
        _mock[Dependency] = new Mock<[IDependency]>();
        _controller = new [ControllerName](_mock[Dependency].Object);
    }

    [Fact]
    public void [MethodName]_[Scenario]_[ExpectedResult]()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### Service Test Template

```csharp
public class [ServiceName]Tests
{
    private readonly [ServiceName] _service;

    public [ServiceName]Tests()
    {
        _service = new [ServiceName]();
    }

    [Theory]
    [InlineData(/* test data */)]
    public void [MethodName]_[Scenario]_[ExpectedResult](/* parameters */)
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

---

This testing guide provides a comprehensive foundation for implementing robust testing practices in the ForAdventure AssetTag API project. Regular testing ensures code quality, facilitates refactoring, and provides confidence in deployments.