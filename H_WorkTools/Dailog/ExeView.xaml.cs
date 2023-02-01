using System.Windows.Controls;

namespace H_WorkTools.Dailog
{
    /// <summary>
    /// InputView.xaml 的交互逻辑
    /// </summary>
    public partial class ExeView : UserControl
    {
        public ExeView(CommonDialogParams ps)
        {
            InitializeComponent();

            ExeViewModel model=new ExeViewModel();
            this.DataContext = model;
            model.InitParams(ps);
        }
    }
}
