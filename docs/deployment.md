# Deployment Guide

This document provides comprehensive deployment instructions for the ForAdventure AssetTag API, focusing on Azure cloud deployment, CI/CD pipelines, environment configuration, and monitoring strategies.

## Deployment Overview

The ForAdventure AssetTag API is designed for cloud-native deployment with Azure as the primary target platform, supporting scalable, reliable, and maintainable deployments.

### Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Cloud                             │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────┐  │
│  │   Azure CDN     │    │  Azure Front    │    │   Azure     │  │
│  │   (Global       │───▶│  Door (WAF +    │───▶│  App Service│  │
│  │   Caching)      │    │  Load Balancer) │    │             │  │
│  └─────────────────┘    └─────────────────┘    └─────────────┘  │
│                                                        │        │
│  ┌─────────────────────────────────────────────────────▼──────┐ │
│  │                  Application Tier                          │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐│ │
│  │  │  App Service    │  │  Azure Key      │  │  Application │││ │
│  │  │  (Primary)      │  │  Vault          │  │  Insights    │││ │
│  │  │  - API Hosting  │  │  - Secrets      │  │  - Monitoring│││ │
│  │  │  - Auto Scaling │  │  - Certificates │  │  - Logging   │││ │
│  │  └─────────────────┘  └─────────────────┘  └──────────────┘││ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                        │        │
│  ┌─────────────────────────────────────────────────────▼──────┐ │
│  │                     Data Tier                              │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐│ │
│  │  │  Azure SQL      │  │  Azure Cosmos   │  │  Azure       │││ │
│  │  │  Database       │  │  DB (NoSQL)     │  │  Storage     │││ │
│  │  │  - Relational   │  │  - Document     │  │  - Blobs     │││ │
│  │  │  - ACID         │  │  - Global Scale │  │  - Files     │││ │
│  │  └─────────────────┘  └─────────────────┘  └──────────────┘││ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Azure App Service Deployment

### Prerequisites

1. **Azure Subscription**: Active Azure subscription
2. **Azure CLI**: Installed and configured
3. **Git**: Version control access
4. **.NET 8 SDK**: For local development and validation

### Quick Deployment (Portal Method)

#### Step 1: Create App Service

1. **Navigate to Azure Portal**: https://portal.azure.com
2. **Create Resource** → **Web App**
3. **Configure Basic Settings**:
   ```
   Subscription: [Your Subscription]
   Resource Group: rg-foradventure-prod
   Name: foradventure-assettag-api
   Publish: Code
   Runtime Stack: .NET 8 (LTS)
   Operating System: Linux
   Region: West US 2 (or preferred region)
   ```

4. **Configure App Service Plan**:
   ```
   App Service Plan: plan-foradventure-prod
   Pricing Tier: B1 (Basic) or higher
   ```

#### Step 2: Configure Deployment

1. **Deployment Center** → **GitHub Actions**
2. **Connect GitHub Repository**:
   ```
   Organization: tcalice
   Repository: AdventureTags
   Branch: main
   ```

3. **Configure Build**:
   ```
   Runtime Stack: .NET
   Version: 8.0
   Build Command: dotnet build
   Startup Command: dotnet ForEveryAdventure.dll
   ```

### Infrastructure as Code (ARM Template)

#### ARM Template: `azuredeploy.json`

