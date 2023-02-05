using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_WorkTools.Dailog
{
    internal class AlarmViewModel : ViewModelBase
    {
        public AlarmViewModel()
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

        private string txttitle;

        public string TxtTitle
        {
            get { return txttitle; }
            set { txttitle = value; RaisePropertyChanged(); }
        }
        private string ico;

        public string Ico
        {
            get { return ico; }
            set { ico = value; }
        }

        private string path;

        public string Path
        {
            get { return path; }
            set { path = value;  }
        }

        private void Cancel(object p)
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new CommonDialogResult() {Button=CommonDialogButton.Cancel,Data=null });
        }

        private void Sure(object p)
        {
            if (string.IsNullOrEmpty(path))
                return;
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                AlarmModel model = new AlarmModel();
                //model.title = txttitle;
                //model.ico = ico;
                //model.path = path;
                string json = JsonConvert.SerializeObject(model);
                DialogHost.Close(DialogHostName, new CommonDialogResult() { Button = CommonDialogButton.Ok, Data = json });
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
