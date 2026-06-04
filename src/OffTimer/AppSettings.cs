using System.Drawing;

namespace OffTimer;

internal sealed class AppSettings
{
    public string Language { get; set; } = "auto";
    public bool OverlayEnabled { get; set; } = true;
    public int OverlayFontSize { get; set; } = 32;
    public int OverlayColorArgb { get; set; } = Color.White.ToArgb();
    public string OverlayCorner { get; set; } = "TopRight";
    public bool MinimizeToTrayOnTimerStart { get; set; } = false;

    public Color OverlayColor
    {
        get => Color.FromArgb(OverlayColorArgb);
        set => OverlayColorArgb = value.ToArgb();
    }
}
