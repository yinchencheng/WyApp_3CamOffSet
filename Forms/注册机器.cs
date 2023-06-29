using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using WY_App.Utility;
using OpenCvSharp.XImgProc;
using static WY_App.登陆界面;
using HslCommunication;
using HslCommunication.BasicFramework;

namespace WY_App
{
    public partial class 注册机器 : Form
    {

        public delegate void TransfDelegate(string value);
        public event TransfDelegate TransfEvent;
        public 注册机器()
        {
            InitializeComponent();
            
        }
        
        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {

            TransfEvent(Deveice_ID.Text);
            Parameters.commministion.DeviceID = Deveice_ID.Text;
            XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");
            this.Close();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            SoftAuthorize softAuthorize = new SoftAuthorize();
            uiTextBox1.Text = softAuthorize.GetMachineCodeString();
        }

    }
}
