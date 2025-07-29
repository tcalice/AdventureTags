# Database Design Documentation

This document provides comprehensive database architecture and design documentation for the ForAdventure AssetTag API, covering both current in-memory storage and future persistent storage solutions using Azure data services.

## Current Storage Architecture

The ForAdventure AssetTag API currently uses in-memory storage for development and testing purposes, providing fast access and simple setup while planning for persistent storage migration.

### In-Memory Storage Implementation

#### Current Storage Interface

```csharp
public interface IAssetTagStore
{
    List<AssetTag> AssetTags { get; }
}

public class AssetTagStore : IAssetTagStore
{
    public List<AssetTag> AssetTags { get; } = new List<AssetTag>();
}
```

**Characteristics:**
- **Lifetime**: Singleton (data persists during application lifetime)
- **Performance**: Extremely fast (O(1) access for simple operations)
- **Scalability**: Limited to single instance, no horizontal scaling
- **Persistence**: Data lost on application restart
- **Concurrency**: Not thread-safe for write operations

#### Data Model Relationships

```
┌─────────────────┐
│    AssetTag     │
├─────────────────┤
│ Id (Guid)       │────┐
│ TagCode         │    │ 1:N
│ UserId (Guid)   │    │
│ ...             │    │
└─────────────────┘    │
                       │
    ┌──────────────────▼──────────────────┐
    │                                     │
    ▼                                     ▼
┌─────────────────┐                ┌─────────────────┐
│EmergencyContact │                │   TripPlan      │
├─────────────────┤                ├─────────────────┤
│ Id (Guid)       │                │ TripId (Guid)   │
│ Name            │                │ Route           │
│ Phone           │                │ StartDate       │
│ Email           │                │ EndDate         │
└─────────────────┘                │ ...             │
                                   └─────────────────┘
                                           │ 1:N
                                           │
                                           ▼
                                   ┌─────────────────┐
                                   │LocationCoords   │
                                   ├─────────────────┤
                                   │ Id (Guid)       │
                                   │ Name            │
                                   │ GPSFormat01     │
                                   │ GPSFormat02     │
                                   │ What3Words      │
                                   │ ...             │
                                   └─────────────────┘
```

## Azure Persistent Storage Architecture

### Recommended Azure Data Architecture

