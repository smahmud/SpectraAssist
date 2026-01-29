# 🧱 Project Structure

This document outlines the folder and file layout of CortexView. It reflects Clean Architecture principles, MVVM design patterns, and enterprise-grade organization.

---

## 📂 Solution Structure

```
SpectraAssist.sln
├── CortexView.Domain/              # Domain Layer (Pure business logic)
├── CortexView.Application/         # Application Layer (Orchestration)
├── CortexView.Infrastructure/      # Infrastructure Layer (External dependencies)
├── CortexView/                     # Presentation Layer (WPF, MVVM)
├── CortexView.Domain.Tests/        # Domain layer tests
├── CortexView.Application.Tests/   # Application layer tests
├── CortexView.Infrastructure.Tests/# Infrastructure layer tests
├── CortexView.Presentation.Tests/  # Presentation layer tests
└── CortexView.Tests/               # Legacy tests (deprecated)
```

---

## 🏗️ Layer 1: Domain Layer

**Project**: `CortexView.Domain`  
**Purpose**: Pure business logic with zero external dependencies

```
CortexView.Domain/
├── Entities/
│   ├── Persona.cs              # AI persona with behavioral parameters
│   ├── AnalysisRequest.cs      # Screenshot analysis request
│   ├── AnalysisResponse.cs     # Analysis result with factory methods
│   └── AuditEntry.cs           # Audit log entry
├── Interfaces/
│   ├── IAiAnalysisService.cs   # AI service contract
│   ├── IScreenCaptureService.cs# Screen capture contract
│   ├── IStorageService.cs      # Storage service contract
│   └── IChangeDetectionService.cs # Change detection contract
├── ValueObjects/
│   └── (Future: Domain value objects)
├── README.md                   # Domain layer documentation
└── CortexView.Domain.csproj    # Project file (zero dependencies)
```

**Key Characteristics**:
- Zero external dependencies (pure .NET 9)
- Immutable entities with `init` properties
- Domain validation logic
- Framework-agnostic design

---

## 🏗️ Layer 2: Application Layer

**Project**: `CortexView.Application`  
**Purpose**: Business logic orchestration and workflow coordination

```
CortexView.Application/
├── Services/
│   ├── AnalysisOrchestrator.cs     # Orchestrates capture → analyze → store
│   ├── ChangeDetectionService.cs   # Pixel comparison and SHA256 hashing
│   └── WindowMonitoringService.cs  # Periodic capture timer management
└── CortexView.Application.csproj   # References Domain only
```

**Key Characteristics**:
- References Domain layer only
- Implements domain interfaces
- Coordinates multiple services
- No external dependencies (except System.Drawing.Common)

---

## 🏗️ Layer 3: Infrastructure Layer

**Project**: `CortexView.Infrastructure`  
**Purpose**: External integrations and platform-specific implementations

```
CortexView.Infrastructure/
├── AI/
│   ├── AwsBedrockService.cs        # AWS Bedrock integration
│   └── MockAiService.cs            # Mock AI service for testing
├── Capture/
│   └── Win32ScreenCaptureService.cs# Windows screen capture (Win32 APIs)
├── Storage/
│   ├── LocalStorageService.cs      # File storage implementation
│   └── AuditService.cs             # Audit logging implementation
├── Configuration/
│   ├── AwsConfig.cs                # AWS configuration model
│   └── StorageConfig.cs            # Storage configuration model
└── CortexView.Infrastructure.csproj# References Domain only
```

**Key Characteristics**:
- References Domain layer only
- Implements domain interfaces
- Contains all external dependencies (AWS SDK, Win32 APIs)
- Platform-specific code isolated here

**External Dependencies**:
- `AWSSDK.BedrockRuntime` v4.0.15
- `Microsoft.Extensions.Options` v10.0.2
- `System.Drawing.Common` v10.0.2

---

## 🏗️ Layer 4: Presentation Layer

**Project**: `CortexView`  
**Purpose**: WPF UI with MVVM pattern

```
CortexView/
├── ViewModels/
│   ├── MainViewModel.cs            # Main window view model
│   ├── ViewModelBase.cs            # Base class with INotifyPropertyChanged
│   └── RelayCommand.cs             # ICommand implementation
├── Services/
│   └── PromptService.cs            # Persona/prompt file loader
├── Models/
│   └── (Legacy models, deprecated)
├── Prompts/
│   ├── code-reviewer.md            # Code reviewer persona
│   ├── general-assistant.md        # General assistant persona
│   └── (Additional personas)
├── MainWindow.xaml                 # Main window XAML
├── MainWindow.xaml.cs              # Main window code-behind (23 lines)
├── App.xaml                        # Application XAML
├── App.xaml.cs                     # DI configuration and startup
├── appsettings.json                # Application configuration
└── CortexView.csproj               # References all layers
```

**Key Characteristics**:
- References all layers (orchestration point)
- MVVM pattern with INotifyPropertyChanged
- Dependency Injection configuration
- Minimal code-behind (23 lines in MainWindow.xaml.cs)

**External Dependencies**:
- `Microsoft.Extensions.DependencyInjection` v10.0.0
- `Microsoft.Extensions.Configuration.Json` v10.0.0
- `Markdig.Wpf` v0.5.0.1 (Markdown rendering)
- `Ookii.Dialogs.Wpf` v5.0.1 (Folder browser)

---

## 🧪 Test Projects

### Domain Tests

**Project**: `CortexView.Domain.Tests`  
**Coverage**: 100%

```
CortexView.Domain.Tests/
├── Entities/
│   ├── PersonaTests.cs             # 7 tests (validation logic)
│   ├── AnalysisRequestTests.cs     # 7 tests (request validation)
│   └── AnalysisResponseTests.cs    # 4 tests (factory methods)
└── CortexView.Domain.Tests.csproj  # xUnit, no mocking needed
```

