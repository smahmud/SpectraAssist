# 🧠 CortexView — Enterprise Documentation

CortexView is an enterprise-grade Windows overlay assistant built with Clean Architecture principles, providing AI-powered contextual assistance through intelligent workspace monitoring and AWS Bedrock integration.

---

## 🚀 Overview

CortexView is a native .NET 9 WPF application that implements Clean Architecture with strict layer isolation, MVVM design patterns, and comprehensive dependency injection. It monitors your active workspace, detects significant changes, and delivers intelligent AI-powered suggestions through an always-on-top panel.

**Current Version**: v0.8.0  
**Architecture**: Clean Architecture (4 layers)  
**Framework**: .NET 9, WPF  
**Testing**: 67 tests, >70% coverage  
**Code Quality**: Enterprise-grade with SOLID principles

For installation and setup, see [installation-guide.md](installation-guide.md).  
For system architecture, see [architecture.md](architecture.md).  
For project structure, see [project_structure.md](project_structure.md).  
For testing strategy, see [test_strategy.md](test_strategy.md).

---

## 📦 Key Features

### Core Capabilities
- **AI-Powered Analysis**: AWS Bedrock (Claude 3) integration for intelligent, context-aware suggestions
- **Smart Change Detection**: Pixel-level comparison with SHA256 hashing to minimize redundant AI calls
- **Window Monitoring**: Automatic periodic capture with configurable intervals (1-60 seconds)
- **Manual Capture**: On-demand screenshot analysis with force-analysis option
- **Persona System**: Customizable AI personalities with adjustable parameters

### Architecture Excellence
- **Clean Architecture**: Domain-driven design with strict layer isolation
- **MVVM Pattern**: Full separation of UI and business logic with data binding
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection with IOptions pattern
- **Immutable Entities**: Domain entities use `init` properties for thread safety
- **Async-First Design**: All I/O operations are asynchronous with cancellation support

### Quality Assurance
- **Comprehensive Testing**: 67 automated tests across all layers
- **Property-Based Testing**: FsCheck integration for universal property verification
- **Code Coverage**: >70% overall (Domain: 100%, Application: >75%)
- **Integration Tests**: AWS and Win32 integration with graceful skipping
- **Minimal Code-Behind**: MainWindow.xaml.cs reduced to 23 lines

### Privacy & Control
- **User-Controlled Capture**: Manual triggers and configurable sensitivity
- **Storage Management**: Optional screenshot storage with retention policies
- **Audit Logging**: Complete audit trail of all AI interactions
- **Privacy-First**: No data sent to AI without user consent

---

## 🏗️ Architecture Overview

CortexView implements Clean Architecture with four distinct layers:

### 1. Domain Layer (`CortexView.Domain`)
**Purpose**: Pure business logic with zero external dependencies

**Components**:
- **Entities**: `Persona`, `AnalysisRequest`, `AnalysisResponse`, `AuditEntry`
- **Interfaces**: `IAiAnalysisService`, `IScreenCaptureService`, `IStorageService`, `IChangeDetectionService`
- **Value Objects**: Immutable configuration objects

**Characteristics**:
- Zero external dependencies (pure .NET)
- Immutable entities with `init` properties
- Domain validation logic (`IsValid()` methods)
- Framework-agnostic design

### 2. Application Layer (`CortexView.Application`)
**Purpose**: Business logic orchestration and workflow coordination

**Components**:
- **AnalysisOrchestrator**: Orchestrates capture → detect → analyze → store workflow
- **ChangeDetectionService**: Pixel comparison and SHA256 hashing
- **WindowMonitoringService**: Periodic capture timer management

**Characteristics**:
- References Domain layer only
- Implements domain interfaces
- Coordinates multiple services
- No external dependencies

### 3. Infrastructure Layer (`CortexView.Infrastructure`)
**Purpose**: External integrations and platform-specific implementations

**Components**:
- **AI Services**: `AwsBedrockService`, `MockAiService`
- **Capture Services**: `Win32ScreenCaptureService`
- **Storage Services**: `LocalStorageService`, `AuditService`
- **Configuration**: `AwsConfig`, `StorageConfig`

**Characteristics**:
- References Domain layer only
- Implements domain interfaces
- Contains all external dependencies (AWS SDK, Win32 APIs)
- Platform-specific code isolated here

### 4. Presentation Layer (`CortexView`)
**Purpose**: WPF UI with MVVM pattern

**Components**:
- **ViewModels**: `MainViewModel`, `ViewModelBase`
- **Commands**: `RelayCommand` implementation
- **Views**: `MainWindow.xaml` with data binding
- **Services**: `PromptService` for persona loading

**Characteristics**:
- References all layers (orchestration point)
- MVVM pattern with INotifyPropertyChanged
- Dependency Injection configuration
- Minimal code-behind (23 lines)

---

## 📊 System Workflow

### Capture and Analysis Flow

```
1. User Action (Manual) or Timer (Automatic)
   ↓
2. MainViewModel.CaptureNow() or MonitoringService.CaptureRequested
   ↓
3. AnalysisOrchestrator.CaptureAndAnalyzeAsync()
   ├─→ IScreenCaptureService.CaptureWindowAsync()
   ├─→ IChangeDetectionService.ComputeChangedFraction()
   ├─→ IChangeDetectionService.IsSignificantChange()
   ├─→ IAiAnalysisService.AnalyzeImageAsync()
   └─→ IStorageService.SaveScreenshotAsync()
   ↓
4. AnalysisResponse returned to MainViewModel
   ↓
5. UI updated via data binding (StatusText, SuggestionText)
```

