using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using H_Util;

namespace H_Util
{

    public class TcpP2p
    {
        /// <summary>
        /// 消息命令类型
        /// </summary>
        [Serializable]
        public enum msgCommand
        {
            /// <summary>
            /// 空
            /// </summary>
            None,
            /// <summary>
            /// 登录
            /// </summary>
            Login,
            /// <summary>
            /// 退出
            /// </summary>
            Exit,
            /// <summary>
            /// 更改密码
            /// </summary>
            ChangePwd,
            /// <summary>
            /// 注册
            /// </summary>
            Register,
            /// <summary>
            /// 获取在线用户列表
            /// </summary>
            GetUserList,
            /// <summary>
            /// 获取通知
            /// </summary>
            GetNotice,
            /// <summary>
            /// 发送确认信息
            /// </summary>
            Confirm,
            /// <summary>
            /// 请求加入
            /// </summary>
            JoinApply,
            /// <summary>
            /// 邀请申请
            /// </summary>
            InviteApply,
            /// <summary>
            /// 加入失败共享结束或未共享
            /// </summary>
            JoinFail,
            /// <summary>
            /// 加入失败密码错误
            /// </summary>
            JoinPassword,
            /// <summary>
            /// 加入成功
            /// </summary>
            JoinSuccess,
            /// <summary>
            /// 加入拒绝
            /// </summary>
            JoinRefuse,
            /// <summary>
            /// 加入成功更新
            /// </summary>
            JoinUpdate,
            /// <summary>
            /// 邀请拒绝
            /// </summary>
            InviteRefuse,
            /// <summary>
            /// 邀请退出
            /// </summary>
            InviteExit,
            /// <summary>
            /// 退出更新
            /// </summary>
            ExitUpdate,
            /// <summary>
            /// 更新EXE列表
            /// </summary>
            ExElistUpdate,
            /// <summary>
            /// 更新闹钟列表
            /// </summary>
            AlarmlistUpdate
        }
        /// <summary>
        /// 消息类型
        /// </summary>
        [Serializable]
        public enum msgType
        {
            /// <summary>
            ///空消息
            /// </summary>
            None,
            /// <summary>
            ///文本消息
            /// </summary>
            SendText,
            /// <summary>
            ///发送文件
            /// </summary>
            SendFile
        }
        /// <summary>
        /// 消息发送状态(用于消息循环接收的开始和技术标识)
        /// </summary>
        [Serializable]
        public enum msgSendState
        {
            None,   //空
            single, //单条消息，一次性接收
            start,  //消息开始
            sending,//消息发送中
            end,     //消息结束
            accept,  //同意
            refuse //拒绝
        }


        [Serializable]
        public class Msg
        {
            public string sendIP = string.Empty;               //发送方IP
            public string sendProt = string.Empty;             //发送方监听端口
            public string sendName = string.Empty;             //发送方名称
            public string recIP = string.Empty;                //接收方IP
            public string recProt = string.Empty;              //接收方监听端口
            public string recName = string.Empty;              //接收方名称

            public int command = 0;       //命令枚举
            public int type = 0;                //消息类型枚举
            public int msgState = 0;  //消息状态枚举
            public byte[] Data;                                //消息内容
            public string Filename = string.Empty;             //文件名称
        }



        private Thread th;
        private TcpListener tcpl;
        //public delegate void ShowMessage(object msgstr);
        public delegate void ShowMessage(Msg msgstr);
        public event ShowMessage ReceivMsg;

        public int myport;
        public void Listener()
        {
            th = new Thread(new ThreadStart(Listen));//新建一个用于监听的线程
            th.Start();//打开新线程
        }
        public void Stop()
        {
            tcpl.Stop();
            th.Abort();//终止线程
        }
        //   private void Listen()
        //   {
        //       try
        //       {
        //           tcpl = new TcpListener(myport);//新建一个TcpListener对象
        //           tcpl.Start();

        //           while (true)//开始监听
        //           {
        //               Socket s = tcpl.AcceptSocket();
        //               string remote = s.RemoteEndPoint.ToString();

        //               Byte[] stream = new Byte[1024];
        //               int i = s.Receive(stream);//接受连接请求的字节流　
        //               string msg;

        //        msg = "[" + remote + "]:" + System.Text.Encoding.UTF8.GetString(stream);

        //               ReceivMsg(msg);
        //           }
        //       }
        //       catch
        //       { }

