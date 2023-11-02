using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using WY_App.Utility;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using HalconDotNet;
using TcpClient = WY_App.Utility.TcpClient;
using Sunny.UI.Win32;
using Newtonsoft.Json.Linq;
using Sunny.UI;
using static WY_App.Utility.Parameters;
using OpenCvSharp.Dnn;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using OpenCvSharp.Flann;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Remoting.Messaging;
using System.Web.UI.WebControls;
using HslCommunication.Secs.Types;
using HslCommunication.BasicFramework;
using static ODT.Common.LanguageTranslationConstants;
using WY_App.Forms;
using System.Web.SessionState;
using System.Xml.Linq;
using IWshRuntimeLibrary;

namespace WY_App
{
    public partial class MainForm : Form
    {
        HslCommunication hslCommunication;
        public static string Alarm = "";
        public static List<string> AlarmList = new List<string>();
        Thread myThread;
        Thread[] MainThread = new Thread[3];
        //Thread ImageThread;
        //public static MVLineScanCam HivCam = new MVLineScanCam(); 
        
        public static Parameters.Rect1[] specifications;
        HWindow[] hWindows0;
        HWindow[] hWindows1;
        HWindow[] hWindows2;
        public static List<HObject> ho_Image = new List<HObject>();
        public static List<HObject> ho_DefectImage = new List<HObject>();
        public static List<HObject> ho_OrigalImage = new List<HObject>();
        public static HObject[] hImage = new HObject[3];
        public static HTuple[] hv_AcqHandle = new HTuple[4];
        double[] defectionValues = new double[4];
        public static string UserName = "人员修改";
        public static int UserLevel=1 ;

        private static Queue<Func<int>> m_List = new Queue<Func<int>>();
        private static object m_obj = new object();
        private bool isExit = false;
        public static HObject[] hObjectOut = new HObject[3];
        public static int CamNum = 0;
        public static int baseNum = 0;
        public static HObject[] hoRegions = new HObject[12];
        bool m_Pause = false;
        public static string productSN = "0000";
        public static string productSheetNum = "0000";
        public static string strDateTime;
        public static string strDateTimeDay;
        private delegate void SetTextValueCallBack(int i, HObject hObject, string path);
        //声明回调
        private SetTextValueCallBack setCallBack;

        public static AutoResetEvent[] ImageEvent = new AutoResetEvent[3];
        public static IKapCLineCam[] iKapCLineCam = new IKapCLineCam[3];

        public static IKapCLineCam0 iKapCLineCam0;
        public static IKapCLineCam1 iKapCLineCam1;
        public static IKapCLineCam2 iKapCLineCam2;
        //Halcon halcon = new Halcon();

