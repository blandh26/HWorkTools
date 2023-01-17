namespace H_ScreenCapture
{
    partial class FrmCapture
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.sizeTrackBar1 = new H_ScreenCapture.SizeTrackBar();
            this.ckbox_mosaic = new System.Windows.Forms.CheckBox();
            this.colorBox1 = new H_ScreenCapture.ColorBox();
            this.captureToolbar1 = new H_ScreenCapture.CaptureToolbar();
            this.imageCroppingBox1 = new H_ScreenCapture.ImageCroppingBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.sizeTrackBar1);
            this.panel1.Controls.Add(this.ckbox_mosaic);
            this.panel1.Controls.Add(this.colorBox1);
            this.panel1.Location = new System.Drawing.Point(42, 137);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(373, 89);
            this.panel1.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.Color.Red;
            this.textBox1.Location = new System.Drawing.Point(29, 54);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 18);
            this.textBox1.TabIndex = 7;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Black;
            this.linkLabel1.LinkColor = System.Drawing.Color.Black;
            this.linkLabel1.Location = new System.Drawing.Point(27, 54);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(53, 12);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "选择字体";
            // 
            // sizeTrackBar1
            // 
            this.sizeTrackBar1.BackColor = System.Drawing.Color.Transparent;
            this.sizeTrackBar1.Color = System.Drawing.Color.Red;
            this.sizeTrackBar1.Location = new System.Drawing.Point(188, 45);
            this.sizeTrackBar1.MaxValue = 30;
            this.sizeTrackBar1.MinValue = 1;
            this.sizeTrackBar1.Name = "sizeTrackBar1";
            this.sizeTrackBar1.Size = new System.Drawing.Size(182, 18);
            this.sizeTrackBar1.TabIndex = 7;
            this.sizeTrackBar1.Text = "sizeTrackBar1";
            this.sizeTrackBar1.Value = 1;
            // 
            // ckbox_mosaic
            // 
            this.ckbox_mosaic.AutoSize = true;
            this.ckbox_mosaic.BackColor = System.Drawing.Color.Transparent;
            this.ckbox_mosaic.ForeColor = System.Drawing.Color.White;
            this.ckbox_mosaic.Location = new System.Drawing.Point(298, 24);
            this.ckbox_mosaic.Name = "ckbox_mosaic";
            this.ckbox_mosaic.Size = new System.Drawing.Size(60, 16);
            this.ckbox_mosaic.TabIndex = 6;
            this.ckbox_mosaic.Text = "马赛克";
            this.ckbox_mosaic.UseVisualStyleBackColor = false;
            // 
            // colorBox1
            // 
            this.colorBox1.Alpha = ((byte)(255));
            this.colorBox1.BackColor = System.Drawing.Color.Transparent;
            this.colorBox1.Color = System.Drawing.Color.Red;
            this.colorBox1.Location = new System.Drawing.Point(3, 3);
            this.colorBox1.Name = "colorBox1";
            this.colorBox1.Size = new System.Drawing.Size(154, 45);
            this.colorBox1.TabIndex = 2;
            this.colorBox1.Text = "colorBox1";
            // 
            // captureToolbar1
            // 
            this.captureToolbar1.BackColor = System.Drawing.Color.White;
            this.captureToolbar1.Location = new System.Drawing.Point(42, 90);
            this.captureToolbar1.Name = "captureToolbar1";
            this.captureToolbar1.Size = new System.Drawing.Size(594, 41);
            this.captureToolbar1.TabIndex = 1;
            this.captureToolbar1.ToolButtonClick += new System.EventHandler(this.captureToolbar1_ToolButtonClick);
            // 
            // imageCroppingBox1
            // 
            this.imageCroppingBox1.BackColor = System.Drawing.Color.Black;
            this.imageCroppingBox1.Image = null;
            this.imageCroppingBox1.IsDrawMagnifier = false;
            this.imageCroppingBox1.IsLockSelected = false;
            this.imageCroppingBox1.IsSetClip = true;
            this.imageCroppingBox1.Location = new System.Drawing.Point(9, 10);
            this.imageCroppingBox1.MaskColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.imageCroppingBox1.Name = "imageCroppingBox1";
            this.imageCroppingBox1.PreViewRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.imageCroppingBox1.SelectedRectangle = new System.Drawing.Rectangle(0, 0, 1, 1);
            this.imageCroppingBox1.Size = new System.Drawing.Size(517, 358);
            this.imageCroppingBox1.TabIndex = 0;
            this.imageCroppingBox1.Text = "imageCroppingBox1";
            this.imageCroppingBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.imageCroppingBox1_Paint);
            this.imageCroppingBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.imageCroppingBox1_KeyDown);
            this.imageCroppingBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseDoubleClick);
            this.imageCroppingBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseDown);
            this.imageCroppingBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseMove);
            this.imageCroppingBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseUp);
            // 
            // FrmCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 253);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.captureToolbar1);
            this.Controls.Add(this.imageCroppingBox1);
            this.Name = "FrmCapture";
            this.Text = "FrmCaption";
            this.Load += new System.EventHandler(this.FrmCaption_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ImageCroppingBox imageCroppingBox1;
        private CaptureToolbar captureToolbar1;
        private ColorBox colorBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox ckbox_mosaic;
        private SizeTrackBar sizeTrackBar1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;

    }
}