**Test Count**: 18 tests (all passing)

### Application Tests

**Project**: `CortexView.Application.Tests`  
**Coverage**: >75%

```
CortexView.Application.Tests/
├── Services/
│   ├── ChangeDetectionServiceTests.cs      # 9 unit tests
│   ├── ChangeDetectionPropertyTests.cs     # 4 property-based tests (FsCheck)
│   └── AnalysisOrchestratorTests.cs        # 8 orchestration tests
└── CortexView.Application.Tests.csproj     # xUnit, Moq, FsCheck
```

**Test Count**: 21 tests (all passing)

### Infrastructure Tests

**Project**: `CortexView.Infrastructure.Tests`  
**Coverage**: >60%

```
CortexView.Infrastructure.Tests/
├── AI/
│   └── AwsBedrockServiceTests.cs           # 5 tests (2 passing, 3 skipped)
├── Capture/
│   └── Win32ScreenCaptureServiceTests.cs   # 6 tests (3 passing, 3 skipped)
└── CortexView.Infrastructure.Tests.csproj  # xUnit
```

**Test Count**: 8 tests (5 passing, 3 skipped for AWS costs)

### Presentation Tests

**Project**: `CortexView.Presentation.Tests`  
**Coverage**: >70%

```
CortexView.Presentation.Tests/
├── ViewModels/
│   └── MainViewModelTests.cs               # 20 tests (commands, properties)
└── CortexView.Presentation.Tests.csproj    # xUnit, Moq
```

**Test Count**: 20 tests (all passing)

---

## 📦 Root-Level Files

| File | Purpose |
|------|---------|
| `README.md` | Project overview and quick start |
| `LICENSE.md` | CC BY-NC-SA 4.0 license terms |
| `NOTICE.md` | Attribution and notices |
| `SpectraAssist.sln` | Visual Studio solution file |
| `SpectraAssist.code-workspace` | VS Code workspace |
| `.gitignore` | Git ignore patterns |
| `coverage.xml` | Code coverage data |

---

## 📘 Documentation Suite

```
docs/
├── README.md                       # Documentation hub
├── architecture.md                 # System architecture
├── project_structure.md            # This file
├── installation-guide.md           # Setup and installation
├── test_strategy.md                # Testing approach
├── Milestone-*.md                  # Milestone status documents
├── releases/                       # Release notes by version
│   └── v0.8.0.md                   # v0.8.0 release notes
├── notes/                          # Development notes (excluded from git)
│   ├── milestone-8-*.md            # Milestone 8 progress notes
│   └── v0.8.0-release-complete.md  # Release completion
├── protocols/                      # Development protocols (excluded from git)
│   └── documentation_update_protocol.md
└── (Work in Progress - not versioned)
    ├── CONTRIBUTING.md             # Contribution guidelines
    ├── development-setup.md        # Developer environment
    ├── user-guide.md               # User documentation
    ├── api-reference.md            # Public API documentation
    ├── persona-guide.md            # Creating personas
    ├── troubleshooting.md          # Common issues
    └── migration-guide.md          # Upgrading from v0.7.5
```
└── sample-documentation/           # Documentation templates
    ├── README.md
    ├── architecture.md
    ├── project_structure.md
    ├── installation-guide.md
    ├── cli-commands.md
    ├── test_strategy.md
    ├── metadata_schema.md
    └── transcript_schema.md
```

---

## 🔧 Configuration Files

### Application Configuration

**File**: `CortexView/appsettings.json`

```json
{
  "AiServiceConfig": {
    "Region": "us-east-1",
    "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0"
  },
  "StorageConfig": {
    "Enabled": true,
    "StoragePath": "./screenshots",
    "RetentionDays": 7
  }
}
```

### Persona Configuration

**Location**: `CortexView/Prompts/*.md`

Personas are Markdown files with YAML frontmatter:

```markdown
---
name: Code Reviewer
temperature: 0.3
top_p: 0.9
max_tokens: 2048
---

System prompt content here...
```

---

## 📊 Project Statistics

### Code Metrics
- **Total Projects**: 8 (4 main + 4 test)
- **Total Tests**: 67 passing, 3 skipped
- **Code Coverage**: >70% average
- **Lines of Code**: ~5,000 (estimated)

### Layer Distribution
- **Domain**: ~500 lines (entities + interfaces)
- **Application**: ~800 lines (orchestration + services)
- **Infrastructure**: ~1,200 lines (external integrations)
- **Presentation**: ~1,500 lines (ViewModels + UI)
- **Tests**: ~2,000 lines (comprehensive coverage)

### Quality Metrics
- **MainWindow.xaml.cs**: 23 lines (target: <100)
- **Cyclomatic Complexity**: Low (SOLID principles)
- **Coupling**: Low (dependency inversion)
- **Cohesion**: High (single responsibility)

---

## 🔮 Future Structure

### Planned Additions (v0.9.0+)
- `CortexView.Plugins/` - Plugin architecture
- `CortexView.Api/` - REST API layer
- `CortexView.Cli/` - Command-line interface
- `CortexView.Benchmarks/` - Performance benchmarks

### Planned Enhancements
- Multi-monitor support in Infrastructure layer
- Cloud storage adapters in Infrastructure layer
- Additional AI providers in Infrastructure layer
- Plugin system in Application layer

---

For detailed architecture information, see [architecture.md](architecture.md).  
For testing strategy, see [test_strategy.md](test_strategy.md).  
For installation instructions, see [installation-guide.md](installation-guide.md).
