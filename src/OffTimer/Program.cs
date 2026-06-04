using System.Threading;

namespace OffTimer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var singleInstance = new SingleInstance();
        if (!singleInstance.TryAcquire())
        {
            SingleInstance.NotifyExistingInstance();
            return;
        }

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var settingsService = new SettingsService();
        var settings = settingsService.Load();
        var localizer = new Localizer(settings);
        var overlay = new OverlayForm(settings);
        using var shutdownService = new ShutdownService();
        var controller = new CountdownController(settings, overlay, shutdownService);

        Application.Run(new MainForm(settingsService, settings, localizer, controller, overlay));
    }
}
