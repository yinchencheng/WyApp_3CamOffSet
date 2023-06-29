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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using HalconDotNet;
using System.Runtime.CompilerServices;
using Sunny.UI;
using OpenCvSharp.Flann;
using System.Numerics;
using OpenCvSharp.Internal.Vectors;
using Sunny.UI.Win32;
using System.Security.Cryptography;
using static WY_App.Utility.Parameters;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.Common;
using SevenZip.Compression.LZ;

namespace WY_App
{
    public partial class 检测设置 : Form
    {
        public static int Round_Edge = 0;
        System.Drawing.Point downPoint;
        public 检测设置()
        {
            InitializeComponent();          
        }
        
        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new System.Drawing.Point(this.Location.X + e.X - downPoint.X,
                    this.Location.Y + e.Y - downPoint.Y);
            }
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            downPoint = new System.Drawing.Point(e.X, e.Y);
        }


        private void btn_Close_System_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close();
        }
       

        /// <summary>
        /// 霍夫变换-直线
        /// </summary>
        /// <param name="imagePath"></param>
        private static void HoughtLine(Mat MatImage, PictureBox picture0)
        {
            
            using (Mat dst = new Mat(MatImage.Size(), MatType.CV_8UC3, Scalar.Blue))
            {
                // 1:边缘检测
                Mat canyy = new Mat(MatImage.Size(), MatImage.Type());
                Cv2.Canny(MatImage, canyy, 60, 200, 3, false);

                /*
                 *  HoughLinesP:使用概率霍夫变换查找二进制图像中的线段。
                 *  参数：
                 *      1； image: 输入图像 （只能输入单通道图像）
                 *      2； rho:   累加器的距离分辨率(以像素为单位) 生成极坐标时候的像素扫描步长
                 *      3； theta: 累加器的角度分辨率(以弧度为单位)生成极坐标时候的角度步长，一般取值CV_PI/180 ==1度
                 *      4； threshold: 累加器阈值参数。只有那些足够的行才会返回 投票(>阈值)；设置认为几个像素连载一起                     才能被看做是直线。
                 *      5； minLineLength: 最小线长度，设置最小线段是有几个像素组成。
                 *      6；maxLineGap: 同一条线上的点之间连接它们的最大允许间隙。(默认情况下是0）：设置你认为像素之间                     间隔多少个间隙也能认为是直线
                 *      返回结果:
                 *      输出线。每条线由一个4元向量(x1, y1, x2，y2)
                 */
                LineSegmentPoint[] linePiont = Cv2.HoughLinesP(canyy, 1, 1, 1, 5, 10);//只能输入单通道图像
                Scalar color = new Scalar(0, 255, 255);
                for (int i = 0; i < linePiont.Count(); i++)
                {
                    OpenCvSharp.Point p1 = linePiont[i].P1;
                    OpenCvSharp.Point p2 = linePiont[i].P2;
                    Cv2.Line(dst, p1, p2, color, 4, LineTypes.Link8);
                }

                picture0.Image = dst.ToBitmap();
                Cv2.WaitKey(0);
                
            }
        }



        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btn_加载检测图片_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();

            if (openfile.ShowDialog() == DialogResult.OK && (openfile.FileName != "")) 
            {
                //picture0.ImageLocation = openfile.FileName;
                //MatImage = Cv2.ImRead(openfile.FileName);
                Halcon.ImgDisplay(openfile.FileName, hWindowControl1.HalconWindow);;
            }
            openfile.Dispose();
        }

        private void btn_绘制检测区域_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawRectAOI(hWindowControl1.HalconWindow,主界面.hImage,ref Parameters.specifications.矩形模板区域);
        }

        

        

        private void btn_SaveParams_Click(object sender, EventArgs e)
        {
            XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, "Parameter/Specifications.xml");
        }

        private void 检测设置_Load(object sender, EventArgs e)
        {

            num_AreaHigh.Value = Parameters.specifications.AreaHigh;
            num_AreaLow.Value = Parameters.specifications.AreaLow;
            num_ThresholdHigh.Value = Parameters.specifications.ThresholdHigh;
            num_ThresholdLow.Value = Parameters.specifications.ThresholdLow;
            num_AreaHigh2.Value = Parameters.specifications.AreaHigh2;
            num_AreaLow2.Value = Parameters.specifications.AreaLow2;
            num_ThresholdHigh2.Value = Parameters.specifications.ThresholdHigh2;
            num_ThresholdLow2.Value = Parameters.specifications.ThresholdLow2;
            num_PixelResolution.Value = Parameters.specifications.PixelResolution;
            chk_SaveOrigalImage.Checked = Parameters.specifications.SaveOrigalImage;
            chk_SaveDefeatImage.Checked = Parameters.specifications.SaveDefeatImage;
            chk_Rect.Checked = Parameters.specifications.RectEnabled;
            chk_trian.Checked = Parameters.specifications.TrianEnabled;
            chk_Cricle.Checked = Parameters.specifications.CricleEnabled;
        }



        static rent2[] rent;

        private void btn_显示检测区域_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconSlelct(hWindowControl1.HalconWindow,主界面.hImage,Parameters.specifications.矩形模板区域, ref rent);
        }

        private void num_PixelResolution_ValueChanged(object sender, double value)
        {
            Parameters.specifications.PixelResolution = num_PixelResolution.Value;
        }

        private void chk_SaveDefeatImage_CheckedChanged(object sender, EventArgs e)
        {
            Parameters.specifications.SaveDefeatImage = chk_SaveDefeatImage.Checked;
        }

        private void chk_SaveOrigalImage_CheckedChanged(object sender, EventArgs e)
        {
            Parameters.specifications.SaveOrigalImage = chk_SaveOrigalImage.Checked;
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconCricle(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.圆形检测, Parameters.specifications.圆形检测.Row, Parameters.specifications.圆形检测.Colum, 20, ref pointReaultCricle);

            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            time.Text = milliseconds.ToString();
        }


        private void uiButton5_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawLineAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.三角检测[0]);
        }

        private void uiButton7_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawLineAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.三角检测[1]);
        }

        private void uiButton6_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawLineAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.三角检测[2]);
        }

        public void Mat2HObjectBpp24(Mat mat, out HObject image)
        {
            int ImageWidth = mat.Width;
            int ImageHeight = mat.Height;
            int channel = mat.Channels();
            long size = ImageWidth * ImageHeight * channel;
            int col_byte_num = ImageWidth * channel;

            byte[] rgbValues = new byte[size];
            //IntPtr imgptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(rgbValues.Length);
            unsafe
            {
                for (int i = 0; i < mat.Height; i++)
                {
                    IntPtr c = mat.Ptr(i);
                    //byte* c1 = (byte*)c;
                    System.Runtime.InteropServices.Marshal.Copy(c, rgbValues, i * col_byte_num, col_byte_num);
                }

                void* p;
                IntPtr ptr;
                fixed (byte* pc = rgbValues)
                {
                    p = (void*)pc;
                    ptr = new IntPtr(p);

                }
                HOperatorSet.GenImageInterleaved(out image, ptr, "bgr", ImageWidth, ImageHeight, 0, "byte", 0, 0, 0, 0, -1, 0);

            }

        }
        public static HRect1[] pointReaultLine = new HRect1[3];
        public static Rect2 pointReaultRect = new Rect2();
        public static Cricle pointReaultCricle = new Cricle();

        private void uiButton10_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconSlelct(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.三角模板区域, ref rent);
            Halcon.DetectionHalconMatch(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.矩形模板区域, Parameters.specifications.模板匹配, ref Halcon.rect2Match);
            Parameters.specifications.三角坐标偏移[0].Row1 = rent[0].Row - Parameters.specifications.三角检测[0].Row1;
            Parameters.specifications.三角坐标偏移[0].Colum1 = rent[0].Column - Parameters.specifications.三角检测[0].Colum1;
            Parameters.specifications.三角坐标偏移[0].Row2 = rent[0].Row - Parameters.specifications.三角检测[0].Row2;
            Parameters.specifications.三角坐标偏移[0].Colum2 = rent[0].Column - Parameters.specifications.三角检测[0].Colum2;
            Halcon.DetectionHalconLine(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.三角检测[0], Halcon.rect2Match[0], Parameters.specifications.三角坐标偏移[0],10, ref pointReaultLine[0]);
            
        }

        private void uiButton8_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconSlelct(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.模板匹配, ref rent);
            Halcon.DetectionHalconMatch(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.矩形模板区域, Parameters.specifications.模板匹配, ref Halcon.rect2Match);
            Parameters.specifications.三角坐标偏移[1].Row1 = rent[0].Row - Parameters.specifications.三角检测[1].Row1;
            Parameters.specifications.三角坐标偏移[1].Colum1 = rent[0].Column - Parameters.specifications.三角检测[1].Colum1;
            Parameters.specifications.三角坐标偏移[1].Row2 = rent[0].Row - Parameters.specifications.三角检测[1].Row2;
            Parameters.specifications.三角坐标偏移[1].Colum2 = rent[0].Column - Parameters.specifications.三角检测[1].Colum2;
            Halcon.DetectionHalconLine(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.三角检测[1], Halcon.rect2Match[0], Parameters.specifications.三角坐标偏移[1], 10,  ref pointReaultLine[1]);
            
        }

        private void uiButton9_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconSlelct(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.模板匹配, ref rent);
            Halcon.DetectionHalconMatch(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.矩形模板区域, Parameters.specifications.模板匹配, ref Halcon.rect2Match);
            Parameters.specifications.三角坐标偏移[2].Row1 = rent[0].Row - Parameters.specifications.三角检测[2].Row1;
            Parameters.specifications.三角坐标偏移[2].Colum1 = rent[0].Column - Parameters.specifications.三角检测[2].Colum1;
            Parameters.specifications.三角坐标偏移[2].Row2 = rent[0].Row - Parameters.specifications.三角检测[2].Row2;
            Parameters.specifications.三角坐标偏移[2].Colum2 = rent[0].Column - Parameters.specifications.三角检测[2].Colum2;
            Halcon.DetectionHalconLine(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.三角检测[2], Halcon.rect2Match[0], Parameters.specifications.三角坐标偏移[2],10, ref pointReaultLine[2]);
            
        }

      

        static HTuple[] point= new HTuple[6];

       
        private void uiButton21_Click(object sender, EventArgs e)
        {
            基准框 flg = new 基准框();
            flg.ShowDialog();
        }

        private void btn_Minimizid_System_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

       

        private void num_ThresholdLow_ValueChanged(object sender, double value)
        {
            Parameters.specifications.ThresholdLow = value;
        }

        private void num_ThresholdHigh_ValueChanged(object sender, double value)
        {
            Parameters.specifications.ThresholdHigh = value;
        }

        private void num_AreaLow_ValueChanged(object sender, double value)
        {
            Parameters.specifications.AreaLow = value;
        }

        private void num_AreaHigh_ValueChanged(object sender, double value)
        {
            Parameters.specifications.AreaHigh = value;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawCriclaAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.圆形检测);
        }

        private void uiButton23_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawRectAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.矩形检测);
        }

        private void uiButton24_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconRect(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.矩形检测, (Parameters.specifications.矩形检测.Row1+ Parameters.specifications.矩形检测.Row2)/2, (Parameters.specifications.矩形检测.Colum1+Parameters.specifications.矩形检测.Colum2)/2, 10, ref pointReaultRect);
        }

        private void chk_Cricle_CheckedChanged(object sender, EventArgs e)
        {
            Parameters.specifications.CricleEnabled = chk_Cricle.Checked;
            Parameters.specifications.RectEnabled = !chk_Cricle.Checked;
            Parameters.specifications.TrianEnabled = !chk_Cricle.Checked;
            uiButton1.Enabled= chk_Cricle.Checked;
            uiButton2.Enabled= chk_Cricle.Checked;
        }

        private void chk_trian_CheckedChanged(object sender, EventArgs e)
        {
            Parameters.specifications.TrianEnabled = chk_trian.Checked;
            Parameters.specifications.CricleEnabled = !chk_trian.Checked;
            Parameters.specifications.RectEnabled = !chk_trian.Checked;
            uiButton3.Enabled = chk_trian.Checked;
            uiButton4.Enabled = chk_trian.Checked;
            uiButton5.Enabled = chk_trian.Checked;
            uiButton6.Enabled = chk_trian.Checked;
            uiButton7.Enabled = chk_trian.Checked;
            uiButton8.Enabled = chk_trian.Checked;
            uiButton9.Enabled = chk_trian.Checked;
            uiButton10.Enabled = chk_trian.Checked;
        }

        private void chk_Rect_CheckedChanged(object sender, EventArgs e)
        {
            Parameters.specifications.RectEnabled = chk_Rect.Checked;
            Parameters.specifications.TrianEnabled = !chk_Rect.Checked;
            Parameters.specifications.CricleEnabled = !chk_Rect.Checked;
            uiButton23.Enabled = chk_Rect.Checked;
            uiButton24.Enabled = chk_Rect.Checked;
        }

        private void uiButton13_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            bool DetectionResult = true;
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            HOperatorSet.SetPart(hWindowControl1.HalconWindow, 0, 0, -1, -1);
            Halcon.Detection(hWindowControl1.HalconWindow, 主界面.hImage,ref DetectionResult);
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            time.Text = milliseconds.ToString();
            

        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            Halcon.DetectionDrawRectAOI(hWindowControl1.HalconWindow, 主界面.hImage, ref Parameters.specifications.模板匹配);
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(主界面.hImage, hWindowControl1.HalconWindow);
            Halcon.DetectionHalconSlelct(hWindowControl1.HalconWindow, 主界面.hImage, Parameters.specifications.模板匹配, ref rent);
        }

        private void uiDoubleUpDown4_ValueChanged(object sender, double value)
        {
            Parameters.specifications.ThresholdLow2 = value;
        }

        private void uiDoubleUpDown3_ValueChanged(object sender, double value)
        {
            Parameters.specifications.ThresholdHigh2 = value;
        }

        private void uiDoubleUpDown2_ValueChanged(object sender, double value)
        {
            Parameters.specifications.AreaLow2 = value;
        }

        private void uiDoubleUpDown1_ValueChanged(object sender, double value)
        {
            Parameters.specifications.AreaHigh2 = value;
        }

        private void uiButton11_Click(object sender, EventArgs e)
        {
            模板匹配 flg = new 模板匹配();
            flg.ShowDialog();
        }

        private void uiCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            uiButton11.Enabled = true;
            uiButton11.Enabled = true;
        }
    }
}
