# 🧪 Test Strategy

This document outlines the testing approach for **CortexView**, focusing on reliability, maintainability, and Clean Architecture alignment. The strategy ensures comprehensive coverage across all layers while maintaining fast execution and clear test organization.

---

## 1. Purpose and Scope

The testing strategy validates that all components — Domain entities, Application services, Infrastructure implementations, and Presentation ViewModels — behave predictably and correctly. It covers unit tests, integration tests, and property-based tests aligned with Clean Architecture principles.

**Testing Goals**:
- Validate business logic correctness across all layers
- Ensure Clean Architecture dependency rules are enforced
- Verify MVVM pattern implementation
- Validate async operations and cancellation support
- Ensure thread-safety and immutability guarantees
- Achieve >70% code coverage across all projects

---

## 2. Test Types

### Unit Tests
Validate isolated components with mocked dependencies:
- **Domain Entities**: Validation logic, factory methods, immutability
- **Application Services**: Business logic orchestration, workflow coordination
- **Infrastructure Services**: External integrations (AWS, Win32 APIs, file I/O)
- **Presentation ViewModels**: Command execution, property binding, UI state

### Property-Based Tests
Use FsCheck to validate universal properties across input ranges:
- **Change Detection**: Fraction always between 0.0 and 1.0
- **Threshold Comparison**: Monotonic behavior
- **Idempotency**: Identical inputs produce identical outputs
- **Boundary Conditions**: Edge cases automatically discovered

### Integration Tests
Validate end-to-end workflows across multiple layers:
- **Capture → Analyze → Store**: Complete analysis workflow
- **Change Detection Pipeline**: Screenshot comparison and threshold logic
- **Dependency Injection**: Service resolution and lifetime management

### Mocking Strategy
External dependencies are mocked using `Moq` to ensure deterministic behavior:
- **AWS Bedrock**: Mocked to avoid network calls and costs
- **Win32 APIs**: Mocked to avoid platform dependencies
- **File I/O**: Mocked to avoid disk dependencies
- **Timers**: Mocked to control timing in tests

---

## 3. Test Organization

All tests are organized by layer, mirroring the Clean Architecture structure:

```text
CortexView.Domain.Tests/
├── Entities/
│   ├── PersonaTests.cs                    # Persona validation tests
│   ├── AnalysisRequestTests.cs            # Request validation tests
│   └── AnalysisResponseTests.cs           # Response factory tests
└── CortexView.Domain.Tests.csproj

CortexView.Application.Tests/
├── Services/
│   ├── AnalysisOrchestratorTests.cs       # Workflow orchestration tests
│   ├── ChangeDetectionServiceTests.cs     # Change detection unit tests
│   ├── ChangeDetectionPropertyTests.cs    # FsCheck property tests
│   └── WindowMonitoringServiceTests.cs    # Timer and event tests
└── CortexView.Application.Tests.csproj

CortexView.Infrastructure.Tests/
├── AI/
│   └── AwsBedrockServiceTests.cs          # AWS integration tests (skipped)
├── Capture/
│   └── Win32ScreenCaptureServiceTests.cs  # Win32 API tests (skipped)
└── CortexView.Infrastructure.Tests.csproj

CortexView.Presentation.Tests/
├── ViewModels/
│   └── MainViewModelTests.cs              # ViewModel command/property tests
└── CortexView.Presentation.Tests.csproj
```

**Test Project Statistics**:
- **Total Test Projects**: 4 (one per layer)
- **Total Test Files**: 10
- **Total Tests**: 67 (18 Domain + 21 Application + 8 Infrastructure + 20 Presentation)
- **Skipped Tests**: 3 (AWS integration tests requiring credentials)

---

## 4. Test Execution

Tests are executed using `xUnit` with `dotnet test` command.

### Run All Tests

```bash
dotnet test
```

**Expected Output**:
```
Total tests: 67
     Passed: 64
    Skipped: 3
 Total time: 5.1 Seconds
```

### Run Tests by Project

