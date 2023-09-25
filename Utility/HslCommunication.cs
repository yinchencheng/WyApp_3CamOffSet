using System;
using System.Threading;
using HslCommunication;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Inovance;
using WY_App.Utility;
using HslCommunication.Core;
using System.IO;

using HslCommunication.ModBus;
//power pmac配置
using ODT.PowerPmacComLib;
using ODT.Common.Services;
using ODT.Common.Core;
using System.Windows.Forms;
using System.Collections.Generic;
using static WY_App.Utility.Parameters;
using static OpenCvSharp.FileStorage;
using static System.Collections.Specialized.BitVector32;
using System.Xml;

namespace WY_App
{
    internal class HslCommunication
    {
        public static OperateResult _connected ;
        public static NetworkDeviceBase _NetworkTcpDevice;
        static ISyncGpasciiCommunicationInterface communication = null;
        //deviceProperties currentDeviceProp = new deviceProperties();
        //deviceProperties currentDevProp = new deviceProperties();
        string commands = String.Empty;
        public static string response = String.Empty;

        public static bool plc_connect_result = false;
        public static ModbusRtu busRtuClient;
		public static string STRproduct="6";                    //接收PLC发送料号
		public static List<string> userList = new List<string>();                                  //已存在料号列表

		public HslCommunication()
        {
            try
            {
                Parameters.plcParams = XMLHelper.BackSerialize<Parameters.PLCParams>("Parameter/PLCParams.xml");
            }
            catch
            {
                Parameters.plcParams = new Parameters.PLCParams();
                XMLHelper.serialize<Parameters.PLCParams>(Parameters.plcParams, "Parameter/PLCParams.xml");
            }
           
            Thread th = new Thread(ini_PLC_Connect);
            th.IsBackground = true;
            th.Start();

            Thread PLC_Read = new Thread(ini_PLC_Read);
            PLC_Read.IsBackground = true;
            PLC_Read.Start();
        }
        public void ini_PLC_Connect()
        {
            if (!Authorization.SetAuthorizationCode("f562cc4c-4772-4b32-bdcd-f3e122c534e3"))
            {
                LogHelper.WriteError("HslCommunication 组件认证失败，组件只能使用8小时!");
            }           
            while (!plc_connect_result)
            {              
                try
                {
                    ////欧姆龙PLC Omron.PMAC.CK3M通讯
                    //if ("Omron.PMAC.CK3M".Equals(Parameters.commministion.PlcType))
                    //{
                    //    currentDevProp.IPAddress = Parameters.commministion.PlcIpAddress;
                    //    currentDevProp.Password = "deltatau";
                    //    currentDevProp.PortNumber = Parameters.commministion.PlcIpPort;
                    //    currentDevProp.User = "root";
                    //    currentDevProp.Protocol = CommunicationGlobals.ConnectionTypes.SSH;
                    //    communication = Connect.CreateSyncGpascii(currentDevProp.Protocol, communication);
                    //    plc_connect_result = communication.ConnectGpAscii(currentDevProp.IPAddress, currentDevProp.PortNumber, currentDevProp.User, currentDevProp.Password);                                              
                    //}
                    //欧姆龙PLC OmronFinsNet通讯
                     if ("Omron.OmronFinsNet".Equals(Parameters.commministion.PlcType))
                    {
                        OmronFinsNet Client = new OmronFinsNet();
                        Client.IpAddress = Parameters.commministion.PlcIpAddress;
                        Client.Port = Parameters.commministion.PlcIpPort;
                        Client.SA1 = Convert.ToByte(Parameters.commministion.PlcDevice);
                        Client.ConnectTimeOut = 5000;
                        _NetworkTcpDevice = Client;
                        _connected = _NetworkTcpDevice.ConnectServer();
                        plc_connect_result = _connected.IsSuccess;
                    }
                    //三菱PLC Melsec.MelsecMcNet通讯
                    else if ("Melsec.MelsecMcNet".Equals(Parameters.commministion.PlcType))
                    {
                        MelsecMcNet Client = new MelsecMcNet();
                        Client.IpAddress = Parameters.commministion.PlcIpAddress;
                        Client.Port = Parameters.commministion.PlcIpPort;
                        Client.ConnectTimeOut = 5000;
                        _NetworkTcpDevice = Client;
                        _connected = _NetworkTcpDevice.ConnectServer();
                        plc_connect_result = _connected.IsSuccess;
                    }
                  
                    //西门子PLC Siemens.SiemensS7Net通讯
                    else if ("Siemens.SiemensS7Net".Equals(Parameters.commministion.PlcType))
                    {
                        SiemensS7Net Client = new SiemensS7Net((SiemensPLCS)Convert.ToInt16(Parameters.commministion.PlcDevice), Parameters.commministion.PlcIpAddress);
                        Client.IpAddress = Parameters.commministion.PlcIpAddress;
                        Client.Port = Parameters.commministion.PlcIpPort;
                        Client.ConnectTimeOut = 5000;
                        _NetworkTcpDevice = Client;
                        _connected = _NetworkTcpDevice.ConnectServer();
                        plc_connect_result = _connected.IsSuccess;
                    }
                    //汇川PLC Inovance.InovanceSerialOverTcp通讯
                    else if ("Inovance.InovanceSerialOverTcp".Equals(Parameters.commministion.PlcType))
                    {
                        InovanceSerialOverTcp Client = new InovanceSerialOverTcp();
                        Client.IpAddress = Parameters.commministion.PlcIpAddress;
                        Client.Port = Parameters.commministion.PlcIpPort;
                        Client.DataFormat = DataFormat.ABCD;
                        Client.ConnectTimeOut = 5000;
                        _NetworkTcpDevice = Client;
                        _connected = _NetworkTcpDevice.ConnectServer();
                        plc_connect_result = _connected.IsSuccess;
                    }
                    //ModbusTcp通讯
                    else if ("ModbusTcpNet".Equals(Parameters.commministion.PlcType))
                    {
                        ModbusTcpNet Client = new ModbusTcpNet();
                        Client.IpAddress = Parameters.commministion.PlcIpAddress;
                        Client.Port = Parameters.commministion.PlcIpPort;
                        Client.Station = 0x01;
                        Client.ConnectTimeOut = 5000;
                        _NetworkTcpDevice = Client;
                        _connected = _NetworkTcpDevice.ConnectServer();
                        plc_connect_result = _connected.IsSuccess;
                    }
                    //新增通讯添加else if判断创建连接
                    else if ("ModbusRtu".Equals(Parameters.commministion.PlcType))
                    {
                        try
                        {
                            busRtuClient = new ModbusRtu(Convert.ToByte(Parameters.commministion.PlcDevice));
                            busRtuClient.SerialPortInni(sp =>
                            {
                                sp.PortName = Parameters.commministion.PlcIpAddress;
                                sp.BaudRate = Parameters.commministion.PlcIpPort;
                                sp.DataBits = 8;
                                sp.StopBits = System.IO.Ports.StopBits.One;
                                sp.Parity = System.IO.Ports.Parity.None;
                            });
                            busRtuClient.Open(); // 打开
                            plc_connect_result = true;
                        }
                        catch (Exception ex)
                        {
                            plc_connect_result = false;
                        }
                        if (plc_connect_result)
                        {
                            LogHelper.WriteInfo(Parameters.commministion.PlcType + "串口" + Parameters.commministion.PlcIpAddress + "打开成功,波特率:" + Parameters.commministion.PlcIpPort);        
                            plc_connect_result = true;
                        }
                        else
                        {
                            LogHelper.WriteError(Parameters.commministion.PlcType + "串口" + Parameters.commministion.PlcIpAddress + "打开失败,波特率:" + Parameters.commministion.PlcIpPort);      
                            plc_connect_result = false;
                        }
                        return;
                    }
                    //Parameter.PlcType字符错误或未定义
                    else
                    {
                        LogHelper.WriteError(Parameters.commministion.PlcType + "类型未定义!!!");
                        plc_connect_result = false;
                    }
                   
                    if (plc_connect_result)
                    {
                        LogHelper.WriteInfo(Parameters.commministion.PlcType + "连接成功:IP" + Parameters.commministion.PlcIpAddress + "  Port:" + Parameters.commministion.PlcIpPort);                        
                        plc_connect_result = true;
                    }
                    else
                    {
                        LogHelper.WriteError(Parameters.commministion.PlcType + "连接失败:IP" + Parameters.commministion.PlcIpAddress + "  Port:" + Parameters.commministion.PlcIpPort);
                        plc_connect_result = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(Parameters.commministion.PlcType + "初始化失败:"+ ex.Message);
                    plc_connect_result = false;
                }
            }
            while (plc_connect_result)
            {
                ////心跳读写，判断PLC是否掉线，不建议线程对plc链接释放重连
                Thread.Sleep(5000);
                
                if ("Omron.PMAC.CK3M".Equals(Parameters.commministion.PlcType))
                {
                    try
                    {
                        if (Parameters.plcParams.HeartBeatAdd != null || Parameters.plcParams.HeartBeatAdd != "")
                        {
                            plc_connect_result = ReadWritePmacVariables(Parameters.plcParams.HeartBeatAdd);
                            plc_connect_result = true;
                        }
                        else
                        {
                            
                        }
                    }
                    catch
                    {
                        plc_connect_result = false;
                    }                    
                }
                else if ("ModbusRtu".Equals(Parameters.commministion.PlcType))
                {
                    try
                    {
                        if (Parameters.plcParams.HeartBeatAdd != null || Parameters.plcParams.HeartBeatAdd != "")
                        {
                            _connected = busRtuClient.Write(Parameters.plcParams.HeartBeatAdd, 1);
                            Thread.Sleep(1000);
                            _connected = busRtuClient.Write(Parameters.plcParams.HeartBeatAdd, 0);
                            Thread.Sleep(1000);
                            plc_connect_result = true;
                        }
                        else
                        {
                            
                        }

                    }
                    catch
                    {
                        busRtuClient.Dispose();
                        plc_connect_result = false;
                    }
                }
                else
                {
                    try
                    {
                        if(Parameters.plcParams.HeartBeatAdd!=null|| Parameters.plcParams.HeartBeatAdd!="")
                        {
                            _connected = _NetworkTcpDevice.Write(Parameters.plcParams.HeartBeatAdd, 1);
                            Thread.Sleep(1000);
                            _connected = _NetworkTcpDevice.Write(Parameters.plcParams.HeartBeatAdd, 0);
                            Thread.Sleep(1000);
                            plc_connect_result = true;
                        }
                        else
                        {
                            
                        }                    
                    }
                    catch
                    {
                        _NetworkTcpDevice.Dispose();
                        plc_connect_result = false;                        
                    }
                }
            }             
        }
		public void ini_PLC_Read()
		{
			while (true)
			{
				if (plc_connect_result)
				{
					if (System.IO.File.Exists("fumiplc.txt"))
					{
						//存在 
						Thread.Sleep(2000);
						string SubStrProduct = _NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[2]).Content.ToString();

						STRproduct = SubStrProduct;
						if (STRproduct != null && STRproduct != "" && STRproduct != "0")
						{
							if ("Omron.PMAC.CK3M".Equals(Parameters.commministion.PlcType))
							{

							}
							else if ("ModbusRut".Equals(Parameters.commministion.PlcType))
							{
								ReadProductXml(userList);
								if (userList.Contains(STRproduct))
								{
									if (MainForm.Product != STRproduct)
									{
										if (MessageBox.Show("是否切换物料？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
										{
											MainForm.Product = STRproduct;
											ChangeProduct(STRproduct);
										}

									}
								}
								else
								{

									AddProductXml(MainForm.Product, STRproduct);
									ChangeProduct(STRproduct);
									MainForm.Product = STRproduct;
								}
								Thread.Sleep(1000);
							}
							else
							{
								ReadProductXml(userList);
								if (userList.Contains(STRproduct))
								{
									if (MainForm.Product != STRproduct)
									{
										if (MessageBox.Show("是否切换物料？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
										{
											MainForm.Product = STRproduct;
											ChangeProduct(STRproduct);
										}

									}
								}
								else
								{

									AddProductXml(MainForm.Product, STRproduct);
									ChangeProduct(STRproduct);
									MainForm.Product = STRproduct;
								}
								Thread.Sleep(1000);

							}
						}
					}
					else
					{
						//不存在 
						Thread.Sleep(2000);
						Int16 iPruductLong = _NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[1]).Content;
						int iProductNameLong;
						if (iPruductLong % 2 == 1)
						{
							iProductNameLong = iPruductLong / 2 + 1;
						}
						else
						{
							iProductNameLong = iPruductLong / 2;
						}

						string SubStrProduct = _NetworkTcpDevice.ReadString(Parameters.plcParams.预留地址[2], (ushort)iProductNameLong).Content;
						if (SubStrProduct != null && SubStrProduct != "")
						{
							STRproduct = SubStrProduct.Substring(0, iPruductLong);
						}

						if (STRproduct != null && STRproduct != "")
						{
							if ("Omron.PMAC.CK3M".Equals(Parameters.commministion.PlcType))
							{

							}
							else if ("ModbusRut".Equals(Parameters.commministion.PlcType))
							{
								ReadProductXml(userList);
								if (userList.Contains(STRproduct))
								{
									if (MainForm.Product != STRproduct)
									{
										if (MessageBox.Show("是否切换物料？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
										{
											MainForm.Product = STRproduct;
											ChangeProduct(STRproduct);
										}

									}
								}
								else
								{

									AddProductXml(MainForm.Product, STRproduct);
									ChangeProduct(STRproduct);
									MainForm.Product = STRproduct;
								}
								Thread.Sleep(1000);
							}
							else
							{
								ReadProductXml(userList);
								if (userList.Contains(STRproduct))
								{
									if (MainForm.Product != STRproduct)
									{
										if (MessageBox.Show("是否切换物料？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
										{
											MainForm.Product = STRproduct;
											ChangeProduct(STRproduct);
										}

									}
								}
								else
								{

									AddProductXml(MainForm.Product, STRproduct);
									ChangeProduct(STRproduct);
									MainForm.Product = STRproduct;
								}
							}
						}

					}
						
				}
				else
				{
					Thread.Sleep(1000);

				}
			}
		}

		//新增产品XML
		public static void AddProductXml(string StrProductFile, string StrProduct)
		{
			StrProductFile = MainForm.Product;
			XmlDocument doc = new XmlDocument();
			doc.Load("Parameter/ProductList.xml");
			XmlNode root = doc.SelectSingleNode("Products");
			//创建一个结点，并设置结点的名称
			XmlElement xelKey = doc.CreateElement("Product");
			//创建子结点
			XmlElement xelUser = doc.CreateElement("Name");
			xelUser.InnerText = StrProduct;

			//将子结点挂靠在相应的父节点
			xelKey.AppendChild(xelUser);
			//最后把book结点挂接在跟结点上，并保存整个文件
			root.AppendChild(xelKey);
			doc.Save("Parameter/ProductList.xml");
			GetFilesAndDirs(StrProductFile, StrProduct);
			MessageBox.Show("保存成功！", "温馨提示");
		}

		//切换产品
		public static void ChangeProduct(string StrProduct)
		{
			Parameters.commministion.productName = StrProduct;
			XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");
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
			}
		}

		public static void GetFilesAndDirs(string srcDir, string destDir)
		{
			if (!Directory.Exists(destDir))//若目标文件夹不存在
			{

                string newPath;
                FileInfo fileInfo;
                Directory.CreateDirectory(destDir);//创建目标文件夹                                                  
                string[] files = Directory.GetFiles(srcDir);//获取源文件夹中的所有文件完整路径
                foreach (string path in files)          //遍历文件     
                {
                    fileInfo = new FileInfo(path);
                    newPath = destDir + "/" + fileInfo.Name;
                    File.Copy(path, newPath, true);
                }
                string[] dirs = Directory.GetDirectories(srcDir);
                foreach (string path in dirs)        //遍历文件夹
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    string newDir = destDir + "/" + directory.Name;
                    GetFilesAndDirs(path + "\\", newDir + "\\");
                }
			}
		}

		//读取产品XML文件
		public static void ReadProductXml(List<string> listProduct)
		{
            listProduct.Clear();
			//加载指定路径的xml文件
			XmlDocument xmlDoc = new XmlDocument();
			XmlReaderSettings settings = new XmlReaderSettings();
			string StrProduct;
			settings.IgnoreComments = true; //忽略文档里面的注释
			XmlReader reader = XmlReader.Create("Parameter/ProductList.xml");
			xmlDoc.Load(reader);
			//得到根节点
			XmlNode xn = xmlDoc.SelectSingleNode("Products");
			//得到根节点的所有子节点
			XmlNodeList xnl = xn.ChildNodes;

			foreach (XmlNode item in xnl)
			{
				//将节点转换为元素，便于得到节点的属性值
				XmlElement xe = (XmlElement)item;
				//得到Name和Password两个属性的属性值
				XmlNodeList xmlnl = xe.ChildNodes;
				StrProduct = xmlnl.Item(0).InnerText;
				listProduct.Add(StrProduct);
			}
			reader.Close(); //读取完数据后需关闭
		}

		

		//对终端操作的通用方法/
		public static bool ReadWritePmacVariables(string command)
        {
            var commads = new List<string>();
            List<string> responses;
            commads.Add(command.ToString());
            var communicationStatus = communication.GetResponse(commads, out responses, 3);
            if (communicationStatus == Status.Ok)
            {
                response = string.Join("", responses.ToArray());
                command = null;
                return  true;
            }
            else
            {
                return  false;
            }
        }


        public static  double plc_Readdouble(string ReadAddress)
        {
            return -1;
        }
        public static void plc_WriteDouble()
        {

        }

        public static void plc_WriteBool()
        {
           
        }
    }
}
