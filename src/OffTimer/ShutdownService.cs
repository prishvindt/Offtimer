using System.Diagnostics;

namespace OffTimer;

internal sealed class ShutdownService : IDisposable
{
    private readonly string _shutdownExePath;

    public ShutdownService()
    {
        _shutdownExePath = Path.Combine(Environment.SystemDirectory, "shutdown.exe");
    }

    public void ScheduleShutdown(int seconds)
    {
        RunShutdownCommand($"/s /t {seconds}");
    }

    public void CancelShutdown()
    {
        RunShutdownCommand("/a");
    }

    private void RunShutdownCommand(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _shutdownExePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("shutdown.exe did not start.");
        process.WaitForExit(5000);

        if (!process.HasExited)
        {
            try
            {
                process.Kill();
            }
            catch
            {
                // ignored
            }

            throw new InvalidOperationException("shutdown.exe timed out.");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"shutdown.exe exit code {process.ExitCode}.");
        }
    }

    public void Dispose()
    {
    }
}