The following architecture provides scalable, reliable, and performant data storage for production workloads:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Application Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  AssetTag API   │  │   Admin Portal  │  │  Mobile App     │  │
│  │                 │  │                 │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Data Access Layer                            │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Entity Framework Core                          │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │ │
│  │  │   Repository    │  │   Unit of Work  │  │   DbContext  │ │ │
│  │  │   Pattern       │  │   Pattern       │  │              │ │ │
│  │  └─────────────────┘  └─────────────────┘  └──────────────┘ │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Data Storage Layer                         │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Azure SQL DB   │  │ Azure Cosmos DB │  │ Azure Storage   │  │
│  │                 │  │                 │  │                 │  │
│  │ • Relational    │  │ • Document DB   │  │ • Blob Storage  │  │
│  │ • ACID          │  │ • Global Scale  │  │ • File Storage  │  │
│  │ • Structured    │  │ • JSON Docs     │  │ • Queue Storage │  │
│  │ • Complex Queries│ │ • Flexible      │  │ • Table Storage │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                                                 │
│  Use Cases:         │  Use Cases:         │  Use Cases:         │
│  • Asset Tags       │  • Trip Logs        │  • Images/Files    │
│  • Users            │  • Analytics Data   │  • Backups         │
│  • Emergency Contacts│ • Real-time GPS    │  • Static Content  │
│  • Structured Data  │  • Session Data     │  • Binary Data     │
└─────────────────────────────────────────────────────────────────┘
```

## Azure SQL Database Implementation

### Database Schema Design

#### Core Tables

**AssetTags Table**
```sql
CREATE TABLE AssetTags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TagCode NVARCHAR(50) NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit fields
    CreatedBy NVARCHAR(256) NULL,
    UpdatedBy NVARCHAR(256) NULL,
    
    -- Index hints
    INDEX IX_AssetTags_UserId (UserId),
    INDEX IX_AssetTags_TagCode (TagCode) WHERE TagCode IS NOT NULL,
    INDEX IX_AssetTags_CreatedAt (CreatedAt DESC)
);
```

**EmergencyContacts Table**
```sql
CREATE TABLE EmergencyContacts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssetTagId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(255) NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(320) NULL,
    Relationship NVARCHAR(100) NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (AssetTagId) REFERENCES AssetTags(Id) ON DELETE CASCADE,
    
    -- Ensure only one primary contact per asset tag
    CONSTRAINT UQ_EmergencyContacts_PrimaryPerAssetTag 
        UNIQUE (AssetTagId, IsPrimary) 
        WHERE IsPrimary = 1,
    
    INDEX IX_EmergencyContacts_AssetTagId (AssetTagId),
    INDEX IX_EmergencyContacts_IsPrimary (IsPrimary) WHERE IsPrimary = 1
);
```

**TripPlans Table**
```sql
CREATE TABLE TripPlans (
    TripIdentifier UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssetTagId UNIQUEIDENTIFIER NOT NULL,
    TripRoute NVARCHAR(500) NULL,
    TripRoutePreference NVARCHAR(1000) NULL,
    TripStartDate DATETIME2(7) NOT NULL,
    TripEndDate DATETIME2(7) NOT NULL,
    TripDurationDays INT COMPUTED (DATEDIFF(DAY, TripStartDate, TripEndDate)),
    TripStatus NVARCHAR(50) NOT NULL DEFAULT 'Planned',
    
    -- Trip metadata
    Difficulty NVARCHAR(50) NULL,
    ExpectedParticipants INT NULL,
    EstimatedDistance DECIMAL(10,2) NULL,
    EstimatedElevationGain DECIMAL(10,2) NULL,
    
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (AssetTagId) REFERENCES AssetTags(Id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT CK_TripPlans_DateRange CHECK (TripEndDate > TripStartDate),
    CONSTRAINT CK_TripPlans_Status CHECK (TripStatus IN ('Planned', 'Active', 'Completed', 'Cancelled')),
    
    INDEX IX_TripPlans_AssetTagId (AssetTagId),
    INDEX IX_TripPlans_StartDate (TripStartDate),
    INDEX IX_TripPlans_Status (TripStatus)
);
```

**LocationCoordinates Table**
```sql
CREATE TABLE LocationCoordinates (
    LocationIdentifier UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TripPlanId UNIQUEIDENTIFIER NOT NULL,
    LocationType NVARCHAR(50) NOT NULL, -- 'Start', 'End', 'Featured', 'Waypoint'
    LocationName NVARCHAR(255) NULL,
    
    -- GPS Coordinates
    Latitude DECIMAL(10,8) NULL,
    Longitude DECIMAL(11,8) NULL,
    Elevation DECIMAL(10,2) NULL,
    Accuracy DECIMAL(10,2) NULL,
    
    -- Format representations
    LocationGPSformat01 NVARCHAR(100) NULL, -- Decimal degrees
    LocationGPSformat02 NVARCHAR(100) NULL, -- Degrees minutes seconds
    LocationWhatThreeWords NVARCHAR(100) NULL,
    LocationAppleMap NVARCHAR(500) NULL,
    LocationGoogleMap NVARCHAR(500) NULL,
    LocationAddressCriteria NVARCHAR(1000) NULL,
    
    -- Ordering for multiple locations of same type
    SortOrder INT NOT NULL DEFAULT 0,
    
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TripPlanId) REFERENCES TripPlans(TripIdentifier) ON DELETE CASCADE,
    
    CONSTRAINT CK_LocationCoordinates_Type 
        CHECK (LocationType IN ('Start', 'End', 'Featured', 'Waypoint')),
    CONSTRAINT CK_LocationCoordinates_Latitude 
        CHECK (Latitude IS NULL OR (Latitude >= -90 AND Latitude <= 90)),
    CONSTRAINT CK_LocationCoordinates_Longitude 
        CHECK (Longitude IS NULL OR (Longitude >= -180 AND Longitude <= 180)),
    
    INDEX IX_LocationCoordinates_TripPlanId (TripPlanId),
    INDEX IX_LocationCoordinates_Type (LocationType),
    INDEX IX_LocationCoordinates_Coordinates (Latitude, Longitude) WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL
);
```

#### Supporting Tables

**Users Table** (Future Enhancement)
```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(256) NOT NULL UNIQUE,
    Email NVARCHAR(320) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    
    -- Authentication
    PasswordHash NVARCHAR(500) NULL,
    SecurityStamp NVARCHAR(100) NULL,
    
    -- Profile
    DateOfBirth DATE NULL,
    EmergencyContactInfo NVARCHAR(1000) NULL,
    ExperienceLevel NVARCHAR(50) NULL,
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2(7) NULL,
    
    INDEX IX_Users_Username (Username),
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_IsActive (IsActive) WHERE IsActive = 1
);
```

**AuditLog Table** (Audit Trail)
```sql
CREATE TABLE AuditLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(100) NOT NULL,
    RecordId NVARCHAR(100) NOT NULL,
    Operation NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    ChangedBy NVARCHAR(256) NULL,
    ChangedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_AuditLog_TableName (TableName),
    INDEX IX_AuditLog_RecordId (RecordId),
    INDEX IX_AuditLog_ChangedAt (ChangedAt DESC)
);
```

### Entity Framework Core Implementation

#### DbContext Configuration

```csharp
public class AssetTagDbContext : DbContext
{
    public AssetTagDbContext(DbContextOptions<AssetTagDbContext> options)
        : base(options)
    {
    }

