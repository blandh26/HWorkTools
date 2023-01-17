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
            this.toolButton2 = new H_ScreenCapture.ToolButton();
            this.toolButton1 = new H_ScreenCapture.ToolButton();
            this.sizeTrackBar1 = new H_ScreenCapture.SizeTrackBar();
            this.colorBox1 = new H_ScreenCapture.ColorBox();
            this.captureToolbar1 = new H_ScreenCapture.CaptureToolbar();
            this.imageCroppingBox1 = new H_ScreenCapture.ImageCroppingBox();
            this.toolButton3 = new H_ScreenCapture.ToolButton();
            this.toolButton4 = new H_ScreenCapture.ToolButton();
            this.toolButton5 = new H_ScreenCapture.ToolButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.toolButton5);
            this.panel1.Controls.Add(this.toolButton4);
            this.panel1.Controls.Add(this.toolButton3);
            this.panel1.Controls.Add(this.toolButton2);
            this.panel1.Controls.Add(this.toolButton1);
            this.panel1.Controls.Add(this.sizeTrackBar1);
            this.panel1.Controls.Add(this.colorBox1);
            this.panel1.Location = new System.Drawing.Point(42, 137);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(679, 204);
            this.panel1.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.ForeColor = System.Drawing.Color.Red;
            this.textBox1.Location = new System.Drawing.Point(28, 26);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 18);
            this.textBox1.TabIndex = 7;
            // 
            // toolButton2
            // 
            this.toolButton2.BtnImage = null;
            this.toolButton2.IsSelected = false;
            this.toolButton2.IsSelectedBtn = true;
            this.toolButton2.IsSingleSelectedBtn = true;
            this.toolButton2.Location = new System.Drawing.Point(295, 15);
            this.toolButton2.Name = "toolButton2";
            this.toolButton2.Size = new System.Drawing.Size(92, 21);
            this.toolButton2.TabIndex = 9;
            this.toolButton2.Text = "toolButton2";
            // 
            // toolButton1
            // 
            this.toolButton1.BtnImage = null;
            this.toolButton1.IsSelected = true;
            this.toolButton1.IsSelectedBtn = true;
            this.toolButton1.IsSingleSelectedBtn = true;
            this.toolButton1.Location = new System.Drawing.Point(182, 15);
            this.toolButton1.Name = "toolButton1";
            this.toolButton1.Size = new System.Drawing.Size(92, 21);
            this.toolButton1.TabIndex = 8;
            this.toolButton1.Text = "toolButton1";
            // 
            // sizeTrackBar1
            // 
            this.sizeTrackBar1.BackColor = System.Drawing.Color.Transparent;
            this.sizeTrackBar1.Color = System.Drawing.Color.Red;
            this.sizeTrackBar1.Location = new System.Drawing.Point(55, 84);
            this.sizeTrackBar1.MaxValue = 30;
            this.sizeTrackBar1.MinValue = 1;
            this.sizeTrackBar1.Name = "sizeTrackBar1";
            this.sizeTrackBar1.Size = new System.Drawing.Size(180, 45);
            this.sizeTrackBar1.TabIndex = 7;
            this.sizeTrackBar1.Text = "sizeTrackBar1";
            this.sizeTrackBar1.Value = 1;
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
            this.captureToolbar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
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
            this.imageCroppingBox1.Size = new System.Drawing.Size(1055, 449);
            this.imageCroppingBox1.TabIndex = 0;
            this.imageCroppingBox1.Text = "imageCroppingBox1";
            this.imageCroppingBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.imageCroppingBox1_Paint);
            this.imageCroppingBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.imageCroppingBox1_KeyDown);
            this.imageCroppingBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseDoubleClick);
            this.imageCroppingBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseDown);
            this.imageCroppingBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseMove);
            this.imageCroppingBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imageCroppingBox1_MouseUp);
            // 
            // toolButton3
            // 
            this.toolButton3.BtnImage = null;
            this.toolButton3.IsSelected = false;
            this.toolButton3.IsSelectedBtn = true;
            this.toolButton3.IsSingleSelectedBtn = true;
            this.toolButton3.Location = new System.Drawing.Point(393, 15);
            this.toolButton3.Name = "toolButton3";
            this.toolButton3.Size = new System.Drawing.Size(92, 21);
            this.toolButton3.TabIndex = 9;
            this.toolButton3.Text = "toolButton2";
            // 
            // toolButton4
            // 
            this.toolButton4.BtnImage = null;
            this.toolButton4.IsSelected = false;
            this.toolButton4.IsSelectedBtn = true;
            this.toolButton4.IsSingleSelectedBtn = true;
            this.toolButton4.Location = new System.Drawing.Point(491, 15);
            this.toolButton4.Name = "toolButton4";
            this.toolButton4.Size = new System.Drawing.Size(92, 21);
            this.toolButton4.TabIndex = 9;
            this.toolButton4.Text = "toolButton2";
            // 
            // toolButton5
            // 
            this.toolButton5.BtnImage = null;
            this.toolButton5.IsSelected = false;
            this.toolButton5.IsSelectedBtn = true;
            this.toolButton5.IsSingleSelectedBtn = true;
            this.toolButton5.Location = new System.Drawing.Point(602, 15);
            this.toolButton5.Name = "toolButton5";
            this.toolButton5.Size = new System.Drawing.Size(92, 21);
            this.toolButton5.TabIndex = 9;
            this.toolButton5.Text = "toolButton2";
            // 
            // FrmCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 497);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.captureToolbar1);
            this.Controls.Add(this.imageCroppingBox1);
            this.Name = "FrmCapture";
            this.Text = "FrmCaption";
            this.Load += new System.EventHandler(this.FrmCaption_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ImageCroppingBox imageCroppingBox1;
        private CaptureToolbar captureToolbar1;
        private ColorBox colorBox1;
        private System.Windows.Forms.Panel panel1;
        private SizeTrackBar sizeTrackBar1;
        private System.Windows.Forms.TextBox textBox1;
        private ToolButton toolButton2;
        private ToolButton toolButton1;
        private ToolButton toolButton5;
        private ToolButton toolButton4;
        private ToolButton toolButton3;
    }
}