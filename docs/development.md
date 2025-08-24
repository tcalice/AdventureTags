# Development Workflow Documentation

This document provides comprehensive guidelines for contributing to the ForAdventure AssetTag API project, including development setup, coding standards, branching strategies, and release processes.

## Development Environment Setup

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | Runtime and development |
| [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) | Latest | IDE |
| [Git](https://git-scm.com/) | 2.40+ | Version control |
| [Docker](https://www.docker.com/) | Latest | Containerization (optional) |
| [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/) | Latest | Azure deployment |
| [SQL Server](https://www.microsoft.com/en-us/sql-server) | 2019+ | Database (development) |

### Local Development Setup

#### 1. Repository Setup

```bash
# Clone the repository
git clone https://github.com/tcalice/AdventureTags.git
cd AdventureTags

# Create and switch to development branch
git checkout -b feature/your-feature-name

# Install .NET tools
dotnet tool restore

# Restore NuGet packages
dotnet restore
```

#### 2. Development Database Setup

**Option A: SQL Server LocalDB (Recommended)**
```bash
# Create local database
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

# Update connection string in appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ForAdventureAssetTagDev;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}

# Run migrations (when implemented)
dotnet ef database update --project AssetTag.API/WebApplication1
```

**Option B: Docker SQL Server**
```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sql-dev \
   -d mcr.microsoft.com/mssql/server:2019-latest

# Update connection string
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ForAdventureAssetTagDev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

#### 3. IDE Configuration

**Visual Studio Setup:**
1. Install required extensions:
   - Azure development workload
   - ASP.NET and web development workload
   - Data storage and processing workload

2. Configure code analysis:
   - Enable StyleCop analyzers
   - Set up EditorConfig compliance
   - Configure live unit testing (optional)

**VS Code Setup:**
```json
// .vscode/settings.json
{
  "dotnet.defaultSolution": "AssetTag.API/WebApplication1/AdventureTags.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.organizeImports": true
  }
}

// .vscode/extensions.json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-vscode.azure-account",
    "bradlc.vscode-tailwindcss",
    "editorconfig.editorconfig"
  ]
}
```

#### 4. Environment Variables

Create `.env` file for local development:
```bash
# .env (not committed to source control)
ASPNETCORE_ENVIRONMENT=Development
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=(localdb)\\MSSQLLocalDB;Database=ForAdventureAssetTagDev;Trusted_Connection=true
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_TENANT_ID=your-tenant-id
```

#### 5. Build and Run

```bash
# Build the solution
cd AssetTag.API/WebApplication1
dotnet build

# Run the application
dotnet run

# Run with hot reload (development)
dotnet watch run

# Run tests
cd ../../AssetTag.API.test/AdventureTagTests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Coding Standards and Guidelines

### C# Coding Standards

#### 1. Naming Conventions

```csharp
// Classes: PascalCase
public class AssetTagController { }

// Methods: PascalCase
public IActionResult MakeAssetTag() { }

// Properties: PascalCase
public string TagCode { get; set; }

// Private fields: camelCase with underscore prefix
private readonly IAssetTagStore _store;

// Local variables: camelCase
var assetTag = new AssetTag();

// Constants: PascalCase
private const int MaxTagLength = 50;

// Interfaces: PascalCase with 'I' prefix
public interface IAssetTagStore { }
```

#### 2. Code Organization

```csharp
// File organization order:
// 1. Using statements (grouped and sorted)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ForEveryAdventure.Models;
using ForEveryAdventure.Services;

// 2. Namespace
namespace ForEveryAdventure.Controllers
{
    // 3. Class with proper documentation
    /// <summary>
    /// Controller for managing AssetTag operations.
    /// Provides endpoints for creating, retrieving, and managing asset tags
    /// for outdoor adventure safety tracking.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTagController : ControllerBase
    {
        // 4. Private fields
        private readonly IAssetTagStore _store;
        private readonly ILogger<AssetTagController> _logger;

        // 5. Constructor
        public AssetTagController(IAssetTagStore store, ILogger<AssetTagController> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 6. Public methods
        // 7. Private methods
    }
}
```

#### 3. Method Structure

