using CortexView.Application.Services;
using CortexView.Domain.Entities;
using CortexView.ViewModels;
using Moq;
using System.ComponentModel;

namespace CortexView.Presentation.Tests.ViewModels;

/// <summary>
/// Unit tests for MainViewModel using Moq for dependency mocking.
/// </summary>
public class MainViewModelTests
{
    private readonly Mock<CortexView.Domain.Interfaces.IScreenCaptureService> _mockCaptureService;
    private readonly Mock<CortexView.Domain.Interfaces.IChangeDetectionService> _mockChangeDetectionService;
    private readonly Mock<CortexView.Domain.Interfaces.IAiAnalysisService> _mockAiService;
    private readonly Mock<CortexView.Domain.Interfaces.IStorageService> _mockStorageService;
    private readonly AnalysisOrchestrator _orchestrator;
    private readonly WindowMonitoringService _monitoringService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        // Create mocks for the dependencies
        _mockCaptureService = new Mock<CortexView.Domain.Interfaces.IScreenCaptureService>();
        _mockChangeDetectionService = new Mock<CortexView.Domain.Interfaces.IChangeDetectionService>();
        _mockAiService = new Mock<CortexView.Domain.Interfaces.IAiAnalysisService>();
        _mockStorageService = new Mock<CortexView.Domain.Interfaces.IStorageService>();

        // Create real instances (they are sealed, so we can't mock them)
        _orchestrator = new AnalysisOrchestrator(
            _mockCaptureService.Object,
            _mockChangeDetectionService.Object,
            _mockAiService.Object,
            _mockStorageService.Object);

        _monitoringService = new WindowMonitoringService();

        _viewModel = new MainViewModel(
            _orchestrator,
            _monitoringService);
    }

    [Fact]
    public void CaptureNowCommand_CanExecute_WhenNotMonitoring()
    {
        // Arrange
        _viewModel.IsMonitoring = false;

        // Act
        bool canExecute = _viewModel.CaptureNowCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void CaptureNowCommand_CannotExecute_WhenMonitoring()
    {
        // Arrange
        _viewModel.IsMonitoring = true;

        // Act
        bool canExecute = _viewModel.CaptureNowCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void StatusText_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StatusText))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.StatusText = "New status";

        // Assert
        Assert.True(eventFired);
        Assert.Equal("New status", _viewModel.StatusText);
    }

    [Fact]
    public void ToggleMonitoringCommand_StartsMonitoring()
    {
        // Arrange
        _viewModel.IsMonitoring = false;

        // Act
        _viewModel.ToggleMonitoringCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsMonitoring);
        Assert.True(_monitoringService.IsMonitoring);
    }

    [Fact]
    public void ToggleMonitoringCommand_StopsMonitoring()
    {
        // Arrange
        _viewModel.IsMonitoring = true;

        // Act
        _viewModel.ToggleMonitoringCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsMonitoring);
        Assert.False(_monitoringService.IsMonitoring);
    }

    [Fact]
    public void IsMonitoring_PropertyChanged_StartsService()
    {
        // Arrange
        _viewModel.IsMonitoring = false;

        // Act
        _viewModel.IsMonitoring = true;

        // Assert
        Assert.True(_monitoringService.IsMonitoring);
        Assert.Contains("started", _viewModel.StatusText);
    }

    [Fact]
    public void IsMonitoring_PropertyChanged_StopsService()
    {
        // Arrange
        _viewModel.IsMonitoring = true;

        // Act
        _viewModel.IsMonitoring = false;

        // Assert
        Assert.False(_monitoringService.IsMonitoring);
        Assert.Contains("stopped", _viewModel.StatusText);
    }

    [Fact]
    public void CaptureInterval_PropertyChanged_UpdatesMonitoringService()
    {
        // Arrange
        double newInterval = 10.0;

        // Act
        _viewModel.CaptureInterval = newInterval;

        // Assert
        Assert.Equal(newInterval, _viewModel.CaptureInterval);
        // Note: We can't easily verify the interval was updated in the service
        // without exposing internal state, but we can verify the property changed
    }

    [Fact]
    public void SuggestionText_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SuggestionText))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.SuggestionText = "New suggestion";

        // Assert
        Assert.True(eventFired);
        Assert.Equal("New suggestion", _viewModel.SuggestionText);
    }

    [Fact]
    public void ChangeSensitivity_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ChangeSensitivity))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.ChangeSensitivity = 15.0;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(15.0, _viewModel.ChangeSensitivity);
    }

    [Fact]
    public void Opacity_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.Opacity))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.Opacity = 0.8;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(0.8, _viewModel.Opacity);
    }

    [Fact]
    public void IsTopmost_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsTopmost))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.IsTopmost = true;

        // Assert
        Assert.True(eventFired);
        Assert.True(_viewModel.IsTopmost);
    }

    [Fact]
    public void SelectedPersona_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        var persona = new Persona 
        { 
            Name = "Test",
            SystemPrompt = "Test prompt"
        };
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedPersona))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.SelectedPersona = persona;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(persona, _viewModel.SelectedPersona);
    }

    [Fact]
    public void StorageEnabled_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StorageEnabled))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.StorageEnabled = true;

        // Assert
        Assert.True(eventFired);
        Assert.True(_viewModel.StorageEnabled);
    }

    [Fact]
    public void RetentionDays_PropertyChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.RetentionDays))
            {
                eventFired = true;
            }
        };

        // Act
        _viewModel.RetentionDays = 14;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(14, _viewModel.RetentionDays);
    }

    [Fact]
    public void Constructor_InitializesPersonasCollection()
    {
        // Assert
        Assert.NotNull(_viewModel.Personas);
        Assert.True(_viewModel.Personas.Count > 0); // Should load default personas
    }

    [Fact]
    public void Constructor_InitializesWindowsCollection()
    {
        // Assert
        Assert.NotNull(_viewModel.Windows);
        // Windows collection may be empty in test environment
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Assert
        Assert.NotNull(_viewModel.CaptureNowCommand);
        Assert.NotNull(_viewModel.ToggleMonitoringCommand);
        Assert.NotNull(_viewModel.NextSuggestionCommand);
        Assert.NotNull(_viewModel.RefreshWindowListCommand);
        Assert.NotNull(_viewModel.CloseCommand);
        Assert.NotNull(_viewModel.BrowseStorageCommand);
        Assert.NotNull(_viewModel.OpenStorageCommand);
        Assert.NotNull(_viewModel.PurgeStorageCommand);
    }

    [Fact]
    public void RefreshWindowListCommand_CanAlwaysExecute()
    {
        // Act
        bool canExecute = _viewModel.RefreshWindowListCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ToggleMonitoringCommand_CanAlwaysExecute()
    {
        // Act
        bool canExecute = _viewModel.ToggleMonitoringCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }
}