    public DbSet<AssetTag> AssetTags { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<TripPlan> TripPlans { get; set; }
    public DbSet<LocationCoordinates> LocationCoordinates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AssetTag entity
        modelBuilder.Entity<AssetTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.TagCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AssetTags_UserId");
            entity.HasIndex(e => e.TagCode).HasDatabaseName("IX_AssetTags_TagCode");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AssetTags_CreatedAt");

            // Configure relationships
            entity.HasMany(e => e.EmergencyContacts)
                  .WithOne()
                  .HasForeignKey("AssetTagId")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TripPlans)
                  .WithOne()
                  .HasForeignKey("AssetTagId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmergencyContact entity
        modelBuilder.Entity<EmergencyContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Relationship).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.AssetTagId, e.IsPrimary })
                  .HasDatabaseName("UQ_EmergencyContacts_PrimaryPerAssetTag")
                  .IsUnique()
                  .HasFilter("[IsPrimary] = 1");
        });

        // Configure TripPlan entity
        modelBuilder.Entity<TripPlan>(entity =>
        {
            entity.HasKey(e => e.TripIdentifier);
            entity.Property(e => e.TripIdentifier).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.TripRoute).HasMaxLength(500);
            entity.Property(e => e.TripRoutePreference).HasMaxLength(1000);
            entity.Property(e => e.TripStatus).HasMaxLength(50).HasDefaultValue("Planned");
            entity.Property(e => e.Difficulty).HasMaxLength(50);
            entity.Property(e => e.EstimatedDistance).HasColumnType("decimal(10,2)");
            entity.Property(e => e.EstimatedElevationGain).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Computed column
            entity.Property(e => e.TripDurationDays)
                  .HasComputedColumnSql("DATEDIFF(DAY, [TripStartDate], [TripEndDate])");

            entity.HasIndex(e => e.AssetTagId).HasDatabaseName("IX_TripPlans_AssetTagId");
            entity.HasIndex(e => e.TripStartDate).HasDatabaseName("IX_TripPlans_StartDate");
            entity.HasIndex(e => e.TripStatus).HasDatabaseName("IX_TripPlans_Status");

            // Configure relationships
            entity.HasMany(e => e.TripLocationStart)
                  .WithOne()
                  .HasForeignKey("TripPlanId")
                  .HasPrincipalKey(e => e.TripIdentifier)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TripLocationEnd)
                  .WithOne()
                  .HasForeignKey("TripPlanId")
                  .HasPrincipalKey(e => e.TripIdentifier)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TripFeaturedLocation)
                  .WithOne()
                  .HasForeignKey("TripPlanId")
                  .HasPrincipalKey(e => e.TripIdentifier)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure LocationCoordinates entity
        modelBuilder.Entity<LocationCoordinates>(entity =>
        {
            entity.HasKey(e => e.LocationIdentifier);
            entity.Property(e => e.LocationIdentifier).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.LocationType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LocationName).HasMaxLength(255);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");
            entity.Property(e => e.Elevation).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Accuracy).HasColumnType("decimal(10,2)");
            entity.Property(e => e.LocationGPSformat01).HasMaxLength(100);
            entity.Property(e => e.LocationGPSformat02).HasMaxLength(100);
            entity.Property(e => e.LocationWhatThreeWords).HasMaxLength(100);
            entity.Property(e => e.LocationAppleMap).HasMaxLength(500);
            entity.Property(e => e.LocationGoogleMap).HasMaxLength(500);
            entity.Property(e => e.LocationAddressCriteria).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.TripPlanId).HasDatabaseName("IX_LocationCoordinates_TripPlanId");
            entity.HasIndex(e => e.LocationType).HasDatabaseName("IX_LocationCoordinates_Type");
            entity.HasIndex(e => new { e.Latitude, e.Longitude })
                  .HasDatabaseName("IX_LocationCoordinates_Coordinates")
                  .HasFilter("[Latitude] IS NOT NULL AND [Longitude] IS NOT NULL");
        });

        // Configure audit properties
        ConfigureAuditProperties(modelBuilder);
    }

    private void ConfigureAuditProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure CreatedAt and UpdatedAt for all entities
            var createdAtProperty = entityType.FindProperty("CreatedAt");
            if (createdAtProperty != null)
            {
                createdAtProperty.SetColumnType("datetime2(7)");
                createdAtProperty.SetDefaultValueSql("GETUTCDATE()");
            }

            var updatedAtProperty = entityType.FindProperty("UpdatedAt");
            if (updatedAtProperty != null)
            {
                updatedAtProperty.SetColumnType("datetime2(7)");
                updatedAtProperty.SetDefaultValueSql("GETUTCDATE()");
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditProperties();
        return base.SaveChanges();
    }

    private void UpdateAuditProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var updatedAtProperty = entry.Property("UpdatedAt");
            if (updatedAtProperty != null)
            {
                updatedAtProperty.CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Property("CreatedAt");
                if (createdAtProperty != null)
                {
                    createdAtProperty.CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
```

#### Repository Pattern Implementation

```csharp
public interface IAssetTagRepository
{
    Task<AssetTag?> GetByIdAsync(Guid id);
    Task<AssetTag?> GetByTagCodeAsync(string tagCode);
    Task<IEnumerable<AssetTag>> GetByUserIdAsync(Guid userId);
    Task<AssetTag> CreateAsync(AssetTag assetTag);
    Task<AssetTag> UpdateAsync(AssetTag assetTag);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> TagCodeExistsAsync(string tagCode);
}

public class AssetTagRepository : IAssetTagRepository
{
    private readonly AssetTagDbContext _context;
    private readonly ILogger<AssetTagRepository> _logger;

    public AssetTagRepository(AssetTagDbContext context, ILogger<AssetTagRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssetTag?> GetByIdAsync(Guid id)
    {
        return await _context.AssetTags
            .Include(at => at.EmergencyContacts)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripLocationStart)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripLocationEnd)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripFeaturedLocation)
            .FirstOrDefaultAsync(at => at.Id == id);
    }

    public async Task<AssetTag?> GetByTagCodeAsync(string tagCode)
    {
        if (string.IsNullOrWhiteSpace(tagCode))
            return null;

        return await _context.AssetTags
            .Include(at => at.EmergencyContacts)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripLocationStart)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripLocationEnd)
            .Include(at => at.TripPlans)
                .ThenInclude(tp => tp.TripFeaturedLocation)
            .FirstOrDefaultAsync(at => at.TagCode == tagCode);
    }

    public async Task<IEnumerable<AssetTag>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AssetTags
            .Include(at => at.EmergencyContacts)
            .Include(at => at.TripPlans)
            .Where(at => at.UserId == userId && at.IsActive)
            .OrderByDescending(at => at.CreatedAt)
            .ToListAsync();
    }

    public async Task<AssetTag> CreateAsync(AssetTag assetTag)
    {
        _context.AssetTags.Add(assetTag);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new asset tag {AssetTagId} for user {UserId}", 
            assetTag.Id, assetTag.UserId);
        
        return assetTag;
    }

    public async Task<AssetTag> UpdateAsync(AssetTag assetTag)
    {
        _context.AssetTags.Update(assetTag);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated asset tag {AssetTagId}", assetTag.Id);
        
        return assetTag;
    }

    public async Task DeleteAsync(Guid id)
    {
        var assetTag = await _context.AssetTags.FindAsync(id);
        if (assetTag != null)
        {
            // Soft delete
            assetTag.IsActive = false;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Soft deleted asset tag {AssetTagId}", id);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AssetTags.AnyAsync(at => at.Id == id && at.IsActive);
    }

    public async Task<bool> TagCodeExistsAsync(string tagCode)
    {
        if (string.IsNullOrWhiteSpace(tagCode))
            return false;

        return await _context.AssetTags.AnyAsync(at => at.TagCode == tagCode && at.IsActive);
    }
}
```

#### Migration Strategy

**Initial Migration**
```bash
# Add Entity Framework Core tools
dotnet tool install --global dotnet-ef