        public MainForm()
        {
            InitializeComponent();
            #region 读取配置文件

            try
            {
                Parameters.deviceName = XMLHelper.BackSerialize<Parameters.DeviceName>(@"D:\\DeviceName.xml");
            }
            catch
            {
                Parameters.deviceName = new Parameters.DeviceName();
                XMLHelper.serialize<Parameters.DeviceName>(Parameters.deviceName, @"D:\\DeviceName.xml");
            }
            if (!EnumDivice(Parameters.deviceName.DeviceID))
            {
                注册机器 flg = new 注册机器();
                flg.TransfEvent += DeviceID_TransfEvent;
                flg.ShowDialog();
                if (!EnumDivice(DeviceID))
                {
                    Environment.Exit(1);
                    return;
                }
            }
            try
            {
                Parameters.commministion = XMLHelper.BackSerialize<Parameters.Commministion>("Parameter/Commministion.xml");
            }
            catch
            {
                Parameters.commministion = new Parameters.Commministion();
                XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");
            }
            try
            {
                Parameters.counts = XMLHelper.BackSerialize<Parameters.Counts>(Parameters.commministion.productName + "/CountsParams.xml");
            }
            catch
            {
                Parameters.counts = new Parameters.Counts();
                XMLHelper.serialize<Parameters.Counts>(Parameters.counts, Parameters.commministion.productName + "/CountsParams.xml");
            }
            try
            {
                Parameters.counts = XMLHelper.BackSerialize<Parameters.Counts>(Parameters.commministion.productName + "/CountsParams.xml");
            }
            catch
            {
                Parameters.cameraParam = new Parameters.CameraParam();
                XMLHelper.serialize<Parameters.CameraParam>(Parameters.cameraParam, Parameters.commministion.productName + "/CameraParam.xml");
            }
            try
            {
                Parameters.specifications = XMLHelper.BackSerialize<Parameters.Specifications>(Parameters.commministion.productName + "/Specifications.xml");
            }
            catch
            {
                Parameters.specifications = new Parameters.Specifications();
                XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, Parameters.commministion.productName + "/Specifications.xml");
            }
            try
            {
                Constructor.cameraParams = XMLHelper.BackSerialize<Constructor.CameraParams>(Parameters.commministion.productName + "/CameraParams.xml");
            }
            catch
            {
                Constructor.cameraParams = new Constructor.CameraParams();
                XMLHelper.serialize<Constructor.CameraParams>(Constructor.cameraParams, Parameters.commministion.productName + "/CameraParams.xml");
            }

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Parameters.detectionSpec[i] = XMLHelper.BackSerialize<Parameters.DetectionSpec>(Parameters.commministion.productName + "/DetectionSpec" + i + ".xml");
                }
                catch
                {
                    Parameters.detectionSpec[i] = new Parameters.DetectionSpec();
                    XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[i], Parameters.commministion.productName + "/DetectionSpec" + i + ".xml");
                }
                //HOperatorSet.ReadRegion(out hoRegions[i], Parameters.commministion.productName + "/halcon/hoRegion" + i + ".tiff");
            }

            Product = Parameters.commministion.productName;
            #endregion
            ImageEvent[0] = new AutoResetEvent(false);
            ImageEvent[1] = new AutoResetEvent(false);
            ImageEvent[2] = new AutoResetEvent(false);
            HOperatorSet.ReadImage(out hImage[0], Parameters.commministion.productName + "/N0.jpeg");
            HOperatorSet.GetImageSize(MainForm.hImage[0], out Halcon.hv_Width[0], out Halcon.hv_Height[0]);//获取图片大小规格
            HOperatorSet.ReadImage(out hImage[1], Parameters.commministion.productName + "/N1.jpeg");
            HOperatorSet.GetImageSize(MainForm.hImage[1], out Halcon.hv_Width[1], out Halcon.hv_Height[1]);//获取图片大小规格
            HOperatorSet.ReadImage(out hImage[2], Parameters.commministion.productName + "/N2.jpeg");
            HOperatorSet.GetImageSize(MainForm.hImage[2], out Halcon.hv_Width[2], out Halcon.hv_Height[2]);//获取图片大小规格
            hWindows0 = new HWindow[3] { hWindowControl1.HalconWindow, hWindowControl4.HalconWindow, hWindowControl5.HalconWindow };
            HOperatorSet.SetPart(hWindows0[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows0[1], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.SetPart(hWindows0[2], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.DispObj(hImage[0], hWindows0[0]);
            hWindows1 = new HWindow[3] { hWindowControl2.HalconWindow, hWindowControl6.HalconWindow, hWindowControl7.HalconWindow };
            HOperatorSet.SetPart(hWindows1[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows1[1], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.SetPart(hWindows1[2], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.DispObj(hImage[1], hWindows1[0]);

            hWindows2 = new HWindow[3] { hWindowControl3.HalconWindow, hWindowControl8.HalconWindow, hWindowControl9.HalconWindow };
            HOperatorSet.SetPart(hWindows2[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows2[1], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.SetPart(hWindows2[2], 0, 0, 1000, 1000);//设置窗体的规格
            HOperatorSet.DispObj(hImage[2], hWindows2[0]);
            pictureBox1.Load(Application.StartupPath + "/image/logo.png");
            
            try
            {
                iKapCLineCam0 = new IKapCLineCam0();
                iKapCLineCam1 = new IKapCLineCam1();
                iKapCLineCam2 = new IKapCLineCam2();
                
                IKapCLineCam0.OpenDevice(iKapCLineCam0);
                IKapCLineCam1.OpenDevice(iKapCLineCam1);
                IKapCLineCam2.OpenDevice(iKapCLineCam2);

            }
            catch
            {

            }
            myThread = new Thread(initAll);
            myThread.IsBackground = true;
            myThread.Start();
        }
        private bool EnumDivice(string DiviceSn)
        {
            SoftAuthorize softAuthorize = new SoftAuthorize();
            if (!softAuthorize.CheckAuthorize(DiviceSn, AuthorizeEncrypted))
            {
                MessageBox.Show("注册码和机器不匹配，请联系能特技术人员获取对应激活序列号！");
                Close();
                return false;
            }
            else
            {
                return true;
            }

        }
        public static string DeviceID = "";
        void DeviceID_TransfEvent(string value)
        {
            DeviceID = value;
        }
        private string AuthorizeEncrypted(string origin)
        {
            return SoftSecurity.MD5Encrypt(origin, "12345678");
        }
        /// <summary>
        /// skinPictureBox1滚动缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {


            }
            catch (Exception x)
            {
                LogHelper.Log.WriteError("缩放异常：" + x.Message);
            }
        }

        //鼠标移动
        int xPos = 0;
        int yPos = 0;
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            xPos = e.X;//当前x坐标.
            yPos = e.Y;//当前y坐标.
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseMove1(object sender, MouseEventArgs e)
        {

        }

        private void initAll()
        {
            while (true)
            {
                Thread.Sleep(1);
                Task task = new Task(() =>
                {
                    MethodInvoker start = new MethodInvoker(() =>
                    {
                        lab_Time.Text = System.DateTime.Now.ToString();
                        if (Parameters.commministion.PlcEnable)
                        {
                            if (HslCommunication.plc_connect_result)
                            {
                                lab_PLCStatus.Text = "在线";
                                lab_PLCStatus.BackColor = Color.Green;
                            }
                            else
                            {
                                lab_PLCStatus.Text = "离线";
                                lab_PLCStatus.BackColor = Color.Red;
                            }
                        }
                        else
                        {
                            lab_PLCStatus.Text = "禁用";
                            lab_PLCStatus.BackColor = Color.Gray;
                        }
                        if (Parameters.commministion.TcpClientEnable)
                        {
                            if (TcpClient.TcpClientConnectResult)
                            {
                                lab_Client.Text = "在线";
                                lab_Client.BackColor = Color.Green;
                            }
                            else
                            {
                                lab_Client.Text = "等待";
                                lab_Client.BackColor = Color.Red;
                            }
                        }
                        else
                        {
                            lab_Client.Text = "禁用";
                            lab_Client.BackColor = Color.Gray;
                        }
                        if (Parameters.commministion.TcpServerEnable)
                        {
                            if (TcpServer.TcpServerConnectResult)
                            {
                                lab_Server.Text = "在线";
                                lab_Server.BackColor = Color.Green;
                            }
                            else
                            {
                                lab_Server.Text = "等待";
                                lab_Server.BackColor = Color.Red;
                            }
                        }
                        else
                        {
                            lab_Server.Text = "禁用";
                            lab_Server.BackColor = Color.Gray;
                        }
                        try
                        {
                            if (AlarmList.Count > 0)
                            {
                                lst_LogInfos.Items.Add(AlarmList[0]);
                                AlarmList.RemoveAt(0);
                            }
                        }
                        catch
                        {

                        }
                        if (lst_LogInfos.Items.Count > 10)
                        {
                            lst_LogInfos.Items.RemoveAt(0);
                        }
                        if (Halcon.CamConnect[0] || IKapCLineCam0.m_hCameraResult)
                        {
                            lab_Cam1.Text = "在线";
                            lab_Cam1.BackColor = Color.Green;
                        }
                        else
                        {
                            lab_Cam1.Text = "等待";
                            lab_Cam1.BackColor = Color.Red;
                        }
                        if (Halcon.CamConnect[1] || IKapCLineCam1.m_hCameraResult)
                        {
                            lab_Cam2.Text = "在线";
                            lab_Cam2.BackColor = Color.Green;
                        }
                        else
                        {
                            lab_Cam2.Text = "等待";
                            lab_Cam2.BackColor = Color.Red;
                        }
                        if (Halcon.CamConnect[2] || IKapCLineCam2.m_hCameraResult)
                        {
                            lab_Cam3.Text = "在线";
                            lab_Cam3.BackColor = Color.Green;
                        }
                        else
                        {
                            lab_Cam3.Text = "等待";
                            lab_Cam3.BackColor = Color.Red;
                        }

                        lab_Product.Text = Product;
                        
                    });
                    this.BeginInvoke(start);
                });
                task.Start();
            }

        }
        public static void RefreshPara()
        {
            try
            {
                Parameters.commministion = XMLHelper.BackSerialize<Parameters.Commministion>("Parameter/Commministion.xml");
            }
            catch
            {
                Parameters.commministion = new Parameters.Commministion();
                XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");
            }
            try
            {
                Parameters.counts = XMLHelper.BackSerialize<Parameters.Counts>(Parameters.commministion.productName + "/CountsParams.xml");
            }
            catch
            {
                Parameters.counts = new Parameters.Counts();
                XMLHelper.serialize<Parameters.Counts>(Parameters.counts, Parameters.commministion.productName + "/CountsParams.xml");
            }
            try
            {
                Parameters.counts = XMLHelper.BackSerialize<Parameters.Counts>(Parameters.commministion.productName + "/CountsParams.xml");
            }
            catch
            {
                Parameters.cameraParam = new Parameters.CameraParam();
                XMLHelper.serialize<Parameters.CameraParam>(Parameters.cameraParam, Parameters.commministion.productName + "/CameraParam.xml");
            }
            try
            {
                Parameters.specifications = XMLHelper.BackSerialize<Parameters.Specifications>(Parameters.commministion.productName + "/Specifications.xml");
            }
            catch
            {
                Parameters.specifications = new Parameters.Specifications();
                XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, Parameters.commministion.productName + "/Specifications.xml");
            }
            try
            {
                Constructor.cameraParams = XMLHelper.BackSerialize<Constructor.CameraParams>(Parameters.commministion.productName + "/CameraParams.xml");
            }
            catch
            {
                Constructor.cameraParams = new Constructor.CameraParams();
                XMLHelper.serialize<Constructor.CameraParams>(Constructor.cameraParams, Parameters.commministion.productName + "/CameraParams.xml");
            }

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Parameters.detectionSpec[i] = XMLHelper.BackSerialize<Parameters.DetectionSpec>(Parameters.commministion.productName + "/DetectionSpec" + i + ".xml");
                }
                catch
                {
                    Parameters.detectionSpec[i] = new Parameters.DetectionSpec();
                    XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[i], Parameters.commministion.productName + "/DetectionSpec" + i + ".xml");
                }
                //HOperatorSet.ReadRegion(out hoRegions[i], Parameters.commministion.productName + "/halcon/hoRegion" + i + ".tiff");
            }
        }
        public static void PlcSheetNum()   //读取片材编号
        {
            //int productSheetTime = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[43]).Content;

            //if (productSheetTime==1)
            // {
            int FileOutLong = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[42]).Content;
            int FileOutStrLong;

            if (FileOutLong % 2 == 1)
            {
                FileOutStrLong = FileOutLong / 2 + 1;
            }
            else
            {
                FileOutStrLong = FileOutLong / 2;
            }

            string SubStrFile = HslCommunication._NetworkTcpDevice.ReadString(Parameters.plcParams.预留地址[41], (ushort)FileOutStrLong).Content;

            if (SubStrFile != null && SubStrFile != "")
            {
                string productSheetNums = SubStrFile.Substring(0, FileOutLong);
                productSheetNum = productSheetNums.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            }

            HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[43], 0);
            //}

        }

        public static void SaveImages(int i, HObject hObject, string path)
        {

            string pathOut = Parameters.commministion.ImageSavePath + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + path + "\\";

            if (productSheetNum != null && productSheetNum != " ")
            {
                if (!System.IO.Directory.Exists(pathOut))
                {
                    System.IO.Directory.CreateDirectory(pathOut);//不存在就创建文件夹
                }
                HOperatorSet.WriteImage(hObject, "jpeg", 0, pathOut + productSheetNum + ".jpeg");
            }

            if (path == "IN-")
            {
                if (Parameters.commministion.UploadOneEnable)
                {
                    UploadImages(i, "IN-", 1);
                }
                if (Parameters.commministion.UploadTwoEnable)
                {
                    UploadImages(i, "IN-", 2);
                }
                if (Parameters.commministion.UploadThreeEnable)
                {
                    UploadImages(i, "IN-", 3);
                }
            }

            if (path == "OUT-")
            {
                if (Parameters.commministion.UploadOneTestEnable)
                {
                    UploadImages(i, "OUT-", 1);
                    if (Parameters.specifications.SaveCropImage)
                    {
                        UploadNgImages(i, 1);
                    }
                }
                if (Parameters.commministion.UploadTwoTestEnable)
                {
                    UploadImages(i, "OUT-", 2);
                    if (Parameters.specifications.SaveCropImage)
                    {
                        UploadNgImages(i, 2);
                    }
                }
                if (Parameters.commministion.UploadThreeTestEnable)
                {
                    UploadImages(i, "OUT-", 3);
                    if (Parameters.specifications.SaveCropImage)
                    {
                        UploadNgImages(i, 3);
                    }
                }
            }


        }

        public static void UploadNgImages(int i, int index)
        {
            string pathOut = Parameters.commministion.ImageSavePath + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + "-NG-";

            if (!System.IO.Directory.Exists(pathOut))
            {
                System.IO.Directory.CreateDirectory(pathOut);//不存在就创建文件夹
            }

            try
            {
                // 获取文件夹中的所有文件的路径
                string[] files = Directory.GetFiles(pathOut);

                string destinationFolderPath = " ";

                if (index == 1)
                {
                    // 目标文件夹的路径（例如：C:\path\to\your\destination\folder）
                    destinationFolderPath = Parameters.commministion.ImageUploadOne + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + "-NG-" + "\\";
                }
                else if (index == 2)
                {
                    destinationFolderPath = Parameters.commministion.ImageUploadTwo + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + "-NG-" + "\\";
                }
                else if (index == 3)
                {
                    destinationFolderPath = Parameters.commministion.ImageUploadThree + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + "-NG-" + "\\";
                }

                for (int j = 0; j < files.Length - 1; j++)
                {
                    // 获取文件名（包括扩展名）
                    string fileName = Path.GetFileName(files[j]);

                    // 拼接目标文件的完整路径
                    string destinationFilePath = Path.Combine(destinationFolderPath, fileName);

                    // 执行复制操作
                    System.IO.File.Copy(fileName, destinationFilePath, true);

                    LogHelper.WriteInfo("文件已成功复制到目标文件夹！");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo("发生错误,查看文件路径是否存在问题");
            }

        }

        public static void UploadImages(int i, string path, int index)
        {
            try
            {
                string sourceFilePath = " ";
                string destinationFolderPath = " ";

                // 要复制的文件的完整路径
                sourceFilePath = Parameters.commministion.ImageSavePath + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + path + "\\" + productSheetNum + ".jpeg";
                if (index == 1)
                {
                    // 目标文件夹的路径（例如：C:\path\to\your\destination\folder）
                    destinationFolderPath = Parameters.commministion.ImageUploadOne + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + path + "\\";
                }
                else if (index == 2)
                {
                    destinationFolderPath = Parameters.commministion.ImageUploadTwo + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + path + "\\";
                }
                else if (index == 3)
                {
                    destinationFolderPath = Parameters.commministion.ImageUploadThree + strDateTimeDay + "\\" + Product + "\\" + productSheetNum + "\\" + "CAM-" + i.ToString() + "\\" + path + "\\";
                }
                if (sourceFilePath != " " && destinationFolderPath != " ")
                {
                    if (!System.IO.Directory.Exists(destinationFolderPath))
                    {
                        System.IO.Directory.CreateDirectory(destinationFolderPath);//不存在就创建文件夹
                    }
                    // 获取文件名（包括扩展名）
                    string fileName = Path.GetFileName(sourceFilePath);

                    // 拼接目标文件的完整路径
                    string destinationFilePath = Path.Combine(destinationFolderPath, fileName);

                    // 执行复制操作
                    System.IO.File.Copy(sourceFilePath, destinationFilePath, true);

                    LogHelper.WriteInfo("文件已成功复制到目标文件夹！");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo("发生错误,查看文件路径是否存在问题");
            }
        }

		public static void ProductReport(string CamIndex, DetectionResult detectionResults,bool NgOk,int PartitionIndex)
        {
            if(Parameters.commministion.ImageSavePath!=null&& strDateTimeDay!=null&& Product != null)
            {
				string Path = Parameters.commministion.ImageSavePath + strDateTimeDay + "\\" + Product ;
				if (!System.IO.Directory.Exists(Path))
				{
					System.IO.Directory.CreateDirectory(Path);//不存在就创建文件夹
				}

				string FilePath = Parameters.commministion.ImageSavePath + strDateTimeDay + "\\" + Product + "\\" + "生产报表.csv";

               

				string sPartitionIndex = " ";
				if (PartitionIndex / 7 == 0)
				{
					sPartitionIndex = "中";
				}
				if (PartitionIndex / 7 == 1)
				{
					sPartitionIndex = "上";
				}
				if (PartitionIndex / 7 == 2)
				{
					sPartitionIndex = "边";
				}
				if (PartitionIndex / 7 == 3)
				{
					sPartitionIndex = "下";
				}
				if (PartitionIndex / 7 == 4)
				{
					sPartitionIndex = "全";
				}

				if (NgOk == false)
				{
					// 检查文件是否存在，如果不存在则创建新文件并写入表头
					try
					{
						if (!System.IO.File.Exists(FilePath))
						{
							using (StreamWriter writer = new StreamWriter(FilePath))
							{
								writer.WriteLine("日期,时间,ID,X坐标,Y坐标,缺陷类型,大小,NgOk,相机序号,分区序号");
							}
						}
					}
					catch
					{
						LogHelper.WriteInfo("发生错误,联系工程师查看文件路径是否存在问题");
					}
					// 要添加的数据
					DateTime currentDate = DateTime.Now;
					string currentTime = DateTime.Now.ToString("HH:mm:ss");
					string id = productSheetNum;
					double xCoordinate = detectionResults.ResultXPosition;
					double yCoordinate = detectionResults.ResultYPosition;
					string Kind = detectionResults.ResultKind.ToString();
					double Area = detectionResults.ResultSize;
					string OkOrNg = "NG";

                    // 将数据追加到文件
                    try
                    {
						using (StreamWriter writer = System.IO.File.AppendText(FilePath))
						{
							// 格式化数据并将其写入文件
							string dataLine = $"{currentDate.ToShortDateString()},{currentTime},{id},{xCoordinate},{yCoordinate},{Kind},{Area},{OkOrNg},{CamIndex},{sPartitionIndex}";
							writer.WriteLine(dataLine);
						}
					}
                    catch
                    {
						LogHelper.WriteInfo("报表路径有误，请联系工程师确认");
					}
					
				}
				else
				{
					// 检查文件是否存在，如果不存在则创建新文件并写入表头
					try
					{
						if (!System.IO.File.Exists(FilePath))
						{
							using (StreamWriter writer = new StreamWriter(FilePath))
							{
								writer.WriteLine("日期,时间,ID,X坐标,Y坐标,缺陷类型,大小,NgOk,相机序号,分区序号");
							}
						}
					}
					catch
					{
						LogHelper.WriteInfo("发生错误,联系工程师查看文件路径是否存在问题");
					}
					// 要添加的数据
					DateTime currentDate = DateTime.Now;
					string currentTime = DateTime.Now.ToString("HH:mm:ss");
					string id = productSheetNum;
					double xCoordinate = 0;
					double yCoordinate = 0;
					string Kind = "NULL";
					double Area = 0.00;
					string OkOrNg = "OK";

					// 将数据追加到文件
                    try
                    {
						using (StreamWriter writer = System.IO.File.AppendText(FilePath))
						{
							// 格式化数据并将其写入文件
							string dataLine = $"{currentDate.ToShortDateString()},{currentTime},{id},{xCoordinate},{yCoordinate},{Kind},{Area},{OkOrNg},{CamIndex},{sPartitionIndex}";
							writer.WriteLine(dataLine);
						}
					}
                    catch
                    {
						LogHelper.WriteInfo("发生错误,联系工程师查看文件路径是否存在问题1");
					}
					
				}
			}
            else
            {
				LogHelper.WriteInfo("报表路径有误，请联系工程师确认");
			}
           
			
		}
	    
            
		private void MainRun0()
        {
            while (true)
            {
                if (m_Pause)
                {
                    PlcSheetNum();
					Int16 startGrapImage = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.Trigger_Detection0).Content;

                    if(startGrapImage==1)
                    {
                       

                        HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[35], strDateTime);

                        try
                        {
                            if (IKapCLineCam0.m_hCameraResult)
                            {
                                ImageEvent[0].WaitOne();
                            }
                            if (Halcon.CamConnect[0])
                            {
                                hImage[0].Dispose();
                                //HOperatorSet.GrabImage(out hImage[0], Halcon.hv_AcqHandle[0]);              //同步采集
                                HOperatorSet.GrabImageAsync(out hImage[0], Halcon.hv_AcqHandle[0],-1);    //异步采集
                            }
                        }
                        catch 
                        {
                            HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion0, 2);
							LogHelper.WriteSingal("ID:"+ productSheetNum+"工位一输出信号--NG--地址："+ Parameters.plcParams.Completion0);

							Halcon.CamConnect[0]=false;
                            LogHelper.WriteError("线程1采图异常终止");
                            return;
                        }
						DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
						strDateTime = dtNow.ToString("yyyyMMddHHmmss");
						strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
						//productSN = HslCommunication._NetworkTcpDevice.ReadString(Parameters.plcParams.SNReadAdd, 2).Content;
						System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start(); //  开始监视代码运行时间
						LogHelper.WriteInfo("线程1开始");

						HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection0, 0);
                        bool DetectionResult = true;

                        HOperatorSet.GetImageSize(hImage[0], out Halcon.hv_Width[0],out Halcon.hv_Height[0]);
                        HOperatorSet.DispObj(hImage[0], hWindows0[0]);
                        HOperatorSet.SetPart(hWindows0[0], 0, 0, -1, -1);
						相机检测设置.set_display_font(hWindows0[0], 10, "mono", "true", "false");
						List<DetectionResult> detectionResults=new List<DetectionResult>();
                        相机检测设置.Detection(0, hWindows0, hImage[0],ref detectionResults,true);
						LogHelper.WriteInfo("线程1阶段1");

						this.Invoke((EventHandler)delegate 
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                hWindows0[i + 1].ClearWindow();
                                HOperatorSet.SetPart(hWindows0[i + 1], 0, 0, 1000, 1000);//设置窗体的规格
                            }
                            messageShow0.lab_Timer.Text = "";
                            messageShow0.lab_Column.Text = "";
                            messageShow0.lab_Row.Text = "";
                            messageShow0.lab_Size.Text = "";
                            messageShow0.lab_Kind.Text = "";
                            messageShow0.lab_Level.Text = "";
                            messageShow0.lab_Gray.Text = "";
                            messageShow1.lab_Timer.Text = "";
                            messageShow1.lab_Column.Text = "";
                            messageShow1.lab_Row.Text = "";
                            messageShow1.lab_Size.Text = "";
                            messageShow1.lab_Kind.Text = "";
                            messageShow1.lab_Level.Text = "";
                            messageShow1.lab_Gray.Text = "";

							if (detectionResults.Count == 0)
							{
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows0[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows0[0], "OK", "window", 12, 12, "black", "true");

								DetectionResult detectionResul = new DetectionResult();
								ProductReport("0", detectionResul, true, 29);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[5], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--OK--地址：" + Parameters.plcParams.预留地址[5]);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[10], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--OK--地址：" + Parameters.plcParams.预留地址[10]);
								LogHelper.WriteInfo("线程1阶段2");
							}

							if (detectionResults.Count != 0)
                            {
                                hWindows0[1].DispObj(detectionResults[0].NGAreahObject);
                                messageShow0.lab_Timer.Text = detectionResults[0].ResultdateTime.ToString();
                                messageShow0.lab_Column.Text = detectionResults[0].ResultXPosition.ToString();
                                messageShow0.lab_Row.Text = detectionResults[0].ResultYPosition.ToString();
                                messageShow0.lab_Size.Text = detectionResults[0].ResultSize.ToString();
                                messageShow0.lab_Kind.Text = detectionResults[0].ResultKind.ToString();
                                messageShow0.lab_Level.Text = detectionResults[0].ResultLevel.ToString();
                                messageShow0.lab_Gray.Text = detectionResults[0].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows0[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows0[0], "NG", "window", 12, 12, "black", "true");

								DetectionResult = false;

                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[5], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--NG--地址："+ Parameters.plcParams.预留地址[5]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[6], (int)detectionResults[0].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[7], (int)detectionResults[0].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[8], (int)detectionResults[0].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[9], (int)detectionResults[0].ResultLevel);
                            }
                            if (detectionResults.Count > 1)
                            {              
                                hWindows0[2].DispObj(detectionResults[1].NGAreahObject);
                                messageShow1.lab_Timer.Text = detectionResults[1].ResultdateTime.ToString();
                                messageShow1.lab_Column.Text = detectionResults[1].ResultXPosition.ToString();
                                messageShow1.lab_Row.Text = detectionResults[1].ResultYPosition.ToString();
                                messageShow1.lab_Size.Text = detectionResults[1].ResultSize.ToString();
                                messageShow1.lab_Kind.Text = detectionResults[1].ResultKind.ToString();
                                messageShow1.lab_Level.Text = detectionResults[1].ResultLevel.ToString();
                                messageShow1.lab_Gray.Text = detectionResults[1].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows0[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows0[0], "NG", "window", 12, 12, "black", "true");

								DetectionResult = false;

                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[10], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--NG--地址：" + Parameters.plcParams.预留地址[10]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[11], (int)detectionResults[1].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[12], (int)detectionResults[1].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[13], (int)detectionResults[1].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[14], (int)detectionResults[1].ResultLevel);
                            }
                        });

						LogHelper.WriteInfo("线程1阶段3");
						if (Parameters.specifications.SaveOrigalImage)
						{
							setCallBack = SaveImages;
							this.Invoke(setCallBack, 0, hImage[0], "IN-");
						}
						if (Parameters.specifications.SaveDefeatImage)
						{
							HOperatorSet.DumpWindowImage(out hObjectOut[0], hWindows0[0]);
							setCallBack = SaveImages;
							this.Invoke(setCallBack, 0, hObjectOut[0], "OUT-");
						}

						LogHelper.WriteInfo("线程1阶段4");
						if (DetectionResult)
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion0, 1);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--OK--地址：" + Parameters.plcParams.Completion0);
						}
						else
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion0, 2);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位一输出信号--NG--地址：" + Parameters.plcParams.Completion0);
						}

                        DateTime dtNowTwo = System.DateTime.Now;  // 获取系统当前时间
                        string strDateTimeTwo = dtNowTwo.ToString("yyyyMMddHHmmss");
                        HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[36], strDateTimeTwo);

                        stopwatch.Stop(); //  停止监视
                        TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                        double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
                        LogHelper.WriteInfo("检测1时间:" + milliseconds.ToString());
                    }
                    
                }
            }
        }
        private void MainRun1()
        {
            while (true)
            {
                if (m_Pause)
                {
                    PlcSheetNum();

					Int16 startGrapImage = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.Trigger_Detection1).Content;
                    if (startGrapImage == 1)
                    {
                       
                        try
                        {
                            if (IKapCLineCam1.m_hCameraResult)
                            {
                                ImageEvent[1].WaitOne();
                            }
                            if (Halcon.CamConnect[1])
                            {
                                hImage[1].Dispose();
                                //HOperatorSet.GrabImage(out hImage[1], Halcon.hv_AcqHandle[1]);              //同步采集
                                HOperatorSet.GrabImageAsync(out hImage[1], Halcon.hv_AcqHandle[1],-1);    //异步采集
                            }
                        }
                        catch
                        {
                            HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion1, 2);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--NG--地址：" + Parameters.plcParams.Completion1);
							Halcon.CamConnect[1] = false;
                            LogHelper.WriteError( "线程2采图异常终止");
                            return;
                        }
						DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
						strDateTime = dtNow.ToString("yyyyMMddHHmmss");
						HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[37], strDateTime);

						System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start(); //  开始监视代码运行时间
						LogHelper.WriteInfo("线程2开始");

						HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection1, 0);
                        bool DetectionResult = true;
                        HOperatorSet.GetImageSize(hImage[1], out Halcon.hv_Width[1],out Halcon.hv_Height[1]);
                        HOperatorSet.DispObj(hImage[1], hWindows1[0]);
                        HOperatorSet.SetPart(hWindows1[0], 0, 0, -1, -1);
						相机检测设置.set_display_font(hWindows1[0], 10, "mono", "true", "false");
						List<DetectionResult> detectionResults = new List<DetectionResult>();
                        相机检测设置.Detection(1, hWindows1, hImage[1], ref detectionResults,true);
						LogHelper.WriteInfo("线程2阶段1");

						this.Invoke((EventHandler)delegate
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                hWindows1[i + 1].ClearWindow();
                                HOperatorSet.SetPart(hWindows1[i + 1], 0, 0, 1000, 1000);//设置窗体的规格
                            }
                            messageShow2.lab_Timer.Text = "";
                            messageShow2.lab_Column.Text = "";
                            messageShow2.lab_Row.Text = "";
                            messageShow2.lab_Size.Text = "";
                            messageShow2.lab_Kind.Text = "";
                            messageShow2.lab_Level.Text = "";
                            messageShow2.lab_Gray.Text = "";
                            messageShow3.lab_Timer.Text = "";
                            messageShow3.lab_Column.Text = "";
                            messageShow3.lab_Row.Text = "";
                            messageShow3.lab_Size.Text = "";
                            messageShow3.lab_Kind.Text = "";
                            messageShow3.lab_Level.Text = "";
                            messageShow3.lab_Gray.Text = "";
							if (detectionResults.Count == 0)
							{
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows1[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows1[0], "OK", "window", 12, 12, "black", "true");
								DetectionResult detectionResul = new DetectionResult();
								ProductReport("1", detectionResul, true, 29);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[15], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--OK--地址：" + Parameters.plcParams.预留地址[15]);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[20], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--OK--地址：" + Parameters.plcParams.预留地址[20]);
								LogHelper.WriteInfo("线程2阶段2");
							}
							if (detectionResults.Count != 0)
                            {
                                hWindows1[1].DispObj(detectionResults[0].NGAreahObject);
                                messageShow2.lab_Timer.Text = detectionResults[0].ResultdateTime.ToString();
                                messageShow2.lab_Column.Text = detectionResults[0].ResultXPosition.ToString();
                                messageShow2.lab_Row.Text = detectionResults[0].ResultYPosition.ToString();
                                messageShow2.lab_Size.Text = detectionResults[0].ResultSize.ToString();
                                messageShow2.lab_Kind.Text = detectionResults[0].ResultKind.ToString();
                                messageShow2.lab_Level.Text = detectionResults[0].ResultLevel.ToString();
                                messageShow2.lab_Gray.Text = detectionResults[0].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows1[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows1[0], "NG", "window", 12, 12, "black", "true");

								DetectionResult = false;

                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[15], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--NG--地址：" + Parameters.plcParams.预留地址[15]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[16], (int)detectionResults[0].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[17], (int)detectionResults[0].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[18], (int)detectionResults[0].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[19], (int)detectionResults[0].ResultLevel);
                               
                            }
                            if (detectionResults.Count > 1)
                            {
                                hWindows1[1].DispObj(detectionResults[0].NGAreahObject);
                                messageShow3.lab_Timer.Text = detectionResults[1].ResultdateTime.ToString();
                                messageShow3.lab_Column.Text = detectionResults[1].ResultXPosition.ToString();
                                messageShow3.lab_Row.Text = detectionResults[1].ResultYPosition.ToString();
                                messageShow3.lab_Size.Text = detectionResults[1].ResultSize.ToString();
                                messageShow3.lab_Kind.Text = detectionResults[1].ResultKind.ToString();
                                messageShow3.lab_Level.Text = detectionResults[1].ResultLevel.ToString();
                                messageShow3.lab_Gray.Text = detectionResults[1].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows1[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows1[0], "NG", "window", 12, 12, "black", "true");

								DetectionResult = false;                           
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[20], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--NG--地址：" + Parameters.plcParams.预留地址[20]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[21], (int)detectionResults[1].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[22], (int)detectionResults[1].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[23], (int)detectionResults[1].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[24], (int)detectionResults[1].ResultLevel);

                            }
                        });
						LogHelper.WriteInfo("线程2阶段3");
						if (Parameters.specifications.SaveOrigalImage)
                        {
                            setCallBack = SaveImages;
                            this.Invoke(setCallBack, 1, hImage[1], "IN-");
                        }
                        if (Parameters.specifications.SaveOrigalImage)
                        {
                            HOperatorSet.DumpWindowImage(out hObjectOut[1], hWindows1[0]);
                            setCallBack = SaveImages;
                            this.Invoke(setCallBack, 1, hObjectOut[1], "OUT-");
                        }

						LogHelper.WriteInfo("线程2阶段4");
						if (DetectionResult)
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion1, 1);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--OK--地址：" + Parameters.plcParams.Completion1);
						}
						else
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion1, 2);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位二输出信号--NG--地址：" + Parameters.plcParams.Completion1);
						}

                        DateTime dtNowTwo = System.DateTime.Now;  // 获取系统当前时间
                        string strDateTimeTwo = dtNowTwo.ToString("yyyyMMddHHmmss");
                        HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[38], strDateTimeTwo);

                        stopwatch.Stop(); //  停止监视
                        TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                        double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
                        LogHelper.WriteInfo("检测2时间:" + milliseconds.ToString());
                    }
                }
            }
        }

        private void MainRun2()
        {
            while (true)
            {
                if (m_Pause)
                {
                    PlcSheetNum();

					Int16 startGrapImage = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.Trigger_Detection2).Content;
                    if (startGrapImage == 1)
                    {
                        try
                        {
                            if (IKapCLineCam2.m_hCameraResult)
                            {
                                ImageEvent[2].WaitOne();
                            }
                            if (Halcon.CamConnect[2])
                            {
                                hImage[2].Dispose();
                                //HOperatorSet.GrabImage(out hImage[2], Halcon.hv_AcqHandle[2]);              //同步采集
                                HOperatorSet.GrabImageAsync(out hImage[2], Halcon.hv_AcqHandle[2],-1);    //异步采集
                            }
                        }
                        catch
                        {
                            HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion2, 2);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--NG--地址：" + Parameters.plcParams.Completion2);
							Halcon.CamConnect[2] = false;
                            LogHelper.WriteError("线程3采图异常终止");
                            return;
                        }

						DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
						strDateTime = dtNow.ToString("yyyyMMddHHmmss");
						HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[39], strDateTime);

						System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start(); //  开始监视代码运行时间
						LogHelper.WriteInfo("线程3开始");

						HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection2, 0);
                        bool DetectionResult = true;
                        HOperatorSet.GetImageSize(hImage[2], out Halcon.hv_Width[2],out Halcon.hv_Height[2]);
                        HOperatorSet.DispObj(hImage[2], hWindows2[0]);
                        HOperatorSet.SetPart(hWindows2[0], 0, 0, -1, -1);
						相机检测设置.set_display_font(hWindows2[0], 10, "mono", "true", "false");
						List<DetectionResult> detectionResults = new List<DetectionResult>();
                        相机检测设置.Detection(2, hWindows2, hImage[2], ref detectionResults,true);
						LogHelper.WriteInfo("线程3阶段1");

						this.Invoke((EventHandler)delegate
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                hWindows2[i + 1].ClearWindow();
                                HOperatorSet.SetPart(hWindows2[i + 1], 0, 0, 1000, 1000);//设置窗体的规格
                            }

							messageShow4.lab_Timer.Text = "";
                            messageShow4.lab_Column.Text = "";
                            messageShow4.lab_Row.Text = "";
                            messageShow4.lab_Size.Text = "";
                            messageShow4.lab_Kind.Text = "";
                            messageShow4.lab_Level.Text = "";
                            messageShow4.lab_Gray.Text = "";
                            messageShow5.lab_Timer.Text = "";
                            messageShow5.lab_Column.Text = "";
                            messageShow5.lab_Row.Text = "";
                            messageShow5.lab_Size.Text = "";
                            messageShow5.lab_Kind.Text = "";
                            messageShow5.lab_Level.Text = "";
                            messageShow5.lab_Gray.Text = "";
							if (detectionResults.Count == 0)
                            {
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows2[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows2[0], "OK", "window", 12, 12, "black", "true");
								DetectionResult detectionResul = new DetectionResult();
								ProductReport("2", detectionResul, true, 29);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[25], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--OK--地址：" + Parameters.plcParams.预留地址[25]);

								HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[30], 1);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--OK--地址：" + Parameters.plcParams.预留地址[30]);
								LogHelper.WriteInfo("线程3阶段2");
							}
							if (detectionResults.Count != 0)
                            {
                                hWindows2[1].DispObj(detectionResults[0].NGAreahObject);
                                messageShow4.lab_Timer.Text = detectionResults[0].ResultdateTime.ToString();
                                messageShow4.lab_Column.Text = detectionResults[0].ResultXPosition.ToString();
                                messageShow4.lab_Row.Text = detectionResults[0].ResultYPosition.ToString();
                                messageShow4.lab_Size.Text = detectionResults[0].ResultSize.ToString();
                                messageShow4.lab_Kind.Text = detectionResults[0].ResultKind.ToString();
                                messageShow4.lab_Level.Text = detectionResults[0].ResultLevel.ToString();
                                messageShow0.lab_Gray.Text = detectionResults[0].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows2[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows2[0], "NG", "window", 12, 12, "black", "true");

								DetectionResult = false;
                                
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[25], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--NG--地址：" + Parameters.plcParams.预留地址[25]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[26], (int)detectionResults[0].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[27], (int)detectionResults[0].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[28], (int)detectionResults[0].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[29], (int)detectionResults[0].ResultLevel);

                               
                            }
                            if (detectionResults.Count > 1)
                            {                               
                                hWindows2[2].DispObj(detectionResults[1].NGAreahObject);
                                messageShow5.lab_Timer.Text = detectionResults[1].ResultdateTime.ToString();
                                messageShow5.lab_Column.Text = detectionResults[1].ResultXPosition.ToString();
                                messageShow5.lab_Row.Text = detectionResults[1].ResultYPosition.ToString();
                                messageShow5.lab_Size.Text = detectionResults[1].ResultSize.ToString();
                                messageShow5.lab_Kind.Text = detectionResults[1].ResultKind.ToString();
                                messageShow5.lab_Level.Text = detectionResults[1].ResultLevel.ToString();
                                messageShow5.lab_Gray.Text = detectionResults[1].ResultGray.ToString();
								HOperatorSet.SetWindowAttr("background_color", "black");
								相机检测设置.set_display_font(hWindows2[0], 67, "mono", "true", "false");
								相机检测设置.disp_message(hWindows2[0], "NG", "window", 12, 12, "black", "true");
								DetectionResult = false;
                               
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[30], 2);
								LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--NG--地址：" + Parameters.plcParams.预留地址[30]);
								
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[31], (int)detectionResults[1].ResultSize);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[32], (int)detectionResults[1].ResultXPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[33], (int)detectionResults[1].ResultYPosition);
                                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[34], (int)detectionResults[1].ResultLevel);                              
                            }
                        });
						LogHelper.WriteInfo("线程3阶段3");
						if (Parameters.specifications.SaveOrigalImage)
                        {
                            setCallBack = SaveImages;
                            this.Invoke(setCallBack, 2, hImage[2], "IN-");
                        }
                        if (Parameters.specifications.SaveDefeatImage)
                        {
                            HOperatorSet.DumpWindowImage(out hObjectOut[2], hWindows2[0]);
                            setCallBack = SaveImages;
                            this.Invoke(setCallBack, 2, hObjectOut[2], "OUT-");
                        }

						if (DetectionResult)
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion2, 1);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--OK--地址：" + Parameters.plcParams.Completion2);
						}
						else
						{
							HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Completion2, 2);
							LogHelper.WriteSingal("ID:" + productSheetNum + "工位三输出信号--NG--地址：" + Parameters.plcParams.Completion2);
						}
						LogHelper.WriteInfo("线程3阶段4");
						DateTime dtNowTwo = System.DateTime.Now;  // 获取系统当前时间
                        string strDateTimeTwo = dtNowTwo.ToString("yyyyMMddHHmmss");
                        HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[40], strDateTimeTwo);

                        stopwatch.Stop(); //  停止监视
                        TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                        double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数           
                        LogHelper.WriteInfo("检测3时间:" + milliseconds.ToString());
                    }
                }
            }
        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定关闭程序吗？", "软件关闭提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                XMLHelper.serialize<Parameters.Counts>(Parameters.counts, "Parameter/CountsParams.xml");
                //Parameter.specifications.右短端.value = 10;
                //XMLHelper.serialize<Parameter.Specifications>(Parameter.specifications, "Specifications.xml");
                myThread.Abort();


                LogHelper.WriteInfo(System.DateTime.Now.ToString() + "软件关闭。");
                this.Close();
            }
        }

        private void btn_Minimizid_System_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            LogHelper.WriteInfo(System.DateTime.Now.ToString() + "窗体最小化。");
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();//隐藏主窗体  
                LogHelper.WriteInfo(System.DateTime.Now.ToString() + "主窗体隐藏。");              
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//当鼠标点击为左键时  
            {
                this.Show();//显示主窗体  
                LogHelper.WriteInfo(System.DateTime.Now.ToString() + "主窗体恢复。");
                this.WindowState = FormWindowState.Normal;//主窗体的大小为默认  
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
			if (Parameters.commministion.PlcEnable)
            {
                hslCommunication = new HslCommunication();
                Thread.Sleep(1000);
                if (HslCommunication.plc_connect_result)
                {
                    lab_PLCStatus.Text = "在线";
                    lab_PLCStatus.BackColor = Color.Green;
                }
                else
                {
                    lab_PLCStatus.Text = "离线";
                    lab_PLCStatus.BackColor = Color.Red;
                }
            }
            else
            {
                lab_PLCStatus.Text = "禁用";
                lab_PLCStatus.BackColor = Color.Gray;
            }

            if (Parameters.commministion.TcpClientEnable)
            {
                TcpClient tcpClientr = new TcpClient();
                Thread.Sleep(1000);
                if (TcpClient.TcpClientConnectResult)
                {
                    lab_Client.Text = "在线";
                    lab_Client.BackColor = Color.Green;
                }
                else
                {
                    lab_Client.Text = "等待";
                    lab_Client.BackColor = Color.Red;
                }
            }
            else
            {
                lab_Client.Text = "禁用";
                lab_Client.BackColor = Color.Gray;
            }

            if (Parameters.commministion.TcpServerEnable)
            {
                TcpServer tcpServer = new TcpServer();
                Thread.Sleep(1000);
                if (TcpServer.TcpServerConnectResult)
                {
                    lab_Server.Text = "在线";
                    lab_Server.BackColor = Color.Green;
                }
                else
                {
                    lab_Server.Text = "等待";
                    lab_Server.BackColor = Color.Red;
                }
            }
            else
            {
                lab_Server.Text = "禁用";
                lab_Server.BackColor = Color.Gray;
            }
            lab_Product.Text = Parameters.commministion.productName;
			if (System.IO.File.Exists("fumi.txt"))
			{
				//存在 
				UserLevel = 3;
			}
			else
			{
				//不存在 
				UserLevel = 1;
			}
			UpdataUI();
            HOperatorSet.SetPart(hWindows0[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows0[1], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows0[2], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.DispObj(hImage[0], hWindows0[0]);

            HOperatorSet.SetPart(hWindows1[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows1[1], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows1[2], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.DispObj(hImage[1], hWindows1[0]);

            HOperatorSet.SetPart(hWindows2[0], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows2[1], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.SetPart(hWindows2[2], 0, 0, -1, -1);//设置窗体的规格
            HOperatorSet.DispObj(hImage[2], hWindows2[0]);

			Halcon.CamConnect[0] = Halcon.initalCamera("LineCam0", ref Halcon.hv_AcqHandle[0]);
			if (Halcon.CamConnect[0])
			{
				Halcon.SetFramegrabberParam(0, Halcon.hv_AcqHandle[0]);
			}
			Halcon.CamConnect[1] = Halcon.initalCamera("LineCam1", ref Halcon.hv_AcqHandle[1]);
			if (Halcon.CamConnect[1])
			{
				Halcon.SetFramegrabberParam(1, Halcon.hv_AcqHandle[1]);

			}
			Halcon.CamConnect[2] = Halcon.initalCamera("LineCam2", ref Halcon.hv_AcqHandle[2]);
			if (Halcon.CamConnect[2])
			{
				Halcon.SetFramegrabberParam(2, Halcon.hv_AcqHandle[2]);
			}

			DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
			strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
			timer1.Enabled = true;

            if (IKapCLineCam0.m_hCameraResult)
            {
                IKapCLineCam0.StartGrabImage(iKapCLineCam0);
            }
            if (IKapCLineCam1.m_hCameraResult)
            {
                IKapCLineCam1.StartGrabImage(iKapCLineCam1);
            }
            if (IKapCLineCam2.m_hCameraResult)
            {
                IKapCLineCam2.StartGrabImage(iKapCLineCam2);
            }
            LogHelper.WriteInfo(System.DateTime.Now.ToString() + "初始化完成");
        }

        #region 点击panel控件移动窗口
        System.Drawing.Point downPoint;
        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            downPoint = new System.Drawing.Point(e.X, e.Y);
        }
        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new System.Drawing.Point(this.Location.X + e.X - downPoint.X,
                    this.Location.Y + e.Y - downPoint.Y);
            }
        }
        #endregion
        private void btn_SettingMean_Click(object sender, EventArgs e)
        {
            通讯设置 flg = new 通讯设置();
            flg.ShowDialog();
        }

        //    Directory.CreateDirectory(string path);//在指定路径中创建所有目录和子目录，除非已经存在
        //    Directory.Delete(string path);//从指定路径删除空目录
        //    Directory.Delete(string path, bool recursive);//布尔参数为true可删除非空目录
        //    Directory.Exists(string path);//确定路径是否存在
        //    Directory.GetCreationTime(string path);//获取目录创建日期和时间
        //    Directory.GetCurrentDirectory();//获取应用程序当前的工作目录
        //    Directory.GetDirectories(string path);//返回指定目录所有子目录名称，包括路径
        //    Directory.GetFiles(string path);//获取指定目录中所有文件的名称，包括路径
        //    Directory.GetFileSystemEntries(string path);//获取指定路径中所有的文件和子目录名称
        //    Directory.GetLastAccessTime(string path);//获取上次访问指定文件或目录的时间和日期
        //    Directory.GetLastWriteTime(string path);//返回上次写入指定文件或目录的时间和日期
        //    Directory.GetParent(string path);//检索指定路径的父目录，包括相对路径和绝对路径
        //    Directory.Move(string soureDirName, string destName);//将文件或目录及其内容移到新的位置
        //    Directory.SetCreationTime(string path);//为指定的目录或文件设置创建时间和日期
        //    Directory.SetCurrentDirectory(string path);//将应用程序工作的当前路径设为指定路径
        //    Directory.SetLastAccessTime(string path);//为指定的目录或文件设置上次访问时间和日期
        //    Directory.SetLastWriteTime(string path);//为指定的目录和文件设置上次访问时间和日期


        private void btn_Start_Click(object sender, EventArgs e)
        {
			timer1.Enabled = false;
            UserLevel = 1;

			if (Halcon.CamConnect[0])
			{
				Halcon.SetFramegrabberParam(0, Halcon.hv_AcqHandle[0]);
			}
			if (Halcon.CamConnect[1])
			{
				Halcon.SetFramegrabberParam(1, Halcon.hv_AcqHandle[1]);
			}
			if (Halcon.CamConnect[2])
			{
				Halcon.SetFramegrabberParam(2, Halcon.hv_AcqHandle[2]);
			}

			if (HslCommunication.plc_connect_result)
            {
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.StartAdd, 1);              
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection0, 0);
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection1, 0);
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.Trigger_Detection2, 0);
            }
            else
            {
                MessageBox.Show("PLC链接异常，请检查!");
                return;
            }

            if (Halcon.CamConnect[0] && Halcon.CamConnect[1] && Halcon.CamConnect[2])
            {
				HOperatorSet.GrabImageStart(Halcon.hv_AcqHandle[0], -1);
				HOperatorSet.GrabImageStart(Halcon.hv_AcqHandle[1], -1);
				HOperatorSet.GrabImageStart(Halcon.hv_AcqHandle[2], -1);
			}
            else if (IKapCLineCam0.m_hCameraResult && IKapCLineCam1.m_hCameraResult && IKapCLineCam1.m_hCameraResult)
            {

            }
            else
            {
                MessageBox.Show("相机链接异常，请检查!");
                //return;
            }
			productSN = HslCommunication.STRproduct;
			btn_Start.FillColor = Color.Red;

			MainThread[0] = new Thread(MainRun0);
            MainThread[0].IsBackground = true;
            MainThread[0].Start();
            MainThread[1] = new Thread(MainRun1);
            MainThread[1].IsBackground = true;
            MainThread[1].Start();
            MainThread[2] = new Thread(MainRun2);
            MainThread[2].IsBackground = true;
            MainThread[2].Start();
            m_Pause = true;
            Permission = "访客";
            UpdataUI();
            btn_Start.Enabled = false;
            btn_Stop.Enabled = true;
            btn_Connutius.Enabled = true;
            btn_Connect.Enabled = false;
            btn_SpecicationSetting.Enabled = false;
            btn_Login.Enabled = false;
            btn_Cam1.Enabled = false;
            btn_Cam2.Enabled = false;
            btn_Cam3.Enabled = false;
            btn_Close_System.Enabled = false;
            uiButton1.Enabled = true;
		}


        private void btn_Stop_Click(object sender, EventArgs e)
        {
			timer1.Enabled = true;
			for (int i = 0; i < 3; i++)
            {
                if (MainThread[i] != null)
                {
                    MainThread[i].Abort();
                }
            }
            if (Halcon.CamConnect[0])
            {
                Halcon.CamConnect[0] = Halcon.CloseFramegrabber(Halcon.hv_AcqHandle[0]);
                Halcon.CamConnect[0] = Halcon.initalCamera("LineCam0", ref Halcon.hv_AcqHandle[0]);

            }
            else
            {
                MessageBox.Show("相机1链接异常，请检查!");
                return;
            }

            if (Halcon.CamConnect[1])
            {
                Halcon.CamConnect[1] = Halcon.CloseFramegrabber(Halcon.hv_AcqHandle[1]);
                Halcon.CamConnect[1] = Halcon.initalCamera("LineCam1", ref Halcon.hv_AcqHandle[1]);
            }
            else
            {
                MessageBox.Show("相机2链接异常，请检查!");
                return;
            }

            if (Halcon.CamConnect[2])
            {
                Halcon.CamConnect[2] = Halcon.CloseFramegrabber(Halcon.hv_AcqHandle[2]);
                Halcon.CamConnect[2] = Halcon.initalCamera("LineCam2", ref Halcon.hv_AcqHandle[2]);
            }
            else
            {
                MessageBox.Show("相机3链接异常，请检查!");
                return;
            }
            if (HslCommunication.plc_connect_result)
            {
                OperateResult result = HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.StartAdd, 0);
                LogHelper.WriteWarn("PLC写入" + result.Message);
                btn_Start.Enabled = true;
                btn_Stop.Enabled = false;
                btn_Connutius.Enabled = false;
                btn_Connect.Enabled = false;
                btn_SpecicationSetting.Enabled = false;
                //btn_Login.Enabled = true;
                btn_Cam1.Enabled = false;
                btn_Cam2.Enabled = false;
                btn_Cam3.Enabled = false;
                btn_Close_System.Enabled = true;
				btn_Start.FillColor = Color.Black;

				UpdataUI();
			}
            else
            {
                MessageBox.Show("链接异常，请检查!");
            }
        }

        private void btn_Connutius_Click(object sender, EventArgs e)
        {
            if (btn_Connutius.Text == "暂停")
            {
                m_Pause = false;
                btn_Connutius.Text = "继续";
            }
            else
            {
                m_Pause = true;
                btn_Connutius.Text = "暂停";
            }

        }
      
        private void btn_检测设置1_Click(object sender, EventArgs e)
        {
            CamNum = 0;
            相机检测设置 flg = new 相机检测设置();
            flg.ShowDialog();
        }

        private void btn_检测设置2_Click(object sender, EventArgs e)
        {
            CamNum = 1;
            相机检测设置 flg = new 相机检测设置();
            flg.ShowDialog();
        }

        private void btn_检测设置3_Click(object sender, EventArgs e)
        {
            CamNum = 2;
            相机检测设置 flg = new 相机检测设置();
            flg.ShowDialog();
        }


        public static string Permission = "访客";
        void User_TransfEvent(string value)
        {
            Permission = value;
        }
        private void UpdataUI()
        {
            if (UserLevel==1)
            {
				btn_Start.Enabled = true;
				btn_Stop.Enabled = false;
				btn_Connutius.Enabled = true;
				//btn_Login.Enabled = true;
				btn_Connect.Enabled = false;
				btn_SpecicationSetting.Enabled = false;
				btn_Cam1.Enabled = false;
				btn_Cam2.Enabled = false;
				btn_Cam3.Enabled = false;
				btn_Close_System.Enabled = true;
				btn_Minimizid_System.Enabled = true;
				lab_User.Text ="操作员";
				//btn_changeProduct.Enabled = true;
				uiButton1.Enabled = true;
			}
            else if (UserLevel==2)
            {
				btn_Start.Enabled = true;
				btn_Stop.Enabled = false;
				btn_Connutius.Enabled = true;
				//btn_Login.Enabled = true;
				btn_Connect.Enabled = false;
				btn_SpecicationSetting.Enabled = false;
				btn_Cam1.Enabled = true;
				btn_Cam2.Enabled = true;
				btn_Cam3.Enabled = true;
				btn_Close_System.Enabled = true;
				btn_Minimizid_System.Enabled = true;
				lab_User.Text = "工程助理";
				//btn_changeProduct.Enabled = true;
				uiButton1.Enabled = true;

		    }
            else if (UserLevel==3)
            {
				btn_Start.Enabled = true;
				btn_Stop.Enabled = false;
				btn_Connutius.Enabled = true;
				//btn_Login.Enabled = true;
				btn_Connect.Enabled = true;
				btn_SpecicationSetting.Enabled = true;
				btn_Cam1.Enabled = true;
				btn_Cam2.Enabled = true;
				btn_Cam3.Enabled = true;
				btn_Close_System.Enabled = true;
				btn_Minimizid_System.Enabled = true;
				lab_User.Text = "工程师";
				//btn_changeProduct.Enabled = true;
				uiButton1.Enabled = true;
			}
            else if(UserLevel == 6)
            {
				btn_Start.Enabled = true;
				btn_Stop.Enabled = false;
				btn_Connutius.Enabled = true;
				//btn_Login.Enabled = true;
				btn_Connect.Enabled = true;
				btn_SpecicationSetting.Enabled = true;
				btn_Cam1.Enabled = true;
				btn_Cam2.Enabled = true;
				btn_Cam3.Enabled = true;
				btn_Close_System.Enabled = true;
				btn_Minimizid_System.Enabled = true;
				lab_User.Text = "厂商";
				//btn_changeProduct.Enabled = true;
				uiButton1.Enabled = true;
			}
            else
            {
				btn_Start.Enabled = false;
				btn_Stop.Enabled = false;
				btn_Connutius.Enabled = false;
				//btn_Login.Enabled = true;
				btn_Connect.Enabled = false;
				btn_SpecicationSetting.Enabled = false;
				btn_Cam1.Enabled = false;
				btn_Cam2.Enabled = false;
				btn_Cam3.Enabled = false;
				btn_Close_System.Enabled = false;
				btn_Minimizid_System.Enabled = false;
				lab_User.Text = "无权限";
				//btn_changeProduct.Enabled = true;
				uiButton1.Enabled = false;
			}
            //if (Permission == "开发员")
            //{
                
            //}
            //else if (Permission == "管理员")
            //{
            //    btn_Start.Enabled = true;
            //    btn_Stop.Enabled = false;
            //    btn_Connutius.Enabled = true;
            //    btn_Login.Enabled = true;
            //    btn_Connect.Enabled = true;
            //    btn_SpecicationSetting.Enabled = true;
            //    btn_Cam1.Enabled = true;
            //    btn_Cam2.Enabled = true;
            //    btn_Cam3.Enabled = true;
            //    btn_Close_System.Enabled = true;
            //    btn_Minimizid_System.Enabled = true;
            //    lab_User.Text = Permission;
            //     btn_changeProduct.Enabled = true;
            //}
            //else if (Permission == "操作员")
            //{
            //    btn_Start.Enabled = false;
            //    btn_Stop.Enabled = false;
            //    btn_Connutius.Enabled = false;
            //    btn_Login.Enabled = true;
            //    btn_Connect.Enabled = false;
            //    btn_SpecicationSetting.Enabled = false;
            //    btn_Cam1.Enabled = false;
            //    btn_Cam2.Enabled = false;
            //    btn_Cam3.Enabled = false;
            //    btn_Close_System.Enabled = false;
            //    btn_Minimizid_System.Enabled = false;
            //    lab_User.Text = Permission;
            //    btn_changeProduct.Enabled = false;
            //}
            //else
            //{
            //    btn_Start.Enabled = true;
            //    btn_Stop.Enabled = false;
            //    btn_Connutius.Enabled = true;
            //    btn_Connect.Enabled = false;
            //    btn_SpecicationSetting.Enabled = false;
            //    btn_Login.Enabled = true;
            //    btn_Cam1.Enabled = false;
            //    btn_Cam2.Enabled = false;
            //    btn_Cam3.Enabled = false;
            //    btn_Close_System.Enabled = false;
            //    btn_Minimizid_System.Enabled = false;
            //    lab_User.Text = Permission;
            //    btn_changeProduct.Enabled = false;
            //}
        }

		public  void Jurisdiction()
		{
            if (HslCommunication.plc_connect_result)
            {
				Int16 PlcSignal = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[3]).Content;
				String sPlcSignal = HslCommunication._NetworkTcpDevice.ReadString(Parameters.plcParams.预留地址[4], 20).Content;

				if (System.IO.File.Exists("fumi.txt"))
				{
					//存在 
					PlcSignal = 3;
				}
				else
				{
					//不存在 
					PlcSignal = 1;
				}

				if (sPlcSignal != "" && sPlcSignal != "0" && sPlcSignal != null)
				{
					UserName = sPlcSignal.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
				}
				else
				{
					LogHelper.WriteWarn("无相关人员操作信息");
					UserName = "无";
				}

				if (PlcSignal == 1)
				{
					if (PlcSignal != UserLevel)
					{
						UserLevel = PlcSignal;
						UpdataUI();
						LogHelper.WriteWarn("操作员：" + UserName + "登录");
					}
				}
				else if (PlcSignal == 2)
				{
					if (PlcSignal !=UserLevel)
					{
						UserLevel = PlcSignal;
						UpdataUI();
						LogHelper.WriteWarn("工程助理：" + UserName + "登录");
					}
				}
				else if (PlcSignal == 3)
				{
					if (PlcSignal !=UserLevel)
					{
						UserLevel = PlcSignal;
						UpdataUI();
						LogHelper.WriteWarn("工程师：" + UserName + "登录");
					}
				}
				else
				{
					if (PlcSignal != UserLevel)
					{
						UserLevel = PlcSignal;
						UpdataUI();
						LogHelper.WriteWarn("厂商：" + UserName + "登录");
					}
				}
			}
			
		}

		private void btn_Login_Click(object sender, EventArgs e)
        {
            登陆界面 flg = new 登陆界面();
            flg.TransfEvent += User_TransfEvent;
            flg.ShowDialog();
            UpdataUI();
        }
        public static string Product = Parameters.commministion.productName;
        void Product_TransfEvent(string value)
        {
            Product = value;
        }
        private void btn_changeProduct_Click(object sender, EventArgs e)
        {
            切换产品 flg = new 切换产品();
            flg.TransfEvent += Product_TransfEvent;
            flg.ShowDialog();
            lab_Product.Text = Product;
        }

        private void btn_SpecicationSetting_Click(object sender, EventArgs e)
        {
			FormCamera flg = new FormCamera();
			flg.ShowDialog();
		}

        private void uiButton1_Click(object sender, EventArgs e)
        {
			CamNum = 0;
			复检1 flg = new 复检1();
			flg.ShowDialog();

		}

		private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
		{

		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			Jurisdiction();
		}
	}
}
