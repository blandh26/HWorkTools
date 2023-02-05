using LiteDB;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace H_WorkTools.Dailog
{
    /// <summary>
    /// InputView.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmView : UserControl
    {

        ExeViewModel model = new ExeViewModel();
        string icoName = "";
        string file = "";
        public AlarmView(CommonDialogParams ps)
        {
            InitializeComponent();
            this.DataContext = model;
            model.InitParams(ps);
        }

        /// <summary>
        ///  选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelect_Click(object sender, EventArgs e)
        {
           
        }
    }
}
