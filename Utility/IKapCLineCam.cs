using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IKapBoardClassLibrary;
using IKapC.NET;
using HalconDotNet;
using OpenCvSharp;

namespace WY_App.Utility
{
    public class IKapCLineCam
    {
        public static HObject[] hv_Image = new HObject[4];
        public static HTuple[] hv_Width = new HTuple[4];
        public static HTuple[] hv_Height = new HTuple[4];
        public static bool[] m_hCameraResult = new bool[4] { false, false, false ,false};
        public static uint numCameras = 0;
        // 相机设备句柄。
        //
        // Camera device handle.


        public IntPtr[] m_hCamera = new IntPtr[4];


        // 采集卡设备句柄。
        //
        // Frame grabber device handle.
        public IntPtr[] m_hBoard = new IntPtr[4];

        // 保存图像的文件名。
        //
        // File name of image.
        public string[] m_strFileName = new string[4] { "D:/Image/Image0.tiff", "D:/Image/Image1.tiff", "D:/Image/Image2.tiff", "D:/Image/Image3.tiff" };

        public string[] m_strCamName = new string[4] { Parameters.commministion.productName + "/CamParams0.vlcf", Parameters.commministion.productName + "/CamParams1.vlcf", Parameters.commministion.productName + "/CamParams2.vlcf", Parameters.commministion.productName + "/CamParams4.vlcf" };
        // 图像缓冲区申请的帧数。
        //
        // The number of frames requested by buffer.
        public int m_nTotalFrameCount = 1;

        /* @brief：判断 IKapC 函数是否成功调用。
         * @param[in] res：函数返回值。
         *
         * @brief：Determine whether the IKapC function is called successfully.
         * @param[in] res：Function return value. */
        static bool CheckIKapC(uint res)
        {
            if (res != (uint)ItkStatusErrorId.ITKSTATUS_OK)
            {
                LogHelper.WriteError("Error Code: {0}.\n" + res.ToString("x8"));
                IKapCLib.ItkManTerminate();
                return false;
                //Environment.Exit(1);
            }
            else { return true; }
        }

        /* @brief：判断 IKapBoard 函数是否成功调用。
         * @param[in] ret：函数返回值。
         *
         * @brief：Determine whether the IKapBoard function is called successfully.
         * @param[in] ret：Function return value. */
        static bool CheckIKapBoard(int ret)
        {
            if (ret != (int)IKapBoardClassLibrary.ErrorCode.IK_RTN_OK)
            {
                string sErrMsg = "";
                IKapBoard.IKAPERRORINFO tIKei = new IKapBoardClassLibrary.IKapBoard.IKAPERRORINFO();

                // 获取错误码信息。
                //
                // Get error code message.
                IKapBoard.IKapGetLastError(ref tIKei, true);

                // 打印错误信息。
                //
                // Print error message.
                sErrMsg = string.Concat(sErrMsg, "Board Type\t = 0x", tIKei.uBoardType.ToString("X4"), "\n", "Board Index\t = 0x", tIKei.uBoardIndex.ToString("X4"), "\n", "Error Code\t = 0x", tIKei.uErrorCode.ToString("X4"), "\n");
                LogHelper.WriteError(sErrMsg);
                return false;
                //Environment.Exit(1);
            }
            else
            {
                return true;
            }
        }

        #region Callback
        delegate void IKapCallBackProc(IntPtr pParam);

        /* @brief：本函数被注册为一个回调函数。当图像采集开始时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When starting grabbing images, the function will be called. */
        private IKapCallBackProc[] OnGrabStartProc = new IKapCallBackProc[4];

        /* @brief：本函数被注册为一个回调函数。当采集丢帧时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When grabbing frame lost, the function will be called. */
        private IKapCallBackProc[] OnFrameLostProc = new IKapCallBackProc[4];

        /* @brief：本函数被注册为一个回调函数。当图像采集超时时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When grabbing images time out, the function will be called. */
        private IKapCallBackProc[] OnTimeoutProc = new IKapCallBackProc[4];