```csharp
/// <summary>
/// Creates a new asset tag with emergency contacts and trip plans.
/// </summary>
/// <param name="assetTag">The asset tag data to create</param>
/// <returns>Response containing the created asset tag ID</returns>
/// <response code="200">Asset tag created successfully</response>
/// <response code="400">Invalid input data</response>
/// <response code="500">Internal server error</response>
[HttpPost("MakeAssetTag")]
[ProducesResponseType(typeof(AssetTagResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> MakeAssetTagAsync([FromBody] AssetTag assetTag)
{
    // Input validation
    if (assetTag == null)
    {
        _logger.LogWarning("MakeAssetTag called with null asset tag");
        return BadRequest("Asset tag data is required");
    }

    try
    {
        // Business logic
        var newTag = await CreateAssetTagAsync(assetTag);
        
        // Logging
        _logger.LogInformation("Created asset tag {AssetTagId} for user {UserId}", 
            newTag.Id, assetTag.UserId);

        // Response
        var response = new AssetTagResponse
        {
            Message = "Retrieve your Asset Sticker with this Unique Asset Tag ID",
            AssetTagId = newTag.Id
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating asset tag for user {UserId}", assetTag.UserId);
        return StatusCode(500, "An error occurred while creating the asset tag");
    }
}
```

#### 4. Error Handling

```csharp
// Use specific exception types
public class AssetTagNotFoundException : Exception
{
    public AssetTagNotFoundException(string tagCode) 
        : base($"Asset tag with code '{tagCode}' was not found")
    {
        TagCode = tagCode;
    }

    public string TagCode { get; }
}

// Global exception handling middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            AssetTagNotFoundException ex => new { message = ex.Message, statusCode = 404 },
            ValidationException ex => new { message = ex.Message, statusCode = 400 },
            UnauthorizedAccessException => new { message = "Unauthorized", statusCode = 401 },
            _ => new { message = "An error occurred", statusCode = 500 }
        };

        response.StatusCode = errorResponse.statusCode;
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
```

### EditorConfig Configuration

Create `.editorconfig` file in repository root:

```ini
# EditorConfig is awesome: https://EditorConfig.org

# top-most EditorConfig file
root = true

# All files
[*]
indent_style = space
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true
charset = utf-8

# Code files
[*.{cs,csx,vb,vbx}]
indent_size = 4

# XML project files
[*.{csproj,vbproj,vcxproj,vcxproj.filters,proj,projitems,shproj}]
indent_size = 2

# JSON files
[*.json]
indent_size = 2

# YAML files
[*.{yml,yaml}]
indent_size = 2

# Markdown files
[*.md]
trim_trailing_whitespace = false

# C# files
[*.cs]

# New line preferences
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_call_parameter_list_parentheses = false
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_parentheses = false
```

## Git Workflow and Branching Strategy

### Branch Structure

```
main (production-ready code)
├── develop (integration branch)
│   ├── feature/asset-tag-enhancement
│   ├── feature/emergency-contacts
│   └── feature/trip-planning-improvements
├── release/v1.1.0 (release preparation)
├── hotfix/critical-security-fix
└── docs/comprehensive-documentation
```

### Branch Types

| Branch Type | Naming Convention | Purpose | Base Branch |
|-------------|------------------|---------|-------------|
| `main` | `main` | Production-ready code | N/A |
| `develop` | `develop` | Integration branch | `main` |
| `feature` | `feature/description` | New features | `develop` |
| `release` | `release/v1.0.0` | Release preparation | `develop` |
| `hotfix` | `hotfix/description` | Critical fixes | `main` |
| `docs` | `docs/description` | Documentation only | `develop` |

### Git Flow Process

#### 1. Feature Development

```bash
# Start new feature
git checkout develop
git pull origin develop
git checkout -b feature/add-asset-image-upload

# Work on feature
git add .
git commit -m "feat: add asset image upload functionality

- Add BlobStorageService for Azure Storage integration
- Implement image validation and resizing
- Add unit tests for upload functionality
- Update API documentation

Closes #123"

# Push feature branch
git push -u origin feature/add-asset-image-upload

# Create pull request to develop branch
```

#### 2. Release Process

