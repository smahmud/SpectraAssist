# 📝 Changelog

All notable changes to **CortexView** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned Features
- Custom persona creation UI
- Multi-monitor support
- Plugin architecture for AI providers
- Cloud storage integration (Azure Blob, AWS S3)
- REST API layer for remote control
- Real-time observability (structured logging, tracing, metrics)

---

## [0.8.0] - 2026-01-28

### 🎉 Major Release: Clean Architecture Refactoring

This release represents a complete architectural overhaul of CortexView, transitioning from a monolithic design to Clean Architecture with full MVVM implementation.

### Added

#### Architecture
- **Clean Architecture**: 4-layer architecture (Domain, Application, Infrastructure, Presentation)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection with IOptions pattern
- **MVVM Pattern**: Full data binding with minimal code-behind (23 lines)
- **Layer Isolation**: Strict dependency rules enforced across all layers

#### Domain Layer
- `Persona` entity with validation and immutability
- `AnalysisRequest` entity for AI analysis requests
- `AnalysisResponse` entity with factory methods
- `AuditEntry` entity for audit logging
- Domain interfaces: `IAiAnalysisService`, `IScreenCaptureService`, `IStorageService`, `IChangeDetectionService`, `IAuditService`

#### Application Layer
- `AnalysisOrchestrator` for workflow coordination
- `ChangeDetectionService` with SHA256 hashing and pixel comparison
- `WindowMonitoringService` for timer-based capture

#### Infrastructure Layer
- `AwsBedrockService` for AWS Bedrock integration
- `Win32ScreenCaptureService` for Windows screen capture
- `LocalStorageService` with retention policies
- `AuditService` for audit logging
- Configuration classes: `AwsConfig`, `StorageConfig`

#### Presentation Layer
- `MainViewModel` with commands and data binding
- `ViewModelBase` with INotifyPropertyChanged
- `RelayCommand` for ICommand implementation
- Minimal code-behind (23 lines, 77% reduction)

#### Testing
- **67 tests** across 4 test projects (18 Domain + 21 Application + 8 Infrastructure + 20 Presentation)
- **Property-based testing** with FsCheck (5 property tests)
- **>70% code coverage** overall (Domain: 100%, Application: >75%)
- **Comprehensive test suite** with unit, integration, and property-based tests

#### Documentation
- Enterprise-level documentation suite
- Architecture guide with all 4 layers documented
- Test strategy with coverage metrics
- Installation guide with AWS setup
- Contributing guidelines
- Development setup guide
- User guide for end users
- Project structure documentation

### Changed

#### Breaking Changes
- **Complete rewrite**: All code refactored to Clean Architecture
- **Configuration format**: New `appsettings.json` structure with IOptions pattern
- **Dependency injection**: All services now use constructor injection
- **Async-first**: All operations now use async/await with cancellation support

#### Improvements
- **77% reduction** in MainWindow code-behind (from 100+ lines to 23 lines)
- **100% test coverage** in Domain layer
- **5.2s build time** (Release configuration)
- **5.1s test execution** (67 tests)
- **Immutable entities** for thread-safety
- **Sealed classes** to prevent inheritance issues

### Removed
- Monolithic `MainWindow.xaml.cs` implementation
- Direct AWS SDK usage in presentation layer
- Hardcoded configuration values
- Synchronous blocking operations

### Fixed
- Race conditions in change detection
- Memory leaks in screenshot capture
- Thread-safety issues in service implementations
- Configuration loading errors

### Security
- No hardcoded credentials (all from configuration)
- Async cancellation support for all operations
- Input validation in all domain entities
- Graceful error handling with no sensitive data exposure

### Performance
- **DI Resolution**: <10ms (estimated)
- **Capture Time**: 50-100ms (depends on window size)
- **Change Detection**: <50ms (hash comparison)
- **Pixel Comparison**: 100-500ms (depends on image size)
- **AI Analysis**: 2-5s (depends on AWS Bedrock response time)
- **UI Refresh**: <16ms (60 FPS maintained)

