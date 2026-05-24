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
        private enum ViewMode { RGB, HSV, YCbCr }
        private ViewMode _viewMode = ViewMode.RGB;

        // ── View state ────────────────────────────────────────────────────
        private float _angleX = 25f;
        private float _angleY = -40f;
        private float _zoom   = 230f;
        private Point _lastMouse;
        private bool  _isDragging;

        // ── Per-mode point data (all in [-0.5, 0.5]³) ────────────────────
        private readonly List<(float x, float y, float z, Color c)> _rgbPts
            = new List<(float, float, float, Color)>();
        private readonly List<(float x, float y, float z, Color c)> _hsvPts
            = new List<(float, float, float, Color)>();
        private readonly List<(float x, float y, float z, Color c)> _ycbcrPts
            = new List<(float, float, float, Color)>();

        // ── Controls ──────────────────────────────────────────────────────
        private DoubleBufferedPanel _canvas;
        private ComboBox _cmbView;
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
            SampleAndConvert(source);
            BuildUI();
        }

        // ── Data preparation ──────────────────────────────────────────────

        private void SampleAndConvert(Bitmap bmp)
        {
            int step = Math.Max(1, (int)Math.Sqrt(bmp.Width * bmp.Height / 6000.0));
            var pixels = new List<Color>();
            for (int y = 0; y < bmp.Height; y += step)
                for (int x = 0; x < bmp.Width; x += step)
                    pixels.Add(bmp.GetPixel(x, y));

            int n = pixels.Count;
            if (n == 0) return;

            // Build a 1×n BGR image for batch color conversion
            Image<Bgr, byte> img = new Image<Bgr, byte>(n, 1);
            for (int i = 0; i < n; i++)
            {
                Color p = pixels[i];
                img[0, i] = new Bgr(p.B, p.G, p.R);
            }

            // RGB — center [0,1] → [-0.5, 0.5]
            foreach (Color c in pixels)
                _rgbPts.Add((c.R / 255f - 0.5f, c.G / 255f - 0.5f, c.B / 255f - 0.5f, c));

            // HSV — cylinder: x=S·cos(H), y=V−0.5, z=S·sin(H)
            using (Mat dst = new Mat())
            {
                CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2Hsv);
                byte[] data = new byte[(int)dst.Step];
                Marshal.Copy(dst.DataPointer, data, 0, data.Length);
                for (int i = 0; i < n; i++)
                {
                    float hDeg = data[i * 3]     * 2f;       // OpenCV H is [0,180]
                    float s    = data[i * 3 + 1] / 255f;     // [0,1]
                    float v    = data[i * 3 + 2] / 255f;     // [0,1]
                    double hRad = hDeg * Math.PI / 180.0;
                    _hsvPts.Add((
                        s * (float)Math.Cos(hRad) * 0.5f,   // x — Hue angle × Saturation
                        v - 0.5f,                             // y — Value (height)
                        s * (float)Math.Sin(hRad) * 0.5f,   // z
                        pixels[i]));
                }
            }

            // YCbCr — Bgr2YCrCb gives ch0=Y, ch1=Cr, ch2=Cb; map to x=Cb, y=Y, z=Cr
            using (Mat dst = new Mat())
            {
                CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2YCrCb);
                byte[] data = new byte[(int)dst.Step];
                Marshal.Copy(dst.DataPointer, data, 0, data.Length);
                for (int i = 0; i < n; i++)
                {
                    float yVal  = data[i * 3]     / 255f - 0.5f;  // Y  → vertical
                    float crVal = data[i * 3 + 1] / 255f - 0.5f;  // Cr → z
                    float cbVal = data[i * 3 + 2] / 255f - 0.5f;  // Cb → x
                    _ycbcrPts.Add((cbVal, yVal, crVal, pixels[i]));
                }
            }

            img.Dispose();
        }

        // ── UI ────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text        = "3D Color Space — RGB Cube";
            this.Size        = new Size(970, 700);
            this.MinimumSize = new Size(800, 580);
            this.BackColor   = Color.FromArgb(30, 30, 30);

            // Top toolbar — view selector
            Panel top = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                BackColor = Color.FromArgb(42, 42, 42)
            };
            Label lblView = new Label
            {
                Text      = "View:",
                ForeColor = Color.White,
                Location  = new Point(10, 12),
                AutoSize  = true
            };
            _cmbView = new ComboBox
            {
                Location      = new Point(55, 8),
                Width         = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.FromArgb(60, 60, 60),
                ForeColor     = Color.White
            };
            _cmbView.Items.AddRange(new object[] { "RGB Cube", "HSV Cylinder", "YCbCr Scatter" });
            _cmbView.SelectedIndex = 0;
            _cmbView.SelectedIndexChanged += (s, e) =>
            {
                _viewMode = (ViewMode)_cmbView.SelectedIndex;
                string[] titles = { "RGB Cube", "HSV Cylinder", "YCbCr Scatter" };
                this.Text = "3D Color Space — " + titles[_cmbView.SelectedIndex];
                _canvas.Invalidate();
            };
            top.Controls.Add(lblView);
            top.Controls.Add(_cmbView);

            // Right info panel
            Panel right = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 220,
                BackColor = Color.FromArgb(42, 42, 42),
                Padding   = new Padding(8)
            };

            Label hint = MkLabel("Drag   →  rotate\nScroll →  zoom\nClick dot  →  inspect", 8, 10, 204, 54);
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

            // Double-buffered canvas
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

            // Add order: right + canvas fill first, then top (so top docks above canvas)
            this.Controls.Add(right);
            this.Controls.Add(_canvas);
            this.Controls.Add(top);
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
            List<(float x, float y, float z, Color c)> pts =
                _viewMode == ViewMode.RGB   ? _rgbPts :
                _viewMode == ViewMode.HSV   ? _hsvPts :
                                              _ycbcrPts;

            float bestDist  = float.MaxValue;
            Color bestColor = Color.Empty;

            foreach (var pt in pts)
            {
                PointF p  = Project(pt.x, pt.y, pt.z);
                float  dx = p.X - e.X, dy = p.Y - e.Y;
                float  d  = dx * dx + dy * dy;
                if (d < bestDist) { bestDist = d; bestColor = pt.c; }
            }

            if (bestDist < 400f)
                ShowColor(bestColor);
        }

        // ── 3D projection (input already in [-0.5, 0.5]³) ─────────────────
        private PointF Project(float x, float y, float z)
        {
            float ay = _angleY * (float)(Math.PI / 180.0);
            float x1 =  x * (float)Math.Cos(ay) + z * (float)Math.Sin(ay);
            float z1 = -x * (float)Math.Sin(ay) + z * (float)Math.Cos(ay);

            float ax = _angleX * (float)(Math.PI / 180.0);
            float y2 =  y * (float)Math.Cos(ax) - z1 * (float)Math.Sin(ax);

            return new PointF(
                _canvas.Width  / 2f + x1 * _zoom,
                _canvas.Height / 2f - y2 * _zoom
            );
        }

        // ── Paint dispatch ────────────────────────────────────────────────
        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            switch (_viewMode)
            {
                case ViewMode.RGB:
                    DrawRgbWireframe(g);
                    DrawRgbAxisLabels(g);
                    DrawPoints(g, _rgbPts);
                    break;
                case ViewMode.HSV:
                    DrawHsvCylinder(g);
                    DrawHsvAxisLabels(g);
                    DrawPoints(g, _hsvPts);
                    break;
                case ViewMode.YCbCr:
                    DrawYcbcrBox(g);
                    DrawYcbcrAxisLabels(g);
                    DrawPoints(g, _ycbcrPts);
                    break;
            }
        }

        // ── RGB Cube ──────────────────────────────────────────────────────

        private void DrawRgbWireframe(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(110, 110, 110), 1f))
            {
                for (int i = 0; i < CubeEdges.GetLength(0); i++)
                {
                    int a = CubeEdges[i, 0], b = CubeEdges[i, 1];
                    PointF p0 = Project(CubeVerts[a,0]-0.5f, CubeVerts[a,1]-0.5f, CubeVerts[a,2]-0.5f);
                    PointF p1 = Project(CubeVerts[b,0]-0.5f, CubeVerts[b,1]-0.5f, CubeVerts[b,2]-0.5f);
                    g.DrawLine(pen, p0, p1);
                }
            }
        }

        private void DrawRgbAxisLabels(Graphics g)
        {
            var corners = new (float x, float y, float z, string txt, Color col)[]
            {
                ( 0.5f, -0.5f, -0.5f, "R  (Red)",   Color.OrangeRed),
                (-0.5f,  0.5f, -0.5f, "G  (Green)", Color.LimeGreen),
                (-0.5f, -0.5f,  0.5f, "B  (Blue)",  Color.CornflowerBlue),
                (-0.5f, -0.5f, -0.5f, "Black",      Color.Gray),
                ( 0.5f,  0.5f,  0.5f, "White",      Color.WhiteSmoke),
            };

            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                foreach (var item in corners)
                {
                    PointF p = Project(item.x, item.y, item.z);
                    using (SolidBrush br = new SolidBrush(item.col))
                        g.DrawString(item.txt, font, br, p.X + 6, p.Y - 8);
                }
            }
        }

        // ── HSV Cylinder ──────────────────────────────────────────────────

        private void DrawHsvCylinder(Graphics g)
        {
            const int   segs = 36;
            const float r    = 0.5f;

            using (Pen penRim  = new Pen(Color.FromArgb(110, 110, 110), 1f))
            using (Pen penMid  = new Pen(Color.FromArgb(65,  65,  65),  1f))
            using (Pen penAxis = new Pen(Color.FromArgb(160, 160, 160), 1f))
            {
                // Top circle (V = 1)
                DrawCircle(g, penRim, r,  0.5f, segs);
                // Bottom circle (V = 0)
                DrawCircle(g, penRim, r, -0.5f, segs);
                // Mid-height ring (V = 0.5)
                DrawCircle(g, penMid, r,  0.0f, segs);

                // 8 vertical lines at major hue angles
                for (int hi = 0; hi < 8; hi++)
                {
                    double ang = hi * 2.0 * Math.PI / 8;
                    float  cx  = r * (float)Math.Cos(ang);
                    float  cz  = r * (float)Math.Sin(ang);
                    g.DrawLine(penRim, Project(cx, 0.5f, cz), Project(cx, -0.5f, cz));
                }

                // Central axis
                g.DrawLine(penAxis, Project(0, -0.5f, 0), Project(0, 0.5f, 0));

                // Radial spokes on top disc at 6 hue angles
                for (int hi = 0; hi < 6; hi++)
                {
                    double ang = hi * 2.0 * Math.PI / 6;
                    float  cx  = r * (float)Math.Cos(ang);
                    float  cz  = r * (float)Math.Sin(ang);
                    g.DrawLine(penMid, Project(0, 0.5f, 0), Project(cx, 0.5f, cz));
                }
            }
        }

        private void DrawCircle(Graphics g, Pen pen, float radius, float yHeight, int segments)
        {
            for (int s = 0; s < segments; s++)
            {
                double a0 = s       * 2.0 * Math.PI / segments;
                double a1 = (s + 1) * 2.0 * Math.PI / segments;
                g.DrawLine(pen,
                    Project(radius * (float)Math.Cos(a0), yHeight, radius * (float)Math.Sin(a0)),
                    Project(radius * (float)Math.Cos(a1), yHeight, radius * (float)Math.Sin(a1)));
            }
        }

        private void DrawHsvAxisLabels(Graphics g)
        {
            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                // V axis top / bottom
                PointF vTop = Project(0, 0.56f, 0);
                PointF vBot = Project(0, -0.56f, 0);
                using (SolidBrush br = new SolidBrush(Color.White))
                {
                    g.DrawString("V = 1  (Bright)", font, br, vTop.X + 5, vTop.Y - 9);
                    g.DrawString("V = 0  (Dark)",   font, br, vBot.X + 5, vBot.Y + 2);
                }

                // Saturation arrow label along the top disc
                PointF sInner = Project(0.08f, 0.52f, 0);
                PointF sOuter = Project(0.52f, 0.52f, 0);
                PointF sMid   = new PointF((sInner.X + sOuter.X) / 2f - 10, Math.Min(sInner.Y, sOuter.Y) - 14);
                using (SolidBrush br = new SolidBrush(Color.Silver))
                    g.DrawString("← S (Saturation) →", font, br, sMid.X, sMid.Y);

                // Hue labels at rim (top face, projected outward)
                var hueLabels = new (int deg, string txt, Color col)[]
                {
                    (0,   "Red",     Color.OrangeRed),
                    (60,  "Yellow",  Color.Yellow),
                    (120, "Green",   Color.LimeGreen),
                    (180, "Cyan",    Color.Cyan),
                    (240, "Blue",    Color.CornflowerBlue),
                    (300, "Magenta", Color.Violet),
                };
                foreach (var (deg, txt, col) in hueLabels)
                {
                    double rad = deg * Math.PI / 180.0;
                    float  cx  = 0.62f * (float)Math.Cos(rad);
                    float  cz  = 0.62f * (float)Math.Sin(rad);
                    PointF p   = Project(cx, 0.5f, cz);
                    using (SolidBrush br = new SolidBrush(col))
                        g.DrawString(txt, font, br, p.X + 3, p.Y - 8);
                }
            }
        }

        // ── YCbCr Scatter ─────────────────────────────────────────────────

        private void DrawYcbcrBox(Graphics g)
        {
            // Faint unit box as reference frame
            using (Pen penBox = new Pen(Color.FromArgb(55, 55, 55), 1f))
            {
                for (int i = 0; i < CubeEdges.GetLength(0); i++)
                {
                    int a = CubeEdges[i, 0], b = CubeEdges[i, 1];
                    PointF p0 = Project(CubeVerts[a,0]-0.5f, CubeVerts[a,1]-0.5f, CubeVerts[a,2]-0.5f);
                    PointF p1 = Project(CubeVerts[b,0]-0.5f, CubeVerts[b,1]-0.5f, CubeVerts[b,2]-0.5f);
                    g.DrawLine(penBox, p0, p1);
                }
            }

            // Highlighted axes from origin corner
            using (Pen penCb   = new Pen(Color.FromArgb(80, 100, 200), 2f))
            using (Pen penY    = new Pen(Color.FromArgb(220, 220, 80), 2f))
            using (Pen penCr   = new Pen(Color.FromArgb(200, 80, 80), 2f))
            {
                PointF origin = Project(-0.5f, -0.5f, -0.5f);
                g.DrawLine(penCb, origin, Project( 0.5f, -0.5f, -0.5f));  // Cb axis (x)
                g.DrawLine(penY,  origin, Project(-0.5f,  0.5f, -0.5f));  // Y  axis (y)
                g.DrawLine(penCr, origin, Project(-0.5f, -0.5f,  0.5f));  // Cr axis (z)
            }
        }

        private void DrawYcbcrAxisLabels(Graphics g)
        {
            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                // Y axis label (top)
                PointF yTop = Project(-0.5f, 0.55f, -0.5f);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(220, 220, 80)))
                    g.DrawString("Y (Luma)", font, br, yTop.X + 5, yTop.Y - 8);

                // Cb axis label
                PointF cbEnd = Project(0.55f, -0.5f, -0.5f);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(100, 140, 230)))
                    g.DrawString("Cb (Blue diff)", font, br, cbEnd.X + 5, cbEnd.Y - 8);

                // Cr axis label
                PointF crEnd = Project(-0.5f, -0.5f, 0.55f);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(220, 100, 80)))
                    g.DrawString("Cr (Red diff)", font, br, crEnd.X + 5, crEnd.Y - 8);

                // Min / max corner hints
                PointF origin = Project(-0.5f, -0.5f, -0.5f);
                using (SolidBrush br = new SolidBrush(Color.Gray))
                    g.DrawString("(0,0,0)", font, br, origin.X + 5, origin.Y + 2);
            }
        }

        // ── Shared point drawing ──────────────────────────────────────────

        private void DrawPoints(Graphics g, List<(float x, float y, float z, Color c)> pts)
        {
            foreach (var pt in pts)
            {
                PointF p = Project(pt.x, pt.y, pt.z);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(190, pt.c)))
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
