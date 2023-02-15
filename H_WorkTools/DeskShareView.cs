using H_Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static H_WorkTools.MainWindow;

namespace H_WorkTools
{
    public partial class DeskShareView : Form
    {
        string connectionString, niceName, mode;
        bool isControl = true;
        string jsonLanguage = "";
        Config cif = new Config();
        LvAudienceUpdate lvAudienceUpdate;
        private static string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下

        public DeskShareView(string _connectionString, string _niceName, LvAudienceUpdate  _lvAudienceUpdate)
        {
            connectionString = _connectionString.Substring(1);
            niceName = _niceName;
            mode = _connectionString.Substring(0,1);
            lvAudienceUpdate= _lvAudienceUpdate;
            InitializeComponent();
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeskShareView_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = getLanguage("DeskShareView");
                toolStripMenuItem1.Text = getLanguage("control");//控制
                axRDPViewer1.SmartSizing = true;
                if (mode == "C")
                {
                    menuStrip1.Visible = true;
                }
                else if (mode == "V")
                {
                    menuStrip1.Visible = false;
                }
                if (connectionString != null)
                {
                    try
                    {
                        axRDPViewer1.Connect(connectionString, niceName, "");
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeskShareView_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                lvAudienceUpdate();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 控制开关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (isControl)
                {
                    toolStripMenuItem1.Text = getLanguage("release");//释放
                    axRDPViewer1.RequestControl(RDPCOMAPILib.CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE);
                    isControl = false;
                    toolStripMenuItem1.Image = H_WorkTools.Properties.Resources._2;
                }
                else
                {
                    toolStripMenuItem1.Text = getLanguage("control");//控制
                    axRDPViewer1.RequestControl(RDPCOMAPILib.CTRL_LEVEL.CTRL_LEVEL_VIEW);
                    isControl = true;
                    toolStripMenuItem1.Image = H_WorkTools.Properties.Resources._1;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 会议结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionTerminated(object sender, AxRDPCOMAPILib._IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {
            this.Hide();
        }

        private void OnError(object sender, AxRDPCOMAPILib._IRDPSessionEvents_OnErrorEvent e)
        {
            int ErrorCode = (int)e.errorInfo;
        }

        private void OnConnectionEstablished(object sender, EventArgs e)
        {

        }
        private void OnConnectionFailed(object sender, EventArgs e)
        {

        }

        public string getLanguage(string key)
        {
            try
            {
                if (jsonLanguage == "")
                {
                    jsonLanguage = System.IO.File.ReadAllText(path + "Language" + "\\" + cif.GetValue("Language") + ".json");
                }
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonLanguage);
                return dic.Where(S => S.Key == key).Select(S => S.Value).First().ToString();
            }
            catch (Exception ee)
            {
                return "";
            }
        }
    }
}
