# API Documentation

This document provides comprehensive API reference documentation for the ForAdventure AssetTag API, including all endpoints, request/response schemas, and usage examples.

## Base Information

- **Base URL**: `https://localhost:7034/api` (Development)
- **Protocol**: HTTPS (HTTP redirected to HTTPS)
- **Content Type**: `application/json`
- **API Version**: v1.0
- **OpenAPI Specification**: Available at `/swagger/v1/swagger.json`

## Authentication

**Current Status**: No authentication required (Open API)

**Future Implementation**: JWT Bearer token authentication planned

```http
Authorization: Bearer <jwt-token>
```

## Endpoints Overview

| Method | Endpoint | Description | Implementation Status |
|--------|----------|-------------|----------------------|
| POST | `/api/AssetTag/MakeAssetTag` | Create new asset tag | ✅ Implemented |
| GET | `/api/AssetTag` | Get all asset tags | 🚧 Minimal API stub |
| GET | `/api/AssetTag/{id}` | Get asset tag by ID | 🚧 Minimal API stub |
| PUT | `/api/AssetTag/{id}` | Update asset tag | 🚧 Minimal API stub |
| DELETE | `/api/AssetTag/{id}` | Delete asset tag | 🚧 Minimal API stub |
| GET | `/api/TripPlan` | Get all trip plans | 🚧 Minimal API stub |
| POST | `/api/TripPlan` | Create trip plan | 🚧 Minimal API stub |
| PUT | `/api/TripPlan/{id}` | Update trip plan | 🚧 Minimal API stub |
| DELETE | `/api/TripPlan/{id}` | Delete trip plan | 🚧 Minimal API stub |

## Core API Endpoints

### Create Asset Tag

Creates a new asset tag with emergency contacts and trip plans.

```http
POST /api/AssetTag/MakeAssetTag
```

#### Request

**Headers**
```http
Content-Type: application/json
```

**Body Schema**
```json
{
  "tagCode": "string",
  "userId": "string (UUID)",
  "emergencyContacts": [
    {
      "id": "string (UUID)",
      "name": "string",
      "phone": "string",
      "email": "string"
    }
  ],
  "tripPlans": [
    {
      "tripIdentifier": "string (UUID)",
      "tripRoutePreference": "string",
      "tripRoute": "string",
      "tripStartDate": "string (ISO 8601)",
      "tripEndDate": "string (ISO 8601)",
      "tripDurationDays": "integer",
      "tripLocationStart": [
        {
          "locationIdentifier": "string (UUID)",
          "locationName": "string",
          "locationGPSformat01": "string",
          "locationGPSformat02": "string",
          "locationWhatThreeWords": "string",
          "locationAppleMap": "string",
          "locationGoogleMap": "string",
          "locationAddressCriteria": "string"
        }
      ],
      "tripLocationEnd": [
        // Same as tripLocationStart
      ],
      "tripFeaturedLocation": [
        // Same as tripLocationStart
      ]
    }
  ]
}
```

#### Request Example

