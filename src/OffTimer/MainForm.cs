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
    private readonly Label _minutesLabel = new();
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
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        MinimumSize = new Size(380, 0);

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
        var root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        Controls.Add(root);

        var inputRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.Controls.Add(inputRow, 0, 0);

        _minutesLabel.AutoSize = true;
        _minutesLabel.Margin = new Padding(0, 7, 8, 0);
        inputRow.Controls.Add(_minutesLabel);

        _minutesTextBox.Width = ScaleByDpi(96);
        _minutesTextBox.Margin = new Padding(0, 3, 8, 0);
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
        inputRow.Controls.Add(_minutesTextBox);

        ConfigureButton(_okButton, new Size(64, 30));
        _okButton.Margin = new Padding(0, 0, 8, 0);
        _okButton.Click += (_, _) => StartFromTextBox();
        inputRow.Controls.Add(_okButton);

        ConfigureButton(_settingsButton, new Size(64, 30));
        _settingsButton.AutoSize = false;
        _settingsButton.Size = new Size(ScaleByDpi(64), ScaleByDpi(30));
        _settingsButton.Padding = Padding.Empty;
        _settingsButton.Font = new Font("Segoe UI Symbol", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _settingsButton.Text = "⚙";
        _settingsButton.TextAlign = ContentAlignment.MiddleCenter;
        _settingsButton.Margin = Padding.Empty;
        _settingsButton.Click += (_, _) => OpenSettings();
        inputRow.Controls.Add(_settingsButton);

        var quickRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 16, 0, 0),
            Padding = Padding.Empty
        };
        root.Controls.Add(quickRow, 0, 1);

        var quickValues = new[] { 30, 45, 60, 90 };
        foreach (var value in quickValues)
        {
            var button = new Button { Text = value.ToString() };
            ConfigureButton(button, new Size(72, 34));
            button.Margin = new Padding(0, 0, 8, 0);
            button.Click += (_, _) => StartTimer(value);
            quickRow.Controls.Add(button);
        }

        var actionsRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 16, 0, 0),
            Padding = Padding.Empty
        };
        root.Controls.Add(actionsRow, 0, 2);

        ConfigureButton(_overlayButton, new Size(140, 34));
        _overlayButton.Margin = new Padding(0, 0, 8, 0);
        _overlayButton.Click += (_, _) => ToggleOverlay();
        actionsRow.Controls.Add(_overlayButton);

        ConfigureButton(_cancelButton, new Size(140, 34));
        _cancelButton.Margin = Padding.Empty;
        _cancelButton.Click += (_, _) => CancelTimer();
        actionsRow.Controls.Add(_cancelButton);

        _statusLabel.AutoSize = true;
        _statusLabel.MaximumSize = new Size(520, 0);
        _statusLabel.Margin = new Padding(0, 18, 0, 0);
        root.Controls.Add(_statusLabel, 0, 3);
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
        _minutesLabel.Text = _localizer.T("minutes");
        _okButton.Text = _localizer.T("ok");
        _cancelButton.Text = _localizer.T("cancel_timer");
        _settingsButton.AccessibleName = _localizer.T("settings");
        UpdateOverlayButtonText();
        RebuildTrayMenu();
        UpdateStateText();
        PerformLayout();
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

    private static void ConfigureButton(Button button, Size minimumSize)
    {
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.MinimumSize = minimumSize;
        button.Padding = new Padding(8, 3, 8, 3);
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseVisualStyleBackColor = true;
    }

    private int ScaleByDpi(int value)
    {
        return (int)Math.Round(value * DeviceDpi / 96.0);
    }
}
