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
    public partial class 模板匹配 : Form
    {

        HWindow hWindow;
        public 模板匹配()
        {
            InitializeComponent();
            hWindow = hWindowControl1.HalconWindow;
            hWindow.SetPart(0, 0, -1, -1);
            hWindow.DispObj(主界面.hImage);
        }
        Point downPoint;

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - downPoint.X,
                    this.Location.Y + e.Y - downPoint.Y);
            }
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            downPoint = new Point(e.X, e.Y);
        }


        private void btn_Close_System_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 模板匹配_Load(object sender, EventArgs e)
        {
            hWindow.SetPart(0, 0, -1, -1);
            hWindow.DispObj(主界面.hImage);
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            hWindow.DispObj(主界面.hImage);
            
            Halcon.DetectionHalconMatch(hWindowControl1.HalconWindow,主界面.hImage, Parameters.specifications.矩形模板区域, Parameters.specifications.模板匹配,ref Halcon.rect2Match);

        }

        private void uiButton11_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawRectAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.模板匹配);
        }

        private void uiButton12_Click(object sender, EventArgs e)
        {
            XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, "Parameter/Specifications.xml");
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
