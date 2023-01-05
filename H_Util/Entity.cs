using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace H_Util
{

    public class Entity
    {
        H_Util.StringFormat S = new StringFormat();
        /// <summary>
        /// 返回每个属性封装代码【表】
        /// </summary>
        /// <param name="langType">语言分类 1vb 2C# 3Java</param>
        /// <param name="oneLetter">是否首字母大写</param>
        /// <param name="generateType">风格 1：下划线转驼峰 2：驼峰转下划线 3：原样</param>
        /// <param name="str">要处理字符串</param>
        /// <param name="describe">功能描述</param>
        /// <param name="create">创建人</param>
        /// <param name="createData">创建时间</param>
        /// <param name="last">最后修改人</param>
        /// <param name="lastData">最后修改时间</param>
        /// <param name="className">类名</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public string GetGenerate(string langType, bool oneLetter, string generateType, string str,
            string describe, string create, string createData, string last, string lastData, string className, string tableName, string txtnamespace)
        {
            string result_str = "";
            try
            {
                string[] slist = str.Split('\r');
                string tmp = "";
                for (int i = 0; i < slist.Length; i++)
                {
                    string[] model = slist[i].Replace("\n", "").Split('\t');
                    if (model[0] == "") { break; }//字段名为空跳出
                    tmp += GetPrivateAttribute(langType, oneLetter, generateType, 
                        model[0], 
                        model[1], 
                        model[2], 
                        model[3],
                        (model.Length == 5 ? model[4] : ""));
                }
                for (int i = 0; i < slist.Length; i++)
                {
                    string[] model = slist[i].Replace("\n", "").Split('\t');
                    if (model[0] == "") { break; }//字段名为空跳出
                    tmp += GetAttribute(langType, oneLetter, generateType, model[0], model[1], model[2], model[3], model[4]);
                }
                if (langType == "1") //VB
                {
                    result_str = " Imports System.ComponentModel" + Environment.NewLine;
                    result_str += "''' <summary>" + Environment.NewLine;
                    result_str += "''' 機能の説明 :" + describe + Environment.NewLine;
                    result_str += "''' 創設者 :" + create + Environment.NewLine;
                    result_str += "''' 作成時間 :" + createData + Environment.NewLine;
                    result_str += "''' 最終変更者 :" + last + Environment.NewLine;
                    result_str += "''' 最終更新日 :" + lastData + Environment.NewLine;
                    result_str += "''' </summary>" + Environment.NewLine;
                    result_str += "<AmbientValue(\"" + tableName + "\")>" + Environment.NewLine;
                    result_str += "Public Class " + className + Environment.NewLine;
                    result_str += tmp;
                    result_str += "End Class";
                }
                else if (langType == "2") //C#
                {
                    result_str = "using System;" + Environment.NewLine;
                    result_str += "using System.Data;" + Environment.NewLine;
                    result_str += "using System.ComponentModel;" + Environment.NewLine;
                    result_str += "namespace " + txtnamespace + Environment.NewLine;
                    result_str += "{ " + Environment.NewLine;
                    result_str += "\t/// <summary>" + Environment.NewLine;
                    result_str += "\t/// 機能の説明 :" + describe + Environment.NewLine;
                    result_str += "\t/// 創設者 :" + create + Environment.NewLine;
                    result_str += "\t/// 作成時間 :" + createData + Environment.NewLine;
                    result_str += "\t/// 最終変更者 :" + last + Environment.NewLine;
                    result_str += "\t/// 最終更新日 :" + lastData + Environment.NewLine;
                    result_str += "\t/// </summary>" + Environment.NewLine;
                    result_str += "\tPublic Class " + className + Environment.NewLine;
                    result_str += "\t{" + Environment.NewLine;
                    result_str += tmp;
                    result_str += "\t}" + Environment.NewLine;
                    result_str += "}";
                }
                else if (langType == "3") //Java
                {

                }

                return result_str;
            }
            catch (Exception ex)
            {
                return result_str;
            }
        }
        /// <summary>
        /// 生成属性
        /// </summary>
        /// <param name="langType">语言分类 1vb 2C# 3Java</param>
        /// <param name="oneLetter">是否首字母大写</param>
        /// <param name="generateType">风格 1：下划线转驼峰 2：驼峰转下划线 3：原样</param>
        /// <param name="name">字段名</param>
        /// <param name="type">数据类型</param>
        /// <param name="isnull">是否为空</param>
        /// <param name="remarks">字段描述</param>
        /// <param name="length">字段长度</param>
        /// <returns></returns>
        private string GetPrivateAttribute(string langType, bool oneLetter, string generateType, string name, string type, string isnull, string remarks, string length)
        {
            if (oneLetter) { name = name.Substring(0, 1).ToUpper() + name.Substring(1); }
            if (generateType == "1") { name = S.LineToCamelName(name); }
            else if (generateType == "2") { name = S.CamelToLineNmae(name); }
            type = this.GetType(type, langType);
            string tmp = "";
            if (langType == "1") //VB
            {
                tmp = "\t''' <summary>" + Environment.NewLine;
                tmp += "\t''' " + remarks + "-" + (isnull == "Unchecked" ? "空にすることはできません " : "") + Environment.NewLine;
                tmp += "\t''' </summary>" + Environment.NewLine;
                tmp += "\tPrivate _" + name + " As " + type + Environment.NewLine;
            }
            else if (langType == "2") //C#
            {
                tmp = "\t\t/// <summary>" + Environment.NewLine;
                tmp += "\t\t/// " + remarks + "-" + (isnull == "Unchecked" ? "空にすることはできません " : "") + Environment.NewLine;
                tmp += "\t\t/// </summary>" + Environment.NewLine;
                tmp += "\t\tpublic " + type + " " + name + " { get; set; }" + Environment.NewLine;
            }
            else if (langType == "3") //Java
            {

            }

            return tmp;
        }
        /// <summary>
        /// 私有化变量生成
        /// </summary>
        /// <param name="langType">语言分类 1vb 2C# 3Java</param>
        /// <param name="oneLetter">是否首字母大写</param>
        /// <param name="generateType">风格 1：下划线转驼峰 2：驼峰转下划线 3：原样</param>
        /// <param name="name">字段名</param>
        /// <param name="type">数据类型</param>
        /// <param name="isnull">是否为空</param>
        /// <param name="remarks">字段描述</param>
        /// <param name="length">字段长度</param>
        /// <returns></returns>
        private string GetAttribute(string langType, bool oneLetter, string generateType, string name, string type, string isnull, string remarks, string length)
        {
            if (oneLetter) { name = name.Substring(0, 1).ToUpper() + name.Substring(1); }
            if (generateType == "1") { name = S.LineToCamelName(name); }
            else if (generateType == "2") { name = S.CamelToLineNmae(name); }
            type = this.GetType(type, langType);
            string tmp = "";
            if (langType == "1") //VB
            {
                tmp = "\t''' <summary>" + Environment.NewLine;
                tmp += "\t''' " + remarks + Environment.NewLine;
                tmp += "\t''' </summary>" + Environment.NewLine;
                tmp += "\t<AmbientValue(\"" + length + "\")>" + Environment.NewLine;
                tmp += "\tPublic Property " + name + " As " + type + Environment.NewLine;
                tmp += "\t\tGet " + Environment.NewLine + "\t\t\tReturn _" + name + "" + Environment.NewLine + "\t\tEnd Get" + Environment.NewLine;
                tmp += "\t\tSet(value As String) " + Environment.NewLine + "\t\t\t_" + name + " = value" + Environment.NewLine + "\t\tEnd Set" + Environment.NewLine;
                tmp += "\tEnd Property" + Environment.NewLine;
            }
            else if (langType == "3") //Java
            {

            }

            return tmp;
        }
        /// <summary>
        /// 数据库数据类转换
        /// </summary>
        /// <param name="type"></param>
        /// <param name="langType">语言分类</param>
        /// <returns></returns>
        private string GetType(string type, string langType)
        {
            type = type.ToLower();
            if (langType == "1") //VB
            {
                if (type.Contains("nvarchar")) { return "String"; }
                else if (type.Contains("varchar")) { return "String"; }
                else if (type.Contains("char")) { return "Char"; }
                else if (type.Contains("nchar")) { return "Char"; }
                else if (type.Contains("text")) { return "String"; }
                else if (type.Contains("ntext")) { return "String"; }
                else if (type.Contains("datetime")) { return "Date"; }
                else if (type.Contains("date")) { return "Date"; }
                else if (type.Contains("numeric")) { return "Decimal"; }
                else if (type.Contains("int")) { return "Integer"; }
                else if (type.Contains("bit")) { return "Boolean"; }
                else if (type.Contains("varbinary")) { return "Byte()"; }
            }
            else if (langType == "2") //C#
            {
                if (type.Contains("nvarchar")) { return "String"; }
                else if (type.Contains("varchar")) { return "String"; }
                else if (type.Contains("char")) { return "Char"; }
                else if (type.Contains("nchar")) { return "Char"; }
                else if (type.Contains("text")) { return "String"; }
                else if (type.Contains("ntext")) { return "String"; }
                else if (type.Contains("datetime")) { return "DateTime"; }
                else if (type.Contains("date")) { return "DateTime"; }
                else if (type.Contains("numeric")) { return "Decimal"; }
                else if (type.Contains("int")) { return "Integer"; }
                else if (type.Contains("bit")) { return "Boolean"; }
                else if (type.Contains("varbinary")) { return "Byte()"; }
            }
            else if (langType == "3") //Java
            {
                if (type.Contains("nvarchar")) { return "String"; }
                else if (type.Contains("varchar")) { return "String"; }
                else if (type.Contains("char")) { return "Char"; }
                else if (type.Contains("nchar")) { return "Char"; }
                else if (type.Contains("text")) { return "String"; }
                else if (type.Contains("ntext")) { return "String"; }
                else if (type.Contains("datetime")) { return "Date"; }
                else if (type.Contains("date")) { return "Date"; }
                else if (type.Contains("numeric")) { return "Decimal"; }
                else if (type.Contains("int")) { return "Integer"; }
                else if (type.Contains("bit")) { return "Boolean"; }
                else if (type.Contains("varbinary")) { return "Byte()"; }
            }
            return "";
        }

    }
}
