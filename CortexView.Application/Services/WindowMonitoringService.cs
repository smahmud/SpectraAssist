namespace CortexView.Application.Services;

/// <summary>
/// Manages periodic window monitoring using a System.Threading.Timer.
/// </summary>
/// <remarks>
/// This service encapsulates timer management logic for automated screenshot captures.
/// It provides start/stop control and interval configuration.
/// Note: This is a framework-agnostic implementation suitable for class libraries.
/// </remarks>
public sealed class WindowMonitoringService : IDisposable
{
    private Timer? _timer;
    private TimeSpan _interval;
    private bool _isMonitoring;
    private bool _isDisposed;
    private readonly object _lock = new();

    /// <summary>
    /// Event fired when the timer triggers a capture request.
    /// </summary>
    public event EventHandler? CaptureRequested;

    /// <summary>
    /// Gets whether monitoring is currently active.
    /// </summary>
    public bool IsMonitoring
    {
        get
        {
            lock (_lock)
            {
                return _isMonitoring;
            }
        }
    }

    /// <summary>
    /// Gets or sets the current capture interval.
    /// </summary>
    public TimeSpan Interval
    {
        get
        {
            lock (_lock)
            {
                return _interval;
            }
        }
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Interval must be greater than zero.");
            }

            lock (_lock)
            {
                _interval = value;
            }
        }
    }

    public WindowMonitoringService()
    {
        _interval = TimeSpan.FromSeconds(5); // Default 5 seconds
        _isMonitoring = false;
    }

    /// <summary>
    /// Starts periodic monitoring with the configured interval.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(WindowMonitoringService));
            }

            if (_isMonitoring)
            {
                return; // Already running
            }

            _timer = new Timer(OnTimerCallback, null, _interval, _interval);
            _isMonitoring = true;
        }
    }

    /// <summary>
    /// Stops periodic monitoring.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isMonitoring)
            {
                return; // Already stopped
            }

            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
            _isMonitoring = false;
        }
    }

    /// <summary>
    /// Updates the capture interval. If monitoring is active, the new interval takes effect immediately.
    /// </summary>
    /// <param name="interval">New capture interval.</param>
    public void UpdateInterval(TimeSpan interval)
    {
        lock (_lock)
        {
            if (interval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
            }

            _interval = interval;

            if (_isMonitoring && _timer != null)
            {
                _timer.Change(_interval, _interval);
            }
        }
    }

    private void OnTimerCallback(object? state)
    {
        // Raise event to notify subscribers (typically the ViewModel)
        CaptureRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                return;
            }

            Stop();
            _isDisposed = true;
        }
    }
}
