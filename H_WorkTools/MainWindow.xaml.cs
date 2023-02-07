using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using H_Util;
using Newtonsoft.Json;
using Clipboard = System.Windows.Forms.Clipboard;
using System.Threading.Tasks;
using static H_Util.WhoUsePort;
using RDPCOMAPILib;
using System.Net;
using Microsoft.Win32;
using H_ScreenCapture;
using System.Security.Principal;
using System.Text.RegularExpressions;
using LiteDB;
using NAudio.Wave;
using MenuItem = System.Windows.Forms.MenuItem;
using H_WorkTools.Dailog;
using System.Windows.Media.Imaging;
using HWorkTools;

namespace H_WorkTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region 系统api
        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);
        /// <summary>
        /// 卸载快捷键
        /// </summary>
        /// <param name="hWnd">定义热键的内容</param>
        /// <param name="id">定义热键ID</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        #endregion
        #region 远程
        protected RDPSession _rdpSession = null;
        string invitationString;//密钥
        string RdpKey = "";
        bool IsDeskShare = true;//true 可以共享 false 不可以
        #endregion
        #region 剪贴板
        bool IsClipboardMode = false;
        bool IsClipboardDelete = false;
        public delegate void AddLvClipboardListDelegate();
        public delegate void LvAudienceUpdate();//定义一个委托 关闭窗体调用
        #endregion
        #region 通信
        private TcpP2p p2p = new TcpP2p();
        bool isInvite = false;//是否是求助模式
        #endregion

        bool IsExeDelete = false;//应用中心 删除状态
        bool IsAlarmDelete = false;//闹铃 删除状态
        bool IsAlarmAdd = false;//闹铃 新增状态
        System.Threading.Timer AlarmTimer;//闹铃 定时任务
        bool IsVideo = false;//录屏按钮控制
        PortUserInfo[] portlist;
        private static string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
        string encrypt_key = "";//加密解密key
        Config cif = new Config();
        FrmCapture m_frmCapture;//截图
        SystemInfo sys = new SystemInfo();
        NotifyIcon notifyIcon;

        #region 窗体事件
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel model = new MainWindowViewModel();
            this.DataContext = model;
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Show(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 窗体状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮 
            if (this.WindowState == WindowState.Minimized)
            {
                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                //图标显示在托盘区 
                this.notifyIcon.Visible = true;
            }
        }

        /// <summary>
        ///  加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.None;
            txtTitle.Text = "HWorkTools[" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]";//版本显示
            var hwnd = new WindowInteropHelper(this).Handle;
            AddClipboardFormatListener(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            encrypt_key = cif.GetValue("Encrypt");

            #region 托盘设置
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Text = "HWorkTools";//鼠标移入图标后显示的名称
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            //打开菜单项
            MenuItem show = new MenuItem("显示主窗体");
            show.Click += new EventHandler(Show);
            //退出菜单项
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            MenuItem[] mis = new MenuItem[] { show, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(mis);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, ee) =>
            {
                if (ee.Button == MouseButtons.Left)
                {
                    this.Show(o, ee);
                }
            });
            #endregion

            #region  应用中心
            using (var db = new LiteDatabase(path + "worktools.db"))
            {
                var exemodel = db.GetCollection<ExeModel>("ExeModel");
                // 在 path 字段上创建唯一索引
                exemodel.EnsureIndex(x => x.path, true);
                List<ExeModel> list = JsonConvert.DeserializeObject<List<ExeModel>>(JsonConvert.SerializeObject(exemodel.FindAll()));
                for (int i = 0; i < list.Count; i++)
                    list[i].ico = path + list[i].ico;
                LvExe.ItemsSource = list;
                db.Dispose();
            }
            #endregion

            #region  闹铃
            txtAlarmTimer.Text = cif.GetValue("Alarm");
            using (var db = new LiteDatabase(path + "worktools.db"))
            {
                var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                // 在 path 字段上创建唯一索引
                alarmmodel.EnsureIndex(x => x.id, true);
                List<AlarmModel> list = JsonConvert.DeserializeObject<List<AlarmModel>>(JsonConvert.SerializeObject(alarmmodel.FindAll().OrderBy(x => x.lastTime)));
                for (int i = 0; i < list.Count; i++)
                    list[i].data = GetAlarmType(list[i]);
                LvAlarm.ItemsSource = list;
                db.Dispose();
            }
            AlarmTimer = new System.Threading.Timer(state =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    using (var db = new LiteDatabase(path + "worktools.db"))
                    {
                        var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                        List<AlarmModel> list = JsonConvert.DeserializeObject<List<AlarmModel>>(JsonConvert.SerializeObject(alarmmodel.FindAll().OrderBy(x => x.lastTime)));
                        foreach (AlarmModel model in list)
                        {
                            try
                            {
                                bool isShow = false;
                                DateTime dt = DateTime.Now;
                                if (model.alarmType == "0")//指定日期
                                {
                                    if (string.IsNullOrEmpty(model.lastTime) && Convert.ToDateTime(model.data + " " + model.time) <= dt)
                                    {
                                        isShow = true;
                                    }
                                }
                                else if (model.alarmType == "1")//每周
                                {
                                    if (model.data.Contains(Convert.ToInt32(dt.DayOfWeek).ToString()) && Convert.ToInt32(model.time.Replace(":", "")) <= Convert.ToInt32(dt.ToString("HHmm")))
                                    {
                                        if (string.IsNullOrEmpty(model.lastTime))
                                        {
                                            isShow = true;
                                        }
                                        else
                                        {
                                            if (Convert.ToDateTime(model.lastTime).ToString("yyyyMMdd") != dt.ToString("yyyyMMdd"))
                                            {
                                                isShow = true;
                                            }
                                        }
                                    }
                                }
                                else if (model.alarmType == "2")//每月
                                {
                                    string[] daiList = model.data.Split(',');
                                    if (daiList.Contains(Convert.ToInt32(dt.ToString("dd")).ToString()) && Convert.ToInt32(model.time.Replace(":", "")) <= Convert.ToInt32(dt.ToString("HHmm")))
                                    {
                                        if (string.IsNullOrEmpty(model.lastTime))
                                        {
                                            isShow = true;
                                        }
                                        else
                                        {
                                            if (Convert.ToDateTime(model.lastTime).ToString("yyyyMMdd") != dt.ToString("yyyyMMdd"))
                                            {
                                                isShow = true;
                                            }
                                        }
                                    }
                                }
                                if (isShow)
                                {
                                    new MessageBoxCustom(model.title, model.content, MessageType.Success, MessageButtons.Ok).ShowDialog();
                                    model.lastTime = dt.ToString("yyyy-MM-dd HH:mm");
                                    alarmmodel.Update(model);
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                        db.Dispose();
                    }
                    AlarmRefresh();
                }));
            }, null, 0, Convert.ToInt32(cif.GetValue("Alarm")));
            #endregion

            #region 窗体位置
            System.Drawing.Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            this.Top = 60;
            this.Left = rectangle.Width - 440;
            #endregion

            #region 初始化远程
            this.txtPath.Text = cif.GetValue("ScreenRecordingPath");
            txtNiceName.Text = cif.GetValue("NickName");
            List<IpModel> ipList = new List<IpModel>();//语言实体类
            string HostName = Dns.GetHostName(); //得到主机名
            IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
            for (int i = 0; i < IpEntry.AddressList.Length; i++)
            {
                if (!IpEntry.AddressList[i].AddressFamily.ToString().Contains("V6"))
                {
                    ipList.Add(new IpModel { id = IpEntry.AddressList[i].ToString(), title = IpEntry.AddressList[i].ToString() });
                }
            }
            cbIp.ItemsSource = ipList;
            if (cif.GetValue("Ip") != "")
            {
                cbIp.SelectedIndex = ipList.FindIndex(ipmodel => ipmodel.id == cif.GetValue("Ip"));
            }
            else
            {
                cbIp.SelectedIndex = 0;
            }

            #region 设置监听
            p2p.myport = Convert.ToInt32(cif.GetValue("Tcp"));
            if (TcpP2p.IsPortOccuped(p2p.myport))
            {
                bool? Result = new MessageBoxCustom("adssad", "是否关闭占用端口程序 ? ", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
                if (Convert.ToBoolean(Result))
                {
                    try
                    {
                        PortUserInfo[] list = WhoUsePort.NetStatus(6280, true);
                        list[0].Process.Kill();
                    }
                    catch (Exception)
                    {
                        new MessageBoxCustom("adssad", "端口相关程序关闭失败，关闭程序 ", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                        Environment.Exit(0); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。
                    };
                }
            }
            p2p.Listener();
            p2p.ReceivMsg += new TcpP2p.ShowMessage(P2P_ReceivMsg);
            #endregion
            #endregion

            #region 录屏
            if (cif.GetValue("ScreenRecordingPath") == "")
            {
                cif.SaveValue("ScreenRecordingPath", path + "HScreenVideo\\");
                if (!Directory.Exists(path + "HScreenVideo"))
                {
                    try
                    {
                        Directory.CreateDirectory(path + "HScreenVideo");//创建文件夹
                    }
                    catch (Exception e1)
                    {
                    }
                }
            }
            cbAudio.ItemsSource = GetDevices();
            cbAudio.SelectedIndex = 0;
            this.txtPath.Text = cif.GetValue("ScreenRecordingPath");
            #endregion

            #region 初始化语言
            List<LanguageModel> languages = new List<LanguageModel>();//语言实体类
            languages.Add(new LanguageModel { id = "zh_cn", title = "中文（简体）" });
            languages.Add(new LanguageModel { id = "en_us", title = "English" });
            languages.Add(new LanguageModel { id = "jp_jp", title = "日本語" });
            languages.Add(new LanguageModel { id = "kr_kr", title = "한국어" });
            cbLanguage.ItemsSource = languages;
            cbLanguage.SelectedIndex = languages.FindIndex(language => language.id == cif.GetValue("Language"));
            if (Convert.ToBoolean(cif.GetValue("Start") == "" || cif.GetValue("Start") == "True" ? true : false))
            {
                rbStart1.IsChecked = true;
            }
            else
            {
                rbStart2.IsChecked = true;
            }
            #endregion

            #region 装载剪贴板
            if (File.Exists(path + "Json\\" + "\\" + "Clipboard.Json"))
            {
                string json = File.ReadAllText(path + "Json\\" + "\\" + "Clipboard.Json", Encoding.UTF8);
                List<string> slist = JsonConvert.DeserializeObject<List<string>>(json);
                for (int i = 0; i < slist.Count; i++)
                {
                    TextBlock text = new TextBlock();
                    text.Text = Encrypt.AESDEncrypt(slist[i], encrypt_key);
                    LvClipboard.Items.Add(text);
                }
            }
            #endregion

        }

        /// <summary>
        ///  关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVideo)
            {//判断是否启动录像中
                IsVideo = false;
                FFmpeg_Stop();
            }
            if (AlarmTimer != null)
            {
                AlarmTimer.Dispose();
            }
            try
            {//卸载快捷键
                var hwnd = new WindowInteropHelper(this).Handle;
                UnRegist(hwnd, () => { });//停止共享
            }
            catch (Exception) { }
            ProcessHelper.ProcessKill("FFmpeg");//杀掉ffmpeg进程
            Environment.Exit(0); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。
        }

        /// <summary>
        /// 重载OnSourceInitialized来在SourceInitialized事件发生后获取窗体的句柄，并且注册全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            #region 截图、共享、剪贴板 快捷键赋值
            ScreenCaptureOpacity.Value = Convert.ToInt32(cif.GetValue("ScreenCapture_Opacity"));
            string ScreenCapture_Key = cif.GetValue("ScreenCapture_Key");
            string[] sckList = ScreenCapture_Key.Split('|');
            cbScreenCaptureCtrl.IsChecked = sckList[0] == "1" ? true : false;
            cbScreenCaptureAlt.IsChecked = sckList[1] == "1" ? true : false;
            cbScreenCaptureShift.IsChecked = sckList[2] == "1" ? true : false;
            txtScreenCaptureKey.Text = sckList[3];
            string ScreenCapture_LastKey = cif.GetValue("ScreenCapture_LastKey");
            string[] sckLastList = ScreenCapture_LastKey.Split('|');
            cbLastCtrl.IsChecked = sckLastList[0] == "1" ? true : false;
            cbLastAlt.IsChecked = sckLastList[1] == "1" ? true : false;
            cbLastShift.IsChecked = sckLastList[2] == "1" ? true : false;
            txtLastKey.Text = sckLastList[3];
            string ScreenCapture_DiffKey = cif.GetValue("ScreenCapture_DiffKey");
            string[] sckDiffList = ScreenCapture_DiffKey.Split('|');
            cbDiffCtrl.IsChecked = sckDiffList[0] == "1" ? true : false;
            cbDiffAlt.IsChecked = sckDiffList[1] == "1" ? true : false;
            cbDiffShift.IsChecked = sckDiffList[2] == "1" ? true : false;
            txtDiffKey.Text = sckDiffList[3];

            ScreenCaptureOpacity.Value = Convert.ToInt32(cif.GetValue("ScreenCapture_Opacity"));
            ScreenCaptureTitle.Text = cif.GetValue("ScreenCapture_Title");

            string DeskShare = cif.GetValue("DeskShare");
            string[] dList = DeskShare.Split('|');
            cbDeskShare.SelectedIndex = Convert.ToInt32(dList[0]);
            txtDeskShareKey.Text = dList[1];
            string Clipboard = cif.GetValue("Clipboard");
            string[] cList = Clipboard.Split('|');
            cbClipboardCtrl.SelectedIndex = Convert.ToInt32(cList[0]);
            cbClipboardNumber.SelectedIndex = Convert.ToInt32(cList[1]);

            #endregion
            RegistAll();//注册所有快捷键
        }

        const int WM_HOTKEY = 0x312;
        //https://blog.csdn.net/u011555996/article/details/113785700 参考 msg 数字
        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (keymap.TryGetValue(id, out var callback))
                {
                    callback();
                }
            }
            else if (msg == 0x031D)//剪贴板
            {
                AddLvClipboardList();
            }
            return hwnd;
        }

        /// <summary>
        /// 窗口移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        ////获取辅助按键
        private void Txt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            (sender as System.Windows.Controls.TextBox).Text = e.Key.ToString();   //显示点下的按键
        }
        #endregion

        #region 录屏   
        //ffmpeg进程
        static Process ffmpegProcess = new Process();

        //ffmpeg.exe实体文件路径，建议把ffmpeg.exe及其配套放在自己的Debug目录下
        static string ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\ffmpeg.exe";

        /// <summary>
        /// 录屏计时线程
        /// </summary>
        System.Threading.Timer FFmpegTimmer;

        /// <summary>
        /// 录屏计时
        /// </summary>
        DateTime dt;

        /// <summary>
        /// 控制台输出信息
        /// </summary>
        private void Output(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    txtVideo.Text += Environment.NewLine + e.Data.ToString();
                    VideoScrollViewer.ScrollToEnd();
                }));
            }
        }

        /// <summary>
        /// 获取音频设备
        /// </summary>
        /// <returns></returns>
        private List<String> GetDevices()
        {
            List<String> list = new List<String>();
            // 返回系统中可用的Wave-In设备数
            int waveInDevices = WaveIn.DeviceCount;
            for (int i = 0; i < waveInDevices; i++)
            {
                list.Add(WaveIn.GetCapabilities(i).ProductName);
            }
            list.Add("-");
            return list;
        }

        /// <summary>
        ///  刷新设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Refresh_Click(object sender, EventArgs e)
        {
            cbAudio.ItemsSource = GetDevices();
            cbAudio.SelectedIndex = 0;
        }

        /// <summary>
        ///  录屏开始和停止按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Video_Click(object sender, EventArgs e)
        {
            if (IsVideo)//true 停止
            {
                try
                {
                    FFmpeg_Stop();
                }
                catch (Exception)
                {
                }
            }
            else//false 录制
            {
                try
                {
                    dt = DateTime.Now;
                    LabMins.Foreground = new SolidColorBrush(Colors.Red);
                    FFmpegTimmer = new System.Threading.Timer(state =>
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
                            TimeSpan ts2 = new TimeSpan(dt.Ticks);
                            TimeSpan ts = ts1.Subtract(ts2).Duration();
                            LabMins.Content = ts.ToString(@"hh\:mm\:ss\.fff");
                        }));
                    }, null, 0, 1);

                    bool isRegiste = false;
                    this.BtnScreenRecordingVideo.ToolTip = "Stop";
                    BtnScreenRecordingVideo.Foreground = new SolidColorBrush(Colors.Red);
                    txtVideo.Text = "";
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\audio_sniffer.dll");
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\audio_sniffer-x64.dll");
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\racob-x64.dll");
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\racob-x86.dll");
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\screen-capture-recorder.dll");
                    isRegiste = ProcessHelper.RegisterDll(AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\screen-capture-recorder-x64.dll");
                    IsVideo = true;
                    string outFilePath = cif.GetValue("ScreenRecordingPath") + "HScreenVideo" + DateTime.Now.ToString("yyyyMMddHHmm") + ".mp4";
                    if (File.Exists(outFilePath))
                    {
                        File.Delete(outFilePath);
                    }
                    string arguments = " -f dshow -i audio=\"virtual-audio-capturer\"";
                    if (cbAudio.Text != "" && cbAudio.Text != "-")
                    {
                        arguments += " -f dshow -i audio=\"" + cbAudio.Text + "\"";
                    }
                    arguments += " -filter_complex amix=inputs=1:duration=first:dropout_transition=0";
                    arguments += " -f dshow -i video=\"screen-capture-recorder\" -pix_fmt yuv420p ";
                    arguments += outFilePath;
                    ffmpegProcess = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath);
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    startInfo.Arguments = arguments;
                    startInfo.UseShellExecute = false;//不使用操作系统外壳程序启动
                    startInfo.RedirectStandardError = true;//重定向标准错误流
                    startInfo.CreateNoWindow = true;//默认不显示窗口
                    startInfo.RedirectStandardInput = true;//启用模拟该进程控制台输入的开关
                    startInfo.RedirectStandardOutput = true;

                    ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(Output);//把FFmpeg的输出写到StandardError流中
                    ffmpegProcess.StartInfo = startInfo;

                    ffmpegProcess.Start();//启动
                    ffmpegProcess.BeginErrorReadLine();//开始异步读取输出
                }
                catch (Exception)
                {
                    //录制失败
                    BtnScreenRecordingVideo.IsChecked = false;
                    BtnScreenRecordingVideo.Foreground = new SolidColorBrush(Colors.White);
                    LabMins.Foreground = new SolidColorBrush(Colors.Black);
                    IsVideo = false;
                }
            }
        }

        /// <summary>
        /// 停止录屏
        /// </summary>
        public void FFmpeg_Stop()
        {
            IsVideo = false;
            this.BtnScreenRecordingVideo.ToolTip = "Start";
            BtnScreenRecordingVideo.Foreground = new SolidColorBrush(Colors.White);
            LabMins.Foreground = new SolidColorBrush(Colors.Black);
            ffmpegProcess.StandardInput.WriteLine("q");//在这个进程的控制台中模拟输入q,用于停止录制
            ffmpegProcess.Close();
            ffmpegProcess.Dispose();
            LabMins.Content = (new TimeSpan(0, 0, 0, 0)).ToString();
            FFmpegTimmer.Dispose();
        }

        /// <summary>
        ///  录屏-打开目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Open_Click(object sender, EventArgs e)
        {
            if (cif.GetValue("ScreenRecordingPath").Trim() != "")
            {
                Process.Start(cif.GetValue("ScreenRecordingPath"));
            }
        }
        /// <summary>
        ///  录屏-设置目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Path_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    cif.SaveValue("ScreenRecordingPath", $"{folderBrowser.SelectedPath}{Path.DirectorySeparatorChar}");
                    this.txtPath.Text = cif.GetValue("ScreenRecordingPath");
                }
            }
        }
        #endregion

        #region 端口
        /// <summary>
        ///  查询端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPortSelect_Click(object sender, EventArgs e)
        {
            portlist = null;
            LvPort.Items.Clear();
            try
            {
                portlist = WhoUsePort.NetStatus(txtPort.Text == "" ? 0 : Convert.ToInt32(txtPort.Text));
                foreach (var item in portlist)
                {
                    TextBlock text = new TextBlock();
                    text.Text = item.LocolPort.ToString().PadRight(7, ' ') + item.Pid.ToString().PadRight(7, ' ') + item.ProcessName.ToString().PadRight(7, ' ');
                    LvPort.Items.Add(text);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        ///  杀掉端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPortKill_Click(object sender, EventArgs e)
        {
            portlist[LvPort.SelectedIndex].Process.Kill();
            LvPort.Items.Remove(LvPort.Items[LvPort.SelectedIndex]);
        }

        #endregion

        #region 远程控制
        /// <summary>
        ///  远程-开始和结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeskShare_Click(object sender, EventArgs e)
        {
            if (txtNiceName.Text == "")
            {
                new MessageBoxCustom("系统提示", "必须输入昵称", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                return;
            }
            if (IsDeskShare)
            {
                _rdpSession = new RDPSession();  // 新建RDP Session
                _rdpSession.OnAttendeeConnected += RdpSessionOnOnAttendeeConnected;
                _rdpSession.OnAttendeeDisconnected += RdpSessionOnOnAttendeeDisconnected;
                _rdpSession.OnAttendeeConnected += new _IRDPSessionEvents_OnAttendeeConnectedEventHandler(OnAttendeeConnected);
                _rdpSession.OnAttendeeDisconnected += new _IRDPSessionEvents_OnAttendeeDisconnectedEventHandler(OnAttendeeDisconnected);
                _rdpSession.OnControlLevelChangeRequest += new _IRDPSessionEvents_OnControlLevelChangeRequestEventHandler(OnControlLevelChangeRequest);//用户的级别
                int Width = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
                int Height = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);
                _rdpSession.SetDesktopSharedRect(0, 0, Width, Height); // 设置共享区域，如果不设置默认为整个屏幕，当然如果有多个屏幕，还是设置下主屏幕，否则，区域会很大           
                _rdpSession.Open();
                IRDPSRAPIInvitation pInvitation = _rdpSession.Invitations.CreateInvitation(txtNiceName.Text, txtNiceName.Text + "PresentationGroup", "", 99);//获取连接的字符串
                invitationString = (rbMode1.IsChecked == true ? "V" : "C") + pInvitation.ConnectionString;
                Random rNum = new Random();//随机生成类 
                RdpKey = rNum.Next(1000, 9999).ToString();
                txtRdpKey.Text = RdpKey;
                BtnDeskShare.Content = "停止";
                BtnDeskShare.Foreground = new SolidColorBrush(Colors.Brown);
                BtnDeskShareJoin.IsEnabled = false;
                BtnDeskShareInvite.IsEnabled = false;
                IsDeskShare = false;
                rbMode1.IsEnabled = false;
                rbMode2.IsEnabled = false;
                cif.SaveValue("Ip", cbIp.SelectedValue.ToString());
                cif.SaveValue("NickName", txtNiceName.Text);
            }
            else
            {
                Stop();
            }
        }

        /// <summary>
        /// 停止共享
        /// </summary>
        public void Stop()
        {
            if (!IsDeskShare)
            {
                try
                {
                    IsDeskShare = true;
                    invitationString = "";
                    if (_rdpSession != null)
                    {
                        _rdpSession.Close();
                        _rdpSession = null;
                    }
                }
                catch (Exception ex)
                { }
                rbMode1.IsEnabled = true;
                rbMode2.IsEnabled = true;
                BtnDeskShare.Content = "共享";
                BtnDeskShare.Foreground = new SolidColorBrush(Colors.White);
                BtnDeskShareJoin.IsEnabled = true;
                BtnDeskShareInvite.IsEnabled = true;
                LvAudience.Items.Clear();
            }
        }

        /// <summary>
        /// 退出更新
        /// </summary>
        private void _LvAudienceUpdate()
        {
            try
            {
                BtnDeskShare.IsEnabled = true;
                BtnDeskShareJoin.IsEnabled = true;
                BtnDeskShareInvite.IsEnabled = true;
                txtIp.IsEnabled = true;
                TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                bool issend;
                if (isInvite)
                {
                    try
                    {
                        isInvite = false;
                        Sendmsg = new TcpP2p.Msg();
                        Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                        Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                        Sendmsg.sendName = txtNiceName.Text;
                        Sendmsg.sendProt = cif.GetValue("Tcp");
                        Sendmsg.Data = Encoding.UTF8.GetBytes(txtRdpKey.Text);
                        Sendmsg.recIP = txtIp.Text.ToString();
                        Sendmsg.recProt = cif.GetValue("Tcp");
                        Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.InviteExit);
                        issend = p2p.Send(Sendmsg);
                    }
                    catch (Exception)
                    {
                    }
                }
                Sendmsg = new TcpP2p.Msg();
                Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                Sendmsg.sendName = txtNiceName.Text;
                Sendmsg.sendProt = cif.GetValue("Tcp");
                Sendmsg.Data = Encoding.UTF8.GetBytes(txtRdpKey.Text);
                Sendmsg.recIP = txtIp.Text.ToString();
                Sendmsg.recProt = cif.GetValue("Tcp");
                Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.ExitUpdate);
                issend = p2p.Send(Sendmsg);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        ///  远程-加入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeskShareJoin_Click(object sender, EventArgs e)
        {
            if (txtNiceName.Text == "")
            {
                new MessageBoxCustom("系统提示", "必须输入昵称", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                return;
            }
            if (txtIp.Text != "")
            {
                if (txtRdpPswKey.Text == "" || txtRdpPswKey.Text.Length != 4)
                {
                    new MessageBoxCustom("系统提示", "输入密码", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    return;
                }
                IPAddress ip;
                if (IPAddress.TryParse(txtIp.Text, out ip))
                {
                    TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                    Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                    Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                    Sendmsg.sendName = txtNiceName.Text;
                    Sendmsg.sendProt = cif.GetValue("Tcp");
                    Sendmsg.Data = Encoding.UTF8.GetBytes(txtRdpPswKey.Text);
                    Sendmsg.recIP = ip.ToString();
                    Sendmsg.recProt = cif.GetValue("Tcp");
                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinApply);
                    bool issend = p2p.Send(Sendmsg);
                    if (!issend)
                    {
                        BtnDeskShare.IsEnabled = false;
                        BtnDeskShareJoin.IsEnabled = false;
                        BtnDeskShareInvite.IsEnabled = false;
                        txtIp.IsEnabled = false;
                        new MessageBoxCustom("系统提示", "加入请求失败请检查ip", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                        return;
                    }
                    cif.SaveValue("Ip", cbIp.SelectedValue.ToString());
                    cif.SaveValue("NickName", txtNiceName.Text);
                }
                else
                {
                    BtnDeskShare.IsEnabled = true;
                    BtnDeskShareJoin.IsEnabled = true;
                    BtnDeskShareInvite.IsEnabled = true;
                    new MessageBoxCustom("系统提示", "输入正确IP后加入", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    return;
                }
            }
        }

        /// <summary>
        ///  远程-邀请
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeskShareInvite_Click(object sender, EventArgs e)
        {
            if (txtNiceName.Text == "")
            {
                new MessageBoxCustom("系统提示", "必须输入昵称", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                return;
            }
            rbMode2.IsChecked = true;//邀请默认控制模式
            BtnDeskShare_Click(sender, e);
            if (txtIp.Text != "")
            {
                IPAddress ip;
                if (IPAddress.TryParse(txtIp.Text, out ip))
                {
                    TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                    Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                    Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                    Sendmsg.sendName = txtNiceName.Text;
                    Sendmsg.sendProt = cif.GetValue("Tcp");
                    Sendmsg.Data = Encoding.UTF8.GetBytes(invitationString);
                    Sendmsg.recIP = ip.ToString();
                    Sendmsg.recProt = cif.GetValue("Tcp");
                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.InviteApply);
                    bool issend = p2p.Send(Sendmsg);
                    if (!issend)
                    {
                        Stop();
                        new MessageBoxCustom("系统提示", "邀请失败请检查ip", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                        return;
                    }
                    cif.SaveValue("Ip", cbIp.SelectedValue.ToString());
                    cif.SaveValue("NickName", txtNiceName.Text);
                }
                else
                {
                    BtnDeskShare.IsEnabled = true;
                    BtnDeskShareJoin.IsEnabled = true;
                    BtnDeskShareInvite.IsEnabled = true;
                    new MessageBoxCustom("系统提示", "输入正确IP后邀请", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    return;
                }
            }
        }

        //连接接入
        private void RdpSessionOnOnAttendeeDisconnected(object pDisconnectInfo)
        {
            IRDPSRAPIAttendeeDisconnectInfo pDiscInfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;
        }
        //连接断开

        private void RdpSessionOnOnAttendeeConnected(object pObjAttendee)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
        }
        //被控时候
        private void OnAttendeeConnected(object pObjAttendee)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
        }
        //不给控制的时候
        void OnAttendeeDisconnected(object pDisconnectInfo)
        {
            IRDPSRAPIAttendeeDisconnectInfo pDiscInfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;
        }

        /// <summary>
        /// 更改控制级别
        /// </summary>
        /// <param name="pObjAttendee"></param>
        /// <param name="RequestedLevel"></param>
        void OnControlLevelChangeRequest(object pObjAttendee, CTRL_LEVEL RequestedLevel)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = RequestedLevel;
        }

        /// <summary>
        /// 本地ip实体
        /// </summary>
        public class IpModel
        {
            public string id { get; set; }
            public string title { get; set; }
        }
        #endregion

        #region 通信
        void P2P_ReceivMsg(TcpP2p.Msg msgstr)
        {
            try
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TcpP2p.ShowMessage update = new TcpP2p.ShowMessage(this.P2P_ReceivMsg);
                        this.Dispatcher.Invoke(update, new object[] { msgstr });

                    }));
                }
                else
                {
                    if (msgstr.type == Convert.ToInt32(TcpP2p.msgType.SendText))
                    {
                        if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinApply))
                        {
                            if (Encoding.UTF8.GetString(msgstr.Data) == txtRdpKey.Text)
                            {
                                if (IsDeskShare)
                                {//加入失败不是共享状态
                                    try
                                    {
                                        TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                        Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                        Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                        Sendmsg.sendName = txtNiceName.Text;
                                        Sendmsg.sendProt = cif.GetValue("Tcp");
                                        Sendmsg.recIP = msgstr.sendIP;
                                        Sendmsg.recProt = cif.GetValue("Tcp");
                                        Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinFail);
                                        p2p.Send(Sendmsg);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                else
                                {//推送密钥
                                    try
                                    {
                                        bool? Result = new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
                                        if (Convert.ToBoolean(Result))
                                        {//同意加入
                                            TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                            Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                            Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                            Sendmsg.sendName = txtNiceName.Text;
                                            Sendmsg.sendProt = cif.GetValue("Tcp");
                                            Sendmsg.Data = Encoding.UTF8.GetBytes(invitationString);
                                            Sendmsg.recIP = msgstr.sendIP;
                                            Sendmsg.recProt = cif.GetValue("Tcp");
                                            Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinSuccess);
                                            p2p.Send(Sendmsg);
                                        }
                                        else
                                        {//加入被拒
                                            TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                            Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                            Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                            Sendmsg.sendName = txtNiceName.Text;
                                            Sendmsg.sendProt = cif.GetValue("Tcp");
                                            Sendmsg.recIP = msgstr.sendIP;
                                            Sendmsg.recProt = cif.GetValue("Tcp");
                                            Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinRefuse);
                                            p2p.Send(Sendmsg);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            else
                            {//密码错误
                                try
                                {
                                    TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                    Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                    Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                    Sendmsg.sendName = txtNiceName.Text;
                                    Sendmsg.sendProt = cif.GetValue("Tcp");
                                    Sendmsg.recIP = msgstr.sendIP;
                                    Sendmsg.recProt = cif.GetValue("Tcp");
                                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinPassword);
                                    p2p.Send(Sendmsg);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinSuccess))
                        {//加入成功
                            BtnDeskShare.IsEnabled = false;
                            BtnDeskShareJoin.IsEnabled = false;
                            BtnDeskShareInvite.IsEnabled = false;
                            txtIp.IsEnabled = false;
                            DeskShareView win = new DeskShareView(Encoding.UTF8.GetString(msgstr.Data), txtNiceName.Text, _LvAudienceUpdate);
                            win.Show();
                            //推送加入成功
                            try
                            {
                                TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                Sendmsg.sendName = txtNiceName.Text;
                                Sendmsg.sendProt = cif.GetValue("Tcp");
                                Sendmsg.recIP = msgstr.sendIP;
                                Sendmsg.recProt = cif.GetValue("Tcp");
                                Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinUpdate);
                                p2p.Send(Sendmsg);
                            }
                            catch (Exception ex)
                            {

                            }
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinRefuse))
                        {//加入拒绝
                            new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Info, MessageButtons.Ok).ShowDialog();
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinUpdate))
                        {//加入成功更新列表
                            #region 添加观众
                            string content = "[" + msgstr.sendName.PadRight(12, ' ') + "]";
                            content += msgstr.sendIP.PadLeft(15, ' ');
                            bool isAdd = true;
                            for (int i = 0; i < LvAudience.Items.Count; i++)
                            {
                                TextBlock text = LvAudience.Items[i] as TextBlock;
                                if (text.Text == content.Trim())
                                {
                                    isAdd = false;
                                    break;
                                }
                            }
                            if (isAdd)
                            {
                                TextBlock text = new TextBlock();
                                text.Text = content.Trim();
                                LvAudience.Items.Add(text);
                            }
                            #endregion
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.ExitUpdate))
                        {//退出更新列表
                            #region 删除观众
                            string content = "[" + msgstr.sendName.PadLeft(8, ' ') + "]";
                            content += msgstr.sendIP;
                            int index = 0;
                            for (int i = 0; i < LvAudience.Items.Count; i++)
                            {
                                TextBlock text = LvAudience.Items[i] as TextBlock;
                                if (text.Text == content.Trim())
                                {
                                    index = i;
                                    break;
                                }
                            }
                            LvAudience.Items.Remove(LvAudience.Items[index]);
                            #endregion         
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinPassword))
                        {//加入失败密码错误
                            new MessageBoxCustom("系统提示", "加入失败，密码错误", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                            BtnDeskShareJoin.IsEnabled = true;
                            BtnDeskShareInvite.IsEnabled = true;
                            BtnDeskShare.IsEnabled = true;
                            txtIp.IsEnabled = true;
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinFail))
                        {//加入失败会议没有
                            new MessageBoxCustom("系统提示", "加入失败，未开启共享或已经结束", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                            BtnDeskShareJoin.IsEnabled = true;
                            BtnDeskShareInvite.IsEnabled = true;
                            BtnDeskShare.IsEnabled = true;
                            txtIp.IsEnabled = true;
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.InviteApply))
                        {//邀请请求
                            bool? Result = new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
                            if (Convert.ToBoolean(Result))
                            {//同意邀请
                                BtnDeskShare.IsEnabled = false;
                                BtnDeskShareJoin.IsEnabled = false;
                                BtnDeskShareInvite.IsEnabled = false;
                                txtIp.IsEnabled = false;
                                isInvite = true;
                                DeskShareView win = new DeskShareView(Encoding.UTF8.GetString(msgstr.Data), txtNiceName.Text, _LvAudienceUpdate);
                                win.Show();
                                //推送加入成功
                                try
                                {
                                    txtIp.Text = msgstr.sendIP;
                                    TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                    Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                    Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                    Sendmsg.sendName = txtNiceName.Text;
                                    Sendmsg.sendProt = cif.GetValue("Tcp");
                                    Sendmsg.recIP = msgstr.sendIP;
                                    Sendmsg.recProt = cif.GetValue("Tcp");
                                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinUpdate);
                                    p2p.Send(Sendmsg);
                                }
                                catch (Exception ex)
                                {

                                }
                                return;
                            }
                            else
                            {//邀请被拒
                                TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                                Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                                Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                                Sendmsg.sendName = txtNiceName.Text;
                                Sendmsg.sendProt = cif.GetValue("Tcp");
                                Sendmsg.recIP = msgstr.sendIP;
                                Sendmsg.recProt = cif.GetValue("Tcp");
                                Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.InviteRefuse);
                                p2p.Send(Sendmsg);
                            }

                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.InviteRefuse))
                        {//邀请拒绝
                            Stop();
                            new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Info, MessageButtons.Ok).ShowDialog();
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.InviteExit))
                        {//邀请拒绝
                            Stop();
                            new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Info, MessageButtons.Ok).ShowDialog();
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.ExElistUpdate))
                        {//刷新exe列表
                            ExeRefresh();
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.AlarmlistUpdate))
                        {//刷新闹钟列表
                            AlarmRefresh();
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region 剪贴板
        /// <summary>
        /// 监听模式-快捷键赋值直接进入list
        /// </summary>
        private void AddLvClipboardList()
        {
            if (IsClipboardMode)
            {
                //显示剪贴板中的文本信息
                if (Clipboard.ContainsText())
                {
                    string[] content = { Clipboard.GetText() };
                    bool isAdd = true;
                    if (content[0].Trim() == "")
                    {
                        return;
                    }
                    for (int i = 0; i < LvClipboard.Items.Count; i++)
                    {
                        TextBlock text = LvClipboard.Items[i] as TextBlock;
                        if (text.Text == content[0].Trim())
                        {
                            isAdd = false;
                            break;
                        }
                    }
                    if (isAdd)
                    {
                        TextBlock text = new TextBlock();
                        text.Text = content[0].Trim();
                        LvClipboard.Items.Add(text);
                        ClipboardCache();
                    }
                }
            }
        }

        /// <summary>
        /// 缓存剪贴板
        /// </summary>
        private void ClipboardCache()
        {
            List<string> slist = new List<string>();
            for (int i = 0; i < LvClipboard.Items.Count; i++)
            {
                TextBlock temp = LvClipboard.Items[i] as TextBlock;

                if (!string.IsNullOrEmpty(temp.Text))
                {
                    slist.Add(Encrypt.AESEncrypt(temp.Text, encrypt_key));
                }
            }
            string json = JsonConvert.SerializeObject(slist);
            Log.WriteJson("Clipboard", json);
        }

        #region 拖拽排序
        private void LvClipboard_OnPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(LvClipboard);  // 获取位置

                #region 源位置
                HitTestResult result = VisualTreeHelper.HitTest(LvClipboard, pos);  //根据位置得到result
                if (result == null)
                {
                    return;    //找不到 返回
                }
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null || listBoxItem.Content != LvClipboard.SelectedItem)
                {
                    return;
                }
                #endregion

                System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as TextBlock);
                DragDrop.DoDragDrop(LvClipboard, dataObj, System.Windows.DragDropEffects.Move);  //调用方法
            }
        }

        private void LvClipboard_OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            var pos = e.GetPosition(LvClipboard);   //获取位置
            var result = VisualTreeHelper.HitTest(LvClipboard, pos);   //根据位置得到result
            if (result == null)
            {
                return;   //找不到 返回
            }
            #region 查找元数据
            var sourcePerson = e.Data.GetData(typeof(TextBlock)) as TextBlock;
            if (sourcePerson == null)
            {
                return;
            }
            #endregion

            #region  查找目标数据
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }
            var targetPerson = listBoxItem.Content as TextBlock;
            if (ReferenceEquals(targetPerson, sourcePerson))
            {
                return;
            }
            #endregion


            int sourceIndex = LvClipboard.Items.IndexOf(sourcePerson);
            int targetIndex = LvClipboard.Items.IndexOf(targetPerson);

            if (sourceIndex < targetIndex)  //从上面移动到下面
            {
                LvClipboard.Items.Remove(sourcePerson);  //删除源
                Console.WriteLine(LvClipboard.Items.IndexOf(targetPerson) + 1);
                LvClipboard.Items.Insert(LvClipboard.Items.IndexOf(targetPerson) + 1, sourcePerson);
            }
            else if (sourceIndex > targetIndex)
            {
                LvClipboard.Items.Remove(sourcePerson);  //删除源
                Console.WriteLine(LvClipboard.Items.IndexOf(targetPerson));
                LvClipboard.Items.Insert(LvClipboard.Items.IndexOf(targetPerson), sourcePerson);
            }

        }

        internal static class Utils
        {
            //根据子元素查找父元素
            public static T FindVisualParent<T>(DependencyObject obj) where T : class
            {
                while (obj != null)
                {
                    if (obj is T)
                        return obj as T;

                    obj = VisualTreeHelper.GetParent(obj);
                }
                return null;
            }
        }
        #endregion

        /// <summary>
        ///  双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvClipboard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsClipboardDelete)
            {
                LvClipboard.Items.Remove(LvClipboard.Items[LvClipboard.SelectedIndex]);
                ClipboardCache();
            }
            else
            {
                TextBlock text = LvClipboard.Items[LvClipboard.SelectedIndex] as TextBlock;
                Clipboard.SetDataObject(text.Text);
            }
        }

        /// <summary>
        ///  置顶按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTopMost_Click(object sender, EventArgs e)
        {
            if (Topmost)
            {
                BtnTopMost.ToolTip = "置顶";
                Topmost = false;
            }
            else
            {
                BtnTopMost.ToolTip = "取消置顶";
                Topmost = true;
            }
        }

        /// <summary>
        ///  剪贴板按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClipboard_Click(object sender, EventArgs e)
        {
            if (BtnClipboard.ToolTip.Equals("监听"))
            {
                BtnClipboard.ToolTip = "取消监听";
                IsClipboardMode = true;
            }
            else
            {
                BtnClipboard.ToolTip = "监听";
                IsClipboardMode = false;
            }
        }

        /// <summary>
        ///  删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (BtnDelete.ToolTip.Equals("删除"))
            {
                BtnDelete.ToolTip = "取消删除";
                IsClipboardDelete = true;
            }
            else
            {
                BtnDelete.ToolTip = "删除";
                IsClipboardDelete = false;
            }
        }
        #endregion

        #region 设置
        /// <summary>
        /// 语言实体
        /// </summary>
        public class LanguageModel
        {
            public string id { get; set; }
            public string title { get; set; }
        }

        /// <summary>
        ///  设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetLanguage_Click(object sender, EventArgs e)
        {
            if (txtAlarmTimer.Text != "")
            {
                cif.SaveValue("Alarm", txtAlarmTimer.Text.ToString());
            }
            cif.SaveValue("Language", cbLanguage.SelectedValue.ToString());
            string requestedCulture = @"Resources/Language/" + cbLanguage.SelectedValue.ToString() + ".xaml";
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in System.Windows.Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source != null) { dictionaryList.Add(dictionary); }
            }
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            System.Windows.Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

            if (txtClipboardPassword.Text != "")
            {
                string file = path + "Json\\" + "\\" + "Clipboard.Json";
                if (File.Exists(file))
                {
                    bool? Result = new MessageBoxCustom("adssad", "从新设置密码清空缓存", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
                    if (Convert.ToBoolean(Result))
                    {
                        File.Delete(file);
                        cif.SaveValue("Encrypt", txtClipboardPassword.Text);
                        LvClipboard.Items.Clear();
                    }
                }
            }
            try
            {
                cif.SaveValue("Start", rbStart1.IsChecked.ToString());
                //根据情况是否写入注册表
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
                if (Convert.ToBoolean(rbStart1.IsChecked))
                {
                    if (regKey == null)
                        regKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");
                    regKey.SetValue("H_WorkTools", System.Windows.Forms.Application.ExecutablePath);
                }
                else
                {
                    if (regKey != null)
                    {
                        if (regKey.GetValue("H_WorkTools") != null)
                            regKey.DeleteValue("H_WorkTools");
                    }
                }
                regKey.Close();
            }
            catch (Exception)
            {
                new MessageBoxCustom("adssad", "开机自动启动失败，请用管理模式打开软件后再试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
            }
            #region 快捷键重新设置
            try
            {//卸载快捷键
                var hwnd = new WindowInteropHelper(this).Handle;
                UnRegist(hwnd, () => { });//停止共享
            }
            catch (Exception) { }
            #endregion

            #region 截图、共享、剪贴板 快捷键保存
            cif.SaveValue("ScreenCapture_Key",
                (cbScreenCaptureCtrl.IsChecked == true ? "1" : "0") + "|" +
                (cbScreenCaptureAlt.IsChecked == true ? "1" : "0") + "|" +
                (cbScreenCaptureShift.IsChecked == true ? "1" : "0") + "|" +
                txtScreenCaptureKey.Text);
            cif.SaveValue("ScreenCapture_LastKey",
                (cbLastCtrl.IsChecked == true ? "1" : "0") + "|" +
                (cbLastAlt.IsChecked == true ? "1" : "0") + "|" +
                (cbLastShift.IsChecked == true ? "1" : "0") + "|" +
                txtLastKey.Text);
            cif.SaveValue("ScreenCapture_DiffKey",
                (cbDiffCtrl.IsChecked == true ? "1" : "0") + "|" +
                (cbDiffAlt.IsChecked == true ? "1" : "0") + "|" +
                (cbDiffShift.IsChecked == true ? "1" : "0") + "|" +
                txtDiffKey.Text);
            cif.SaveValue("ScreenCapture_Opacity", ScreenCaptureOpacity.Value.ToString());
            cif.SaveValue("ScreenCapture_Title", ScreenCaptureTitle.Text);
            cif.SaveValue("DeskShare", cbDeskShare.SelectedIndex + "|" + txtDeskShareKey.Text);
            cif.SaveValue("Clipboard", cbClipboardCtrl.SelectedIndex + "|" + cbClipboardNumber.SelectedIndex);
            #endregion

            RegistAll();//注册快捷键
        }
        #endregion

        #region 应用中心
        #region 拖拽排序
        private void LvExe_OnPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(LvExe);  // 获取位置
                #region 源位置
                HitTestResult result = VisualTreeHelper.HitTest(LvExe, pos);  //根据位置得到result
                if (result == null)
                {
                    return;    //找不到 返回
                }
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null || listBoxItem.Content != LvExe.SelectedItem)
                {
                    return;
                }
                #endregion

                System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as ExeModel);
                DragDrop.DoDragDrop(LvExe, dataObj, System.Windows.DragDropEffects.Move);  //调用方法
            }
        }

        private void LvExe_OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            var pos = e.GetPosition(LvExe);   //获取位置
            var result = VisualTreeHelper.HitTest(LvExe, pos);   //根据位置得到result
            if (result == null)
            {
                return;   //找不到 返回
            }
            #region 查找元数据
            ExeModel sourcePerson = e.Data.GetData(typeof(ExeModel)) as ExeModel;
            if (sourcePerson == null)
            {
                return;
            }
            #endregion

            #region  查找目标数据
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }
            ExeModel targetPerson = listBoxItem.Content as ExeModel;
            if (ReferenceEquals(targetPerson, sourcePerson))
            {
                return;
            }
            #endregion


            int sourceIndex = LvExe.Items.IndexOf(sourcePerson);
            int targetIndex = LvExe.Items.IndexOf(targetPerson);

            List<ExeModel> list_temp = JsonConvert.DeserializeObject<List<ExeModel>>(JsonConvert.SerializeObject(LvExe.ItemsSource));
            list_temp.RemoveAt(sourceIndex);
            list_temp.Insert(targetIndex, sourcePerson);
            LvExe.ItemsSource = list_temp;
            ExeSave();
            ExeRefresh();
        }

        private void txtApp_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = txtApp.Text.Trim();
            try
            {
                if (txt == "")
                {
                    ExeRefresh();
                }
                else
                {
                    LvExe.ItemsSource = null;
                    using (var db = new LiteDatabase(path + "worktools.db"))
                    {
                        var exemodel = db.GetCollection<ExeModel>("ExeModel");
                        List<ExeModel> list = JsonConvert.DeserializeObject<List<ExeModel>>(JsonConvert.SerializeObject(exemodel.Find(x => x.title.Contains(txt))));
                        for (int i = 0; i < list.Count; i++)
                            list[i].ico = path + list[i].ico;
                        LvExe.ItemsSource = list;
                        db.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 应用双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                System.Windows.Controls.Image img = (System.Windows.Controls.Image)e.Source;
                if (!IsExeDelete)
                {//启动

                    System.Diagnostics.ProcessStartInfo pinfo = new System.Diagnostics.ProcessStartInfo();
                    pinfo.UseShellExecute = true;
                    pinfo.FileName = ((ExeModel)img.DataContext).path;
                    //启动进程
                    try
                    {
                        Process p = Process.Start(pinfo);
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {//删除
                    using (var db = new LiteDatabase(path + "worktools.db"))
                    {
                        var exemodel = db.GetCollection<ExeModel>("ExeModel");
                        exemodel.DeleteMany(x => x.path == ((ExeModel)img.DataContext).path);
                        db.Dispose();
                    }
                    ExeRefresh();
                }
            }
        }

        /// <summary>
        /// exe保存
        /// </summary>
        public void ExeSave()
        {
            using (var db = new LiteDatabase(path + "worktools.db"))
            {
                var exemodel = db.GetCollection<ExeModel>("ExeModel");
                exemodel.DeleteAll();
                for (int i = 0; i < LvExe.Items.Count; i++)
                {
                    ExeModel model = (ExeModel)LvExe.Items[i];
                    int index = model.ico.LastIndexOf("\\");  //返回“//”最后一次出现的位置
                    model.ico = "ico\\" + model.ico.Substring(index + 1);  //截取文件名
                    exemodel.Insert(model);
                }
                db.Dispose();
            }
        }

        /// <summary>
        /// exe列表刷新
        /// </summary>
        public void ExeRefresh()
        {
            LvExe.ItemsSource = null;
            using (var db = new LiteDatabase(path + "worktools.db"))
            {
                var exemodel = db.GetCollection<ExeModel>("ExeModel");
                List<ExeModel> list = JsonConvert.DeserializeObject<List<ExeModel>>(JsonConvert.SerializeObject(exemodel.FindAll()));
                for (int i = 0; i < list.Count; i++)
                    list[i].ico = path + list[i].ico;
                LvExe.ItemsSource = list;
                db.Dispose();
            }
        }

        /// <summary>
        ///  删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExeDelete_Click(object sender, EventArgs e)
        {
            if (BtnExeDelete.ToolTip.Equals("删除"))
            {
                BtnExeDelete.ToolTip = "取消删除";
                IsExeDelete = true;
            }
            else
            {
                BtnExeDelete.ToolTip = "删除";
                IsExeDelete = false;
            }
        }
        #endregion

        #region 注册快捷键有关

        static int keyid = 10;
        public static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();

        public delegate void HotKeyCallBackHanlder();

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="window">持有快捷键窗口</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="key">快捷键</param>
        /// <param name="callBack">回调函数</param>
        public void Regist(Window window, HotkeyModifiers fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            AddClipboardFormatListener(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            MainWindow aa = new MainWindow();
            _hwndSource.AddHook(aa.WndProc);

            int id = keyid++;

            var vk = KeyInterop.VirtualKeyFromKey(key);
            if (!RegisterHotKey(hwnd, id, fsModifiers, (uint)vk))
            {
                throw new Exception();
            }
            keymap[id] = callBack;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="hWnd">持有快捷键窗口的句柄</param>
        /// <param name="callBack">回调函数</param>
        public static void UnRegist(IntPtr hWnd, HotKeyCallBackHanlder callBack)
        {
            foreach (KeyValuePair<int, HotKeyCallBackHanlder> var in keymap)
            {
                UnregisterHotKey(hWnd, var.Key);
            }
            RemoveClipboardFormatListener(hWnd);
            keymap.Clear();
        }

        public enum HotkeyModifiers
        {
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8,
            MOD_ALT_CONTROL = (0x1 | 0x2),
            MOD_ALT_SHIFT = (0x1 | 0x4),
            MOD_CONTROLT_SHIFT = (0x2 | 0x4),
            MOD_CONTROLT_SHIFT_ALT = (0x1 | 0x2 | 0x4)
        }

        /// <summary>
        /// select 转enum
        /// </summary>
        /// <param name="select">select</param>
        public HotkeyModifiers GetHotkeyModifiers(string select)
        {
            switch (select)
            {
                case "Alt+Shift": return HotkeyModifiers.MOD_ALT_SHIFT;
                case "Alt+Ctrl": return HotkeyModifiers.MOD_ALT_CONTROL;
                case "Ctrl+Shift": return HotkeyModifiers.MOD_CONTROLT_SHIFT;
                case "Alt": return HotkeyModifiers.MOD_ALT;
                case "Shift": return HotkeyModifiers.MOD_SHIFT;
                case "Ctrl": return HotkeyModifiers.MOD_CONTROL;
            }
            return HotkeyModifiers.MOD_WIN;
        }

        /// <summary>
        /// 注册所有快捷键
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void RegistAll()
        {
            #region  剪贴板 默认快捷键Ctrl + 1-10
            try
            {
                HotkeyModifiers key = GetHotkeyModifiers(cbClipboardCtrl.Text);
                if (cbClipboardNumber.SelectedIndex == 1)
                {
                    Regist(this, key, Key.NumPad0, () => { try { Clipboard.SetDataObject((LvClipboard.Items[0] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad1, () => { try { Clipboard.SetDataObject((LvClipboard.Items[1] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad2, () => { try { Clipboard.SetDataObject((LvClipboard.Items[2] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad3, () => { try { Clipboard.SetDataObject((LvClipboard.Items[3] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad4, () => { try { Clipboard.SetDataObject((LvClipboard.Items[4] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad5, () => { try { Clipboard.SetDataObject((LvClipboard.Items[5] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad6, () => { try { Clipboard.SetDataObject((LvClipboard.Items[6] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad7, () => { try { Clipboard.SetDataObject((LvClipboard.Items[7] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad8, () => { try { Clipboard.SetDataObject((LvClipboard.Items[8] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.NumPad9, () => { try { Clipboard.SetDataObject((LvClipboard.Items[9] as TextBlock).Text); } catch { } });

                }
                else
                {
                    Regist(this, key, Key.D0, () => { try { Clipboard.SetDataObject((LvClipboard.Items[0] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D1, () => { try { Clipboard.SetDataObject((LvClipboard.Items[1] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D2, () => { try { Clipboard.SetDataObject((LvClipboard.Items[2] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D3, () => { try { Clipboard.SetDataObject((LvClipboard.Items[3] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D4, () => { try { Clipboard.SetDataObject((LvClipboard.Items[4] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D5, () => { try { Clipboard.SetDataObject((LvClipboard.Items[5] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D6, () => { try { Clipboard.SetDataObject((LvClipboard.Items[6] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D7, () => { try { Clipboard.SetDataObject((LvClipboard.Items[7] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D8, () => { try { Clipboard.SetDataObject((LvClipboard.Items[8] as TextBlock).Text); } catch { } });
                    Regist(this, key, Key.D9, () => { try { Clipboard.SetDataObject((LvClipboard.Items[9] as TextBlock).Text); } catch { } });
                }
            }
            catch (Exception)
            {
                new MessageBoxCustom("adssad", "剪贴板快捷键设置失败，修改后再设置试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
                return;
            }
            #endregion

            #region 停止共享快捷键
            try
            {
                HotkeyModifiers key = GetHotkeyModifiers(cbDeskShare.Text);
                Regist(this, key, (Key)Enum.Parse(typeof(Key), txtDeskShareKey.Text), () =>
                { try { Stop(); } catch { } });//停止共享
            }
            catch (Exception)
            {
                new MessageBoxCustom("adssad", "停止共享快捷键设置失败，修改后再设置试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
                return;
            }
            #endregion

            #region 截图快捷键
            try
            {
                uint keyNumber = 0;
                if (Convert.ToBoolean(cbLastCtrl.IsChecked))
                    keyNumber = keyNumber | 0x2;
                if (Convert.ToBoolean(cbLastAlt.IsChecked))
                    keyNumber = keyNumber | 0x1;
                if (Convert.ToBoolean(cbLastShift.IsChecked))
                    keyNumber = keyNumber | 0x4;
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), txtScreenCaptureKey.Text), () =>
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(0);
                    m_frmCapture.Show();
                });
            }
            catch
            {
                new MessageBoxCustom("adssad", "截图快捷键设置失败，修改后再设置试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
                return;
            }
            #endregion

            #region 截图上一次快捷键
            try
            {
                uint keyNumber = 0;
                if (Convert.ToBoolean(cbLastCtrl.IsChecked))
                    keyNumber = keyNumber | 0x2;
                if (Convert.ToBoolean(cbLastAlt.IsChecked))
                    keyNumber = keyNumber | 0x1;
                if (Convert.ToBoolean(cbLastShift.IsChecked))
                    keyNumber = keyNumber | 0x4;
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), txtLastKey.Text), () =>
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(1);
                    m_frmCapture.Show();
                });
            }
            catch
            {
                new MessageBoxCustom("adssad", "截图上一次快捷键设置失败，修改后再设置试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
                return;
            }
            #endregion

            #region 截图对比快捷键
            try
            {
                uint keyNumber = 0;
                if (Convert.ToBoolean(cbDiffCtrl.IsChecked))
                    keyNumber = keyNumber | 0x2;
                if (Convert.ToBoolean(cbDiffAlt.IsChecked))
                    keyNumber = keyNumber | 0x1;
                if (Convert.ToBoolean(cbDiffShift.IsChecked))
                    keyNumber = keyNumber | 0x4;
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), txtDiffKey.Text), () =>
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(2);
                    m_frmCapture.Show();
                });
            }
            catch
            {
                new MessageBoxCustom("adssad", "截图对比快捷键设置失败，修改后再设置试试", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
                return;
            }
            #endregion
        }
        #endregion

        #region 闹铃

        private void txtAlarm_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = txtAlarm.Text.Trim();
            try
            {
                if (txt == "")
                {
                    AlarmRefresh();
                }
                else
                {
                    LvAlarm.ItemsSource = null;
                    using (var db = new LiteDatabase(path + "worktools.db"))
                    {
                        var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                        List<AlarmModel> list = JsonConvert.DeserializeObject<List<AlarmModel>>(JsonConvert.SerializeObject(alarmmodel.Find(x => x.title.Contains(txt)).OrderBy(x => x.lastTime)));
                        for (int i = 0; i < list.Count; i++)
                            list[i].data = GetAlarmType(list[i]);
                        LvAlarm.ItemsSource = list;
                        db.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 闹钟列表刷新
        /// </summary>
        public void AlarmRefresh()
        {
            LvExe.ItemsSource = null;
            using (var db = new LiteDatabase(path + "worktools.db"))
            {
                var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                List<AlarmModel> list = JsonConvert.DeserializeObject<List<AlarmModel>>(JsonConvert.SerializeObject(alarmmodel.FindAll().OrderBy(x => x.lastTime)));
                for (int i = 0; i < list.Count; i++)
                    list[i].data = GetAlarmType(list[i]);
                LvAlarm.ItemsSource = list;
                db.Dispose();
            }
        }

        public string GetAlarmType(AlarmModel t)
        {
            switch (t.alarmType)
            {
                case "0":
                    return "[指定] " + t.data + " " + t.time;
                case "1":
                    return "[每周] " + t.data;
                case "2":
                    return "[每月] " + t.data;
            }
            return "";
        }

        /// <summary>
        ///  新增按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmInsert_Click(object sender, EventArgs e)
        {
            AlarmTemp.json = "";
        }

        /// <summary>
        ///  双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvAlarm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AlarmModel model = (AlarmModel)((System.Windows.Controls.ListViewItem)sender).Content; //Casting back to the binded Track
            if (IsAlarmDelete)
            {//删除
                using (var db = new LiteDatabase(path + "worktools.db"))
                {
                    var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                    alarmmodel.DeleteMany(x => x.id == model.id);
                    db.Dispose();
                }
                AlarmRefresh();
            }
            else
            {
                using (var db = new LiteDatabase(path + "worktools.db"))
                {
                    var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                    model = alarmmodel.FindOne(x => x.id == model.id);
                    AlarmTemp.json = JsonConvert.SerializeObject(model);
                    db.Dispose();
                }
                var vm = this.DataContext as MainWindowViewModel;
                vm.AlarmInsertCommand.Execute("");
            }
        }

        /// <summary>
        ///  删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmDelete_Click(object sender, EventArgs e)
        {
            if (BtnAlarmDelete.ToolTip.Equals("删除"))
            {
                BtnAlarmDelete.ToolTip = "取消删除";
                IsAlarmDelete = true;
            }
            else
            {
                BtnAlarmDelete.ToolTip = "删除";
                IsAlarmDelete = false;
            }
        }
        #endregion

        private void Button_Click1(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(LvAlarm);  // 获取位置

            #region 源位置
            HitTestResult result = VisualTreeHelper.HitTest(LvAlarm, pos);  //根据位置得到result
            if (result == null)
            {
                return;    //找不到 返回
            }
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null || listBoxItem.Content != LvAlarm.SelectedItem)
            {
                return;
            }
            #endregion

            System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as TextBlock);
            DragDrop.DoDragDrop(LvAlarm, dataObj, System.Windows.DragDropEffects.Move);  //调用方法
        }
    }

    #region 弹出框有关
    internal class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Action<object> a = new Action<object>(AlarmInsert);
            AlarmInsertCommand = new DelegateCommand(a);
        }
        public DelegateCommand AlarmInsertCommand { get; private set; }
        public async void AlarmInsert(object src)
        {
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
            string showtitle = "";
            if (AlarmTemp.json == "")
                showtitle = "Add";
            else
                showtitle = "Edit";
            CommonDialogResult result = await CommonDialogShow.ShowInsertAlarm("Root", "Add") as CommonDialogResult;
            if (result.Button == CommonDialogButton.Ok)
            {
                await CommonDialogShow.ShowCurcularProgress("Root", () =>
                {
                    try
                    {
                        using (var db = new LiteDatabase(path + "worktools.db"))
                        {
                            var alarmmodel = db.GetCollection<AlarmModel>("AlarmModel");
                            AlarmModel model = JsonConvert.DeserializeObject<AlarmModel>(result.Data.ToString());
                            if (AlarmTemp.json == "")
                            {
                                alarmmodel.Insert(model);
                            }
                            else
                            {
                                alarmmodel.Update(model);
                            }
                            db.Dispose();
                        }
                    }
                    catch (Exception ee)
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            new MessageBoxCustom("adssad", "添加失败，不可以重复添加 ", MessageType.Info, MessageButtons.Ok).ShowDialog();
                        }));
                    }
                    TcpP2p p2p = new TcpP2p();
                    Config cif = new Config();
                    TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                    Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                    Sendmsg.sendIP = src.ToString();
                    Sendmsg.sendProt = cif.GetValue("Tcp");
                    Sendmsg.recIP = src.ToString();
                    Sendmsg.recProt = cif.GetValue("Tcp");
                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.AlarmlistUpdate);
                    p2p.Send(Sendmsg);
                });
            }
            System.Diagnostics.Debug.WriteLine("MouseDoubleClick Command.");
        }
        private DelegateCommand AppInsertCommand
        {
            get
            {
                return new DelegateCommand(async (src) =>
                {
                    string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
                    CommonDialogResult result = await CommonDialogShow.ShowInsertExe("Root", "Add") as CommonDialogResult;
                    if (result.Button == CommonDialogButton.Ok)
                    {
                        await CommonDialogShow.ShowCurcularProgress("Root", () =>
                        {
                            try
                            {
                                using (var db = new LiteDatabase(path + "worktools.db"))
                                {
                                    var exemodel = db.GetCollection<ExeModel>("ExeModel");
                                    ExeModel model = JsonConvert.DeserializeObject<ExeModel>(result.Data.ToString());
                                    exemodel.Insert(model);
                                    db.Dispose();
                                }
                            }
                            catch (Exception ee)
                            {
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    new MessageBoxCustom("adssad", "添加失败，不可以重复添加 ", MessageType.Info, MessageButtons.Ok).ShowDialog();
                                }));
                            }
                            TcpP2p p2p = new TcpP2p();
                            Config cif = new Config();
                            TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                            Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                            Sendmsg.sendIP = src.ToString();
                            Sendmsg.sendProt = cif.GetValue("Tcp");
                            Sendmsg.recIP = src.ToString();
                            Sendmsg.recProt = cif.GetValue("Tcp");
                            Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.ExElistUpdate);
                            p2p.Send(Sendmsg);
                        });
                    }
                });
            }
        }

    }
    #endregion
}