        /* @brief：本函数被注册为一个回调函数。当一帧图像采集完成时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When a frame of image grabbing ready, the function will be called. */
        private IKapCallBackProc[] OnFrameReadyProc = new IKapCallBackProc[4];

        /* @brief：本函数被注册为一个回调函数。当图像采集停止时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When stopping grabbing images, the function will be called. */
        private IKapCallBackProc[] OnGrabStopProc = new IKapCallBackProc[4];
        #endregion

        #region Callback
        /* @brief：本函数被注册为一个回调函数。当图像采集开始时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When starting grabbing images, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnGrabStartFunc(IntPtr pParam)
        {
            LogHelper.WriteInfo("Start grabbing image");
        }

        /* @brief：本函数被注册为一个回调函数。当采集丢帧时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When grabbing frame lost, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnFrameLostFunc(IntPtr pParam)
        {
            LogHelper.WriteError("Grab frame lost");
        }

        /* @brief：本函数被注册为一个回调函数。当图像采集超时时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When grabbing images time out, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnTimeoutFunc(IntPtr pParam)
        {
            LogHelper.WriteError("Grab image timeout");
        }

        /* @brief：本函数被注册为一个回调函数。当一帧图像采集完成时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When a frame of image grabbing ready, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnFrameReadyFunc0(IntPtr pParam)
        {
            LogHelper.WriteInfo("Grab frame ready0");
            IntPtr hDev = (IntPtr)pParam;
            IntPtr pUserBuffer = IntPtr.Zero;
            int nFrameSize = 0;
            int nFrameWidht = 0;
            int nFrameHeight = 0;
            int nFrameCount = 0;
            IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();
            IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_COUNT, ref nFrameCount);
            IKapBoard.IKapGetBufferStatus(hDev, 0, ref status);
            if (status.uFull == 1)
            {
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_SIZE, ref nFrameSize);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref nFrameWidht);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref nFrameHeight);
                IKapBoard.IKapGetBufferAddress(hDev, 0, ref pUserBuffer);
                hv_Width[0] = nFrameWidht;
                hv_Height[0] = nFrameHeight;
                HOperatorSet.GenImage1(out hv_Image[0], "byte", nFrameWidht, nFrameHeight, pUserBuffer);
                LogHelper.WriteInfo("Grab frame ImageEvent0");
                MainForm.ImageEvent[0].Set();
            }
        }
        public void OnFrameReadyFunc1(IntPtr pParam)
        {
            LogHelper.WriteInfo("Grab frame ready1");
            IntPtr hDev = (IntPtr)pParam;
            IntPtr pUserBuffer = IntPtr.Zero;
            int nFrameSize = 0;
            int nFrameWidht = 0;
            int nFrameHeight = 0;
            int nFrameCount = 0;
            IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();
            IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_COUNT, ref nFrameCount);
            IKapBoard.IKapGetBufferStatus(hDev, 0, ref status);
            if (status.uFull == 1)
            {
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_SIZE, ref nFrameSize);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref nFrameWidht);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref nFrameHeight);
                IKapBoard.IKapGetBufferAddress(hDev, 0, ref pUserBuffer);
                hv_Width[1] = nFrameWidht;
                hv_Height[1] = nFrameHeight;
                HOperatorSet.GenImage1(out hv_Image[1], "byte", nFrameWidht, nFrameHeight, pUserBuffer);
                LogHelper.WriteInfo("Grab frame ImageEvent1");
                MainForm.ImageEvent[1].Set();
            }
        }
        public void OnFrameReadyFunc2(IntPtr pParam)
        {
            LogHelper.WriteInfo("Grab frame ready2");
            IntPtr hDev = (IntPtr)pParam;
            IntPtr pUserBuffer = IntPtr.Zero;
            int nFrameSize = 0;
            int nFrameWidht = 0;
            int nFrameHeight = 0;
            int nFrameCount = 0;
            IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();
            IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_COUNT, ref nFrameCount);
            IKapBoard.IKapGetBufferStatus(hDev, 0, ref status);
            if (status.uFull == 1)
            {
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_SIZE, ref nFrameSize);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref nFrameWidht);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref nFrameHeight);
                IKapBoard.IKapGetBufferAddress(hDev, 0, ref pUserBuffer);
                hv_Width[2] = nFrameWidht;
                hv_Height[2] = nFrameHeight;
                HOperatorSet.GenImage1(out hv_Image[2], "byte", nFrameWidht, nFrameHeight, pUserBuffer);
                LogHelper.WriteInfo("Grab frame ImageEvent2");
                MainForm.ImageEvent[2].Set();
            }
        }
        public void OnFrameReadyFunc3(IntPtr pParam)
        {
            LogHelper.WriteInfo("Grab frame ready3");
            IntPtr hDev = (IntPtr)pParam;
            IntPtr pUserBuffer = IntPtr.Zero;
            int nFrameSize = 0;
            int nFrameWidht = 0;
            int nFrameHeight = 0;
            int nFrameCount = 0;
            IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();
            IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_COUNT, ref nFrameCount);
            IKapBoard.IKapGetBufferStatus(hDev, 0, ref status);
            if (status.uFull == 1)
            {
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_SIZE, ref nFrameSize);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref nFrameWidht);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref nFrameHeight);
                IKapBoard.IKapGetBufferAddress(hDev, 0, ref pUserBuffer);
                hv_Width[3] = nFrameWidht;
                hv_Height[3] = nFrameHeight;
                HOperatorSet.GenImage1(out hv_Image[3], "byte", nFrameWidht, nFrameHeight, pUserBuffer);
                LogHelper.WriteInfo("Grab frame ImageEvent3");
                MainForm.ImageEvent[3].Set();
            }
        }

        /* @brief：本函数被注册为一个回调函数。当图像采集停止时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When stopping grabbing images, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnGrabStopFunc(IntPtr pParam)
        {
            LogHelper.WriteInfo("Stop grabbing image");
        }
        #endregion

        #region member function

        /* @brief：初始化IKapC 运行环境。
         *
         * @brief：Initialize IKapC runtime environment. */
        private bool InitEnvironment()
        {
            // IKapC 函数返回值。
            //
            // Return value of IKapC functions.
            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;

            res = IKapCLib.ItkManInitialize();
            return CheckIKapC(res);
        }

