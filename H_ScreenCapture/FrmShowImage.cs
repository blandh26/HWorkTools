using H_Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace H_ScreenCapture
{
    public partial class FrmShowImage : Form
    {
        Config cif = new Config();
        Point poit;
        Size size;
        string title;
        Image img;
        public FrmShowImage(Point poit_tmp, Size size_tmp, Image img_tmp)
        {
            InitializeComponent();
            string strTitle = cif.GetValue("ScreenCapture_Title");
            string strFormat = Regex.Match(strTitle, @"\${(.*)\}", RegexOptions.Singleline).Groups[1].Value;//大括号{}
            title = strTitle.Replace("${"+ strFormat + "}", DateTime.Now.ToString(strFormat));
            poit = poit_tmp;
            size = size_tmp;
            img = img_tmp;
        }

        private void FrmShowImage_Load(object sender, EventArgs e)
        {
            this.Opacity = Convert.ToInt32(cif.GetValue("ScreenCapture_Opacity")) / 100.00;
            this.Text = title;
            this.Location = poit;
            this.Size = size;
            pictureBox1.Image = img;
        }

        private void FrmShowImage_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyCode.ToString());//这里捕获不到方向键

            switch (e.KeyCode)
            {
                case Keys.Escape: this.Close(); break;
                case Keys.Right: this.Location = new Point(this.Location.X + 1, this.Location.Y); break;
                case Keys.Left: this.Location = new Point(this.Location.X - 1, this.Location.Y); break;
                case Keys.Up: this.Location = new Point(this.Location.X , this.Location.Y - 1); break;
                case Keys.Down: this.Location = new Point(this.Location.X , this.Location.Y + 1); break;
            }
        }
    }
}
