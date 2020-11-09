using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace SoundAndLightAlarm
{
    /// <summary>
    /// 报警器
    /// </summary>
    public class LightAlarmer
    {
        private static object _locker = new object();
        private static LightAlarmer instance;

        SerialPort SP = new SerialPort();

        private System.Timers.Timer MyTimer = new System.Timers.Timer(1000);

        private DateTime m_StartTime = DateTime.Now;
        private bool m_bIsAlarm = false;//是否正在报警

        //private int m_AlarmerEnable = 0;//报警器是否有效 0－无效 1－有效
        private int m_AlarmSeconds = 10;//报警时长（秒）
        private string m_ComPort = "COM1";//通讯端口
        private int m_BaudRate = 2400;//波特率

        private int m_Parity = 0;//奇偶校验位
        private int m_StopBits = 1;//停止位数
        private int m_DataBits = 8;//数据位数

        private byte[] m_StartCMD = new byte[0];//启动命令
        private byte[] m_StopCMD = new byte[0];//停止命令

        AlarmConfig m_userConfig = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userConfig"></param>
        public LightAlarmer()
        {
            MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(MyTimer_Elapsed);
        }

        public static LightAlarmer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_locker)
                    {
                        if (instance == null)
                        {
                            instance = new LightAlarmer();
                        }
                    }
                }
                return instance;
            }
        }

        private void MyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan ts = DateTime.Now - m_StartTime;
            if (ts.Seconds >= m_AlarmSeconds)
            {
                StopAlarm();
                MyTimer.Stop();
            }
        }

        #region 公共属性
        /// <summary>
        /// 报警时长（秒）
        /// </summary>
        public int FD_AlarmSeconds
        {
            get { return m_AlarmSeconds; }
        }

        public int FD_DataBits { set { m_DataBits = value; } }
        public int FD_Parity { set { m_Parity = value; } }
        public int FD_StopBits { set { m_StopBits = value; } }
        #endregion 公共属性

        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        public void InitPara(AlarmConfig userConfig)
        {
            m_userConfig = userConfig;
            m_AlarmSeconds = m_userConfig.AlarmSeconds;
            m_ComPort = m_userConfig.ComPort;
            m_BaudRate = m_userConfig.BaudRate;

            m_DataBits = m_userConfig.DataBits;
            m_Parity = m_userConfig.Parity;
            m_StopBits = m_userConfig.StopBits;

            string cmd = m_userConfig.StartCMD;
            m_StartCMD = BuildHexCMD(cmd);
            cmd = m_userConfig.StopCMD;
            m_StopCMD = BuildHexCMD(cmd);
        }

        /// <summary>
        /// 测试报警
        /// </summary>
        /// <param name="comPort">端口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="alarmSeconds">报警时长</param>
        /// <param name="startCMD">启动命令</param>
        /// <param name="stopCMD">停止命令</param>
        public EnumAlarmRet TestAlarm(string comPort, int baudRate, int alarmSeconds, string startCMD, string stopCMD)
        {
            EnumAlarmRet ret = EnumAlarmRet.OtherError;



            m_AlarmSeconds = alarmSeconds;
            m_ComPort = comPort;
            m_BaudRate = baudRate;

            startCMD = startCMD.Trim();
            m_StartCMD = BuildHexCMD(startCMD);
            stopCMD = stopCMD.Trim();
            m_StopCMD = BuildHexCMD(stopCMD);

            ret = StartAlarm();

            return ret;
        }



        /// <summary>
        /// 启动报警
        /// </summary>
        public EnumAlarmRet StartAlarm()
        {
            EnumAlarmRet ret = EnumAlarmRet.OtherError;

            try
            {
                if (m_bIsAlarm)
                {
                    ret = EnumAlarmRet.AlarmON;
                }
                else
                {
                    if (m_StartCMD.Length > 0)
                    {
                        if (OpenPort())
                        {
                            SP.Write(m_StartCMD, 0, m_StartCMD.Length);
                            m_bIsAlarm = true;
                            m_StartTime = DateTime.Now;
                            MyTimer.Start();
                            ret = EnumAlarmRet.OK;
                        }
                        else
                        {
                            ret = EnumAlarmRet.ComError;
                        }
                        ClosePort();
                    }
                    else
                    {
                        ret = EnumAlarmRet.OtherError;
                    }
                }
            }
            catch
            {
                ret = EnumAlarmRet.OtherError;
            }


            return ret;
        }

        /// <summary>
        /// 停止报警
        /// </summary>
        public EnumAlarmRet StopAlarm()
        {
            EnumAlarmRet ret = EnumAlarmRet.OtherError;
            try
            {
                if (m_bIsAlarm)
                {
                    MyTimer.Stop();
                    m_bIsAlarm = false;

                    if (m_StartCMD.Length > 0)
                    {
                        if (OpenPort())
                        {
                            SP.Write(m_StopCMD, 0, m_StopCMD.Length);
                            ret = EnumAlarmRet.OK;
                        }
                        else
                        {
                            ret = EnumAlarmRet.ComError;
                        }
                        ClosePort();
                    }
                    else
                    {
                        ret = EnumAlarmRet.OtherError;
                    }
                }
                else
                {
                    ret = EnumAlarmRet.AlarmOFF;
                }
            }
            catch
            {
                ret = EnumAlarmRet.OtherError;
            }
            return ret;
        }
        /// <summary>
        /// 将命令字符串转换成二进制数组
        /// </summary>
        /// <param name="CMDString">命令字符串</param>
        /// <returns>byte[]</returns>
        private byte[] BuildHexCMD(string CMDString)
        {
            byte[] ret = new byte[0];
            try
            {
                string[] hexValuesSplit = CMDString.Split(' ');
                if (hexValuesSplit.Length > 0)
                {
                    ret = new byte[hexValuesSplit.Length];
                    for (int i = 0; i < hexValuesSplit.Length; i++)
                    {
                        ret[i] = Convert.ToByte(hexValuesSplit[i], 16);
                    }
                }
            }
            catch
            {
                ret = new byte[0];
            }
            return ret;
        }
        #endregion 公共函数

        #region 内部函数
        /// <summary>
        /// 打开通讯端口
        /// </summary>
        /// <returns></returns>
        private bool OpenPort()
        {
            bool ret = false;
            try
            {
                SP.Close();
                SP.PortName = m_ComPort;
                SP.BaudRate = m_BaudRate;
                SP.DataBits = m_DataBits;
                SP.Parity = (Parity)m_Parity;
                SP.StopBits = (StopBits)m_StopBits;

                SP.WriteTimeout = 200;
                SP.ReadTimeout = 300;

                SP.ReadBufferSize = 1024;
                SP.WriteBufferSize = 1024;
                SP.Open();

                ret = true;
            }
            catch(Exception ex)
            {
                if (SP.IsOpen)
                {
                    SP.Close();
                }
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// 关闭通讯端口
        /// </summary>
        private void ClosePort()
        {
            try
            {
                if (SP.IsOpen)
                {
                    SP.DiscardInBuffer();
                    SP.DiscardOutBuffer();
                    SP.Close();
                }
            }
            catch
            {
            }
        }
        #endregion 内部函数





    }
    /// <summary>
    /// 报警器返回值
    /// </summary>
    public enum EnumAlarmRet
    {
        /// <summary> 启动报警命令已经下发成功 </summary>
        OK = 0,
        /// <summary>  </summary>
        Invalid = 1,
        /// <summary>  </summary>
        AlarmOFF = 2,
        /// <summary> 当前报警器处于报警状态，无需下发启动报警命令 </summary>
        AlarmON = 3,
        /// <summary> 打开端口出现错误，请设置正确的通讯端口 </summary>
        ComError = 4,
        /// <summary>  </summary>
        OtherError = 5
    }
}