```json
{
    "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "appName": {
            "type": "string",
            "defaultValue": "foradventure-assettag-api",
            "metadata": {
                "description": "Name of the App Service"
            }
        },
        "location": {
            "type": "string",
            "defaultValue": "[resourceGroup().location]",
            "metadata": {
                "description": "Location for all resources"
            }
        },
        "sku": {
            "type": "string",
            "defaultValue": "B1",
            "allowedValues": [
                "F1",
                "B1",
                "B2",
                "B3",
                "S1",
                "S2",
                "S3",
                "P1v2",
                "P2v2",
                "P3v2"
            ],
            "metadata": {
                "description": "App Service Plan pricing tier"
            }
        }
    },
    "variables": {
        "appServicePlanName": "[concat('plan-', parameters('appName'))]",
        "keyVaultName": "[concat('kv-', uniqueString(resourceGroup().id))]",
        "applicationInsightsName": "[concat('ai-', parameters('appName'))]"
    },
    "resources": [
        {
            "type": "Microsoft.Web/serverfarms",
            "apiVersion": "2021-03-01",
            "name": "[variables('appServicePlanName')]",
            "location": "[parameters('location')]",
            "sku": {
                "name": "[parameters('sku')]"
            },
            "kind": "linux",
            "properties": {
                "reserved": true
            }
        },
        {
            "type": "Microsoft.Web/sites",
            "apiVersion": "2021-03-01",
            "name": "[parameters('appName')]",
            "location": "[parameters('location')]",
            "dependsOn": [
                "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]",
                "[resourceId('Microsoft.Insights/components', variables('applicationInsightsName'))]"
            ],
            "properties": {
                "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]",
                "siteConfig": {
                    "linuxFxVersion": "DOTNETCORE|8.0",
                    "appSettings": [
                        {
                            "name": "ASPNETCORE_ENVIRONMENT",
                            "value": "Production"
                        },
                        {
                            "name": "APPLICATIONINSIGHTS_CONNECTION_STRING",
                            "value": "[reference(resourceId('Microsoft.Insights/components', variables('applicationInsightsName')), '2020-02-02').ConnectionString]"
                        },
                        {
                            "name": "KeyVaultUri",
                            "value": "[concat('https://', variables('keyVaultName'), '.vault.azure.net/')]"
                        }
                    ],
                    "connectionStrings": []
                }
            },
            "identity": {
                "type": "SystemAssigned"
            }
        },
        {
            "type": "Microsoft.KeyVault/vaults",
            "apiVersion": "2021-11-01-preview",
            "name": "[variables('keyVaultName')]",
            "location": "[parameters('location')]",
            "properties": {
                "sku": {
                    "family": "A",
                    "name": "standard"
                },
                "tenantId": "[subscription().tenantId]",
                "accessPolicies": [
                    {
                        "tenantId": "[subscription().tenantId]",
                        "objectId": "[reference(resourceId('Microsoft.Web/sites', parameters('appName')), '2021-03-01', 'full').identity.principalId]",
                        "permissions": {
                            "secrets": [
                                "get",
                                "list"
                            ]
                        }
                    }
                ],
                "enabledForTemplateDeployment": true,
                "enableRbacAuthorization": false
            },
            "dependsOn": [
                "[resourceId('Microsoft.Web/sites', parameters('appName'))]"
            ]
        },
        {
            "type": "Microsoft.Insights/components",
            "apiVersion": "2020-02-02",
            "name": "[variables('applicationInsightsName')]",
            "location": "[parameters('location')]",
            "kind": "web",
            "properties": {
                "Application_Type": "web",
                "Request_Source": "rest"
            }
        }
    ],
    "outputs": {
        "appServiceUrl": {
            "type": "string",
            "value": "[concat('https://', parameters('appName'), '.azurewebsites.net')]"
        },
        "keyVaultUri": {
            "type": "string",
            "value": "[concat('https://', variables('keyVaultName'), '.vault.azure.net/')]"
        }
    }
}
```

#### ARM Parameters: `azuredeploy.parameters.json`

```json
{
    "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "appName": {
            "value": "foradventure-assettag-api-prod"
        },
        "sku": {
            "value": "B1"
        }
    }
}
```

### Bicep Template Alternative

#### Main Bicep File: `main.bicep`

```bicep
@description('Name of the App Service')
param appName string = 'foradventure-assettag-api'

@description('Location for all resources')
param location string = resourceGroup().location

@description('App Service Plan pricing tier')
@allowed([
  'F1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1v2'
  'P2v2'
  'P3v2'
])
param sku string = 'B1'

@description('Environment name')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'prod'

var appServicePlanName = 'plan-${appName}-${environment}'
var keyVaultName = 'kv-${uniqueString(resourceGroup().id)}'
var applicationInsightsName = 'ai-${appName}-${environment}'
var logAnalyticsWorkspaceName = 'law-${appName}-${environment}'

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: sku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2021-03-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'KeyVaultUri'
          value: keyVault.properties.vaultUri
        }
      ]
    }
    httpsOnly: true
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: appService.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
    enabledForTemplateDeployment: true
    enableRbacAuthorization: false
  }
}

// Output values
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output keyVaultUri string = keyVault.properties.vaultUri
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
```

