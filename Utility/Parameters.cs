using HalconDotNet;
using System;

namespace WY_App.Utility
{
    public class Parameters
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public enum LogLevelEnum
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3,
            Fatal = 4
        }

        public enum MeanImageEnum
        {
            直方图均值化 = 0,
            增强对比度 = 1,
            均值滤波 = 2,
            中值滤波 = 3,
            高斯滤波 = 4,
            无滤波处理 = 5
        }
        public enum ImageErrorKind
        {
            气泡 = 0,
            黑点 = 1,
            异物 = 2,
            裂纹 = 3,
            缺口 = 4,
            刮伤 = 5,
            其他 = 6
        }

        public class CameraParam
        {
            /// <summary>
            /// 采集卡
            /// </summary>
            public string[] CamInterface = new string[3];
            /// <summary>
            /// 相机ID
            /// </summary>
            public string[] CamDeviceID = new string[3];
            /// <summary>
            /// 相机曝光
            /// </summary>
            public double[] CamShutter = new double[3];
            /// <summary>
            /// 相机补偿
            /// </summary>
            public double[] CamGain = new double[3];
            /// <summary>
            /// 相机软触发
            /// </summary>
            public bool CamSoftwareMode;

            /// <summary>
            /// 相机触发采集模式/连续采集模式
            /// </summary>
            public bool CamContinuesMode;

            public CameraParam()
            {
                CamSoftwareMode = false;
                CamContinuesMode = false;
                for (int i = 0; i < 3; i++)
                {
                    CamInterface[i] = "采集卡" + i;
                    CamDeviceID[i] = "Cam" + i;
                    CamShutter[i] = 1000;
                    CamGain[i] = 1;
                }
            }
        }
        public static CameraParam cameraParam = new CameraParam();
        public struct Rect1
        {
            public double Row1;           
            public double Colum1;
            public double Row2;
            public double Colum2;
        }
        public struct DetectionResult
        {
            public DateTime ResultdateTime;
            public double ResultXPosition;
            public double ResultYPosition;
            public double ResultSize;
            public ImageErrorKind ResultKind;
            public int ResultLevel;
            public double ResultGray;
            public double ResultWidth;
            public double ResultHeight;
            public double ResultRa;
            public double ResultRb;
            public double ResultBlue;
            public HObject NGAreahObject;
        }
       
        public struct HRect1
        {
            public HTuple Row1;
            public HTuple Colum1;
            public HTuple Row2;
            public HTuple Colum2;
        }

        public struct Rect2
        {
            public double Row;
            public double Colum;
            public double Phi;
            public double Length1;
            public double Length2;
            public double 阈值;
            public string 极性;
            public double simga;
        }
        public class DetectionSpec
        {
            /// <summary>
            ///Y方向放大比例
            /// </summary>
            public double PixelResolutionRow;
            //public bool doubleYBaseEnabled;
            /// <summary>
            ///X方向放大比例
            /// </summary>
            public double PixelResolutionColum;
            public double[] RowBase = new double[4];
            public double[] ColumBase = new double[4];

            public double[] Row1 = new double[4];
            public double[] Colum1 = new double[4];
            public double[] Row2 = new double[4];
            public double[] Colum2 = new double[4];
            public uint[] OffSet = new uint[4];

            public uint[] MeasureLength1 = new uint[4];
            public uint[] MeasureLength2 = new uint[4];
            public double[] MeasureSigma = new double[4];
            public uint[] MeasureThreshold = new uint[4];
            public string[] MeasureTransition = new string[4];

            public double[] lengthWidthRatio = new double[32];
            public double[] min = new double[32];
            public double[] max = new double[32];
            public double[] adjust = new double[32];
            public double[] ThresholdLow = new double[32];
            public double[] ThresholdHigh = new double[32];
            public double[] AreaLow = new double[32];
            public double[] AreaHigh = new double[32];

            public DetectionSpec()
            {
                PixelResolutionRow = 1;
                PixelResolutionColum = 1;
                for (int i = 0; i < 4; i++)
                {
                    RowBase[i] = 0;
                    ColumBase[i] = 0;
                    Row1[i] = i;
                    Colum1[i] = i;
                    Row2[i] = 500;
                    Colum2[i] = 1000;
                    OffSet[i] = 0;
                    MeasureLength1[i] = 1000;
                    MeasureLength2[i] = 5;
                    MeasureSigma[i] = 3;
                    MeasureThreshold[i] = 40;
                    MeasureTransition[i] = "all";
                }

                for (int i = 0; i < 24; i++)
                {
                    lengthWidthRatio[i] = 0;
                    max[i] = 0;
                    min[i] = 0;
                    adjust[i] = 0;
                    ThresholdLow[i] = 0;
                    ThresholdHigh[i] = 0;
                    AreaLow[i] = 0;
                    AreaHigh[i] = 0;
                }
            }
        }

        public static DetectionSpec[] detectionSpec = new DetectionSpec[3];
        
        public struct rent2
        {
            public double Column;
            public double Row;
            public double Phi;
            public double Length1;
            public double Length2;
            public double Bettween;
            public double Measure_num;
            public double sigma;
            public double threshold;
            public string polarity;
            public double x_shift;
            public double y_shift;
        }

        public class Rent2
        {
            public rent2 rent2;

            public Rent2()
            {
                rent2.Column = 100;
                rent2.Row = 100;
                rent2.Phi = 0;
                rent2.Length1 = 10;
                rent2.Length2 = 20;
                rent2.Bettween = 93;
                rent2.Measure_num = 10;
                rent2.sigma = 1;
                rent2.threshold = 20;
                rent2.polarity = "all";
                rent2.x_shift = 0;
                rent2.y_shift = 0;
            }
        }
        public class Specifications
        {
            public bool SaveOrigalImage;

            public bool SaveDefeatImage;

            public bool SaveCropImage;

            public int CropImagelength;

            public bool MeanImageEnabled;

			public int ImageWidth;
			public int ImageHeigth;

			public int meanImageEnum { get; set; }

            public Specifications()
            {
                SaveOrigalImage = false;
                SaveDefeatImage = false;
                SaveCropImage = false;
                CropImagelength = 500;
                MeanImageEnabled = false;
                meanImageEnum = 0;

				ImageWidth = 1000;
				ImageHeigth = 800;
			}
        }
        public static Specifications specifications = new Specifications();

        public class Commministion
        {
            /// <summary>
            /// 当前保存日志等级
            /// </summary>
            public LogLevelEnum LogLevel;

            /// <summary>
            /// 日志存放路径
            /// </summary>
            public string LogFilePath;

            /// <summary>
            /// 日志存放天数
            /// </summary>
            public int LogFileExistDay;

            /// <summary>
            /// plc启用标志
            /// </summary>
            public bool PlcEnable;

            /// <summary>
            /// plc型号
            /// </summary>
            public string PlcType;

            /// <summary>
            /// plc ip地址
            /// </summary>
            public string PlcIpAddress;

            /// <summary>
            /// plc ip端口号
            /// </summary>
            public int PlcIpPort;

            /// <summary>
            /// plc 站号/型号
            /// </summary>
            public string PlcDevice;

            /// <summary>
            /// Tcp客户端启用标志
            /// </summary>
            public bool TcpClientEnable;

            /// <summary>
            /// tcp ip地址
            /// </summary>
            public string TcpClientIpAddress;

            /// <summary>
            /// tcp ip端口号
            /// </summary>
            public int TcpClientIpPort;

            /// <summary>
            /// Tcp服务端启用标志
            /// </summary>
            public bool TcpServerEnable;

            /// <summary>
            /// tcp服务器 ip地址
            /// </summary>
            public string TcpServerIpAddress;

            /// <summary>
            /// tcp服务器 ip端口号
            /// </summary>
            public int TcpServerIpPort;
            /// <summary>
            /// path
            /// </summary>
            public string ImagePath;

            /// <summary>
            /// path
            /// </summary>
            public string ImageSavePath;

            /// <summary>
            /// path
            /// </summary>
            public string productName;

            /// <summary>
            /// path
            /// </summary>
            public string DeviceID;
            /// <summary>
            /// 联机参数设置
            /// </summary>
            public Commministion()
            {
                //PLC启用标志
                PlcEnable = false;
                //--PLC参数设置--
                //--欧姆龙Omron.OmronFinsNet--
                //--西门子Siemens.SiemensS7Net--
                //--三菱Melsec.MelsecMcNet--
                //--汇川Inovance.InovanceSerialOverTcp--
                //--ModbusTcp

                //PLC 类型
                PlcType = "Omron.OmronFinsNet";
                //PLC 地址
                PlcIpAddress = "127.0.0.1";
                //PLC站号/型号
                PlcDevice = "200";
                //PLC 端口号
                PlcIpPort = 9600;

                //--TCP客户端参数设置--
                //Tcp客户端启用标志
                TcpClientEnable = true;
                //TCP 客户端 地址
                TcpClientIpAddress = "127.0.0.1";
                //TCP 客户端 端口号
                TcpClientIpPort = 9600;

                //--TCP服务器参数设置--
                //Tcp服务器启用标志
                TcpServerEnable = false;
                //TCP服务器 地址
                TcpServerIpAddress = "127.0.0.1";
                //TCP服务器 端口号
                TcpServerIpPort = 9600;

                ImagePath = @"D:\VisionDetect\InspectImage\";
                ImageSavePath = @"D:\Image\";
                productName = "55";
                DeviceID = "";
;           }
        }

        public static Commministion commministion = new Commministion();

        public class PLCParams
        {
            public string Trigger_Detection0;
            public string Completion0;
            public string Trigger_Detection1;
            public string Completion1;
            public string Trigger_Detection2;
            public string Completion2;
            public string HeartBeatAdd;
            public string StartAdd;
            public string SNReadAdd;
            public string[] 预留地址 = new string[200];
           
            public PLCParams()
            {
                HeartBeatAdd = "D100";
                HeartBeatAdd = "D102";
                Trigger_Detection0 = "D104";
                Completion0 = "D106";
                Trigger_Detection1 = "D108";
                Completion1 = "D110";
                Trigger_Detection2 = "D112";
                Completion2 = "D114";
                SNReadAdd = "D116";
                for (int i = 0; i < 200; i++)
                {
                    预留地址[i] = "D" + i * 2 + 200;
                }
            }
        }

        public class Counts
        {
            public int[] Count = new int[10];

            public Counts()
            {
              
                
            }
        }

        public static PLCParams plcParams = new PLCParams();
        public static Counts counts = new Counts();
    }
}