# Add migration
dotnet ef migrations add InitialCreate --context AssetTagDbContext

# Update database
dotnet ef database update --context AssetTagDbContext
```

**Migration Files Structure**
```
Migrations/
├── 20240101000000_InitialCreate.cs
├── 20240115000000_AddAuditFields.cs
├── 20240201000000_AddUserTable.cs
├── 20240215000000_AddGeoSpatialIndexes.cs
└── AssetTagDbContextModelSnapshot.cs
```

## Azure Cosmos DB Implementation

For scenarios requiring global distribution, high availability, and flexible schema, Azure Cosmos DB provides an excellent NoSQL alternative.

### Document Structure

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "partitionKey": "user-456",
  "type": "assetTag",
  "tagCode": "SUMMIT-2024-007",
  "userId": "456e7890-1234-5678-9012-345678901234",
  "emergencyContacts": [
    {
      "id": "987fcdeb-51a2-43d1-b5c6-789012345678",
      "name": "Sarah Johnson",
      "phone": "+1-206-555-0123",
      "email": "sarah.johnson@example.com",
      "relationship": "Spouse",
      "isPrimary": true
    }
  ],
  "tripPlans": [
    {
      "tripIdentifier": "abc123def-456g-789h-012i-345jklmnop",
      "tripRoute": "Mount Rainier - Skyline Trail",
      "tripRoutePreference": "Scenic route via Panorama Point",
      "tripStartDate": "2024-08-15T06:00:00Z",
      "tripEndDate": "2024-08-17T20:00:00Z",
      "tripDurationDays": 3,
      "tripStatus": "Planned",
      "locations": {
        "start": [
          {
            "locationIdentifier": "start-001",
            "locationName": "Paradise Visitor Center",
            "coordinates": {
              "latitude": 46.7869,
              "longitude": -121.7355,
              "elevation": 1646
            },
            "formats": {
              "decimal": "46.7869° N, 121.7355° W",
              "dms": "46°47'13\"N 121°44'08\"W",
              "what3words": "frozen.purple.admits"
            },
            "links": {
              "apple": "https://maps.apple.com/?q=46.7869,-121.7355",
              "google": "https://maps.google.com/?q=46.7869,-121.7355"
            }
          }
        ],
        "end": [...],
        "featured": [...]
      }
    }
  ],
  "metadata": {
    "createdAt": "2024-01-15T08:30:00Z",
    "updatedAt": "2024-01-15T08:30:00Z",
    "version": 1,
    "isActive": true
  },
  "_etag": "\"8700dadf-0000-0d00-0000-5e0b32450000\""
}
```

