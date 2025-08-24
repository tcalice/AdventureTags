# ForAdventure AssetTag API

A comprehensive .NET 8 Web API for outdoor adventure tracking and safety management. The AssetTag system enables adventurers to create digital asset tags that contain emergency contacts, trip plans, and location data for enhanced safety during outdoor activities.

## 🎯 Overview

ForAdventure AssetTag API is designed to support outdoor enthusiasts by providing a digital safety net through asset tags that contain crucial information for emergency situations. Each asset tag serves as a digital identifier linked to emergency contacts, detailed trip plans, and location coordinates.

### Key Features

- **Digital Asset Tag Creation**: Generate unique asset tags with QR codes for outdoor gear
- **Emergency Contact Management**: Store and manage emergency contact information
- **Trip Planning Integration**: Detailed trip plans with GPS coordinates and route information
- **Location Services**: Multiple GPS format support including What3Words integration
- **RESTful API**: OpenAPI/Swagger documented endpoints
- **In-Memory Storage**: Fast, lightweight data storage for development and testing

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   HTTP Client   │───▶│  AssetTag API    │───▶│  Data Storage   │
│                 │    │                  │    │                 │
│ - Web Browser   │    │ - Controllers    │    │ - IAssetTagStore│
│ - Mobile App    │    │ - Services       │    │ - In-Memory     │
│ - QR Scanner    │    │ - Models         │    │ - (Future: DB)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Core Components

- **Models**: AssetTag, EmergencyContact, TripPlan, LocationCoordinates
- **Controllers**: AssetTagController for HTTP endpoint handling
- **Services**: AdventureAPIService for external integrations, ForAdventureLogic for business logic
- **Storage**: IAssetTagStore interface with in-memory implementation
- **API Endpoints**: Both controller-based and minimal API endpoints

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or Visual Studio Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/tcalice/AdventureTags.git
   cd AdventureTags
   ```

2. **Build the solution**
   ```bash
   cd AssetTag.API/WebApplication1
   dotnet restore
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the API**
   - API Base URL: `https://localhost:7034` (or `http://localhost:5034`)
   - Swagger UI: `https://localhost:7034/swagger`
   - API Documentation: `https://localhost:7034/swagger/v1/swagger.json`

### Running Tests

```bash
cd AssetTag.API.test/AdventureTagTests
dotnet test
```

## 📝 API Usage Examples

### Create an Asset Tag

```http
POST /api/AssetTag/MakeAssetTag
Content-Type: application/json

{
  "tagCode": "ADV-2024-001",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "emergencyContacts": [
    {
      "name": "John Doe",
      "phone": "+1-555-0123",
      "email": "john.doe@example.com"
    }
  ],
  "tripPlans": [
    {
      "tripRoute": "Mount Rainier Summit Trail",
      "tripStartDate": "2024-07-15T08:00:00Z",
      "tripEndDate": "2024-07-17T18:00:00Z",
      "tripDurationDays": 3
    }
  ]
}
```

### Response

```json
{
  "message": "Retrieve your Asset Sticker with this Unique Asset Tag ID",
  "assetTagId": "987fcdeb-51a2-43d1-b5c6-789012345678"
}
```

## 📊 Project Structure

```
AdventureTags/
├── AssetTag.API/
│   └── WebApplication1/           # Main API project
│       ├── Controllers/           # HTTP controllers
│       ├── Models/               # Data models and interfaces
│       ├── Services/             # Business logic and external services
│       ├── Properties/           # Launch settings
│       └── Program.cs            # Application entry point
├── AssetTag.API.test/
│   └── AdventureTagTests/        # Unit tests
├── docs/                         # Documentation
└── README.md                     # This file
```

## 🔧 Configuration

### Development Settings

The application uses standard .NET configuration patterns:

- `appsettings.json`: Production settings
- `appsettings.Development.json`: Development overrides
- Environment variables: Override any configuration

### Dependency Injection

The application uses .NET's built-in DI container with the following services:

```csharp
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAssetTagStore, AssetTagStore>();
```

## 🧪 Testing

The project includes comprehensive unit tests using:

- **xUnit**: Testing framework
- **Moq**: Mocking framework  
- **Microsoft.NET.Test.SDK**: Test runner

See [Testing Guide](docs/testing.md) for detailed testing strategies and coverage information.

## 🚀 Deployment

For production deployment options:

- **Azure App Service**: Recommended for cloud deployment
- **Docker**: Container-based deployment
- **IIS**: On-premises Windows deployment

See [Deployment Guide](docs/deployment.md) for detailed deployment instructions.

## 📖 Documentation

- [Architecture Documentation](docs/architecture.md) - System design and request flow
- [API Documentation](docs/api.md) - Comprehensive API reference
- [Testing Guide](docs/testing.md) - Testing strategies and coverage
- [Deployment Guide](docs/deployment.md) - Azure deployment and CI/CD
- [Database Design](docs/database.md) - Data storage architecture
- [Development Workflow](docs/development.md) - Contributing guidelines

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For questions and support:

- Create an [Issue](https://github.com/tcalice/AdventureTags/issues)
- Review the [Documentation](docs/)
- Check the [API Reference](docs/api.md)

## 🗺️ Roadmap

### Current Version (v1.0)
- ✅ Basic asset tag creation
- ✅ Emergency contact management
- ✅ In-memory data storage
- ✅ OpenAPI documentation

### Future Enhancements
- 🔄 Azure SQL Database integration
- 🔄 Real-time GPS tracking
- 🔄 QR code generation
- 🔄 Mobile app integration
- 🔄 Emergency alert system
- 🔄 Advanced trip analytics

---

Built with ❤️ for the outdoor adventure community