using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace H_Util
{
    class SocketLib
    {
        /// <summary>
        /// 等待客户端的连接 并且创建与之通信的Socket
        /// </summary>
        Socket socketSend;
        void Listen(object o)
        {
            try
            {
                Socket socketWatch = o as Socket;
                while (true)
                {
                    socketSend = socketWatch.Accept();//等待接收客户端连接
                    //ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功!");
                    //开启一个新线程，执行接收消息方法
                    Thread r_thread = new Thread(Received);
                    r_thread.IsBackground = true;
                    r_thread.Start(socketSend);
                }
            }
            catch { }
        }

        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Received(object o)
        {
            try
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    //客户端连接服务器成功后，服务器接收客户端发送的消息
                    byte[] buffer = new byte[1024 * 1024 * 3];
                    //实际接收到的有效字节数
                    int len = socketSend.Receive(buffer);
                    if (len == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, len);
                    //ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                }
            }
            catch { }
        }
        /// <summary>
        /// 服务器向客户端发送消息
        /// </summary>
        /// <param name="str"></param>
        void Send(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffer);
        }

    }
}
