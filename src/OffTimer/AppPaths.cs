namespace OffTimer;

internal static class AppPaths
{
    public const string DataFolderName = "offtimer";
    public const string SettingsFileName = "settings.json";

    public static string GetDataDirectory()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var portableDirectory = Path.Combine(baseDirectory, DataFolderName);

        if (CanWriteToDirectory(baseDirectory))
        {
            Directory.CreateDirectory(portableDirectory);
            return portableDirectory;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var fallbackDirectory = Path.Combine(appData, "OffTimer");
        Directory.CreateDirectory(fallbackDirectory);
        return fallbackDirectory;
    }

    private static bool CanWriteToDirectory(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var testFile = Path.Combine(directory, $".offtimer_write_test_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
