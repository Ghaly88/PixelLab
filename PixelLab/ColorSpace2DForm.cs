using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PixelLab
{
    public class ColorSpace2DForm : Form
    {
        // ── View state ────────────────────────────────────────────────────
        private float _panX = 0f, _panY = 0f, _scale = 1f;
        private Point _lastMouse;
        private bool  _isPanning;

        // Cached rendered bitmap for current view (invalidated on resize/view change)
        private Bitmap _viewBitmap;
        private int    _lastViewIdx = -1;

        // ── Controls ──────────────────────────────────────────────────────
        private DoubleBufferedPanel _canvas;
        private ComboBox _cmb;
        private Panel    _swatchPanel;
        private Label    _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB, _lblCMY;
        private Label    _lblCoords;

        public ColorSpace2DForm()
        {
            BuildUI();
        }

        // ── UI ────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            Text        = "2D Color Space Viewer";
            Size        = new Size(860, 720);
            MinimumSize = new Size(600, 500);
            BackColor   = Color.FromArgb(30, 30, 30);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(42, 42, 42) };
            var lblV = new Label { Text = "View:", ForeColor = Color.White, Location = new Point(10, 12), AutoSize = true };

            _cmb = new ComboBox
            {
                Location = new Point(55, 8), Width = 210,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White
            };
            _cmb.Items.AddRange(new object[]
            {
                "HSV — Color Wheel",
                "YCbCr — Cb vs Cr",
                "YUV — U vs V",
                "LAB — a* vs b*",
                "RGB — R vs G"
            });
            _cmb.SelectedIndex = 0;
            _cmb.SelectedIndexChanged += (s, e) => { _viewBitmap = null; _canvas.Invalidate(); };

            var btnReset = new Button
            {
                Text = "Reset View", Location = new Point(280, 8), Size = new Size(90, 24),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(65, 65, 65)
            };
            btnReset.FlatAppearance.BorderColor = Color.Gray;
            btnReset.Click += (s, e) => { _panX = 0; _panY = 0; _scale = 1f; _viewBitmap = null; _canvas.Invalidate(); };

            top.Controls.Add(lblV);
            top.Controls.Add(_cmb);
            top.Controls.Add(btnReset);

            // Right info panel
            Panel right = new Panel { Dock = DockStyle.Right, Width = 200, BackColor = Color.FromArgb(42, 42, 42) };
            var hint = MkLabel("Drag → pan\nScroll → zoom\nClick → inspect color", 8, 10, 184, 54);
            hint.ForeColor = Color.Silver;
            var sep = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(8, 72), Size = new Size(184, 2) };
            var selTitle = MkLabel("Selected Color", 8, 80, 184, 18);
            selTitle.ForeColor = Color.White;
            selTitle.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

            _swatchPanel = new Panel
            {
                Location = new Point(8, 104), Size = new Size(184, 28),
                BackColor = Color.FromArgb(60, 60, 60), BorderStyle = BorderStyle.FixedSingle
            };

            _lblCoords = MkLabel("Click to inspect", 8, 136, 184, 18);
            _lblCoords.ForeColor = Color.Silver;
            _lblRGB   = MkLabel("RGB:    -", 8, 158, 184, 18);
            _lblHSV   = MkLabel("HSV:    -", 8, 178, 184, 18);
            _lblYCbCr = MkLabel("YCbCr: -", 8, 198, 184, 18);
            _lblYUV   = MkLabel("YUV:    -", 8, 218, 184, 18);
            _lblLAB   = MkLabel("LAB:    -", 8, 238, 184, 18);
            _lblCMY   = MkLabel("CMY:    -", 8, 258, 184, 18);
            foreach (var l in new[] { _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB, _lblCMY })
                l.ForeColor = Color.LightGray;

            right.Controls.Add(hint);
            right.Controls.Add(sep);
            right.Controls.Add(selTitle);
            right.Controls.Add(_swatchPanel);
            right.Controls.Add(_lblCoords);
            right.Controls.Add(_lblRGB);
            right.Controls.Add(_lblHSV);
            right.Controls.Add(_lblYCbCr);
            right.Controls.Add(_lblYUV);
            right.Controls.Add(_lblLAB);
            right.Controls.Add(_lblCMY);

            _canvas = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 18) };
            _canvas.Paint      += OnPaint;
            _canvas.Resize     += (s, e) => { _viewBitmap = null; _canvas.Invalidate(); };
            _canvas.MouseDown  += (s, e) => { _isPanning = true; _lastMouse = e.Location; };
            _canvas.MouseUp    += (s, e) => _isPanning = false;
            _canvas.MouseMove  += OnMouseMove;
            _canvas.MouseWheel += OnMouseWheel;
            _canvas.MouseClick += OnCanvasClick;

            Controls.Add(_canvas);
            Controls.Add(right);
            Controls.Add(top);
        }

        private Label MkLabel(string text, int x, int y, int w, int h)
            => new Label { Text = text, Location = new Point(x, y), Size = new Size(w, h), ForeColor = Color.LightGray, AutoSize = false };

        // ── Mouse handlers ────────────────────────────────────────────────

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning) return;
            _panX += e.X - _lastMouse.X;
            _panY += e.Y - _lastMouse.Y;
            _lastMouse = e.Location;
            _canvas.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float oldScale = _scale;
            _scale = Math.Max(0.5f, Math.Min(8f, _scale * (e.Delta > 0 ? 1.15f : 1f / 1.15f)));
            // Zoom toward cursor
            _panX = e.X + (_panX - e.X) * (_scale / oldScale);
            _panY = e.Y + (_panY - e.Y) * (_scale / oldScale);
            _canvas.Invalidate();
        }

        private void OnCanvasClick(object sender, MouseEventArgs e)
        {
            // Map canvas pixel → normalised [0,1] coordinates in the plot area
            GetPlotRect(out int ml, out int mt, out int pw, out int ph);
            float nx = ((e.X - ml - _panX) / (_scale * pw));
            float ny = 1f - ((e.Y - mt - _panY) / (_scale * ph));
            nx = Math.Max(0f, Math.Min(1f, nx));
            ny = Math.Max(0f, Math.Min(1f, ny));
            Color c = NormToColor(nx, ny, _cmb.SelectedIndex);
            if (c != Color.Empty)
                ShowColorInfo(c, nx, ny);
        }

        // ── Paint ─────────────────────────────────────────────────────────

        private void OnPaint(object sender, PaintEventArgs e)
        {
            GetPlotRect(out int ml, out int mt, out int pw, out int ph);
            if (pw < 4 || ph < 4) return;

            // Regenerate the bitmap if stale
            if (_viewBitmap == null || _lastViewIdx != _cmb.SelectedIndex)
            {
                _viewBitmap?.Dispose();
                _viewBitmap  = RenderView(pw, ph, _cmb.SelectedIndex);
                _lastViewIdx = _cmb.SelectedIndex;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // Apply pan/zoom transform around the plot origin
            g.TranslateTransform(ml + _panX, mt + _panY);
            g.ScaleTransform(_scale, _scale);
            g.DrawImage(_viewBitmap, 0, 0, pw, ph);
            g.ResetTransform();

            // Border and axis labels (drawn in canvas coords, no transform)
            DrawAxes(g, ml, mt, pw, ph);
        }

        private Bitmap RenderView(int pw, int ph, int viewIdx)
        {
            Bitmap bmp = new Bitmap(pw, ph);
            for (int py = 0; py < ph; py++)
            {
                float ny = 1f - py / (float)(ph - 1);
                for (int px = 0; px < pw; px++)
                {
                    float nx = px / (float)(pw - 1);
                    Color c = NormToColor(nx, ny, viewIdx);
                    if (c != Color.Empty)
                        bmp.SetPixel(px, py, c);
                }
            }
            return bmp;
        }

        // Map normalised (nx,ny) in [0,1]² to a display color for the given view.
        private Color NormToColor(float nx, float ny, int viewIdx)
        {
            switch (viewIdx)
            {
                case 0: // HSV wheel: nx,ny are in [0,1], center is (0.5,0.5)
                {
                    float dx = nx - 0.5f, dy = ny - 0.5f;
                    float s = (float)Math.Sqrt(dx * dx + dy * dy) * 2f; // [0,1] at rim
                    if (s > 1f) return Color.Empty;
                    float hDeg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (hDeg < 0) hDeg += 360f;
                    return HsvToRgb(hDeg, s, 1f);
                }
                case 1: // YCbCr — x=Cb [0,255], y=Cr [0,255], Y fixed at 128
                {
                    byte Cb = (byte)(nx * 255);
                    byte Cr = (byte)(ny * 255);
                    return YcbcrToRgb(128, Cr, Cb);
                }
                case 2: // YUV — x=U [0,255], y=V [0,255], Y fixed at 128
                {
                    byte U = (byte)(nx * 255);
                    byte V = (byte)(ny * 255);
                    return YuvToRgb(128, U, V);
                }
                case 3: // LAB — x=a* [0,255], y=b* [0,255], L fixed at 128
                {
                    byte aS = (byte)(nx * 255);
                    byte bS = (byte)(ny * 255);
                    return LabToRgb(128, aS, bS);
                }
                case 4: // RGB — x=R, y=G, B fixed at 128
                {
                    byte R = (byte)(nx * 255);
                    byte G = (byte)(ny * 255);
                    return Color.FromArgb(R, G, 128);
                }
                default:
                    return Color.Empty;
            }
        }

        private void DrawAxes(Graphics g, int ml, int mt, int pw, int ph)
        {
            using (Pen border = new Pen(Color.FromArgb(85, 85, 85), 1))
                g.DrawRectangle(border, ml, mt, pw, ph);

            string[] xLabels, yLabels, title;
            switch (_cmb.SelectedIndex)
            {
                case 0: xLabels = new[]{"",""}; yLabels = new[]{"",""}; title = new[]{"HSV — Color Wheel  (radius = Saturation, angle = Hue)"}; break;
                case 1: xLabels = new[]{"Cb=0","Cb=255"}; yLabels = new[]{"Cr=0","Cr=255"}; title = new[]{"YCbCr — Cb vs Cr  (Y=128)"}; break;
                case 2: xLabels = new[]{"U=0", "U=255"}; yLabels = new[]{"V=0", "V=255"}; title = new[]{"YUV — U vs V  (Y=128)"}; break;
                case 3: xLabels = new[]{"a*=0","a*=255"}; yLabels = new[]{"b*=0","b*=255"}; title = new[]{"LAB — a* vs b*  (L=128)"}; break;
                default: xLabels = new[]{"R=0", "R=255"}; yLabels = new[]{"G=0", "G=255"}; title = new[]{"RGB — R vs G  (B=128)"}; break;
            }

            using (Font f = new Font("Arial", 9f))
            using (SolidBrush br = new SolidBrush(Color.Silver))
            {
                g.DrawString(xLabels[0], f, br, ml, mt + ph + 4);
                SizeF rSz = g.MeasureString(xLabels[1], f);
                g.DrawString(xLabels[1], f, br, ml + pw - rSz.Width, mt + ph + 4);
                g.DrawString(yLabels[0], f, br, ml - 38, mt + ph - 12);
                g.DrawString(yLabels[1], f, br, ml - 38, mt);
            }
            using (Font f = new Font("Arial", 10f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(title[0], f, br, ml, mt - 22);
        }

        // Determines the plot area rectangle in canvas coordinates.
        private void GetPlotRect(out int ml, out int mt, out int pw, out int ph)
        {
            ml = 50; mt = 30;
            pw = Math.Max(4, _canvas.Width  - ml - 15);
            ph = Math.Max(4, _canvas.Height - mt - 50);
        }

        // ── Color space back-conversions via EmguCV ───────────────────────

        private static Color YcbcrToRgb(byte y, byte cr, byte cb)
            => ConvertSingle(y, cr, cb, ColorConversion.YCrCb2Bgr);

        private static Color YuvToRgb(byte y, byte u, byte v)
            => ConvertSingle(y, u, v, ColorConversion.Yuv2Bgr);

        private static Color LabToRgb(byte l, byte a, byte b)
            => ConvertSingle(l, a, b, ColorConversion.Lab2Bgr);

        private static Color ConvertSingle(byte ch0, byte ch1, byte ch2, ColorConversion conv)
        {
            try
            {
                byte[] buf = { ch0, ch1, ch2 };
                Mat src = new Mat(1, 1, DepthType.Cv8U, 3);
                Marshal.Copy(buf, 0, src.DataPointer, 3);
                Mat dst = new Mat();
                CvInvoke.CvtColor(src, dst, conv);
                byte[] res = new byte[3];
                Marshal.Copy(dst.DataPointer, res, 0, 3);
                src.Dispose(); dst.Dispose();
                return Color.FromArgb(res[2], res[1], res[0]); // BGR→RGB
            }
            catch { return Color.FromArgb(128, 128, 128); }
        }

        // ── Pixel inspector ───────────────────────────────────────────────

        private void ShowColorInfo(Color c, float nx, float ny)
        {
            _swatchPanel.BackColor = c;

            string coordHint;
            switch (_cmb.SelectedIndex)
            {
                case 0: coordHint = $"H={nx*360f:F0}°  S={ny*100f:F0}%"; break;
                case 1: coordHint = $"Cb={nx*255f:F0}  Cr={ny*255f:F0}"; break;
                case 2: coordHint = $"U={nx*255f:F0}  V={ny*255f:F0}"; break;
                case 3: coordHint = $"a*={nx*255f:F0}  b*={ny*255f:F0}"; break;
                default: coordHint = $"R={nx*255f:F0}  G={ny*255f:F0}"; break;
            }
            _lblCoords.Text = coordHint;
            _lblRGB.Text    = $"RGB:    ({c.R}, {c.G}, {c.B})";
            _lblCMY.Text    = $"CMY:    ({255-c.R}, {255-c.G}, {255-c.B})";

            try
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(1, 1);
                img[0, 0] = new Bgr(c.B, c.G, c.R);
                Mat m = new Mat();
                byte[] buf = new byte[3];

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Hsv);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                _lblHSV.Text = $"HSV:    ({buf[0]*2}°, {(int)(buf[1]/255.0*100)}%, {(int)(buf[2]/255.0*100)}%)";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2YCrCb);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                _lblYCbCr.Text = $"YCbCr: ({buf[0]}, {buf[1]}, {buf[2]})";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Yuv);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                _lblYUV.Text = $"YUV:    ({buf[0]}, {buf[1]}, {buf[2]})";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Lab);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                _lblLAB.Text = $"LAB:    ({buf[0]}, {buf[1]}, {buf[2]})";

                img.Dispose(); m.Dispose();
            }
            catch { }
        }

        // ── HSV→RGB utility ───────────────────────────────────────────────

        private static Color HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float x = c * (1f - Math.Abs(h / 60f % 2f - 1f));
            float m = v - c;
            float r, gr, b;
            if      (h < 60)  { r = c;  gr = x;  b = 0; }
            else if (h < 120) { r = x;  gr = c;  b = 0; }
            else if (h < 180) { r = 0;  gr = c;  b = x; }
            else if (h < 240) { r = 0;  gr = x;  b = c; }
            else if (h < 300) { r = x;  gr = 0;  b = c; }
            else              { r = c;  gr = 0;  b = x; }
            return Color.FromArgb(
                Clamp((int)((r  + m) * 255)),
                Clamp((int)((gr + m) * 255)),
                Clamp((int)((b  + m) * 255)));
        }

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _viewBitmap?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
