using LiteDB;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        AlarmViewModel vmodel = new AlarmViewModel();
        public AlarmView(CommonDialogParams ps)
        {
            InitializeComponent();
            this.DataContext = vmodel;
            vmodel.InitParams(ps);
            cbType.SelectedIndex = 0;
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbType.SelectedIndex == 0)
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
            else if (cbType.SelectedIndex == 2)
            {
                DatePicker.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_W.Visibility = System.Windows.Visibility.Hidden;
                StackPanel_M.Visibility = System.Windows.Visibility.Visible;
                this.Height = 450;
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DatePickerValue.Text = "";
            if (cbType.SelectedIndex == 0)
            {
                DatePickerValue.Text = Convert.ToDateTime(DatePicker.SelectedDate).ToString("yyyy-MM-dd");
            }
            else if(cbType.SelectedIndex == 1)
            {
                foreach (System.Windows.UIElement element in StackPanel_W.Children)
                {
                    CheckBox c = (CheckBox)element;
                    if (Convert.ToBoolean(c.IsChecked))
                    {
                        if (DatePickerValue.Text.ToString() == "")
                            DatePickerValue.Text = c.DataContext.ToString();
                        else
                            DatePickerValue.Text += "," + c.DataContext.ToString();
                    }
                }
            }
            else if (cbType.SelectedIndex == 2)
            {
                foreach (System.Windows.UIElement element in StackPanel_M.Children)
                {
                    CheckBox c = (CheckBox)element;
                    if (Convert.ToBoolean(c.IsChecked))
                    {
                        if (DatePickerValue.Text.ToString() == "")
                            DatePickerValue.Text = c.DataContext.ToString();
                        else
                            DatePickerValue.Text += "," + c.DataContext.ToString();
                    }
                }
            }
            vmodel.TxtDate = DatePickerValue.Text.ToString();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AlarmTemp.json != "")
            {
                AlarmModel model = JsonConvert.DeserializeObject<AlarmModel>(AlarmTemp.json);
                vmodel.TxtID = model.id;
                if (model.alarmType == "0")
                {
                    DatePicker.SelectedDate = Convert.ToDateTime(model.data);
                }
                else if (model.alarmType == "1")
                {
                    string[] list = model.data.Split(',');
                    foreach (System.Windows.UIElement element in StackPanel_W.Children)
                    {
                        CheckBox cb = (CheckBox)element;
                        if (list.Contains(cb.DataContext.ToString()))
                        {
                            cb.IsChecked = true;
                        }
                    }
                }
                else if (model.alarmType == "2")
                {
                    string[] list = model.data.Split(',');
                    foreach (System.Windows.UIElement element in StackPanel_M.Children)
                    {
                        CheckBox cb = (CheckBox)element;
                        if (list.Contains(cb.DataContext.ToString()))
                        {
                            cb.IsChecked = true;
                        }
                    }
                }
            }
            else
            {
                vmodel.TxtID = Guid.NewGuid().ToString();
                cbType.SelectedIndex = 0;
                DatePicker.SelectedDate = DateTime.Now;
                TimePicker.SelectedTime = DateTime.Now;
            }
        }
    }
}
