# AI Agent Instructions for PoBabyTouch

## 1.0 General Principles

### 1.1 Source of Truth & Naming
Our chat conversation is the source of truth. Your initial prompt will describe the application, including its name (e.g., AppName). All projects and the solution will be prefixed with **Po** (e.g., PoAppName).

### 1.2 Design Philosophy
Prioritize simplicity, correctness, and expandability. Adhere to SOLID principles. If a Gang of Four (GoF) design pattern is used, note it with a comment (e.g., `// Applying Strategy Pattern`).

### 1.3 Debugging Workflow
I will generate code that writes server-side logs to log.txt. To debug, you will run the code and then provide me with the full console output and the complete contents of log.txt. I will then analyze them for errors.

## 2.0 Interactive Workflow

### 2.1 Clarify, Propose, Confirm
Based on your initial request, I will ask any necessary clarifying questions. I will then propose a best-practice architecture (e.g., "For this project, Vertical Slice Architecture is appropriate. Do you agree?") and await your confirmation before generating code.

### 2.2 Focused Execution
I will execute only the immediate task we have agreed upon. After completion, I will confirm its success and ask, "What is the next step?"

### 2.3 Failure Protocol
If you report a failure, I will stop. You must provide the entire error message and the full contents of log.txt. I will analyze this information and provide a fix.

### 2.4 File Cleanup
If I identify potentially unused files/code, I will list them and ask for your permission before generating commands to remove them.

## 3.0 Solution & Code Structure

### 3.1 CLI-Based Scaffolding
For new projects, I will provide a complete, numbered sequence of dotnet CLI commands to be executed in order. This will create the entire solution and project structure. I will not use scripts.

Example Sequence:
```bash
dotnet new sln --name PoAppName
dotnet new blazor -o src/PoAppName.Client --hosted
dotnet new xunit -o tests/PoAppName.UnitTests
dotnet sln add src/PoAppName.Client/Server/PoAppName.Client.Server.csproj
```

### 3.2 Mandatory Root Structure
```
/PoAppName/
├── .github/workflows/deploy.yml
├── .vscode/ (launch.json, tasks.json)
├── AzuriteData/
├── src/
│   ├── PoAppName.Api/ (hosts the blazor webassembly project)
│   ├── PoAppName.Application/
│   ├── PoAppName.Client/ (blazor webassembly)
│   ├── PoAppName.Domain/
│   └── PoAppName.Infrastructure/
├── tests/
│   ├── PoAppName.ApiTests/
│   ├── PoAppName.IntegrationTests/
│   └── PoAppName.UnitTests/
├── .editorconfig
├── .gitignore
├── PoAppName.sln
├── README.md
└── log.txt
```

## 4.0 Backend (C# / .NET)

### 4.1 Framework & Architecture
Target the latest stable .NET. Use the architecture we agree upon (defaulting to Vertical Slice or Onion Architecture (you decide)). Justify the choice in a Program.cs comment.

### 4.2 Standards
- Use Dependency Injection
- Implement a global exception handler
- For external HTTP calls, use Polly to implement the Circuit Breaker pattern

## 5.0 Azure Integration

### 5.1 Command Generation
I will provide you with the exact az cli commands to retrieve keys, connection strings, or other values. You will execute these commands. I will use placeholders in the code (e.g., `builder.Configuration["MySecret"]`) where the retrieved values should be configured.

### 5.2 Table Storage
Use Azurite for local dev. Name Azure tables PoAppName[TableName].

## 6.0 Frontend Development (UI)

### 6.1 Blazor WebAssembly
Create a hosted Blazor WebAssembly project. Use Radzen for complex controls when needed.

## 7.0 Testing & Quality

### 7.1 Framework & Approach
Use xUnit. Create services and their integration tests first. Confirm tests pass before UI implementation.

### 7.2 Structure
Use separate tests/ sub-projects as defined in the solution structure.

## 8.0 Logging & Diagnostics

### 8.1 Logging Implementation
I will implement Serilog configured to log to the Console and a rolling log.txt file in the project root, with a default level of Debug.

### 8.2 Mandatory Diagnostics View
All UI apps must have a diagnostics view (/diag or Diag.tscn) that checks and displays the real-time status of critical dependencies.
