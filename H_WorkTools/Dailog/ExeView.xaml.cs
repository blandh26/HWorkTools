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
    public partial class ExeView : UserControl
    {
        //details: https://msdn.microsoft.com/en-us/library/windows/desktop/ms648075(v=vs.85).aspx
        //Creates an array of handles to icons that are extracted from a specified file.
        //This function extracts from executable (.exe), DLL (.dll), icon (.ico), cursor (.cur), animated cursor (.ani), and bitmap (.bmp) files.
        //Extractions from Windows 3.x 16-bit executables (.exe or .dll) are also supported.
        [DllImport("User32.dll")]
        public static extern int PrivateExtractIcons(
            string lpszFile, //file name
            int nIconIndex,  //The zero-based index of the first icon to extract.
            int cxIcon,      //The horizontal icon size wanted.
            int cyIcon,      //The vertical icon size wanted.
            IntPtr[] phicon, //(out) A pointer to the returned array of icon handles.
            int[] piconid,   //(out) A pointer to a returned resource identifier.
            int nIcons,      //The number of icons to extract from the file. Only valid when *.exe and *.dll
            int flags        //Specifies flags that control this function.
        );

        //details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
        //Destroys an icon and frees any memory the icon occupied.
        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(
            IntPtr hIcon //A handle to the icon to be destroyed. The icon must not be in use.
        );

        ExeViewModel model = new ExeViewModel();
        string icoName = "";
        string file = "";
        public ExeView(CommonDialogParams ps)
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
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下

            //选择文件对话框
            var opfd = new System.Windows.Forms.OpenFileDialog { Filter = "*.exe|" };
            if (opfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            file = opfd.FileName;

            //指定存放图标的文件夹
            string folderToSave = path + "ico\\";
            if (!Directory.Exists(folderToSave)) Directory.CreateDirectory(folderToSave);

            //选中文件中的图标总数
            var iconTotalCount = PrivateExtractIcons(file, 0, 0, 0, null, null, 0, 0);

            //用于接收获取到的图标指针
            IntPtr[] hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            int[] ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons(file, 0, 256, 256, hIcons, ids, iconTotalCount, 0);

            //遍历并保存图标
            for (var i = 0; i < successCount; i++)
            {
                //指针为空，跳过
                if (hIcons[i] == IntPtr.Zero) continue;

                using (var ico = Icon.FromHandle(hIcons[i]))
                {
                    if (i == 0)
                    {
                        icoName = DateTime.Now.ToString("yyyyMMddHHmmsss") + ".png";
                        using (var myIcon = ico.ToBitmap())
                        {
                            myIcon.Save(folderToSave + icoName, ImageFormat.Png);
                        }
                        image1.Source = new BitmapImage(new Uri(path + "ico\\" + icoName));
                        model.Ico = "ico\\" + icoName;
                        model.Path = file;
                        return;
                    }
                }
                //内存回收
                DestroyIcon(hIcons[i]);
            }
        }
    }
}
