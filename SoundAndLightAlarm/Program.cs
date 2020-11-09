using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SoundAndLightAlarm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //认为是手动启动
            if (args == null || args.Count() == 0)
            {
                //args = new string[1] { "soundandlightalarm:" };
                args = new string[1] { "" };
            }

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
                Note.SendMsg("SoundAlarm", args[0]);
                return;
            }
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SoundAlarm(args[0]));
        }
    }
}
