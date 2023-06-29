using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using HslCommunication.LogNet;
using static WY_App.Utility.Parameters;

namespace WY_App.Utility
{
    /// <summary>
    /// Fatal级别的日志由系统全局抓取
    /// </summary>
    
    public class LogHelper
    {
        public static ILogNet Log = new LogNetSingle(@"D:\\Logs\\"+System.DateTime.Now.ToString("yyyyMMddHH")+".txt");
        public static void WriteError(string log )
        {
           MainForm.AlarmList.Add("Error" + System.DateTime.Now.ToString() + log);
           Log.WriteError(System.DateTime.Now.ToString() + log);
        }
        public static void WriteWarn(string log)
        {
            MainForm.AlarmList.Add("Warn" + System.DateTime.Now.ToString() + log);
            Log.WriteWarn(System.DateTime.Now.ToString() + log);
        }
        public static void WriteInfo(string log)
        {
            MainForm.AlarmList.Add("Info" + System.DateTime.Now.ToString() + log);
            Log.WriteInfo(System.DateTime.Now.ToString() + log);
        }
        public static void WriteNewLine(string log)
        {            
            Log.WriteNewLine();
        }
        public static void WriteDescrition(string log)
        {
            MainForm.AlarmList.Add("Descrition" + System.DateTime.Now.ToString() + log);
            Log.WriteDescrition(System.DateTime.Now.ToString() + log);
        }
        public static void WriteDebug(string log)
        {
            MainForm.AlarmList.Add("Debug" + System.DateTime.Now.ToString() + log);
            Log.WriteDebug(System.DateTime.Now.ToString() + log);
        }
    }
    
}