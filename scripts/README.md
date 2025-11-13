# PoBabyTouch Development Scripts

This folder contains helper scripts for local development.

## Available Scripts

### start-azurite.ps1
Starts Azurite (Azure Storage Emulator) for local development and testing.

**Usage:**
```powershell
.\scripts\start-azurite.ps1
```

**What it does:**
- Checks if Azurite is installed (installs via npm if missing)
- Starts Azurite with data stored in `AzuriteData` folder
- Provides connection endpoints for local development

**Stopping Azurite:**
```powershell
Stop-Process -Name azurite
```

## Prerequisites

- Node.js and npm must be installed
- PowerShell execution policy must allow script execution:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

## Local Development Workflow

1. Start Azurite: `.\scripts\start-azurite.ps1`
2. Build solution: `dotnet build`
3. Run tests: `dotnet test`
4. Start API: Press F5 in VS Code or `dotnet run --project src/PoBabyTouchGc.Api`
