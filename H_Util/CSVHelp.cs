using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public class CSVHelp
    {
        public static bool DatatableToCSV(DataTable dt, string fileName,string path)
        {
            bool createFLAG = false;

            try
            {
                StringBuilder sb = new StringBuilder();
                string line = "";

                if (dt != null && dt.Rows.Count > 0)
                {
                    //table head
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        line += string.Format("\"{0}\",", dt.Columns[i].ColumnName);
                    }

                    line = line.TrimEnd(',');
                    sb.AppendLine(line);

                    //every row
                    foreach (DataRow row in dt.Rows)
                    {
                        line = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            line += string.Format("\"{0}\",", row[j].ToString().Replace("\"", "\"\""));
                        }

                        line = line.TrimEnd(',');
                        sb.AppendLine(line);
                    }

                    //write file
                    //日志文件夹路径
                    string LogFileWJJ = AppDomain.CurrentDomain.BaseDirectory + path;

                    if (File.Exists(LogFileWJJ) == false)
                    {
                        //不存在MyLog文件夹就创建
                        Directory.CreateDirectory(LogFileWJJ);
                    }

                    //当前日期的文件夹路径
                    string jinTianWJJ = LogFileWJJ + "\\" + DateTime.Now.ToString("yyyy-MM-dd");

                    if (File.Exists(jinTianWJJ) == false)
                    {
                        //不存在当前日期的文件夹就创建
                        Directory.CreateDirectory(jinTianWJJ);
                    }

                    //日志TXT文件
                    string csvName = jinTianWJJ + "\\" + fileName + ".csv";

                    File.WriteAllText(csvName, sb.ToString(), Encoding.UTF8);
                }//end if 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return createFLAG;
        }
    }
}