```http
POST /api/AssetTag/MakeAssetTag
Content-Type: application/json

{
  "tagCode": "SUMMIT-2024-007",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "emergencyContacts": [
    {
      "id": "987fcdeb-51a2-43d1-b5c6-789012345678",
      "name": "Sarah Johnson",
      "phone": "+1-206-555-0123",
      "email": "sarah.johnson@example.com"
    },
    {
      "id": "456fcdeb-51a2-43d1-b5c6-789012345678",
      "name": "Mike Davis",
      "phone": "+1-206-555-0456",
      "email": "mike.davis@example.com"
    }
  ],
  "tripPlans": [
    {
      "tripIdentifier": "abc123def-456g-789h-012i-345jklmnop",
      "tripRoutePreference": "Scenic route via Panorama Point",
      "tripRoute": "Mount Rainier - Skyline Trail",
      "tripStartDate": "2024-08-15T06:00:00Z",
      "tripEndDate": "2024-08-17T20:00:00Z",
      "tripDurationDays": 3,
      "tripLocationStart": [
        {
          "locationIdentifier": "start-001",
          "locationName": "Paradise Visitor Center",
          "locationGPSformat01": "46.7869° N, 121.7355° W",
          "locationGPSformat02": "46°47'13\"N 121°44'08\"W",
          "locationWhatThreeWords": "frozen.purple.admits",
          "locationAppleMap": "https://maps.apple.com/?q=46.7869,-121.7355",
          "locationGoogleMap": "https://maps.google.com/?q=46.7869,-121.7355",
          "locationAddressCriteria": "Paradise Road, Ashford, WA 98304"
        }
      ],
      "tripLocationEnd": [
        {
          "locationIdentifier": "end-001",
          "locationName": "Camp Muir",
          "locationGPSformat01": "46.7869° N, 121.7355° W",
          "locationGPSformat02": "46°47'13\"N 121°44'08\"W",
          "locationWhatThreeWords": "camps.higher.summit",
          "locationAppleMap": "https://maps.apple.com/?q=46.8534,-121.7273",
          "locationGoogleMap": "https://maps.google.com/?q=46.8534,-121.7273",
          "locationAddressCriteria": "Mount Rainier National Park"
        }
      ],
      "tripFeaturedLocation": [
        {
          "locationIdentifier": "featured-001",
          "locationName": "Panorama Point",
          "locationGPSformat01": "46.7900° N, 121.7200° W",
          "locationGPSformat02": "46°47'24\"N 121°43'12\"W",
          "locationWhatThreeWords": "views.amazing.panoramic",
          "locationAppleMap": "https://maps.apple.com/?q=46.7900,-121.7200",
          "locationGoogleMap": "https://maps.google.com/?q=46.7900,-121.7200",
          "locationAddressCriteria": "Skyline Trail, Mount Rainier National Park"
        }
      ]
    }
  ]
}
```

#### Response

**Success Response (200 OK)**

```json
{
  "message": "Retrieve your Asset Sticker with this Unique Asset Tag ID",
  "assetTagId": "789fcdeb-51a2-43d1-b5c6-123456789012"
}
```

**Response Schema**
```json
{
  "message": "string",
  "assetTagId": "string (UUID)"
}
```

#### Error Responses

