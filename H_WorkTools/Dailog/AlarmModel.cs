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
        public string title { get; set; }
        public string content { get; set; }
        /// <summary>
        /// 类型（1每日，2每周，3每月,4指定）
        /// </summary>
        public string alarmType { get; set; }
        /// <summary>
        /// 存储 123类型数据 4不存储
        /// </summary>
        public string alarm { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        /// <summary>
        /// 提醒状态
        /// 类型4 有2个状态 
        /// 类型1，2，3 根据 存储个数对应数量  全状态 都1的时候  重置 
        /// </summary>
        public string state { get; set; }
    }
}
