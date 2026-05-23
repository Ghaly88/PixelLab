namespace PixelLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.load = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.lblImageInfo = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnRGB = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            // Channel Controls
            this.grpChannels = new System.Windows.Forms.GroupBox();
            this.lblRedOffset = new System.Windows.Forms.Label();
            this.trkR = new System.Windows.Forms.TrackBar();
            this.chkR = new System.Windows.Forms.CheckBox();
            this.lblGreenOffset = new System.Windows.Forms.Label();
            this.trkG = new System.Windows.Forms.TrackBar();
            this.chkG = new System.Windows.Forms.CheckBox();
            this.lblBlueOffset = new System.Windows.Forms.Label();
            this.trkB = new System.Windows.Forms.TrackBar();
            this.chkB = new System.Windows.Forms.CheckBox();
            this.lblBrightness = new System.Windows.Forms.Label();
            this.trkBrightness = new System.Windows.Forms.TrackBar();
            this.btnResetChannels = new System.Windows.Forms.Button();
            // Pixel Inspector
            this.grpPixelInfo = new System.Windows.Forms.GroupBox();
            this.lblInspectHint = new System.Windows.Forms.Label();
            this.pnlColorSwatch = new System.Windows.Forms.Panel();
            this.lblPixelCoords = new System.Windows.Forms.Label();
            this.lblPixelRGB = new System.Windows.Forms.Label();
            this.lblPixelHSV = new System.Windows.Forms.Label();
            this.lblPixelYCbCr = new System.Windows.Forms.Label();
            this.lblPixelYUV = new System.Windows.Forms.Label();
            this.lblPixelLAB = new System.Windows.Forms.Label();
            // Color Quantization
            this.grpQuantize = new System.Windows.Forms.GroupBox();
            this.lblQuantize = new System.Windows.Forms.Label();
            this.trkQuantize = new System.Windows.Forms.TrackBar();
            this.btnResetQuantize = new System.Windows.Forms.Button();
            this.btnOpen2D = new System.Windows.Forms.Button();
            this.btnOpen3D = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkQuantize)).BeginInit();
            this.grpChannels.SuspendLayout();
            this.grpPixelInfo.SuspendLayout();
            this.grpQuantize.SuspendLayout();
            this.SuspendLayout();

            // ── load ──────────────────────────────────────────────────────
            this.load.Location = new System.Drawing.Point(1202, 608);
            this.load.Name = "load";
            this.load.Size = new System.Drawing.Size(75, 23);
            this.load.Text = "load";
            this.load.UseVisualStyleBackColor = true;
            this.load.Click += new System.EventHandler(this.load_Click);

            // ── label1 ────────────────────────────────────────────────────
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 495);
            this.label1.Name = "label1";

            // ── pictureBox1 ───────────────────────────────────────────────
            this.pictureBox1.BackColor = System.Drawing.Color.SeaShell;
            this.pictureBox1.Location = new System.Drawing.Point(21, 53);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(621, 398);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox1.AllowDrop = true;
            this.pictureBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragEnter);
            this.pictureBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragDrop);
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);

            // ── button2 (save) ────────────────────────────────────────────
            this.button2.Location = new System.Drawing.Point(995, 608);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.Text = "save";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);

            // ── button3 (reset image) ─────────────────────────────────────
            this.button3.Location = new System.Drawing.Point(1094, 608);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.Text = "reset";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);

            // ── lblImageInfo ──────────────────────────────────────────────
            this.lblImageInfo.AutoSize = true;
            this.lblImageInfo.Location = new System.Drawing.Point(32, 462);
            this.lblImageInfo.Name = "lblImageInfo";
            this.lblImageInfo.Text = "No image loaded";

            // ── pictureBox2 ───────────────────────────────────────────────
            this.pictureBox2.Location = new System.Drawing.Point(667, 53);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(597, 398);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabStop = false;

            // ── color space buttons ────────────────────────────────────────
            this.btnRGB.Location = new System.Drawing.Point(35, 587);
            this.btnRGB.Name = "btnRGB";
            this.btnRGB.Size = new System.Drawing.Size(75, 23);
            this.btnRGB.Text = "RGB";
            this.btnRGB.UseVisualStyleBackColor = true;
            this.btnRGB.Click += new System.EventHandler(this.btnRGB_Click);

            this.button7.Location = new System.Drawing.Point(117, 587);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.Text = "CMY";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);

            this.button6.Location = new System.Drawing.Point(199, 587);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.Text = "HSV";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click_1);

            this.button8.Location = new System.Drawing.Point(281, 587);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.Text = "YCbCr";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);

            this.button9.Location = new System.Drawing.Point(363, 587);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.Text = "YUV";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);

            this.button10.Location = new System.Drawing.Point(445, 587);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.Text = "LAB";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);

            // ══ Channel Controls GroupBox ══════════════════════════════════

            this.lblRedOffset.AutoSize = true;
            this.lblRedOffset.Location = new System.Drawing.Point(8, 22);
            this.lblRedOffset.Name = "lblRedOffset";
            this.lblRedOffset.Text = "Red Offset: 0";

            this.trkR.Location = new System.Drawing.Point(8, 40);
            this.trkR.Minimum = -255;
            this.trkR.Maximum = 255;
            this.trkR.Value = 0;
            this.trkR.TickFrequency = 51;
            this.trkR.SmallChange = 5;
            this.trkR.LargeChange = 25;
            this.trkR.Size = new System.Drawing.Size(175, 45);
            this.trkR.Name = "trkR";
            this.trkR.ValueChanged += new System.EventHandler(this.trkR_ValueChanged);

            this.chkR.AutoSize = true;
            this.chkR.Checked = true;
            this.chkR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkR.Location = new System.Drawing.Point(192, 50);
            this.chkR.Name = "chkR";
            this.chkR.Text = "R On";
            this.chkR.CheckedChanged += new System.EventHandler(this.chkR_CheckedChanged);

            this.lblGreenOffset.AutoSize = true;
            this.lblGreenOffset.Location = new System.Drawing.Point(8, 92);
            this.lblGreenOffset.Name = "lblGreenOffset";
            this.lblGreenOffset.Text = "Green Offset: 0";

            this.trkG.Location = new System.Drawing.Point(8, 110);
            this.trkG.Minimum = -255;
            this.trkG.Maximum = 255;
            this.trkG.Value = 0;
            this.trkG.TickFrequency = 51;
            this.trkG.SmallChange = 5;
            this.trkG.LargeChange = 25;
            this.trkG.Size = new System.Drawing.Size(175, 45);
            this.trkG.Name = "trkG";
            this.trkG.ValueChanged += new System.EventHandler(this.trkG_ValueChanged);

            this.chkG.AutoSize = true;
            this.chkG.Checked = true;
            this.chkG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkG.Location = new System.Drawing.Point(192, 120);
            this.chkG.Name = "chkG";
            this.chkG.Text = "G On";
            this.chkG.CheckedChanged += new System.EventHandler(this.chkG_CheckedChanged);

            this.lblBlueOffset.AutoSize = true;
            this.lblBlueOffset.Location = new System.Drawing.Point(8, 162);
            this.lblBlueOffset.Name = "lblBlueOffset";
            this.lblBlueOffset.Text = "Blue Offset: 0";

            this.trkB.Location = new System.Drawing.Point(8, 180);
            this.trkB.Minimum = -255;
            this.trkB.Maximum = 255;
            this.trkB.Value = 0;
            this.trkB.TickFrequency = 51;
            this.trkB.SmallChange = 5;
            this.trkB.LargeChange = 25;
            this.trkB.Size = new System.Drawing.Size(175, 45);
            this.trkB.Name = "trkB";
            this.trkB.ValueChanged += new System.EventHandler(this.trkB_ValueChanged);

            this.chkB.AutoSize = true;
            this.chkB.Checked = true;
            this.chkB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkB.Location = new System.Drawing.Point(192, 190);
            this.chkB.Name = "chkB";
            this.chkB.Text = "B On";
            this.chkB.CheckedChanged += new System.EventHandler(this.chkB_CheckedChanged);

            this.lblBrightness.AutoSize = true;
            this.lblBrightness.Location = new System.Drawing.Point(8, 232);
            this.lblBrightness.Name = "lblBrightness";
            this.lblBrightness.Text = "Brightness: 0";

            this.trkBrightness.Location = new System.Drawing.Point(8, 250);
            this.trkBrightness.Minimum = -255;
            this.trkBrightness.Maximum = 255;
            this.trkBrightness.Value = 0;
            this.trkBrightness.TickFrequency = 51;
            this.trkBrightness.SmallChange = 5;
            this.trkBrightness.LargeChange = 25;
            this.trkBrightness.Size = new System.Drawing.Size(240, 45);
            this.trkBrightness.Name = "trkBrightness";
            this.trkBrightness.ValueChanged += new System.EventHandler(this.trkBrightness_ValueChanged);

            this.btnResetChannels.Location = new System.Drawing.Point(8, 305);
            this.btnResetChannels.Name = "btnResetChannels";
            this.btnResetChannels.Size = new System.Drawing.Size(240, 26);
            this.btnResetChannels.Text = "Reset Channel Sliders";
            this.btnResetChannels.UseVisualStyleBackColor = true;
            this.btnResetChannels.Click += new System.EventHandler(this.btnResetChannels_Click);

            this.grpChannels.Controls.Add(this.lblRedOffset);
            this.grpChannels.Controls.Add(this.trkR);
            this.grpChannels.Controls.Add(this.chkR);
            this.grpChannels.Controls.Add(this.lblGreenOffset);
            this.grpChannels.Controls.Add(this.trkG);
            this.grpChannels.Controls.Add(this.chkG);
            this.grpChannels.Controls.Add(this.lblBlueOffset);
            this.grpChannels.Controls.Add(this.trkB);
            this.grpChannels.Controls.Add(this.chkB);
            this.grpChannels.Controls.Add(this.lblBrightness);
            this.grpChannels.Controls.Add(this.trkBrightness);
            this.grpChannels.Controls.Add(this.btnResetChannels);
            this.grpChannels.Location = new System.Drawing.Point(1275, 53);
            this.grpChannels.Name = "grpChannels";
            this.grpChannels.Size = new System.Drawing.Size(260, 345);
            this.grpChannels.TabStop = false;
            this.grpChannels.Text = "Channel Controls";

            // ══ Pixel Inspector GroupBox ═══════════════════════════════════

            // Row 1: hint text (full width, no overlap)
            this.lblInspectHint.AutoSize = true;
            this.lblInspectHint.Location = new System.Drawing.Point(8, 20);
            this.lblInspectHint.Name = "lblInspectHint";
            this.lblInspectHint.Text = "Click original image to inspect";

            // Row 2: colour swatch + pixel coordinates side by side
            this.pnlColorSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlColorSwatch.Location = new System.Drawing.Point(8, 42);
            this.pnlColorSwatch.Name = "pnlColorSwatch";
            this.pnlColorSwatch.Size = new System.Drawing.Size(44, 22);
            this.pnlColorSwatch.BackColor = System.Drawing.SystemColors.ControlDark;

            this.lblPixelCoords.AutoSize = true;
            this.lblPixelCoords.Location = new System.Drawing.Point(58, 46);
            this.lblPixelCoords.Name = "lblPixelCoords";
            this.lblPixelCoords.Text = "Pixel: -";

            // Rows 3-7: one color space per line
            this.lblPixelRGB.AutoSize = true;
            this.lblPixelRGB.Location = new System.Drawing.Point(8, 72);
            this.lblPixelRGB.Name = "lblPixelRGB";
            this.lblPixelRGB.Text = "RGB:    -";

            this.lblPixelHSV.AutoSize = true;
            this.lblPixelHSV.Location = new System.Drawing.Point(8, 92);
            this.lblPixelHSV.Name = "lblPixelHSV";
            this.lblPixelHSV.Text = "HSV:    -";

            this.lblPixelYCbCr.AutoSize = true;
            this.lblPixelYCbCr.Location = new System.Drawing.Point(8, 112);
            this.lblPixelYCbCr.Name = "lblPixelYCbCr";
            this.lblPixelYCbCr.Text = "YCbCr: -";

            this.lblPixelYUV.AutoSize = true;
            this.lblPixelYUV.Location = new System.Drawing.Point(8, 132);
            this.lblPixelYUV.Name = "lblPixelYUV";
            this.lblPixelYUV.Text = "YUV:    -";

            this.lblPixelLAB.AutoSize = true;
            this.lblPixelLAB.Location = new System.Drawing.Point(8, 152);
            this.lblPixelLAB.Name = "lblPixelLAB";
            this.lblPixelLAB.Text = "LAB:    -";

            this.grpPixelInfo.Controls.Add(this.lblInspectHint);
            this.grpPixelInfo.Controls.Add(this.pnlColorSwatch);
            this.grpPixelInfo.Controls.Add(this.lblPixelCoords);
            this.grpPixelInfo.Controls.Add(this.lblPixelRGB);
            this.grpPixelInfo.Controls.Add(this.lblPixelHSV);
            this.grpPixelInfo.Controls.Add(this.lblPixelYCbCr);
            this.grpPixelInfo.Controls.Add(this.lblPixelYUV);
            this.grpPixelInfo.Controls.Add(this.lblPixelLAB);
            this.grpPixelInfo.Location = new System.Drawing.Point(1275, 405);
            this.grpPixelInfo.Name = "grpPixelInfo";
            this.grpPixelInfo.Size = new System.Drawing.Size(260, 178);
            this.grpPixelInfo.TabStop = false;
            this.grpPixelInfo.Text = "Pixel Inspector";

            // ══ Color Quantization GroupBox ════════════════════════════════

            this.lblQuantize.AutoSize = true;
            this.lblQuantize.Location = new System.Drawing.Point(8, 22);
            this.lblQuantize.Name = "lblQuantize";
            this.lblQuantize.Text = "Levels per channel: 256  (no quantization)";

            this.trkQuantize.Location = new System.Drawing.Point(8, 42);
            this.trkQuantize.Minimum = 2;
            this.trkQuantize.Maximum = 256;
            this.trkQuantize.Value = 256;
            this.trkQuantize.TickFrequency = 32;
            this.trkQuantize.SmallChange = 1;
            this.trkQuantize.LargeChange = 8;
            this.trkQuantize.Size = new System.Drawing.Size(240, 45);
            this.trkQuantize.Name = "trkQuantize";
            this.trkQuantize.ValueChanged += new System.EventHandler(this.trkQuantize_ValueChanged);

            this.btnResetQuantize.Location = new System.Drawing.Point(8, 95);
            this.btnResetQuantize.Name = "btnResetQuantize";
            this.btnResetQuantize.Size = new System.Drawing.Size(240, 24);
            this.btnResetQuantize.Text = "Reset Quantization";
            this.btnResetQuantize.UseVisualStyleBackColor = true;
            this.btnResetQuantize.Click += new System.EventHandler(this.btnResetQuantize_Click);

            this.btnOpen2D.Location = new System.Drawing.Point(700, 608);
            this.btnOpen2D.Name = "btnOpen2D";
            this.btnOpen2D.Size = new System.Drawing.Size(90, 23);
            this.btnOpen2D.Text = "2D View";
            this.btnOpen2D.UseVisualStyleBackColor = true;
            this.btnOpen2D.Click += new System.EventHandler(this.btnOpen2D_Click);

            this.btnOpen3D.Location = new System.Drawing.Point(800, 608);
            this.btnOpen3D.Name = "btnOpen3D";
            this.btnOpen3D.Size = new System.Drawing.Size(90, 23);
            this.btnOpen3D.Text = "3D View";
            this.btnOpen3D.UseVisualStyleBackColor = true;
            this.btnOpen3D.Click += new System.EventHandler(this.btnOpen3D_Click);

            this.grpQuantize.Controls.Add(this.lblQuantize);
            this.grpQuantize.Controls.Add(this.trkQuantize);
            this.grpQuantize.Controls.Add(this.btnResetQuantize);
            this.grpQuantize.Location = new System.Drawing.Point(700, 462);
            this.grpQuantize.Name = "grpQuantize";
            this.grpQuantize.Size = new System.Drawing.Size(260, 128);
            this.grpQuantize.TabStop = false;
            this.grpQuantize.Text = "Color Quantization";

            // ── Form ──────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1548, 645);
            this.Controls.Add(this.btnRGB);
            this.Controls.Add(this.btnOpen2D);
            this.Controls.Add(this.btnOpen3D);
            this.Controls.Add(this.grpQuantize);
            this.Controls.Add(this.grpPixelInfo);
            this.Controls.Add(this.grpChannels);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.lblImageInfo);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.load);
            this.Name = "Form1";
            this.Text = "PixelLab";
            this.Load += new System.EventHandler(this.lblImageInfo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkQuantize)).EndInit();
            this.grpChannels.ResumeLayout(false);
            this.grpChannels.PerformLayout();
            this.grpPixelInfo.ResumeLayout(false);
            this.grpPixelInfo.PerformLayout();
            this.grpQuantize.ResumeLayout(false);
            this.grpQuantize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button load;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label lblImageInfo;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        // Channel Controls
        private System.Windows.Forms.GroupBox grpChannels;
        private System.Windows.Forms.Label lblRedOffset;
        private System.Windows.Forms.TrackBar trkR;
        private System.Windows.Forms.CheckBox chkR;
        private System.Windows.Forms.Label lblGreenOffset;
        private System.Windows.Forms.TrackBar trkG;
        private System.Windows.Forms.CheckBox chkG;
        private System.Windows.Forms.Label lblBlueOffset;
        private System.Windows.Forms.TrackBar trkB;
        private System.Windows.Forms.CheckBox chkB;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.TrackBar trkBrightness;
        private System.Windows.Forms.Button btnResetChannels;
        // Pixel Inspector
        private System.Windows.Forms.GroupBox grpPixelInfo;
        private System.Windows.Forms.Label lblInspectHint;
        private System.Windows.Forms.Panel pnlColorSwatch;
        private System.Windows.Forms.Label lblPixelCoords;
        private System.Windows.Forms.Label lblPixelRGB;
        private System.Windows.Forms.Label lblPixelHSV;
        private System.Windows.Forms.Label lblPixelYCbCr;
        private System.Windows.Forms.Label lblPixelYUV;
        private System.Windows.Forms.Label lblPixelLAB;
        // Color Quantization
        private System.Windows.Forms.GroupBox grpQuantize;
        private System.Windows.Forms.Label lblQuantize;
        private System.Windows.Forms.TrackBar trkQuantize;
        private System.Windows.Forms.Button btnResetQuantize;
        private System.Windows.Forms.Button btnRGB;
        private System.Windows.Forms.Button btnOpen2D;
        private System.Windows.Forms.Button btnOpen3D;
    }
}
