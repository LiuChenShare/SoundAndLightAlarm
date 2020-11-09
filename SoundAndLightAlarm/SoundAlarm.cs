using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SoundAndLightAlarm
{
    public partial class SoundAlarm : Form
    {
        /// <summary>
        /// 程序启动时传过来的指令
        /// </summary>
        private string oString = null; 
        
        private static HttpListener httpPostRequest = new HttpListener();

        public SoundAlarm()
        {
            InitializeComponent();
        }
        public SoundAlarm(string str)
        {
            oString = str;
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Note.WM_COPYDATA)
            {
                string s = ((Note.CopyData)Marshal.PtrToStructure(m.LParam, typeof(Note.CopyData))).lpData;
                //MessageBox.Show(s);
                AlarmEventByStr(s);
            }
            else
                base.WndProc(ref m);
        }

        private void SoundAlarm_Load(object sender, EventArgs e)
        {
            var pts = System.IO.Ports.SerialPort.GetPortNames();
            if (pts != null && pts.Length > 0)
            {
                foreach (var ptNm in pts)
                {
                    cboAlarmerComPort.Items.Add(ptNm);
                }
                cboAlarmerComPort.Text = pts[pts.Length - 1];
            }


            httpPostRequest.Prefixes.Add("http://127.0.0.1:3213/");
            httpPostRequest.Start();

            Thread ThrednHttpPostRequest = new Thread(new ThreadStart(httpPostRequestHandle));
            ThrednHttpPostRequest.Start();

        }


        private void httpPostRequestHandle()
        {
            while (true)
            {
                HttpListenerContext requestContext = httpPostRequest.GetContext();
                Thread threadsub = new Thread(new ParameterizedThreadStart((requestcontext) =>
                {
                    HttpListenerContext request = (HttpListenerContext)requestcontext;

                    var rawUrl = request.Request.RawUrl;

                    string action = "";
                    string value = "";
                    bool success = false;
                    var pos = rawUrl.IndexOf('?');
                    if (pos == -1)
                    {
                        action = rawUrl;
                    }
                    else
                    {
                        action = rawUrl.Substring(0, pos);
                        value = rawUrl.Substring(pos + 1, rawUrl.Length - pos - 1);
                    }

                    switch (action)
                    {
                        case "/alarm":
                            AlarmEventByStr($"soundandlightalarm:{value}"); 
                            success = true;
                            break;
                        default:
                            request.Response.StatusCode = 500;
                            break;
                    }

                    request.Response.StatusCode = 200;
                    request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    request.Response.ContentType = "application/json";
                    requestContext.Response.ContentEncoding = Encoding.UTF8;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes($"{{\"success\":{success.ToString().ToLower()}}}");
                    request.Response.ContentLength64 = buffer.Length;
                    var output = request.Response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    #region test

                    ////获取Post请求中的参数和值帮助类
                    //HttpListenerPostParaHelper httppost = new HttpListenerPostParaHelper(request);
                    ////获取Post过来的参数和数据
                    //List<HttpListenerPostValue> lst = httppost.GetHttpListenerPostValue();

                    //string userName = "";
                    //string password = "";
                    //string suffix = "";
                    //string adType = "";

                    ////使用方法
                    //foreach (var key in lst)
                    //{
                    //    if (key.type == 0)
                    //    {
                    //        string value = Encoding.UTF8.GetString(key.datas).Replace("\r\n", "");
                    //        if (key.name == "username")
                    //        {
                    //            userName = value;
                    //            Console.WriteLine(value);
                    //        }
                    //        if (key.name == "password")
                    //        {
                    //            password = value;
                    //            Console.WriteLine(value);
                    //        }
                    //        if (key.name == "suffix")
                    //        {
                    //            suffix = value;
                    //            Console.WriteLine(value);
                    //        }
                    //        if (key.name == "adtype")
                    //        {
                    //            adType = value;
                    //            Console.WriteLine(value);
                    //        }
                    //    }
                    //    if (key.type == 1)
                    //    {
                    //        string fileName = request.Request.QueryString["FileName"];
                    //        if (!string.IsNullOrEmpty(fileName))
                    //        {
                    //            string filePath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyMMdd_HHmmss_ffff") + Path.GetExtension(fileName).ToLower();
                    //            if (key.name == "File")
                    //            {
                    //                FileStream fs = new FileStream(filePath, FileMode.Create);
                    //                fs.Write(key.datas, 0, key.datas.Length);
                    //                fs.Close();
                    //                fs.Dispose();
                    //            }
                    //        }
                    //    }
                    //}

                    ////Response
                    //request.Response.StatusCode = 200;
                    //request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    //request.Response.ContentType = "application/json";
                    //requestContext.Response.ContentEncoding = Encoding.UTF8;
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = "true", msg = "提交成功" }));
                    //request.Response.ContentLength64 = buffer.Length;
                    //var output = request.Response.OutputStream;
                    //output.Write(buffer, 0, buffer.Length);
                    //output.Close();
                    #endregion
                }));
                threadsub.Start(requestContext);
            }
        }


        private void SoundAlarm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void MyNotifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            this.Show(); // 窗体显现
            this.WindowState = FormWindowState.Normal; //窗体回复正常大小
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要退出？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                this.notifyIcon1.Visible = false;
                this.Close();
                this.Dispose();
                System.Environment.Exit(System.Environment.ExitCode);

            }
        }

        private void SoundAlarm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 注意判断关闭事件reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //取消"关闭窗口"事件
                e.Cancel = true; // 取消关闭窗体 

                //使关闭时窗口向右下角缩小的效果
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
                //this.m_cartoonForm.CartoonClose();
                this.Hide();
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ////var str = args[0].ToLower().Replace("soundandlightalarm:", "");
            //var aaa = new AlarmConfig();
            //aaa.ComPort = "COM5";
            //LightAlarmer.Instance.InitPara(aaa);

            //var ret = LightAlarmer.Instance.StartAlarm();
            AlarmEventByStr("soundandlightalarm:");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var ret = LightAlarmer.Instance.StopAlarm();
            AlarmEventByStr("soundandlightalarm:open=0");
        }

        private void SoundAlarm_Shown(object sender, EventArgs e)
        {
            //第一次开启程序时如果有传入的报警指令，需要处理一下
            if (!string.IsNullOrWhiteSpace(oString))
            {
                AlarmEventByStr(oString);
            }
        }

        /// <summary>
        /// 报警指令处理
        /// </summary>
        /// <param name="str"></param>
        private EnumAlarmRet AlarmEventByStr(string eventStr)
        {
            EnumAlarmRet ret = EnumAlarmRet.Invalid;
            try
            {
                if (string.IsNullOrWhiteSpace(eventStr)) { return ret; }
                AlarmConfig config;
                cboAlarmerComPort.Invoke(new Action(() =>
                {
                    config = HandleEventStr(ref eventStr);
                    ret = AlarmEvent(config);
                }));
            }
            catch(Exception ex)
            {

            }
            return ret;
        }

        /// <summary>
        /// 报警指令处理
        /// </summary>
        private EnumAlarmRet AlarmEvent(AlarmConfig config)
        {
            EnumAlarmRet ret;
            if (config.Open)
            {
                LightAlarmer.Instance.InitPara(config);
                ret = LightAlarmer.Instance.StartAlarm();
            }
            else
            {
                ret = LightAlarmer.Instance.StopAlarm();
            }
            return ret;
        }

        /// <summary>
        /// 处理报警指令的字符串
        /// </summary>
        /// <param name="eventStr"></param>
        /// <returns></returns>
        private AlarmConfig HandleEventStr(ref string eventStr)
        {
            var dic = new Dictionary<string, string>
            {
                { "open", "1" },   //1开0关
                { "time", "10" },  //默认10秒
                { "com", "0" }    //数字对应com端口，0使用默认页面上选择的端口
            };
            eventStr = eventStr.ToLower().Replace("soundandlightalarm:", "");
            var strs = eventStr.Split('&');
            foreach (var str in strs)
            {
                if (string.IsNullOrWhiteSpace(str)) { continue; }

                var pos = str.IndexOf('=');
                if (pos == -1) { continue; }

                var key = str.Substring(0, pos);
                var value = str.Substring(pos + 1, str.Length - pos - 1);
                dic[key] = value;
            }

            var config = new AlarmConfig
            {
                AlarmSeconds = (int)nudAlarmerSeconds.Value,
                ComPort = cboAlarmerComPort.Text
            };
            //config.ComPort = "COM5";
            if (dic.ContainsKey("open") && int.TryParse(dic["open"], out int open)) { config.Open = open == 1; }
            if (dic.ContainsKey("time") && int.TryParse(dic["time"], out int time)) { config.AlarmSeconds = time; }
            if (dic.ContainsKey("com") && int.TryParse(dic["com"], out int com) && com != 0) { config.ComPort = $"COM{com}"; }

            return config;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var aaa = textBox1.Text;

            AlarmEventByStr(aaa);
        }
    }
}
