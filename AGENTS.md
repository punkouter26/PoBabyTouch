Foundation
Solution Naming: The .sln file name (e.g., PoProject.sln) is the base identifier. It must be used as the name for all Azure services/resource groups (e.g., PoProject) and the user-facing HTML <title>.
.NET Version: All projects must target .NET 9. The global.json file must be locked to a 9.0.xxx SDK version.
Package Management: All NuGet packages must be managed centrally in a Directory.Packages.props file at the repository root.
Null Safety: Nullable Reference Types (<Nullable>enable</Nullable>) must be enabled in all .csproj files.
2.  Architecture
Code Organization: The API must use Vertical Slice Architecture. All API logic (endpoints, CQRS handlers) must be co-located by feature in /src/Po.[AppName].Api/Features/.
Design Philosophy: Apply SOLID principles and standard GoF design patterns. Document their use in code comments or the PRD.
API Design: Use Minimal APIs and the CQRS pattern for all new endpoints.
Repository Structure: Adhere to the standard root folder structure: /src, /tests, /docs, /infra, and /scripts.
/src projects must follow the separation of concerns: ...Api, ...Client, and ...Shared (for DTOs/contracts).
/docs will contain the README.md, Prd.md, diagrams, and KQL query library.
Documentation Constraint: No .md files shall be created outside of README.md, Prd.md
All helper scripts that help development should be in /scripts folder
Mermaid diagram files and svg files go in /docs/diagrams/.
3.  Implementation
API & Backend
API Documentation: All API endpoints must have Swagger (OpenAPI) generation enabled. .http files must be maintained for manual verification.
Health Checks: Implement a health check endpoint at api/health that validates connectivity to all external dependencies.
Error Handling: All API calls must return robust, structured error details. Use structured ILogger.LogError within all catch blocks.
Frontend (Blazor)
UI Framework Principle: Microsoft.FluentUI.AspNetCore.Components is the primary component library. Radzen.Blazor may only be used for complex requirements not met by FluentUI.
Responsive Design: The UI must be mobile-first (portrait mode), responsive, fluid, and touch-friendly.
Development Environment
Debug Launch: The environment must support a one-step 'F5' debug launch for the API and browser. (Implementation: Commit a launch.json with a serverReadyAction to the repository).
Local Secrets: Use Azure Key Vault to store secrets (keys) for both local code and code deployed to azure
Local Storage: Emulate all required Azure Storage (Table, Blob) services locally. (Implementation: Use Azurite for local development and integration testing).






4.  Quality & Testing
Code Hygiene: All build warnings/errors must be resolved before a pushing changes to github. Run dotnet format to ensure style consistency.
Dependency Hygiene: Regularly check for and apply updates to all packages via Directory.Packages.props.
Workflow: Strictly follow a Test-Driven Development (TDD) workflow (Red -> Green -> Refactor).
Test Naming: Test methods must follow the MethodName_StateUnderTest_ExpectedBehavior convention.
Code Coverage (dotnet-coverage):
Enforce a minimum 80% line coverage threshold for all new business logic.
A combined coverage report must be generated in docs/coverage/.
Unit Tests (xUnit): Must cover all backend business logic (e.g., MediatR handlers) with all external dependencies mocked.
Component Tests (bUnit): Must cover all new Blazor components (rendering, user interactions, state changes), mocking dependencies like IHttpClientFactory
Integration Tests (xUnit): A "happy path" test must be created for every new API endpoint, running against a test host and an in-memory database emulator. Realistic test data should be generated.
E2E Tests (Playwright):
Tests must target Chromium (mobile and desktop views).
Use network interception to mock API responses for stable testing.
Integrate automated accessibility and visual regression checks.
5.  Operations & Azure
Provisioning: All Azure infrastructure must be provisioned using Bicep (from the /infra folder) and deployed via Azure Developer CLI (azd).
CI/CD: The GitHub Actions workflow must use Federated Credentials (OIDC) for secure, secret-less connection to Azure.
GitHub CI/CD / The YML file must simply build the code and deploy it to the resource group (same name as .sln) as an App Service (same name as .sln)
Required Services: Bicep scripts must provision, at minimum: Application Insights & Log Analytics, App Service, and Azure Storage to all be in the same resource group
Cost Management: A $5 monthly cost limit must be created for the application's resource group and if it goes over this amount the resource group must be disabled
Logging:
Use Serilog for all structured logging.
Configuration must be driven by appsettings.json to write to the Debug Console (in Development) and Application Insights (in Production).
Telemetry:
Use modern OpenTelemetry abstractions for all custom telemetry.
Traces: Use ActivitySource to create custom spans for key business actions.
Metrics: Use Meter to create custom metrics for business-critical values.
Production Diagnostics:
Enable the Application Insights Snapshot Debugger on the App Service.
Enable the Application Insights Profiler on the App Service.
KQL Library: The docs/kql/ folder must be populated with essential queries for monitoring health, performance, and custom business metrics.
