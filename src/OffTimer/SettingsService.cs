using System.Text.Json;

namespace OffTimer;

internal sealed class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppPaths.GetDataDirectory(), AppPaths.SettingsFileName);
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaultSettings = new AppSettings();
                Save(defaultSettings);
                return defaultSettings;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            Normalize(settings);
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Normalize(settings);
        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public string SettingsPath => _settingsPath;

    private static void Normalize(AppSettings settings)
    {
        if (settings.Language is not ("auto" or "ru" or "en"))
        {
            settings.Language = "auto";
        }

        if (settings.OverlayFontSize < 12)
        {
            settings.OverlayFontSize = 12;
        }
        else if (settings.OverlayFontSize > 220)
        {
            settings.OverlayFontSize = 220;
        }

        if (settings.OverlayCorner is not ("TopLeft" or "TopRight" or "BottomLeft" or "BottomRight"))
        {
            settings.OverlayCorner = "TopRight";
        }
    }
}
