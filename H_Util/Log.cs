using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public static class Log
    {
        private static string lockString = "ab3cdef1ghi5jklmno4pqrstuvw23xyz123456ds789098e7654321zyxabc;;;asd0fasdfa";
        private static string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下

        /// <summary>
        /// 记录错误消息到文件日志
        /// </summary>
        /// <param name="error">错误消息</param>
        public static void WriteErrorLog(string error)
        {
            WriteToLog(path, "Error", error);
        }
        /// <summary>
        /// 记录操作日志到文件日志
        /// </summary>
        /// <param name="message">操作日志</param>
        public static void WriteOperateLog(string message)
        {
            WriteToLog(path, "Operate", message);
        }

        /// <summary>
        /// 记录系统操作
        /// </summary>
        /// <param name="message">系统操作消息</param>
        public static void WriteSystemLog(string message)
        {
            WriteToLog(path, "System", message);
        }

        public static void WriteException(string message)
        {
            WriteToLog(path, "Exception", message);
        }

        private static void WriteToLog(string filePath, string fileName, string message)
        {
            if (string.IsNullOrEmpty(filePath) || filePath.Trim() == "")
            {
                filePath = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            try
            {
                lock (lockString + fileName)
                {
                    if (!Directory.Exists(filePath + "log\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString()))
                    {
                        Directory.CreateDirectory(filePath + "log\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString());
                    }
                    using (StreamWriter sw = new StreamWriter(filePath + "log\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + fileName + "_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "]\r\n" + message);
                        sw.Flush();
                        sw.Dispose();
                    }
                }
            }
            catch
            {
                //已经无法再次处理了
            }
        }

        public static void WriteJson(string fileName, string txt)
        {
            if (string.IsNullOrEmpty(path) || path.Trim() == "")
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            try
            {
                lock (fileName)
                {
                    if (!Directory.Exists(path + "Json\\"))
                    {
                        Directory.CreateDirectory(path + "Json\\");
                    }
                    using (StreamWriter sw = new StreamWriter(path + "Json\\" + "\\" + fileName + ".Json", false, Encoding.UTF8))
                    {
                        sw.Write(txt);
                        sw.Flush();
                        sw.Dispose();
                    }
                }
            }
            catch
            {
                //已经无法再次处理了
            }
        }

    }
}
