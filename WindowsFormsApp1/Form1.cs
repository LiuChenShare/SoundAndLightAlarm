using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //NotifyIcon MyNotifyIcon = new NotifyIcon();//实例化

            //MyNotifyIcon.Visible = true;//可见性
            //MyNotifyIcon.Text = "我的托盘程序";//鼠标放在托盘时显示的文字
            //MyNotifyIcon.BalloonTipText = "我的托盘程序";//气泡显示的文字
            //MyNotifyIcon.ShowBalloonTip(1000);//托盘气泡的显现时间
            //MyNotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);//托盘的外观（系统）

            ////MyNotifyIcon.ContextMenuStrip.Items.Add(new ToolStripItemCollection())

            ////自定义托盘外观
            ////Icon i = new Icon("FileName");
            ////MyNotifyIcon.Icon = i;
            //MyNotifyIcon.MouseDoubleClick += MyNotifyIcon_MouseDoubleClick;//托盘的鼠标窗机时间注册方法


            notifyIcon1.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);//托盘的外观（系统）

        }
        private void MyNotifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            this.Show(); // 窗体显现
            this.WindowState = FormWindowState.Normal; //窗体回复正常大小
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
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


        private void Form1_Load(object sender, EventArgs e)
        {
            var pts = System.IO.Ports.SerialPort.GetPortNames();
            if (pts != null && pts.Length > 0)
            {
                foreach (var ptNm in pts)
                {
                    cboAlarmerComPort.Items.Add(ptNm);
                }
            }
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Note.WM_COPYDATA)
            {
                string s = ((Note.CopyData)Marshal.PtrToStructure(m.LParam, typeof(Note.CopyData))).lpData;
                MessageBox.Show(s);
            }
            else
                base.WndProc(ref m);

        }
    }
}
