using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_WorkTools.Dailog
{
    internal class ExeViewModel: ViewModelBase
    {
        public ExeViewModel()
        {
            SureCommand = new DelegateCommand(Sure);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }
        private string tipText;
        public string TipText
        {
            get { return tipText; }
            set { tipText = value; RaisePropertyChanged(); }
        }
        private string inputString;

        public string InputString
        {
            get { return inputString; }
            set { inputString = value; RaisePropertyChanged(); }
        }

        private void Cancel(object p)
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new CommonDialogResult() {Button=CommonDialogButton.Cancel,Data=null });
        }

        private void Sure(object p)
        {
            if (string.IsNullOrEmpty(InputString))
                return;
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogHost.Close(DialogHostName, new CommonDialogResult() { Button = CommonDialogButton.Ok, Data = inputString });
            }
        }

        private string DialogHostName { get; set; } = "Root";
        public DelegateCommand SureCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public void InitParams(CommonDialogParams p)
        {
            DialogHostName = p.DialogHost;
            Title = p.DialogTitle;
        }
    }
}