        /* @brief：释放 IKapC 运行环境。
         *
         * @brief：Release IKapC runtime environment. */
        private void ReleaseEnvironment()
        {
            IKapCLib.ItkManTerminate();
        }

        /* @brief：配置相机设备。
         *
         * @brief：Configure camera device. */
        private bool ConfigureCamera(int i)
        {
            try
            {
                uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
                // 枚举可用相机的数量。在打开相机前，必须调用 ItkManGetDeviceCount() 函数。
                //
                // Enumerate the number of available cameras. Before opening the camera, ItkManGetDeviceCount() function must be called.
                res = IKapCLib.ItkManGetDeviceCount(ref numCameras);
                //if(res==0)
                //{
                //    return false;
                //}
                CheckIKapC(res);

                // 当没有连接的相机时。
                //
                // When there is no connected cameras.
                if (numCameras == 0)
                {
                    LogHelper.WriteInfo("No CL  camera.");
                    IKapCLib.ItkManTerminate();
                    return false;
                }

                // 打开CameraLink相机。
                //
                // Open CameraLink camera.

                IKapCLib.ITKDEV_INFO di = new IKapCLib.ITKDEV_INFO();
                res = IKapCLib.ItkManGetDeviceInfo((uint)i, ref di);
                // 当设备为 CameraLink 相机且序列号正确时。
                //
                // When the device is CameraLink camera and the serial number is proper.
                if (di.DeviceClass == "CameraLink" && di.SerialNumber != "")
                {
                    // 获取相机设备信息。
                    //
                    // Get camera device information.

                    LogHelper.WriteInfo("Cam" + i + ":serial:{0},name:{1}, interface:{2}." + di.SerialNumber + di.FullName + di.DeviceClass);
                    IKapCLib.ITK_CL_DEV_INFO cl_board_info = new IKapCLib.ITK_CL_DEV_INFO();

                    // 打开相机。
                    //
                    // Open camera.
                    res = IKapCLib.ItkDevOpen((uint)i, (int)ItkDeviceAccessMode.ITKDEV_VAL_ACCESS_MODE_EXCLUSIVE, ref m_hCamera[i]);
                    CheckIKapC(res);

                    // 获取 CameraLink 相机设备信息。
                    //
                    // Get CameraLink camera device information.
                    res = IKapCLib.ItkManGetCLDeviceInfo((uint)i, ref cl_board_info);
                    CheckIKapC(res);

                    // 打开采集卡。
                    //
                    // Open frame grabber.
                    m_hBoard[i] = IKapBoard.IKapOpen(cl_board_info.HostInterface, cl_board_info.BoardIndex);
                    if (m_hBoard.Equals(-1))
                        CheckIKapBoard((int)IKapBoardClassLibrary.ErrorCode.IKStatus_OpenBoardFail);

                    return true;
                }
                else
                {
                    LogHelper.WriteInfo("No CLCam");
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /* @brief：配置采集卡设备。
         *
         * @brief：Configure frame grabber device. */
        public bool ConfigureFrameGrabber(int i)
        {
            try
            {
                int ret = (int)IKapBoardClassLibrary.ErrorCode.IK_RTN_OK;
                string configFileName;

                // 导入配置文件。
                //
                // Load configuration file.
                m_strCamName[i] = Parameters.commministion.productName + "/CamParams" + i + ".vlcf";
                configFileName = m_strCamName[i];
                if (configFileName == null)
                {
                    LogHelper.WriteInfo("Fail to get configuration, using default setting!\n");
                }
                else
                {
                    ret = IKapBoard.IKapLoadConfigurationFromFile(m_hBoard[i], configFileName);
                    CheckIKapBoard(ret);
                }
                ret = IKapBoard.IKapSetInfo(m_hBoard[i], (uint)INFO_ID.IKP_IMAGE_HEIGHT, Halcon.hv_Height[i]);
                CheckIKapBoard(ret);
                //设置图像缓冲区帧数。
                //
                // Set frame count of buffer.
                ret = IKapBoard.IKapSetInfo(m_hBoard[i], (uint)INFO_ID.IKP_FRAME_COUNT, m_nTotalFrameCount);
                CheckIKapBoard(ret);

                // 设置超时时间。
                //
                // Set time out time.
                int timeout = -1;
                ret = IKapBoard.IKapSetInfo(m_hBoard[i], (uint)INFO_ID.IKP_TIME_OUT, timeout);
                CheckIKapBoard(ret);

                // 设置采集模式。
                //
                // Set grab mode.
                int grab_mode = (int)GrabMode.IKP_GRAB_NON_BLOCK;
                ret = IKapBoard.IKapSetInfo(m_hBoard[i], (uint)INFO_ID.IKP_GRAB_MODE, grab_mode);
                CheckIKapBoard(ret);

                // 设置传输模式。
                //
                // Set transfer mode.
                int transfer_mode = (int)FrameTransferMode.IKP_FRAME_TRANSFER_SYNCHRONOUS_NEXT_EMPTY_WITH_PROTECT;
                ret = IKapBoard.IKapSetInfo(m_hBoard[i], (uint)INFO_ID.IKP_FRAME_TRANSFER_MODE, transfer_mode);
                CheckIKapBoard(ret);

                // 注册回调函数
                //
                // Register callback functions.
                OnGrabStartProc[i] = new IKapCallBackProc(OnGrabStartFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_GrabStart, Marshal.GetFunctionPointerForDelegate(OnGrabStartProc[i]), m_hBoard[i]);
                //GC.KeepAlive(OnGrabStartProc);//关键的两句
                //GC.KeepAlive(OnGrabStartProc);//关键的两句
                CheckIKapBoard(ret);
                if (i == 0)
                {
                    OnFrameReadyProc[i] = new IKapCallBackProc(OnFrameReadyFunc0);                   
                }
                if (i == 1)
                {
                    OnFrameReadyProc[i] = new IKapCallBackProc(OnFrameReadyFunc1);
                }
                if (i == 2)
                {
                    OnFrameReadyProc[i] = new IKapCallBackProc(OnFrameReadyFunc2);
                }
                if (i == 3)
                {
                    OnFrameReadyProc[i] = new IKapCallBackProc(OnFrameReadyFunc3);
                }
                
                ret = IKapBoard.IKapRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_FrameReady, Marshal.GetFunctionPointerForDelegate(OnFrameReadyProc[i]), m_hBoard[i]);
                CheckIKapBoard(ret);
                OnFrameLostProc[i] = new IKapCallBackProc(OnFrameLostFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_FrameLost, Marshal.GetFunctionPointerForDelegate(OnFrameLostProc[i]), m_hBoard[i]);

                CheckIKapBoard(ret);
                OnTimeoutProc[i] = new IKapCallBackProc(OnTimeoutFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_TimeOut, Marshal.GetFunctionPointerForDelegate(OnTimeoutProc[i]), m_hBoard[i]);
                CheckIKapBoard(ret);
                OnGrabStopProc[i] = new IKapCallBackProc(OnGrabStopFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_GrabStop, Marshal.GetFunctionPointerForDelegate(OnGrabStopProc[i]), m_hBoard[i]);
                CheckIKapBoard(ret);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /* @brief：清除回调函数。
         *
         * @brief：Unregister callback functions. */
        private void UnRegisterCallback(int i)
        {
            int ret = (int)IKapBoardClassLibrary.ErrorCode.IK_RTN_OK;
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_GrabStart);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_FrameReady);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_FrameLost);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_TimeOut);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard[i], (uint)CallBackEvents.IKEvent_GrabStop);
        }

