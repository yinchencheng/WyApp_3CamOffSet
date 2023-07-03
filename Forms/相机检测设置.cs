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
using static WY_App.Utility.Parameters;
using WY_App.UserControls;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
using System.Data.SqlTypes;
using OpenCvSharp.Flann;
using Sunny.UI;

namespace WY_App
{
    public partial class 相机检测设置 : Form
    {
        static HWindow[] hWindows;       
        HWindow hWindow = new HWindow();
        static HRect1[,] BaseReault = new HRect1[3,4];
        public 相机检测设置()
        {
            InitializeComponent();
            hWindow = hWindowControl4.HalconWindow;
            hWindows = new HWindow[3] { hWindowControl1.HalconWindow, hWindowControl2.HalconWindow, hWindowControl3.HalconWindow };
            HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows[1], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.SetPart(hWindows[2], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[1]);
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[2]);
            HOperatorSet.SetPart(hWindow, 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
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

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        

        private void btn_DrawXbase_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
            Halcon.DetectionDrawLineAOI(0,hWindow, MainForm.hImage[MainForm.CamNum],ref Parameters.detectionSpec[MainForm.CamNum]);
        }

        private void 相机1检测设置_Load(object sender, EventArgs e)
        {
            HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格            
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            HOperatorSet.SetPart(hWindow, 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
            uiComboBox1.SelectedIndex = 0;
            cmb_Indication.SelectedIndex = 0;
            chk_MeanImageEnabled.Checked = Parameters.specifications.MeanImageEnabled;
            cmb_MeanImageList.DataSource = Enum.GetNames(typeof(Parameters.MeanImageEnum));
            cmb_MeanImageList.SelectedIndex = Parameters.specifications.meanImageEnum;
            cmb_Indication.DataSource = Enum.GetNames(typeof(Parameters.ImageErrorKind));
            chk_SaveOrigalImage.Checked = Parameters.specifications.SaveOrigalImage;
            chk_SaveDefeatImage.Checked = Parameters.specifications.SaveDefeatImage;
            chk_SaveCropImage.Checked = Parameters.specifications.SaveCropImage;
            txt_PixelResolutionRow.Text = Parameters.detectionSpec[MainForm.CamNum].PixelResolutionRow.ToString();
            txt_PixelResolutionColumn.Text = Parameters.detectionSpec[MainForm.CamNum].PixelResolutionColum.ToString();
            uiDoubleUpDown1.Value = Parameters.detectionSpec[MainForm.CamNum].RowBase[1];
            uiDoubleUpDown2.Value = Parameters.detectionSpec[MainForm.CamNum].ColumBase[1];
        }

        private void btn_OpenTestimage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();

            if (openfile.ShowDialog() == DialogResult.OK && (openfile.FileName != ""))
            {
                //HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格
                //HOperatorSet.SetPart(hWindow, 0, 0, -1, -1);//设置窗体的规格
                Halcon.ImgDisplay(MainForm.CamNum,openfile.FileName, hWindows[0], hWindow); 
            }
            openfile.Dispose();
        }

        private void btn_DrawY1base_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
            Halcon.DetectionDrawLineAOI(1,hWindow, MainForm.hImage[MainForm.CamNum], ref Parameters.detectionSpec[MainForm.CamNum]);
        }

