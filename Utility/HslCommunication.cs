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
            while(true)
            {
                if(plc_connect_result)
                {
                    if ("Omron.PMAC.CK3M".Equals(Parameters.commministion.PlcType))
                    {

                    }
                    else if ("ModbusRut".Equals(Parameters.commministion.PlcType))
                    {

                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                  
                }                
            }
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
