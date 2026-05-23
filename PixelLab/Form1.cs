using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void lblImageInfo_Load(object sender, EventArgs e)
        {
        }

        private void load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap loadedImage = new Bitmap(openFileDialog.FileName);
                pictureBox1.Image = loadedImage;
                pictureBox2.Image = new Bitmap(loadedImage);

                System.IO.FileInfo fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                string fileName = fileInfo.Name;
                long fileSizeInKB = fileInfo.Length / 1024;
                string format = System.IO.Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToUpper();

                lblImageInfo.Text = $"Image: {fileName}\nSize: {fileSizeInKB} KB\nDimensions: {loadedImage.Width} x {loadedImage.Height}\nFormat: {format}";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox2.Image = new Bitmap(pictureBox1.Image);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save Modified Image";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image.Save(saveFileDialog.FileName);
                    MessageBox.Show("Image saved successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No image to save!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Bitmap blueOnly = new Bitmap(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color p = bmp.GetPixel(x, y);
                        blueOnly.SetPixel(x, y, Color.FromArgb(0, 0, p.B));
                    }
                pictureBox2.Image = blueOnly;
                bmp.Dispose();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap rgbImage = new Bitmap(pictureBox1.Image);
                Bitmap redComponentImage = new Bitmap(rgbImage.Width, rgbImage.Height);

                for (int y = 0; y < rgbImage.Height; y++)
                    for (int x = 0; x < rgbImage.Width; x++)
                    {
                        Color pixelColor = rgbImage.GetPixel(x, y);
                        redComponentImage.SetPixel(x, y, Color.FromArgb(pixelColor.R, 0, 0));
                    }

                pictureBox2.Image = redComponentImage;
                rgbImage.Dispose();
            }
            else
            {
                MessageBox.Show("Please load an image first!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Bitmap greenOnly = new Bitmap(bmp.Width, bmp.Height);
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color p = bmp.GetPixel(x, y);
                        greenOnly.SetPixel(x, y, Color.FromArgb(0, p.G, 0));
                    }
                pictureBox2.Image = greenOnly;
                bmp.Dispose();
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);

                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixelColor = bmp.GetPixel(x, y);
                        img[y, x] = new Bgr(pixelColor.B, pixelColor.G, pixelColor.R);
                    }

                Image<Hsv, byte> hsvImg = img.Convert<Hsv, byte>();
                Image<Bgr, byte> bgrResult = hsvImg.Convert<Bgr, byte>();

                Bitmap resultBmp = new Bitmap(hsvImg.Width, hsvImg.Height);
                for (int y = 0; y < hsvImg.Height; y++)
                    for (int x = 0; x < hsvImg.Width; x++)
                    {
                        var p = bgrResult[y, x];
                        resultBmp.SetPixel(x, y, Color.FromArgb((int)p.Red, (int)p.Green, (int)p.Blue));
                    }

                pictureBox2.Image = resultBmp;

                img.Dispose();
                hsvImg.Dispose();
                bgrResult.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    int C = 255 - p.R;
                    int M = 255 - p.G;
                    int Y = 255 - p.B;
                    result.SetPixel(x, y, Color.FromArgb(C, M, Y));
                }

            pictureBox2.Image = result;
            bmp.Dispose();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);

                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color p = bmp.GetPixel(x, y);
                        img[y, x] = new Bgr(p.B, p.G, p.R);
                    }

                Mat dst = new Mat();
                CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2YCrCb);
                pictureBox2.Image = dst.ToBitmap();

                img.Dispose();
                dst.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);

                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color p = bmp.GetPixel(x, y);
                        img[y, x] = new Bgr(p.B, p.G, p.R);
                    }

                Mat dst = new Mat();
                CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2Yuv);
                pictureBox2.Image = dst.ToBitmap();

                img.Dispose();
                dst.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Image<Bgr, byte> img = new Image<Bgr, byte>(bmp.Width, bmp.Height);

                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color p = bmp.GetPixel(x, y);
                        img[y, x] = new Bgr(p.B, p.G, p.R);
                    }

                Mat dst = new Mat();
                CvInvoke.CvtColor(img.Mat, dst, ColorConversion.Bgr2Lab);
                pictureBox2.Image = dst.ToBitmap();

                img.Dispose();
                dst.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ── Requirement 3: Channel Controls ───────────────────────────────

        private void trkR_ValueChanged(object sender, EventArgs e)
        {
            lblRedOffset.Text = "Red Offset: " + trkR.Value;
            ApplyChannelAdjustments();
        }

        private void trkG_ValueChanged(object sender, EventArgs e)
        {
            lblGreenOffset.Text = "Green Offset: " + trkG.Value;
            ApplyChannelAdjustments();
        }

        private void trkB_ValueChanged(object sender, EventArgs e)
        {
            lblBlueOffset.Text = "Blue Offset: " + trkB.Value;
            ApplyChannelAdjustments();
        }

        private void trkBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = "Brightness: " + trkBrightness.Value;
            ApplyChannelAdjustments();
        }

        private void chkR_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();
        private void chkG_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();
        private void chkB_CheckedChanged(object sender, EventArgs e) => ApplyChannelAdjustments();

        private void btnResetChannels_Click(object sender, EventArgs e)
        {
            trkR.Value = 0;
            trkG.Value = 0;
            trkB.Value = 0;
            trkBrightness.Value = 0;
            chkR.Checked = true;
            chkG.Checked = true;
            chkB.Checked = true;
            if (pictureBox1.Image != null)
                pictureBox2.Image = new Bitmap(pictureBox1.Image);
        }

        private void ApplyChannelAdjustments()
        {
            if (pictureBox1.Image == null) return;

            Bitmap src = new Bitmap(pictureBox1.Image);
            Bitmap result = new Bitmap(src.Width, src.Height);

            int rOffset = trkR.Value;
            int gOffset = trkG.Value;
            int bOffset = trkB.Value;
            int brightness = trkBrightness.Value;
            bool rOn = chkR.Checked;
            bool gOn = chkG.Checked;
            bool bOn = chkB.Checked;

            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    Color p = src.GetPixel(x, y);
                    int r = rOn ? Clamp(p.R + rOffset + brightness, 0, 255) : 0;
                    int g = gOn ? Clamp(p.G + gOffset + brightness, 0, 255) : 0;
                    int b = bOn ? Clamp(p.B + bOffset + brightness, 0, 255) : 0;
                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }

            pictureBox2.Image = result;
            src.Dispose();
        }

        private int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        // ── Requirement 4: 3D Color Space Visualization ───────────────────

        private void btnOpen3D_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }
            ColorSpaceForm csf = new ColorSpaceForm(new Bitmap(pictureBox1.Image));
            csf.Show();
        }

        // ── Requirement 7: Color Quantization ─────────────────────────────

        private void trkQuantize_ValueChanged(object sender, EventArgs e)
        {
            int levels = trkQuantize.Value;
            if (levels >= 256)
                lblQuantize.Text = "Levels per channel: 256  (no quantization)";
            else
            {
                long maxColors = (long)levels * levels * levels;
                lblQuantize.Text = $"Levels per channel: {levels}  →  {maxColors} max colors";
            }
            ApplyQuantization();
        }

        private void btnResetQuantize_Click(object sender, EventArgs e)
        {
            trkQuantize.Value = 256;
            if (pictureBox1.Image != null)
                pictureBox2.Image = new Bitmap(pictureBox1.Image);
        }

        private void ApplyQuantization()
        {
            if (pictureBox1.Image == null) return;

            int levels = trkQuantize.Value;
            if (levels >= 256)
            {
                pictureBox2.Image = new Bitmap(pictureBox1.Image);
                return;
            }

            int step = 256 / levels;
            Bitmap src = new Bitmap(pictureBox1.Image);
            Bitmap result = new Bitmap(src.Width, src.Height);

            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    Color p = src.GetPixel(x, y);
                    int r = Clamp((p.R / step) * step, 0, 255);
                    int g = Clamp((p.G / step) * step, 0, 255);
                    int b = Clamp((p.B / step) * step, 0, 255);
                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }

            pictureBox2.Image = result;
            src.Dispose();
        }

        // ── Requirement 5: Pixel Inspector ────────────────────────────────

        private void pictureBox1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;

            Point imgCoord = GetImageCoords(pictureBox1, e.Location);
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Color c = bmp.GetPixel(imgCoord.X, imgCoord.Y);
            bmp.Dispose();

            pnlColorSwatch.BackColor = c;
            lblPixelCoords.Text = $"Pixel: ({imgCoord.X}, {imgCoord.Y})";
            lblPixelRGB.Text    = $"RGB:    ({c.R}, {c.G}, {c.B})";

            try
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(1, 1);
                img[0, 0] = new Bgr(c.B, c.G, c.R);

                // HSV — display as degrees / percent / percent
                Mat hsvMat = new Mat();
                CvInvoke.CvtColor(img.Mat, hsvMat, ColorConversion.Bgr2Hsv);
                byte[] hsv = new byte[3];
                System.Runtime.InteropServices.Marshal.Copy(hsvMat.DataPointer, hsv, 0, 3);
                int hDeg = hsv[0] * 2;                         // OpenCV stores H as 0-180
                int sPct = (int)(hsv[1] / 255.0 * 100);
                int vPct = (int)(hsv[2] / 255.0 * 100);
                lblPixelHSV.Text = $"HSV:    ({hDeg}°, {sPct}%, {vPct}%)";
                hsvMat.Dispose();

                // YCbCr
                Mat ycbcrMat = new Mat();
                CvInvoke.CvtColor(img.Mat, ycbcrMat, ColorConversion.Bgr2YCrCb);
                byte[] ycbcr = new byte[3];
                System.Runtime.InteropServices.Marshal.Copy(ycbcrMat.DataPointer, ycbcr, 0, 3);
                lblPixelYCbCr.Text = $"YCbCr: ({ycbcr[0]}, {ycbcr[1]}, {ycbcr[2]})";
                ycbcrMat.Dispose();

                // YUV
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(img.Mat, yuvMat, ColorConversion.Bgr2Yuv);
                byte[] yuv = new byte[3];
                System.Runtime.InteropServices.Marshal.Copy(yuvMat.DataPointer, yuv, 0, 3);
                lblPixelYUV.Text = $"YUV:    ({yuv[0]}, {yuv[1]}, {yuv[2]})";
                yuvMat.Dispose();

                // LAB
                Mat labMat = new Mat();
                CvInvoke.CvtColor(img.Mat, labMat, ColorConversion.Bgr2Lab);
                byte[] lab = new byte[3];
                System.Runtime.InteropServices.Marshal.Copy(labMat.DataPointer, lab, 0, 3);
                lblPixelLAB.Text = $"LAB:    ({lab[0]}, {lab[1]}, {lab[2]})";
                labMat.Dispose();

                img.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inspector error: " + ex.Message);
            }
        }

        // Maps a click position on a Zoom-mode PictureBox to the actual image pixel
        private Point GetImageCoords(System.Windows.Forms.PictureBox pb, Point click)
        {
            float imgW = pb.Image.Width;
            float imgH = pb.Image.Height;
            float scale = Math.Min(pb.Width / imgW, pb.Height / imgH);
            float displayW = imgW * scale;
            float displayH = imgH * scale;
            float offsetX = (pb.Width  - displayW) / 2f;
            float offsetY = (pb.Height - displayH) / 2f;
            int px = (int)((click.X - offsetX) / scale);
            int py = (int)((click.Y - offsetY) / scale);
            px = Math.Max(0, Math.Min(px, (int)imgW - 1));
            py = Math.Max(0, Math.Min(py, (int)imgH - 1));
            return new Point(px, py);
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            string filePath = files[0];
            string ext = System.IO.Path.GetExtension(filePath).ToLower();

            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
            {
                Bitmap loadedImage = new Bitmap(filePath);
                pictureBox1.Image = loadedImage;
                pictureBox2.Image = new Bitmap(loadedImage);

                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                string fileName = fileInfo.Name;
                long fileSizeInKB = fileInfo.Length / 1024;
                string format = System.IO.Path.GetExtension(filePath).TrimStart('.').ToUpper();

                lblImageInfo.Text = $"Image: {fileName}\nSize: {fileSizeInKB} KB\nDimensions: {loadedImage.Width} x {loadedImage.Height}\nFormat: {format}";
            }
            else
            {
                MessageBox.Show("Please drop a .jpg, .png, or .bmp file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