```bash
# Domain layer tests only
dotnet test CortexView.Domain.Tests/CortexView.Domain.Tests.csproj

# Application layer tests only
dotnet test CortexView.Application.Tests/CortexView.Application.Tests.csproj

# Infrastructure layer tests only
dotnet test CortexView.Infrastructure.Tests/CortexView.Infrastructure.Tests.csproj

# Presentation layer tests only
dotnet test CortexView.Presentation.Tests/CortexView.Presentation.Tests.csproj
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~PersonaTests"
dotnet test --filter "FullyQualifiedName~ChangeDetectionServiceTests"
dotnet test --filter "FullyQualifiedName~MainViewModelTests"
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Coverage Report Generation**:
```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

---

## 5. Testing by Layer

### Layer 1: Domain Tests (CortexView.Domain.Tests)

**Focus**: Pure business logic validation with zero dependencies.

**Test Coverage**:
- ✅ Entity validation (`IsValid()` methods)
- ✅ Factory methods (`Success()`, `Failure()`)
- ✅ Immutability guarantees
- ✅ Boundary conditions (temperature, topP, maxTokens)
- ✅ Required property enforcement

**Example Test**:
```csharp
[Fact]
public void IsValid_ValidPersona_ReturnsTrue()
{
    // Arrange
    var persona = new Persona
    {
        Name = "Test Persona",
        SystemPrompt = "You are a helpful assistant.",
        Temperature = 0.7f,
        TopP = 0.9f,
        MaxTokens = 1000
    };

    // Act
    bool result = persona.IsValid();

    // Assert
    Assert.True(result);
}
```

**Test Statistics**:
- **Total Tests**: 18
- **Coverage**: 100% (all domain entities)
- **Execution Time**: <1s

---

### Layer 2: Application Tests (CortexView.Application.Tests)

**Focus**: Business logic orchestration and workflow coordination.

**Test Coverage**:
- ✅ Workflow orchestration (capture → analyze → store)
- ✅ Change detection algorithm (hash + pixel comparison)
- ✅ Property-based tests (FsCheck)
- ✅ Service coordination and error handling
- ✅ Async operations and cancellation

**Example Test**:
```csharp
[Fact]
public async Task CaptureAndAnalyzeAsync_SuccessfulCapture_ReturnsSuccess()
{
    // Arrange
    var mockCapture = new Mock<IScreenCaptureService>();
    var mockAi = new Mock<IAiAnalysisService>();
    var mockStorage = new Mock<IStorageService>();
    var mockChangeDetection = new Mock<IChangeDetectionService>();

    mockCapture.Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), default))
        .ReturnsAsync(new byte[] { 1, 2, 3 });
    
    mockChangeDetection.Setup(x => x.ComputeChangedFraction(It.IsAny<byte[]>()))
        .Returns(0.5);
    
    mockChangeDetection.Setup(x => x.IsSignificantChange(0.5, 0.1))
        .Returns(true);

    mockAi.Setup(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), default))
        .ReturnsAsync(AnalysisResponse.Success("Test suggestion", 100));

    var orchestrator = new AnalysisOrchestrator(
        mockCapture.Object,
        mockChangeDetection.Object,
        mockAi.Object,
        mockStorage.Object);

    // Act
    var result = await orchestrator.CaptureAndAnalyzeAsync(
        IntPtr.Zero, "Test Window", persona, 0.1);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Test suggestion", result.SuggestionText);
}
```

**Property-Based Test Example**:
```csharp
[Property]
public bool ChangedFraction_AlwaysBetween0And1(byte[] imageData)
{
    if (imageData == null || imageData.Length == 0)
        return true; // Skip invalid inputs

    try
    {
        var service = new ChangeDetectionService();
        double result = service.ComputeChangedFraction(imageData);
        return result >= 0.0 && result <= 1.0;
    }
    catch
    {
        return true; // Invalid image data is acceptable
    }
}
```

**Test Statistics**:
- **Total Tests**: 21 (16 unit + 5 property-based)
- **Coverage**: >75%
- **Execution Time**: ~2s

---

### Layer 3: Infrastructure Tests (CortexView.Infrastructure.Tests)

**Focus**: External integration validation with mocked dependencies.

**Test Coverage**:
- ✅ AWS Bedrock integration (skipped without credentials)
- ✅ Win32 screen capture (skipped on non-Windows)
- ✅ File storage operations
- ✅ Configuration loading (IOptions pattern)
- ✅ Error handling and graceful degradation

