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
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        MinimumSize = new Size(470, 0);

        BuildUi(settingsPath);
        LoadValues();
    }

    private void BuildUi(string settingsPath)
    {
        var root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        Controls.Add(root);

        var row = 0;
        AddLabeledControl(root, row++, _localizer.T("language"), _languageCombo);
        _languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageCombo.Width = 230;
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_auto"), "auto"));
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_ru"), "ru"));
        _languageCombo.Items.Add(new ComboItem(_localizer.T("language_en"), "en"));

        ConfigureCheckBox(_overlayEnabledCheck, _localizer.T("overlay_enabled"));
        AddFullWidthControl(root, row++, _overlayEnabledCheck);

        AddLabeledControl(root, row++, _localizer.T("overlay_size"), _fontSizeNumber);
        _fontSizeNumber.Width = 110;
        _fontSizeNumber.Minimum = 12;
        _fontSizeNumber.Maximum = 220;
        _fontSizeNumber.Increment = 2;

        AddLabeledControl(root, row++, _localizer.T("overlay_color"), _colorButton);
        _colorButton.AutoSize = false;
        _colorButton.Size = new Size(110, 30);
        _colorButton.Click += (_, _) => PickColor();

        AddLabeledControl(root, row++, _localizer.T("overlay_corner"), _cornerCombo);
        _cornerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _cornerCombo.Width = 230;
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_topleft"), "TopLeft"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_topright"), "TopRight"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_bottomleft"), "BottomLeft"));
        _cornerCombo.Items.Add(new ComboItem(_localizer.T("corner_bottomright"), "BottomRight"));

        ConfigureCheckBox(_minimizeOnStartCheck, _localizer.T("minimize_on_start"));
        AddFullWidthControl(root, row++, _minimizeOnStartCheck);

        var pathLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(620, 0),
            Text = $"{_localizer.T("settings_path")}:\n{settingsPath}",
            Margin = new Padding(0, 14, 0, 0)
        };
        AddFullWidthControl(root, row++, pathLabel);

        var buttonsRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 18, 0, 0),
            Padding = Padding.Empty
        };
        AddFullWidthControl(root, row, buttonsRow);

        var cancelButton = CreateDialogButton(_localizer.T("cancel"), DialogResult.Cancel);
        buttonsRow.Controls.Add(cancelButton);

        var saveButton = CreateDialogButton(_localizer.T("save"), DialogResult.OK);
        saveButton.Click += (_, _) => SaveValues();
        buttonsRow.Controls.Add(saveButton);

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
        _colorButton.Text = string.Empty;
    }

    private static void AddLabeledControl(TableLayoutPanel root, int row, string labelText, Control control)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            AutoSize = true,
            Text = labelText,
            Margin = new Padding(0, 6, 18, 10)
        };
        root.Controls.Add(label, 0, row);

        control.Margin = new Padding(0, 0, 0, 10);
        root.Controls.Add(control, 1, row);
    }

    private static void AddFullWidthControl(TableLayoutPanel root, int row, Control control)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(control, 0, row);
        root.SetColumnSpan(control, 2);
    }

    private static void ConfigureCheckBox(CheckBox checkBox, string text)
    {
        checkBox.AutoSize = true;
        checkBox.MaximumSize = new Size(620, 0);
        checkBox.Text = text;
        checkBox.Margin = new Padding(0, 4, 0, 10);
    }

    private static Button CreateDialogButton(string text, DialogResult dialogResult)
    {
        return new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(96, 32),
            Padding = new Padding(10, 3, 10, 3),
            Text = text,
            DialogResult = dialogResult,
            Margin = new Padding(8, 0, 0, 0),
            UseVisualStyleBackColor = true
        };
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
