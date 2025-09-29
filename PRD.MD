# PoBabyTouchGc - Product Requirements Document (PRD)

## Table of Contents
1. [Application Overview](#application-overview)
2. [UI Component Architecture](#ui-component-architecture)
3. [Domain Model](#domain-model)
4. [API Endpoints](#api-endpoints)
5. [Technical Stack](#technical-stack)
6. [Quality & Testing](#quality--testing)

## Application Overview

### Product Vision
PoBabyTouchGc is an interactive baby touch game built with Blazor WebAssembly and .NET, designed to provide a fun and engaging way for babies to learn about cause and effect through colorful circle-touching gameplay.

### Core Features
- **Real-time Circle Game**: Dynamic circles with physics-based movement featuring characters (Matt, Nick, Kim)
- **High Score System**: Player leaderboards with 3-letter initials and game mode support
- **Multiple Game Modes**: Default, Easy, Hard, and Expert difficulty levels
- **Responsive Design**: Touch-optimized interface for mobile and tablet devices
- **Azure Cloud Integration**: Persistent storage and telemetry with Application Insights

### Target Audience
- Primary: Parents with babies/toddlers (6 months - 3 years)
- Secondary: Early childhood development centers and daycare facilities

### Success Metrics
- User engagement duration (target: 3+ minutes per session)
- High score submissions (target: 30% of game completions)
- Repeat usage patterns (target: 70% return within 7 days)

## UI Component Architecture

### Page Components

#### 1. Home Page (`/`)
- **Purpose**: Main landing page and navigation hub
- **Key Elements**:
  - Game title with branding
  - "Play Game" primary action button
  - "Leaderboard" secondary navigation
  - Version footer
- **User Flow**: Entry point → Game selection → Game launch

#### 2. Game Page (`/game`)
- **Purpose**: Core gameplay experience with real-time interaction
- **Key Elements**:
  - Score display (top-left header)
  - Timer countdown (top-right header)
  - Game area with moving circles (Matt/Nick/Kim characters)
  - High score modal (when applicable)
  - Game over overlay with replay options
- **Interactions**:
  - Circle clicking for score increment
  - Background click to start game
  - High score initials entry (3-letter validation)
- **State Management**: GameStateService integration for score/timing

#### 3. High Scores Page (`/highscores`)
- **Purpose**: Comprehensive leaderboard with filtering
- **Key Elements**:
  - Game mode selector dropdown
  - Top 20 scores display with ranking
  - Medal icons for top 3 positions
  - Auto-refresh functionality
  - Back-to-game navigation
- **Data Binding**: Real-time API integration with Azure Table Storage

#### 4. Leader Page (`/leader`)
- **Purpose**: Simplified leaderboard view
- **Key Elements**:
  - Top 10 scores only
  - Clean, focused display
  - Navigation buttons to game and home
- **Use Case**: Quick score checking without detailed filtering

#### 5. Diagnostics Page (`/diag`)
- **Purpose**: System health monitoring and troubleshooting
- **Key Elements**:
  - API connection status
  - Azure Table Storage connectivity
  - Application Insights telemetry status
  - Real-time dependency health checks
- **Target Users**: Developers and system administrators

### Shared Components

#### HighScoreDisplay Component
- **Reusable Display**: Used across multiple pages (game, highscores, leader)
- **Configuration Props**:
  - `GameMode`: Filter scores by difficulty
  - `MaxScores`: Limit display count (5, 10, 20)
  - `ShowRefreshButton`: Manual refresh capability
  - `AutoRefresh`: Automatic data polling
  - `AutoRefreshInterval`: Polling frequency control
- **Visual Features**:
  - Ranking badges (🥇🥈🥉 for top 3)
  - Score formatting with comma separators
  - Date/time stamps for score achievement
  - Loading states and error handling

#### Layout Components
- **MainLayout**: Primary application shell with navigation
- **NavMenu**: Responsive navigation with game-themed icons
  - Home (🏠)
  - Game (🕹️)
  - High Scores (🏆)
  - Diagnostics (🔧)

### UI/UX Design Principles

#### Visual Design
- **Color Scheme**: Orange/black retro gaming aesthetic
- **Typography**: 'Courier New' monospace for retro feel
- **Gradients**: Purple-blue backgrounds for modern appeal
- **Accessibility**: High contrast ratios and large touch targets

#### Interaction Design
- **Touch-First**: Optimized for finger interaction on mobile devices
- **Immediate Feedback**: Visual responses to all user actions
- **Error Prevention**: Input validation and user guidance
- **Progressive Disclosure**: Simple → Advanced features flow

#### Responsive Behavior
- **Mobile-First**: Primary design for phone/tablet usage
- **Desktop Scaling**: Appropriate sizing for larger screens
- **Touch Zones**: Minimum 44px touch targets
- **Performance**: 60fps animation targets for smooth gameplay

## Domain Model

### Core Entities

#### HighScore Entity
```csharp
public class HighScore : ITableEntity
{
    public string PlayerInitials { get; set; }    // 3-letter player identification
    public int Score { get; set; }                // Game score (points earned)
    public string GameMode { get; set; }          // Default/Easy/Hard/Expert
    public DateTime ScoreDate { get; set; }       // Achievement timestamp
    
    // Azure Table Storage properties
    public string PartitionKey { get; set; }      // GameMode for performance
    public string RowKey { get; set; }            // Negative score + timestamp for sorting
}
```

#### GameCircle Entity (Client-Side)
```csharp
public class GameCircle
{
    public Guid Id { get; set; }                  // Unique circle identifier
    public float X, Y { get; set; }               // Screen position
    public int Radius { get; set; }               // Circle size
    public float VelocityX, VelocityY { get; set; } // Movement physics
    public bool IsVisible { get; set; }           // Render state
    public bool IsHit { get; set; }               // Animation trigger
    public string Person { get; set; }            // Character (Matt/Nick/Kim)
    public string PersonClass { get; set; }       // CSS styling
}
```

#### GameState (Client Service)
```csharp
public class GameStateService
{
    public int CurrentScore { get; private set; }     // Live score tracking
    public bool IsGameActive { get; private set; }    // Gameplay state
    public bool IsGameOver { get; private set; }      // End game state
    public event Action OnChange;                     // State change notifications
}
```

### Request/Response Models

#### SaveHighScoreRequest
```csharp
public class SaveHighScoreRequest
{
    public string PlayerInitials { get; set; }    // Validated 3-letter input
    public int Score { get; set; }                // Final game score
    public string GameMode { get; set; }          // Selected difficulty
}
```

#### ApiResponse<T>
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }             // Operation status
    public string Message { get; set; }           // User-friendly message
    public T Data { get; set; }                   // Typed response data
    public string ErrorCode { get; set; }         // Error classification
    public DateTime Timestamp { get; set; }       // Response time
}
```

#### ClientLogRequest (Telemetry)
```csharp
public class ClientLogRequest
{
    public string Level { get; set; }             // Debug/Info/Warning/Error
    public string Message { get; set; }           // Log content
    public string Category { get; set; }          // Log classification
    public Dictionary<string, object> Properties { get; set; } // Structured data
}
```

### Business Rules

#### Score Validation
- **Range**: 0 to 999,999 points maximum
- **Increment**: +1 point per circle click
- **Time Bonus**: No time-based scoring (future enhancement)

#### High Score Logic
- **Top N Storage**: Maximum 10 scores per game mode
- **Tie Breaking**: Later scores rank higher than earlier identical scores
- **Initials Validation**: Exactly 3 alphabetic characters, auto-uppercase
- **Duplicate Handling**: Same player can have multiple entries

#### Game Physics
- **Circle Count**: 7 simultaneous circles
- **Movement**: Random velocity with boundary collision detection
- **Spawn Logic**: Non-overlapping placement with retry mechanism
- **Character Distribution**: Even rotation through Matt/Nick/Kim characters

## API Endpoints

### High Scores Controller (`/api/highscores`)

#### GET `/api/highscores`
- **Purpose**: Retrieve top scores for leaderboard display
- **Query Parameters**:
  - `count` (optional): Number of scores (default: 10)
  - `gameMode` (optional): Filter by mode (default: "Default")
- **Response**: `ApiResponse<List<HighScore>>`
- **Use Cases**: Leaderboard population, score comparison

#### POST `/api/highscores`
- **Purpose**: Save new high score entry
- **Body**: `SaveHighScoreRequest`
- **Validation**: Initials format, score range, game mode enum
- **Response**: `ApiResponse<object>`
- **Side Effects**: Azure Table Storage insert, telemetry logging

#### GET `/api/highscores/check/{score}`
- **Purpose**: Determine if score qualifies as high score
- **Parameters**: 
  - `score` (path): Score value to check
  - `gameMode` (query): Target game mode
- **Response**: `ApiResponse<bool>`
- **Logic**: Compare against 10th place score in specified mode

#### GET `/api/highscores/rank/{score}`
- **Purpose**: Calculate player ranking for given score
- **Parameters**: 
  - `score` (path): Score value to rank
  - `gameMode` (query): Target game mode
- **Response**: `ApiResponse<int>`
- **Algorithm**: Count scores higher than provided value + 1

#### GET `/api/highscores/test`
- **Purpose**: Health check and diagnostics
- **Response**: `ApiResponse<string>`
- **Validation**: Service availability, dependency connectivity

### Health Controller (`/api/health`)

#### GET `/api/health`
- **Purpose**: System health monitoring
- **Response**: Health check status
- **Dependencies**: Azure Table Storage, Application Insights connectivity

### Log Controller (`/api/log`)

#### POST `/api/log/client`
- **Purpose**: Client-side telemetry ingestion
- **Body**: `ClientLogRequest`
- **Processing**: Structured logging to Application Insights
- **Use Cases**: User interaction tracking, error reporting, performance metrics

## Technical Stack

### Frontend Architecture
- **Framework**: Blazor WebAssembly (.NET 9.0)
- **Hosting**: Hosted model with integrated API backend
- **State Management**: Dependency injection with singleton services
- **Styling**: Component-scoped CSS with CSS Grid/Flexbox
- **Build Tool**: .NET CLI with hot reload support

### Backend Architecture
- **Framework**: ASP.NET Core Web API (.NET 9.0)
- **Architecture Pattern**: Vertical Slice with Service Layer
- **Design Patterns**: 
  - Repository Pattern (data access abstraction)
  - Strategy Pattern (validation strategies)
  - Service Layer Pattern (business logic separation)
- **Dependency Injection**: Built-in container with interface-based abstractions

### Data Storage
- **Primary**: Azure Table Storage (NoSQL)
- **Local Development**: Azurite emulator
- **Partitioning Strategy**: By game mode for query performance
- **Indexing**: Negative score in RowKey for descending sort

### Cloud Services
- **Application Insights**: Telemetry, logging, and performance monitoring
- **Azure Storage Account**: Table storage for high scores
- **Azure App Service**: Production hosting platform
- **Resource Group**: PoBabyTouch (centralized resource management)

### Development Tools
- **IDE**: Visual Studio Code with Blazor extensions
- **Version Control**: Git with GitHub integration
- **Local Development**: Azurite for storage emulation
- **Build Pipeline**: GitHub Actions for CI/CD

## Quality & Testing

### Test Strategy
- **Framework**: xUnit for unit and integration testing
- **Coverage Targets**: 80%+ for business logic, 60%+ overall
- **Test Types**:
  - Unit tests for services and validation logic
  - Integration tests for API endpoints with real Azure services
  - Component tests for Blazor UI interactions

### Testing Structure
```
PoBabyTouchGc.Tests/
├── Integration/
│   └── HighScoreIntegrationTests.cs    // API → Azure end-to-end
├── Unit/
│   ├── HighScoreServiceTests.cs        // Business logic validation
│   ├── ValidationServiceTests.cs       // Input validation rules
│   └── GameStateServiceTests.cs        // Client state management
└── Components/
    └── HighScoreDisplayTests.cs        // UI component behavior
```

### Quality Assurance
- **Performance**: 60fps gameplay, <100ms API response times
- **Accessibility**: WCAG 2.1 AA compliance for core interactions
- **Security**: Input sanitization, HTTPS enforcement, CORS configuration
- **Monitoring**: Application Insights dashboards for real-time metrics

### Error Handling
- **Global Exception Middleware**: Centralized error processing
- **Circuit Breaker Pattern**: Polly integration for external service calls
- **Graceful Degradation**: Offline-capable UI states
- **User-Friendly Messages**: No technical details exposed to end users

### Logging Strategy
- **Framework**: Serilog with structured logging
- **Sinks**: File system (local), Application Insights (cloud)
- **Log Levels**: Debug (development), Information (production)
- **Telemetry**: User interactions, performance counters, error tracking

---

*Document Version: 1.0*  
*Last Updated: January 2025*  
*Author: AI Development Team*