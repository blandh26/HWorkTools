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


        #region 表单
        private string txtID;

        public string TxtID
        {
            get { return txtID; }
            set { txtID = value; RaisePropertyChanged(); }
        }
        private string txtTitle;

        public string TxtTitle
        {
            get { return txtTitle; }
            set { txtTitle = value; RaisePropertyChanged(); }
        }
        private string txtContent;

        public string TxtContent
        {
            get { return txtContent; }
            set { txtContent = value; RaisePropertyChanged(); }
        }
        private string cbType;

        public string CbType
        {
            get { return cbType; }
            set { cbType = value; RaisePropertyChanged(); }
        }
        private string txtTime;

        public string TxtTime
        {
            get { return txtTime; }
            set { txtTime = value; RaisePropertyChanged(); }
        }
        private string txtDate;

        public string TxtDate
        {
            get { return txtDate; }
            set { txtDate = value; RaisePropertyChanged(); }
        }
        #endregion

        private void Cancel(object p)
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new CommonDialogResult() { Button = CommonDialogButton.Cancel, Data = null });
        }

        private void Sure(object p)
        {
            if (string.IsNullOrEmpty(txtTitle))
                return;
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                AlarmModel model = new AlarmModel();
                model.id = txtID;
                model.title = txtTitle;
                model.content = txtContent;
                model.alarmType = cbType;
                model.data = txtDate;                
                model.time = txtTime;
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
            if (AlarmTemp.json != "")
            {
                AlarmModel model = JsonConvert.DeserializeObject<AlarmModel>(AlarmTemp.json);
                txtTitle = model.title;
                txtContent = model.content;
                cbType = model.alarmType;
                txtDate = model.data;
                txtTime = model.time;
            }
        }
    }
}