```bash
# Create release branch
git checkout develop
git pull origin develop
git checkout -b release/v1.1.0

# Update version numbers and changelog
# Fix any release-specific issues
git commit -m "chore: prepare release v1.1.0"

# Merge to main
git checkout main
git merge --no-ff release/v1.1.0
git tag -a v1.1.0 -m "Release version 1.1.0"

# Merge back to develop
git checkout develop
git merge --no-ff release/v1.1.0

# Push all changes
git push origin main develop --tags
```

#### 3. Hotfix Process

```bash
# Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/security-vulnerability

# Fix the issue
git commit -m "fix: resolve security vulnerability in asset tag validation

- Add input sanitization for TagCode field
- Implement rate limiting for API endpoints
- Update security headers configuration

Fixes #456"

# Merge to main
git checkout main
git merge --no-ff hotfix/security-vulnerability
git tag -a v1.0.1 -m "Hotfix version 1.0.1"

# Merge to develop
git checkout develop
git merge --no-ff hotfix/security-vulnerability

# Push changes
git push origin main develop --tags
```

### Commit Message Conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

#### Commit Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or modifying tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements
- `ci`: CI/CD changes
- `build`: Build system changes

#### Examples

```bash
# Feature commit
git commit -m "feat(api): add asset tag search functionality

- Implement full-text search across asset tags
- Add search filters for date range and user
- Include pagination support for search results
- Add comprehensive unit tests

Closes #789"

# Bug fix commit
git commit -m "fix(storage): resolve null reference in asset tag store

The AssetTags property was returning null when the store
was not properly initialized, causing application crashes.

Added null checks and proper initialization.

Fixes #456"

# Documentation commit
git commit -m "docs: update API documentation with new endpoints

- Add OpenAPI specifications for search endpoints
- Update README with new feature descriptions
- Include example requests and responses"

# Breaking change commit
git commit -m "refactor!: change asset tag ID from int to GUID

BREAKING CHANGE: Asset tag IDs are now GUIDs instead of integers.
This affects all API endpoints that accept or return asset tag IDs.

Migration script provided in /scripts/migrate-ids.sql"
```

## Code Review Process

### Pull Request Guidelines

#### 1. PR Creation Checklist

- [ ] Branch is up-to-date with target branch
- [ ] All tests pass locally
- [ ] Code follows project coding standards
- [ ] Documentation updated (if applicable)
- [ ] Breaking changes documented
- [ ] Security implications considered

#### 2. PR Template

```markdown
## Description
Brief description of changes made and the problem they solve.

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Code refactoring

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] Performance testing (if applicable)

## Documentation
- [ ] Code comments added/updated
- [ ] API documentation updated
- [ ] README updated (if applicable)
- [ ] Migration guide created (for breaking changes)

## Security
- [ ] No sensitive data exposed
- [ ] Input validation implemented
- [ ] Authentication/authorization considered
- [ ] Dependencies checked for vulnerabilities

## Screenshots (if applicable)
Add screenshots or GIFs demonstrating the changes.

## Related Issues
Closes #123
Relates to #456

## Checklist
- [ ] My code follows the project's coding standards
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
```

#### 3. Review Process

**Reviewer Responsibilities:**
1. **Code Quality**: Check for adherence to coding standards
2. **Logic**: Verify business logic correctness
3. **Security**: Look for security vulnerabilities
4. **Performance**: Identify potential performance issues
5. **Tests**: Ensure adequate test coverage
6. **Documentation**: Verify documentation completeness

**Review Checklist:**
```markdown
## Code Review Checklist

### Functionality
- [ ] Code achieves the intended purpose
- [ ] Edge cases are handled properly
- [ ] Error handling is appropriate
- [ ] Business logic is correct

### Code Quality
- [ ] Code is readable and well-structured
- [ ] Naming conventions are followed
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] Comments explain the "why" not the "what"

### Security
- [ ] No hardcoded secrets or credentials
- [ ] Input validation is implemented
- [ ] SQL injection prevention (if applicable)
- [ ] XSS prevention (if applicable)

### Performance
- [ ] No obvious performance bottlenecks
- [ ] Database queries are optimized
- [ ] Caching is used appropriately
- [ ] Resource disposal is handled properly

### Testing
- [ ] Unit tests cover new/changed code
- [ ] Integration tests are appropriate
- [ ] Tests are meaningful and not just for coverage
- [ ] Mock usage is appropriate

### Documentation
- [ ] API documentation is updated
- [ ] Code comments are helpful
- [ ] README changes are appropriate
- [ ] Breaking changes are documented
```

