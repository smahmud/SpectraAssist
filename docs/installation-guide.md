# Installation & Setup Guide

This guide provides comprehensive instructions for installing CortexView and all its dependencies, along with validation steps to ensure everything is working correctly.

## 📋 Overview

CortexView requires both **.NET 9 Runtime** and **AWS credentials** to function properly. This guide covers:

- System requirements
- .NET 9 installation
- AWS credentials configuration
- Application installation
- Installation verification
- Troubleshooting common issues

---

## 💻 System Requirements

### Minimum Requirements
- **Operating System**: Windows 10 (64-bit) or later
- **Processor**: Intel Core i3 or equivalent
- **Memory**: 4 GB RAM
- **Disk Space**: 500 MB free space
- **Display**: 1280x720 resolution or higher
- **.NET**: .NET 9.0 Runtime or SDK

### Recommended Requirements
- **Operating System**: Windows 11 (64-bit)
- **Processor**: Intel Core i5 or equivalent
- **Memory**: 8 GB RAM
- **Disk Space**: 2 GB free space (for screenshot storage)
- **Display**: 1920x1080 resolution or higher
- **.NET**: .NET 9.0 SDK (for development)

### External Dependencies
- **AWS Account**: Required for AI analysis features
- **Internet Connection**: Required for AWS Bedrock API calls

---

## 🔧 .NET 9 Installation

### Check Existing Installation

First, check if .NET 9 is already installed:

```powershell
dotnet --version
```

If the output shows `9.0.x`, you're ready to go. Otherwise, proceed with installation.

### Windows Installation

**Option 1: Using Windows Package Manager (Recommended)**
```powershell
# Run as Administrator
winget install Microsoft.DotNet.SDK.9

# Restart your terminal after installation
```