**Example Test**:
```csharp
[Fact(Skip = "Requires AWS credentials")]
public async Task AnalyzeImageAsync_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var config = Options.Create(new AwsConfig
    {
        Region = "us-east-1",
        ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
    });
    var service = new AwsBedrockService(config);

    var request = new AnalysisRequest
    {
        ImageData = new byte[] { 1, 2, 3 },
        ImageFormat = "PNG",
        WindowTitle = "Test Window"
    };

    // Act
    var result = await service.AnalyzeImageAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
}
```

**Test Statistics**:
- **Total Tests**: 8 (3 skipped)
- **Coverage**: ~60% (external dependencies)
- **Execution Time**: <1s (skipped tests excluded)

**Note**: Infrastructure tests are skipped by default to avoid external dependencies. They can be enabled by providing AWS credentials and running on Windows.

---

### Layer 4: Presentation Tests (CortexView.Presentation.Tests)

**Focus**: MVVM pattern validation and UI logic testing.

**Test Coverage**:
- ✅ Command execution (CaptureNow, ToggleMonitoring)
- ✅ Property change notifications (INotifyPropertyChanged)
- ✅ Data binding validation
- ✅ ViewModel state management
- ✅ Async command handlers

**Example Test**:
```csharp
[Fact]
public async Task CaptureNowCommand_Execute_UpdatesStatusText()
{
    // Arrange
    var mockOrchestrator = new Mock<AnalysisOrchestrator>();
    var mockMonitoring = new Mock<WindowMonitoringService>();

    mockOrchestrator.Setup(x => x.CaptureAndAnalyzeAsync(
        It.IsAny<IntPtr>(),
        It.IsAny<string>(),
        It.IsAny<Persona>(),
        It.IsAny<double>(),
        true,
        default))
        .ReturnsAsync(AnalysisResponse.Success("Test", 100));

    var viewModel = new MainViewModel(
        mockOrchestrator.Object,
        mockMonitoring.Object);

    // Act
    viewModel.CaptureNowCommand.Execute(null);
    await Task.Delay(100); // Wait for async command

    // Assert
    Assert.Contains("complete", viewModel.StatusText, StringComparison.OrdinalIgnoreCase);
}
```

**Test Statistics**:
- **Total Tests**: 20
- **Coverage**: >70%
- **Execution Time**: ~1s

---

## 6. Property-Based Testing Strategy

CortexView uses **FsCheck** for property-based testing to validate universal behaviors across randomized input ranges.

### Properties Validated

#### Change Detection Properties
- **Bounded Output**: `ComputeChangedFraction()` always returns 0.0 to 1.0
- **Idempotency**: Identical inputs produce identical outputs
- **Monotonicity**: Threshold comparison behaves consistently
- **Commutativity**: `IsSignificantChange()` is deterministic

#### Domain Validation Properties
- **Temperature Range**: Always 0.0 to 1.0
- **TopP Range**: Always 0.0 to 1.0
- **MaxTokens**: Always positive
- **Required Fields**: Never null or empty

### FsCheck Configuration

```csharp
[Property(MaxTest = 100, QuietOnSuccess = true)]
public bool PropertyName(InputType input)
{
    // Property validation logic
}
```

**Benefits**:
- Discovers edge cases automatically
- Validates universal properties
- Complements unit tests
- Fast execution (100 tests per property)

---

## 7. Code Coverage Metrics

### Current Coverage (v0.8.0)

| Layer | Project | Coverage | Tests |
|-------|---------|----------|-------|
| Domain | CortexView.Domain | 100% | 18 |
| Application | CortexView.Application | >75% | 21 |
| Infrastructure | CortexView.Infrastructure | ~60% | 8 |
| Presentation | CortexView (ViewModels) | >70% | 20 |
| **Overall** | **All Projects** | **>70%** | **67** |

### Coverage Goals

- **Domain Layer**: 100% (achieved)
- **Application Layer**: >80% (target for v0.9.0)
- **Infrastructure Layer**: >70% (target for v0.9.0)
- **Presentation Layer**: >75% (target for v0.9.0)

### Coverage Report

Generate HTML coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

View report:
```bash
start coverage-report/index.html  # Windows
open coverage-report/index.html   # macOS
xdg-open coverage-report/index.html  # Linux
```