**400 Bad Request** - Invalid input data
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "TagCode": ["The TagCode field is required."],
    "UserId": ["The UserId field is required."]
  }
}
```

**500 Internal Server Error** - Server error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

## Minimal API Endpoints (Planned)

The following endpoints are defined as minimal API endpoints but not yet fully implemented:

### Asset Tag Operations

#### Get All Asset Tags
```http
GET /api/AssetTag
```
Returns array of all asset tags (currently returns empty AssetTag object).

#### Get Asset Tag by ID
```http
GET /api/AssetTag/{id}
```
Returns specific asset tag by ID (not implemented).

#### Update Asset Tag
```http
PUT /api/AssetTag/{id}
```
Updates existing asset tag (returns 204 No Content).

#### Delete Asset Tag
```http
DELETE /api/AssetTag/{id}
```
Deletes asset tag by ID (not implemented).

### Trip Plan Operations

#### Get All Trip Plans
```http
GET /api/TripPlan
```
Returns array of all trip plans (currently returns empty TripPlan object).

#### Create Trip Plan
```http
POST /api/TripPlan
```
Creates new trip plan (not implemented).

#### Update Trip Plan
```http
PUT /api/TripPlan/{id}
```
Updates existing trip plan (returns 204 No Content).

#### Delete Trip Plan
```http
DELETE /api/TripPlan/{id}
```
Deletes trip plan by ID (not implemented).

## Data Models

### AssetTag Model

```csharp
public class AssetTag
{
    public Guid Id { get; set; }                           // Auto-generated unique identifier
    public string? TagCode { get; set; }                   // User-defined tag code
    public Guid UserId { get; set; }                       // User identifier
    public List<EmergencyContact> EmergencyContacts { get; set; } // Emergency contacts list
    public List<TripPlan> TripPlans { get; set; }          // Associated trip plans
}
```

**Validation Rules:**
- `TagCode`: Optional, but recommended for identification
- `UserId`: Required, must be valid GUID
- `EmergencyContacts`: Optional list, can be empty
- `TripPlans`: Optional list, can be empty

### EmergencyContact Model

```csharp
public class EmergencyContact
{
    public Guid Id { get; set; }        // Unique identifier
    public string? Name { get; set; }   // Contact full name
    public string? Phone { get; set; }  // Phone number
    public string? Email { get; set; }  // Email address
}
```

**Validation Rules:**
- `Name`: Optional, but recommended
- `Phone`: Optional, should follow international format
- `Email`: Optional, must be valid email format when provided

### TripPlan Model

```csharp
public class TripPlan
{
    public Guid TripIdentifier { get; set; }                              // Unique trip ID
    public string? TripRoutePreference { get; set; }                      // Route preferences/notes
    public string? TripRoute { get; set; }                                // Main route name
    public DateTime TripStartDate { get; set; }                           // Trip start date/time
    public DateTime TripEndDate { get; set; }                             // Trip end date/time
    public int TripDurationDays { get; set; }                             // Duration in days
    public List<LocationCoordinates> TripLocationStart { get; set; }      // Starting locations
    public List<LocationCoordinates> TripLocationEnd { get; set; }        // Ending locations
    public List<LocationCoordinates> TripFeaturedLocation { get; set; }   // Featured/waypoint locations
}
```

**Validation Rules:**
- `TripStartDate`: Must be valid DateTime
- `TripEndDate`: Must be after TripStartDate
- `TripDurationDays`: Should match calculated date difference
- Location lists: Can be empty but recommended to have at least start/end

### LocationCoordinates Model

```csharp
public class LocationCoordinates
{
    public Guid LocationIdentifier { get; set; }        // Unique location ID
    public string? LocationName { get; set; }           // Human-readable name
    public string? LocationGPSformat01 { get; set; }    // Decimal degrees (DD)
    public string? LocationGPSformat02 { get; set; }    // Degrees minutes seconds (DMS)
    public string? LocationWhatThreeWords { get; set; } // What3Words address
    public string? LocationAppleMap { get; set; }       // Apple Maps URL
    public string? LocationGoogleMap { get; set; }      // Google Maps URL
    public string? LocationAddressCriteria { get; set; }// Street address
}
```

**GPS Format Examples:**
- `LocationGPSformat01`: "46.7869° N, 121.7355° W" (Decimal Degrees)
- `LocationGPSformat02`: "46°47'13\"N 121°44'08\"W" (Degrees Minutes Seconds)
- `LocationWhatThreeWords`: "frozen.purple.admits"

## External Service Integration

### AdventureAPIService

The `AdventureAPIService` provides methods for external API integration:

```csharp
public class AdventureAPIService
{
    private const string BaseUrl = "http://localhost:5034/api";
    
    public async Task<AssetTag> CreateAssetTagAsync(Guid userId);
    public async Task<TripPlan> AddTripPlanAsync(TripPlan plan);
    public async Task<AssetTag> GetAssetTagAsync(string tagCode);
    public async Task<string> SendEmergencyAlertAsync(string tagCode);
}
```

**Usage Example:**
```csharp
var service = new AdventureAPIService();
var assetTag = await service.CreateAssetTagAsync(userId);
```

## Error Handling

### Standard HTTP Status Codes

| Status Code | Description | When Used |
|-------------|-------------|-----------|
| 200 OK | Request successful | Successful operations |
| 201 Created | Resource created | Asset tag creation (future) |
| 204 No Content | Update successful | Update operations |
| 400 Bad Request | Invalid request data | Validation failures |
| 401 Unauthorized | Authentication required | Missing/invalid auth (future) |
| 404 Not Found | Resource not found | Invalid IDs |
| 500 Internal Server Error | Server error | Unhandled exceptions |

### Error Response Format

All error responses follow the RFC 7807 Problem Details format:

```json
{
  "type": "string (URI)",
  "title": "string",
  "status": "integer",
  "detail": "string (optional)",
  "instance": "string (optional)",
  "errors": {
    "field": ["validation message"]
  }
}
```

## Rate Limiting

**Current Status**: No rate limiting implemented

**Future Implementation**: Rate limiting planned with the following limits:
- 100 requests per minute per IP
- 1000 requests per hour per authenticated user

## OpenAPI/Swagger Integration

### Accessing API Documentation

- **Swagger UI**: `https://localhost:7034/swagger`
- **OpenAPI JSON**: `https://localhost:7034/swagger/v1/swagger.json`
- **Development Only**: Swagger UI only available in development environment

