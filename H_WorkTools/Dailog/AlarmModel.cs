using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_WorkTools.Dailog
{
    /// <summary>
    /// 闹钟实体类
    /// </summary>
    public class AlarmModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        /// <summary>
        /// 类型
        /// 1：指定
        /// 2：每周
        /// 3：每月
        /// </summary>
        public string alarmType { get; set; }
        /// <summary>
        /// 1：指定(存年月日)
        /// 2：每周（存2）
        /// 3：每月（存3）
        /// </summary>
        public string data { get; set; }
        public string time { get; set; }
        /// <summary>
        /// 最后提醒时间
        /// </summary>
        public string lastTime { get; set; }
    }
}
