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

        AlarmViewModel model = new AlarmViewModel();
        public AlarmView(CommonDialogParams ps)
        {
            InitializeComponent();
            this.DataContext = model;
            model.InitParams(ps);
            cbType.SelectedIndex = 0;
        }

        /// <summary>
        ///  选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelect_Click(object sender, EventArgs e)
        {
           
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbType.SelectedIndex==0)
            {
                DatePicker.Visibility = System.Windows.Visibility.Visible;
                StackPanel_W.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_M.Visibility = System.Windows.Visibility.Hidden;
                this.Height = 360;
            }
            else if (cbType.SelectedIndex == 1)
            {
                DatePicker.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_W.Visibility = System.Windows.Visibility.Visible;
                StackPanel_M.Visibility = System.Windows.Visibility.Hidden;
                this.Height = 360;
            }
            else if(cbType.SelectedIndex == 2)
            {
                DatePicker.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_W.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_M.Visibility = System.Windows.Visibility.Visible;
                this.Height = 450;
            }

            foreach (System.Windows.UIElement element in StackPanel_W.Children)
            {
                CheckBox c = (CheckBox)element;
                string aaa = c.Content.ToString()+ c.IsChecked.ToString();
                Console.WriteLine(aaa);
            }
            foreach (System.Windows.UIElement element in StackPanel_M.Children)
            {
                CheckBox c = (CheckBox)element;
                string aaa = c.Content.ToString() + c.IsChecked.ToString();
                Console.WriteLine(aaa);
            }

        }
    }
}