### Change Detection Algorithm

```
1. Capture current screenshot as byte[]
   ↓
2. Compute SHA256 hash of image data
   ↓
3. Compare hash with previous capture
   ├─→ If identical: Return 0% change (skip analysis)
   └─→ If different: Proceed to pixel comparison
   ↓
4. Pixel-by-pixel comparison
   ├─→ Convert byte[] to Bitmap
   ├─→ Compare each pixel (Color equality)
   └─→ Calculate changed fraction (0.0 to 1.0)
   ↓
5. Compare against sensitivity threshold
   ├─→ If below threshold: Skip analysis
   └─→ If above threshold: Trigger AI analysis
```

---

## 🧪 Testing Strategy

### Test Coverage by Layer

| Layer | Tests | Coverage | Test Types |
|-------|-------|----------|------------|
| **Domain** | 18 | 100% | Unit tests, validation tests |
| **Application** | 21 | >75% | Unit tests, property-based tests, orchestration tests |
| **Infrastructure** | 8 | >60% | Unit tests, integration tests (3 skipped) |
| **Presentation** | 20 | >70% | ViewModel tests, command tests, property tests |
| **Total** | 67 | >70% | Mixed (54 unit, 4 property-based, 9 integration) |

### Testing Frameworks
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependency injection
- **FsCheck**: Property-based testing (100 iterations per property)

### Test Execution
```bash
# Run all tests
dotnet test SpectraAssist.sln

# Run specific test project
dotnet test CortexView.Domain.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

For detailed testing information, see [test_strategy.md](test_strategy.md).

---

## 🔧 Configuration

### Application Settings (`appsettings.json`)

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

### Persona Configuration (`Prompts/*.md`)

Personas are defined as Markdown files in the `Prompts/` directory:

```markdown
---
name: Code Reviewer
temperature: 0.3
top_p: 0.9
max_tokens: 2048
---

You are an expert code reviewer. Analyze the screenshot for:
- Code quality issues
- Potential bugs
- Performance concerns
- Best practice violations

Provide concise, actionable feedback.
```

---

## 📈 Performance Metrics

### Build Performance
- **Full Build**: 5.2s (Release configuration)
- **Incremental Build**: <2s
- **Test Execution**: 5.1s (67 tests)

### Runtime Performance
- **DI Resolution**: <10ms (estimated)
- **Capture Time**: 50-100ms (depends on window size)
- **Change Detection**: <50ms (hash comparison)
- **UI Refresh**: <16ms (60 FPS maintained)

### Resource Usage
- **Memory**: ~50-100MB (idle)
- **CPU**: <5% (monitoring mode)
- **Disk**: Configurable (screenshot storage)

---

## 🔒 Security & Privacy

### Data Handling
- **Screenshots**: Captured in-memory, optionally saved to disk
- **AI Requests**: Sent to AWS Bedrock only when analysis is triggered
- **Audit Logs**: Local storage only, never transmitted
- **Credentials**: Stored in appsettings.json (user-controlled)

### Privacy Controls
- **Manual Capture**: User explicitly triggers analysis
- **Sensitivity Threshold**: User controls when automatic analysis occurs
- **Storage Toggle**: User can disable screenshot storage
- **Retention Policy**: Automatic cleanup of old screenshots

### Security Best Practices
- **No Hardcoded Credentials**: All credentials from configuration
- **Async Cancellation**: All operations support cancellation tokens
- **Error Handling**: Graceful degradation on failures
- **Input Validation**: All domain entities validate input

---

## 🚀 Deployment

### Prerequisites
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime
- AWS credentials (for AI features)

### Installation Methods

**Method 1: Binary Release**
1. Download latest release from GitHub
2. Extract to desired location
3. Configure `appsettings.json`
4. Run `CortexView.exe`

**Method 2: Build from Source**
```bash
git clone https://github.com/yourusername/SpectraAssist.git
cd SpectraAssist
dotnet build --configuration Release
dotnet run --project CortexView --configuration Release
```

For detailed installation instructions, see [installation-guide.md](installation-guide.md).

---

## 📚 Additional Documentation

### User Documentation
- **[Installation Guide](installation-guide.md)** - Setup and configuration

### Developer Documentation
- **[Architecture](architecture.md)** - System design and technical details
- **[Project Structure](project_structure.md)** - Codebase organization
- **[Test Strategy](test_strategy.md)** - Testing approach and coverage

### Release Documentation
- **[Release Notes v0.8.0](releases/v0.8.0.md)** - v0.8.0 changes
- **[Changelog](../CHANGELOG.md)** - Version history

---

## 🔮 Roadmap

### v0.9.0 (Planned)
- Automated UI testing with WPF UI Automation
- Performance benchmarks and monitoring
- Enhanced error handling and logging
- Configuration UI improvements

### v1.0.0 (Future)
- Plugin architecture for custom AI providers
- Multi-monitor support
- Cloud storage integration
- Real-time collaboration features

---

## 📄 License

This project is licensed under  
**Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)**

You may:
- Share and adapt the material with attribution
- Not use it for commercial purposes
- Not use it for training machine learning models (including LLMs) without explicit permission

See [LICENSE.md](../LICENSE.md) for full legal terms.  
Full license text: [https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)
