using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace H_Util
{
    /// <summary>
    /// 操作winform配置文件
    /// </summary>
    public  class Config
    {
        public  Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        /// <summary>
        /// 获得值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public  string GetValue(string key)
        {
            return config.AppSettings.Settings[key].Value;
        }

        /// <summary>
        /// 修改或增加值（保存值）
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public  void SaveValue(string key, string value)
        {
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
            config.Save();
        }

        /// <summary>
        /// 删除值
        /// </summary>
        /// <param name="key">key</param>
        public  void DeleteValue(string key)
        {
            config.AppSettings.Settings.Remove(key);
        }

    }
}