**Option 2: Manual Installation**
1. Download from [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Choose "Download .NET 9.0 SDK" (for development) or "Download .NET 9.0 Runtime" (for running only)
3. Run the installer
4. Restart your terminal

### Verify Installation

```powershell
# Check .NET version
dotnet --version
# Expected output: 9.0.x

# List installed SDKs
dotnet --list-sdks
# Should show: 9.0.x [installation path]

# List installed runtimes
dotnet --list-runtimes
# Should show: Microsoft.NETCore.App 9.0.x [installation path]
```

---

## 🔑 AWS Credentials Configuration

CortexView uses AWS Bedrock for AI analysis. You'll need AWS credentials with Bedrock access.

### Step 1: Create AWS Account

If you don't have an AWS account:
1. Go to [https://aws.amazon.com/](https://aws.amazon.com/)
2. Click "Create an AWS Account"
3. Follow the registration process

### Step 2: Enable AWS Bedrock Access

1. Log in to AWS Console
2. Navigate to AWS Bedrock service
3. Request access to Claude 3 models (if not already enabled)
4. Wait for approval (usually instant for most accounts)

### Step 3: Create IAM User

1. Go to IAM Console: [https://console.aws.amazon.com/iam/](https://console.aws.amazon.com/iam/)
2. Click "Users" → "Add users"
3. Enter username (e.g., `cortexview-user`)
4. Select "Programmatic access"
5. Click "Next: Permissions"

### Step 4: Attach Bedrock Policy

1. Click "Attach existing policies directly"
2. Search for "Bedrock"
3. Select `AmazonBedrockFullAccess` (or create custom policy with minimal permissions)
4. Click "Next: Tags" → "Next: Review" → "Create user"

### Step 5: Save Credentials

1. **IMPORTANT**: Copy the Access Key ID and Secret Access Key
2. Store them securely (you won't be able to see the secret key again)

### Step 6: Configure AWS Credentials

**Option 1: AWS CLI Configuration (Recommended)**
```powershell
# Install AWS CLI if not already installed
winget install Amazon.AWSCLI

# Configure credentials
aws configure
# Enter:
#   AWS Access Key ID: [your-access-key-id]
#   AWS Secret Access Key: [your-secret-access-key]
#   Default region name: us-east-1
#   Default output format: json
```

**Option 2: Environment Variables**
```powershell
# Set environment variables (PowerShell)
$env:AWS_ACCESS_KEY_ID="your-access-key-id"
$env:AWS_SECRET_ACCESS_KEY="your-secret-access-key"
$env:AWS_DEFAULT_REGION="us-east-1"

# Make permanent (add to system environment variables)
[System.Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "your-access-key-id", "User")
[System.Environment]::SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "your-secret-access-key", "User")
[System.Environment]::SetEnvironmentVariable("AWS_DEFAULT_REGION", "us-east-1", "User")
```

**Option 3: Credentials File**
Create `~/.aws/credentials`:
```ini
[default]
aws_access_key_id = your-access-key-id
aws_secret_access_key = your-secret-access-key
```

Create `~/.aws/config`:
```ini
[default]
region = us-east-1
output = json
```

---

## 📦 CortexView Installation

### Method 1: Binary Release (Recommended for Users)

1. **Download Latest Release**
   - Go to [GitHub Releases](https://github.com/yourusername/SpectraAssist/releases)
   - Download `CortexView-v0.8.0-win-x64.zip`

2. **Extract Files**
   ```powershell
   # Extract to desired location
   Expand-Archive -Path CortexView-v0.8.0-win-x64.zip -DestinationPath C:\CortexView
   cd C:\CortexView
   ```

3. **Configure Application**
   - Copy `appsettings.json.example` to `appsettings.json`
   - Edit `appsettings.json` with your AWS region and model ID

4. **Run Application**
   ```powershell
   .\CortexView.exe
   ```

### Method 2: Build from Source (Recommended for Developers)

1. **Clone Repository**
   ```powershell
   git clone https://github.com/yourusername/SpectraAssist.git
   cd SpectraAssist
   ```

2. **Restore Dependencies**
   ```powershell
   dotnet restore SpectraAssist.sln
   ```

3. **Build Solution**
   ```powershell
   # Debug build
   dotnet build SpectraAssist.sln --configuration Debug

   # Release build
   dotnet build SpectraAssist.sln --configuration Release
   ```

4. **Configure Application**
   - Navigate to `CortexView/bin/Release/net9.0-windows/`
   - Copy `appsettings.json.example` to `appsettings.json`
   - Edit `appsettings.json` with your configuration

5. **Run Application**
   ```powershell
   # From solution root
   dotnet run --project CortexView --configuration Release

   # Or directly
   cd CortexView/bin/Release/net9.0-windows
   .\CortexView.exe
   ```

---

## ⚙️ Configuration

### Application Settings

Edit `appsettings.json`:

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

**Configuration Options**:

| Setting | Description | Default | Options |
|---------|-------------|---------|---------|
| `AiServiceConfig.Region` | AWS region for Bedrock | `us-east-1` | Any AWS region with Bedrock |
| `AiServiceConfig.ModelId` | Claude model ID | `anthropic.claude-3-sonnet-20240229-v1:0` | See AWS Bedrock console |
| `StorageConfig.Enabled` | Enable screenshot storage | `true` | `true`, `false` |
| `StorageConfig.StoragePath` | Screenshot storage directory | `./screenshots` | Any valid path |
| `StorageConfig.RetentionDays` | Days to keep screenshots | `7` | Any positive integer |

### Available Claude Models

| Model ID | Description | Cost | Speed |
|----------|-------------|------|-------|
| `anthropic.claude-3-sonnet-20240229-v1:0` | Balanced performance | Medium | Fast |
| `anthropic.claude-3-haiku-20240307-v1:0` | Fastest, most economical | Low | Very Fast |
| `anthropic.claude-3-opus-20240229-v1:0` | Most capable | High | Slower |

### Persona Configuration

Personas are defined in `Prompts/*.md` files:

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

**Persona Parameters**:
- `name`: Display name for the persona
- `temperature`: Creativity (0.0-1.0, lower = more focused)
- `top_p`: Nucleus sampling (0.0-1.0, lower = more deterministic)
- `max_tokens`: Maximum response length (1-4096)

---

## ✅ Installation Verification

### Step 1: Verify .NET Installation

```powershell
dotnet --version
# Expected: 9.0.x
```

### Step 2: Verify AWS Credentials

```powershell
# If using AWS CLI
aws sts get-caller-identity
# Should return your AWS account information

# If using environment variables
echo $env:AWS_ACCESS_KEY_ID
# Should show your access key ID
```

### Step 3: Verify Application Build

```powershell
# From solution root
dotnet build SpectraAssist.sln --configuration Release
# Should complete without errors
```

### Step 4: Run Tests

```powershell
# Run all tests
dotnet test SpectraAssist.sln

# Expected output:
# Total Tests: 70
# Passed: 67
# Skipped: 3
# Failed: 0
```

### Step 5: Launch Application

```powershell
dotnet run --project CortexView --configuration Release
```

**Expected Behavior**:
- Application window appears in top-right corner
- Status shows "Ready"
- Window list populates with available windows
- Persona list shows available personas

### Step 6: Test Capture

1. Click "Capture Now" button
2. Status should change to "Capturing..."
3. After a few seconds, status should show "Analysis complete"
4. Suggestion text should appear with AI-generated content

---

## 🧪 Validation Script

Save this as `validate_installation.ps1`:

```powershell
#!/usr/bin/env pwsh
# CortexView Installation Validator

Write-Host "🔍 CortexView Installation Validator" -ForegroundColor Cyan
Write-Host "=" * 50

# Check .NET
Write-Host "`n📦 Checking .NET Installation..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET not found. Please install .NET 9.0" -ForegroundColor Red
    exit 1
}

# Check AWS Credentials
Write-Host "`n🔑 Checking AWS Credentials..." -ForegroundColor Yellow
if ($env:AWS_ACCESS_KEY_ID) {
    Write-Host "✅ AWS_ACCESS_KEY_ID found" -ForegroundColor Green
} else {
    Write-Host "⚠️  AWS_ACCESS_KEY_ID not found in environment" -ForegroundColor Yellow
}

# Check Solution
Write-Host "`n🏗️  Checking Solution..." -ForegroundColor Yellow
if (Test-Path "SpectraAssist.sln") {
    Write-Host "✅ Solution file found" -ForegroundColor Green
} else {
    Write-Host "❌ Solution file not found" -ForegroundColor Red
    exit 1
}

# Build Solution
Write-Host "`n🔨 Building Solution..." -ForegroundColor Yellow
$buildResult = dotnet build SpectraAssist.sln --configuration Release --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

# Run Tests
Write-Host "`n🧪 Running Tests..." -ForegroundColor Yellow
$testResult = dotnet test SpectraAssist.sln --no-build --configuration Release --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some tests failed (check output above)" -ForegroundColor Yellow
}

Write-Host "`n" + "=" * 50
Write-Host "🎉 Installation validation complete!" -ForegroundColor Green
Write-Host "✅ CortexView is ready to use." -ForegroundColor Green
```

**Usage**:
```powershell
.\validate_installation.ps1
```

---

## 🔧 Troubleshooting

### Common Issues and Solutions

#### 1. .NET Not Found

**Error**: `'dotnet' is not recognized as an internal or external command`

**Solutions**:
- Restart your terminal after .NET installation
- Verify .NET is in your system PATH
- Reinstall .NET 9.0 SDK

**Verification**:
```powershell
# Check PATH
$env:PATH -split ';' | Select-String "dotnet"

# Should show: C:\Program Files\dotnet\
```

#### 2. AWS Credentials Not Found

**Error**: `Unable to get IAM security credentials from EC2 Instance Metadata Service`

**Solutions**:
- Verify AWS credentials are configured (see AWS Configuration section)
- Check environment variables: `echo $env:AWS_ACCESS_KEY_ID`
- Verify `~/.aws/credentials` file exists and is valid

**Verification**:
```powershell
# Test AWS credentials
aws sts get-caller-identity
```

#### 3. Build Errors

**Error**: `error MSB4236: The SDK 'Microsoft.NET.Sdk' specified could not be found`

**Solutions**:
- Install .NET 9.0 SDK (not just Runtime)
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Restore packages: `dotnet restore SpectraAssist.sln`

#### 4. Missing Dependencies

**Error**: `Could not load file or assembly 'AWSSDK.BedrockRuntime'`

**Solutions**:
```powershell
# Restore NuGet packages
dotnet restore SpectraAssist.sln

# Clean and rebuild
dotnet clean SpectraAssist.sln
dotnet build SpectraAssist.sln --configuration Release
```

#### 5. Application Won't Start

**Error**: Application crashes on startup

**Solutions**:
- Check `appsettings.json` exists and is valid JSON
- Verify AWS credentials are configured
- Check Windows Event Viewer for error details
- Run from command line to see error messages:
  ```powershell
  dotnet run --project CortexView --configuration Release
  ```

#### 6. AWS Bedrock Access Denied

**Error**: `User is not authorized to perform: bedrock:InvokeModel`

**Solutions**:
- Verify IAM user has Bedrock permissions
- Attach `AmazonBedrockFullAccess` policy to IAM user
- Check AWS region supports Bedrock (use `us-east-1` or `us-west-2`)
- Request Bedrock model access in AWS Console

#### 7. Screenshot Capture Fails

**Error**: `Failed to capture window`

**Solutions**:
- Ensure target window is visible and not minimized
- Run CortexView as Administrator (for some protected windows)
- Check Windows permissions for screen capture
- Verify target window handle is valid

---

## 📚 Additional Resources

### Documentation Links
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [AWS Bedrock Documentation](https://docs.aws.amazon.com/bedrock/)
- [Claude 3 Model Documentation](https://docs.anthropic.com/claude/docs)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

### Getting Help
- **Issues**: [GitHub Issues](https://github.com/yourusername/SpectraAssist/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/SpectraAssist/discussions)
- **Documentation**: [docs/](.)

### Version Compatibility
- **.NET**: 9.0+ (required)
- **Operating System**: Windows 10+ (64-bit)
- **AWS Bedrock**: Claude 3 models (Haiku, Sonnet, Opus)
- **Architecture**: x64 only

---

## 🚀 Next Steps

After successful installation:

1. **Configure Personas**: Customize `Prompts/*.md` files for your use cases
2. **Adjust Settings**: Fine-tune capture interval and sensitivity in the UI
3. **Test Workflow**: Try manual capture and automatic monitoring
4. **Review Documentation**: See [docs/README.md](README.md) for comprehensive documentation

---

For architecture details, see [architecture.md](architecture.md).  
For project structure, see [project_structure.md](project_structure.md).  
For testing information, see [test_strategy.md](test_strategy.md).
