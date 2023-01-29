using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class ProcessHelper
    {
        /// <summary>
        /// 注册dll  需要管理员权限
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public static bool RegisterDll(String dllPath)
        {
            bool result = true;
            try
            {
                if (!File.Exists(dllPath))
                {
                    //Loger.Write(string.Format("“{0}”目录下无“XXX.dll”文件！", AppDomain.CurrentDomain.BaseDirectory));
                    return false;
                }
                //拼接命令参数
                string startArgs = string.Format("/s \"{0}\"", dllPath);
                Process p = new Process();//创建一个新进程，以执行注册动作
                p.StartInfo.FileName = "regsvr32";
                p.StartInfo.Arguments = startArgs;
                //以管理员权限注册dll文件
                WindowsIdentity winIdentity = WindowsIdentity.GetCurrent(); //引用命名空间 System.Security.Principal
                WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
                if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    p.StartInfo.Verb = "runas";//管理员权限运行
                }
                p.Start();
                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                result = false;         //记录日志，抛出异常
            }
            return result;
        }

        /// <summary>
        /// 注册dll  需要管理员权限
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public static bool UnInstallDll(String dllPath)
        {
            bool result = true;
            try
            {
                if (!File.Exists(dllPath))
                {
                    //Loger.Write(string.Format("“{0}”目录下无“XXX.dll”文件！", AppDomain.CurrentDomain.BaseDirectory));
                    return false;
                }
                //拼接命令参数
                string startArgs = string.Format("/u \"{0}\"", dllPath);
                Process p = new Process();//创建一个新进程，以执行注册动作
                p.StartInfo.FileName = "regsvr32";
                p.StartInfo.Arguments = startArgs;
                //以管理员权限注册dll文件
                WindowsIdentity winIdentity = WindowsIdentity.GetCurrent(); //引用命名空间 System.Security.Principal
                WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
                if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    p.StartInfo.Verb = "runas";//管理员权限运行
                }
                p.Start();
                p.WaitForExit();
                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                result = false;         //记录日志，抛出异常
            }
            return result;
        }

        /// <summary>
        /// 注强制关闭进程
        /// </summary>
        /// <param name="ProcessName">进程名称</param>
        /// <returns></returns>
        public static bool ProcessKill(String ProcessName)
        {
            bool result = true;
            try
            {
                //using System.Diagnostics;
                var process = Process.GetProcesses().Where(pr => pr.ProcessName.Replace(" ","").ToLower() == ProcessName.Replace(" ", "").ToLower());
                foreach (var pk in process)
                {
                    try
                    {
                        pk.Kill();
                    }
                    catch
                    {   // 进程被保护而抛出异常(可以使用其它手段,如C\C++)
                        result = false;
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;         //记录日志，抛出异常
            }
            return result;
        }
    }
}
