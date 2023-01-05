using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class StringFormat
    {

        /// <summary>
        /// sql语句转vb代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public string VbSplice(string txtStr)
        {
            string str = "\t\tDim sql As String =  ";
            string[] txtlist = txtStr.ToString().Replace("\r\n", "♠").Split('♠');
            for (int i = 0; i < txtlist.Length; i++)
            {
                if (i == 0)
                {
                    str += "\"" + txtlist[i].Trim() + "\"  " + Environment.NewLine;
                }
                else
                {
                    if (txtlist[i].Trim() != "")
                    {
                        str += "\t\t\tsql &=\"" + txtlist[i].Trim() + "\" " + Environment.NewLine;

                    }
                }
            }
            return str.Trim();
        }

        /// <summary>
        /// sql语句转vb代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public string VbSpliceSql(string txtStr)
        {
            string str = "\t\tDim sql As String =  ";
            string[] txtlist = txtStr.ToString().Replace("\r\n", "♠").Split('♠');
            for (int i = 0; i < txtlist.Length; i++)
            {
                if (i == 0)
                {
                    str += "\"" + txtlist[i].Trim() + " \"  " + Environment.NewLine;
                }
                else
                {
                    if (txtlist[i].Trim() != "")
                    {
                        str += "\t\t\tsql &=\"" + txtlist[i].Trim() + " \" " + Environment.NewLine;

                    }
                }
            }
            return str.Trim();
        }

        /// <summary>
        /// sql语句转C#代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public string CsharpSplice(string txtStr)
        {
            string str = "\t\tString sql =\"\" ; " + Environment.NewLine;
            string[] txtlist = txtStr.ToString().Replace("\r\n", "♠").Split('♠');
            for (int i = 0; i < txtlist.Length; i++)
            {
                if (txtlist[i].Trim() != "")
                {
                    str += "\t\t\tsql +=\"" + txtlist[i].Trim() + "\"; " + Environment.NewLine;
                }
            }
            return str.Trim().Substring(0, str.Trim().Length - 1);
        }

        /// <summary>
        /// sql语句转C#代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public string CsharpSpliceSql(string txtStr)
        {
            string str = "\t\tString sql =\"\" ; " + Environment.NewLine;
            string[] txtlist = txtStr.ToString().Replace("\r\n", "♠").Split('♠');
            for (int i = 0; i < txtlist.Length; i++)
            {
                if (txtlist[i].Trim() != "")
                {
                    str += "\t\t\tsql +=\"" + txtlist[i].Trim() + " \"; " + Environment.NewLine;
                }
            }
            return str.Trim().Substring(0, str.Trim().Length - 1);
        }
        /// <summary>
        /// 批量替换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public string BacthReplce(string txtStr, string replace1, string replace2)
        {
            try
            {
                string[] slist1 = replace1.Replace("\n", "").Split('\r');
                string[] slist2 = replace2.Replace("\n", "").Split('\r');
                string temp = txtStr;
                for (int i = 0; i < slist1.Length; i++)
                {
                    if (slist1[i].Trim() == "")
                    {
                        break;
                    }
                    temp = temp.Replace(slist1[i].Trim(), slist2[i].Trim());
                }
                return temp;
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String LineToCamelName(String name)
        {
            StringBuilder result = new StringBuilder();
            if (name == null || name.Length < 1)   // 快速检查
            {
                return ""; // 没必要转换
            }
            else if (!name.Contains("_"))
            {
                return name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();// 不含下划线，仅将首字母小写
            }
            string[] camels = name.Split('_');// 用下划线将原始字符串分割
            foreach (string camel in camels)// 跳过原始字符串中开头、结尾的下换线或双重下划线
            {
                if (camel == string.Empty) { continue; }
                result.Append(camel.Substring(0, 1).ToUpper());                //驼峰，首字母大写
                result.Append(camel.Substring(1).ToLower());
            }
            return result.ToString();
        }
        /// <summary>
        /// 驼峰转下划线
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string CamelToLineNmae(string name)
        {
            StringBuilder result = new StringBuilder();
            if (name != null && name.Length > 0) // 将第一个字符处理成大写
            {
                result.Append(name.Substring(0, 1).ToUpper());// 循环处理其余字符
                for (int i = 1; i < name.Length; i++)
                {
                    String s = name.Substring(i, 1);// 在大写字母前添加下划线

                    if (s.Equals(s.ToUpper())) { result.Append("_"); }// 其他字符直接转成大写
                    result.Append(s.ToUpper());
                }
            }
            return result.ToString();
        }
    }
}
