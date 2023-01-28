using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class ScreenRecordHelper
    {
        #region 模拟控制台信号需要使用的API

        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        #endregion

        //ffmpeg进程
        static Process ffmpegProcess = new Process();

        //ffmpeg.exe实体文件路径，建议把ffmpeg.exe及其配套放在自己的Debug目录下
        static string ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "bin\\ffmpeg.exe";
        //static string ffmpegPath = @"C:\Users\awang\Desktop\ScreenRecord\ffmpeg-N-101429-g54e5d21aca-win64-gpl-shared-vulkan\bin\ffmpeg.exe";

        /// <summary>
        /// 开始录制
        /// </summary>
        /// <param name="outFilePath">录屏文件保存路径</param>
        public static void Start(string outFilePath)
        {
            if (File.Exists(outFilePath))
            {
                File.Delete(outFilePath);
            }

            string arguments = "-f gdigrab -framerate 15 -r 15 -i desktop -pix_fmt yuv420p -f dshow -i audio=" + "\"virtual-audio-capturer\"" + " -vcodec" + " h264_qsv" + " \"" + outFilePath + "\"";
            /*转码，
             * 视频录制设备：gdigrab；
             * 录制对象：整个桌面desktop；
             * 音频录制方式：dshow；
             * 音频输入：virtual-audio-capturer；
             * 视频编码格式：h.264；
             * 视频帧率：15；
             * 硬件加速：若N卡加速：h264_nvenc；若集显加速：h264_qsv；若软件编码：libx264；
            */

            ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;//不使用操作系统外壳程序启动
            startInfo.RedirectStandardError = true;//重定向标准错误流
            startInfo.CreateNoWindow = true;//默认不显示窗口
            startInfo.RedirectStandardInput = true;//启用模拟该进程控制台输入的开关
                                                   //startInfo.RedirectStandardOutput = true;

            ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(Output);//把FFmpeg的输出写到StandardError流中
            ffmpegProcess.StartInfo = startInfo;

            ffmpegProcess.Start();//启动
            ffmpegProcess.BeginErrorReadLine();//开始异步读取输出
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public static void Stop()
        {
            ffmpegProcess.StandardInput.WriteLine("q");//在这个进程的控制台中模拟输入q,用于停止录制
            ffmpegProcess.Close();
            ffmpegProcess.Dispose();
        }

        /// <summary>
        /// 控制台输出信息
        /// </summary>
        private static void Output(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data.ToString());
            }
        }
    }
}
