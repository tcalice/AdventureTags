# Testing Strategy

## Test Coverage Goals
- **Controllers**: 90% coverage of status codes and response types
- **Services**: 95% coverage of business logic paths
- **Data Access**: 80% coverage via repository interfaces

## Dependency Injection in Tests
Tests use the TestServer approach to verify DI configuration:

## Azure-Ready Test Considerations
- Integration tests can use LocalDB or the Azure SQL Emulator
- Tests should verify configuration is properly loaded from environment variables
- Performance tests should validate app meets Azure SLAs

## Unit Test Structure
- **Arrange-Act-Assert**: Structure all tests with clear separation of these phases
- **One Assertion Per Test**: Maintain test clarity by focusing on a single behavior
- **Test Class Per Component**: Create dedicated test classes for each controller/service

### Naming Convention
- **MethodName_Scenario_ExpectedResult**: Example: `GetTags_WithValidFilter_ReturnsFilteredTags`

## API Coverage Patterns

### Controller Tests

### Service Layer Tests

## Testing New API Capabilities

### Test-Driven Approach
1. **Write Tests First**: Document expected behavior through tests before implementation
2. **Cover Edge Cases**: Include tests for input validation, error handling, and boundary conditions
3. **Test API Contracts**: Validate response structures match API specifications

### API Test Matrix Template
| Endpoint | Scenario | Input | Expected Status | Expected Response | Coverage Areas |
|----------|----------|-------|-----------------|-------------------|----------------|
| GET /api/tags | Valid request | `?category=hiking` | 200 | Array of hiking tags | Controller, Service |
| GET /api/tags | Invalid category | `?category=<invalid>` | 400 | Error message | Validation, Error handling |
| POST /api/tags | Valid tag | `{name: "Trail", category: "Hiking"}` | 201 | Created tag with ID | Controller, Service, Repository |
| POST /api/MobileAdventure/TripType | Valid trip type | `{"adventureId": "adv123", "type": "Section"}` | 201 | Created confirmation | Controller, Service |
| POST /api/MobileAdventure/TripType | Missing adventure ID | `{"type": "Circuit"}` | 400 | Validation error | Input validation |
| POST /api/MobileAdventure/TripStartLocation | Valid coordinates | `{"adventureId": "adv123", "latitude": 47.6, "longitude": -122.3}` | 201 | Location ID | Controller, Service, Repository |
| POST /api/MobileAdventure/TripStartLocation | Invalid coordinates | `{"adventureId": "adv123", "latitude": 91, "longitude": -122.3}` | 400 | Validation error | Input validation |
| POST /api/MobileAdventure/TripEndLocation | Valid end location | `{"adventureId": "adv123", "latitude": 48.1, "longitude": -123.4}` | 201 | Location ID | Controller, Service |
| POST /api/MobileAdventure/TripEndLocation | Circuit with different end point | `{"adventureId": "circuit123", "latitude": 48.1, "longitude": -123.4}` | 400 | Business rule error | Service validation |
| GET /api/MobileAdventure/CurrentStatus | Get live trip status | `?adventureId=adv123` | 200 | Status object with coordinates, duration, etc. | Real-time tracking |
| POST /api/MobileAdventure/CheckPoint | Record waypoint | `{"adventureId": "adv123", "latitude": 47.6, "longitude": -122.3, "timestamp": "2023-05-01T14:30:00Z"}` | 201 | Checkpoint ID | Progress tracking |
| PUT /api/MobileAdventure/PauseTrip | Pause tracking | `{"adventureId": "adv123", "reason": "Break"}` | 200 | Updated trip status | Trip management |
| POST /api/MobileAdventure/BulkSync | Sync offline data | `{"adventureId": "adv123", "checkpoints": [...], "photos": [...]}` | 200 | Sync results with conflicts | Offline capability |
| GET /api/MobileAdventure/DownloadMapTiles | Get offline maps | `?region=pacific-nw&zoom=12` | 200 | Map tile package | Offline navigation |
| POST /api/MobileAdventure/ShareTrip | Generate shareable link | `{"adventureId": "adv123", "permission": "view"}` | 201 | Sharing URL and settings | Social engagement |
| GET /api/MobileAdventure/NearbyAdventures | Discover adventures | `?latitude=47.6&longitude=-122.3&radiusKm=10` | 200 | List of public adventures nearby | Discovery |
| POST /api/MobileAdventure/SOSAlert | Signal emergency | `{"adventureId": "adv123", "latitude": 47.6, "longitude": -122.3, "message": "Need help"}` | 200 | Confirmation of alert receipt | Safety |
| PUT /api/MobileAdventure/UpdateSchedule | Update ETA | `{"adventureId": "adv123", "estimatedCompletionTime": "2023-05-01T18:00:00Z"}` | 200 | Updated schedule | Safety monitoring |
| POST /api/MobileAdventure/Photos | Upload trip photo | `multipart/form-data with image & metadata` | 201 | Photo ID and URL | Trip documentation |
| POST /api/MobileAdventure/TrackConditions | Report trail conditions | `{"adventureId": "adv123", "condition": "Muddy", "details": "Heavy rain made trail difficult"}` | 201 | Report confirmation | Community intelligence |
| GET /api/MobileAdventure/RecentTrips | Get user history | `?userId=user123&limit=5` | 200 | List of recent adventures | User experience |
| POST /api/MobileAdventure/DeviceSettings | Save preferences | `{"userId": "user123", "mapType": "satellite", "unitSystem": "metric"}` | 200 | Updated settings | Personalization |
