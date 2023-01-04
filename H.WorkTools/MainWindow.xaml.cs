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
using H.Util;
using Newtonsoft.Json;
using OpenCvSharp;
using Clipboard = System.Windows.Forms.Clipboard;
using System.Threading.Tasks;
using OpenCvSharp.Extensions;
using static H.Util.WhoUsePort;
using RDPCOMAPILib;
using System.Net;
using Microsoft.Win32;
using H.ScreenCapture;

namespace H.WorkTools
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
        #endregion
        #region 通信
        private TcpP2p p2p = new TcpP2p();
        #endregion

        PortUserInfo[] portlist;
        private static string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
        string encrypt_key = "";//加密解密key
        Config cif = new Config();
        Hotkey hotkey = null;
        Icon MouseLeftImg = null;//左键ico
        Icon MousePointerImg = null;//正常状态ico
        Icon MouseRightImg = null;//左键ico
        Icon MouseWheelImg = null;//滑轮ico

        SystemInfo sys = new SystemInfo();

        #region 窗体事件
        public MainWindow()
        {
            //bool? Result = new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
            //Result = new MessageBoxCustom("adssad", "Are you sure, You want close         applicationapplicationapplicationapplicationapplication ? ", MessageType.Success, MessageButtons.OkCancel).ShowDialog();
            //Result = new MessageBoxCustom("adssad", "Are you sure, You want to close         application ? ", MessageType.Warning, MessageButtons.Ok).ShowDialog();

            //if (Result.Value)
            //    {
            //        Application.Current.Shutdown();
            //    }
        }

        /// <summary>
        ///  加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            AddClipboardFormatListener(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            encrypt_key = cif.GetValue("Encrypt");
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
            cbIp.SelectedIndex = ipList.FindIndex(ipmodel => ipmodel.id == cif.GetValue("Ip"));
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
            Stream MouseLeftImgStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/MouseLeftImg.ico")).Stream;
            Stream MousePointerImgStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/MousePointerImg.ico")).Stream;
            Stream MouseRightImgStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/MouseRightImg.ico")).Stream;
            Stream MouseWheelImgStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/MouseWheelImg.ico")).Stream;
            MouseLeftImg = new System.Drawing.Icon(MouseLeftImgStream);
            MousePointerImg = new System.Drawing.Icon(MousePointerImgStream);
            MouseRightImg = new System.Drawing.Icon(MouseRightImgStream);
            MouseWheelImg = new System.Drawing.Icon(MouseWheelImgStream);
            Btn_ScreenRecording_Start.IsEnabled = true;
            Btn_ScreenRecording_Pause.IsEnabled = false;
            Btn_ScreenRecording_Stop.IsEnabled = true;
            if (!string.IsNullOrEmpty(cif.GetValue("ScreenRecordingFps")))
            {
                slFps.Value = Convert.ToDouble(cif.GetValue("ScreenRecordingFps"));
            }
            _timedIntervalTaskGetScreenImg = new TimedIntervalTask(() =>
            {
                if (_timedIntervalTaskGetScreenImg.IsRunning)
                {
                    _screenImgByteArray.Enqueue(GetScreenImgByteArray());
                    _totalFrames++;
                }
            }, Convert.ToInt32(slFps.Value));

            _timedIntervalTaskVideoWriter = new TimedIntervalTask(() =>
            {
                int count = _screenImgByteArray.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Bitmap img = null;
                        try
                        {
                            if (_screenImgByteArray.TryDequeue(out img))
                            {
                                using (Mat mat = BitmapConverter.ToMat(img))
                                {
                                    using (InputArray input = InputArray.Create(mat))
                                    {
                                        lock (_videoWriteingLock)
                                        {
                                            _videoWriter?.Write(input);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        finally
                        {
                            img?.Dispose();
                            img = null;
                        }
                    }
                }
            }, 1000);

            _timedIntervalTaskScreenMins = new TimedIntervalTask(() =>
            {
                var ts = TimeSpan.FromSeconds(_totalFrames / Convert.ToDouble(slFps.Value));

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.LabMins.Content = $"{ts.Hours} : {ts.Minutes} : {ts.Seconds} s";
                }));
            }, 200);
            #endregion

            #region 初始化语言
            List<LanguageModel> languages = new List<LanguageModel>();//语言实体类
            languages.Add(new LanguageModel { id = "zh_cn", title = "中文（简体）" });
            languages.Add(new LanguageModel { id = "en_us", title = "English" });
            languages.Add(new LanguageModel { id = "jp_jp", title = "日本語" });
            languages.Add(new LanguageModel { id = "kr_kr", title = "한국어" });
            cbLanguage.ItemsSource = languages;
            cbLanguage.SelectedIndex = languages.FindIndex(language => language.id == cif.GetValue("Language"));
            if (Convert.ToBoolean(cif.GetValue("Start") == ""|| cif.GetValue("Start") == "True" ? true : false))
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
            if ((_timedIntervalTaskGetScreenImg?.IsStarted ?? false) || (_timedIntervalTaskVideoWriter?.IsStarted ?? false))
            {
                if (!_timedIntervalTaskGetScreenImg.IsStop || !_timedIntervalTaskVideoWriter.IsStop)
                {
                    new MessageBoxCustom("退出提示", "录屏任务没有结束 ", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    e.Cancel = true;
                    return;
                }
            }

            if (_videoWriter != null)
            {
                new MessageBoxCustom("退出提示", "录屏任务没有结束 ", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                e.Cancel = true;
                return;
            }
            Environment.Exit(0); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。
        }

        /// <summary>
        /// 重载OnSourceInitialized来在SourceInitialized事件发生后获取窗体的句柄，并且注册全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hotkey = new Hotkey(AddLvClipboardList);
            #region  剪贴板 默认快捷键Ctrl + 1-10
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad0, () => { try { Clipboard.SetDataObject((LvClipboard.Items[0] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad1, () => { try { Clipboard.SetDataObject((LvClipboard.Items[1] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad2, () => { try { Clipboard.SetDataObject((LvClipboard.Items[2] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad3, () => { try { Clipboard.SetDataObject((LvClipboard.Items[3] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad4, () => { try { Clipboard.SetDataObject((LvClipboard.Items[4] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad5, () => { try { Clipboard.SetDataObject((LvClipboard.Items[5] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad6, () => { try { Clipboard.SetDataObject((LvClipboard.Items[6] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad7, () => { try { Clipboard.SetDataObject((LvClipboard.Items[7] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad8, () => { try { Clipboard.SetDataObject((LvClipboard.Items[8] as TextBlock).Text); } catch { } });
            hotkey.Regist(this, HotkeyModifiers.MOD_CONTROL, Key.NumPad9, () => { try { Clipboard.SetDataObject((LvClipboard.Items[9] as TextBlock).Text); } catch { } });
            #endregion
            hotkey.Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.Escape, () => { try { Stop(); } catch { } });
        }
        const int WM_HOTKEY = 0x312;
        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {

            }
            else if (msg == 0x031D)//剪贴板
            {

            }
            return IntPtr.Zero;
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
            StartCapture(false);
            //hotkey.Regist(this, HotkeyModifiers.MOD_ALT_SHIFT, Key.P, () =>
            //{
            //    try
            //    {
            //        System.Windows.MessageBox.Show("aaa");
            //    }
            //    catch { }
            //});
        }
        private FrmCapture m_frmCapture;
        //启动截图
        private void StartCapture(bool bFromClip)
        {
            if (m_frmCapture == null || m_frmCapture.IsDisposed)
                m_frmCapture = new FrmCapture();
            m_frmCapture.IsCaptureCursor = true;//鼠标是否截图
            m_frmCapture.IsFromClipBoard = bFromClip;
            m_frmCapture.Show();
        }
        #endregion

        #region 录屏
        #region 录屏定义变量
        /// <summary>
        /// 用来存放 桌面屏幕图片的线程安全队列
        /// </summary>
        private readonly ConcurrentQueue<Bitmap> _screenImgByteArray = new ConcurrentQueue<Bitmap>();

        /// <summary>
        /// 声明一个 VideoWriter 对象，用来写入视频文件
        /// </summary>
        private VideoWriter _videoWriter = null;

        /// <summary>
        /// 视频写入中锁对象
        /// </summary>
        private readonly object _videoWriteingLock = new object();

        /// <summary>
        /// 声明一个 Rectangle 对象，用来指定矩形的位置和大小
        /// </summary>
        private readonly System.Drawing.Rectangle _bounds = Screen.PrimaryScreen.Bounds;//可以控制多显示录屏测试

        /// <summary>
        /// 视频的保存目录
        /// </summary>
        private string _saveDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}{Path.DirectorySeparatorChar}";

        /// <summary>
        /// 视频写入 任务
        /// </summary>
        private TimedIntervalTask _timedIntervalTaskVideoWriter;

        /// <summary>
        /// 获取屏幕 任务
        /// </summary>
        private TimedIntervalTask _timedIntervalTaskGetScreenImg;

        /// <summary>
        /// 录屏时长 任务
        /// </summary>
        private TimedIntervalTask _timedIntervalTaskScreenMins;

        /// <summary>
        /// 总帧数
        /// </summary>
        private int _totalFrames = 0;

        /// <summary>
        /// 获取鼠标位置的 Rectangle 对象值
        /// </summary>
        /// <returns></returns>
        private static System.Drawing.Rectangle GetMousePositionRectangle()
        {
            return new System.Drawing.Rectangle(System.Windows.Forms.Control.MousePosition.X + -5, System.Windows.Forms.Control.MousePosition.Y + -5, 32, 32);
        }

        /// <summary>
        /// 获取 桌面屏幕图片
        /// </summary>
        /// <returns></returns>
        private Bitmap GetScreenImgByteArray()
        {
            Bitmap bitmap = new Bitmap(_bounds.Width, _bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, _bounds.Size, CopyPixelOperation.SourceCopy);
                switch (System.Windows.Forms.Control.MouseButtons)
                {
                    default:
                        graphics.DrawIcon(MousePointerImg, GetMousePositionRectangle());
                        break;

                    case MouseButtons.Left:
                        graphics.DrawIcon(MouseLeftImg, GetMousePositionRectangle());
                        break;

                    case MouseButtons.Right:
                        graphics.DrawIcon(MouseRightImg, GetMousePositionRectangle());
                        break;

                    case MouseButtons.Middle:
                        graphics.DrawIcon(MouseWheelImg, GetMousePositionRectangle());
                        break;
                }

                return bitmap;
            }
        }
        #endregion
        /// <summary>
        ///  录屏-开始和结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Start_Click(object sender, EventArgs e)
        {
            string path = "";
            if (string.IsNullOrEmpty(cif.GetValue("ScreenRecordingPath")))
            {
                path = $"{_saveDirectory}ScreenVideo-{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            }
            else
            {
                path = $"{cif.GetValue("ScreenRecordingPath")}ScreenVideo-{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            txtPath.Text = path;
            cif.SaveValue("ScreenRecordingFps", slFps.Value.ToString());
            _videoWriter = new VideoWriter(path, new FourCC(FourCC.XVID), Convert.ToDouble(slFps.Value), new OpenCvSharp.Size(_bounds.Width, _bounds.Height));

            slFps.IsEnabled = false;
            Btn_ScreenRecording_Start.IsEnabled = false;
            Btn_ScreenRecording_Pause.IsEnabled = true;

            _totalFrames = 0;
            _timedIntervalTaskScreenMins.Startup();
            _timedIntervalTaskVideoWriter.Startup();

            _timedIntervalTaskGetScreenImg.IntervalTime = Convert.ToInt32(slFps.Value);
            _timedIntervalTaskGetScreenImg.Startup();
        }
        /// <summary>
        ///  录屏-暂停和继续
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Pause_Click(object sender, EventArgs e)
        {
            if (!_timedIntervalTaskGetScreenImg.IsStarted)
            {
                return;
            }

            if (_timedIntervalTaskGetScreenImg.IsRunning)
            {
                _timedIntervalTaskGetScreenImg.Pause();
                Btn_ScreenRecording_Pause.ToolTip = "继 续";
                Btn_ScreenRecording_Pause.Foreground = new SolidColorBrush(Colors.Brown);
            }
            else
            {
                _timedIntervalTaskGetScreenImg.GoOn();
                Btn_ScreenRecording_Pause.ToolTip = "暂 停";
                Btn_ScreenRecording_Pause.Foreground = new SolidColorBrush(Colors.White);
            }
        }
        /// <summary>
        ///  录屏-结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenRecording_Stop_Click(object sender, EventArgs e)
        {
            if (!_timedIntervalTaskGetScreenImg.IsStarted)
            {
                return;
            }

            _timedIntervalTaskGetScreenImg.Stop();
            _timedIntervalTaskVideoWriter.IntervalTime = 500;

            Btn_ScreenRecording_Stop.IsEnabled = false;
            Btn_ScreenRecording_Pause.IsEnabled = false;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(250);
                    if (_screenImgByteArray.Count < 1)
                    {
                        _timedIntervalTaskVideoWriter.Stop();
                        await Task.Delay(250);
                        break;
                    }
                }
                _timedIntervalTaskScreenMins.Stop();
            }).ContinueWith(t =>
            {
                Task.Run(() =>
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageBoxCustom("录屏提示", "录屏视频已结束并保存完成 ", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                    }));
                });

                lock (_videoWriteingLock)
                {
                    _videoWriter?.Dispose();
                    _videoWriter = null;
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Btn_ScreenRecording_Pause.ToolTip = "暂 停";
                    Btn_ScreenRecording_Pause.Foreground = new SolidColorBrush(Colors.White);
                    Btn_ScreenRecording_Pause.IsEnabled = false;

                    Btn_ScreenRecording_Start.IsEnabled = true;
                    Btn_ScreenRecording_Stop.IsEnabled = true;
                    slFps.IsEnabled = true;
                }));
            });
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
                BtnDeskShare.Content = "停止共享";
                BtnDeskShare.Foreground = new SolidColorBrush(Colors.Brown);
                BtnDeskShareJoin.IsEnabled = false;
                IsDeskShare = false;
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
                _rdpSession.Close();
                _rdpSession = null;
            }
            catch (Exception ex)
            { }
            BtnDeskShare.Content = "开始共享";
            BtnDeskShare.Foreground = new SolidColorBrush(Colors.White);
            BtnDeskShareJoin.IsEnabled = true;
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
                txtIp.IsEnabled = true;
                TcpP2p.Msg Sendmsg = new TcpP2p.Msg();
                Sendmsg.type = Convert.ToInt32(TcpP2p.msgType.SendText);
                Sendmsg.sendIP = cbIp.SelectedValue.ToString();
                Sendmsg.sendName = txtNiceName.Text;
                Sendmsg.sendProt = cif.GetValue("Tcp");
                Sendmsg.Data = Encoding.UTF8.GetBytes(txtRdpKey.Text);
                Sendmsg.recIP = txtIp.Text.ToString();
                Sendmsg.recProt = cif.GetValue("Tcp");
                Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.ExitUpdate);
                bool issend = p2p.Send(Sendmsg);
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
                if (txtRdpKey.Text == "" || txtRdpKey.Text.Length != 4)
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
                    Sendmsg.Data = Encoding.UTF8.GetBytes(txtRdpKey.Text);
                    Sendmsg.recIP = ip.ToString();
                    Sendmsg.recProt = cif.GetValue("Tcp");
                    Sendmsg.command = Convert.ToInt32(TcpP2p.msgCommand.JoinApply);
                    bool issend = p2p.Send(Sendmsg);
                    if (!issend)
                    {
                        BtnDeskShare.IsEnabled = false;
                        BtnDeskShareJoin.IsEnabled = false;
                        txtIp.IsEnabled = false;
                        new MessageBoxCustom("系统提示", "加入请求失败请检查ip", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                        return;
                    }
                    cif.SaveValue("Ip", cbIp.SelectedValue.ToString());
                    cif.SaveValue("NickName", txtNiceName.Text);
                }
                else
                {
                    new MessageBoxCustom("系统提示", "输入正确IP后加入", MessageType.Warning, MessageButtons.Ok).ShowDialog();
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
                                            Sendmsg.Data = Encoding.UTF8.GetBytes(invitationString);
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
                            BtnDeskShareJoin.IsEnabled = true;
                            BtnDeskShare.IsEnabled = true;
                            txtIp.IsEnabled = true;
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinPassword))
                        {//加入失败密码错误
                            new MessageBoxCustom("系统提示", "加入失败，密码错误", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                            BtnDeskShareJoin.IsEnabled = true;
                            BtnDeskShare.IsEnabled = true;
                            txtIp.IsEnabled = true;
                            return;
                        }
                        else if (msgstr.command == Convert.ToInt32(TcpP2p.msgCommand.JoinFail))
                        {//加入失败会议没有
                            new MessageBoxCustom("系统提示", "加入失败，未开启共享或已经结束", MessageType.Warning, MessageButtons.Ok).ShowDialog();
                            BtnDeskShareJoin.IsEnabled = true;
                            BtnDeskShare.IsEnabled = true;
                            txtIp.IsEnabled = true;
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
                    regKey.SetValue("H.WorkTools", System.Windows.Forms.Application.ExecutablePath);
                }
                else
                {
                    if (regKey != null)
                    {
                        if (regKey.GetValue("H.WorkTools") != null)
                            regKey.DeleteValue("H.WorkTools");
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
    }
}