### Cosmos DB Service Implementation

```csharp
public interface ICosmosAssetTagService
{
    Task<AssetTag> CreateAsync(AssetTag assetTag);
    Task<AssetTag?> GetAsync(string id, string partitionKey);
    Task<AssetTag?> GetByTagCodeAsync(string tagCode);
    Task<IEnumerable<AssetTag>> GetByUserIdAsync(string userId);
    Task<AssetTag> UpdateAsync(AssetTag assetTag);
    Task DeleteAsync(string id, string partitionKey);
}

public class CosmosAssetTagService : ICosmosAssetTagService
{
    private readonly Container _container;
    private readonly ILogger<CosmosAssetTagService> _logger;

    public CosmosAssetTagService(CosmosClient cosmosClient, ILogger<CosmosAssetTagService> logger)
    {
        _container = cosmosClient.GetContainer("ForAdventureDB", "AssetTags");
        _logger = logger;
    }

    public async Task<AssetTag> CreateAsync(AssetTag assetTag)
    {
        var document = new AssetTagDocument
        {
            id = assetTag.Id.ToString(),
            partitionKey = $"user-{assetTag.UserId}",
            type = "assetTag",
            tagCode = assetTag.TagCode,
            userId = assetTag.UserId.ToString(),
            emergencyContacts = assetTag.EmergencyContacts?.Select(ec => new EmergencyContactDocument
            {
                id = ec.Id.ToString(),
                name = ec.Name,
                phone = ec.Phone,
                email = ec.Email
            }).ToList(),
            tripPlans = assetTag.TripPlans?.Select(tp => new TripPlanDocument
            {
                tripIdentifier = tp.TripIdentifier.ToString(),
                tripRoute = tp.TripRoute,
                tripStartDate = tp.TripStartDate,
                tripEndDate = tp.TripEndDate,
                // ... map other properties
            }).ToList(),
            metadata = new DocumentMetadata
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                version = 1,
                isActive = true
            }
        };

        var response = await _container.CreateItemAsync(document, new PartitionKey(document.partitionKey));
        
        _logger.LogInformation("Created asset tag {AssetTagId} in Cosmos DB", assetTag.Id);
        
        return MapToAssetTag(response.Resource);
    }

    public async Task<AssetTag?> GetAsync(string id, string partitionKey)
    {
        try
        {
            var response = await _container.ReadItemAsync<AssetTagDocument>(id, new PartitionKey(partitionKey));
            return MapToAssetTag(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<AssetTag?> GetByTagCodeAsync(string tagCode)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.type = 'assetTag' AND c.tagCode = @tagCode AND c.metadata.isActive = true")
            .WithParameter("@tagCode", tagCode);

        var iterator = _container.GetItemQueryIterator<AssetTagDocument>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var document = response.FirstOrDefault();
            if (document != null)
            {
                return MapToAssetTag(document);
            }
        }

        return null;
    }

    public async Task<IEnumerable<AssetTag>> GetByUserIdAsync(string userId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.type = 'assetTag' AND c.userId = @userId AND c.metadata.isActive = true ORDER BY c.metadata.createdAt DESC")
            .WithParameter("@userId", userId);

        var results = new List<AssetTag>();
        var iterator = _container.GetItemQueryIterator<AssetTagDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var document in response)
            {
                results.Add(MapToAssetTag(document));
            }
        }

        return results;
    }

    private AssetTag MapToAssetTag(AssetTagDocument document)
    {
        return new AssetTag
        {
            Id = Guid.Parse(document.id),
            TagCode = document.tagCode,
            UserId = Guid.Parse(document.userId),
            EmergencyContacts = document.emergencyContacts?.Select(ec => new EmergencyContact
            {
                Id = Guid.Parse(ec.id),
                Name = ec.name,
                Phone = ec.phone,
                Email = ec.email
            }).ToList() ?? new List<EmergencyContact>(),
            TripPlans = document.tripPlans?.Select(tp => new TripPlan
            {
                TripIdentifier = Guid.Parse(tp.tripIdentifier),
                TripRoute = tp.tripRoute,
                TripStartDate = tp.tripStartDate,
                TripEndDate = tp.tripEndDate,
                // ... map other properties
            }).ToList() ?? new List<TripPlan>()
        };
    }
}
```

