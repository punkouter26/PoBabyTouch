Standardized Namespaces: Apply a consistent short prefix based on the solution name (e.g., Po<SolutionName>.*) across all project namespaces.
Testing Suite: Maintain three distinct test projects for every solution (Use docker testcontainers for database)
.NET Unit Tests
.NET Integration Tests (targeting the API)
Playwright E2E Tests (TypeScript-based)
Testing Workflow: Create new tests simultaneously with new functionality to verify code behavior.
Playwright Constraints: Limit TypeScript Playwright tests to Chromium and Mobile rendering only.
Context Optimization: Maintain a .copilotignore file to restrict AI attention to relevant source code and configurations only.
Make sure a .gitignore file exists that works well for .NET 10 projects
.NET BLAZOR WEB APP
Framework: Target .NET 10 using the Unified Blazor Web App templates as the starting point.
Use Interactive Auto render mode feature
Architecture: Adhere to the official Blazor Web App project structure.
Rendering Strategy: Utilize a Blazor Server project (SSR) and a Blazor Client project (WASM). Default to Server-side rendering unless low-latency responsive rendering (e.g., games) is required.
Package Management: Enforce Central Package Management (CPM) via Directory.Packages.props with transitive pinning enabled.
Quality Gates: Set <TreatWarningsAsErrors>true</TreatWarningsAsErrors> for all projects to eliminate technical debt accumulation.
Modern Tooling: Use Context7 MCP to fetch and maintain the latest SDKs and NuGet versions.
Vertical Slice Architecture (VSA): Organize projects by features rather than layers. Group Endpoints, DTOs, and Logic within single, flattened feature folders.
UI Components: Use standard Blazor components by default; incorporate Radzen Blazor components when advanced UI functionality is required.
ASPIRE
Orchestration: Use .NET Aspire (AppHost and ServiceDefaults) for all local and cloud orchestration.
Tooling: Ensure the Aspire CLI is installed and updated across the development environment.
Keep all ports static and defined in the appSettings.json file so I can easily connect to Aspire/Database
AZURE DEPLOYMENT AS ACA
Service Discovery: Use Aspire project references for service discovery; never hardcode ports.
Hybrid Secret Strategy:
Local: Use dotnet user-secrets for primary development.
Cloud/Fallback: Use Azure Key Vault via Managed Identity for production and shared team secrets.
Storage: Store all keys/secrets in the PoShared resource group. Use these as a fallback locally if user-secrets do not exist.
Telemetry: Enable OpenTelemetry (Logs, Traces, Metrics) across all services. Aggregate data in Application Insights within the PoShared resource group.
Deployment Workflow:
Use azd up to generate and deploy infrastructure derived directly from the Aspire model.
Verify that CI/CD pipelines successfully deploy to Azure without errors upon every GitHub push.
Unified Naming: Use the Solution Name as the master identifier for all Azure Resource Groups and environments.
