using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class WhoUsePort
    {
        public class PortUserInfo
        {
            /// <summary>
            /// 端口协议类型,TCP,UDP等
            /// </summary>
            public string Type;
            /// <summary>
            /// 端口状态
            /// </summary>
            public string State;
            /// <summary>
            /// 本地终端地址
            /// </summary>
            public string LocolEndPoint;
            /// <summary>
            /// 本地IP
            /// </summary>
            public IPAddress LocolIPAddress;
            /// <summary>
            /// 本地端口
            /// </summary>
            public int LocolPort;
            /// <summary>
            /// 远程终端地址
            /// </summary>
            public string RemoteEndPoint;
            /// <summary>
            /// 占用端口的进程ID
            /// </summary>
            public int Pid;
            /// <summary>
            /// 占用端口的进程
            /// </summary>
            public Process Process;
            /// <summary>
            /// 占用端口的进程进程名
            /// </summary>
            public string ProcessName;
        }
        /// <summary>
        /// 根据netstat命令找到占用端口的进程id,并返回占用进程对象信息
        /// </summary>
        /// <param name="port">端口</param>
        /// <returns></returns>
        public static PortUserInfo[] NetStatus(int port, bool strict = false)
        {
            string strInput = $"echo off&netstat -aon|findstr \"{port}\"&exit";
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = "cmd.exe";
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            //启动程序
            p.Start();
            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(strInput);
            p.StandardInput.AutoFlush = true;
            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            int index = strOuput.IndexOf(strInput);
            string reply = strOuput.Remove(0, index + strInput.Length);
            List<PortUserInfo> processes = new List<PortUserInfo>();
            string[] sp = new string[] { "\r\n" };
            string[] infos = reply.Split(sp, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in infos)
            {
                sp = new string[] { " " };
                string[] column = line.Split(sp, StringSplitOptions.RemoveEmptyEntries);
                if (column.Length >= 5)
                {
                    PortUserInfo pinfo = new PortUserInfo();
                    pinfo.Type = column[0];
                    pinfo.LocolEndPoint = column[1];
                    int portindex = column[1].IndexOf(":") + 1;
                    int cnt = column[1].Length - portindex;
                    string ipstring = column[1].Substring(0, portindex - 1);
                    string portstring = column[1].Substring(portindex, cnt);
                    if (IPAddress.TryParse(ipstring, out IPAddress localip))
                    {
                        pinfo.LocolIPAddress = localip;
                    }
                    if (int.TryParse(portstring, out int localport))
                    {
                        pinfo.LocolPort = localport;
                    }
                    if (strict)
                    {
                        if (localport != port)
                        {
                            continue;
                        }
                    }
                    pinfo.RemoteEndPoint = column[2];
                    pinfo.State = column[3];
                    string pidstring = column[4];
                    if (int.TryParse(pidstring, out int pid))
                    {
                        pinfo.Pid = pid;
                        Process proc = Process.GetProcessById(pid);
                        pinfo.Process = proc;
                        pinfo.ProcessName = proc.ProcessName;
                        processes.Add(pinfo);
                    }
                }
            }
            return processes.ToArray();
        }
    }
}
