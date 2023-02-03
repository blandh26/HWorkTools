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
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
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
        public delegate void LvExeRefresh();//定义一个委托 更新exe

        #endregion
        #region 通信
        private TcpP2p p2p = new TcpP2p();
        bool isInvite = false;//是否是求助模式
        #endregion

        bool IsExeDelete = false;//应用中心 删除状态
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

            #region 窗体位置
            System.Drawing.Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            this.Top = 60;
            this.Left = rectangle.Width - 470;
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
                cif.SaveValue("ScreenRecordingPath", path + "HScreenVideo");
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
            {
                IsVideo = false;
                ScreenRecordHelper.Stop();
            }
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
            #region  剪贴板 默认快捷键Ctrl + 1-10
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad0, () => { try { Clipboard.SetDataObject((LvClipboard.Items[0] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad1, () => { try { Clipboard.SetDataObject((LvClipboard.Items[1] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad2, () => { try { Clipboard.SetDataObject((LvClipboard.Items[2] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad3, () => { try { Clipboard.SetDataObject((LvClipboard.Items[3] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad4, () => { try { Clipboard.SetDataObject((LvClipboard.Items[4] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad5, () => { try { Clipboard.SetDataObject((LvClipboard.Items[5] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad6, () => { try { Clipboard.SetDataObject((LvClipboard.Items[6] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad7, () => { try { Clipboard.SetDataObject((LvClipboard.Items[7] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad8, () => { try { Clipboard.SetDataObject((LvClipboard.Items[8] as TextBlock).Text); } catch { } });
            Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad9, () => { try { Clipboard.SetDataObject((LvClipboard.Items[9] as TextBlock).Text); } catch { } });
            #endregion
            Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.Escape, () => { try { Stop(); } catch { } });//停止共享
            Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.A, () =>
            {
                try
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(0);
                    m_frmCapture.Show();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });//截图快捷键
            Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.S, () =>
            {
                try
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(1);
                    m_frmCapture.Show();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });//上一次区域截图快捷键
            Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.D, () =>
            {
                try
                {
                    if (m_frmCapture == null || m_frmCapture.IsDisposed)
                        m_frmCapture = new FrmCapture(2);
                    m_frmCapture.Show();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });//上一次区域截图快捷键
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

        private void Minimize_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 截图
        /// <summary>
        ///  设置截图快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenCapture_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region 录屏   
        //ffmpeg进程
        static Process ffmpegProcess = new Process();

        //ffmpeg.exe实体文件路径，建议把ffmpeg.exe及其配套放在自己的Debug目录下
        static string ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "FFmpeg\\ffmpeg.exe";

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
        private List<WaveInCapabilities> GetDevices()
        {
            List<WaveInCapabilities> devices = new List<WaveInCapabilities>();
            // 返回系统中可用的Wave-In设备数
            int waveInDevices = WaveIn.DeviceCount;
            for (int i = 0; i < waveInDevices; i++)
            {
                devices.Add(WaveIn.GetCapabilities(i));
            }
            return devices;
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
                    IsVideo = false;
                    this.BtnScreenRecordingVideo.ToolTip = "Start";
                    BtnScreenRecordingVideo.Foreground = new SolidColorBrush(Colors.White);
                    ffmpegProcess.StandardInput.WriteLine("q");//在这个进程的控制台中模拟输入q,用于停止录制
                    ffmpegProcess.Close();
                    ffmpegProcess.Dispose();
                }
                catch (Exception)
                {
                }
            }
            else//false 录制
            {
                try
                {
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
                    if (cbAudio.Text != "")
                    {
                        arguments += "-f dshow -i audio=\"" + cbAudio.Text + "\"";
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
                    IsVideo = false;
                }
            }
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
            Console.WriteLine("ooo");
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
                throw new Exception("regist hotkey fail.");
            keymap[id] = callBack;
        }

        static int keyid = 10;
        public static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();

        public delegate void HotKeyCallBackHanlder();

        public enum HotkeyModifiers
        {
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8,
            MOD_ALT_CONTROL = (0x1 | 0x2),
            MOD_ALT_SHIFT = (0x1 | 0x4),
            MOD_CONTROLT_SHIFT = (0x2 | 0x4)
        }
        #endregion
    }

    #region 弹出框有关
    internal class MainWindowViewModel : ViewModelBase
    {
        public DelegateCommand UpdateCommand
        {
            get
            {
                return new DelegateCommand(async (src) =>
                {
                    string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
                    CommonDialogResult result = await CommonDialogShow.ShowInputDialog("Root", "Add") as CommonDialogResult;
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