## Testing Strategy

### Test Pyramid

```
    ┌─────────────────┐
    │   E2E Tests     │ ← Few, slow, expensive
    │   (API Tests)   │
    ├─────────────────┤
    │ Integration     │ ← Some, moderate speed
    │ Tests           │
    ├─────────────────┤
    │  Unit Tests     │ ← Many, fast, cheap
    │                 │
    └─────────────────┘
```

### Testing Guidelines

#### 1. Unit Tests

```csharp
[TestClass]
public class AssetTagControllerTests
{
    private Mock<IAssetTagStore> _mockStore;
    private Mock<ILogger<AssetTagController>> _mockLogger;
    private AssetTagController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockStore = new Mock<IAssetTagStore>();
        _mockLogger = new Mock<ILogger<AssetTagController>>();
        _controller = new AssetTagController(_mockStore.Object, _mockLogger.Object);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [TestCategory("Controller")]
    public async Task MakeAssetTag_ValidInput_ReturnsOkWithAssetTagId()
    {
        // Arrange
        var assetTag = CreateValidAssetTag();
        _mockStore.Setup(s => s.AssetTags).Returns(new List<AssetTag>());

        // Act
        var result = await _controller.MakeAssetTagAsync(assetTag);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as AssetTagResponse;
        
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.AssetTagId);
        
        // Verify mock interactions
        _mockStore.Verify(s => s.AssetTags, Times.Once);
    }

    private AssetTag CreateValidAssetTag()
    {
        return new AssetTag
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
            }
        };
    }
}
```

#### 2. Integration Tests

```csharp
[TestClass]
public class AssetTagIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace services for testing
                    services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IAssetTagStore)));
                    services.AddSingleton<IAssetTagStore, InMemoryAssetTagStore>();
                });
            });

        _client = _factory.CreateClient();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task CreateAssetTag_EndToEnd_Success()
    {
        // Arrange
        var assetTag = new
        {
            tagCode = "INTEGRATION-001",
            userId = Guid.NewGuid(),
            emergencyContacts = new[]
            {
                new { name = "Test Contact", phone = "+1-555-0123", email = "test@example.com" }
            }
        };

        var json = JsonSerializer.Serialize(assetTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AssetTag/MakeAssetTag", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AssetTagResponse>(responseContent);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.AssetTagId);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
```

#### 3. Test Data Management

```csharp
public static class TestDataBuilder
{
    public static AssetTag CreateAssetTag(Action<AssetTag> configure = null)
    {
        var assetTag = new AssetTag
        {
            Id = Guid.NewGuid(),
            TagCode = $"TEST-{Random.Shared.Next(1000, 9999)}",
            UserId = Guid.NewGuid(),
            EmergencyContacts = new List<EmergencyContact>(),
            TripPlans = new List<TripPlan>()
        };

        configure?.Invoke(assetTag);
        return assetTag;
    }

    public static EmergencyContact CreateEmergencyContact(Action<EmergencyContact> configure = null)
    {
        var contact = new EmergencyContact
        {
            Id = Guid.NewGuid(),
            Name = "Test Contact",
            Phone = "+1-555-0123",
            Email = "test@example.com"
        };

        configure?.Invoke(contact);
        return contact;
    }

    public static TripPlan CreateTripPlan(Action<TripPlan> configure = null)
    {
        var tripPlan = new TripPlan
        {
            TripIdentifier = Guid.NewGuid(),
            TripRoute = "Test Route",
            TripStartDate = DateTime.UtcNow.AddDays(1),
            TripEndDate = DateTime.UtcNow.AddDays(3),
            TripDurationDays = 2
        };

        configure?.Invoke(tripPlan);
        return tripPlan;
    }
}

// Usage in tests
var assetTag = TestDataBuilder.CreateAssetTag(at =>
{
    at.TagCode = "SPECIFIC-CODE";
    at.EmergencyContacts.Add(TestDataBuilder.CreateEmergencyContact());
});
```

## Release Management

### Versioning Strategy

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version (X.0.0): Breaking changes
- **MINOR** version (X.Y.0): New features (backward compatible)
- **PATCH** version (X.Y.Z): Bug fixes (backward compatible)

