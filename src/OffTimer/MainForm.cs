namespace OffTimer;

internal sealed class MainForm : Form
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly Localizer _localizer;
    private readonly CountdownController _controller;
    private readonly OverlayForm _overlay;
    private readonly NotifyIcon _notifyIcon;
    private readonly TextBox _minutesTextBox = new();
    private readonly Button _okButton = new();
    private readonly Button _cancelButton = new();
    private readonly Button _overlayButton = new();
    private readonly Button _settingsButton = new();
    private readonly Label _statusLabel = new();
    private bool _exitRequested;

    public MainForm(
        SettingsService settingsService,
        AppSettings settings,
        Localizer localizer,
        CountdownController controller,
        OverlayForm overlay)
    {
        _settingsService = settingsService;
        _settings = settings;
        _localizer = localizer;
        _controller = controller;
        _overlay = overlay;

        _controller.StateChanged += (_, _) => UpdateStateText();

        Text = _localizer.T("app_title");
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(330, 205);

        BuildUi();
        _notifyIcon = CreateNotifyIcon();
        ApplyLocalization();
        UpdateStateText();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == SingleInstance.ShowMessageId)
        {
            ShowFromTray();
        }

        base.WndProc(ref m);
    }

    private void BuildUi()
    {
        var minutesLabel = new Label
        {
            Left = 16,
            Top = 19,
            Width = 80
        };
        minutesLabel.Name = "minutesLabel";
        Controls.Add(minutesLabel);

        _minutesTextBox.Left = 95;
        _minutesTextBox.Top = 16;
        _minutesTextBox.Width = 90;
        _minutesTextBox.Text = "60";
        _minutesTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartFromTextBox();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };
        Controls.Add(_minutesTextBox);

        _okButton.Left = 195;
        _okButton.Top = 15;
        _okButton.Width = 50;
        _okButton.Height = 25;
        _okButton.Click += (_, _) => StartFromTextBox();
        Controls.Add(_okButton);

        _settingsButton.Left = 255;
        _settingsButton.Top = 15;
        _settingsButton.Width = 34;
        _settingsButton.Height = 25;
        _settingsButton.Text = "⚙";
        _settingsButton.Click += (_, _) => OpenSettings();
        Controls.Add(_settingsButton);

        var quickValues = new[] { 30, 45, 60, 90 };
        for (var i = 0; i < quickValues.Length; i++)
        {
            var value = quickValues[i];
            var button = new Button
            {
                Text = value.ToString(),
                Left = 16 + i * 74,
                Top = 58,
                Width = 64,
                Height = 30
            };
            button.Click += (_, _) => StartTimer(value);
            Controls.Add(button);
        }

        _overlayButton.Left = 16;
        _overlayButton.Top = 105;
        _overlayButton.Width = 135;
        _overlayButton.Height = 30;
        _overlayButton.Click += (_, _) => ToggleOverlay();
        Controls.Add(_overlayButton);

        _cancelButton.Left = 161;
        _cancelButton.Top = 105;
        _cancelButton.Width = 135;
        _cancelButton.Height = 30;
        _cancelButton.Click += (_, _) => CancelTimer();
        Controls.Add(_cancelButton);

        _statusLabel.Left = 16;
        _statusLabel.Top = 155;
        _statusLabel.Width = 285;
        _statusLabel.Height = 35;
        Controls.Add(_statusLabel);
    }

    private NotifyIcon CreateNotifyIcon()
    {
        var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        var menu = new ContextMenuStrip();
        menu.Items.Add(_localizer.T("show"), null, (_, _) => ShowFromTray());
        menu.Items.Add(_localizer.T("cancel_timer"), null, (_, _) => CancelTimer());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_localizer.T("exit"), null, (_, _) => ExitApplication());

        var notifyIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "OffTimer",
            Visible = true,
            ContextMenuStrip = menu
        };
        notifyIcon.DoubleClick += (_, _) => ShowFromTray();
        return notifyIcon;
    }

    private void ApplyLocalization()
    {
        Text = _localizer.T("app_title");
        if (Controls.Find("minutesLabel", false).FirstOrDefault() is Label minutesLabel)
        {
            minutesLabel.Text = _localizer.T("minutes");
        }

        _okButton.Text = _localizer.T("ok");
        _cancelButton.Text = _localizer.T("cancel_timer");
        _settingsButton.AccessibleName = _localizer.T("settings");
        UpdateOverlayButtonText();
        RebuildTrayMenu();
        UpdateStateText();
    }

    private void RebuildTrayMenu()
    {
        if (_notifyIcon.ContextMenuStrip is null)
        {
            return;
        }

        _notifyIcon.ContextMenuStrip.Items.Clear();
        _notifyIcon.ContextMenuStrip.Items.Add(_localizer.T("show"), null, (_, _) => ShowFromTray());
        _notifyIcon.ContextMenuStrip.Items.Add(_localizer.T("cancel_timer"), null, (_, _) => CancelTimer());
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add(_localizer.T("exit"), null, (_, _) => ExitApplication());
    }

    private void StartFromTextBox()
    {
        if (!int.TryParse(_minutesTextBox.Text.Trim(), out var minutes) || minutes < 1 || minutes > 1440)
        {
            MessageBox.Show(this, _localizer.T("invalid_minutes"), _localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        StartTimer(minutes);
    }

    private void StartTimer(int minutes)
    {
        try
        {
            _controller.Start(minutes);
            UpdateStateText();
            if (_settings.MinimizeToTrayOnTimerStart)
            {
                HideToTray(showBalloon: false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, _localizer.F("shutdown_error", ex.Message), _localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CancelTimer()
    {
        try
        {
            _controller.Cancel();
            UpdateStateText();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, _localizer.F("cancel_error", ex.Message), _localizer.T("app_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleOverlay()
    {
        _settings.OverlayEnabled = !_settings.OverlayEnabled;
        _settingsService.Save(_settings);
        UpdateOverlayButtonText();

        if (!_settings.OverlayEnabled && (!_controller.IsActive || _controller.Remaining.TotalSeconds > 60))
        {
            _overlay.HideOverlay();
        }
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm(_settings, _localizer, _settingsService.SettingsPath);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _settingsService.Save(_settings);
            _overlay.ApplySettings(_settings);
            ApplyLocalization();
        }
    }

    private void UpdateOverlayButtonText()
    {
        _overlayButton.Text = _settings.OverlayEnabled
            ? _localizer.T("overlay_on")
            : _localizer.T("overlay_off");
    }

    private void UpdateStateText()
    {
        if (!_controller.IsActive)
        {
            _statusLabel.Text = _localizer.T("timer_off");
            if (_notifyIcon is not null)
            {
                _notifyIcon.Text = "OffTimer";
            }
            return;
        }

        var remaining = _controller.Remaining;
        var text = remaining.TotalSeconds <= 60
            ? CountdownController.FormatOverlayValue((int)Math.Ceiling(remaining.TotalSeconds))
            : $"{(int)Math.Floor(remaining.TotalHours):00}:{remaining.Minutes:00}:{remaining.Seconds:00}";

        _statusLabel.Text = _localizer.F("timer_active", text);
        _notifyIcon.Text = text.Length <= 63 ? $"OffTimer - {text}" : "OffTimer";
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void HideToTray(bool showBalloon)
    {
        Hide();
        if (showBalloon)
        {
            _notifyIcon.BalloonTipTitle = "OffTimer";
            _notifyIcon.BalloonTipText = _localizer.T("running_in_tray");
            _notifyIcon.ShowBalloonTip(1000);
        }
    }

    private void ExitApplication()
    {
        if (_controller.IsActive)
        {
            CancelTimer();
        }

        _exitRequested = true;
        _notifyIcon.Visible = false;
        Close();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            HideToTray(showBalloon: false);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_exitRequested && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray(showBalloon: false);
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _overlay.Dispose();
        base.OnFormClosing(e);
    }
}