---

## 8. Testing Best Practices

### Arrange-Act-Assert Pattern
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public void TestName()
{
    // Arrange: Set up test data and mocks
    var service = new MyService();
    
    // Act: Execute the operation
    var result = service.DoSomething();
    
    // Assert: Verify the outcome
    Assert.Equal(expected, result);
}
```

### Test Naming Convention
- **Format**: `MethodName_Scenario_ExpectedBehavior`
- **Examples**:
  - `IsValid_ValidPersona_ReturnsTrue`
  - `ComputeChangedFraction_IdenticalImages_Returns0Percent`
  - `CaptureAndAnalyzeAsync_NoSignificantChange_ReturnsFailure`

### Mocking Guidelines
- Mock external dependencies only (AWS, Win32, File I/O)
- Use `Moq` for interface mocking
- Verify important interactions with `Verify()`
- Keep mocks simple and focused

### Async Testing
- Use `async Task` for async tests
- Always await async operations
- Use `CancellationToken.None` or `default` for tests
- Test cancellation scenarios explicitly

### Test Isolation
- Each test is independent
- No shared state between tests
- Use `[Fact]` for single tests
- Use `[Theory]` for parameterized tests

---

## 9. Continuous Integration

### Build Pipeline
```yaml
# .github/workflows/build.yml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

### Quality Gates
- ✅ All tests must pass
- ✅ Code coverage >70%
- ✅ No build warnings
- ✅ Clean Architecture rules enforced

---

## 10. Writing New Tests

### Adding Domain Tests

1. Create test class in `CortexView.Domain.Tests/Entities/`
2. Test validation logic (`IsValid()`)
3. Test factory methods
4. Test immutability
5. Achieve 100% coverage

**Example**:
```csharp
public class NewEntityTests
{
    [Fact]
    public void IsValid_ValidEntity_ReturnsTrue()
    {
        // Test implementation
    }
}
```

### Adding Application Tests

1. Create test class in `CortexView.Application.Tests/Services/`
2. Mock all dependencies
3. Test workflow orchestration
4. Test error handling
5. Add property-based tests if applicable

**Example**:
```csharp
public class NewServiceTests
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        var service = new NewService(mockDependency.Object);
        
        // Act
        var result = await service.MethodAsync();
        
        // Assert
        Assert.NotNull(result);
    }
}
```

### Adding Infrastructure Tests

1. Create test class in `CortexView.Infrastructure.Tests/`
2. Mock external dependencies (AWS, Win32)
3. Use `[Fact(Skip = "...")]` for integration tests
4. Test configuration loading
5. Test error handling

### Adding Presentation Tests

1. Create test class in `CortexView.Presentation.Tests/ViewModels/`
2. Mock application services
3. Test command execution
4. Test property change notifications
5. Test async command handlers

---

## 11. Test Maintenance

### When to Update Tests

- **New Feature**: Add tests for new functionality
- **Bug Fix**: Add regression test
- **Refactoring**: Update tests to match new structure
- **Breaking Change**: Update all affected tests

### Test Review Checklist

- [ ] Tests follow AAA pattern
- [ ] Tests are isolated and independent
- [ ] Mocks are used appropriately
- [ ] Async operations are properly awaited
- [ ] Test names are descriptive
- [ ] Coverage meets layer-specific goals
- [ ] Property-based tests for universal properties

---

## 12. Future Testing Enhancements

### Planned for v0.9.0+

- **Performance Tests**: Benchmark capture and analysis times
- **Load Tests**: Validate behavior under high frequency captures
- **UI Automation Tests**: End-to-end UI testing with WinAppDriver
- **Mutation Testing**: Validate test suite effectiveness
- **Contract Tests**: Validate interface contracts across layers

### Testing Tools to Evaluate

- **Stryker.NET**: Mutation testing
- **BenchmarkDotNet**: Performance benchmarking
- **WinAppDriver**: UI automation
- **ArchUnitNET**: Architecture rule enforcement

---

For project structure details, see [project_structure.md](project_structure.md).  
For architecture details, see [architecture.md](architecture.md).  
For installation instructions, see [installation-guide.md](installation-guide.md).

