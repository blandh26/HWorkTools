namespace H.WorkTools
{
    partial class DeskShareView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeskShareView));
            this.axRDPViewer1 = new AxRDPCOMAPILib.AxRDPViewer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).BeginInit();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // axRDPViewer1
            // 
            this.axRDPViewer1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.axRDPViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axRDPViewer1.Enabled = true;
            this.axRDPViewer1.Location = new System.Drawing.Point(0, 0);
            this.axRDPViewer1.Name = "axRDPViewer1";
            this.axRDPViewer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axRDPViewer1.OcxState")));
            this.axRDPViewer1.Size = new System.Drawing.Size(800, 350);
            this.axRDPViewer1.TabIndex = 0;
            this.axRDPViewer1.OnConnectionEstablished += new System.EventHandler(this.OnConnectionEstablished);
            this.axRDPViewer1.OnConnectionFailed += new System.EventHandler(this.OnConnectionFailed);
            this.axRDPViewer1.OnConnectionTerminated += new AxRDPCOMAPILib._IRDPSessionEvents_OnConnectionTerminatedEventHandler(this.OnConnectionTerminated);
            this.axRDPViewer1.OnError += new AxRDPCOMAPILib._IRDPSessionEvents_OnErrorEventHandler(this.OnError);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.axRDPViewer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 350);
            this.panel1.TabIndex = 4;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 25);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::H.WorkTools.Properties.Resources._1;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 21);
            this.toolStripMenuItem1.Text = "toolStripMenuItem1";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // DeskShareView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 350);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DeskShareView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "観覧";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeskShareView_FormClosing);
            this.Load += new System.EventHandler(this.DeskShareView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxRDPCOMAPILib.AxRDPViewer axRDPViewer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}