using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        private enum CsMode { RGB, CMY, HSV, YCbCr, YUV, LAB }
        private CsMode _csMode = CsMode.RGB;
        private bool _suppressAdjustments = false;

        private static readonly string[][] _csLabels =
        {
            new[] { "Red",       "Green",      "Blue"   },
            new[] { "Cyan",      "Magenta",    "Yellow" },
            new[] { "Hue",       "Saturation", "Value"  },
            new[] { "Y (Luma)",  "Cr",         "Cb"     },
            new[] { "Y (Luma)",  "U",          "V"      },
            new[] { "L (Light)", "a*",         "b*"     },
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void lblImageInfo_Load(object sender, EventArgs e) { }

        // ── Load / Save / Reset ───────────────────────────────────────────

        private void load_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            Bitmap loaded = new Bitmap(dlg.FileName);
            pictureBox1.Image = loaded;
            pictureBox2.Image = new Bitmap(loaded);

            var fi = new System.IO.FileInfo(dlg.FileName);
            lblImageInfo.Text = $"Image: {fi.Name}\nSize: {fi.Length / 1024} KB\n" +
                                $"Dimensions: {loaded.Width} x {loaded.Height}\n" +
                                $"Format: {System.IO.Path.GetExtension(dlg.FileName).TrimStart('.').ToUpper()}";

            SetColorSpace(CsMode.RGB);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null) { MessageBox.Show("No image to save!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            SaveFileDialog dlg = new SaveFileDialog { Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp", Title = "Save Modified Image" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image.Save(dlg.FileName);
                MessageBox.Show("Image saved successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Color space conversion buttons ────────────────────────────────

        private void button7_Click(object sender, EventArgs e)   // CMY
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Please load an image first!"); return; }
            Bitmap bmp = new Bitmap(pictureBox1.Image), result = new Bitmap(bmp.Width, bmp.Height);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++) { Color p = bmp.GetPixel(x, y); result.SetPixel(x, y, Color.FromArgb(255 - p.R, 255 - p.G, 255 - p.B)); }
            pictureBox2.Image = result; bmp.Dispose();
            SetColorSpace(CsMode.CMY);
        }

        private void button6_Click_1(object sender, EventArgs e)   // HSV
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Please load an image first!"); return; }
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++) { Color p = bmp.GetPixel(x, y); img[y, x] = new Bgr(p.B, p.G, p.R); }
                Image<Hsv, byte> hsv = img.Convert<Hsv, byte>();
                Image<Bgr, byte> back = hsv.Convert<Bgr, byte>();
                Bitmap result = new Bitmap(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++) { var p = back[y, x]; result.SetPixel(x, y, Color.FromArgb((int)p.Red, (int)p.Green, (int)p.Blue)); }
                pictureBox2.Image = result;
                img.Dispose(); hsv.Dispose(); back.Dispose(); bmp.Dispose();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            SetColorSpace(CsMode.HSV);
        }

        private void button8_Click(object sender, EventArgs e)   // YCbCr
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Please load an image first!"); return; }
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++) { Color p = bmp.GetPixel(x, y); img[y, x] = new Bgr(p.B, p.G, p.R); }
                Mat dst = new Mat(); CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2YCrCb);
                pictureBox2.Image = dst.ToBitmap();
                img.Dispose(); dst.Dispose(); bmp.Dispose();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            SetColorSpace(CsMode.YCbCr);
        }

        private void button9_Click(object sender, EventArgs e)   // YUV
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Please load an image first!"); return; }
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++) { Color p = bmp.GetPixel(x, y); img[y, x] = new Bgr(p.B, p.G, p.R); }
                Mat dst = new Mat(); CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2Yuv);
                pictureBox2.Image = dst.ToBitmap();
                img.Dispose(); dst.Dispose(); bmp.Dispose();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            SetColorSpace(CsMode.YUV);
        }

        private void button10_Click(object sender, EventArgs e)   // LAB
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Please load an image first!"); return; }
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++) { Color p = bmp.GetPixel(x, y); img[y, x] = new Bgr(p.B, p.G, p.R); }
                Mat dst = new Mat(); CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2Lab);
                pictureBox2.Image = dst.ToBitmap();
                img.Dispose(); dst.Dispose(); bmp.Dispose();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            SetColorSpace(CsMode.LAB);
        }

        // ── Color space tracker ───────────────────────────────────────────

        private void SetColorSpace(CsMode mode)
        {
            _csMode = mode;
            string[] n = _csLabels[(int)mode];

            // Reset sliders to centre without each change firing ApplyChannelAdjustments
            _suppressAdjustments = true;
            trkR.Value = 0; trkG.Value = 0; trkB.Value = 0; trkBrightness.Value = 0;
            chkR.Checked = true; chkG.Checked = true; chkB.Checked = true;
            _suppressAdjustments = false;

            lblRedOffset.Text   = $"{n[0]}: 0";
            lblGreenOffset.Text = $"{n[1]}: 0";
            lblBlueOffset.Text  = $"{n[2]}: 0";
            lblBrightness.Text  = "Brightness: 0";
            // Checkboxes keep fixed text "On" — channel name is shown in the label above
            grpChannels.Text = $"Channel Controls ({mode})";
            // pictureBox2 is left as-is; the calling button already set the view
        }

        private void btnRGB_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;
            pictureBox2.Image = new Bitmap(pictureBox1.Image);
            SetColorSpace(CsMode.RGB);
        }

        // ── Channel controls ──────────────────────────────────────────────

        // ── Trackbar handlers — update label + NUD + apply ───────────────

        private void trkR_ValueChanged(object sender, EventArgs e)
        {
            lblRedOffset.Text = _csLabels[(int)_csMode][0] + ": " + trkR.Value;
            nudR.Value = trkR.Value;
            ApplyChannelAdjustments();
        }

        private void trkG_ValueChanged(object sender, EventArgs e)
        {
            lblGreenOffset.Text = _csLabels[(int)_csMode][1] + ": " + trkG.Value;
            nudG.Value = trkG.Value;
            ApplyChannelAdjustments();
        }

        private void trkB_ValueChanged(object sender, EventArgs e)
        {
            lblBlueOffset.Text = _csLabels[(int)_csMode][2] + ": " + trkB.Value;
            nudB.Value = trkB.Value;
            ApplyChannelAdjustments();
        }

        private void trkBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = "Brightness: " + trkBrightness.Value;
            nudBrightness.Value = trkBrightness.Value;
            ApplyChannelAdjustments();
        }

        // ── NUD handlers — drive the trackbar (which then re-syncs the NUD) ─

        private void nudR_ValueChanged(object sender, EventArgs e)         => trkR.Value         = (int)nudR.Value;
        private void nudG_ValueChanged(object sender, EventArgs e)         => trkG.Value         = (int)nudG.Value;
        private void nudB_ValueChanged(object sender, EventArgs e)         => trkB.Value         = (int)nudB.Value;
        private void nudBrightness_ValueChanged(object sender, EventArgs e) => trkBrightness.Value = (int)nudBrightness.Value;

        private void chkR_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();
        private void chkG_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();
        private void chkB_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();

        // ── Reset All — channel sliders, NUDs, checkboxes, quantization ──

        private void btnResetChannels_Click(object sender, EventArgs e)
        {
            _suppressAdjustments = true;

            // Reset channel sliders (each fires trkX_ValueChanged → syncs its NUD)
            trkR.Value = 0; trkG.Value = 0; trkB.Value = 0; trkBrightness.Value = 0;

            // Uncheck then recheck so state is always refreshed
            chkR.Checked = false; chkR.Checked = true;
            chkG.Checked = false; chkG.Checked = true;
            chkB.Checked = false; chkB.Checked = true;

            // Reset quantization (ApplyQuantization checks _suppressAdjustments)
            trkQuantize.Value = 256;

            _suppressAdjustments = false;

            // Restore original image
            if (pictureBox1.Image != null)
                pictureBox2.Image = new Bitmap(pictureBox1.Image);
        }

        private void ApplyChannelAdjustments()
        {
            if (_suppressAdjustments || pictureBox1.Image == null) return;

            int off0 = trkR.Value, off1 = trkG.Value, off2 = trkB.Value;
            int brightness = trkBrightness.Value;
            bool on0 = chkR.Checked, on1 = chkG.Checked, on2 = chkB.Checked;
            Bitmap src = new Bitmap(pictureBox1.Image);

            switch (_csMode)
            {
                case CsMode.RGB:
                {
                    Bitmap result = new Bitmap(src.Width, src.Height);
                    for (int y = 0; y < src.Height; y++)
                        for (int x = 0; x < src.Width; x++)
                        {
                            Color p = src.GetPixel(x, y);
                            result.SetPixel(x, y, Color.FromArgb(
                                on0 ? Clamp(p.R + off0 + brightness, 0, 255) : 0,
                                on1 ? Clamp(p.G + off1 + brightness, 0, 255) : 0,
                                on2 ? Clamp(p.B + off2 + brightness, 0, 255) : 0));
                        }
                    pictureBox2.Image = result;
                    break;
                }
                case CsMode.CMY:
                {
                    Bitmap result = new Bitmap(src.Width, src.Height);
                    for (int y = 0; y < src.Height; y++)
                        for (int x = 0; x < src.Width; x++)
                        {
                            Color p = src.GetPixel(x, y);
                            int C = on0 ? Clamp(255 - p.R + off0 + brightness, 0, 255) : 0;
                            int M = on1 ? Clamp(255 - p.G + off1 + brightness, 0, 255) : 0;
                            int Y = on2 ? Clamp(255 - p.B + off2 + brightness, 0, 255) : 0;
                            result.SetPixel(x, y, Color.FromArgb(255 - C, 255 - M, 255 - Y));
                        }
                    pictureBox2.Image = result;
                    break;
                }
                // HSV: brightness → Value (channel 2)
                case CsMode.HSV:
                    pictureBox2.Image = ApplyViaEmgu(src, off0, off1, off2, brightness, on0, on1, on2,
                                                     ColorConversion.Bgr2Hsv, ColorConversion.Hsv2Bgr, 2);
                    break;
                // YCbCr/YUV/LAB: brightness → luminance (channel 0)
                case CsMode.YCbCr:
                    pictureBox2.Image = ApplyViaEmgu(src, off0, off1, off2, brightness, on0, on1, on2,
                                                     ColorConversion.Bgr2YCrCb, ColorConversion.YCrCb2Bgr, 0);
                    break;
                case CsMode.YUV:
                    pictureBox2.Image = ApplyViaEmgu(src, off0, off1, off2, brightness, on0, on1, on2,
                                                     ColorConversion.Bgr2Yuv, ColorConversion.Yuv2Bgr, 0);
                    break;
                case CsMode.LAB:
                    pictureBox2.Image = ApplyViaEmgu(src, off0, off1, off2, brightness, on0, on1, on2,
                                                     ColorConversion.Bgr2Lab, ColorConversion.Lab2Bgr, 0);
                    break;
            }
            src.Dispose();
        }

        // Converts src to target color space, applies per-channel offsets, converts back to RGB.
        // brightnessChannel: which channel index (0/1/2) receives the brightness offset.
        private Bitmap ApplyViaEmgu(Bitmap src,
                                     int off0, int off1, int off2, int brightness,
                                     bool on0, bool on1, bool on2,
                                     ColorConversion fwd, ColorConversion inv,
                                     int brightnessChannel)
        {
            Image<Bgr, byte> bgrImg = new Image<Bgr, byte>(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                { Color p = src.GetPixel(x, y); bgrImg[y, x] = new Bgr(p.B, p.G, p.R); }

            Mat targetMat = new Mat();
            CvInvoke.CvtColor(bgrImg.Mat, targetMat, fwd);

            int rows = src.Height, cols = src.Width;
            int step = (int)targetMat.Step;
            byte[] data = new byte[rows * step];
            Marshal.Copy(targetMat.DataPointer, data, 0, rows * step);

            int[] offsets = { off0, off1, off2 };
            bool[] ons    = { on0,  on1,  on2  };
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    int i = y * step + x * 3;
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int brt = (ch == brightnessChannel) ? brightness : 0;
                        data[i + ch] = ons[ch]
                            ? (byte)Clamp(data[i + ch] + offsets[ch] + brt, 0, 255)
                            : (byte)0;
                    }
                }

            Mat modMat = new Mat(rows, cols, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            int modStep = (int)modMat.Step;
            for (int y = 0; y < rows; y++)
                Marshal.Copy(data, y * step, IntPtr.Add(modMat.DataPointer, y * modStep), cols * 3);

            Mat bgrMat = new Mat();
            CvInvoke.CvtColor(modMat, bgrMat, inv);
            Bitmap output = bgrMat.ToBitmap();

            bgrImg.Dispose(); targetMat.Dispose(); modMat.Dispose(); bgrMat.Dispose();
            return output;
        }

        private int Clamp(int v, int min, int max) => Math.Max(min, Math.Min(max, v));

        // ── 2D / 3D Color Space Visualization ────────────────────────────

        private void btnOpen2D_Click(object sender, EventArgs e) => new ColorSpace2DForm().Show();

        private void btnOpen3D_Click(object sender, EventArgs e) => new ColorSpaceForm().Show();

        // ── Color Quantization ────────────────────────────────────────────

        private void trkQuantize_ValueChanged(object sender, EventArgs e)
        {
            int levels = trkQuantize.Value;
            lblQuantize.Text = levels >= 256
                ? "Levels per channel: 256  (no quantization)"
                : $"Levels per channel: {levels}  →  {(long)levels * levels * levels} max colors";
            ApplyQuantization();
        }

        private void btnResetQuantize_Click(object sender, EventArgs e)
        {
            trkQuantize.Value = 256;
            if (pictureBox1.Image != null) pictureBox2.Image = new Bitmap(pictureBox1.Image);
        }

        private void ApplyQuantization()
        {
            if (_suppressAdjustments || pictureBox1.Image == null) return;
            int levels = trkQuantize.Value;
            if (levels >= 256) { pictureBox2.Image = new Bitmap(pictureBox1.Image); return; }

            int step = 256 / levels;
            Bitmap src = new Bitmap(pictureBox1.Image), result = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    Color p = src.GetPixel(x, y);
                    result.SetPixel(x, y, Color.FromArgb(
                        Clamp((p.R / step) * step, 0, 255),
                        Clamp((p.G / step) * step, 0, 255),
                        Clamp((p.B / step) * step, 0, 255)));
                }
            pictureBox2.Image = result; src.Dispose();
        }

        // ── Pixel Inspector ───────────────────────────────────────────────

        private void pictureBox1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;
            Point coord = GetImageCoords(pictureBox1, e.Location);
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Color c = bmp.GetPixel(coord.X, coord.Y);
            bmp.Dispose();

            pnlColorSwatch.BackColor = c;
            lblPixelCoords.Text = $"Pixel: ({coord.X}, {coord.Y})";
            lblPixelRGB.Text    = $"RGB:    ({c.R}, {c.G}, {c.B})";

            try
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(1, 1);
                img[0, 0] = new Bgr(c.B, c.G, c.R);

                Mat m = new Mat();
                byte[] buf = new byte[3];

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Hsv);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                lblPixelHSV.Text = $"HSV:    ({buf[0] * 2}°, {(int)(buf[1] / 255.0 * 100)}%, {(int)(buf[2] / 255.0 * 100)}%)";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2YCrCb);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                lblPixelYCbCr.Text = $"YCbCr: ({buf[0]}, {buf[1]}, {buf[2]})";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Yuv);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                lblPixelYUV.Text = $"YUV:    ({buf[0]}, {buf[1]}, {buf[2]})";

                CvInvoke.CvtColor(img.Mat, m, ColorConversion.Bgr2Lab);
                Marshal.Copy(m.DataPointer, buf, 0, 3);
                lblPixelLAB.Text = $"LAB:    ({buf[0]}, {buf[1]}, {buf[2]})";

                img.Dispose(); m.Dispose();
            }
            catch (Exception ex) { MessageBox.Show("Inspector error: " + ex.Message); }
        }

        private Point GetImageCoords(System.Windows.Forms.PictureBox pb, Point click)
        {
            float imgW = pb.Image.Width, imgH = pb.Image.Height;
            float scale = Math.Min(pb.Width / imgW, pb.Height / imgH);
            float offsetX = (pb.Width  - imgW * scale) / 2f;
            float offsetY = (pb.Height - imgH * scale) / 2f;
            int px = Math.Max(0, Math.Min((int)((click.X - offsetX) / scale), (int)imgW - 1));
            int py = Math.Max(0, Math.Min((int)((click.Y - offsetY) / scale), (int)imgH - 1));
            return new Point(px, py);
        }

        // ── Drag & Drop ───────────────────────────────────────────────────

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
            => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            string fp = files[0], ext = System.IO.Path.GetExtension(fp).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp")
            { MessageBox.Show("Please drop a .jpg, .png, or .bmp file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            Bitmap loaded = new Bitmap(fp);
            pictureBox1.Image = loaded;
            pictureBox2.Image = new Bitmap(loaded);
            var fi = new System.IO.FileInfo(fp);
            lblImageInfo.Text = $"Image: {fi.Name}\nSize: {fi.Length / 1024} KB\n" +
                                $"Dimensions: {loaded.Width} x {loaded.Height}\n" +
                                $"Format: {ext.TrimStart('.').ToUpper()}";
            SetColorSpace(CsMode.RGB);
        }
    }
}
