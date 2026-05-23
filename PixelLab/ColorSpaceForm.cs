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
    public class ColorSpaceForm : Form
    {
        // ── View state ────────────────────────────────────────────────────
        private float _angleX = 25f;
        private float _angleY = -40f;
        private float _zoom   = 230f;
        private Point _lastMouse;
        private bool  _isDragging;

        // ── Sampled pixel colors ──────────────────────────────────────────
        private readonly List<Color> _points = new List<Color>();

        // ── Controls ──────────────────────────────────────────────────────
        private Panel  _canvas;   // double-buffered panel — draws via Paint
        private Panel  _swatchPanel;
        private Label  _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB;

        // ── RGB unit-cube geometry ─────────────────────────────────────────
        private static readonly float[,] CubeVerts =
        {
            {0,0,0},{1,0,0},{1,1,0},{0,1,0},
            {0,0,1},{1,0,1},{1,1,1},{0,1,1}
        };
        private static readonly int[,] CubeEdges =
        {
            {0,1},{1,2},{2,3},{3,0},
            {4,5},{5,6},{6,7},{7,4},
            {0,4},{1,5},{2,6},{3,7}
        };

        // ── Constructor ───────────────────────────────────────────────────
        public ColorSpaceForm(Bitmap source)
        {
            SampleColors(source);
            BuildUI();
        }

        private void SampleColors(Bitmap bmp)
        {
            int total = bmp.Width * bmp.Height;
            int step  = Math.Max(1, (int)Math.Sqrt(total / 6000.0));
            for (int y = 0; y < bmp.Height; y += step)
                for (int x = 0; x < bmp.Width; x += step)
                    _points.Add(bmp.GetPixel(x, y));
        }

        // ── UI ────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text        = "3D Color Space — RGB Cube";
            this.Size        = new Size(970, 700);
            this.MinimumSize = new Size(800, 580);
            this.BackColor   = Color.FromArgb(30, 30, 30);

            // Right info panel (must be added first so Fill canvas respects it)
            Panel right = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 220,
                BackColor = Color.FromArgb(42, 42, 42),
                Padding   = new Padding(8)
            };

            Label hint = MkLabel("Drag   →  rotate\nScroll →  zoom\nClick dot  →  inspect",
                                  8, 10, 204, 54);
            hint.ForeColor = Color.Silver;

            Label sep = new Label { BorderStyle = BorderStyle.Fixed3D,
                                    Location = new Point(8, 72), Size = new Size(204, 2) };

            Label selTitle = MkLabel("Selected Color", 8, 80, 204, 18);
            selTitle.ForeColor = Color.White;
            selTitle.Font      = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

            _swatchPanel = new Panel
            {
                Location    = new Point(8, 104),
                Size        = new Size(204, 28),
                BackColor   = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblRGB   = MkLabel("RGB:    -", 8, 140, 204, 18);
            _lblHSV   = MkLabel("HSV:    -", 8, 160, 204, 18);
            _lblYCbCr = MkLabel("YCbCr: -", 8, 180, 204, 18);
            _lblYUV   = MkLabel("YUV:    -", 8, 200, 204, 18);
            _lblLAB   = MkLabel("LAB:    -", 8, 220, 204, 18);
            foreach (Label l in new[] { _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB })
                l.ForeColor = Color.LightGray;

            Button btnReset = new Button
            {
                Text      = "Reset View",
                Location  = new Point(8, 254),
                Size      = new Size(204, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(65, 65, 65)
            };
            btnReset.FlatAppearance.BorderColor = Color.Gray;
            btnReset.Click += (s, e) => { _angleX = 25f; _angleY = -40f; _zoom = 230f; _canvas.Invalidate(); };

            right.Controls.Add(hint);
            right.Controls.Add(sep);
            right.Controls.Add(selTitle);
            right.Controls.Add(_swatchPanel);
            right.Controls.Add(_lblRGB);
            right.Controls.Add(_lblHSV);
            right.Controls.Add(_lblYCbCr);
            right.Controls.Add(_lblYUV);
            right.Controls.Add(_lblLAB);
            right.Controls.Add(btnReset);

            // Double-buffered canvas — fill remaining space
            _canvas = new DoubleBufferedPanel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                Cursor    = Cursors.SizeAll
            };
            _canvas.Paint      += OnPaint;
            _canvas.MouseDown  += (s, e) => { _isDragging = true; _lastMouse = e.Location; };
            _canvas.MouseUp    += (s, e) => _isDragging = false;
            _canvas.MouseMove  += OnMouseMove;
            _canvas.MouseWheel += OnMouseWheel;
            _canvas.MouseClick += OnCanvasClick;

            // Right panel first → docked to right before Fill is applied
            this.Controls.Add(right);
            this.Controls.Add(_canvas);
        }

        private Label MkLabel(string text, int x, int y, int w, int h)
            => new Label { Text = text, Location = new Point(x, y),
                           Size = new Size(w, h), ForeColor = Color.LightGray, AutoSize = false };

        // ── Mouse handlers ────────────────────────────────────────────────

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            _angleY += (e.X - _lastMouse.X) * 0.5f;
            _angleX += (e.Y - _lastMouse.Y) * 0.5f;
            _lastMouse = e.Location;
            _canvas.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            _zoom = Math.Max(80f, Math.Min(700f, _zoom + e.Delta * 0.15f));
            _canvas.Invalidate();
        }

        private void OnCanvasClick(object sender, MouseEventArgs e)
        {
            float bestDist  = float.MaxValue;
            Color bestColor = Color.Empty;

            foreach (Color c in _points)
            {
                PointF p  = Project(c.R / 255f, c.G / 255f, c.B / 255f);
                float  dx = p.X - e.X, dy = p.Y - e.Y;
                float  d  = dx * dx + dy * dy;
                if (d < bestDist) { bestDist = d; bestColor = c; }
            }

            if (bestDist < 400f)
                ShowColor(bestColor);
        }

        // ── 3D Projection ─────────────────────────────────────────────────
        private PointF Project(float r, float g, float b)
        {
            float x = r - 0.5f, y = g - 0.5f, z = b - 0.5f;

            float ay = _angleY * (float)(Math.PI / 180.0);
            float x1 =  x * (float)Math.Cos(ay) + z * (float)Math.Sin(ay);
            float z1 = -x * (float)Math.Sin(ay) + z * (float)Math.Cos(ay);

            float ax = _angleX * (float)(Math.PI / 180.0);
            float y2 = y * (float)Math.Cos(ax) - z1 * (float)Math.Sin(ax);

            return new PointF(
                _canvas.Width  / 2f + x1 * _zoom,
                _canvas.Height / 2f - y2 * _zoom
            );
        }

        // ── Paint ─────────────────────────────────────────────────────────
        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawWireframe(g);
            DrawAxisLabels(g);
            DrawColorPoints(g);
        }

        private void DrawWireframe(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(110, 110, 110), 1f))
            {
                for (int i = 0; i < CubeEdges.GetLength(0); i++)
                {
                    int a = CubeEdges[i, 0], b = CubeEdges[i, 1];
                    PointF p0 = Project(CubeVerts[a, 0], CubeVerts[a, 1], CubeVerts[a, 2]);
                    PointF p1 = Project(CubeVerts[b, 0], CubeVerts[b, 1], CubeVerts[b, 2]);
                    g.DrawLine(pen, p0, p1);
                }
            }
        }

        private void DrawAxisLabels(Graphics g)
        {
            var corners = new (float r, float gr, float b, string text, Color col)[]
            {
                (1f, 0f, 0f, "R  (Red)",   Color.OrangeRed),
                (0f, 1f, 0f, "G  (Green)", Color.LimeGreen),
                (0f, 0f, 1f, "B  (Blue)",  Color.CornflowerBlue),
                (0f, 0f, 0f, "Black",      Color.Gray),
                (1f, 1f, 1f, "White",      Color.WhiteSmoke),
            };

            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                foreach (var item in corners)
                {
                    PointF p = Project(item.r, item.gr, item.b);
                    using (SolidBrush br = new SolidBrush(item.col))
                        g.DrawString(item.text, font, br, p.X + 6, p.Y - 8);
                }
            }
        }

        private void DrawColorPoints(Graphics g)
        {
            foreach (Color c in _points)
            {
                PointF p = Project(c.R / 255f, c.G / 255f, c.B / 255f);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(190, c)))
                    g.FillEllipse(br, p.X - 2f, p.Y - 2f, 4f, 4f);
            }
        }

        // ── Color inspector ───────────────────────────────────────────────
        private void ShowColor(Color c)
        {
            _swatchPanel.BackColor = c;
            _lblRGB.Text = $"RGB:    ({c.R}, {c.G}, {c.B})";

            try
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(1, 1);
                img[0, 0] = new Bgr(c.B, c.G, c.R);

                Mat hsvMat = new Mat();
                CvInvoke.CvtColor(img.Mat, hsvMat, ColorConversion.Bgr2Hsv);
                byte[] hsv = new byte[3];
                Marshal.Copy(hsvMat.DataPointer, hsv, 0, 3);
                _lblHSV.Text = $"HSV:    ({hsv[0] * 2}°, {(int)(hsv[1] / 255.0 * 100)}%, {(int)(hsv[2] / 255.0 * 100)}%)";
                hsvMat.Dispose();

                Mat ycbcrMat = new Mat();
                CvInvoke.CvtColor(img.Mat, ycbcrMat, ColorConversion.Bgr2YCrCb);
                byte[] ycbcr = new byte[3];
                Marshal.Copy(ycbcrMat.DataPointer, ycbcr, 0, 3);
                _lblYCbCr.Text = $"YCbCr: ({ycbcr[0]}, {ycbcr[1]}, {ycbcr[2]})";
                ycbcrMat.Dispose();

                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(img.Mat, yuvMat, ColorConversion.Bgr2Yuv);
                byte[] yuv = new byte[3];
                Marshal.Copy(yuvMat.DataPointer, yuv, 0, 3);
                _lblYUV.Text = $"YUV:    ({yuv[0]}, {yuv[1]}, {yuv[2]})";
                yuvMat.Dispose();

                Mat labMat = new Mat();
                CvInvoke.CvtColor(img.Mat, labMat, ColorConversion.Bgr2Lab);
                byte[] lab = new byte[3];
                Marshal.Copy(labMat.DataPointer, lab, 0, 3);
                _lblLAB.Text = $"LAB:    ({lab[0]}, {lab[1]}, {lab[2]})";
                labMat.Dispose();

                img.Dispose();
            }
            catch { }
        }
    }

    // Panel subclass with double buffering enabled to prevent flicker
    internal class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