### Deployment Commands

#### Azure CLI Deployment

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "Your-Subscription-Name"

# Create resource group
az group create --name rg-foradventure-prod --location "West US 2"

# Deploy ARM template
az deployment group create \
  --resource-group rg-foradventure-prod \
  --template-file azuredeploy.json \
  --parameters azuredeploy.parameters.json

# Deploy Bicep template (alternative)
az deployment group create \
  --resource-group rg-foradventure-prod \
  --template-file main.bicep \
  --parameters appName=foradventure-assettag-api environment=prod
```

#### PowerShell Deployment

```powershell
# Connect to Azure
Connect-AzAccount

# Set subscription context
Set-AzContext -SubscriptionName "Your-Subscription-Name"

# Create resource group
New-AzResourceGroup -Name "rg-foradventure-prod" -Location "West US 2"

# Deploy ARM template
New-AzResourceGroupDeployment `
  -ResourceGroupName "rg-foradventure-prod" `
  -TemplateFile "azuredeploy.json" `
  -TemplateParameterFile "azuredeploy.parameters.json"
```

## CI/CD Pipeline with GitHub Actions

### GitHub Actions Workflow

#### Main Deployment Workflow: `.github/workflows/deploy.yml`

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: foradventure-assettag-api-prod
  AZURE_WEBAPP_PACKAGE_PATH: './AssetTag.API/WebApplication1'
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
      working-directory: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
    
    - name: Build application
      run: dotnet build --configuration Release --no-restore
      working-directory: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
    
    - name: Run unit tests
      run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
      working-directory: ./AssetTag.API.test/AdventureTagTests
    
    - name: Generate test coverage report
      uses: danielpalme/ReportGenerator-GitHub-Action@5.1.26
      with:
        reports: '**/coverage.cobertura.xml'
        targetdir: 'coverage'
        reporttypes: 'Html;Cobertura'
    
    - name: Upload coverage reports to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/Cobertura.xml
        flags: unittests
        name: codecov-umbrella
    
    - name: Publish application
      run: dotnet publish --configuration Release --output ./publish
      working-directory: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
    
    - name: Upload artifact for deployment job
      uses: actions/upload-artifact@v3
      with:
        name: .net-app
        path: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}/publish

  deploy-to-staging:
    runs-on: ubuntu-latest
    needs: build-and-test
    if: github.ref == 'refs/heads/main'
    environment:
      name: 'staging'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}
    
    steps:
    - name: Download artifact from build job
      uses: actions/download-artifact@v3
      with:
        name: .net-app
    
    - name: Deploy to Azure Web App (Staging)
      id: deploy-to-webapp
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}-staging
        slot-name: 'staging'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_STAGING }}
        package: .
    
    - name: Run smoke tests
      run: |
        # Wait for deployment to be ready
        sleep 30
        
        # Basic health check
        curl -f -s ${{ steps.deploy-to-webapp.outputs.webapp-url }}/health || exit 1
        
        # API endpoint test
        curl -f -s ${{ steps.deploy-to-webapp.outputs.webapp-url }}/swagger/v1/swagger.json || exit 1

  deploy-to-production:
    runs-on: ubuntu-latest
    needs: deploy-to-staging
    if: github.ref == 'refs/heads/main'
    environment:
      name: 'production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}
    
    steps:
    - name: Download artifact from build job
      uses: actions/download-artifact@v3
      with:
        name: .net-app
    
    - name: Deploy to Azure Web App (Production)
      id: deploy-to-webapp
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: .
    
    - name: Run production smoke tests
      run: |
        # Wait for deployment to be ready
        sleep 30
        
        # Health check
        curl -f -s ${{ steps.deploy-to-webapp.outputs.webapp-url }}/health || exit 1
        
        # Verify API is responding
        response=$(curl -s -o /dev/null -w "%{http_code}" ${{ steps.deploy-to-webapp.outputs.webapp-url }}/swagger/v1/swagger.json)
        if [ $response -ne 200 ]; then
          echo "Production deployment verification failed"
          exit 1
        fi
        
        echo "Production deployment verified successfully"
