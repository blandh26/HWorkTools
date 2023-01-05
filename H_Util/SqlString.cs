using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    class SqlString
    {
        /// <summary>
        /// 返回每个属性封装代码【表】
        /// </summary>
        /// <param name="sqlType">数据库类型</param>
        /// <param name="operation">操作类型</param>
        /// <param name="tableName">表名</param>
        /// <param name="field">字段</param>
        /// <param name="data">数据</param>
        /// <param name="where">条件字段</param>
        /// <returns></returns>
        public string GetGenerate(string sqlType, bool operation, string tableName, string field, string data, string where)
        {
            string result_str = "";
            try
            {
                string[] flist = field.Replace("\n", "").Split('\r');
                string[] dlist = data.Replace("\n", "").Split('\r');
                string[] wlist = where.Replace("\n", "").Split('\r');

                if (sqlType=="")
                {

                }
                else if (sqlType == "")
                { }
                else if (sqlType == "")
                { }
                return result_str;
            }
            catch (Exception ex)
            {
                return result_str;
            }
        }
    }
}
