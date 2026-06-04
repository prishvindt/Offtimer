using System.Threading;

namespace OffTimer;

internal sealed class SingleInstance : IDisposable
{
    private const string MutexName = "Global\\OffTimer_2D7E7E62_9C9D_41D7_B346_A1FA2EF46D3D";
    private const string ShowMessageName = "OffTimer_ShowMainWindow_2D7E7E62_9C9D_41D7_B346_A1FA2EF46D3D";
    private readonly Mutex _mutex = new(false, MutexName);
    private bool _acquired;

    public static int ShowMessageId { get; } = NativeMethods.RegisterWindowMessage(ShowMessageName);

    public bool TryAcquire()
    {
        try
        {
            _acquired = _mutex.WaitOne(0, false);
            return _acquired;
        }
        catch (AbandonedMutexException)
        {
            _acquired = true;
            return true;
        }
    }

    public static void NotifyExistingInstance()
    {
        if (ShowMessageId != 0)
        {
            NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, ShowMessageId, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public void Dispose()
    {
        if (_acquired)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch
            {
                // ignored
            }
        }

        _mutex.Dispose();
    }
}