## Azure Storage Implementation

For binary data, file uploads, and blob storage requirements:

### Blob Storage for Asset Images

```csharp
public interface IBlobStorageService
{
    Task<string> UploadAssetImageAsync(Guid assetTagId, Stream imageStream, string contentType);
    Task<Stream> DownloadAssetImageAsync(string blobName);
    Task DeleteAssetImageAsync(string blobName);
    Task<IEnumerable<string>> ListAssetImagesAsync(Guid assetTagId);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;
    private const string ContainerName = "asset-images";

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadAssetImageAsync(Guid assetTagId, Stream imageStream, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobName = $"{assetTagId}/{Guid.NewGuid()}.jpg";
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(imageStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders,
            Metadata = new Dictionary<string, string>
            {
                ["AssetTagId"] = assetTagId.ToString(),
                ["UploadedAt"] = DateTime.UtcNow.ToString("O")
            }
        });

        _logger.LogInformation("Uploaded image {BlobName} for asset tag {AssetTagId}", blobName, assetTagId);

        return blobName;
    }

    public async Task<Stream> DownloadAssetImageAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task DeleteAssetImageAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
        
        _logger.LogInformation("Deleted image {BlobName}", blobName);
    }

    public async Task<IEnumerable<string>> ListAssetImagesAsync(Guid assetTagId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobs = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: assetTagId.ToString()))
        {
            blobs.Add(blobItem.Name);
        }

        return blobs;
    }
}
```

## Connection String Management

### Azure Key Vault Integration

```csharp
public static class DatabaseConfiguration
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // SQL Server with Entity Framework
        services.AddDbContext<AssetTagDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly("ForEveryAdventure");
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Cosmos DB
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDB");
        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            services.AddSingleton(serviceProvider =>
            {
                return new CosmosClient(cosmosConnectionString, new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                });
            });
        }

        // Azure Storage
        var storageConnectionString = configuration.GetConnectionString("AzureStorage");
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            services.AddSingleton(x => new BlobServiceClient(storageConnectionString));
        }

        // Register repositories
        services.AddScoped<IAssetTagRepository, AssetTagRepository>();
        services.AddScoped<ICosmosAssetTagService, CosmosAssetTagService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
    }
}
```

### Connection String Security

**Key Vault Secret Names:**
- `ConnectionStrings--DefaultConnection` (SQL Server)
- `ConnectionStrings--CosmosDB` (Cosmos DB)
- `ConnectionStrings--AzureStorage` (Storage Account)

**Connection String Format Examples:**
```
SQL Server:
Server=tcp:foradventure-sql.database.windows.net,1433;Database=ForAdventureAssetTagDB;User ID=sqladmin;Password={password};Encrypt=true;Connection Timeout=30;

Cosmos DB:
AccountEndpoint=https://foradventure-cosmos.documents.azure.com:443/;AccountKey={key};

Azure Storage:
DefaultEndpointsProtocol=https;AccountName=foradventurestorage;AccountKey={key};EndpointSuffix=core.windows.net
```

## Performance Optimization

### SQL Server Optimization

#### Indexing Strategy

