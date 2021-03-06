﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SoundAndLightAlarm
{

    /// <summary>
    /// 声光报警器配置
    /// </summary>
    public class AlarmConfig
    {
        public AlarmConfig()
        {
        }



        /// <summary>
        /// 开启报警还是关闭报警 1开0关
        /// </summary>
        public bool Open { get; set; } = true;

        /// <summary>
        /// 报警时长(声光报警器)
        /// </summary>
        public int AlarmSeconds { get; set; } = 10;

        /// <summary>
        /// 串口端口号(声光报警器)
        /// </summary>
        public string ComPort { get; set; } = "";//"COM1";
        /// <summary>
        /// 波特率(声光报警器)
        /// </summary>
        public int BaudRate { get; set; } = 2400;
        /// <summary>
        /// 数据位数(声光报警器)
        /// </summary>
        public int DataBits { get; set; } = 8;
        /// <summary>
        /// 奇偶校验(声光报警器)
        /// </summary>
        public int Parity { get; set; } = 0;
        /// <summary>
        /// 停止位(声光报警器)
        /// </summary>
        public int StopBits { get; set; } = 1;
        /// <summary>
        /// 开始命令(声光报警器)
        /// </summary>
        public string StartCMD { get; set; } = "FF 01 00 08 00 FF 08";
        /// <summary>
        /// 停止命令(声光报警器)
        /// </summary>
        public string StopCMD { get; set; } = "FF 01 00 00 00 00 01";
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; set; } = "http://127.0.0.1:3213/";



        /// <summary>
        /// 从配置文件读取配置
        /// </summary>
        /// <returns></returns>
        public static AlarmConfig GetConfig()
        {
            try
            {
                string file = System.Windows.Forms.Application.StartupPath + @"\config.json";

                if (File.Exists(file))
                {
                    var str = File.ReadAllText(file);
                    return JsonConvert.DeserializeObject<AlarmConfig>(str);
                }
            }
            catch (Exception ex)
            {

            }
            return new AlarmConfig();
        }
        /// <summary>
        /// 保存配置到配置文件
        /// </summary>
        /// <returns></returns>
        public static Tuple<bool,string> SaveConfig(AlarmConfig config)
        {
            try
            {
                string file = System.Windows.Forms.Application.StartupPath + @"\config.json";

                var str = JsonConvert.SerializeObject(config);

                File.WriteAllText(file, str);
                return new Tuple<bool, string>(true, "");
            }
            catch(Exception ex)
            {
                return new Tuple<bool, string>(false, ex.ToString());
            }
        }
    }
}