```

#### Security Scanning Workflow: `.github/workflows/security.yml`

```yaml
name: Security Scanning

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 2 * * 1' # Weekly on Monday at 2 AM

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Run .NET Security Analysis
      run: |
        dotnet list package --vulnerable --include-transitive || true
        dotnet list package --deprecated || true
```

### Environment Configuration

#### Development Environment

```yaml
# appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ForAdventureAssetTagDev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7034"
  }
}
```

#### Staging Environment

**App Service Configuration:**
```bash
# Application Settings
ASPNETCORE_ENVIRONMENT=Staging
APPLICATIONINSIGHTS_CONNECTION_STRING=[From Key Vault]
ConnectionStrings__DefaultConnection=[From Key Vault]
ApiSettings__BaseUrl=https://foradventure-assettag-api-staging.azurewebsites.net

# Connection Strings (Alternative to App Settings)
DefaultConnection=[Staging Database Connection String]
```

#### Production Environment

**App Service Configuration:**
```bash
# Application Settings
ASPNETCORE_ENVIRONMENT=Production
APPLICATIONINSIGHTS_CONNECTION_STRING=[From Key Vault]
ConnectionStrings__DefaultConnection=[From Key Vault]
ApiSettings__BaseUrl=https://foradventure-assettag-api-prod.azurewebsites.net

# Security Settings
WEBSITE_HTTPLOGGING_RETENTION_DAYS=7
WEBSITE_LOAD_CERTIFICATES=*
```

### Environment Secrets Management

#### GitHub Secrets Configuration

Required secrets for GitHub Actions:

```
# Azure Deployment
AZURE_WEBAPP_PUBLISH_PROFILE          # Production publish profile
AZURE_WEBAPP_PUBLISH_PROFILE_STAGING  # Staging publish profile
AZURE_CLIENT_ID                       # Service Principal ID
AZURE_CLIENT_SECRET                   # Service Principal Secret
AZURE_TENANT_ID                       # Azure Tenant ID
AZURE_SUBSCRIPTION_ID                 # Azure Subscription ID

# Database
DATABASE_CONNECTION_STRING_PROD       # Production database
DATABASE_CONNECTION_STRING_STAGING    # Staging database

# External Services
API_KEY_EXTERNAL_SERVICE              # Third-party API keys
```

#### Azure Key Vault Integration

**Key Vault Configuration:**

```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["KeyVaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}
```

**Key Vault Secrets:**
- `ConnectionStrings--DefaultConnection`
- `ApplicationInsights--ConnectionString`
- `ExternalApi--ApiKey`
- `Jwt--SecretKey`

## Monitoring and Logging

### Application Insights Configuration

#### Program.cs Configuration

```csharp
// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
});

// Add custom telemetry
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

// Add logging
builder.Logging.AddApplicationInsights();
```

#### Custom Telemetry Initializer

```csharp
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Component.Version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        
        telemetry.Context.GlobalProperties["Environment"] = 
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
    }
}
```

### Log Analytics Queries

#### Performance Monitoring

```kql
// Average response time by endpoint
requests
| where timestamp > ago(1h)
| summarize avg(duration) by name
| order by avg_duration desc

// Error rate by endpoint
requests
| where timestamp > ago(24h)
| summarize total = count(), errors = countif(success == false) by name
| extend error_rate = (errors * 100.0) / total
| order by error_rate desc

// Top exceptions
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc
```

#### Custom Metrics

```csharp
public class AssetTagController : ControllerBase
{
    private readonly ILogger<AssetTagController> _logger;
    private readonly TelemetryClient _telemetryClient;