```sql
-- Primary indexes for common queries
CREATE NONCLUSTERED INDEX IX_AssetTags_UserId_Active 
ON AssetTags (UserId, IsActive) 
INCLUDE (Id, TagCode, CreatedAt)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_AssetTags_TagCode_Active 
ON AssetTags (TagCode) 
INCLUDE (Id, UserId, CreatedAt)
WHERE TagCode IS NOT NULL AND IsActive = 1;

-- Composite index for trip plan queries
CREATE NONCLUSTERED INDEX IX_TripPlans_AssetTag_Status_Dates 
ON TripPlans (AssetTagId, TripStatus) 
INCLUDE (TripIdentifier, TripStartDate, TripEndDate, TripRoute);

-- Spatial index for location coordinates
CREATE SPATIAL INDEX IX_LocationCoordinates_Spatial 
ON LocationCoordinates (Coordinates) 
USING GEOMETRY_GRID;
```

#### Query Optimization

```csharp
// Efficient paging
public async Task<(IEnumerable<AssetTag> Items, int Total)> GetPagedAsync(
    Guid userId, int page, int pageSize)
{
    var query = _context.AssetTags
        .Where(at => at.UserId == userId && at.IsActive)
        .OrderByDescending(at => at.CreatedAt);

    var total = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Include(at => at.EmergencyContacts)
        .Include(at => at.TripPlans.Where(tp => tp.TripStatus == "Active"))
        .ToListAsync();

    return (items, total);
}

// Projection for list views
public async Task<IEnumerable<AssetTagSummary>> GetSummariesAsync(Guid userId)
{
    return await _context.AssetTags
        .Where(at => at.UserId == userId && at.IsActive)
        .Select(at => new AssetTagSummary
        {
            Id = at.Id,
            TagCode = at.TagCode,
            CreatedAt = at.CreatedAt,
            ActiveTripCount = at.TripPlans.Count(tp => tp.TripStatus == "Active"),
            EmergencyContactCount = at.EmergencyContacts.Count
        })
        .OrderByDescending(ats => ats.CreatedAt)
        .ToListAsync();
}
```

### Cosmos DB Optimization

#### Partition Key Strategy

```csharp
// Optimal partition key design
public class AssetTagDocument
{
    [JsonPropertyName("id")]
    public string id { get; set; }
    
    // Partition by user to ensure related data is colocated
    [JsonPropertyName("partitionKey")]
    public string partitionKey { get; set; } // Format: "user-{userId}"
    
    // Include document type for multi-entity containers
    [JsonPropertyName("type")]
    public string type { get; set; } = "assetTag";
    
    // TTL for automatic cleanup (optional)
    [JsonPropertyName("ttl")]
    public int? ttl { get; set; }
}
```

#### Query Optimization

```csharp
// Cross-partition query with proper indexing
public async Task<IEnumerable<AssetTag>> SearchByLocationAsync(double latitude, double longitude, double radiusKm)
{
    var query = new QueryDefinition(@"
        SELECT * FROM c 
        WHERE c.type = 'assetTag' 
        AND c.metadata.isActive = true 
        AND ST_DISTANCE(c.currentLocation, {'type': 'Point', 'coordinates': [@lng, @lat]}) < @radius")
        .WithParameter("@lat", latitude)
        .WithParameter("@lng", longitude)
        .WithParameter("@radius", radiusKm * 1000); // Convert to meters

    var results = new List<AssetTag>();
    var iterator = _container.GetItemQueryIterator<AssetTagDocument>(query);

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        results.AddRange(response.Select(MapToAssetTag));
    }

    return results;
}
```

## Backup and Recovery

### SQL Server Backup Strategy

```sql
-- Automated backup configuration
EXEC sp_configure 'backup compression default', 1;
RECONFIGURE;

-- Full backup (automated by Azure SQL)
BACKUP DATABASE [ForAdventureAssetTagDB] 
TO URL = 'https://foradventurestorage.blob.core.windows.net/backups/full-backup.bak'
WITH COMPRESSION, CHECKSUM, STATS = 10;

-- Transaction log backup (automated by Azure SQL)
BACKUP LOG [ForAdventureAssetTagDB] 
TO URL = 'https://foradventurestorage.blob.core.windows.net/backups/log-backup.trn'
WITH COMPRESSION, CHECKSUM, STATS = 10;
```

### Point-in-Time Recovery

```powershell
# Restore to specific point in time
$resourceGroup = "rg-foradventure-prod"
$serverName = "foradventure-sql"
$sourceDatabaseName = "ForAdventureAssetTagDB"
$targetDatabaseName = "ForAdventureAssetTagDB-Restored"
$restorePoint = "2024-01-15T08:00:00Z"

Restore-AzSqlDatabase `
    -FromPointInTimeBackup `
    -PointInTime $restorePoint `
    -ResourceGroupName $resourceGroup `
    -ServerName $serverName `
    -TargetDatabaseName $targetDatabaseName `
    -ResourceId (Get-AzSqlDatabase -ResourceGroupName $resourceGroup -ServerName $serverName -DatabaseName $sourceDatabaseName).ResourceId
```