#### Version Examples
- `1.0.0` - Initial release
- `1.1.0` - Added trip planning features
- `1.1.1` - Fixed asset tag validation bug
- `2.0.0` - Changed from integer IDs to GUIDs (breaking change)

### Release Process

#### 1. Release Planning

Create release milestone in GitHub:
```markdown
# Release v1.2.0 - Enhanced Trip Planning

## Target Date: 2024-02-15

## Features
- [ ] Advanced trip route planning (#123)
- [ ] GPS coordinate validation (#124)
- [ ] Trip sharing functionality (#125)

## Bug Fixes
- [ ] Fix asset tag duplicate detection (#126)
- [ ] Resolve memory leak in location services (#127)

## Documentation
- [ ] Update API documentation
- [ ] Create migration guide
- [ ] Update README

## Testing
- [ ] Performance testing completed
- [ ] Security testing completed
- [ ] User acceptance testing completed
```

#### 2. Release Checklist

```markdown
## Pre-Release Checklist

### Code Quality
- [ ] All tests passing
- [ ] Code coverage > 80%
- [ ] No critical/high security vulnerabilities
- [ ] Performance benchmarks met
- [ ] Documentation updated

### Deployment Preparation
- [ ] Database migration scripts ready
- [ ] Configuration changes documented
- [ ] Rollback plan prepared
- [ ] Infrastructure capacity verified

### Communication
- [ ] Release notes drafted
- [ ] Stakeholders notified
- [ ] Support team briefed
- [ ] Marketing materials prepared (if applicable)

### Post-Release
- [ ] Monitoring dashboards configured
- [ ] Alerting rules updated
- [ ] Backup verification completed
- [ ] Health checks validated
```

#### 3. Release Notes Template

```markdown
# Release Notes - Version 1.2.0

**Release Date:** February 15, 2024  
**Deployment:** Staged rollout over 24 hours

## 🎉 New Features

### Enhanced Trip Planning
- **Advanced Route Planning** - Create detailed trip routes with multiple waypoints
- **GPS Coordinate Validation** - Automatic validation of location coordinates
- **Trip Sharing** - Share trip plans with emergency contacts and fellow adventurers

### Improved User Experience
- **Faster Asset Tag Creation** - 50% reduction in creation time
- **Enhanced Search** - Full-text search across all asset tag data

## 🐛 Bug Fixes

- Fixed asset tag duplicate detection for similar tag codes
- Resolved memory leak in location coordinate processing
- Corrected timezone handling in trip date calculations
- Fixed API response formatting for empty emergency contact lists

## ⚡ Performance Improvements

- Reduced API response time by 30% through optimized database queries
- Implemented response caching for frequently accessed endpoints
- Optimized memory usage in trip plan processing

## 🔒 Security Updates

- Enhanced input validation for all API endpoints
- Updated authentication token expiration handling
- Improved rate limiting configuration

## 📚 Documentation

- Updated API documentation with new endpoints
- Added migration guide for breaking changes
- Enhanced troubleshooting section in README

## 🔧 Technical Changes

### Breaking Changes
⚠️ **Important:** This release contains breaking changes

- **Asset Tag IDs**: Changed from integer to GUID format
  - **Migration Required**: Run `scripts/migrate-asset-tag-ids.sql`
  - **API Impact**: All endpoints returning asset tag IDs now return GUIDs

### Database Changes
- Added `Coordinates` table for location data
- Added indexes for improved query performance
- Modified `AssetTags` table structure

### API Changes
- Added `/api/v2/AssetTag/search` endpoint
- Modified response format for `/api/AssetTag/MakeAssetTag`
- Deprecated `/api/AssetTag/list` (will be removed in v2.0)

## 📦 Dependencies

### Updated
- Microsoft.AspNetCore.OpenApi: 8.0.11 → 8.0.12
- Swashbuckle.AspNetCore: 6.9.0 → 6.9.1

### Added
- Azure.Storage.Blobs: 12.19.1 (for future file upload functionality)

## 🚀 Deployment

### Prerequisites
- .NET 8.0 runtime
- SQL Server 2019 or later
- Azure Storage Account (for file uploads)

### Migration Steps
1. Backup current database
2. Run migration script: `scripts/v1.2.0-migration.sql`
3. Update application configuration
4. Deploy application
5. Verify health checks

### Rollback Plan
If issues occur, rollback using:
1. Restore database from backup
2. Deploy previous version (v1.1.2)
3. Update configuration to previous state

## 🐛 Known Issues

- Trip plan export may timeout for very large datasets (>1000 plans)
  - **Workaround**: Use date range filters to limit export size
  - **Fix planned**: Version 1.2.1

## 📞 Support

For questions or issues:
- Create an issue on [GitHub](https://github.com/tcalice/AdventureTags/issues)
- Contact support at: support@foradventure.com
- Documentation: [docs.foradventure.com](https://docs.foradventure.com)

---

**Full Changelog**: [v1.1.2...v1.2.0](https://github.com/tcalice/AdventureTags/compare/v1.1.2...v1.2.0)
```

