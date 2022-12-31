using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using H.Util;
using H.WorkTools;
using static H.Util.WhoUsePort;

namespace HWorkTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Config cif = new Config();
        System.Threading.Mutex mutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source!=null)
                {
                    dictionaryList.Add(dictionary);
                }                
            }
            string requestedCulture = @"Resources/Language/"+ cif.GetValue("Language") + ".xaml";
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

            #region 只能开一个
            bool ret;
            mutex = new System.Threading.Mutex(true, "ElectronicNeedleTherapySystem", out ret);
            if (!ret)
            {
                MessageBox.Show("已有一个程序实例运行");
                Environment.Exit(0);
            }
            #endregion

        }
    }
}
