using System.Drawing.Drawing2D;

namespace OffTimer;

internal sealed class OverlayForm : Form
{
    private readonly StringFormat _format = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };

    private AppSettings _settings;
    private string _value = "00";
    private Font _font;

    public OverlayForm(AppSettings settings)
    {
        _settings = settings;
        _font = CreateOverlayFont(settings.OverlayFontSize);

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        TopMost = true;
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        if ((int)_font.Size != settings.OverlayFontSize)
        {
            _font.Dispose();
            _font = CreateOverlayFont(settings.OverlayFontSize);
        }
    }

    public void UpdateCountdown(string value)
    {
        _value = value;
        UpdateBounds();
        Invalidate();
    }

    public void ShowOverlay()
    {
        UpdateBounds();
        if (!Visible)
        {
            Show();
        }

        TopMost = true;
    }

    public void HideOverlay()
    {
        Hide();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var shadowBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
        using var textBrush = new SolidBrush(_settings.OverlayColor);
        var shadowRect = ClientRectangle;
        shadowRect.Offset(2, 2);
        e.Graphics.DrawString(_value, _font, shadowBrush, shadowRect, _format);
        e.Graphics.DrawString(_value, _font, textBrush, ClientRectangle, _format);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _font.Dispose();
            _format.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateBounds()
    {
        var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        using var graphics = CreateGraphics();
        var measured = graphics.MeasureString(_value, _font);
        var width = Math.Max(60, (int)Math.Ceiling(measured.Width) + 20);
        var height = Math.Max(40, (int)Math.Ceiling(measured.Height) + 12);
        Size = new Size(width, height);

        const int margin = 16;
        var x = _settings.OverlayCorner is "TopLeft" or "BottomLeft"
            ? screen.Left + margin
            : screen.Right - width - margin;
        var y = _settings.OverlayCorner is "TopLeft" or "TopRight"
            ? screen.Top + margin
            : screen.Bottom - height - margin;

        Location = new Point(x, y);
    }

    private static Font CreateOverlayFont(int size)
    {
        return new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
    }
}
