using Microsoft.Win32;
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

            SetAutoBootStatu(true);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SoundAlarm(args[0]));
        }

        /// <summary>  
        /// 在注册表中添加、删除开机自启动键值  
        /// </summary>  
        public static int SetAutoBootStatu(bool isAutoBoot)
        {
            try
            {
                string execPath = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (isAutoBoot)
                {
                    rk2.SetValue("SoundAndLightAlarm", execPath);
                    Console.WriteLine(string.Format("[注册表操作]添加注册表键值：path = {0}, key = {1}, value = {2} 成功", rk2.Name, "TuniuAutoboot", execPath));
                }
                else
                {
                    rk2.DeleteValue("SoundAndLightAlarm", false);
                    Console.WriteLine(string.Format("[注册表操作]删除注册表键值：path = {0}, key = {1} 成功", rk2.Name, "TuniuAutoboot"));
                }
                rk2.Close();
                rk.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[注册表操作]向注册表写开机启动信息失败, Exception: {0}", ex.Message));
                return -1;
            }
        }
    }
}