        /* @brief：关闭设备。
         *
         * @brief：Close device. */
        public void CloseIKapBoard(int i)
        {
            try
            {
                // 关闭采集卡设备。
                //
                // Close frame grabber device.
                if (!m_hBoard.Equals(-1))
                {
                    IKapBoard.IKapClose(m_hBoard[i]);
                    m_hBoard[i] = (IntPtr)(-1);
                }

                // 关闭相机设备。
                //
                // Close camera device.
                if (!m_hCamera[i].Equals(-1))
                {
                    IKapCLib.ItkDevClose(m_hCamera[i]);
                    m_hCamera[i] = (IntPtr)(-1);
                }
            }
            catch (Exception e)
            {

            }

        }
        #endregion
        public static bool StartGrabImage(int i, IKapCLineCam grab)
        {
            try
            {
                int ret = 0;
                // 开始图像采集。
                //
                // Start grabbing images.
                ret = IKapBoard.IKapStartGrab(grab.m_hBoard[i], 0);
                CheckIKapBoard(ret);
                // 等待图像采集结束。
                //
                // Wait for grabbing images finished.
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public static bool StopGrabImage(int i, IKapCLineCam grab)
        {
            try
            {
                // 停止图像采集。
                //
                // Stop grabbing images.
                int ret = 0;
                ret = IKapBoard.IKapStopGrab(grab.m_hBoard[i]);
                CheckIKapBoard(ret);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public static bool ColseDevice(int i, IKapCLineCam grab)
        {
            try
            {
                // 清除回调函数。
                //
                // Unregister callback functions.
                grab.UnRegisterCallback(i);

                // 关闭设备。
                //
                // Close device.
                grab.CloseIKapBoard(i);

                // 释放 IKapC 运行环境。
                //
                // Release IKapC runtime environment.
                grab.ReleaseEnvironment();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool OpenDevice(int i, IKapCLineCam grab)
        {
            try
            {
                // 初始化 IKapC 运行环境。
                //
                // Initialize IKapC runtime environment.
                bool res = grab.InitEnvironment();
                if (!res)
                {
                    return false;
                }
                // 配置相机设备。
                //
                // Configure camera device.
                res = grab.ConfigureCamera(i);
                if (!res)
                {
                    return false;
                }
                // 配置采集卡设备。
                //
                // Configure frame grabber device.
                res = grab.ConfigureFrameGrabber(i);
                if (!res)
                {
                    return false;
                }
                m_hCameraResult[i] = true;
                return true;
            }
            catch (Exception e)
            {
                m_hCameraResult[i] = false;
                return false;
            }
        }
    }
}