    public AssetTagController(
        IAssetTagStore store, 
        ILogger<AssetTagController> logger,
        TelemetryClient telemetryClient)
    {
        _store = store;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    [HttpPost("MakeAssetTag")]
    public IActionResult MakeAssetTag([FromBody] AssetTag assetTag)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Business logic
            var newTag = CreateAssetTag(assetTag);
            
            // Track success metrics
            _telemetryClient.TrackMetric("AssetTag.Created", 1);
            _telemetryClient.TrackEvent("AssetTag.Creation.Success", 
                new Dictionary<string, string>
                {
                    ["TagCode"] = assetTag.TagCode,
                    ["EmergencyContactsCount"] = assetTag.EmergencyContacts.Count.ToString(),
                    ["TripPlansCount"] = assetTag.TripPlans.Count.ToString()
                });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            _telemetryClient.TrackMetric("AssetTag.Creation.Error", 1);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _telemetryClient.TrackMetric("AssetTag.Creation.Duration", 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Health Checks

#### Health Check Configuration

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<ExternalApiHealthCheck>("external-api");

// Configure health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

#### Custom Health Checks

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IAssetTagStore _store;

    public DatabaseHealthCheck(IAssetTagStore store)
    {
        _store = store;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connectivity
            var count = _store.AssetTags.Count;
            return Task.FromResult(HealthCheckResult.Healthy($"Database accessible. Asset count: {count}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Database not accessible", ex));
        }
    }
}
```

## Performance Optimization

### App Service Configuration

#### Scaling Configuration

```json
{
  "autoScaleSettings": {
    "enabled": true,
    "profiles": [
      {
        "name": "Auto created scale condition",
        "capacity": {
          "minimum": "1",
          "maximum": "10",
          "default": "1"
        },
        "rules": [
          {
            "scaleAction": {
              "direction": "Increase",
              "type": "ChangeCount",
              "value": "1",
              "cooldown": "PT5M"
            },
            "metricTrigger": {
              "metricName": "CpuPercentage",
              "operator": "GreaterThan",
              "threshold": 70,
              "timeAggregation": "Average",
              "timeGrain": "PT1M",
              "timeWindow": "PT5M"
            }
          },
          {
            "scaleAction": {
              "direction": "Decrease",
              "type": "ChangeCount",
              "value": "1",
              "cooldown": "PT10M"
            },
            "metricTrigger": {
              "metricName": "CpuPercentage",
              "operator": "LessThan",
              "threshold": 30,
              "timeAggregation": "Average",
              "timeGrain": "PT1M",
              "timeWindow": "PT10M"
            }
          }
        ]
      }
    ]
  }
}
```

### Application Performance

#### Response Caching

```csharp
// In Program.cs
builder.Services.AddResponseCaching();

// In controller
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "tagCode" })]
public IActionResult GetAssetTag(string tagCode)
{
    // Implementation
}
```

#### Output Caching (.NET 8)

```csharp
// In Program.cs
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
    options.AddPolicy("AssetTagPolicy", builder => 
        builder.Cache()
               .Expire(TimeSpan.FromMinutes(5))
               .VaryByQuery("tagCode"));
});

app.UseOutputCache();

// In controller
[OutputCache(PolicyName = "AssetTagPolicy")]
public IActionResult GetAssetTag(string tagCode)
{
    // Implementation
}
```

## Disaster Recovery

### Backup Strategy

#### App Service Backup

```powershell
# Configure automated backups
$resourceGroup = "rg-foradventure-prod"
$webAppName = "foradventure-assettag-api-prod"
$storageAccountName = "safooradventurebackup"
$containerName = "backups"

# Create backup configuration
$backupConfig = @{
    Name = "Daily-Backup"
    Enabled = $true
    StorageAccountUrl = "https://$storageAccountName.blob.core.windows.net/$containerName"
    FrequencyInterval = "1"
    FrequencyUnit = "Day"
    RetentionPeriodInDays = "30"
    StartTime = "02:00"
}