### Swagger Configuration

```csharp
// In Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### OpenAPI Annotations

Minimal API endpoints use OpenAPI annotations:

```csharp
group.MapGet("/", () => { /* implementation */ })
    .WithName("GetAllAssetTags")
    .WithOpenApi();
```

## Testing the API

### Using Swagger UI

1. Navigate to `https://localhost:7034/swagger`
2. Expand the desired endpoint
3. Click "Try it out"
4. Fill in the request parameters
5. Click "Execute"

### Using curl

```bash
# Create Asset Tag
curl -X POST "https://localhost:7034/api/AssetTag/MakeAssetTag" \
  -H "Content-Type: application/json" \
  -d '{
    "tagCode": "TEST-001",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "emergencyContacts": [
      {
        "name": "Emergency Contact",
        "phone": "+1-555-0123",
        "email": "contact@example.com"
      }
    ],
    "tripPlans": []
  }'
```

### Using PowerShell

```powershell
$body = @{
    tagCode = "TEST-001"
    userId = "123e4567-e89b-12d3-a456-426614174000"
    emergencyContacts = @(
        @{
            name = "Emergency Contact"
            phone = "+1-555-0123"
            email = "contact@example.com"
        }
    )
    tripPlans = @()
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "https://localhost:7034/api/AssetTag/MakeAssetTag" -Method Post -Body $body -ContentType "application/json"
```

## Integration Examples

### JavaScript/Fetch API

```javascript
const createAssetTag = async (assetTagData) => {
  try {
    const response = await fetch('https://localhost:7034/api/AssetTag/MakeAssetTag', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(assetTagData)
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const result = await response.json();
    console.log('Asset tag created:', result.assetTagId);
    return result;
  } catch (error) {
    console.error('Error creating asset tag:', error);
    throw error;
  }
};
```

### C# HttpClient

```csharp
public class AssetTagClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://localhost:7034/api";
    
    public AssetTagClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<AssetTagResponse> CreateAssetTagAsync(AssetTag assetTag)
    {
        var json = JsonSerializer.Serialize(assetTag);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{BaseUrl}/AssetTag/MakeAssetTag", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AssetTagResponse>(responseJson);
    }
}
```

## Future API Enhancements

### Planned Endpoints

1. **Asset Tag Management**
   - `GET /api/AssetTag/{tagCode}` - Get by tag code
   - `PATCH /api/AssetTag/{id}` - Partial updates
   - `GET /api/AssetTag/user/{userId}` - Get by user ID

2. **Emergency Features**
   - `POST /api/Emergency/{tagCode}/alert` - Send emergency alert
   - `GET /api/Emergency/{tagCode}/status` - Check emergency status

3. **Location Services**
   - `POST /api/Location/geocode` - Geocoding service
   - `GET /api/Location/nearby/{coordinates}` - Find nearby locations

4. **Analytics**
   - `GET /api/Analytics/usage` - API usage statistics
   - `GET /api/Analytics/trips` - Trip analytics

### Versioning Strategy

Future API versions will use URL path versioning:
- Current: `/api/AssetTag/MakeAssetTag`
- Version 2: `/api/v2/AssetTag/MakeAssetTag`

### Pagination

Future list endpoints will support pagination:

```http
GET /api/AssetTag?page=1&pageSize=20&sortBy=created&sortOrder=desc
```

Response:
```json
{
  "items": [...],
  "totalCount": 150,
  "currentPage": 1,
  "totalPages": 8,
  "hasNext": true,
  "hasPrevious": false
}
```

---

This API documentation provides comprehensive information for integrating with the ForAdventure AssetTag API. For additional support, refer to the [Architecture Documentation](architecture.md) and [Testing Guide](testing.md).