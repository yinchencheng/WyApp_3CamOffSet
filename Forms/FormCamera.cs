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
using HalconDotNet;

namespace WY_App
{
    public partial class FormCamera : Form
    {
        public static int Round_Edge = 0;

        public FormCamera()
        {
            InitializeComponent();

        }
        Point downPoint;

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            downPoint = new Point(e.X, e.Y);
        }


        private void btn_Close_System_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void xshow_Click(object sender, EventArgs e)
        {
              
        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void num_ExposureTime_ValueChanged(object sender, EventArgs e)
        {

        }

        private void FormCamera_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0; 
            num_ExposureTime.Value =(decimal)Constructor.cameraParams.ExposureTime[0];
            num_Width.Value = (decimal)Constructor.cameraParams.Height[0];
            num_AcquisitionLineRate.Value = (decimal)Constructor.cameraParams.AcquisitionLineRate[0];

            comboBox2.SelectedIndex = Constructor.cameraParams.GammaEnable[0]; 
            comboBox3.SelectedIndex = Constructor.cameraParams.PRNUCUserEnable[0]; 
            comboBox4.SelectedIndex = Constructor.cameraParams.FPNCUserEnable[0];
            comboBox5.SelectedIndex = Constructor.cameraParams.DeviceTapGeometry[0];
            comboBox6.SelectedIndex = Constructor.cameraParams.PreampGain[0];
            /// <summary>
            /// 相机新增参数设置5
            /// </summary>

        }

        private void btn_Close_System_Click_2(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            num_ExposureTime.Value = (decimal)Constructor.cameraParams.ExposureTime[comboBox1.SelectedIndex];
            num_Width.Value= (decimal)Constructor.cameraParams.Height[comboBox1.SelectedIndex];
            num_AcquisitionLineRate.Value= (decimal)Constructor.cameraParams.AcquisitionLineRate[comboBox1.SelectedIndex];

            comboBox2.SelectedIndex = Constructor.cameraParams.GammaEnable[comboBox1.SelectedIndex];
            comboBox3.SelectedIndex = Constructor.cameraParams.PRNUCUserEnable[comboBox1.SelectedIndex];
            comboBox4.SelectedIndex = Constructor.cameraParams.FPNCUserEnable[comboBox1.SelectedIndex];
            comboBox5.SelectedIndex = Constructor.cameraParams.DeviceTapGeometry[comboBox1.SelectedIndex];
            comboBox6.SelectedIndex = Constructor.cameraParams.PreampGain[comboBox1.SelectedIndex];
            /// <summary>
            /// 相机新增参数设置4
            /// </summary>

        }

        private void btn_Save_Click(object sender, EventArgs e)
        {

            Constructor.cameraParams.ExposureTime[comboBox1.SelectedIndex]=(double)num_ExposureTime.Value;
            Constructor.cameraParams.Height[comboBox1.SelectedIndex] = (int)num_Width.Value;
            Constructor.cameraParams.AcquisitionLineRate[comboBox1.SelectedIndex] = (int)num_AcquisitionLineRate.Value;

            Constructor.cameraParams.GammaEnable[comboBox1.SelectedIndex]= comboBox2.SelectedIndex;
            Constructor.cameraParams.PRNUCUserEnable[comboBox1.SelectedIndex] = comboBox3.SelectedIndex;
            Constructor.cameraParams.FPNCUserEnable[comboBox1.SelectedIndex] = comboBox4.SelectedIndex;
            Constructor.cameraParams.DeviceTapGeometry[comboBox1.SelectedIndex] = comboBox5.SelectedIndex;
            Constructor.cameraParams.PreampGain[comboBox1.SelectedIndex]= comboBox6.SelectedIndex;
            /// <summary>
            /// 相机新增参数设置6
            /// </summary>


            if (Halcon.CamConnect[comboBox1.SelectedIndex])
            {
                Halcon.SetFramegrabberParam(comboBox1.SelectedIndex, Halcon.hv_AcqHandle[comboBox1.SelectedIndex]);
               
                /// <summary>
                /// 相机新增参数设置7、去
                /// </summary>
            }
            else
            {
                MessageBox.Show("相机未连接，设置失败");
                return;
            }


            XMLHelper.serialize<Constructor.CameraParams>(Constructor.cameraParams, Parameters.commministion.productName + "/CameraParams.xml");
        }

        private void btn_ChangePassword_Click(object sender, EventArgs e)
        {
            btn_Save.Enabled = true;
        }

        private void num_Width_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