## Continuous Integration/Continuous Deployment

### GitHub Actions Workflow

The project uses GitHub Actions for automated CI/CD. Key workflows:

1. **Build and Test** - Runs on every PR and push
2. **Security Scan** - Weekly security vulnerability scanning  
3. **Deploy to Staging** - Automatic deployment to staging environment
4. **Deploy to Production** - Manual approval required

### Quality Gates

Before code can be merged to `main`:

| Gate | Requirement | Status |
|------|-------------|--------|
| **Build** | Must pass | Required |
| **Unit Tests** | >95% pass rate | Required |
| **Code Coverage** | >80% coverage | Required |
| **Security Scan** | No critical/high vulnerabilities | Required |
| **Code Review** | 2 approvals from maintainers | Required |
| **Integration Tests** | All tests pass | Required |

### Deployment Strategy

**Staging Environment:**
- Automatic deployment from `develop` branch
- Used for integration testing and demos
- Reset weekly with fresh test data

**Production Environment:**
- Manual deployment with approval gates
- Blue-green deployment strategy
- Automated rollback capability
- Phased rollout (10% → 50% → 100%)

## Contributing Guidelines

### Getting Started

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create** a feature branch
4. **Make** your changes
5. **Test** thoroughly
6. **Submit** a pull request

### Contribution Types

We welcome various types of contributions:

- 🐛 **Bug fixes** - Help us squash bugs
- ✨ **New features** - Add exciting functionality
- 📝 **Documentation** - Improve or add documentation
- 🎨 **UI/UX improvements** - Enhance user experience
- ⚡ **Performance** - Make things faster
- 🧹 **Refactoring** - Clean up code
- 🧪 **Tests** - Improve test coverage

### Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Follow the golden rule

### Getting Help

- 📖 Check the [documentation](docs/)
- 🔍 Search [existing issues](https://github.com/tcalice/AdventureTags/issues)
- 💬 Start a [discussion](https://github.com/tcalice/AdventureTags/discussions)
- 📧 Contact maintainers directly

## Troubleshooting Common Issues

### Development Environment

**Issue: Build fails with package restore errors**
```bash
# Solution: Clear NuGet cache and restore
dotnet nuget locals all --clear
dotnet restore --force
dotnet build
```

**Issue: Database connection fails**
```bash
# Solution: Check connection string and ensure SQL Server is running
dotnet ef database update --verbose
# Check connection string in appsettings.Development.json
```

**Issue: Tests fail with timeout errors**
```bash
# Solution: Increase test timeout and check async/await usage
dotnet test --logger "console;verbosity=detailed"
```

### Git Workflow

**Issue: Branch is behind main**
```bash
# Solution: Rebase your feature branch
git checkout feature/your-feature
git rebase main
git push --force-with-lease origin feature/your-feature
```

**Issue: Merge conflicts**
```bash
# Solution: Resolve conflicts manually
git checkout feature/your-feature
git rebase main
# Resolve conflicts in files
git add .
git rebase --continue
```

**Issue: Accidentally committed to wrong branch**
```bash
# Solution: Cherry-pick commits to correct branch
git log --oneline  # Find commit hash
git checkout correct-branch
git cherry-pick <commit-hash>
git checkout wrong-branch
git reset --hard HEAD~1  # Remove from wrong branch
```

---

This development workflow documentation provides comprehensive guidance for contributing to the ForAdventure AssetTag API project. Following these guidelines ensures code quality, consistency, and effective collaboration among team members.