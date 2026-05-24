using System;
using System.Collections.Generic;
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
        private readonly List<Color> _pixels = new List<Color>();
  
        private readonly List<(float x, float y, Color c)>[] _views =
            new List<(float, float, Color)>[5];

        private DoubleBufferedPanel _canvas;
        private ComboBox           _cmb;

        public ColorSpace2DForm(Bitmap source)
        {
            SamplePixels(source);
            ComputeViews();
            BuildUI();
        }


        private void SamplePixels(Bitmap bmp)
        {
            int step = Math.Max(1, (int)Math.Sqrt(bmp.Width * bmp.Height / 6000.0));
            for (int y = 0; y < bmp.Height; y += step)
                for (int x = 0; x < bmp.Width; x += step)
                    _pixels.Add(bmp.GetPixel(x, y));
        }

        private void ComputeViews()
        {
            int n = _pixels.Count;
            if (n == 0) return;

            Image<Bgr, byte> img = new Image<Bgr, byte>(n, 1);
            for (int i = 0; i < n; i++)
            {
                Color p = _pixels[i];
                img[0, i] = new Bgr(p.B, p.G, p.R);
            }

            // HSV:   
            _views[0] = Batch(img, n, ColorConversion.Bgr2Hsv,   0, 1, 1f/180f, 1f/255f);
            // YCbCr: 
            _views[1] = Batch(img, n, ColorConversion.Bgr2YCrCb, 2, 1, 1f/255f, 1f/255f);
            // YUV:   
            _views[2] = Batch(img, n, ColorConversion.Bgr2Yuv,   1, 2, 1f/255f, 1f/255f);
            // LAB:  
            _views[3] = Batch(img, n, ColorConversion.Bgr2Lab,   1, 2, 1f/255f, 1f/255f);
            // RGB:   
            _views[4] = new List<(float, float, Color)>(n);
            for (int i = 0; i < n; i++)
                _views[4].Add((_pixels[i].R / 255f, _pixels[i].G / 255f, _pixels[i]));

            img.Dispose();
        }

        private List<(float x, float y, Color c)> Batch(
            Image<Bgr, byte> src, int n, ColorConversion conv,
            int chX, int chY, float sx, float sy)
        {
            Mat dst = new Mat();
            CvInvoke.CvtColor(src.Mat, dst, conv);

            byte[] row = new byte[(int)dst.Step];
            Marshal.Copy(dst.DataPointer, row, 0, row.Length);
            dst.Dispose();

            var list = new List<(float, float, Color)>(n);
            for (int i = 0; i < n; i++)
                list.Add((row[i * 3 + chX] * sx, row[i * 3 + chY] * sy, _pixels[i]));
            return list;
        }


        private void BuildUI()
        {
            Text        = "2D Color Space Viewer";
            Size        = new Size(680, 720);
            MinimumSize = new Size(480, 480);
            BackColor   = Color.FromArgb(30, 30, 30);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(42, 42, 42) };
            Label lbl = new Label  { Text = "View:", ForeColor = Color.White, Location = new Point(10, 12), AutoSize = true };

            _cmb = new ComboBox
            {
                Location      = new Point(55, 8),
                Width         = 210,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.FromArgb(60, 60, 60),
                ForeColor     = Color.White
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
            _cmb.SelectedIndexChanged += (s, e) => _canvas.Invalidate();

            top.Controls.Add(lbl);
            top.Controls.Add(_cmb);

            _canvas = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 18) };
            _canvas.Paint  += OnPaint;
            _canvas.Resize += (s, e) => _canvas.Invalidate();

            Controls.Add(_canvas);
            Controls.Add(top);
        }


        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            switch (_cmb.SelectedIndex)
            {
                case 0: DrawWheel(e.Graphics);                            break;
                case 1: DrawScatter(e.Graphics, "Cb", "Cr", _views[1]);  break;
                case 2: DrawScatter(e.Graphics, "U",  "V",  _views[2]);  break;
                case 3: DrawScatter(e.Graphics, "a*", "b*", _views[3]);  break;
                case 4: DrawScatter(e.Graphics, "R",  "G",  _views[4]);  break;
            }
        }

        // ── HSV Color Wheel ───────────────────────────────────────────────

        private void DrawWheel(Graphics g)
        {
            const int margin = 50;
            float cx = _canvas.Width  / 2f;
            float cy = _canvas.Height / 2f;
            float maxR  = Math.Min(cx, cy) - margin;
            if (maxR < 10) return;

            // Outer hue ring: 360 clockwise wedges, each 1.5° wide so no gaps
            for (int h = 0; h < 360; h++)
                using (SolidBrush br = new SolidBrush(HsvToRgb(h, 1f, 0.92f)))
                    g.FillPie(br, cx - maxR, cy - maxR, maxR * 2, maxR * 2, h, 1.5f);

            // Dark inner circle — gives the ring effect
            float innerR = maxR * 0.88f;
            using (SolidBrush br = new SolidBrush(Color.FromArgb(18, 18, 18)))
                g.FillEllipse(br, cx - innerR, cy - innerR, innerR * 2, innerR * 2);

            // Saturation guide circles at 25 / 50 / 75 / 100 %
            using (Pen pen = new Pen(Color.FromArgb(55, 55, 55), 1))
                foreach (float f in new[] { 0.25f, 0.5f, 0.75f, 1f })
                {
                    float r = innerR * f;
                    g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);
                }

            // Hue labels around the ring
            var labels = new (int deg, string txt, Color col)[]
            {
                (0,   "R", Color.OrangeRed),
                (60,  "Y", Color.Yellow),
                (120, "G", Color.LimeGreen),
                (180, "C", Color.Cyan),
                (240, "B", Color.CornflowerBlue),
                (300, "M", Color.Violet),
            };
            using (Font f = new Font("Arial", 9f, FontStyle.Bold))
                foreach (var (deg, txt, col) in labels)
                {
                    double rad = deg * Math.PI / 180.0;
                    float lx = cx + (maxR + 18) * (float)Math.Cos(rad) - 6;
                    float ly = cy + (maxR + 18) * (float)Math.Sin(rad) - 8;
                    using (SolidBrush br = new SolidBrush(col))
                        g.DrawString(txt, f, br, lx, ly);
                }

            // Pixel dots — _views[0].x = H/180 (0→1 = full circle), .y = S/255 (0→1)
            if (_views[0] != null)
                foreach (var (hNorm, s, c) in _views[0])
                {
                    double angle = hNorm * 2 * Math.PI;          // clockwise (Y-down screen)
                    float px = cx + s * innerR * (float)Math.Cos(angle);
                    float py = cy + s * innerR * (float)Math.Sin(angle);
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(200, c)))
                        g.FillEllipse(br, px - 2f, py - 2f, 4f, 4f);
                }

            DrawTitle(g, "HSV — Color Wheel  (radius = Saturation,  angle = Hue)");
        }

        // ── Scatter plot ──────────────────────────────────────────────────

        private void DrawScatter(Graphics g, string xLbl, string yLbl,
                                  List<(float x, float y, Color c)> data)
        {
            const int ml = 55, mr = 15, mt = 35, mb = 42;
            float pw = _canvas.Width  - ml - mr;
            float ph = _canvas.Height - mt - mb;
            if (pw < 10 || ph < 10) return;

            // Background
            using (SolidBrush br = new SolidBrush(Color.FromArgb(26, 26, 26)))
                g.FillRectangle(br, ml, mt, pw, ph);

            // Grid at 0.25 intervals
            using (Pen pen = new Pen(Color.FromArgb(42, 42, 42), 1))
                for (float t = 0.25f; t < 1f; t += 0.25f)
                {
                    g.DrawLine(pen, ml + t * pw, mt, ml + t * pw, mt + ph);
                    g.DrawLine(pen, ml, mt + (1 - t) * ph, ml + pw, mt + (1 - t) * ph);
                }

            // Neutral crosshair at 128/255 ≈ 0.502
            float nx = ml + 0.502f * pw, ny = mt + (1 - 0.502f) * ph;
            using (Pen pen = new Pen(Color.FromArgb(78, 78, 78), 1))
            {
                g.DrawLine(pen, nx, mt, nx, mt + ph);
                g.DrawLine(pen, ml, ny, ml + pw, ny);
            }

            // Border
            using (Pen pen = new Pen(Color.FromArgb(85, 85, 85), 1))
                g.DrawRectangle(pen, ml, mt, pw, ph);

            // Axis tick labels
            using (Font f = new Font("Arial", 8f))
            using (SolidBrush br = new SolidBrush(Color.Silver))
                foreach (int v in new[] { 0, 64, 128, 192, 255 })
                {
                    float gx = ml + v / 255f * pw;
                    float gy = mt + (1 - v / 255f) * ph;
                    g.DrawString(v.ToString(), f, br, gx - 8, mt + ph + 5);
                    g.DrawString(v.ToString(), f, br, ml - 32, gy - 7);
                }

            // Axis labels
            using (Font f = new Font("Arial", 10f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
            {
                SizeF xSz = g.MeasureString(xLbl, f);
                g.DrawString(xLbl, f, br, ml + (pw - xSz.Width) / 2, mt + ph + 22);

                g.TranslateTransform(ml - 44, mt + ph / 2f);
                g.RotateTransform(-90);
                SizeF ySz = g.MeasureString(yLbl, f);
                g.DrawString(yLbl, f, br, -ySz.Width / 2, 0);
                g.ResetTransform();
            }

            // Pixel dots
            if (data != null)
                foreach (var (x, y, c) in data)
                {
                    float px = ml + x * pw;
                    float py = mt + (1 - y) * ph;
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(200, c)))
                        g.FillEllipse(br, px - 2f, py - 2f, 4f, 4f);
                }

            DrawTitle(g, $"{yLbl} vs {xLbl}");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void DrawTitle(Graphics g, string title)
        {
            using (Font f = new Font("Arial", 11f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(title, f, br, 10, 8);
        }

        // Pure software HSV→RGB — only used for drawing the wheel background
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
    }
}
