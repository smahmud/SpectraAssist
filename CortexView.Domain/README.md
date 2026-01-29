# CortexView.Domain

## Overview
Pure business logic layer with **zero external dependencies**.

## Contents

### Entities
Core business objects representing the domain model:
- `Persona` - AI assistant personality configuration
- `AnalysisRequest` - Request for AI image analysis
- `AnalysisResponse` - Result of AI analysis
- `AuditEntry` - Audit log record

### Interfaces
Contracts for infrastructure services (implemented in Infrastructure layer):
- `IAiAnalysisService` - AI image analysis
- `IScreenCaptureService` - Screen/window capture
- `IStorageService` - File storage and cleanup
- `IChangeDetectionService` - Image change detection

### ValueObjects
Immutable domain configuration objects:
- `DomainConfig` - Domain-specific configuration

## Principles
- **Dependency Rule**: Domain depends on nothing
- **Immutability**: All entities use `init` properties
- **Validation**: All entities have `IsValid()` methods
- **Documentation**: All public APIs have XML documentation