## Monitoring and Metrics

### Database Performance Monitoring

```csharp
public class DatabaseMetricsService
{
    private readonly AssetTagDbContext _context;
    private readonly TelemetryClient _telemetryClient;

    public async Task TrackQueryPerformance<T>(string queryName, Func<Task<T>> query)
    {
        var stopwatch = Stopwatch.StartNew();
        var connectionsBefore = GetActiveConnections();

        try
        {
            var result = await query();
            
            stopwatch.Stop();
            
            _telemetryClient.TrackMetric($"Database.Query.{queryName}.Duration", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackMetric($"Database.Query.{queryName}.Success", 1);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _telemetryClient.TrackException(ex);
            _telemetryClient.TrackMetric($"Database.Query.{queryName}.Error", 1);
            _telemetryClient.TrackMetric($"Database.Query.{queryName}.Duration", stopwatch.ElapsedMilliseconds);
            
            throw;
        }
        finally
        {
            var connectionsAfter = GetActiveConnections();
            _telemetryClient.TrackMetric("Database.Connections.Active", connectionsAfter);
        }
    }

    private int GetActiveConnections()
    {
        // Implementation to get active connection count
        return 0; // Placeholder
    }
}
```

### Custom Dashboards

**Application Insights Queries:**
```kql
// Database operation performance
customMetrics
| where name startswith "Database.Query."
| extend QueryName = extract(@"Database\.Query\.(.+)\.Duration", 1, name)
| summarize avg(value), percentile(value, 95) by QueryName
| order by avg_value desc

// Asset tag creation trends
customEvents
| where name == "AssetTag.Creation.Success"
| summarize count() by bin(timestamp, 1h)
| render timechart

// Error rate by operation
customMetrics
| where name endswith ".Error"
| extend Operation = extract(@"(.+)\.Error", 1, name)
| summarize sum(value) by Operation, bin(timestamp, 5m)
| render timechart
```

## Migration Strategy

### Current to SQL Server Migration

**Phase 1: Parallel Implementation**
1. Implement Entity Framework DbContext alongside current in-memory store
2. Add feature flag to toggle between storage implementations
3. Implement data synchronization for testing

**Phase 2: Gradual Migration**
1. Route new data to SQL Server
2. Migrate existing data in batches
3. Verify data integrity

**Phase 3: Complete Migration**
1. Switch all operations to SQL Server
2. Remove in-memory implementation
3. Monitor performance and optimize

### Migration Script Example

```csharp
public class DataMigrationService
{
    private readonly IAssetTagStore _inMemoryStore;
    private readonly IAssetTagRepository _sqlRepository;
    private readonly ILogger<DataMigrationService> _logger;

    public async Task MigrateAllDataAsync()
    {
        var inMemoryAssetTags = _inMemoryStore.AssetTags;
        var migrationBatchSize = 100;
        var totalMigrated = 0;

        for (int i = 0; i < inMemoryAssetTags.Count; i += migrationBatchSize)
        {
            var batch = inMemoryAssetTags.Skip(i).Take(migrationBatchSize);
            
            foreach (var assetTag in batch)
            {
                try
                {
                    // Check if already exists
                    if (!await _sqlRepository.ExistsAsync(assetTag.Id))
                    {
                        await _sqlRepository.CreateAsync(assetTag);
                        totalMigrated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to migrate asset tag {AssetTagId}", assetTag.Id);
                }
            }

            _logger.LogInformation("Migrated batch {BatchNumber}, total migrated: {TotalMigrated}", 
                (i / migrationBatchSize) + 1, totalMigrated);
        }

        _logger.LogInformation("Migration completed. Total records migrated: {TotalMigrated}", totalMigrated);
    }

    public async Task<bool> ValidateMigrationAsync()
    {
        var inMemoryCount = _inMemoryStore.AssetTags.Count;
        var sqlCount = await _sqlRepository.GetCountAsync();

        if (inMemoryCount != sqlCount)
        {
            _logger.LogWarning("Migration validation failed. InMemory: {InMemoryCount}, SQL: {SqlCount}", 
                inMemoryCount, sqlCount);
            return false;
        }

        _logger.LogInformation("Migration validation successful. Record count: {RecordCount}", sqlCount);
        return true;
    }
}
```

---

This database design documentation provides a comprehensive foundation for transitioning from the current in-memory storage to robust, scalable Azure data services. The modular approach allows for gradual migration while maintaining system reliability and performance.