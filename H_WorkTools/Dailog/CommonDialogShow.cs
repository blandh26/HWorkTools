using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H_WorkTools.Dailog;

namespace H_WorkTools.Dailog
{
    internal class CommonDialogShow
    {
        public static async Task<object> ShowInsertExe(string host, string title)
        {
            CommonDialogParams ps = new CommonDialogParams()
            { DialogHost = host, DialogTitle = title };
            return await DialogHost.Show(new ExeView(ps), host);
        }
        public static async Task<object> ShowInsertAlarm(string host, string title, string tipText, bool isQuestion)
        {
            CommonDialogParams ps = new CommonDialogParams()
            { DialogHost = host, DialogTitle = title };
            return await DialogHost.Show(new AlarmView(ps), host);
        }
        public static async Task<object> ShowCurcularProgress(string host, Action action)
        {
            DialogHost.Show(new CurcularProgressView(), host);
            try
            {
                await Task.Run(() =>
                {
                    action();
                });
                return new CommonDialogResult() { Button = CommonDialogButton.Ok };
            }
            finally
            {
                DialogHost.Close(host);
            }
        }
    }
    
    internal enum CommonDialogButton
    {
        Cancel=0, //取消
        Ok=1 //确认
    }
    /// <summary>
    /// 对话框返回的结果
    /// </summary>
    internal class CommonDialogResult
    {
        /// <summary>
        /// 点击了哪个按钮
        /// </summary>
        public CommonDialogButton Button { get; set; }
        /// <summary>
        /// 回传的数据
        /// </summary>
        public object Data { get; set; }
    }
    /// <summary>
    /// 对话框的参数
    /// </summary>
    public class CommonDialogParams
    {
        /// <summary>
        /// 对话框悬停的Host名称
        /// </summary>
        public string DialogHost { get; set; } = "Root";
        /// <summary>
        /// 对话框的标题
        /// </summary>
        public string DialogTitle { get; set; }
        /// <summary>
        /// 对话框的提示语
        /// </summary>
        public string DialogTipText { get; set; }
        /// <summary>
        /// 如果是输入框，则可设置默认值
        /// </summary>
        public string DefaultText { get; set; }
        /// <summary>
        /// 是否是询问对话框
        /// </summary>
        public bool IsQuestion { get; set; }
    }
}