        //}
        public void Listen()
        {
            try
            {
                tcpl = new TcpListener(myport);//新建一个TcpListener对象
                tcpl.Start();

                while (true)//开始监听
                {

                    Socket s = tcpl.AcceptSocket();
                    MemoryStream mStream = new MemoryStream();
                    mStream.Position = 0;
                    byte[] buffer = new byte[1024];

                    while (true)
                    {
                        int ReceiveCount = s.Receive(buffer, buffer.Length, 0);
                        if (ReceiveCount == 0)
                        {
                            break;//接收到的字节数为0时break
                        }
                        else
                        {
                            mStream.Write(buffer, 0, ReceiveCount); //将接收到的数据写入内存流
                        }
                    }
                    mStream.Flush();
                    mStream.Position = 0;
                    mStream.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (mStream.Capacity > 0)
                    {
                        formatter.Binder = new UBinder();
                        Msg recmsg = (Msg)formatter.Deserialize(mStream);//将接收到的内存流反序列化为对象
                        ReceivMsg(recmsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("接收信息异常：");
                Log.WriteErrorLog(ex.Message);
            }
        }

        public class UBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return typeof(Msg);
            }
        }

        public void Send(string msgstr, string ip, int port)
        {

            try
            {
                System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient(ip, port);//新建一个TcpClient对象 
                NetworkStream tcpStream = tcp.GetStream();
                StreamWriter sw = new StreamWriter(tcpStream);
                sw.WriteLine(msgstr);
                sw.Flush();//发送信息 　
                sw.Close();
                tcp.Close();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("发送信息异常：");
                Log.WriteErrorLog(ex.Message);
            }
        }

        public bool Send(Msg msg)
        {

            try
            {
                System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient(msg.recIP, Convert.ToInt16(msg.recProt));//新建一个TcpClient对象 
                NetworkStream tcpStream = tcp.GetStream();

                BinaryFormatter bformatter = new BinaryFormatter();  //二进制序列化类
                bformatter.Serialize(tcpStream, msg); //将消息类转换为内存流
                tcpStream.Flush();


                StreamWriter sw = new StreamWriter(tcpStream);
                sw.Flush();//发送信息 

                sw.Close();
                tcp.Close();
                return true;
            }
            catch (Exception ex)
            { return false; }
        }


        public void SendFileFunc(object obj, string FilePath, string FileName)
        {
            TcpListener tcpListener = obj as TcpListener;
            while (true)
            {
                try
                {
                    System.Net.Sockets.TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected)
                    {
                        NetworkStream stream = tcpClient.GetStream();
                        string fileName = FileName;

                        byte[] fileNameByte = Encoding.Unicode.GetBytes(fileName);

                        byte[] fileNameLengthForValueByte = Encoding.Unicode.GetBytes(fileNameByte.Length.ToString("D11"));
                        byte[] fileAttributeByte = new byte[fileNameByte.Length + fileNameLengthForValueByte.Length];

                        fileNameLengthForValueByte.CopyTo(fileAttributeByte, 0);  //文件名字符流的长度的字符流排在前面。

                        fileNameByte.CopyTo(fileAttributeByte, fileNameLengthForValueByte.Length);  //紧接着文件名的字符流

                        stream.Write(fileAttributeByte, 0, fileAttributeByte.Length);
                        FileStream fileStrem = new FileStream(FilePath, FileMode.Open);

                        int fileReadSize = 0;
                        long fileLength = 0;
                        while (fileLength < fileStrem.Length)
                        {
                            byte[] buffer = new byte[2048];
                            fileReadSize = fileStrem.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, fileReadSize);
                            fileLength += fileReadSize;
                        }
                        fileStrem.Flush();
                        stream.Flush();
                        fileStrem.Close();
                        stream.Close();
                        //ReceivMsg(string.Format("{0}文件发送成功", fileName));

                    }


                }
                catch (Exception ex)
                {
                    //ReceivMsg(string.Format("{0}文件发送成功", FileName));
                }
            }
        }

        /// <summary>
        /// 检查端口，true表示已被占用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Boolean IsPortOccuped(Int32 port)
        {
            System.Net.NetworkInformation.IPGlobalProperties iproperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            System.Net.IPEndPoint[] ipEndPoints = iproperties.GetActiveTcpListeners();
            foreach (var item in ipEndPoints)
            {
                if (item.Port == port)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
