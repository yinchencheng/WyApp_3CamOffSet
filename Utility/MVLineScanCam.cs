using HalconDotNet;
using MvFGCtrlC.NET;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WY_App.Utility
{
    public class MVLineScanCam
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory0", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public enum SAVE_IAMGE_TYPE
        {
            Image_Undefined = 0,                    // ch:未定义的图像格式 | en:Undefined Image Type
            Image_Bmp = 1,                          // ch:BMP图像格式 | en:BMP Image Type
            Image_Jpeg = 2,                         // ch:JPEG图像格式 | en:Jpeg Image Type
        }

        /// <summary>
        /// ch:判断用户自定义像素格式 | en:Determine custom pixel format
        /// </summary>
        public const Int32 CUSTOMER_PIXEL_FORMAT = unchecked((Int32)0x80000000);
        /// <summary>
        /// ch:触发模式开 | en:Trigger mode on
        /// </summary>
        public UInt32 TRIGGER_MODE_ON = 1;
        /// <summary>
        /// ch:触发模式关 | en:Trigger mode off
        /// </summary>
        public const UInt32 TRIGGER_MODE_OFF = 0;
        /// <summary>
        /// ch:操作采集卡 | en:Interface operations
        /// </summary>
        CSystem m_cSystem = new CSystem();
        /// <summary>
        /// ch:操作采集卡和设备 | en:Interface and device operation
        /// </summary>
        CInterface[] m_cInterface = new CInterface[3];
        /// <summary>
        /// ch:操作设备和流 | en:Device and stream operation
        /// </summary>
        CDevice[] m_cDevice = new CDevice[3];
        /// <summary>
        /// ch:操作流和缓存 | en:Stream and buffer operation
        /// </summary>
        CStream[] m_cStream = null;
        /// <summary>
        /// ch:采集卡数量 | en:Interface number
        /// </summary>
        public uint m_nInterfaceNum = 0;
        /// <summary>
        /// ch:采集卡是否打开 | en:Whether to open interface
        /// </summary>
        public bool[] m_bIsIFOpen = new bool[4] { false,false,false,false };
        /// <summary>
        /// ch:设备是否打开 | en:Whether to open device
        /// </summary>
        public bool[] m_bIsDeviceOpen = new bool[4] { false, false, false, false };
        /// <summary>
        /// ch:是否在抓图 | en:Whether to start grabbing
        /// </summary>
        public bool[] m_bIsGrabbing = new bool[4] { false, false, false, false };
        /// <summary>
        /// ch:触发模式 | en:Trigger Mode
        /// </summary>
        public uint m_nTriggerMode = TRIGGER_MODE_OFF;
        /// <summary>
        /// ch:线程状态 | en:Thread state
        /// </summary>
        bool[] m_bThreadState = new bool[4] { false, false, false, false };
        /// <summary>
        /// ch:取流线程 | en:Grabbing thread
        /// </summary>
        Thread[] m_hGrabThread = null;
        /// <summary>
        /// ch:数据缓存 | en:Data buffer
        /// </summary>
        IntPtr[] m_pDataBuf =new IntPtr[4] { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        /// <summary>
        /// ch:数据缓存大小 | en:Length of data buffer
        /// </summary>
        uint[] m_nDataBufSize =new uint[4] { 0, 0, 0, 0 };
        /// <summary>
        /// ch:图像缓存 | en:Image buffer
        /// </summary>
        IntPtr[] m_pSaveImageBuf = new IntPtr[4] { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        /// <summary>
        /// ch:图像缓存大小 | en:Length of image buffer
        /// </summary>
        uint[] m_nSaveImageBufSize  = new uint[4] { 0, 0, 0, 0 };
        /// <summary>
        /// ch:存图锁 | en:Lock for saving image
        /// </summary>
        private static Object[] m_LockForSaveImage = new Object[4];
        /// <summary>
        /// ch:图像信息 | en:Image info
        /// </summary>
        MV_FG_INPUT_IMAGE_INFO[] m_stImageInfo = new MV_FG_INPUT_IMAGE_INFO[4];   
        delegate void ShowDisplayError(int nRet);

        public List<string> cmbInterfaceList = new List<string>();

        public List<string> cmbDeviceList = new List<string>();

        public AutoResetEvent[] ImageEvent = null;

        public MVLineScanCam()
        {
            ImageEvent = new AutoResetEvent[4]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };
            Thread th = new Thread(ini_MVLineScanCam);
            th.IsBackground = true;
            th.Start();
        }

        public void ini_MVLineScanCam()
        {
            while (true)
            {
                try
                {
                    if (m_nInterfaceNum==0)
                    {
                        EnumInterface();
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        for (uint index = 0; index < m_nInterfaceNum; index++)
                        {
                            if (!m_bIsIFOpen[index])
                            {
                                OpenInterface(index);
                                Thread.Sleep(10000);
                            }
                        }
                        Thread.Sleep(1000);
                        for (uint index = 0; index < m_nInterfaceNum; index++)
                        {
                            EnumDevice(index);
                        }
                        Thread.Sleep(3000);
                        for (uint index = 0; index < m_nInterfaceNum; index++)
                        {
                            if (!m_bIsDeviceOpen[index])
                            {
                                OpenDevice(index);
                            }
                        }
                        
                    }                    
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("相机链接失败:" + ex.Message);
                }
                Thread.Sleep(3000);
            }
        }

        public void EnumInterface()
        {
            int nRet = 0;
            bool bChanged = false;

            // ch:枚举采集卡 | en:Enum interface
            nRet = m_cSystem.UpdateInterfaceList(
                CParamDefine.MV_FG_CAMERALINK_INTERFACE | CParamDefine.MV_FG_GEV_INTERFACE | CParamDefine.MV_FG_CXP_INTERFACE,
                ref bChanged);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("枚举采集卡失败，故障码: " + nRet.ToString("X"));
                return;
            }
            m_nInterfaceNum = 0;

            // ch:获取采集卡数量 | en:Get interface num
            nRet = m_cSystem.GetNumInterfaces(ref m_nInterfaceNum);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("获取采集卡数量失败，故障码:" + nRet.ToString("X"));
                return;
            }
            if (0 == m_nInterfaceNum)
            {
                LogHelper.WriteInfo("未发现采集卡");                
                return;
            }

            if (bChanged)
            {
                // ch:向下拉框添加采集卡信息 | en:Add interface info in Combo
                MV_FG_INTERFACE_INFO stIfInfo = new MV_FG_INTERFACE_INFO();
                for (uint i = 0; i < m_nInterfaceNum; i++)
                {
                    // ch:获取采集卡信息 | en:Get interface info
                    nRet = m_cSystem.GetInterfaceInfo(i, ref stIfInfo);
                    if (CErrorCode.MV_FG_SUCCESS != nRet)
                    {
                        LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                        return;
                    }

                    string strShowIfInfo = null;
                    switch (stIfInfo.nTLayerType)
                    {
                        case CParamDefine.MV_FG_GEV_INTERFACE:
                            {
                                MV_GEV_INTERFACE_INFO stGevIFInfo = (MV_GEV_INTERFACE_INFO)CAdditional.ByteToStruct(
                                    stIfInfo.SpecialInfo.stGevIfInfo, typeof(MV_GEV_INTERFACE_INFO));
                                strShowIfInfo += "GEV[" + i.ToString() + "]: " + stGevIFInfo.chDisplayName + " | " +
                                    stGevIFInfo.chInterfaceID + " | " + stGevIFInfo.chSerialNumber;
                                break;
                            }
                        case CParamDefine.MV_FG_CXP_INTERFACE:
                            {
                                MV_CXP_INTERFACE_INFO stCxpIFInfo = (MV_CXP_INTERFACE_INFO)CAdditional.ByteToStruct(
                                    stIfInfo.SpecialInfo.stCXPIfInfo, typeof(MV_CXP_INTERFACE_INFO));
                                strShowIfInfo += "CXP[" + i.ToString() + "]: " + stCxpIFInfo.chDisplayName + " | " +
                                    stCxpIFInfo.chInterfaceID + " | " + stCxpIFInfo.chSerialNumber;
                                break;
                            }
                        case CParamDefine.MV_FG_CAMERALINK_INTERFACE:
                            {
                                MV_CML_INTERFACE_INFO stCmlIFInfo = (MV_CML_INTERFACE_INFO)CAdditional.ByteToStruct(
                                    stIfInfo.SpecialInfo.stCMLIfInfo, typeof(MV_CML_INTERFACE_INFO));
                                strShowIfInfo += "CML[" + i.ToString() + "]: " + stCmlIFInfo.chDisplayName + " | " +
                                    stCmlIFInfo.chInterfaceID + " | " + stCmlIFInfo.chSerialNumber;
                                break;
                            }
                        default:
                            {
                                strShowIfInfo += "未知采集卡[" + i.ToString() + "]";
                                break;
                            }
                    }
                    cmbInterfaceList.Add(strShowIfInfo);
                }
            }
        }

        /// <summary>
        /// 打开采集卡
        /// </summary>
        public void OpenInterface(uint index)
        {
            int nRet = m_cSystem.OpenInterface(Convert.ToUInt32(index), out m_cInterface[index]);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                return;
            }
            m_bIsIFOpen[index] = true;
        }

        /// <summary>
        /// 关闭采集卡
        /// </summary>
        public void CloseInterface(uint index)
        {
            if (m_bIsIFOpen[index] && m_bIsDeviceOpen[index])
            {
                CloseDevice(index);
                // ch:关闭采集卡 | en:Close interface
                int nRet = m_cInterface[index].CloseInterface();
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    MessageBox.Show("关闭采集卡" + index + "失败，故障代码:" + nRet.ToString("X"));
                    return;
                }
            }
            m_bIsIFOpen[index] = false;

        }

        public void EnumDevice(uint index) 
        {

            if (m_bIsIFOpen[index])
            {
                int nRet = 0;
                bool bChanged = false;
                uint nDeviceNum = 0;

                // ch:枚举采集卡上的相机 | en:Enum camera of interface
                nRet = m_cInterface[index].UpdateDeviceList(ref bChanged);
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                    return;
                }

                // ch:获取设备数量 | en:Get device number
                nRet = m_cInterface[index].GetNumDevices(ref nDeviceNum);
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                    return;
                }
                if (0 == nDeviceNum)
                {
                    LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                    return;
                }

                if (bChanged)
                {
                    cmbDeviceList.Clear();
                    // ch:向下拉框添加设备信息 | en:Add device info in Combo
                    MV_FG_DEVICE_INFO stDeviceInfo = new MV_FG_DEVICE_INFO();
                    for (uint i = 0; i < nDeviceNum; i++)
                    {
                        // ch:获取设备信息 | en:Get device info
                        nRet = m_cInterface[index].GetDeviceInfo(i, ref stDeviceInfo);
                        if (CErrorCode.MV_FG_SUCCESS != nRet)
                        {
                            cmbDeviceList.Clear();
                            LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                            return;
                        }

                        string strShowDevInfo = null;
                        switch (stDeviceInfo.nDevType)
                        {
                            case CParamDefine.MV_FG_GEV_DEVICE:
                                {
                                    MV_GEV_DEVICE_INFO stGevDevInfo = (MV_GEV_DEVICE_INFO)CAdditional.ByteToStruct(
                                        stDeviceInfo.DevInfo.stGEVDevInfo, typeof(MV_GEV_DEVICE_INFO));
                                    strShowDevInfo += "GEV[" + i.ToString() + "]: " + stGevDevInfo.chUserDefinedName + " | " +
                                        stGevDevInfo.chModelName + " | " + stGevDevInfo.chSerialNumber;
                                    break;
                                }
                            case CParamDefine.MV_FG_CXP_DEVICE:
                                {
                                    MV_CXP_DEVICE_INFO stCxpDevInfo = (MV_CXP_DEVICE_INFO)CAdditional.ByteToStruct(
                                        stDeviceInfo.DevInfo.stCXPDevInfo, typeof(MV_CXP_DEVICE_INFO));
                                    strShowDevInfo += "CXP[" + i.ToString() + "]: " + stCxpDevInfo.chUserDefinedName + " | " +
                                        stCxpDevInfo.chModelName + " | " + stCxpDevInfo.chSerialNumber;
                                    break;
                                }
                            case CParamDefine.MV_FG_CAMERALINK_DEVICE:
                                {
                                    MV_CML_DEVICE_INFO stCmlDevInfo = (MV_CML_DEVICE_INFO)CAdditional.ByteToStruct(
                                        stDeviceInfo.DevInfo.stCMLDevInfo, typeof(MV_CML_DEVICE_INFO));
                                    strShowDevInfo += "CML[" + i.ToString() + "]: " + stCmlDevInfo.chUserDefinedName + " | " +
                                        stCmlDevInfo.chModelName + " | " + stCmlDevInfo.chSerialNumber;
                                    break;
                                }
                            default:
                                {
                                    strShowDevInfo += "未知相机设备[" + i.ToString() + "]";
                                    break;
                                }
                        }
                        cmbDeviceList.Add(strShowDevInfo);
                    }
                }
            }
            else
            {
                LogHelper.WriteInfo("采集卡未打开");
            }
        }
        
        /// <summary>
        /// 打开相机
        /// </summary>
        public void OpenDevice(uint index)
        {
            if (m_bIsIFOpen[index] && !m_bIsDeviceOpen[index])
            {
                // ch:打开设备，获得设备句柄 | en:Open device, get handle
                int nRet = m_cInterface[index].OpenDevice(0, out m_cDevice[index]);
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    LogHelper.WriteInfo("打开相机设备" + index + "失败，故障代码: " + nRet.ToString("X"));
                    m_bIsDeviceOpen[index] = false;
                    return;
                }
                // ch:设置连续采集模式 | en:Set Continuous Aquisition Mode
                CParam cDeviceParam = new CParam(m_cDevice[index]);
                cDeviceParam.SetEnumValue("AcquisitionMode", 0);  // 0 - SingleFrame, 2 - Continuous
                cDeviceParam.SetEnumValue("TriggerMode", 0);      // ch:触发模式关 | en:Trigger mode off  
                m_bIsDeviceOpen[index] = true;
            }
        }

        /// <summary>
        /// 关闭相机
        /// </summary>
        public void CloseDevice(uint index)
        {
            int nRet = 0;
            // ch:关闭流通道 | en:Close stream channel
            nRet = m_cStream[index].CloseStream();
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                MessageBox.Show("关闭取流通道" + index + "失败，故障代码:" + nRet.ToString("X"));
            }
            // ch:关闭设备 | en:Close device
            nRet = m_cDevice[index].CloseDevice();
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                MessageBox.Show("关闭相机设备" + index + "失败，故障代码:" + nRet.ToString("X"));
            }
            if (IntPtr.Zero != m_pDataBuf[index])
            {
                Marshal.FreeHGlobal(m_pDataBuf[index]);
                m_pDataBuf[index] = IntPtr.Zero;
            }

            if (IntPtr.Zero != m_pSaveImageBuf[index])
            {
                Marshal.FreeHGlobal(m_pSaveImageBuf[index]);
                m_pSaveImageBuf[index] = IntPtr.Zero;
            }
            m_bIsDeviceOpen[index] = false;
            m_bIsGrabbing[index] = false;
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public void StartGrab(uint index)
        {
            if (!m_bIsDeviceOpen[index])
            {
                MessageBox.Show("请确认相机连接状态");
                return;
            }

            if (m_bIsGrabbing[index])
            {
                return;
            }

            // ch:获取流通道个数 | en:Get number of stream
            uint nStreamNum = 0;
            int nRet = m_cDevice[index].GetNumStreams(ref nStreamNum);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("获取流通道数量失败，故障码:" + nRet.ToString("X"));
                return;
            }
            if (0 == nStreamNum)
            {
                LogHelper.WriteInfo("没有可用的数据通道");
                return;
            }

            // ch:打开流通道(目前只支持单个通道) | en:Open stream(Only a single stream is supported now)
            nRet = m_cDevice[index].OpenStream(0, out m_cStream[index]);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("打开流通道失败，故障码:" + nRet.ToString("X"));
                return;
            }

            // ch:设置SDK内部缓存数量 | en:Set internal buffer number
            const uint nBufNum = 5;
            nRet = m_cStream[index].SetBufferNum(nBufNum);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo(":设置缓存数量失败，故障码:" + nRet.ToString("X"));;
                return;
            }

            // ch:创建取流线程 | en:Create acquistion thread
            m_bThreadState[index] = true;
            m_hGrabThread[index] = new Thread(new ParameterizedThreadStart(ReceiveThreadProcess));
            m_hGrabThread[index].Start(index);
            // ch:开始取流 | en:Start Acquisition
            nRet = m_cStream[index].StartAcquisition();
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                m_bThreadState[index] = false;
                LogHelper.WriteInfo("开始取流失败，故障码:" + nRet.ToString("X"));
                return;
            }
            m_bIsGrabbing[index] = true;
        }       

        public void StopGrab(uint index)
        {
            if (m_bIsDeviceOpen[index])
            {
                if (false == m_bIsDeviceOpen[index] || false == m_bIsGrabbing[index])
                {
                    return;
                }
                // ch:停止取流 | en:Stop Acquisition
                int nRet = m_cStream[index].StopAcquisition();
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    MessageBox.Show("停止取流" + index + "失败, 故障代码:" + nRet.ToString("X"));
                    return;
                }

                // ch:关闭流通道 | en:Close stream channel
                nRet = m_cStream[index].CloseStream();
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    MessageBox.Show("关闭取流通道" + index + "失败, 故障代码:" + nRet.ToString("X"));
                }
                m_bIsGrabbing[index] = false;
            }                     
        }

        /// <summary>
        /// 触发采集
        /// </summary>
        public void TriggerExec(uint index)
        {
            if (true != m_bIsGrabbing[index])
            {
                return;
            }
            CParam cDeviceParam = new CParam(m_cDevice[index]);

            // ch:触发命令 | en:Trigger command
            int nRet = cDeviceParam.SetCommandValue("TriggerSoftware");
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                MessageBox.Show("相机" + index + "触发采集失败, 错误代码:" + nRet.ToString("X"));
            }
  
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        public void TriggerMode(uint index)
        {
            CParam cDeviceParam = new CParam(m_cDevice[index]);
            // ch:打开触发模式 | en:Open Trigger Mode
            int nRet = cDeviceParam.SetEnumValue("TriggerMode", TRIGGER_MODE_ON);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                MessageBox.Show("相机" + index + "打开触发采集模式失败, 错误代码:" + nRet.ToString("X"));
                return; ;
            }
            m_nTriggerMode = TRIGGER_MODE_ON;
            // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;
            if (Parameters.cameraParam.CamSoftwareMode)
            {
                cDeviceParam.SetEnumValue("TriggerSource", (uint)7);
            }
            else
            {
                cDeviceParam.SetEnumValue("TriggerSource", (uint)0);
            }
            
        }

        public void SoftTrigger(uint index)
        {       
            CParam cDeviceParam = new CParam(m_cDevice[index]);

            // ch:触发源设置 | en:Set trigger source
            // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;
            if (Parameters.cameraParam.CamSoftwareMode)
            {
                int nRet = cDeviceParam.SetEnumValue("TriggerSource", (uint)7);
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    MessageBox.Show("设置相机软触发失败, 错误代码:" + nRet.ToString("X"));
                    return;
                }
            }
            else
            {
                int nRet = cDeviceParam.SetEnumValue("TriggerSource", (uint)0);
                if (CErrorCode.MV_FG_SUCCESS != nRet)
                {
                    MessageBox.Show("设置相机硬触发失败, 错误代码:" + nRet.ToString("X"));
                    return;
                }
            }
                      
        }

        /// <summary>
        /// 设置连续采集模式
        /// </summary>
        public void ContinuesMode(int index)
        {
            CParam cDeviceParam = new CParam(m_cDevice[index]);

            // ch:关闭触发模式 | en:Turn off Trigger Mode
            int nRet = cDeviceParam.SetEnumValue("TriggerMode", TRIGGER_MODE_OFF);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                MessageBox.Show("设置相机连续采集失败, 错误代码:" + nRet.ToString("X"));
                return;
            }
            m_nTriggerMode = TRIGGER_MODE_OFF;
        }
        
        public void ReceiveThreadProcess(object index)
        {
            uint uintdex=Convert.ToUInt32(index);
            CImageProcess cImgProc = new CImageProcess(m_cStream[uintdex]);
            MV_FG_BUFFER_INFO stFrameInfo = new MV_FG_BUFFER_INFO();          // ch:图像信息 | en:Frame info
            MV_FG_INPUT_IMAGE_INFO stDisplayInfo = new MV_FG_INPUT_IMAGE_INFO();   // ch:显示的图像信息 | en:Display frame info
            const uint nTimeout = 1000;
            int nRet = 0;

            while (m_bThreadState[uintdex])
            {
                if (m_bIsGrabbing[uintdex])
                {
                    // ch:获取一帧图像缓存信息 | en:Get one frame buffer's info
                    nRet = m_cStream[uintdex].GetFrameBuffer(ref stFrameInfo, nTimeout);
                    if (CErrorCode.MV_FG_SUCCESS == nRet)
                    {
                        // 用于保存图片
                        lock (m_LockForSaveImage)
                        {
                            if (IntPtr.Zero == m_pDataBuf[uintdex] || m_nDataBufSize[uintdex] < stFrameInfo.nFilledSize)
                            {
                                if (IntPtr.Zero != m_pDataBuf[uintdex])
                                {
                                    Marshal.FreeHGlobal(m_pDataBuf[uintdex]);
                                    m_pDataBuf[uintdex] = IntPtr.Zero;
                                }

                                m_pDataBuf[uintdex] = Marshal.AllocHGlobal((Int32)stFrameInfo.nFilledSize);
                                if (IntPtr.Zero == m_pDataBuf[0])
                                {
                                    m_cStream[uintdex].ReleaseFrameBuffer(stFrameInfo);
                                    break;
                                }
                                m_nDataBufSize[uintdex] = stFrameInfo.nFilledSize;
                            }
                            CopyMemory(m_pDataBuf[uintdex], stFrameInfo.pBuffer, stFrameInfo.nFilledSize);

                            m_stImageInfo[uintdex].nWidth = stFrameInfo.nWidth;
                            m_stImageInfo[uintdex].nHeight = stFrameInfo.nHeight;
                            m_stImageInfo[uintdex].enPixelType = stFrameInfo.enPixelType;
                            m_stImageInfo[uintdex].pImageBuf = m_pDataBuf[0];
                            m_stImageInfo[uintdex].nImageBufLen = stFrameInfo.nFilledSize;
                        }
                        // 配置显示图像的参数
                        stDisplayInfo.nWidth = stFrameInfo.nWidth;
                        stDisplayInfo.nHeight = stFrameInfo.nHeight;
                        stDisplayInfo.enPixelType = stFrameInfo.enPixelType;
                        stDisplayInfo.pImageBuf = stFrameInfo.pBuffer;
                        stDisplayInfo.nImageBufLen = stFrameInfo.nFilledSize;

                        try
                        {
                            HOperatorSet.GenImage1Extern(out MainForm.hImage[uintdex], "byte", stFrameInfo.nWidth, stFrameInfo.nHeight, (HTuple)stFrameInfo.pBuffer, IntPtr.Zero);
                            ImageEvent[uintdex].Set();
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        m_cStream[uintdex].ReleaseFrameBuffer(stFrameInfo);
                    }
                    else
                    {
                        if (TRIGGER_MODE_ON == m_nTriggerMode)
                        {
                            Thread.Sleep(5);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
        }

        public void SaveBmp(uint index)
        {
            int nRet = SaveImage(index, SAVE_IAMGE_TYPE.Image_Bmp);
            if (CErrorCode.MV_FG_SUCCESS != nRet)
            {
                LogHelper.WriteInfo("获取采集卡信息失败，故障码:" + nRet.ToString("X"));
                return;
            }            
        }

        private int SaveImage(uint index, SAVE_IAMGE_TYPE enSaveImageType)
        {
            int nRet = 0;

            lock (m_LockForSaveImage)
            {
                if (IntPtr.Zero == m_pDataBuf[index])
                {
                    return CErrorCode.MV_FG_ERR_NO_DATA;
                }

                if (RemoveCustomPixelFormats(m_stImageInfo[index].enPixelType))
                {
                    return CErrorCode.MV_FG_ERR_INVALID_VALUE;
                }

                uint nMaxImageLen = m_stImageInfo[index].nWidth * m_stImageInfo[index].nHeight * 4 + 2048; // 确保存图空间足够，包括图像头

                if (IntPtr.Zero == m_pSaveImageBuf[index] || m_nSaveImageBufSize[index] < nMaxImageLen)
                {
                    if (IntPtr.Zero != m_pSaveImageBuf[index])
                    {
                        Marshal.FreeHGlobal(m_pSaveImageBuf[index]);
                        m_pSaveImageBuf[index] = IntPtr.Zero;
                    }

                    m_pSaveImageBuf[index] = Marshal.AllocHGlobal((Int32)nMaxImageLen);
                    if (IntPtr.Zero == m_pSaveImageBuf[index])
                    {
                        return CErrorCode.MV_FG_ERR_OUT_OF_MEMORY;
                    }
                    m_nSaveImageBufSize[index] = nMaxImageLen;
                }

                CImageProcess cImgSave = new CImageProcess(m_cStream[index]);
                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;

                do
                {
                    if (SAVE_IAMGE_TYPE.Image_Bmp == enSaveImageType)
                    {
                        MV_FG_SAVE_BITMAP_INFO stBmpInfo = new MV_FG_SAVE_BITMAP_INFO();

                        stBmpInfo.stInputImageInfo = m_stImageInfo[index];
                        stBmpInfo.pBmpBuf = m_pSaveImageBuf[index];
                        stBmpInfo.nBmpBufSize = m_nSaveImageBufSize[index];
                        stBmpInfo.enCfaMethod = MV_FG_CFA_METHOD.MV_FG_CFA_METHOD_OPTIMAL;

                        // ch:保存BMP图像 | en:Save to BMP
                        nRet = cImgSave.SaveBitmap(ref stBmpInfo);
                        if (CErrorCode.MV_FG_SUCCESS != nRet)
                        {
                            break;
                        }

                        // ch:将图像数据保存到本地文件 | en:Save image data to local file
                        byte[] byteData = new byte[stBmpInfo.nBmpBufLen];
                        Marshal.Copy(stBmpInfo.pBmpBuf, byteData, 0, (int)stBmpInfo.nBmpBufLen);

                        string strName = "Image_w" + stBmpInfo.stInputImageInfo.nWidth.ToString() +
                            "_h" + stBmpInfo.stInputImageInfo.nHeight.ToString() + "_" + currentTime.Minute.ToString() +
                            currentTime.Second.ToString() + currentTime.Millisecond.ToString() + ".bmp";

                        FileStream pFile = new FileStream(strName, FileMode.Create);
                        if (null == pFile)
                        {
                            nRet = CErrorCode.MV_FG_ERR_ERROR;
                            break;
                        }
                        pFile.Write(byteData, 0, byteData.Length);
                        pFile.Close();
                    }
                    else if (SAVE_IAMGE_TYPE.Image_Jpeg == enSaveImageType)
                    {
                        MV_FG_SAVE_JPEG_INFO stJpgInfo = new MV_FG_SAVE_JPEG_INFO();

                        stJpgInfo.stInputImageInfo = m_stImageInfo[index];
                        stJpgInfo.pJpgBuf = m_pSaveImageBuf[index];
                        stJpgInfo.nJpgBufSize = m_nSaveImageBufSize[index];
                        stJpgInfo.nJpgQuality = 80;                   // JPG编码质量(0-100]
                        stJpgInfo.enCfaMethod = MV_FG_CFA_METHOD.MV_FG_CFA_METHOD_OPTIMAL;

                        // ch:保存JPG图像 | en:Save to JPG
                        nRet = cImgSave.SaveJpeg(ref stJpgInfo);
                        if (CErrorCode.MV_FG_SUCCESS != nRet)
                        {
                            break;
                        }

                        // ch:将图像数据保存到本地文件 | en:Save image data to local file
                        byte[] byteData = new byte[stJpgInfo.nJpgBufLen];
                        Marshal.Copy(stJpgInfo.pJpgBuf, byteData, 0, (int)stJpgInfo.nJpgBufLen);

                        string strName = "Image_w" + stJpgInfo.stInputImageInfo.nWidth.ToString() +
                            "_h" + stJpgInfo.stInputImageInfo.nHeight.ToString() + "_" + currentTime.Minute.ToString() +
                            currentTime.Second.ToString() + currentTime.Millisecond.ToString() + ".jpg";

                        FileStream pFile = new FileStream(strName, FileMode.Create);
                        if (null == pFile)
                        {
                            nRet = CErrorCode.MV_FG_ERR_ERROR;
                            break;
                        }
                        pFile.Write(byteData, 0, byteData.Length);
                        pFile.Close();
                    }
                    else
                    {
                        nRet = CErrorCode.MV_FG_ERR_INVALID_PARAMETER;
                        break;
                    }
                } while (false);
            }

            return nRet;
        }

        // ch:去除自定义的像素格式 | en:Remove custom pixel formats
        private bool RemoveCustomPixelFormats(MV_FG_PIXEL_TYPE enPixelFormat)
        {
            Int32 nResult = ((int)enPixelFormat) & CUSTOMER_PIXEL_FORMAT;
            if (CUSTOMER_PIXEL_FORMAT == nResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
