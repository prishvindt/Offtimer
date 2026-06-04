namespace OffTimer;

internal sealed class CountdownController
{
    private readonly AppSettings _settings;
    private readonly OverlayForm _overlay;
    private readonly ShutdownService _shutdownService;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _shutdownAtUtc;

    public CountdownController(AppSettings settings, OverlayForm overlay, ShutdownService shutdownService)
    {
        _settings = settings;
        _overlay = overlay;
        _shutdownService = shutdownService;
        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (_, _) => Tick();
    }

    public event EventHandler? StateChanged;

    public bool IsActive { get; private set; }

    public TimeSpan Remaining
    {
        get
        {
            if (!IsActive)
            {
                return TimeSpan.Zero;
            }

            var remaining = _shutdownAtUtc - DateTime.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }

    public void Start(int minutes)
    {
        CancelInternal(callShutdownAbort: true);

        var seconds = checked(minutes * 60);
        _shutdownService.ScheduleShutdown(seconds);
        _shutdownAtUtc = DateTime.UtcNow.AddSeconds(seconds);
        IsActive = true;
        _timer.Start();
        Tick();
    }

    public void Cancel()
    {
        CancelInternal(callShutdownAbort: true);
    }

    private void CancelInternal(bool callShutdownAbort)
    {
        var wasActive = IsActive;
        IsActive = false;
        _timer.Stop();
        _overlay.HideOverlay();

        if (callShutdownAbort && wasActive)
        {
            _shutdownService.CancelShutdown();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Tick()
    {
        if (!IsActive)
        {
            return;
        }

        var remaining = Remaining;
        var remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);

        if (remainingSeconds <= 0)
        {
            IsActive = false;
            _timer.Stop();
            _overlay.HideOverlay();
            StateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (_settings.OverlayEnabled || remainingSeconds <= 60)
        {
            _overlay.ApplySettings(_settings);
            _overlay.UpdateCountdown(FormatOverlayValue(remainingSeconds));
            _overlay.ShowOverlay();
        }
        else
        {
            _overlay.HideOverlay();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public static string FormatOverlayValue(int remainingSeconds)
    {
        if (remainingSeconds <= 60)
        {
            return remainingSeconds.ToString("00");
        }

        var minutes = (int)Math.Ceiling(remainingSeconds / 60.0);
        return minutes.ToString("00");
    }
}