### Technical Debt Resolved
- ✅ Eliminated monolithic code-behind
- ✅ Removed tight coupling between layers
- ✅ Eliminated synchronous blocking operations
- ✅ Removed hardcoded configuration
- ✅ Eliminated code duplication
- ✅ Removed circular dependencies

---

## [0.7.5] - 2025-12-15

### Added
- Basic change detection using pixel comparison
- Screenshot storage with configurable retention
- Multiple persona support
- Configurable capture intervals

### Changed
- Improved UI responsiveness
- Enhanced error handling

### Fixed
- Memory leaks in screenshot capture
- UI freezing during AI analysis

---

## [0.7.0] - 2025-11-20

### Added
- AWS Bedrock integration for AI analysis
- Automatic window monitoring
- Manual capture mode
- Basic overlay UI

### Changed
- Switched from OpenAI to AWS Bedrock
- Improved screenshot capture performance

---

## [0.6.0] - 2025-10-15

### Added
- Initial release
- Basic screenshot capture
- OpenAI integration
- Simple overlay window

---

## Version History Summary

| Version | Release Date | Type | Key Changes |
|---------|--------------|------|-------------|
| 0.8.0 | 2026-01-28 | Major | Clean Architecture refactoring, MVVM, DI, 67 tests |
| 0.7.5 | 2025-12-15 | Minor | Change detection, storage, personas |
| 0.7.0 | 2025-11-20 | Minor | AWS Bedrock, monitoring, overlay UI |
| 0.6.0 | 2025-10-15 | Major | Initial release |

---

## Migration Guides

### Migrating from v0.7.5 to v0.8.0

**Breaking Changes**:
1. **Configuration Format**: `appsettings.json` structure has changed
2. **Dependency Injection**: All services now use constructor injection
3. **Async Operations**: All methods now use async/await

**Migration Steps**:

1. **Update Configuration**:
   ```json
   // Old (v0.7.5)
   {
     "AwsRegion": "us-east-1",
     "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0"
   }

   // New (v0.8.0)
   {
     "AiServiceConfig": {
       "Region": "us-east-1",
       "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0"
     },
     "StorageConfig": {
       "Enabled": true,
       "StoragePath": "C:\\CortexView\\Screenshots",
       "RetentionDays": 7
     }
   }
   ```

2. **Update Code** (if extending):
   - Replace synchronous methods with async equivalents
   - Use constructor injection instead of service locator
   - Update to use new domain entities

3. **Test Thoroughly**:
   - Run all tests: `dotnet test`
   - Verify configuration loading
   - Test all features manually

---

## Deprecation Notices

### v0.8.0
- **Synchronous APIs**: All synchronous methods are deprecated. Use async equivalents.
- **Service Locator**: Service locator pattern is deprecated. Use constructor injection.
- **Hardcoded Configuration**: Hardcoded values are deprecated. Use `appsettings.json`.

---

## Upgrade Path

### From v0.6.0 → v0.7.0
- Update AWS credentials to use Bedrock instead of OpenAI
- Update configuration file format

### From v0.7.0 → v0.7.5
- No breaking changes
- Update configuration to enable new features

### From v0.7.5 → v0.8.0
- **Major upgrade**: Follow migration guide above
- Backup configuration before upgrading
- Test thoroughly after upgrade

---

## Release Notes

Detailed release notes for each version are available in the `docs/releases/` directory:

- [v0.8.0 Release Notes](docs/releases/v0.8.0.md)

---

## Contributing

For guidelines on contributing to CortexView, please follow:
- Clean Architecture principles (see [Architecture Guide](docs/architecture.md))
- Comprehensive testing requirements (see [Test Strategy](docs/test_strategy.md))
- C# coding conventions and async/await patterns

---

## Links

- [GitHub Repository](https://github.com/ORIGINAL_OWNER/CortexView)
- [Issue Tracker](https://github.com/ORIGINAL_OWNER/CortexView/issues)
- [Documentation](README.md)

---

**Note**: This changelog follows [Keep a Changelog](https://keepachangelog.com/) format and [Semantic Versioning](https://semver.org/) principles.

