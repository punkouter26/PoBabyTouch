# PoBabyTouch

A fun and interactive baby touch game built with Blazor WebAssembly and .NET, designed to teach babies about cause and effect through colorful character circles with physics-based movement.

## 🎮 Quick Start

### Run Locally (Development)
1. **Start Azurite** (Azure Storage Emulator):
   ```powershell
   azurite --silent --location ./AzuriteData --debug ./AzuriteData/debug.log
   ```

2. **Launch the Game**:
   ```powershell
   dotnet run --project Server/PoBabyTouchGc.Server
   ```

3. **Open Browser**: Navigate to `http://localhost:5000`

### Run on Azure (Production)
Visit the live deployment: **https://pobabytouch.azurewebsites.net**

## 🌟 Features

- **Interactive Gameplay**: Touch moving circles featuring characters Matt, Nick, and Kim
- **Physics Engine**: Realistic circle movement with collision detection and boundary bouncing
- **High Score System**: Persistent leaderboards with Azure Table Storage integration
- **Multiple Game Modes**: Default, Easy, Hard, and Expert difficulty levels
- **Cloud Telemetry**: Real-time monitoring with Azure Application Insights
- **Responsive Design**: Optimized for mobile, tablet, and desktop devices
- **Real-time Diagnostics**: Built-in health monitoring for all Azure dependencies

## 🏗️ Architecture

**Frontend**: Blazor WebAssembly (.NET 9.0) with hosted backend  
**Backend**: ASP.NET Core Web API with Vertical Slice Architecture  
**Storage**: Azure Table Storage (production) / Azurite (development)  
**Monitoring**: Azure Application Insights with structured logging  
**Patterns**: Repository, Service Layer, Strategy, and Dependency Injection

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) (recommended)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) (for local development)

## 🛠️ Development Setup

### Local Development with Azurite

1. **Clone and Navigate**:
   ```powershell
   git clone https://github.com/punkouter26/PoBabyTouch.git
   cd PoBabyTouch
   ```

2. **Start Storage Emulator**:
   ```powershell
   azurite --silent --location ./AzuriteData --debug ./AzuriteData/debug.log
   ```

3. **Run Application**:
   ```powershell
   dotnet run --project Server/PoBabyTouchGc.Server
   ```

4. **Open Browser**: Navigate to `http://localhost:5000`

### Using Azure Resources

Replace `appsettings.Development.json` connection strings with your Azure resources and run normally.

## ☁️ Azure Deployment

The game is deployed to Azure App Service with the following resources:
- **Storage Account**: pobabytouch (Table Storage for high scores)
- **Application Insights**: PoBabyTouch (telemetry and monitoring)
- **App Service**: PoBabyTouch (hosted at https://pobabytouch.azurewebsites.net)
- **Resource Group**: PoBabyTouch (eastus2 region)

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run integration tests only
dotnet test --filter "Integration"
```

All tests include Azure Table Storage integration with automatic cleanup.

## 📁 Project Structure

```
PoBabyTouch/
├── Client/PoBabyTouchGc.Client/          # Blazor WebAssembly UI
├── Server/PoBabyTouchGc.Server/          # ASP.NET Core API
├── Shared/PoBabyTouchGc.Shared/          # Shared models and contracts
├── PoBabyTouchGc.Tests/                  # Integration and unit tests
├── infra/                                # Bicep infrastructure files
├── Diagram/                              # Mermaid diagrams and visuals
└── AzuriteData/                          # Local storage emulator data
```

## 📊 Monitoring

- **Health Check**: Visit `/api/health` for system status
- **Diagnostics**: Visit `/diag` for dependency health monitoring
- **Application Insights**: Real-time telemetry in Azure portal

---

*Built with ❤️ using Blazor WebAssembly and .NET 9*
