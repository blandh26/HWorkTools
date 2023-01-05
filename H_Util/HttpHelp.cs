using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class HttpHelp
    {
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="codeType">编码类型</param>
        /// <param name="ContentType">内容类型</param>
        /// <param name="jsonParas">参数</param>
        /// <returns></returns>
        public string Post(string Url, string codeType, string ContentType, string jsonParas, int time)
        {
            try
            {
                string strURL = Url; //创建一个HTTP请求  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);    //Post请求方式  
                request.Method = "POST"; //内容类型
                request.ContentType = ContentType;//设置参数，并进行URL编码 
                request.Timeout = time * 1000;
                string paraUrlCoded = jsonParas;//System.Web.HttpUtility.UrlEncode(jsonParas);   
                byte[] payload;//将Json字符串转化为字节 
                payload = System.Text.Encoding.GetEncoding(codeType).GetBytes(paraUrlCoded); //设置请求的ContentLength  
                request.ContentLength = payload.Length; //发送请求，获得请求流 
                Stream writer;
                try
                {
                    writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
                }
                catch (Exception)
                {
                    writer = null;
                    return "连接服务器失败!";
                }
                writer.Write(payload, 0, payload.Length);//将请求参数写入流
                writer.Close();//关闭请求流
                // String strValue = "";//strValue为http响应所返回的字符流
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();//获得响应流
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();//  Stream postData = Request.InputStream;
                StreamReader sRead = new StreamReader(s);
                string postContent = sRead.ReadToEnd();
                sRead.Close();
                return postContent;//返回Json数据
            }
            catch (Exception)
            {
                return "请求异常！";
            }
        }
    }
}
