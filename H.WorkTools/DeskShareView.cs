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
using static H.WorkTools.MainWindow;

namespace H.WorkTools
{
    public partial class DeskShareView : Form
    {
        string connectionString, niceName, mode;
        bool isControl = true;
        LvAudienceUpdate lvAudienceUpdate;
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
                    axRDPViewer1.RequestControl(RDPCOMAPILib.CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE);
                    isControl = false;
                    toolStripMenuItem1.Image = H.WorkTools.Properties.Resources._2;
                }
                else
                {
                    axRDPViewer1.RequestControl(RDPCOMAPILib.CTRL_LEVEL.CTRL_LEVEL_VIEW);
                    isControl = true;
                    toolStripMenuItem1.Image = H.WorkTools.Properties.Resources._1;
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
    }
}
