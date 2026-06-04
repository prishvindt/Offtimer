namespace OffTimer;

internal sealed class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly Localizer _localizer;
    private readonly ComboBox _languageCombo = new();
    private readonly CheckBox _overlayEnabledCheck = new();
    private readonly NumericUpDown _fontSizeNumber = new();
    private readonly Button _colorButton = new();
    private readonly ComboBox _cornerCombo = new();
    private readonly CheckBox _minimizeOnStartCheck = new();
    private Color _selectedColor;

    public SettingsForm(AppSettings settings, Localizer localizer, string settingsPath)
    {
        _settings = settings;
        _localizer = localizer;
        _selectedColor = settings.OverlayColor;

        Text = _localizer.T("settings");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(390, 350);

        BuildUi(settingsPath);
        LoadValues();
    }

    private void BuildUi(string settingsPath)
    {
        var labelWidth = 130;
        var inputLeft = 165;
        var top = 18;
        var row = 38;

        Controls.Add(new Label
        {
            Text = _localizer.T("language"),
            Left = 16,
            Top = top + 4,
            Width = labelWidth
        });

        _languageCombo.Left = inputLeft;
        _languageCombo.Top = top;
        _languageCombo.Width = 190;
        _languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_auto"), "auto"));
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_ru"), "ru"));
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_en"), "en"));
        Controls.Add(_languageCombo);

        top += row;
        _overlayEnabledCheck.Text = _localizer.T("overlay_enabled");
        _overlayEnabledCheck.Left = 16;
        _overlayEnabledCheck.Top = top;
        _overlayEnabledCheck.Width = 340;
        Controls.Add(_overlayEnabledCheck);

        top += row;
        Controls.Add(new Label
        {
            Text = _localizer.T("overlay_size"),
            Left = 16,
            Top = top + 4,
            Width = labelWidth
        });

        _fontSizeNumber.Left = inputLeft;
        _fontSizeNumber.Top = top;
        _fontSizeNumber.Width = 90;
        _fontSizeNumber.Minimum = 12;
        _fontSizeNumber.Maximum = 220;
        _fontSizeNumber.Increment = 2;
        Controls.Add(_fontSizeNumber);

        top += row;
        Controls.Add(new Label
        {
            Text = _localizer.T("overlay_color"),
            Left = 16,
            Top = top + 4,
            Width = labelWidth
        });

        _colorButton.Left = inputLeft;
        _colorButton.Top = top;
        _colorButton.Width = 90;
        _colorButton.Height = 26;
        _colorButton.Click += (_, _) => PickColor();
        Controls.Add(_colorButton);

        top += row;
        Controls.Add(new Label
        {
            Text = _localizer.T("overlay_corner"),
            Left = 16,
            Top = top + 4,
            Width = labelWidth
        });

        _cornerCombo.Left = inputLeft;
        _cornerCombo.Top = top;
        _cornerCombo.Width = 190;
        _cornerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_topleft"), "TopLeft"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_topright"), "TopRight"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_bottomleft"), "BottomLeft"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_bottomright"), "BottomRight"));
        Controls.Add(_cornerCombo);

        top += row;
        _minimizeOnStartCheck.Text = _localizer.T("minimize_on_start");
        _minimizeOnStartCheck.Left = 16;
        _minimizeOnStartCheck.Top = top;
        _minimizeOnStartCheck.Width = 340;
        Controls.Add(_minimizeOnStartCheck);

        top += 48;
        Controls.Add(new Label
        {
            Text = $"{_localizer.T("settings_path")}:\n{settingsPath}",
            Left = 16,
            Top = top,
            Width = 350,
            Height = 45
        });

        var saveButton = new Button
        {
            Text = _localizer.T("save"),
            Left = 190,
            Top = 310,
            Width = 80,
            DialogResult = DialogResult.OK
        };
        saveButton.Click += (_, _) => SaveValues();
        Controls.Add(saveButton);

        var cancelButton = new Button
        {
            Text = _localizer.T("cancel"),
            Left = 280,
            Top = 310,
            Width = 80,
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(cancelButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadValues()
    {
        SelectComboValue(_languageCombo, _settings.Language);
        _overlayEnabledCheck.Checked = _settings.OverlayEnabled;
        _fontSizeNumber.Value = _settings.OverlayFontSize;
        UpdateColorButton();
        SelectComboValue(_cornerCombo, _settings.OverlayCorner);
        _minimizeOnStartCheck.Checked = _settings.MinimizeToTrayOnTimerStart;
    }

    private void SaveValues()
    {
        _settings.Language = GetComboValue(_languageCombo, "auto");
        _settings.OverlayEnabled = _overlayEnabledCheck.Checked;
        _settings.OverlayFontSize = (int)_fontSizeNumber.Value;
        _settings.OverlayColor = _selectedColor;
        _settings.OverlayCorner = GetComboValue(_cornerCombo, "TopRight");
        _settings.MinimizeToTrayOnTimerStart = _minimizeOnStartCheck.Checked;
    }

    private void PickColor()
    {
        using var dialog = new ColorDialog
        {
            Color = _selectedColor,
            FullOpen = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _selectedColor = dialog.Color;
            UpdateColorButton();
        }
    }

    private void UpdateColorButton()
    {
        _colorButton.BackColor = _selectedColor;
        _colorButton.Text = "";
    }

    private static void SelectComboValue(ComboBox comboBox, string value)
    {
        for (var i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is ComboItem item && item.Value == value)
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }

        if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private static string GetComboValue(ComboBox comboBox, string fallback)
    {
        return comboBox.SelectedItem is ComboItem item ? item.Value : fallback;
    }

    private sealed record ComboItem(string Text, string Value)
    {
        public override string ToString() => Text;
    }
}
