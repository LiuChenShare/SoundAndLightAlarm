using SoundAndLightAlarm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args == null || args.Count() == 0)
            {
                args = new string[1] { "soundandlightalarm:" };
            }
            var str = args[0].ToLower().Replace("soundandlightalarm:", "");
            var aaa = new AlarmConfig();
            aaa.ComPort = "COM5";
            //LightAlarmer.Instance.InitPara(aaa);

            //var ret = LightAlarmer.Instance.StartAlarm();

            #region 判断程序是否已打开
            int processCount = 0;
            var processName = Process.GetCurrentProcess().ProcessName;
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            foreach (Process PTest in pa)
            {
                if (PTest.ProcessName == processName)
                {
                    processCount += 1;
                }
            }
            if (processCount > 1)
            {
                Note.SendMsg("SoundAndLightAlarm", "嘿嘿嘿嘿嘿");
                return;
            }
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
