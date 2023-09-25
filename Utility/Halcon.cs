using HalconDotNet;
using OpenCvSharp;
using SevenZip.Compression.LZ;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WY_App;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static WY_App.Utility.Parameters;

namespace WY_App.Utility
{
    public class Halcon
    {
        public static HTuple[] hv_Width = new HTuple[4];
        public static HTuple[] hv_Height = new HTuple[4];
        public static HTuple[] hv_AcqHandle = new HTuple[4];
        public static bool[] CamConnect = new bool[4] { false ,false,false,false};
        public static bool initalCamera(string CamID, ref HTuple hv_AcqHandle)
        {
            try
            {
                //获取相机句柄
                HOperatorSet.OpenFramegrabber("GenICamTL", 0, 0, 0, 0, 0, 0, "progressive", -1, "default", -1, "false", "default", CamID, 0, -1, out hv_AcqHandle);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "ScanMode", "LineScan");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerSelector", "LineStart");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerMode", "On");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerSource", "CC1");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "grab_timeout", 20000);

				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]DeviceTemperatureSelector", "Mainboard");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]StreamTriggerEnable", 1);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]StreamTriggerSource", "D485Input1");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]EncoderSelector", "Encoder0");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]EncoderSourceA", "D485Input1");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]EncoderSourceB", "Off");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]LineSelector", "D485InOut1");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]LineMode", "Input");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]LineInputPolarity", "SingleEnded");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]LineDebouncerTime", 300000);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]UserOutputSelector", "UserOutput0");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]UserOutputValue", 0);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]CameraControlEnable", 1);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]CameraControlSource", "Timer0");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerSelector", "Timer0");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerDuration", 5);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerDelay", 5);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerFrequency", 100000);
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerTriggerSource", "Continuous");
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]TimerTriggerActivation", "RisingEdge");

				return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(System.DateTime.Now.ToString() + CamID + "相机链接失败" + ex.Message);               
                return false;
            }

        }

        public Halcon()
        {

            Thread th = new Thread(ini_Cam);
            th.IsBackground = true;
            th.Start();

        }

        private void ini_Cam()
        {
            while (true)
            {
                //Thread.Sleep(5000);
                while (!CamConnect[0])
                {
                    Thread.Sleep(5000);
                    if (!CamConnect[0])
                    {
                        CamConnect[0] = initalCamera("LineCam0", ref hv_AcqHandle[0]);                        
                    }
                    else
                    {
                        LogHelper.WriteInfo(System.DateTime.Now.ToString() + "相机1链接成功");                       
                    }
                }
                while (!CamConnect[1])
                {
                    Thread.Sleep(5000);
                    if (!CamConnect[1])
                    {
                        CamConnect[1] = initalCamera("LineCam1", ref hv_AcqHandle[1]);                        
                    }
                    else
                    {
                        LogHelper.WriteInfo(System.DateTime.Now.ToString() + "相机2链接成功");
                    }
                } 
                while (!CamConnect[2])
                {
                    Thread.Sleep(5000);
                    if (!CamConnect[2])
                    {
                        CamConnect[2] = initalCamera("LineCam2", ref hv_AcqHandle[2]);                       
                    }
                    else
                    {
                        LogHelper.WriteInfo(System.DateTime.Now.ToString() + "相机3链接成功");
                    }
                }
            }
        }
        public static bool ImgDisplay(int index,string imgPath, HWindow Hwindow1, HWindow Hwindow2)
        {
            MainForm.hImage[index].Dispose();
            HOperatorSet.GenEmptyObj(out MainForm.hImage[index]);
            
            HOperatorSet.ReadImage(out MainForm.hImage[index], imgPath);//读取图片存入到HalconImage           
            HOperatorSet.GetImageSize(MainForm.hImage[index], out hv_Width[index], out hv_Height[index]);//获取图片大小规格
            HOperatorSet.SetPart(Hwindow1, 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(Hwindow2, 0, 0, -1, -1);//设置窗体的规格

            HOperatorSet.DispObj(MainForm.hImage[index], Hwindow1);//显示图片
            HOperatorSet.DispObj(MainForm.hImage[index], Hwindow2);//显示图片

            return true;
        }

        public static bool GrabImage(int i,  HTuple hv_AcqHandle, out HObject ho_Image)
        {
            try
            {
                HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, -1);
                return true;
            }
            catch
            {
                ho_Image = null;
                return false;
            }
        }

		public static bool ImgDisplayAlone(int index, string imgPath, HWindow Hwindow)
		{
			MainForm.hImage[index].Dispose();
			HOperatorSet.GenEmptyObj(out MainForm.hImage[index]);

			HOperatorSet.ReadImage(out MainForm.hImage[index], imgPath);//读取图片存入到HalconImage           
			HOperatorSet.GetImageSize(MainForm.hImage[index], out hv_Width[index], out hv_Height[index]);//获取图片大小规格
			HOperatorSet.SetPart(Hwindow, 0, 0, -1, -1);//设置窗体的规格
			HOperatorSet.DispObj(MainForm.hImage[index], Hwindow);//显示图片
			return true;
		}

		public static bool CloseFramegrabber(HTuple hv_AcqHandle)
		{
			HOperatorSet.CloseFramegrabber(hv_AcqHandle);
			return false;
		}
		public static void TriggerModeOff(HTuple hv_AcqHandle)
        {
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerMode", "Off");
        }
        public static void TriggerModeOn(HTuple hv_AcqHandle)
        {
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerMode", "On");
        }
        public static void SetFramegrabberParam(int i,HTuple hv_AcqHandle)
        {

			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "ExposureTime", Constructor.cameraParams.ExposureTime[i]);
			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "[Interface]ImageHeight", Constructor.cameraParams.Height[i]);
			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "AcquisitionLineRate", Constructor.cameraParams.AcquisitionLineRate[i]);

			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "GammaEnable", Constructor.cameraParams.GammaEnable[i]);
			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "PRNUCUserEnable", Constructor.cameraParams.PRNUCUserEnable[i]);
			HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "FPNCUserEnable", Constructor.cameraParams.FPNCUserEnable[i]);

			if (Constructor.cameraParams.DeviceTapGeometry[i] == 0)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "DeviceTapGeometry", "Geometry_1X2");
			}
			else if (Constructor.cameraParams.DeviceTapGeometry[i] == 1)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "DeviceTapGeometry", "Geometry_1X4");
			}
			else if (Constructor.cameraParams.DeviceTapGeometry[i] == 2)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "DeviceTapGeometry", "Geometry_1X8");
			}
			else if (Constructor.cameraParams.DeviceTapGeometry[i] == 3)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "DeviceTapGeometry", "Geometry_1X10");
			}

			if (Constructor.cameraParams.PreampGain[i] == 0)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "PreampGain", "gain_1000x");
			}
			else if (Constructor.cameraParams.PreampGain[i] == 1)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "PreampGain", "gain_2000x");
			}
			else if (Constructor.cameraParams.PreampGain[i] == 2)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "PreampGain", "gain_4000x");
			}
			else if (Constructor.cameraParams.PreampGain[i] == 3)
			{
				HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "PreampGain", "gain_8000x");
			}
		}
        public static void GrabImageAsync(HTuple hv_AcqHandle, out HObject himage)
        {
            HOperatorSet.GrabImageAsync(out himage, hv_AcqHandle, -1);
        }
        public static void GrabImageStart(HTuple hv_AcqHandle)
        {
            HOperatorSet.GrabImageStart(hv_AcqHandle, -1);
        }

		public static void ImgZoomTwo(HObject L_Img, HTuple Hwindow, HTuple HwindowTwo, int Delta = 1)
		{
			HTuple Zoom = new HTuple(), Row = new HTuple(), Col = new HTuple(), L_Button = new HTuple();
			HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
			HTuple Row0 = new HTuple(), Column0 = new HTuple(), Row00 = new HTuple(), Column00 = new HTuple(), Ht = new HTuple(), Wt = new HTuple();
			HTuple[] Now_Pos = new HTuple[4];
			try
			{

				if (Delta > 0)//鼠标滚动格值，一般120
				{
					Zoom = 1.001;//向上滚动,放大倍数
				}
				else
				{
					Zoom = 0.999;//向下滚动,缩小倍数
				}
				HOperatorSet.GetMposition(Hwindow, out Row, out Col, out L_Button);//获取当前鼠标的位置
				HOperatorSet.GetPart(Hwindow, out Row0, out Column0, out Row00, out Column00);//获取当前窗体的大小规格
				HOperatorSet.GetImageSize(L_Img, out hv_Width, out hv_Height);//获取图片大小规格
				Ht = Row00 - Row0;
				Wt = Column00 - Column0;
				if (Ht * Wt < 32000 * 32000 || Zoom == 1.2)
				{
					Now_Pos[0] = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
					Now_Pos[1] = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
					Now_Pos[2] = Now_Pos[0] + (Ht / Zoom);
					Now_Pos[3] = Now_Pos[1] + (Wt / Zoom);

					HOperatorSet.SetPart(Hwindow, Now_Pos[0], Now_Pos[1], Now_Pos[2], Now_Pos[3]);
					HOperatorSet.ClearWindow(Hwindow);
					HOperatorSet.DispObj(L_Img, Hwindow);

					HOperatorSet.SetPart(HwindowTwo, Now_Pos[0], Now_Pos[1], Now_Pos[2], Now_Pos[3]);
					HOperatorSet.ClearWindow(HwindowTwo);
					HOperatorSet.DispObj(L_Img, HwindowTwo);

					Now_Pos[0].Dispose();
					Now_Pos[1].Dispose();
					Now_Pos[2].Dispose();
					Now_Pos[3].Dispose();
				}
				else
				{
					ImgIsNotStretchDisplay(L_Img, Hwindow);//不拉伸显示
					ImgIsNotStretchDisplay(L_Img, HwindowTwo);//不拉伸显示
				}
				Zoom.Dispose();
				Row.Dispose(); Col.Dispose(); L_Button.Dispose();
				hv_Width.Dispose(); hv_Height.Dispose();
				Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();

			}
			catch
			{
				Zoom.Dispose();
				Row.Dispose(); Col.Dispose(); L_Button.Dispose();
				hv_Width.Dispose(); hv_Height.Dispose();
				Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();
			}
		}
		public static void ImgZoom(HObject L_Img, HTuple Hwindow, int Delta = 1)
        {
            HTuple Zoom = new HTuple(), Row = new HTuple(), Col = new HTuple(), L_Button = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple Row0 = new HTuple(), Column0 = new HTuple(), Row00 = new HTuple(), Column00 = new HTuple(), Ht = new HTuple(), Wt = new HTuple();
            HTuple[] Now_Pos = new HTuple[4];
            try
            {

                if (Delta > 0)//鼠标滚动格值，一般120
                {
                    Zoom = 1.01;//向上滚动,放大倍数
                }
                else
                {
                    Zoom = 0.99;//向下滚动,缩小倍数
                }
                HOperatorSet.GetMposition(Hwindow, out Row, out Col, out L_Button);//获取当前鼠标的位置
                HOperatorSet.GetPart(Hwindow, out Row0, out Column0, out Row00, out Column00);//获取当前窗体的大小规格
                HOperatorSet.GetImageSize(L_Img, out hv_Width, out hv_Height);//获取图片大小规格
                Ht = Row00 - Row0;
                Wt = Column00 - Column0;
                if (Ht * Wt < 32000 * 32000 || Zoom == 1.2)
                {
                    Now_Pos[0] = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
                    Now_Pos[1] = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
                    Now_Pos[2] = Now_Pos[0] + (Ht / Zoom);
                    Now_Pos[3] = Now_Pos[1] + (Wt / Zoom);
                    HOperatorSet.SetPart(Hwindow, Now_Pos[0], Now_Pos[1], Now_Pos[2], Now_Pos[3]);
                    HOperatorSet.ClearWindow(Hwindow);
                    HOperatorSet.DispObj(L_Img, Hwindow);
                    Now_Pos[0].Dispose();
                    Now_Pos[1].Dispose();
                    Now_Pos[2].Dispose();
                    Now_Pos[3].Dispose();
                }
                else
                {
                    ImgIsNotStretchDisplay(L_Img, Hwindow);//不拉伸显示
                }
                Zoom.Dispose();
                Row.Dispose(); Col.Dispose(); L_Button.Dispose();
                hv_Width.Dispose(); hv_Height.Dispose();
                Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();

            }
            catch
            {
                Zoom.Dispose();
                Row.Dispose(); Col.Dispose(); L_Button.Dispose();
                hv_Width.Dispose(); hv_Height.Dispose();
                Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();
            }
        }

		public static void ImgZoomDouble(HObject L_Img, HTuple Hwindow, int Delta = 1)
		{
			HTuple Zoom = new HTuple(), Row = new HTuple(), Col = new HTuple(), L_Button = new HTuple();
			HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
			HTuple Row0 = new HTuple(), Column0 = new HTuple(), Row00 = new HTuple(), Column00 = new HTuple(), Ht = new HTuple(), Wt = new HTuple();
			HTuple[] Now_Pos = new HTuple[4];
			try
			{

				if (Delta > 0)//鼠标滚动格值，一般120
				{
					Zoom = 1.007;//向上滚动,放大倍数
				}
				else
				{
					Zoom = 0.993;//向下滚动,缩小倍数
				}
				HOperatorSet.GetMposition(Hwindow, out Row, out Col, out L_Button);//获取当前鼠标的位置
				HOperatorSet.GetPart(Hwindow, out Row0, out Column0, out Row00, out Column00);//获取当前窗体的大小规格
				HOperatorSet.GetImageSize(L_Img, out hv_Width, out hv_Height);//获取图片大小规格
				Ht = Row00 - Row0;
				Wt = Column00 - Column0;
				if (Ht * Wt < 32000 * 32000 || Zoom == 1.2)
				{
					Now_Pos[0] = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
					Now_Pos[1] = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
					Now_Pos[2] = Now_Pos[0] + (Ht / Zoom);
					Now_Pos[3] = Now_Pos[1] + (Wt / Zoom);
					HOperatorSet.SetPart(Hwindow, Now_Pos[0], Now_Pos[1], Now_Pos[2], Now_Pos[3]);
					HOperatorSet.ClearWindow(Hwindow);
					HOperatorSet.DispObj(L_Img, Hwindow);
					Now_Pos[0].Dispose();
					Now_Pos[1].Dispose();
					Now_Pos[2].Dispose();
					Now_Pos[3].Dispose();
				}
				else
				{
					ImgIsNotStretchDisplay(L_Img, Hwindow);//不拉伸显示
				}
				Zoom.Dispose();
				Row.Dispose(); Col.Dispose(); L_Button.Dispose();
				hv_Width.Dispose(); hv_Height.Dispose();
				Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();

			}
			catch
			{
				Zoom.Dispose();
				Row.Dispose(); Col.Dispose(); L_Button.Dispose();
				hv_Width.Dispose(); hv_Height.Dispose();
				Row0.Dispose(); Column0.Dispose(); Row00.Dispose(); Column00.Dispose(); Ht.Dispose(); Wt.Dispose();
			}
		}

		public static void ImgIsNotStretchDisplay(HObject L_Img, HTuple Hwindow)
        {
            HTuple hv_Width, hv_Height;
            HTuple win_Width, win_Height, win_Col, win_Row, cwin_Width, cwin_Height;
            HOperatorSet.ClearWindow(Hwindow);
            HOperatorSet.GetImageSize(L_Img, out hv_Width, out hv_Height);//获取图片大小规格
            HOperatorSet.GetWindowExtents(Hwindow, out win_Row, out win_Col, out win_Width, out win_Height);//获取窗体大小规格
            cwin_Height = 1.0 * win_Height / win_Width * hv_Width;//宽不变计算高          
            if (cwin_Height > hv_Height)//宽不变高能容纳
            {
                cwin_Height = 1.0 * (cwin_Height - hv_Height) / 2;
                HOperatorSet.SetPart(Hwindow, -cwin_Height, 0, cwin_Height + hv_Height, hv_Width);//设置窗体的规格
            }
            else//高不变宽能容纳
            {
                cwin_Width = 1.0 * win_Width / win_Height * hv_Height;//高不变计算宽
                cwin_Width = 1.0 * (cwin_Width - hv_Width) / 2;
                HOperatorSet.SetPart(Hwindow, 0, -cwin_Width, hv_Height, cwin_Width + hv_Width);//设置窗体的规格
                cwin_Width.Dispose();
            }
            HOperatorSet.DispObj(L_Img, Hwindow);//显示图片
            hv_Width.Dispose(); hv_Height.Dispose();
            win_Width.Dispose(); win_Height.Dispose(); win_Col.Dispose(); win_Row.Dispose(); cwin_Height.Dispose();
        }
        public static void DetectionShowAOI(int CamNum, HWindow hWindow, out HObject hv_Region)
        {
            HOperatorSet.ReadRegion(out hv_Region, Parameters.commministion.productName + "/halcon/hoRegion" + CamNum + ".tiff");
            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            HOperatorSet.DispObj(hv_Region, hWindow);
        }

        public static void DetectionDrawAOI(HWindow hWindow, out HObject hv_Region)
        {
            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            HOperatorSet.DrawRegion(out hv_Region, hWindow);
            HOperatorSet.DispObj(hv_Region, hWindow);
        }

        public static void DetectionMeanImageint (Parameters.MeanImageEnum meanImageEnum, HObject hObject,ref HObject hObject1)
        {
            switch (meanImageEnum)
            {
                case MeanImageEnum.直方图均值化:
                    {
                        HOperatorSet.EquHistoImage(hObject, out hObject1);
                        break;
                    }

                case MeanImageEnum.增强对比度:
                    {
                        HOperatorSet.Emphasize(hObject, out hObject1, 10, 10, 1.5);
                        break;
                    }
                case MeanImageEnum.均值滤波:
                    {
                        HOperatorSet.MeanImage(hObject, out hObject1, 25, 25);
                        break;
                    }
                case MeanImageEnum.中值滤波:
                    {
                        HOperatorSet.MedianImage(hObject, out hObject1, "square", 1.5, "mirrored");
                        break;
                    }
                case MeanImageEnum.高斯滤波:
                    {
                        HOperatorSet.GaussFilter(hObject, out hObject1, 11);
                        break;
                    }
                case MeanImageEnum.无滤波处理:
                    {
                        HOperatorSet.CopyImage(hObject, out hObject1);
                        break;
                    }                
            }
        }


        public static void DetectionSaveAOI(string pathName,HObject hv_Region)
        {
            HOperatorSet.WriteRegion(hv_Region, pathName);
        }

        public static void DetectionDrawRect2AOI(HWindow hWindow,ref Parameters.Rect2 rect2)
        {
            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            hWindow.DrawRectangle2(out rect2.Row, out rect2.Colum, out rect2.Phi, out rect2.Length1, out rect2.Length2);

        }
        public static void DetectionMeasurePos(HWindow hWindow, HObject hImage, Parameters.Rect2 rect2)
        {
            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            //HOperatorSet.GenMeasureRectangle2(rect2.Row, rect2.Colum, rect2.Phi, rect2.Length1, rect2.Length2);

        }

        public static void DetectionDrawLineAOI(int index, HWindow hWindow, HObject hImage, ref Parameters.DetectionSpec rect1)
        {
            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            hWindow.DrawLine(out rect1.Row1[index], out rect1.Colum1[index], out rect1.Row2[index], out rect1.Colum2[index]);
            HOperatorSet.DispLine(hWindow, rect1.Row1[index], rect1.Colum1[index], rect1.Row2[index], rect1.Colum2[index]);
        }

        /// <summary>
        /// 直线卡尺工具
        /// </summary>
        /// <param name="hWindow"></param>
        /// <param name="hImage"></param>
        /// <param name="rect1"></param>
        /// <param name="PointXY"></param>
        /// <returns></returns>
        public static bool DetectionHalconLine(int CamNum, int BaseNum, HWindow hWindow, HObject hImage, Parameters.DetectionSpec rect1, ref HRect1 PointXY)
        {

            HObject ho_Contours=new HObject() , ho_Cross = new HObject(), ho_Contour = new HObject();

            // Local control variables 

            HTuple hv_shapeParam = new HTuple();
            HTuple hv_MetrologyHandle = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Parameter = new HTuple();

            HTuple hv_Nc = new HTuple(), hv_Dist = new HTuple(), hv_Nr = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                HOperatorSet.SetLineWidth(hWindow, 1);
                //HOperatorSet.DispObj(hImage, hWindow);
                //标记测量位置         
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_shapeParam = new HTuple();
                    hv_shapeParam = hv_shapeParam.TupleConcat(rect1.Row1[BaseNum], rect1.Colum1[BaseNum], rect1.Row2[BaseNum], rect1.Colum2[BaseNum]);
                }
                //创建测量句柄
                HOperatorSet.CreateMetrologyModel(out hv_MetrologyHandle);
                //添加测量对象
                HOperatorSet.SetMetrologyModelImageSize(hv_MetrologyHandle, hv_Width[CamNum], hv_Height[CamNum]);
                hv_Index.Dispose();
                HOperatorSet.AddMetrologyObjectGeneric(hv_MetrologyHandle, "line", hv_shapeParam, rect1.MeasureLength1[BaseNum], rect1.MeasureLength2[BaseNum], rect1.MeasureSigma[BaseNum], rect1.MeasureThreshold[BaseNum], new HTuple(), new HTuple(), out hv_Index);
                HOperatorSet.SetMetrologyObjectParam(hv_MetrologyHandle, "all", "measure_transition", rect1.MeasureTransition[BaseNum]);
                HOperatorSet.SetMetrologyObjectParam(hv_MetrologyHandle, "all", "num_measures", 30);
				HOperatorSet.SetMetrologyObjectParam(hv_MetrologyHandle, "all", "measure_select", rect1.MeasureDirection[BaseNum]);

				//执行测量，获取边缘点集
				HOperatorSet.SetColor(hWindow, "yellow");
                HOperatorSet.ApplyMetrologyModel(hImage, hv_MetrologyHandle);
                hv_Row.Dispose(); hv_Column.Dispose();
                HOperatorSet.GetMetrologyObjectMeasures(out ho_Contours, hv_MetrologyHandle, "all", "all", out hv_Row, out hv_Column);
                HOperatorSet.DispObj(ho_Contours, hWindow);
                HOperatorSet.SetColor(hWindow, "red");
                HOperatorSet.GenCrossContourXld(out ho_Cross, hv_Row, hv_Column, 1, 0.785398);
                //获取最终测量数据和轮廓线
                HOperatorSet.SetColor(hWindow, "green");
                HOperatorSet.SetLineWidth(hWindow, 1);
                hv_Parameter.Dispose();
                HOperatorSet.GetMetrologyObjectResult(hv_MetrologyHandle, "all", "all", "result_type", "all_param", out hv_Parameter);
                HOperatorSet.GetMetrologyObjectResultContour(out ho_Contour, hv_MetrologyHandle, "all", "all", 15);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out PointXY.Row1, out PointXY.Colum1, out PointXY.Row2, out PointXY.Colum2, out hv_Nr, out hv_Nc, out hv_Dist);

                HOperatorSet.DispObj(ho_Cross, hWindow);
                HOperatorSet.SetColor(hWindow, "red");
                HOperatorSet.DispObj(ho_Contour, hWindow);
                //释放测量句柄
                HOperatorSet.ClearMetrologyModel(hv_MetrologyHandle);
                ho_Contours.Dispose();
                ho_Cross.Dispose();
                ho_Contour.Dispose();
                hv_shapeParam.Dispose();
                hv_MetrologyHandle.Dispose();
                hv_Index.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Parameter.Dispose();


                return true;
            }
            catch
            {
                ho_Contours.Dispose();
                ho_Cross.Dispose();
                ho_Contour.Dispose();
                hv_shapeParam.Dispose();
                hv_MetrologyHandle.Dispose();
                hv_Index.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Parameter.Dispose();


                return false;
            }
            
            
        }
		public static string HalconKind(int KindSelect)
		{
			string sKindSelect;
			if (KindSelect % 7 == 0)
			{
				sKindSelect = "气泡";
			}
			else if (KindSelect % 7 == 1)
			{
				sKindSelect = "黑点";
			}
			else if (KindSelect % 7 == 2)
			{
				sKindSelect = "异物";
			}
			else if (KindSelect % 7 == 3)
			{
				sKindSelect = "裂纹";
			}
			else if (KindSelect % 7 == 4)
			{
				sKindSelect = "缺口";
			}
			else if (KindSelect % 7 == 5)
			{
				sKindSelect = "刮伤";
			}
			else
			{
				sKindSelect = "其他";
			}


			return sKindSelect;
		}
		public static void HalconColorSelect(int ColorSelect, HWindow hWindow)
		{
			if (ColorSelect % 7 == 0)
			{
				HOperatorSet.SetColor(hWindow, "blue");
			}
			else if (ColorSelect % 7 == 1)
			{
				HOperatorSet.SetColor(hWindow, "white");
			}
			else if (ColorSelect % 7 == 2)
			{
				HOperatorSet.SetColor(hWindow, "black");
			}
			else if (ColorSelect % 7 == 3)
			{
				HOperatorSet.SetColor(hWindow, "magenta");
			}
			else if (ColorSelect % 7 == 4)
			{
				HOperatorSet.SetColor(hWindow, "orange");
			}
			else if (ColorSelect % 7 == 5)
			{
				HOperatorSet.SetColor(hWindow, "gray");
			}
			else
			{
				HOperatorSet.SetColor(hWindow, "orange");
			}
		}

		public static bool DetectionHalconRegion(int CamNum, int BaseNum, HWindow[] hWindow, HObject hImage, Parameters.DetectionSpec spec , HObject hObject ,ref List<DetectionResult>  detectionResult, bool bProductReport)
        {


            if(Parameters.detectionSpec[CamNum].DefectDetection[BaseNum]==false)
            {
				return true;
			}

            if(Parameters.detectionSpec[CamNum].ThresholdLow[BaseNum] >=Parameters.detectionSpec[CamNum].ThresholdHigh[BaseNum]
                || Parameters.detectionSpec[CamNum].AreaLow[BaseNum]>=Parameters.detectionSpec[CamNum].AreaHigh[BaseNum]
                || Parameters.detectionSpec[CamNum].ThresholdLow[BaseNum]>255|| Parameters.detectionSpec[CamNum].ThresholdLow[BaseNum]<0
                || Parameters.detectionSpec[CamNum].ThresholdHigh[BaseNum]>255|| Parameters.detectionSpec[CamNum].ThresholdHigh[BaseNum]<0
                || Parameters.detectionSpec[CamNum].AreaLow[BaseNum]<0|| Parameters.detectionSpec[CamNum].AreaHigh[BaseNum]<0
				)
            {
				MessageBox.Show("相机"+CamNum.ToString()+"缺陷检测"+ BaseNum.ToString() + "参数设定有误，请检查");
				LogHelper.WriteWarn("相机" + CamNum.ToString() + "缺陷检测" + BaseNum.ToString() + "参数设定有误，请检查");
				return false;
            }

			// Local iconic variables             
			HObject ho_ImageReduced, ho_Region, ho_ConnectedRegions, ho_ConnectedRegionsed;
			HObject ho_SelectedRegions1, ho_ObjectSelected, ho_SelectedRegions;
			HObject ho_Rectangle;

			// Local control variables 

			HTuple hv_Area = new HTuple(), hv_Area1 = new HTuple(), hv_Area2 = new HTuple(), hv_Row = new HTuple();
			HTuple hv_Column = new HTuple(), hv_Indices = new HTuple();
			HTuple hv_Length = new HTuple(), hvLength = new HTuple();
			HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
			HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
			HTuple hv_Row12 = new HTuple(), hv_Column12 = new HTuple();
			
			// Initialize local and output iconic variables 
			try
            {
				HOperatorSet.GenEmptyObj(out ho_ImageReduced);
				HOperatorSet.GenEmptyObj(out ho_Region);
				HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
				HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
				HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
				HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
				HOperatorSet.GenEmptyObj(out ho_Rectangle);
				HOperatorSet.GenEmptyObj(out ho_ConnectedRegionsed);
			}
            catch
            {
				LogHelper.WriteWarn(CamNum.ToString()+ BaseNum.ToString()+"检测退出，请检查");
				return false;
            }

            try
            {
				//读取测试图
				//阈值分割图像
				HOperatorSet.SetDraw(hWindow[0], "margin");
				HOperatorSet.SetColor(hWindow[0], "green");
				HOperatorSet.DispObj(hObject, hWindow[0]);
				HOperatorSet.ReduceDomain(hImage, hObject, out ho_ImageReduced);

				ho_Region.Dispose();
				HOperatorSet.Threshold(ho_ImageReduced, out ho_Region, spec.ThresholdLow[BaseNum], spec.ThresholdHigh[BaseNum]);
				ho_ConnectedRegions.Dispose();
				HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
				ho_SelectedRegions1.Dispose();
				HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions1, "area", "and", spec.AreaLow[BaseNum] / (Parameters.detectionSpec[CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum), spec.AreaHigh[BaseNum] / (Parameters.detectionSpec[MainForm.CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum));
				hv_Area.Dispose(); hv_Row.Dispose(); hv_Column.Dispose();
				HOperatorSet.AreaCenter(ho_SelectedRegions1, out hv_Area, out hv_Row, out hv_Column);
				//对元组的元素进行排序并返回排序后的元组的索引
				hv_Indices.Dispose();
				HOperatorSet.TupleSortIndex(hv_Area, out hv_Indices);
                HOperatorSet.TupleLength( hv_Area,out hvLength);
			}
            catch
            {
				return false;
			}

			//计算元组长度
			
			if (hvLength == 0)
			{
    //            try
    //            {
				//	DetectionResult detectionResul = new DetectionResult();
    //                if(bProductReport)
    //                {
				//		MainForm.ProductReport(CamNum.ToString(), detectionResul, true, BaseNum);
				//	}
				//	return true;
				//}
    //            catch
    //            {
					return true;
				//}				
			}
			else if (hvLength == 1)
			{
				HOperatorSet.SetColor(hWindow[0], "red");
				HOperatorSet.DispObj(ho_SelectedRegions1, hWindow[0]);
				HOperatorSet.SmallestRectangle1(ho_SelectedRegions1, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
				HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2, hv_Column2);
				HalconColorSelect(BaseNum, hWindow[0]);

				//LogHelper.WriteSingal("框选坐标hv_Row1:" + hv_Row1.ToString() + "  hv_Column1：" + hv_Column1.ToString());

				HOperatorSet.DispObj(ho_Rectangle, hWindow[0]);
				HTuple Mean = new HTuple();
				DetectionResult detectionResult1 = new DetectionResult();
				HTuple hv_Value = new HTuple();
				HOperatorSet.RegionFeatures(ho_SelectedRegions1, new HTuple("area").TupleConcat("row").TupleConcat("column").TupleConcat("width").TupleConcat("height").TupleConcat("ra").TupleConcat("rb"), out hv_Value);
				HOperatorSet.GrayFeatures(ho_SelectedRegions1, hImage, "mean", out Mean);
				detectionResult1.ResultdateTime = DateTime.Now;
				detectionResult1.ResultGray = Mean.D;
				detectionResult1.ResultLevel = BaseNum & 7 + 1;
				detectionResult1.ResultKind = (ImageErrorKind)(BaseNum%7);
				detectionResult1.ResultSize = hv_Value.TupleSelect(0).D * Parameters.detectionSpec[CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum;
				detectionResult1.ResultYPosition = hv_Value.TupleSelect(1).D * Parameters.detectionSpec[CamNum].PixelResolutionRow - Parameters.detectionSpec[CamNum].RowBase[0] + Parameters.detectionSpec[CamNum].RowBase[1];
				detectionResult1.ResultXPosition = hv_Value.TupleSelect(2).D * Parameters.detectionSpec[CamNum].PixelResolutionColum - Parameters.detectionSpec[CamNum].ColumBase[0] + Parameters.detectionSpec[CamNum].ColumBase[1];
				detectionResult1.ResultWidth = hv_Value.TupleSelect(3).D * Parameters.detectionSpec[CamNum].PixelResolutionColum;
				detectionResult1.ResultHeight = hv_Value.TupleSelect(4).D * Parameters.detectionSpec[CamNum].PixelResolutionRow;
				detectionResult1.ResultRa = hv_Value.TupleSelect(5).D;
				detectionResult1.ResultRb = hv_Value.TupleSelect(6).D;

				hv_Row = hv_Value.TupleSelect(1);
				hv_Column = hv_Value.TupleSelect(2);
				if (hv_Value.TupleSelect(1) < 500)
				{
					hv_Row = 500;
				}
				else if (hv_Value.TupleSelect(1) > hv_Height[CamNum] - 500)
				{
					hv_Row = hv_Height[CamNum] - 500;
				}
				if (hv_Value.TupleSelect(2) < 500)
				{
					hv_Column = 500;
				}
				else if (hv_Value.TupleSelect(2) > hv_Width[CamNum] - 500)
				{
					hv_Column = hv_Width[CamNum] - 500;
				}

				//if (hv_Row < 500)
				//{
				//	hv_Row = 500;
				//}
				//else if (hv_Row > hv_Height[CamNum] - 500)
				//{
				//	hv_Row = hv_Height[CamNum] - 500;
				//}
				//if (hv_Column < 500)
				//{
				//	hv_Column = 500;
				//}
				//else if (hv_Column > hv_Width[CamNum] - 500)
				//{
				//	hv_Column = hv_Width[CamNum] - 500;
				//}
				HOperatorSet.CropPart(hImage, out detectionResult1.NGAreahObject, hv_Row - 500, hv_Column - 500, 1000, 1000);
				detectionResult.Add(detectionResult1);
				//HOperatorSet.SetColor(hWindow[0], "red");
				HalconColorSelect(BaseNum, hWindow[0]);
				HOperatorSet.SetTposition(hWindow[0], hv_Row, hv_Column);
			    HOperatorSet.WriteString(hWindow[0], "Area:" + (hv_Area.D * Parameters.detectionSpec[CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum).ToString("0.00"));
				
                HOperatorSet.SetTposition(hWindow[0], hv_Row + 500, hv_Column);
				string Kind = HalconKind(BaseNum);
				HOperatorSet.WriteString(hWindow[0], "缺陷：" + Kind);

                try
                {
					if (bProductReport)
                    {
						MainForm.ProductReport(CamNum.ToString(), detectionResult1, false, BaseNum);
					}						
				}
				catch
                {

                }

				hv_Value.Dispose();
				Mean.Dispose();
			}
			else
			{
				HOperatorSet.SetColor(hWindow[0], "red");
				HOperatorSet.DispObj(ho_SelectedRegions1, hWindow[0]);
				
                int iAreaNum = 0;
                if (hv_Area.Length > 20)
                {
                    iAreaNum = 20;
                }
                else
                {
                    iAreaNum = hv_Area.Length;
                }

                //借助形状特征选择区域，这里选取前5
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
				{
					ho_ObjectSelected.Dispose();
					HOperatorSet.SelectObj(ho_SelectedRegions1, out ho_ObjectSelected, hv_Indices[hvLength - iAreaNum] +1);
                    hv_Area1.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                    HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_Area1, out hv_Row1, out hv_Column1);
                    HOperatorSet.SelectShape(ho_SelectedRegions1, out ho_SelectedRegions,"area","and", hv_Area1, hv_Area.TupleMax());
                    hv_Area1.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                    HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area1, out hv_Row1, out hv_Column1);
                }
                HOperatorSet.CountObj(ho_SelectedRegions, out hvLength);
                for (int i = 0; i < iAreaNum; i++)
				{
                    HTuple Mean = new HTuple();
                    DetectionResult[] detectionResult1 = new DetectionResult[iAreaNum];
					HTuple hv_Value = new HTuple();
                    HOperatorSet.SelectObj(ho_SelectedRegions, out ho_ObjectSelected, i + 1);
                    HOperatorSet.RegionFeatures(ho_ObjectSelected, new HTuple("area").TupleConcat("row").TupleConcat("column").TupleConcat("width").TupleConcat("height").TupleConcat("ra").TupleConcat("rb"), out hv_Value);
                    HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                    HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2, hv_Column2);

					//LogHelper.WriteSingal(i.ToString()+"  框选坐标hv_Row1:" + hv_Row1.ToString() + "  hv_Column1：" + hv_Column1.ToString());

					HalconColorSelect(BaseNum, hWindow[0]);
                    HOperatorSet.SetTposition(hWindow[0], hv_Row1, hv_Column1);
                    HOperatorSet.WriteString(hWindow[0], "Area:" + (hv_Value.TupleSelect(0).D * Parameters.detectionSpec[CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum).ToString("0.00"));
                    HOperatorSet.SetTposition(hWindow[0], hv_Row1 + 500, hv_Column1);
                    string Kind = HalconKind(BaseNum);
                    HOperatorSet.WriteString(hWindow[0], "缺陷：" + Kind);

					//LogHelper.WriteSingal(i.ToString() + "   显示坐标hv_Row1:" + hv_Row1.ToString() + "  hv_Column1：" + hv_Column1.ToString());

					//HOperatorSet.SetColor(hWindow[0], "blue");
					HalconColorSelect(BaseNum, hWindow[0]);
                    HOperatorSet.DispObj(ho_Rectangle, hWindow[0]);
                    HOperatorSet.GrayFeatures(ho_ObjectSelected, hImage, "mean", out Mean);
					detectionResult1[i].ResultdateTime = DateTime.Now;
					detectionResult1[i].ResultGray = Mean.D;
					detectionResult1[i].ResultLevel = BaseNum % 7 + 1;
					detectionResult1[i].ResultKind = (ImageErrorKind)(BaseNum%7);
					detectionResult1[i].ResultSize = hv_Value.TupleSelect(0 ).D * Parameters.detectionSpec[CamNum].PixelResolutionRow * Parameters.detectionSpec[CamNum].PixelResolutionColum;
					detectionResult1[i].ResultYPosition = hv_Value.TupleSelect(1 ).D * Parameters.detectionSpec[CamNum].PixelResolutionRow - Parameters.detectionSpec[CamNum].RowBase[0] + Parameters.detectionSpec[CamNum].RowBase[1];
					detectionResult1[i].ResultXPosition = hv_Value.TupleSelect(2 ).D * Parameters.detectionSpec[CamNum].PixelResolutionColum - Parameters.detectionSpec[CamNum].ColumBase[0] + Parameters.detectionSpec[CamNum].ColumBase[1];
					detectionResult1[i].ResultWidth = hv_Value.TupleSelect(3 ).D * Parameters.detectionSpec[CamNum].PixelResolutionColum;
					detectionResult1[i].ResultHeight = hv_Value.TupleSelect(4 ).D * Parameters.detectionSpec[CamNum].PixelResolutionRow;
					detectionResult1[i].ResultRa = hv_Value.TupleSelect(5 ).D;
					detectionResult1[i].ResultRb = hv_Value.TupleSelect(6 ).D;
                    hv_Row12 = hv_Value.TupleSelect(1);
                    hv_Column12= hv_Value.TupleSelect(2);
                    if (hv_Value.TupleSelect(1) < 500)
					{
						hv_Row12 = 500;
					}
					else if (hv_Value.TupleSelect(1) > hv_Height[CamNum] - 500)
					{
						hv_Row12 = hv_Height[CamNum] - 500;
					}
					if (hv_Value.TupleSelect(2) < 500)
					{
						hv_Column12 = 500;
					}
					else if (hv_Value.TupleSelect(2) > hv_Width[CamNum] - 500)
					{
						hv_Column12 = hv_Width[CamNum] - 500;
					}
					HOperatorSet.CropPart(hImage, out detectionResult1[i].NGAreahObject, hv_Row12 - 500, hv_Column12 - 500, 1000, 1000);

					//LogHelper.WriteSingal(i.ToString() + "   截图坐标hv_Row12:" + hv_Row12.ToString() + "  hv_Column12：" + hv_Column12.ToString());


					string stfFileNameOut = MainForm.productSheetNum+"-NG-"+i.ToString();  // 默认的图像保存名称
					string pathOut = Parameters.commministion.ImageSavePath + "\\" + MainForm.strDateTimeDay + "\\" + MainForm.productSN + "\\"+ MainForm.productSheetNum+"\\"+"CAM-"+ CamNum + "\\"+"NG-"+"\\";
					if (!System.IO.Directory.Exists(pathOut))
					{
						System.IO.Directory.CreateDirectory(pathOut);//不存在就创建文件夹
					}
					HOperatorSet.WriteImage(detectionResult1[i].NGAreahObject, "jpeg", 0, pathOut + stfFileNameOut + ".jpeg");
					detectionResult.Add(detectionResult1[i]);

                    try
                    {
						if (bProductReport)
                        {
							MainForm.ProductReport(CamNum.ToString(), detectionResult1[i], false, BaseNum);
						}							
					}
                    catch
                    {

                    }
                   

					hv_Value.Dispose();
					Mean.Dispose();
				}
			}
			//储存进元组
			hv_Length.Dispose();
			ho_ImageReduced.Dispose();
			ho_Region.Dispose();
			ho_ConnectedRegions.Dispose();
			ho_SelectedRegions1.Dispose();
			ho_ObjectSelected.Dispose();
			ho_SelectedRegions.Dispose();
			ho_Rectangle.Dispose();

			hv_Area.Dispose();
			hv_Area1.Dispose();
			hv_Row.Dispose();
			hv_Column.Dispose();
			hv_Indices.Dispose();
			hv_Length.Dispose();
			hv_Row12.Dispose();
			hv_Column12.Dispose();
			hv_Row1.Dispose();
			hv_Column1.Dispose();
			hv_Row2.Dispose();
			hv_Column2.Dispose();
			return true;
		}


        public static bool DetectionHalconSlelct(HWindow hWindow, HObject hImage, Parameters.Rect1 rect1, Parameters.DetectionSpec spec, ref rent2[] result)
        {
            HObject ho_Rectangle, ho_ImageReduced;
            HObject ho_Regions,  ho_ConnectedRegions ;
            HObject ho_SelectedRegions ;

            // Local control variables 

            HTuple hv_WindowHandle = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Area = new HTuple();
            HTuple hv_Number = new HTuple(), hv_Index = new HTuple();

            // Initialize local and output iconic variables 

            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            //dev_open_window(...);

            HOperatorSet.SetColor(hWindow, "green");
            HOperatorSet.SetDraw(hWindow, "margin");
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle, rect1.Row1, rect1.Colum1, rect1.Row2, rect1.Colum2);
            HOperatorSet.DispObj(ho_Rectangle, hWindow);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(hImage, ho_Rectangle, out ho_ImageReduced);
            ho_Regions.Dispose();           
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Regions, spec.ThresholdLow, spec.ThresholdHigh);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            
            HOperatorSet.SetColor(hWindow, "red");
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area", "and", spec.AreaLow, spec.AreaHigh);
            HOperatorSet.DispObj(ho_SelectedRegions, hWindow);

            hv_Area.Dispose(); hv_Row.Dispose(); hv_Column.Dispose();
            HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row, out hv_Column);
            hv_Number.Dispose();
            HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
            result = new rent2[hv_Number];
            HTuple end_val13 = hv_Number - 1;
            HTuple step_val13 = 1;
            for (hv_Index = 0; hv_Index.Continue(end_val13, step_val13); hv_Index = hv_Index.TupleAdd(step_val13))
            {
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetTposition(hWindow, hv_Row.TupleSelect(hv_Index), hv_Column.TupleSelect(hv_Index));
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.WriteString(hWindow, hv_Area.TupleSelect(hv_Index));
                    result[hv_Index].Row = hv_Row.TupleSelect(hv_Index);
                    result[hv_Index].Column = hv_Column.TupleSelect(hv_Index);
                }
            }

            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Regions.Dispose();           
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            hv_WindowHandle.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Area.Dispose();
            hv_Number.Dispose();
            hv_Index.Dispose();

            return true;
        }

        public static HRect1[] pointReaultLine = new HRect1[3];
        public static Rect2 pointReaultRect = new Rect2();


        public static rent2[] rect2Match;
        public static bool DetectionHalconMatch(HWindow hv_ExpDefaultWinHandle, HObject ho_Image, Parameters.Rect1 rect1, Parameters.Rect1 rect, ref rent2[] rent2)
        {
            HObject ho_Rectangle, ho_Rectangle1,ho_ImageReduced,ho_ImageReduced1;
            HObject ho_ModelImages, ho_ModelRegions, ho_ModelContours;
            HObject ho_ContoursAffineTrans = null, ho_Region = null;

            // Local control variables 

            HTuple hv_WindowHandle = new HTuple(), hv_Row11 = new HTuple();
            HTuple hv_Column11 = new HTuple(), hv_Row21 = new HTuple();
            HTuple hv_Column21 = new HTuple(), hv_ModelID = new HTuple();
            HTuple hv_ModelID1 = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Angle = new HTuple();
            HTuple hv_Scale = new HTuple(), hv_Score = new HTuple();
            HTuple hv_i = new HTuple(), hv_HomMat2DRotate = new HTuple();
            HTuple hv_HomMat2DScale = new HTuple(), hv_Row1 = new HTuple();
            HTuple hv_Column1 = new HTuple(), hv_Row2 = new HTuple();
            HTuple hv_Column2 = new HTuple();
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_ModelImages);
            HOperatorSet.GenEmptyObj(out ho_ModelRegions);
            HOperatorSet.GenEmptyObj(out ho_ModelContours);
            HOperatorSet.GenEmptyObj(out ho_ContoursAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_Region);

            //******定位模板********
            //以合适的尺寸打开图像
            hv_WindowHandle.Dispose();
            //设置画图填充方式
            HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, "margin");
            //设置颜色
            HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "red");
            //设置线宽
            HOperatorSet.SetLineWidth(hv_ExpDefaultWinHandle, 1);
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle1, rect1.Row1, rect1.Colum1, rect1.Row2, rect1.Colum2);
            HOperatorSet.DispObj(ho_Rectangle1, hv_ExpDefaultWinHandle);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle1, out ho_ImageReduced1);
            //阈值化
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle, rect.Row1, rect.Colum1, rect.Row2, rect.Colum2);

            //裁剪目标区域图像
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced1, ho_Rectangle, out ho_ImageReduced);

            //******生成模板********
            //监视模板
            ho_ModelImages.Dispose(); ho_ModelRegions.Dispose();
            HOperatorSet.InspectShapeModel(ho_ImageReduced, out ho_ModelImages, out ho_ModelRegions,1, 40);

            //创建形状模板
            //使用用图像创建带有缩放的匹配模板
            //NumLevels 最高金子塔层数
            //AngleStart 开始角度加rad(90)是将弧度制转为角度值
            //AngleExtent 角度范围
            //AngleStep 旋转角度步长
            //ScaleMin 模板行方向缩放最小尺度
            //ScaleMax 模板行方向缩放最大尺寸
            //MinScore 最低匹配分值 百分比
            //ScaleStep 步长
            //Optimization 优化选项 是否减少模板点数
            //Metric 匹配度量级性旋转
            //MinContrast 最小对比度
            //ModelID 生成模板ID
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_ModelID.Dispose();
                HOperatorSet.CreateScaledShapeModel(ho_ImageReduced, 5, (new HTuple(-180)).TupleRad(), (new HTuple(180)).TupleRad(), 0, 0.9, 1.0, 0, "auto", "ignore_global_polarity", 40, 10, out hv_ModelID);
            }


            //保存模板
            HOperatorSet.WriteShapeModel(hv_ModelID, Parameters.commministion.productName + "/halcon/匹配模板.shm");

            //清除模板
            HOperatorSet.ClearShapeModel(hv_ModelID);


            //******模板匹配********


            //读取形状模板
            hv_ModelID1.Dispose();
            HOperatorSet.ReadShapeModel(Parameters.commministion.productName + "/halcon/匹配模板.shm", out hv_ModelID1);

            //模板匹配
            //寻找单个带尺度形状模板最佳匹配
            //ImageRectifiedFixed 要搜索的图像
            //ModelID 模板ID
            //AngleStart 开始角度加rad(90)是将弧度制转为角度值
            //AngleExtent 角度范围
            //ScaleMin 模板行方向缩放最小尺度
            //ScaleMax 模板行方向缩放最大尺寸
            //MinScore 最低匹配分值 百分比
            //NumMatches 匹配实例的个数； 0时，全部检测
            //MaxOverlap 最大重叠 在有重叠时也可检测匹配
            //SubPixel 是否亚像素精度
            //NumLevels 金子塔层数
            //Greediness 搜索贪婪度； 0安全慢；1块不稳定；其他就是介于中间值
            //剩下的几个参数是匹配图像的位置状态等参数
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Row.Dispose(); hv_Column.Dispose(); hv_Angle.Dispose(); hv_Scale.Dispose(); hv_Score.Dispose();
                HOperatorSet.FindScaledShapeModel(ho_ImageReduced1, hv_ModelID1, (new HTuple(-180)).TupleRad()
                    , (new HTuple(180)).TupleRad(), 1, 1.0, 0.8, 10, 0, "least_squares", 5, 0.1,
                    out hv_Row, out hv_Column, out hv_Angle, out hv_Scale, out hv_Score);
            }

            //返回一个轮廓模型的轮廓表示
            ho_ModelContours.Dispose();
            HOperatorSet.GetShapeModelContours(out ho_ModelContours, hv_ModelID1, 1);

            HOperatorSet.DispObj(ho_Image, hv_ExpDefaultWinHandle);
            int index = (int)new HTuple(hv_Score.TupleLength());
            rect2Match = new rent2[index];
            //循环
            for (hv_i = 0; (int)hv_i < index; hv_i++)
            {
                //得到对应匹配目标的旋转矩阵
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_HomMat2DRotate.Dispose();
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_Row.TupleSelect(hv_i), hv_Column.TupleSelect(hv_i), hv_Angle.TupleSelect(hv_i), out hv_HomMat2DRotate);
                    rect2Match[hv_i].Row = hv_Row.TupleSelect(hv_i);
                    rect2Match[hv_i].Column = hv_Column.TupleSelect(hv_i);
                    rect2Match[hv_i].Phi = hv_Angle.TupleSelect(hv_i);
                    HOperatorSet.SetTposition(hv_ExpDefaultWinHandle, hv_Row.TupleSelect(hv_i), hv_Column.TupleSelect(hv_i));
                    HOperatorSet.WriteString(hv_ExpDefaultWinHandle, "Phi=" + rect2Match[hv_i].Phi.ToString("0.000"));
                }
                //在旋转矩阵的基础上添加缩放量，生成新的矩阵
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_HomMat2DScale.Dispose();
                    HOperatorSet.HomMat2dScale(hv_HomMat2DRotate, hv_Scale.TupleSelect(hv_i), hv_Scale.TupleSelect(
                        hv_i), hv_Row.TupleSelect(hv_i), hv_Column.TupleSelect(hv_i), out hv_HomMat2DScale);
                }
                //矩阵变换（xld基础上）
                ho_ContoursAffineTrans.Dispose();
                HOperatorSet.AffineTransContourXld(ho_ModelContours, out ho_ContoursAffineTrans,
                    hv_HomMat2DScale);
                //xld转换成region
                ho_Region.Dispose();
                HOperatorSet.GenRegionContourXld(ho_ContoursAffineTrans, out ho_Region, "filled");
                //对区域，生成最小外接矩形
                hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
                HOperatorSet.SmallestRectangle1(ho_Region, out hv_Row1, out hv_Column1, out hv_Row2,
                    out hv_Column2);
                //使用绿色矩形框，框选匹配结果
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "green");
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.DispRectangle1(hv_ExpDefaultWinHandle, hv_Row1.TupleSelect(0),
                        hv_Column1.TupleSelect(0), hv_Row2.TupleSelect(0), hv_Column2.TupleSelect(
                        0));
                }
                //使用红色勾画匹配结果轮廓
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "red");
                HOperatorSet.DispObj(ho_ContoursAffineTrans, hv_ExpDefaultWinHandle);

            }
            ho_Rectangle.Dispose();
            ho_Rectangle1.Dispose();
            ho_ImageReduced.Dispose();
            ho_ModelImages.Dispose();
            ho_ModelRegions.Dispose();
            ho_ModelContours.Dispose();
            ho_ContoursAffineTrans.Dispose();
            ho_Region.Dispose();

            hv_WindowHandle.Dispose();
            hv_Row11.Dispose();
            hv_Column11.Dispose();
            hv_Row21.Dispose();
            hv_Column21.Dispose();
            hv_ModelID.Dispose();
            hv_ModelID1.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Angle.Dispose();
            hv_Scale.Dispose();
            hv_Score.Dispose();
            hv_i.Dispose();
            hv_HomMat2DRotate.Dispose();
            hv_HomMat2DScale.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
            hv_Row2.Dispose();
            hv_Column2.Dispose();

            return true;
        }
	}
}


