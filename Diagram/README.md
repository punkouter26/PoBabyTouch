# PoBabyTouch - Diagram Documentation

This folder contains Mermaid diagrams documenting the PoBabyTouch application architecture.

## Diagram Files

### 1. Project Dependency Diagram
- **File**: `ProjectDependency.mmd` / `SIMPLE_ProjectDependency.mmd`
- **Purpose**: Shows how .NET projects (.csproj), APIs, and Azure services are interconnected
- **Key Elements**: Client, Server, Shared, Tests, Azure Table Storage, Application Insights

### 2. Class Diagram
- **File**: `ClassDiagram.mmd` / `SIMPLE_ClassDiagram.mmd`
- **Purpose**: Models core domain entities, their properties, methods, and relationships
- **Key Elements**: HighScore, Services, Repositories, Validation, Request/Response models

### 3. Sequence Diagram
- **File**: `SequenceDiagram.mmd` / `SIMPLE_SequenceDiagram.mmd`
- **Purpose**: Traces API request flow from frontend through backend to Azure services
- **Key Feature**: High score submission and leaderboard retrieval workflows

### 4. Flowchart
- **File**: `Flowchart.mmd` / `SIMPLE_Flowchart.mmd`
- **Purpose**: Game logic flow including user interactions and decision points
- **Key Feature**: Complete game loop from start to high score save

### 5. Component Hierarchy
- **File**: `ComponentHierarchy.mmd` / `SIMPLE_ComponentHierarchy.mmd`
- **Purpose**: Blazor component nesting and relationships
- **Key Elements**: Pages, Layout, Shared Components, Services

## Viewing Diagrams

### Online Viewers
- [Mermaid Live Editor](https://mermaid.live/)
- Paste the contents of any `.mmd` file to view and edit

### VS Code
- Install the "Markdown Preview Mermaid Support" extension
- View diagrams directly in markdown preview

### Converting to SVG
To convert diagrams to SVG format:

```bash
# Install Mermaid CLI globally
npm install -g @mermaid-js/mermaid-cli

# Convert all diagrams
npx mmdc -i Diagram/ProjectDependency.mmd -o Diagram/ProjectDependency.svg
npx mmdc -i Diagram/SIMPLE_ProjectDependency.mmd -o Diagram/SIMPLE_ProjectDependency.svg
npx mmdc -i Diagram/ClassDiagram.mmd -o Diagram/ClassDiagram.svg
npx mmdc -i Diagram/SIMPLE_ClassDiagram.mmd -o Diagram/SIMPLE_ClassDiagram.svg
npx mmdc -i Diagram/SequenceDiagram.mmd -o Diagram/SequenceDiagram.svg
npx mmdc -i Diagram/SIMPLE_SequenceDiagram.mmd -o Diagram/SIMPLE_SequenceDiagram.svg
npx mmdc -i Diagram/Flowchart.mmd -o Diagram/Flowchart.svg
npx mmdc -i Diagram/SIMPLE_Flowchart.mmd -o Diagram/SIMPLE_Flowchart.svg
npx mmdc -i Diagram/ComponentHierarchy.mmd -o Diagram/ComponentHierarchy.svg
npx mmdc -i Diagram/SIMPLE_ComponentHierarchy.mmd -o Diagram/SIMPLE_ComponentHierarchy.svg
```

## Diagram Conventions

- **Green (#4CAF50)**: Entry points, success states, client layer
- **Blue (#2196F3)**: Server/API layer, active processes
- **Orange (#FF9800)**: Storage, data persistence
- **Purple (#9C27B0)**: Testing, validation
- **Cyan (#00BCD4)**: Monitoring, diagnostics
- **Red (#F44336)**: End states, errors

## Simplified Diagrams

Each `SIMPLE_*.mmd` file is a condensed version focusing on the core concepts, ideal for:
- Quick reference
- Presentations
- High-level overviews
- New team member onboarding
