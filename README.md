# 🧠 SpectraAssist — CortexView

An enterprise-grade Windows overlay assistant built with Clean Architecture principles. CortexView provides AI-powered contextual assistance by monitoring your active workspace and delivering intelligent suggestions through an always-on-top panel.

---

## 🚀 Overview

CortexView is a native .NET 9 WPF application that sits alongside your existing applications, intelligently observes your workflow, and surfaces contextual AI-powered suggestions. Built with enterprise-grade architecture, comprehensive testing, and MVVM design patterns for maximum maintainability and scalability.

For installation and setup, see [docs/installation-guide.md](docs/installation-guide.md).  
For system architecture, see [docs/architecture.md](docs/architecture.md).  
For project structure, see [docs/project_structure.md](docs/project_structure.md).  
For testing strategy, see [docs/test_strategy.md](docs/test_strategy.md).

---

## 📦 Key Features

- **AI-Powered Analysis**: AWS Bedrock integration for intelligent, context-aware suggestions
- **Smart Change Detection**: Pixel-level comparison with SHA256 hashing to minimize redundant AI calls
- **Clean Architecture**: Domain-driven design with strict layer isolation and dependency inversion
- **MVVM Pattern**: Full separation of concerns with data binding and command patterns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection with IOptions configuration
- **Comprehensive Testing**: 67 automated tests with >70% code coverage and property-based testing
- **Privacy-First**: User-controlled screenshot capture, storage, and retention policies
- **Persona System**: Customizable AI personalities with adjustable parameters (temperature, top-p, max tokens)

---

## 📈 Version Status

### ✅ Current Release: v0.8.0
- Clean Architecture with 4 layers (Domain, Application, Infrastructure, Presentation)
- MVVM pattern with full data binding
- Dependency Injection with IOptions pattern
- 67 automated tests with >70% coverage
- MainWindow.xaml.cs reduced to 23 lines (77% reduction from monolithic design)
- Property-based testing with FsCheck
- Enterprise-grade code quality and documentation

### 🧭 Upcoming
- 🤖 Automated UI testing with WPF UI Automation
- 📊 Performance benchmarks and monitoring
- 🔌 Plugin architecture for custom AI providers
- 🖥️ Multi-monitor support
- ☁️ Cloud storage integration options

---

## 🏗️ Architecture Highlights

CortexView follows Clean Architecture principles with strict dependency rules:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (WPF, MVVM, Dependency Injection)                          │
└────────────────────┬────────────────────────────────────────┘
                     │ Depends on ↓
┌────────────────────┴────────────────────────────────────────┐
│                   Application Layer                          │
│  (Business Logic Orchestration)                             │
└────────────────────┬────────────────────────────────────────┘
                     │ Depends on ↓
┌────────────────────┴────────────────────────────────────────┐
│                     Domain Layer                             │
│  (Pure Business Logic, Zero Dependencies)                   │
└─────────────────────────────────────────────────────────────┘
                     ↑ Implemented by
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (External Dependencies: AWS, Win32, File I/O)              │
└─────────────────────────────────────────────────────────────┘
```

**Key Principles**:
- Domain layer has zero external dependencies
- Application and Infrastructure layers reference Domain only
- Presentation layer orchestrates via Dependency Injection
- SOLID principles enforced throughout

---

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 (64-bit)
- .NET 9.0 SDK or Runtime
- AWS credentials (for AI features)

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/SpectraAssist.git
cd SpectraAssist

# Build the solution
dotnet build SpectraAssist.sln --configuration Release

# Run the application
dotnet run --project CortexView --configuration Release
```

### Configuration

1. Copy `appsettings.json.example` to `appsettings.json`
2. Configure your AWS credentials:
```json
{
  "AiServiceConfig": {
    "Region": "us-east-1",
    "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0"
  }
}
```

3. Customize personas in `Prompts/` directory

For detailed installation instructions, see [docs/installation-guide.md](docs/installation-guide.md).

---

## 📚 Documentation

### Core Documentation
- **[Installation Guide](docs/installation-guide.md)** - Setup and configuration
- **[Architecture](docs/architecture.md)** - System design and technical details
- **[Project Structure](docs/project_structure.md)** - Codebase organization
- **[Test Strategy](docs/test_strategy.md)** - Testing approach and coverage

### Release Documentation
- **[Release Notes v0.8.0](docs/releases/v0.8.0.md)** - v0.8.0 changes
- **[Changelog](CHANGELOG.md)** - Version history

---

## 🧪 Testing

CortexView maintains >70% test coverage across all layers:

```bash
# Run all tests
dotnet test SpectraAssist.sln

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:coverage.xml -targetdir:coverage-report
```

**Test Statistics**:
- **Total Tests**: 67 passing, 3 skipped
- **Domain Layer**: 18 tests (100% coverage)
- **Application Layer**: 21 tests (>75% coverage)
- **Infrastructure Layer**: 8 tests (>60% coverage)
- **Presentation Layer**: 20 tests (>70% coverage)

---

## 📄 License

This project is licensed under  
**Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)**

You may:
- Share and adapt the material with attribution
- Not use it for commercial purposes
- Not use it for training machine learning models (including LLMs) without explicit permission

See [LICENSE.md](LICENSE.md) for full legal terms.  
Full license text: [https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)

---

## 🤝 Contributing

We welcome contributions! For details on contributing to CortexView:
- Follow Clean Architecture principles
- Write comprehensive tests (>70% coverage)
- Follow C# coding conventions
- Submit pull requests with clear descriptions

See the [Architecture Guide](docs/architecture.md) and [Test Strategy](docs/test_strategy.md) for technical details.

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/SpectraAssist/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/SpectraAssist/discussions)
- **Documentation**: [docs/](docs/)

---

## 🙏 Acknowledgments

Built with:
- **.NET 9** - Modern cross-platform framework
- **WPF** - Windows Presentation Foundation
- **AWS Bedrock** - AI model hosting
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FsCheck** - Property-based testing

Inspired by Clean Architecture principles by Robert C. Martin.