        private void btn_DrawY2base_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
            Halcon.DetectionDrawLineAOI(2, hWindow, MainForm.hImage[MainForm.CamNum], ref Parameters.detectionSpec[MainForm.CamNum]);
            
        }
        public static void showXBase(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            Halcon.DetectionHalconLine(0, 0, hWindows[0], MainForm.hImage[MainForm.CamNum], Parameters.detectionSpec[MainForm.CamNum], ref BaseReault[MainForm.CamNum, 0]);
        }

        public static void showY1Base(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            Halcon.DetectionHalconLine(MainForm.CamNum,1, hWindows[0], MainForm.hImage[MainForm.CamNum], Parameters.detectionSpec[MainForm.CamNum], ref BaseReault[MainForm.CamNum,1]);
            BaseReault[MainForm.CamNum, 1].Row1 = BaseReault[MainForm.CamNum, 1].Row1 + 50;
            BaseReault[MainForm.CamNum, 1].Row2 = BaseReault[MainForm.CamNum, 1].Row2 + 50;
        }

        public static void showY2Base(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            bool resuult = Halcon.DetectionHalconLine(MainForm.CamNum,2, hWindows[0], MainForm.hImage[MainForm.CamNum], Parameters.detectionSpec[MainForm.CamNum], ref BaseReault[MainForm.CamNum,2]);
            //if(resuult)
            //{
            //    Parameters.detectionSpec[MainForm.CamNum].Row1[3] = BaseReault[MainForm.CamNum, 2].Row1 + 300;
            //    Parameters.detectionSpec[MainForm.CamNum].Colum1[3] = BaseReault[MainForm.CamNum, 2].Colum1;
            //    Parameters.detectionSpec[MainForm.CamNum].Row2[3] = BaseReault[MainForm.CamNum, 2].Row2 + 300;
            //    Parameters.detectionSpec[MainForm.CamNum].Colum2[3] = BaseReault[MainForm.CamNum, 2].Colum2;
            //    Halcon.DetectionHalconLine(MainForm.CamNum, 3, hWindows[0], MainForm.hImage[MainForm.CamNum], Parameters.detectionSpec[MainForm.CamNum], ref BaseReault[MainForm.CamNum, 3]);
            //}
            
        }
        List<DetectionResult> detectionResults;
        private void btn_Detection_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间
            DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
            MainForm.strDateTime = dtNow.ToString("HHmmss");
            MainForm.strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
            detectionResults = new List<DetectionResult>();
            Detection(MainForm.CamNum, hWindows, MainForm.hImage[MainForm.CamNum], ref detectionResults);
            this.Invoke((EventHandler)delegate
            {
                for (int i = 0; i < 2; i++)
                {
                    hWindows[i + 1].ClearWindow();
                    HOperatorSet.SetPart(hWindows[i + 1], 0, 0, 1000, 1000);//设置窗体的规格
                }
                messageShow4.lab_Timer.Text = "";
                messageShow4.lab_Column.Text = "";
                messageShow4.lab_Row.Text = "";
                messageShow4.lab_Size.Text = "";
                messageShow4.lab_Kind.Text = "";
                messageShow4.lab_Level.Text = "";
                messageShow4.lab_Gray.Text = "";
                messageShow3.lab_Timer.Text = "";
                messageShow3.lab_Column.Text = "";
                messageShow3.lab_Row.Text = "";
                messageShow3.lab_Size.Text = "";
                messageShow3.lab_Kind.Text = "";
                messageShow3.lab_Level.Text = "";
                messageShow3.lab_Gray.Text = "";
                if (detectionResults.Count != 0)
                {
                    hWindows[1].DispObj(detectionResults[0].NGAreahObject);
                    messageShow3.lab_Timer.Text = detectionResults[0].ResultdateTime.ToString();
                    messageShow3.lab_Column.Text = detectionResults[0].ResultXPosition.ToString();
                    messageShow3.lab_Row.Text = detectionResults[0].ResultYPosition.ToString();
                    messageShow3.lab_Size.Text = detectionResults[0].ResultSize.ToString();
                    messageShow3.lab_Kind.Text = detectionResults[0].ResultKind.ToString();
                    messageShow3.lab_Level.Text = detectionResults[0].ResultLevel.ToString();
                    messageShow3.lab_Gray.Text = detectionResults[0].ResultGray.ToString();
                }
                if (detectionResults.Count > 1)
                {
                    hWindows[2].DispObj(detectionResults[1].NGAreahObject);
                    messageShow4.lab_Timer.Text = detectionResults[1].ResultdateTime.ToString();
                    messageShow4.lab_Column.Text = detectionResults[1].ResultXPosition.ToString();
                    messageShow4.lab_Row.Text = detectionResults[1].ResultYPosition.ToString();
                    messageShow4.lab_Size.Text = detectionResults[1].ResultSize.ToString();
                    messageShow4.lab_Kind.Text = detectionResults[1].ResultKind.ToString();
                    messageShow4.lab_Level.Text = detectionResults[1].ResultLevel.ToString();
                    messageShow4.lab_Gray.Text = detectionResults[1].ResultGray.ToString();
                }
            });




            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            lab_detectionTime.Text = milliseconds.ToString();
        }

        public static void DetectionBase(int indexCam, HWindow[] hWindows, HObject hImage)
        {
            HOperatorSet.DispObj(hImage, hWindows[0]);
            HOperatorSet.SetLineWidth(hWindows[0], 1);
            try
            {
                if(indexCam!=1)
                {
                    Halcon.DetectionHalconLine(indexCam, 0, hWindows[0], hImage, Parameters.detectionSpec[indexCam], ref BaseReault[indexCam, 0]);
                }
                else
                {
                    BaseReault[indexCam, 0].Row1 = 0;
                    BaseReault[indexCam, 0].Colum1 = 0;
                    BaseReault[indexCam, 0].Row2 = 20000;
                    BaseReault[indexCam, 0].Colum2 = 0;
                }
                Halcon.DetectionHalconLine(indexCam, 1, hWindows[0], hImage, Parameters.detectionSpec[indexCam], ref BaseReault[indexCam, 1]);

                Halcon.DetectionHalconLine(indexCam, 2, hWindows[0], hImage, Parameters.detectionSpec[indexCam], ref BaseReault[indexCam, 2]);                        
            }
            catch
            {
                LogHelper.WriteError("基准线查找异常，请联系软件工程师");
                return;
            }
            HTuple Row = new HTuple();
            HTuple Column = new HTuple();

            HTuple Row1 = new HTuple();
            HTuple Col1 = new HTuple();

            HTuple Row2 = new HTuple();
            HTuple Col2 = new HTuple();

            HTuple Row3 = new HTuple();
            HTuple Col3 = new HTuple();

            HTuple hRow = new HTuple();
            HTuple hColumn = new HTuple();
            HTuple IsOverlapping;
            Row[0] = 0;
            try
            {
                HOperatorSet.SetDraw(hWindows[0], "margin");
                
                if (indexCam == 0)
                {
                    HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Parameters.detectionSpec[indexCam].RowBase[0] = hRow * Parameters.detectionSpec[indexCam].PixelResolutionRow;
                    Parameters.detectionSpec[indexCam].ColumBase[0] = hColumn * Parameters.detectionSpec[indexCam].PixelResolutionColum;
                    Row[0] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1]+20;
                    Column[0] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0]+20;
                    Row1[0] = hRow + 10;
                    Col1[0] = hColumn + 10;
                    Row1[3] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1]-20;
                    Col1[3] = hColumn + 10;
                    Row2[0] = hRow +(HTuple)Parameters.detectionSpec[indexCam].OffSet[1] +20;
                    Col2[0] = hColumn + 20;
                    Row2[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] +20;
                    Col2[1] = hColumn +(HTuple)Parameters.detectionSpec[indexCam].OffSet[0] -20;
                    HOperatorSet.SetColor(hWindows[0], "red");
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    HOperatorSet.IntersectionLines(0, Halcon.hv_Width[0], Halcon.hv_Height[0], Halcon.hv_Width[0],
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1]+20;
                    Column[1] = hColumn;
                    Row1[1] = hRow + 00;
                    Col1[1] = hColumn;
                    Row1[2] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1]-10;
                    Col1[2] = hColumn ;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    HOperatorSet.IntersectionLines(0, Halcon.hv_Width[0], Halcon.hv_Height[0], Halcon.hv_Width[0],
                    BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[2] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2]-20;
                    Column[2] = hColumn;
                    Row3[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[1] = hColumn;
                    Row3[1] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[1] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    Row3[2] = hRow - 20;
                    Col3[2] = hColumn;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                        BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[3] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Column[3] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    Row2[3] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] -20;
                    Col2[3] = hColumn + 20;
                    Row2[2] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] -20;
                    Col2[2] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] -20;
                    Row3[0] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[0] = hColumn + 20;
                    Row3[3] = hRow - 20;
                    Col3[3] = hColumn + 20;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    if (Parameters.detectionSpec[indexCam].OffSet[1] != 0)
                    {
                        HOperatorSet.SetColor(hWindows[0], "blue");
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 1], Row1, Col1);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 1], hWindows[0]);
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 2], Row2, Col2);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 2], hWindows[0]);
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 3], Row3, Col3);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 3], hWindows[0]);
                    }
                }
                else if (indexCam == 1)
                {
                    HOperatorSet.IntersectionLines(0, 0, Halcon.hv_Height[indexCam], 0,
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Parameters.detectionSpec[indexCam].RowBase[0] = hRow * Parameters.detectionSpec[indexCam].PixelResolutionRow;
                    Parameters.detectionSpec[indexCam].ColumBase[0] = hColumn * Parameters.detectionSpec[indexCam].PixelResolutionColum;
                    Row[0] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Column[0] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    Row1[0] = hRow + 20;
                    Col1[0] = hColumn + 20;
                    Row1[3] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] - 20;
                    Col1[3] = hColumn + 20;
                    HOperatorSet.SetColor(hWindows[0], "red");
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 60, 0);

                    HOperatorSet.IntersectionLines(0, Halcon.hv_Width[indexCam], Halcon.hv_Height[0], Halcon.hv_Width[indexCam],
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 60, 0);
                    Row[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Column[1] = hColumn;
                    Row1[1] = hRow + 20;
                    Col1[1] = hColumn;
                    Row1[2] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] - 20;
                    Col1[2] = hColumn;
                    HOperatorSet.IntersectionLines(0, Halcon.hv_Width[indexCam], Halcon.hv_Height[0], Halcon.hv_Width[indexCam],
                    BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[2] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Column[2] = hColumn - 20;
                    Row3[1] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[1] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    Row3[2] = hRow - 20;
                    Col3[2] = hColumn;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 60, 0);
                    HOperatorSet.IntersectionLines(0, 0, Halcon.hv_Height[indexCam], 0,
                    BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                    
                    Row[3] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Column[3] = hColumn + (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    Row3[0] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[0] = hColumn + 20;
                    Row3[3] = hRow - 20;
                    Col3[3] = hColumn + 20;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    if (Parameters.detectionSpec[indexCam].OffSet[1] != 0)
                    {
                        HOperatorSet.SetColor(hWindows[0], "blue");
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 1], Row1, Col1);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 1], hWindows[0]);
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 3], Row3, Col3);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 3], hWindows[0]);
                    }
                }
                else
                {
                    HOperatorSet.IntersectionLines(0, 0, Halcon.hv_Height[0], 0,
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Parameters.detectionSpec[indexCam].RowBase[0] = hRow * Parameters.detectionSpec[indexCam].PixelResolutionColum;
                    Parameters.detectionSpec[indexCam].ColumBase[0] = hColumn * Parameters.detectionSpec[indexCam].PixelResolutionRow;
                    Row[0] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Column[0] = hColumn;
                    Row1[0] = hRow + 20;
                    Col1[0] = hColumn;
                    Row1[3] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] - 20;
                    Col1[3] = hColumn ;
                    
                    HOperatorSet.SetColor(hWindows[0], "red");
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);

                    HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                    BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Column[1] = hColumn - (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] - 20;
                    Row1[1] = hRow + 20;
                    Col1[1] = hColumn-20;
                    Row1[2] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] - 20;
                    Col1[2] = hColumn-20;
                    Row2[0] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Col2[0] = hColumn - 20;
                    Row2[1] = hRow + (HTuple)Parameters.detectionSpec[indexCam].OffSet[1] + 20;
                    Col2[1] = hColumn - (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);
                    HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                    BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                   
                    Row[2] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Column[2] = hColumn -(HTuple)Parameters.detectionSpec[indexCam].OffSet[0] - 20; ;
                    Row3[1] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[1] = hColumn - 20;
                    Row3[2] = hRow - 20;
                    Col3[2] = hColumn-20;
                    Row2[3] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Col2[3] = hColumn - 20;
                    Row2[2] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Col2[2] = hColumn - (HTuple)Parameters.detectionSpec[indexCam].OffSet[0] + 20;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);

                    HOperatorSet.IntersectionLines(0, 0, Halcon.hv_Height[0], 0,
                   BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out hRow, out hColumn, out IsOverlapping);
                    Row[3] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] - 20;
                    Column[3] = hColumn;
                   
                    Row3[0] = hRow - (HTuple)Parameters.detectionSpec[indexCam].OffSet[2] + 20;
                    Col3[0] = hColumn;
                    Row3[3] = hRow - 20;
                    Col3[3] = hColumn;
                    HOperatorSet.DispCross(hWindows[0], hRow, hColumn, 600, 0);

                    if (Parameters.detectionSpec[indexCam].OffSet[1] != 0)
                    {
                        HOperatorSet.SetColor(hWindows[0], "blue");
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 1], Row1, Col1);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 1], hWindows[0]);
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 2], Row2, Col2);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 2], hWindows[0]);
                        HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4 + 3], Row3, Col3);
                        HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4 + 3], hWindows[0]);
                    }
                }
                HOperatorSet.GenRegionPolygonFilled(out MainForm.hoRegions[indexCam * 4], Row, Column);
                HOperatorSet.DispObj(MainForm.hoRegions[indexCam * 4], hWindows[0]);
            }
            catch
            {
                LogHelper.WriteError("基准点寻找异常，请联系软件工程师");
                return;
            }
            IsOverlapping.Dispose();
            Row.Dispose();
            Column.Dispose();
            Row1.Dispose();
            Col1.Dispose();
            Row2.Dispose();
            Col2.Dispose();
            Row3.Dispose();
            Col3.Dispose();
            hRow.Dispose();
            hColumn.Dispose();
            
        }
        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {

                Parameters.detectionSpec[MainForm.CamNum].RowBase[0] = 0;
                Parameters.detectionSpec[MainForm.CamNum].ColumBase[0] = 0;
                Parameters.detectionSpec[MainForm.CamNum].RowBase[1] = uiDoubleUpDown1.Value;
                Parameters.detectionSpec[MainForm.CamNum].ColumBase[1] = uiDoubleUpDown2.Value;
                Parameters.detectionSpec[MainForm.CamNum].PixelResolutionColum = Convert.ToDouble(txt_PixelResolutionColumn.Text);
                Parameters.detectionSpec[MainForm.CamNum].PixelResolutionRow = Convert.ToDouble(txt_PixelResolutionRow.Text);
                XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[MainForm.CamNum], Parameters.commministion.productName + "/DetectionSpec" + MainForm.CamNum + ".xml");
                XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, Parameters.commministion.productName + "/Specifications.xml");
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        public static void Detection(int indexCam, HWindow[] hWindows,HObject hImage, ref List<DetectionResult> detectionResult)
        {
            if( Parameters.specifications.MeanImageEnabled)
            {
                try
                {
                    Halcon.DetectionMeanImageint((MeanImageEnum)Parameters.specifications.meanImageEnum, hImage, ref hImage);
                }
                catch
                {
                    //MessageBox.Show("滤波处理异常，请联系软件工程师","严重错误提示");
                    return;
                }               
            }
            HOperatorSet.DispObj(hImage, hWindows[0]);
            HOperatorSet.SetLineWidth(hWindows[0], 1);
            try
            {
                DetectionBase(indexCam, hWindows, hImage);
            }
            catch
            {
                //MessageBox.Show("基准线查找异常，请联系软件工程师", "严重错误提示");
                return;
            }
            HTuple Row, Column, IsOverlapping;
            try
            {
                HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                BaseReault[indexCam, 1].Row1, BaseReault[indexCam, 1].Colum1, BaseReault[indexCam, 1].Row2, BaseReault[indexCam, 1].Colum2, out Row, out Column, out IsOverlapping);
                Parameters.detectionSpec[indexCam].RowBase[0] = Row;
                Parameters.detectionSpec[indexCam].ColumBase[0] = Column;
                HOperatorSet.SetColor(hWindows[0], "red");
                HOperatorSet.DispCross(hWindows[0], Row, Column, 60, 0);

                HOperatorSet.IntersectionLines(BaseReault[indexCam, 0].Row1, BaseReault[indexCam, 0].Colum1, BaseReault[indexCam, 0].Row2, BaseReault[indexCam, 0].Colum2,
                    BaseReault[indexCam, 2].Row1, BaseReault[indexCam, 2].Colum1, BaseReault[indexCam, 2].Row2, BaseReault[indexCam, 2].Colum2, out Row, out Column, out IsOverlapping);
                HOperatorSet.DispCross(hWindows[0], Row, Column, 60, 0);
            }
            catch
            {
                //MessageBox.Show("基准点寻找异常，请联系软件工程师", "严重错误提示");
                return;
            }
            //HTuple HomMat2DIdentity = new HTuple();
            //HTuple HomMat2DRotate = new HTuple();
            //HObject ImageAffineTran = new HObject();
            try
            {
                //HOperatorSet.HomMat2dIdentity(out HomMat2DIdentity);
                //HOperatorSet.HomMat2dRotate(HomMat2DIdentity, 0, Row, Column, out HomMat2DRotate);
                //HOperatorSet.AffineTransImage(MainForm.hoRegions[indexCam], out ImageAffineTran, HomMat2DRotate, "constant", "false");

                for (int indexKind = 0; indexKind < 7; indexKind++)
                {
                    Halcon.DetectionHalconRegion(indexCam, indexKind, hWindows, hImage, Parameters.detectionSpec[indexCam], MainForm.hoRegions[indexCam*4], ref detectionResult);
                    Halcon.DetectionHalconRegion(indexCam, 7+indexKind, hWindows, hImage, Parameters.detectionSpec[indexCam], MainForm.hoRegions[indexCam * 4+1], ref detectionResult);
                    Halcon.DetectionHalconRegion(indexCam, 21+indexKind, hWindows, hImage, Parameters.detectionSpec[indexCam], MainForm.hoRegions[indexCam * 4 + 3], ref detectionResult);
                    if (indexCam!=1)
                    {
                        Halcon.DetectionHalconRegion(indexCam, 14+indexKind, hWindows, hImage, Parameters.detectionSpec[indexCam], MainForm.hoRegions[indexCam * 4 + 2], ref detectionResult);
                        
                    }                 
                }                
            }
            catch (Exception ex)
            {
                //HomMat2DIdentity.Dispose();
                //HomMat2DRotate.Dispose();
                MessageBox.Show("瑕疵检测处理异常，请联系软件工程师", "严重错误提示");
                //return;
            }
            //HomMat2DIdentity.Dispose();
            //HomMat2DRotate.Dispose();
            Row.Dispose();
            Column.Dispose();
            IsOverlapping.Dispose();
        }

        private void btn_DrawAOI_Click(object sender, EventArgs e)
        {
            HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);
            Halcon.DetectionDrawAOI(hWindow,  out MainForm.hoRegions[MainForm.CamNum]);
        }

        private void btn_SaveAOI_Click(object sender, EventArgs e)
        {
            //Halcon.DetectionSaveAOI(Parameters.commministion.productName + "/halcon/hoRegion" + MainForm.CamNum + ".tiff", MainForm.hoRegions[MainForm.CamNum]);
        }

        private void chk_SaveDefeatImage_CheckedChanged(object sender, EventArgs e)
        {
			if (Parameters.specifications.SaveDefeatImage != chk_SaveDefeatImage.Checked)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "SaveDefeatImage:" + Parameters.specifications.SaveDefeatImage + "=>" + chk_SaveDefeatImage.Checked);
				Parameters.specifications.SaveDefeatImage = chk_SaveDefeatImage.Checked;
			}
			Parameters.specifications.SaveDefeatImage = chk_SaveDefeatImage.Checked;
        }

        private void chk_SaveOrigalImage_CheckedChanged(object sender, EventArgs e)
        {
			if (Parameters.specifications.SaveOrigalImage != chk_SaveOrigalImage.Checked)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "SaveOrigalImage:" + Parameters.specifications.SaveOrigalImage + "=>" + chk_SaveOrigalImage.Checked);
				Parameters.specifications.SaveOrigalImage = chk_SaveOrigalImage.Checked;
			}
			Parameters.specifications.SaveOrigalImage = chk_SaveOrigalImage.Checked;

        }
        private void btn_XBaseSetting_Click(object sender, EventArgs e)
        {
            MainForm.baseNum = 0;
            卡尺工具设置 flg = new  卡尺工具设置();
            flg.ShowDialog();
        }
        private void btn_Y1BaseSetting_Click(object sender, EventArgs e)
        {
            MainForm.baseNum = 1;
            卡尺工具设置 flg = new 卡尺工具设置();
            flg.ShowDialog();
        }

        private void btn_Y2BaseSetting_Click(object sender, EventArgs e)
        {
            MainForm.baseNum = 2;
            卡尺工具设置 flg = new 卡尺工具设置();
            flg.ShowDialog();
        }

        

        private void btn_ShowAOI_Click(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
                HOperatorSet.SetDraw(hWindows[0], "margin");
                HOperatorSet.DispObj(MainForm.hoRegions[MainForm.CamNum * 4 + uiComboBox1.SelectedIndex], hWindows[0]);
            }
            catch
            {
                MessageBox.Show("请先获取基准点");
            }
            
        }

        private void btnn_IndicationTest_Click(object sender, EventArgs e)
        {
            DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
            MainForm.strDateTime = dtNow.ToString("HHmmss");
            MainForm.strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
            if (Parameters.specifications.MeanImageEnabled)
            {
                Halcon.DetectionMeanImageint((MeanImageEnum)Parameters.specifications.meanImageEnum, MainForm.hImage[MainForm.CamNum], ref MainForm.hImage[MainForm.CamNum]);
            }
            try
            {
                HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
                HOperatorSet.DispObj(MainForm.hoRegions[MainForm.CamNum * 4 + uiComboBox1.SelectedIndex], hWindows[0]);
                if (Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0 
                    && Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0
                    || Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0 
                    && Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0
                    || (Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0
                    || Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0)
                    && (Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0
                    || Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] == 0))
                {
                    MessageBox.Show("检测参数为空，请检查!");
                    return;
                }
                detectionResults = new List<DetectionResult>();
                Halcon.DetectionHalconRegion(MainForm.CamNum, uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex, hWindows, MainForm.hImage[MainForm.CamNum], Parameters.detectionSpec[MainForm.CamNum], MainForm.hoRegions[MainForm.CamNum * 4 + uiComboBox1.SelectedIndex], ref detectionResults);

                for (int i = 0; i < 2; i++)
                {
                    hWindows[i + 1].ClearWindow();
                    HOperatorSet.SetPart(hWindows[i + 1], 0, 0, 1000, 1000);//设置窗体的规格
                }
                messageShow4.lab_Timer.Text = "";
                messageShow4.lab_Column.Text = "";
                messageShow4.lab_Row.Text = "";
                messageShow4.lab_Size.Text = "";
                messageShow4.lab_Kind.Text = "";
                messageShow4.lab_Level.Text = "";
                messageShow4.lab_Gray.Text = "";
                messageShow3.lab_Timer.Text = "";
                messageShow3.lab_Column.Text = "";
                messageShow3.lab_Row.Text = "";
                messageShow3.lab_Size.Text = "";
                messageShow3.lab_Kind.Text = "";
                messageShow3.lab_Level.Text = "";
                messageShow3.lab_Gray.Text = "";
                if (detectionResults.Count != 0)
                {
                    hWindows[1].DispObj(detectionResults[0].NGAreahObject);
                    messageShow3.lab_Timer.Text = detectionResults[0].ResultdateTime.ToString();
                    messageShow3.lab_Column.Text = detectionResults[0].ResultXPosition.ToString();
                    messageShow3.lab_Row.Text = detectionResults[0].ResultYPosition.ToString();
                    messageShow3.lab_Size.Text = detectionResults[0].ResultSize.ToString();
                    messageShow3.lab_Kind.Text = detectionResults[0].ResultKind.ToString();
                    messageShow3.lab_Level.Text = detectionResults[0].ResultLevel.ToString();
                    messageShow3.lab_Gray.Text = detectionResults[0].ResultGray.ToString();
                }
                if (detectionResults.Count > 1)
                {                   
                    hWindows[2].DispObj(detectionResults[1].NGAreahObject);
                    messageShow4.lab_Timer.Text = detectionResults[1].ResultdateTime.ToString();
                    messageShow4.lab_Column.Text = detectionResults[1].ResultXPosition.ToString();
                    messageShow4.lab_Row.Text = detectionResults[1].ResultYPosition.ToString();
                    messageShow4.lab_Size.Text = detectionResults[1].ResultSize.ToString();
                    messageShow4.lab_Kind.Text = detectionResults[1].ResultKind.ToString();
                    messageShow4.lab_Level.Text = detectionResults[1].ResultLevel.ToString();
                    messageShow4.lab_Gray.Text = detectionResults[1].ResultGray.ToString();
                }
            }
            catch  (Exception ex)
            {
                MessageBox.Show("请先获取基准点"+ ex.Message);
            }

        }

        private void cmb_Indication_SelectedIndexChanged(object sender, EventArgs e)
        {
            num_lengthWidthRatio.Value = Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex*7+cmb_Indication.SelectedIndex];
            num_ThresholdLow.Value = Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex];
            num_ThresholdHigh.Value = Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex];
            num_AreaLow.Value = Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex];
            num_AreaHigh.Value = Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex];
        }

        private void num_lengthWidthRatio_ValueChanged(object sender, double value)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] != num_lengthWidthRatio.Value)
			{
				LogHelper.WriteWarn(" "+MainForm.UserName + "lengthWidthRatio:" + Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] + "  第" + Convert.ToString(uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex) + "号缺陷" + "=>" + num_lengthWidthRatio.Value);
				Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_lengthWidthRatio.Value;
			}

			Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_lengthWidthRatio.Value;
        }

        private void num_ThresholdLow_ValueChanged(object sender, double value)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] != num_ThresholdLow.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "ThresholdLow:" + Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] + "  第" + Convert.ToString(uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex) + "号缺陷" + "=>" + num_ThresholdLow.Value);
				Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_ThresholdLow.Value;
			}
			Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_ThresholdLow.Value;
        }

        private void num_ThresholdHigh_ValueChanged(object sender, double value)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] != num_ThresholdHigh.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "ThresholdHigh:" + Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] + "  第" + Convert.ToString(uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex) + "号缺陷" + "=>" + num_ThresholdHigh.Value);
				Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_ThresholdHigh.Value;
			}
			Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_ThresholdHigh.Value;
        }

        private void num_AreaLow_ValueChanged(object sender, double value)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] != num_AreaLow.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "AreaLow:" + Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] + "  第" + Convert.ToString(uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex) + "号缺陷" + "=>" + num_AreaLow.Value);
				Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_AreaLow.Value;
			}
			Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_AreaLow.Value ;
        }

        private void num_AreaHigh_ValueChanged(object sender, double value)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] != num_AreaHigh.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "AreaHigh:" + Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] + "  第" + Convert.ToString(uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex) + "号缺陷" + "=>" + num_AreaHigh.Value);
				Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_AreaHigh.Value;
			}
			Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7 + cmb_Indication.SelectedIndex] = num_AreaHigh.Value;
        }

        private void btn_MeanImageTest_Click(object sender, EventArgs e)
        {
            HObject hObject = new HObject();
            Halcon.DetectionMeanImageint((MeanImageEnum)cmb_MeanImageList.SelectedIndex, MainForm.hImage[MainForm.CamNum],ref hObject);
            HOperatorSet.DispObj(hObject, hWindow);
            hObject.Dispose();
        }

        private void cmb_MeanImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
			Parameters.specifications.meanImageEnum = cmb_MeanImageList.SelectedIndex;
        }

        private void chk_MeanImageEnabled_CheckedChanged(object sender, EventArgs e)
        {
			if (Parameters.specifications.MeanImageEnabled != chk_MeanImageEnabled.Checked)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeanImageEnabled:" + Parameters.specifications.MeanImageEnabled + "=>" + chk_MeanImageEnabled.Checked);
				Parameters.specifications.MeanImageEnabled = chk_MeanImageEnabled.Checked;
			}
			Parameters.specifications.MeanImageEnabled = chk_MeanImageEnabled.Checked;
        }

        private void btn_Minimizid_System_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btn_showXBase_Click(object sender, EventArgs e)
        {
            showXBase(sender, e);
        }

        private void btn_showYBase_Click(object sender, EventArgs e)
        {
            相机检测设置.showY1Base(sender, e);
        }

        private void btn_showY2Base_Click(object sender, EventArgs e)
        {
            相机检测设置.showY2Base(sender, e);
        }

        private void chk_SaveCropImage_CheckedChanged(object sender, EventArgs e)
        {
			if (Parameters.specifications.SaveCropImage != chk_SaveCropImage.Checked)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "SaveCropImage:" + Parameters.specifications.SaveCropImage + "=>" + chk_SaveCropImage.Checked);
				Parameters.specifications.SaveCropImage = chk_SaveCropImage.Checked;
			}
			Parameters.specifications.SaveCropImage = chk_SaveCropImage.Checked;
        }

        private void txt_PixelResolutionColumn_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txt_PixelResolutionRow_TextChanged(object sender, EventArgs e)
        {
            
            
        }

        private void PixelResolutionRow_Paint(object sender, PaintEventArgs e)
        {

        }
        private delegate void SetTextValueCallBack(int i, HObject hObject, string path);

        //声明回调
        private SetTextValueCallBack setCallBack;

        private void SaveImages(int i, HObject hObject, string path)
        {
            string stfFileNameOut = "CAM" + MainForm.CamNum + path + i + MainForm.productSN + "-" + MainForm.strDateTime;  // 默认的图像保存名称  // 默认的图像保存名称
            string pathOut = Parameters.commministion.ImageSavePath + "/" + MainForm.strDateTimeDay + "/" + MainForm.productSN + "/";
            if (!System.IO.Directory.Exists(pathOut))
            {
                System.IO.Directory.CreateDirectory(pathOut);//不存在就创建文件夹
            }
            HOperatorSet.WriteImage(hObject, "jpeg", 0, pathOut + stfFileNameOut + ".jpeg");
        }
        private void uiButton1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间
            List<DetectionResult> detectionResults = new List<DetectionResult>();           
            setCallBack = SaveImages;
            this.Invoke(setCallBack, MainForm.CamNum, MainForm.hImage[MainForm.CamNum], "-IN-");
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            lab_detectionTime.Text = milliseconds.ToString();
            
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间
            HOperatorSet.DumpWindowImage(out MainForm.hObjectOut[MainForm.CamNum], hWindows[0]);
            setCallBack = SaveImages;
            this.Invoke(setCallBack, MainForm.CamNum, MainForm.hObjectOut[MainForm.CamNum], "-OUT-");
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            lab_detectionTime.Text = milliseconds.ToString();
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间

            setCallBack = SaveImages;
            for(int i=0;i< detectionResults.Count; i++)
            {             
                this.Invoke(setCallBack, i, detectionResults[i].NGAreahObject, "-Area-");
            }
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
            lab_detectionTime.Text = milliseconds.ToString();
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            try
            {
                DetectionBase(MainForm.CamNum, hWindows, MainForm.hImage[MainForm.CamNum]);
            }
            catch
            {
                MessageBox.Show("基准线查找异常", "严重错误提示");
            }
        }
        HObject AreahObject = new HObject();
        private void hWindowControl4_HMouseDown(object sender, HMouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                HOperatorSet.SetPart(hWindow, 0, 0, -1, -1);//设置窗体的规格
                HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindow);//显示图片
            }
            if (e.Clicks == 1)
            {
                HTuple X = new HTuple();
                HTuple Y = new HTuple();
                HTuple row = new HTuple();
                HTuple col = new HTuple();
                HTuple grayval = new HTuple();
                hWindows[1].SetPart(0, 0, 400, 400);
                HOperatorSet.GetImageSize(MainForm.hImage[MainForm.CamNum], out X, out Y);
                //hWindowControl1.Size = new System.Drawing.Size(X, Y);
                hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, X, Y);
                if ((int)e.X < 200)
                {
                    col = 200;
                }
                else if ((int)e.X > Halcon.hv_Width[MainForm.CamNum] - 200)
                {
                    col = (int)e.X - 200;
                }
                else
                {
                    col = (int)e.X;
                }
                if ((int)e.Y < 200)
                {
                    row = 200;
                }
                else if ((int)e.Y > Halcon.hv_Height[MainForm.CamNum] - 200)
                {
                    row = (int)e.Y - 200;
                }
                else
                {
                    row = (int)e.Y;
                }
                col = (int)e.X;
                row = (int)e.Y;
                hWindows[1].SetPart(0, 0, 400, 400);
                try
                {
                    HOperatorSet.CropPart(MainForm.hImage[MainForm.CamNum], out AreahObject, row - 200, col - 200, 400, 400);
                    HOperatorSet.DispObj(AreahObject, hWindows[1]);
                }
                catch (Exception ex)
                {
                    
                }
                HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
                X.Dispose();
                Y.Dispose();
                row.Dispose();
                col.Dispose();
                grayval.Dispose();
            }
                
        }

        private void hWindowControl2_HMouseDown(object sender, HMouseEventArgs e)
        {
            HTuple X = new HTuple();
            HTuple Y = new HTuple();
            HTuple row = new HTuple();
            HTuple col = new HTuple();
            HTuple grayval = new HTuple();
            hWindows[2].SetPart(0, 0, 40, 40);
            HOperatorSet.GetImageSize(AreahObject, out X, out Y);
            //hWindows[2].Size = new System.Drawing.Size(X, Y);
            hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, X, Y);
            if ((int)e.X < 20)
            {
                col = 20;
            }
            else if ((int)e.X > 400 - 20)
            {
                col = (int)e.X - 20;
            }
            else
            {
                col = (int)e.X;
            }
            if ((int)e.Y < 20)
            {
                row = 20;
            }
            else if ((int)e.Y > 400 - 20)
            {
                row = (int)e.Y - 20;
            }
            else
            {
                row = (int)e.Y;
            }
            //HOperatorSet.GetGrayval(MainForm.hImage[MainForm.CamNum], row, col, out grayval);
            //MessageBox.Show("当前坐标：  Y：" + row.ToString() + "  X: " + col.ToString() + "  灰度值  " + grayval.ToString());
            hWindows[2].SetPart(0, 0, 40, 40);
            HObject AreahObject1;
            HOperatorSet.CropPart(AreahObject, out AreahObject1, row - 20, col - 20, 40, 40);
            HOperatorSet.DispObj(AreahObject1, hWindows[2]);
            //HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);
            AreahObject1.Dispose();
            X.Dispose();
            Y.Dispose();
            row.Dispose();
            col.Dispose();
            grayval.Dispose();
        }

        private void hWindowControl4_HMouseWheel(object sender, HMouseEventArgs e)
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(LMap_MouseWheel4);
        }
        public void LMap_MouseWheel4(object sender, MouseEventArgs e)
        {
            //当e.Delta > 0时鼠标滚轮是向上滚动，e.Delta < 0时鼠标滚轮向下滚动
            if (e.Delta > 0)//滚轮向上
            {
                Halcon.ImgZoom(MainForm.hImage[MainForm.CamNum], hWindow, 1);
            }
            else
            {
                Halcon.ImgZoom(MainForm.hImage[MainForm.CamNum], hWindow, 0);
            }
        }

        private void hWindowControl1_HMouseWheel(object sender, HMouseEventArgs e)
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(LMap_MouseWheel1);
        }
        public void LMap_MouseWheel1(object sender, MouseEventArgs e)
        {
            //当e.Delta > 0时鼠标滚轮是向上滚动，e.Delta < 0时鼠标滚轮向下滚动
            if (e.Delta > 0)//滚轮向上
            {
                Halcon.ImgZoom(MainForm.hImage[MainForm.CamNum], hWindows[0], 1);
            }
            else
            {
                Halcon.ImgZoom(MainForm.hImage[MainForm.CamNum], hWindows[0], 0);
            }
        }

        private void hWindowControl1_HMouseDown(object sender, HMouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格
                HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[0]);//显示图片
            }
        }

        private void uiButton5_Click(object sender, EventArgs e)
        {
            MainForm.baseNum = 3;
            卡尺工具设置 flg = new 卡尺工具设置();
            flg.ShowDialog();
        }

        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_Indication.SelectedIndex = 0;
            num_lengthWidthRatio.Value = Parameters.detectionSpec[MainForm.CamNum].lengthWidthRatio[uiComboBox1.SelectedIndex * 7];
            num_ThresholdLow.Value = Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[uiComboBox1.SelectedIndex * 7];
            num_ThresholdHigh.Value = Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[uiComboBox1.SelectedIndex *7];
            num_AreaLow.Value = Parameters.detectionSpec[MainForm.CamNum].AreaLow[uiComboBox1.SelectedIndex *7];
            num_AreaHigh.Value = Parameters.detectionSpec[MainForm.CamNum].AreaHigh[uiComboBox1.SelectedIndex * 7];
        }
    }
}