New-AzWebAppBackup -ResourceGroupName $resourceGroup -Name $webAppName @backupConfig
```

#### Database Backup

For Azure SQL Database:
- **Automated Backups**: Built-in point-in-time recovery
- **Long-term Retention**: Configure for compliance requirements
- **Geo-redundant Backup**: Cross-region backup replication

### Multi-Region Deployment

#### Traffic Manager Configuration

```json
{
  "type": "Microsoft.Network/trafficmanagerprofiles",
  "apiVersion": "2018-08-01",
  "name": "foradventure-api-tm",
  "location": "global",
  "properties": {
    "profileStatus": "Enabled",
    "trafficRoutingMethod": "Priority",
    "dnsConfig": {
      "relativeName": "foradventure-api",
      "ttl": 30
    },
    "monitorConfig": {
      "protocol": "HTTPS",
      "port": 443,
      "path": "/health"
    },
    "endpoints": [
      {
        "type": "Microsoft.Network/trafficmanagerprofiles/azureEndpoints",
        "name": "primary-endpoint",
        "properties": {
          "targetResourceId": "[resourceId('Microsoft.Web/sites', 'foradventure-assettag-api-westus')]",
          "priority": 1
        }
      },
      {
        "type": "Microsoft.Network/trafficmanagerprofiles/azureEndpoints", 
        "name": "secondary-endpoint",
        "properties": {
          "targetResourceId": "[resourceId('Microsoft.Web/sites', 'foradventure-assettag-api-eastus')]",
          "priority": 2
        }
      }
    ]
  }
}
```

## Security Configuration

### App Service Security

#### Network Security

```csharp
// IP Restrictions (via ARM template)
"ipSecurityRestrictions": [
    {
        "ipAddress": "0.0.0.0/0",
        "action": "Allow",
        "priority": 1000,
        "name": "Allow all",
        "description": "Allow all access"
    }
],
"scmIpSecurityRestrictions": [
    {
        "ipAddress": "10.0.0.0/8",
        "action": "Allow", 
        "priority": 1000,
        "name": "Allow internal network",
        "description": "Allow access from internal network only"
    }
]
```

#### SSL/TLS Configuration

```json
{
  "properties": {
    "httpsOnly": true,
    "siteConfig": {
      "minTlsVersion": "1.2",
      "http20Enabled": true,
      "ftpsState": "Disabled"
    }
  }
}
```

### Application Security

#### CORS Configuration

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});

if (app.Environment.IsProduction())
{
    app.UseCors("ProductionPolicy");
}
else
{
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
```

#### Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
    
    await next();
});
```

## Troubleshooting

### Common Deployment Issues

#### 1. Build Failures

**Issue**: Build fails during CI/CD
**Solution**:
```bash
# Check build logs
az webapp log tail --name foradventure-assettag-api-prod --resource-group rg-foradventure-prod

# Verify .NET version
dotnet --version

# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --configuration Release
```

#### 2. Runtime Errors

**Issue**: Application fails to start
**Solution**:
```bash
# Check application logs
az webapp log show --name foradventure-assettag-api-prod --resource-group rg-foradventure-prod

# Verify environment variables
az webapp config appsettings list --name foradventure-assettag-api-prod --resource-group rg-foradventure-prod

# Test locally with production settings
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

#### 3. Database Connection Issues

**Issue**: Cannot connect to database
**Solution**:
```bash
# Test connection string
sqlcmd -S server.database.windows.net -d database -U username -P password

# Check firewall rules
az sql server firewall-rule list --server servername --resource-group rg-foradventure-prod

# Verify Key Vault access
az keyvault secret show --vault-name keyvaultname --name ConnectionStrings--DefaultConnection
```

### Performance Troubleshooting

#### Application Insights Queries

```kql
// Slow requests
requests
| where timestamp > ago(1h)
| where duration > 5000
| order by duration desc
| take 20

// Memory usage
performanceCounters
| where timestamp > ago(1h)
| where category == "Memory"
| summarize avg(value) by bin(timestamp, 5m)
| render timechart
```

---

This deployment guide provides comprehensive instructions for deploying the ForAdventure AssetTag API to Azure with best practices for security, monitoring, and scalability. Regular review and updates of deployment processes ensure reliable and efficient application delivery.