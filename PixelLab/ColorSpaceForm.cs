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

        // ── Pre-generated full color-space point clouds ───────────────────
        // RGB: just the cube corners/edges/faces — no point cloud needed
        private readonly List<(float x, float y, float z, Color c)> _hsvPts
            = new List<(float, float, float, Color)>();
        private readonly List<(float x, float y, float z, Color c)> _ycbcrPts
            = new List<(float, float, float, Color)>();

        // ── Controls ──────────────────────────────────────────────────────
        private DoubleBufferedPanel _canvas;
        private ComboBox _cmbView;
        private Panel  _swatchPanel;
        private Label  _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB, _lblCMY;

        // ── RGB unit-cube geometry — vertices in [0,1]³ ───────────────────
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
        private static readonly Color[] CornerColors =
        {
            Color.FromArgb(  0,   0,   0),  // 0 Black
            Color.FromArgb(255,   0,   0),  // 1 Red
            Color.FromArgb(255, 255,   0),  // 2 Yellow
            Color.FromArgb(  0, 255,   0),  // 3 Green
            Color.FromArgb(  0,   0, 255),  // 4 Blue
            Color.FromArgb(255,   0, 255),  // 5 Magenta
            Color.FromArgb(255, 255, 255),  // 6 White
            Color.FromArgb(  0, 255, 255),  // 7 Cyan
        };
        private static readonly int[,] CubeFaceVerts =
        {
            {0, 1, 2, 3},
            {4, 5, 6, 7},
            {0, 1, 5, 4},
            {3, 2, 6, 7},
            {0, 3, 7, 4},
            {1, 2, 6, 5},
        };
        private static readonly string[] CornerNames =
            { "Black", "Red", "Yellow", "Green", "Blue", "Magenta", "White", "Cyan" };

        // ── Constructor ───────────────────────────────────────────────────
        public ColorSpaceForm()
        {
            GenerateHsvCloud();
            GenerateYcbcrCloud();
            BuildUI();
        }

        // ── Procedural point-cloud generation ─────────────────────────────

        private void GenerateHsvCloud()
        {
            // Sample H/S/V space: 36 hue slices × 10 saturation rings × 8 value levels
            const int hSteps = 36, sSteps = 10, vSteps = 8;
            for (int hi = 0; hi < hSteps; hi++)
            {
                float hDeg = hi * 360f / hSteps;
                double hRad = hDeg * Math.PI / 180.0;
                for (int si = 1; si <= sSteps; si++)
                {
                    float s = si / (float)sSteps;
                    for (int vi = 0; vi <= vSteps; vi++)
                    {
                        float v = vi / (float)vSteps;
                        Color c = HsvToRgb(hDeg, s, v);
                        float x = s * (float)Math.Cos(hRad) * 0.5f;
                        float y = v - 0.5f;
                        float z = s * (float)Math.Sin(hRad) * 0.5f;
                        _hsvPts.Add((x, y, z, c));
                    }
                }
            }
        }

        private void GenerateYcbcrCloud()
        {
            // Sample Y/Cb/Cr space on a regular grid, back-convert to RGB for color
            const int steps = 12;
            int n = (steps + 1) * (steps + 1) * (steps + 1);
            Image<Bgr, byte> src = new Image<Bgr, byte>(n, 1);

            // We'll build an index list alongside the image rows
            var entries = new List<(float x, float y, float z)>(n);
            int idx = 0;
            for (int yi = 0; yi <= steps; yi++)
                for (int cbi = 0; cbi <= steps; cbi++)
                    for (int cri = 0; cri <= steps; cri++)
                    {
                        byte Y  = (byte)(yi  * 255 / steps);
                        byte Cb = (byte)(cbi * 255 / steps);
                        byte Cr = (byte)(cri * 255 / steps);
                        // Store as YCrCb order (OpenCV convention ch0=Y,ch1=Cr,ch2=Cb)
                        // We can't directly set YCrCb pixels into a BGR image — instead
                        // we create a YCrCb Mat manually below.
                        entries.Add((
                            Cb / 255f - 0.5f,   // x = Cb
                            Y  / 255f - 0.5f,   // y = Y
                            Cr / 255f - 0.5f)); // z = Cr
                        src[0, idx++] = new Bgr(Cb, Cr, Y);  // placeholder, overwritten below
                    }

            // Build a raw YCrCb byte buffer and convert to BGR
            byte[] ycrcbBuf = new byte[n * 3];
            idx = 0;
            for (int yi = 0; yi <= steps; yi++)
                for (int cbi = 0; cbi <= steps; cbi++)
                    for (int cri = 0; cri <= steps; cri++)
                    {
                        ycrcbBuf[idx * 3 + 0] = (byte)(yi  * 255 / steps); // Y
                        ycrcbBuf[idx * 3 + 1] = (byte)(cri * 255 / steps); // Cr
                        ycrcbBuf[idx * 3 + 2] = (byte)(cbi * 255 / steps); // Cb
                        idx++;
                    }

            Mat ycrcbMat = new Mat(1, n, DepthType.Cv8U, 3);
            Marshal.Copy(ycrcbBuf, 0, ycrcbMat.DataPointer, ycrcbBuf.Length);

            Mat bgrMat = new Mat();
            CvInvoke.CvtColor(ycrcbMat, bgrMat, ColorConversion.YCrCb2Bgr);

            byte[] bgrBuf = new byte[n * 3];
            Marshal.Copy(bgrMat.DataPointer, bgrBuf, 0, bgrBuf.Length);

            for (int i = 0; i < n; i++)
            {
                byte b = bgrBuf[i * 3 + 0];
                byte g = bgrBuf[i * 3 + 1];
                byte r = bgrBuf[i * 3 + 2];
                Color c = Color.FromArgb(r, g, b);
                var (x, y, z) = entries[i];
                _ycbcrPts.Add((x, y, z, c));
            }

            src.Dispose(); ycrcbMat.Dispose(); bgrMat.Dispose();
        }

        // ── UI ────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text        = "3D Color Space — RGB Cube";
            this.Size        = new Size(970, 700);
            this.MinimumSize = new Size(800, 580);
            this.BackColor   = Color.FromArgb(30, 30, 30);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(42, 42, 42) };
            Label lblView = new Label { Text = "View:", ForeColor = Color.White, Location = new Point(10, 12), AutoSize = true };
            _cmbView = new ComboBox
            {
                Location = new Point(55, 8), Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White
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

            Panel right = new Panel { Dock = DockStyle.Right, Width = 220, BackColor = Color.FromArgb(42, 42, 42), Padding = new Padding(8) };

            Label hint = MkLabel("Drag   →  rotate\nScroll →  zoom\nClick  →  inspect color", 8, 10, 204, 54);
            hint.ForeColor = Color.Silver;
            Label sep = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(8, 72), Size = new Size(204, 2) };
            Label selTitle = MkLabel("Selected Color", 8, 80, 204, 18);
            selTitle.ForeColor = Color.White;
            selTitle.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

            _swatchPanel = new Panel
            {
                Location = new Point(8, 104), Size = new Size(204, 28),
                BackColor = Color.FromArgb(60, 60, 60), BorderStyle = BorderStyle.FixedSingle
            };

            _lblRGB   = MkLabel("RGB:    -", 8, 140, 204, 18);
            _lblHSV   = MkLabel("HSV:    -", 8, 160, 204, 18);
            _lblYCbCr = MkLabel("YCbCr: -", 8, 180, 204, 18);
            _lblYUV   = MkLabel("YUV:    -", 8, 200, 204, 18);
            _lblLAB   = MkLabel("LAB:    -", 8, 220, 204, 18);
            _lblCMY   = MkLabel("CMY:    -", 8, 240, 204, 18);
            foreach (Label l in new[] { _lblRGB, _lblHSV, _lblYCbCr, _lblYUV, _lblLAB, _lblCMY })
                l.ForeColor = Color.LightGray;

            Button btnReset = new Button
            {
                Text = "Reset View", Location = new Point(8, 270), Size = new Size(204, 28),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(65, 65, 65)
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
            right.Controls.Add(_lblCMY);
            right.Controls.Add(btnReset);

            _canvas = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 18), Cursor = Cursors.SizeAll };
            _canvas.Paint      += OnPaint;
            _canvas.MouseDown  += (s, e) => { _isDragging = true; _lastMouse = e.Location; };
            _canvas.MouseUp    += (s, e) => _isDragging = false;
            _canvas.MouseMove  += OnMouseMove;
            _canvas.MouseWheel += OnMouseWheel;
            _canvas.MouseClick += OnCanvasClick;

            this.Controls.Add(right);
            this.Controls.Add(_canvas);
            this.Controls.Add(top);
        }

        private Label MkLabel(string text, int x, int y, int w, int h)
            => new Label { Text = text, Location = new Point(x, y), Size = new Size(w, h), ForeColor = Color.LightGray, AutoSize = false };

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
            Color picked;
            if (_viewMode == ViewMode.RGB)
            {
                // For RGB cube, pick from nearest face-sampled grid point
                picked = PickRgbFromClick(e.Location);
            }
            else
            {
                List<(float x, float y, float z, Color c)> pts =
                    _viewMode == ViewMode.HSV ? _hsvPts : _ycbcrPts;
                float bestDist = float.MaxValue;
                picked = Color.Empty;
                foreach (var pt in pts)
                {
                    PointF p  = Project(pt.x, pt.y, pt.z);
                    float  dx = p.X - e.X, dy = p.Y - e.Y;
                    float  d  = dx * dx + dy * dy;
                    if (d < bestDist) { bestDist = d; picked = pt.c; }
                }
                if (bestDist > 400f) return;
            }
            if (picked != Color.Empty)
                ShowColor(picked);
        }

        private Color PickRgbFromClick(Point click)
        {
            // Find the nearest sampled RGB grid point on the cube surface
            float best = float.MaxValue;
            Color found = Color.Empty;
            const int div = 16;
            for (int fi = 0; fi < CubeFaceVerts.GetLength(0); fi++)
            {
                int i0 = CubeFaceVerts[fi, 0], i1 = CubeFaceVerts[fi, 1];
                int i2 = CubeFaceVerts[fi, 2], i3 = CubeFaceVerts[fi, 3];
                Color c0 = CornerColors[i0], c1 = CornerColors[i1];
                Color c2 = CornerColors[i2], c3 = CornerColors[i3];
                float[] v0 = { CubeVerts[i0,0]-0.5f, CubeVerts[i0,1]-0.5f, CubeVerts[i0,2]-0.5f };
                float[] v1 = { CubeVerts[i1,0]-0.5f, CubeVerts[i1,1]-0.5f, CubeVerts[i1,2]-0.5f };
                float[] v2 = { CubeVerts[i2,0]-0.5f, CubeVerts[i2,1]-0.5f, CubeVerts[i2,2]-0.5f };
                float[] v3 = { CubeVerts[i3,0]-0.5f, CubeVerts[i3,1]-0.5f, CubeVerts[i3,2]-0.5f };
                for (int ui = 0; ui <= div; ui++)
                {
                    float u = ui / (float)div;
                    for (int vi = 0; vi <= div; vi++)
                    {
                        float vv = vi / (float)div;
                        float px = Lerp3(v0[0], v1[0], v2[0], v3[0], u, vv);
                        float py = Lerp3(v0[1], v1[1], v2[1], v3[1], u, vv);
                        float pz = Lerp3(v0[2], v1[2], v2[2], v3[2], u, vv);
                        PointF proj = Project(px, py, pz);
                        float dx = proj.X - click.X, dy = proj.Y - click.Y;
                        float d = dx * dx + dy * dy;
                        if (d < best)
                        {
                            best = d;
                            // Bilinear color interpolation across the face
                            int r = (int)Lerp3(c0.R, c1.R, c2.R, c3.R, u, vv);
                            int g = (int)Lerp3(c0.G, c1.G, c2.G, c3.G, u, vv);
                            int b = (int)Lerp3(c0.B, c1.B, c2.B, c3.B, u, vv);
                            found = Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
                        }
                    }
                }
            }
            return best < 400f ? found : Color.Empty;
        }

        private static float Lerp3(float a, float b, float c, float d, float u, float v)
            => (a * (1 - u) + b * u) * (1 - v) + (d * (1 - u) + c * u) * v;

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        // ── 3D projection ─────────────────────────────────────────────────
        private PointF Project(float x, float y, float z)
        {
            float ay = _angleY * (float)(Math.PI / 180.0);
            float x1 =  x * (float)Math.Cos(ay) + z * (float)Math.Sin(ay);
            float z1 = -x * (float)Math.Sin(ay) + z * (float)Math.Cos(ay);

            float ax = _angleX * (float)(Math.PI / 180.0);
            float y2 =  y * (float)Math.Cos(ax) - z1 * (float)Math.Sin(ax);

            return new PointF(
                _canvas.Width  / 2f + x1 * _zoom,
                _canvas.Height / 2f - y2 * _zoom);
        }

        // ── Paint dispatch ────────────────────────────────────────────────
        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            switch (_viewMode)
            {
                case ViewMode.RGB:
                    DrawRgbFaces(g);
                    DrawRgbGradientEdges(g);
                    DrawRgbCorners(g);
                    DrawRgbAxisLabels(g);
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

        private void DrawRgbFaces(Graphics g)
        {
            const int alpha = 60;
            for (int fi = 0; fi < CubeFaceVerts.GetLength(0); fi++)
            {
                int i0 = CubeFaceVerts[fi, 0], i1 = CubeFaceVerts[fi, 1];
                int i2 = CubeFaceVerts[fi, 2], i3 = CubeFaceVerts[fi, 3];

                PointF p0 = Project(CubeVerts[i0,0]-0.5f, CubeVerts[i0,1]-0.5f, CubeVerts[i0,2]-0.5f);
                PointF p1 = Project(CubeVerts[i1,0]-0.5f, CubeVerts[i1,1]-0.5f, CubeVerts[i1,2]-0.5f);
                PointF p2 = Project(CubeVerts[i2,0]-0.5f, CubeVerts[i2,1]-0.5f, CubeVerts[i2,2]-0.5f);
                PointF p3 = Project(CubeVerts[i3,0]-0.5f, CubeVerts[i3,1]-0.5f, CubeVerts[i3,2]-0.5f);

                float cross = (p1.X-p0.X)*(p3.Y-p0.Y) - (p3.X-p0.X)*(p1.Y-p0.Y);
                if (Math.Abs(cross) < 4f) continue;

                Color c0 = CornerColors[i0], c1 = CornerColors[i1];
                Color c2 = CornerColors[i2], c3 = CornerColors[i3];
                Color center = Color.FromArgb(alpha,
                    (c0.R+c1.R+c2.R+c3.R)/4, (c0.G+c1.G+c2.G+c3.G)/4, (c0.B+c1.B+c2.B+c3.B)/4);

                PointF[] quad = { p0, p1, p2, p3 };
                using (var brush = new PathGradientBrush(quad))
                {
                    brush.CenterColor    = center;
                    brush.SurroundColors = new[]
                    {
                        Color.FromArgb(alpha, c0), Color.FromArgb(alpha, c1),
                        Color.FromArgb(alpha, c2), Color.FromArgb(alpha, c3)
                    };
                    g.FillPolygon(brush, quad);
                }
            }
        }

        private void DrawRgbGradientEdges(Graphics g)
        {
            for (int i = 0; i < CubeEdges.GetLength(0); i++)
            {
                int a = CubeEdges[i, 0], b = CubeEdges[i, 1];
                PointF p0 = Project(CubeVerts[a,0]-0.5f, CubeVerts[a,1]-0.5f, CubeVerts[a,2]-0.5f);
                PointF p1 = Project(CubeVerts[b,0]-0.5f, CubeVerts[b,1]-0.5f, CubeVerts[b,2]-0.5f);
                float dx = p1.X - p0.X, dy = p1.Y - p0.Y;
                if (dx*dx + dy*dy < 1f) continue;
                using (var brush = new LinearGradientBrush(p0, p1, CornerColors[a], CornerColors[b]))
                using (var pen   = new Pen(brush, 2.5f))
                    g.DrawLine(pen, p0, p1);
            }
        }

        private void DrawRgbCorners(Graphics g)
        {
            const float r = 5f;
            for (int i = 0; i < 8; i++)
            {
                PointF p = Project(CubeVerts[i,0]-0.5f, CubeVerts[i,1]-0.5f, CubeVerts[i,2]-0.5f);
                using (var fill   = new SolidBrush(CornerColors[i]))
                using (var border = new Pen(Color.FromArgb(160, 255, 255, 255), 1f))
                {
                    g.FillEllipse(fill,   p.X - r, p.Y - r, r * 2, r * 2);
                    g.DrawEllipse(border, p.X - r, p.Y - r, r * 2, r * 2);
                }
            }
        }

        private void DrawRgbAxisLabels(Graphics g)
        {
            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                for (int i = 0; i < 8; i++)
                {
                    PointF p   = Project(CubeVerts[i,0]-0.5f, CubeVerts[i,1]-0.5f, CubeVerts[i,2]-0.5f);
                    Color  col = (i == 0) ? Color.Gray : CornerColors[i];
                    using (var br = new SolidBrush(col))
                        g.DrawString(CornerNames[i], font, br, p.X + 7, p.Y - 8);
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
                DrawCircle(g, penRim, r,  0.5f, segs);
                DrawCircle(g, penRim, r, -0.5f, segs);
                DrawCircle(g, penMid, r,  0.0f, segs);
                for (int hi = 0; hi < 8; hi++)
                {
                    double ang = hi * 2.0 * Math.PI / 8;
                    float cx = r * (float)Math.Cos(ang), cz = r * (float)Math.Sin(ang);
                    g.DrawLine(penRim, Project(cx, 0.5f, cz), Project(cx, -0.5f, cz));
                }
                g.DrawLine(penAxis, Project(0, -0.5f, 0), Project(0, 0.5f, 0));
                for (int hi = 0; hi < 6; hi++)
                {
                    double ang = hi * 2.0 * Math.PI / 6;
                    float cx = r * (float)Math.Cos(ang), cz = r * (float)Math.Sin(ang);
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
                using (SolidBrush br = new SolidBrush(Color.White))
                {
                    g.DrawString("V = 1  (Bright)", font, br, Project(0, 0.56f, 0).X + 5,  Project(0, 0.56f, 0).Y - 9);
                    g.DrawString("V = 0  (Dark)",   font, br, Project(0, -0.56f, 0).X + 5, Project(0, -0.56f, 0).Y + 2);
                }
                PointF sInner = Project(0.08f, 0.52f, 0);
                PointF sOuter = Project(0.52f, 0.52f, 0);
                PointF sMid   = new PointF((sInner.X + sOuter.X) / 2f - 10, Math.Min(sInner.Y, sOuter.Y) - 14);
                using (SolidBrush br = new SolidBrush(Color.Silver))
                    g.DrawString("← S (Saturation) →", font, br, sMid.X, sMid.Y);

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
            using (Pen penBox = new Pen(Color.FromArgb(55, 55, 55), 1f))
                for (int i = 0; i < CubeEdges.GetLength(0); i++)
                {
                    int a = CubeEdges[i, 0], b = CubeEdges[i, 1];
                    g.DrawLine(penBox,
                        Project(CubeVerts[a,0]-0.5f, CubeVerts[a,1]-0.5f, CubeVerts[a,2]-0.5f),
                        Project(CubeVerts[b,0]-0.5f, CubeVerts[b,1]-0.5f, CubeVerts[b,2]-0.5f));
                }

            using (Pen penCb = new Pen(Color.FromArgb(80, 100, 200), 2f))
            using (Pen penY  = new Pen(Color.FromArgb(220, 220, 80), 2f))
            using (Pen penCr = new Pen(Color.FromArgb(200, 80, 80), 2f))
            {
                PointF origin = Project(-0.5f, -0.5f, -0.5f);
                g.DrawLine(penCb, origin, Project( 0.5f, -0.5f, -0.5f));
                g.DrawLine(penY,  origin, Project(-0.5f,  0.5f, -0.5f));
                g.DrawLine(penCr, origin, Project(-0.5f, -0.5f,  0.5f));
            }
        }

        private void DrawYcbcrAxisLabels(Graphics g)
        {
            using (Font font = new Font("Arial", 9f, FontStyle.Bold))
            {
                using (SolidBrush br = new SolidBrush(Color.FromArgb(220, 220, 80)))
                    g.DrawString("Y (Luma)", font, br, Project(-0.5f, 0.55f, -0.5f).X + 5, Project(-0.5f, 0.55f, -0.5f).Y - 8);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(100, 140, 230)))
                    g.DrawString("Cb (Blue diff)", font, br, Project(0.55f, -0.5f, -0.5f).X + 5, Project(0.55f, -0.5f, -0.5f).Y - 8);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(220, 100, 80)))
                    g.DrawString("Cr (Red diff)", font, br, Project(-0.5f, -0.5f, 0.55f).X + 5, Project(-0.5f, -0.5f, 0.55f).Y - 8);
                using (SolidBrush br = new SolidBrush(Color.Gray))
                    g.DrawString("(0,0,0)", font, br, Project(-0.5f, -0.5f, -0.5f).X + 5, Project(-0.5f, -0.5f, -0.5f).Y + 2);
            }
        }

        // ── Shared point drawing ──────────────────────────────────────────

        private void DrawPoints(Graphics g, List<(float x, float y, float z, Color c)> pts)
        {
            foreach (var pt in pts)
            {
                PointF p = Project(pt.x, pt.y, pt.z);
                using (SolidBrush br = new SolidBrush(Color.FromArgb(210, pt.c)))
                    g.FillEllipse(br, p.X - 2f, p.Y - 2f, 4f, 4f);
            }
        }

        // ── Color inspector ───────────────────────────────────────────────

        private void ShowColor(Color c)
        {
            _swatchPanel.BackColor = c;
            _lblRGB.Text  = $"RGB:    ({c.R}, {c.G}, {c.B})";
            _lblCMY.Text  = $"CMY:    ({255-c.R}, {255-c.G}, {255-c.B})";

            try
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(1, 1);
                img[0, 0] = new Bgr(c.B, c.G, c.R);
                Mat m = new Mat();
                byte[] buf = new byte[3];

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Hsv);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                _lblHSV.Text = $"HSV:    ({buf[0] * 2}°, {(int)(buf[1]/255.0*100)}%, {(int)(buf[2]/255.0*100)}%)";

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
