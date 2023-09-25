using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using MvFGCtrlC.NET;
using WY_App.Utility;

namespace WY_App
{
    public partial class 通讯设置 : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public enum SAVE_IAMGE_TYPE
        {
            Image_Undefined = 0,            // ch:未定义的图像格式 | en:Undefined Image Type
            Image_Bmp = 1,            // ch:BMP图像格式 | en:BMP Image Type
            Image_Jpeg = 2,            // ch:JPEG图像格式 | en:Jpeg Image Type
        }

        // ch:判断用户自定义像素格式 | en:Determine custom pixel format
        public const Int32 CUSTOMER_PIXEL_FORMAT = unchecked((Int32)0x80000000);

        public const UInt32 TRIGGER_MODE_ON = 1;   // ch:触发模式开 | en:Trigger mode on
        public const UInt32 TRIGGER_MODE_OFF = 0;   // ch:触发模式关 | en:Trigger mode off

        CSystem m_cSystem = new CSystem();            // ch:操作采集卡 | en:Interface operations
        CInterface m_cInterface = null;                  // ch:操作采集卡和设备 | en:Interface and device operation
        CDevice m_cDevice = null;                     // ch:操作设备和流 | en:Device and stream operation
        CStream m_cStream = null;                     // ch:操作流和缓存 | en:Stream and buffer operation

        uint m_nInterfaceNum = 0;                   // ch:采集卡数量 | en:Interface number
        bool m_bIsIFOpen = false;                   // ch:采集卡是否打开 | en:Whether to open interface
        bool m_bIsDeviceOpen = false;               // ch:设备是否打开 | en:Whether to open device
        bool m_bIsGrabbing = false;                 // ch:是否在抓图 | en:Whether to start grabbing
        uint m_nTriggerMode = TRIGGER_MODE_OFF;     // ch:触发模式 | en:Trigger Mode

        bool m_bThreadState = false;        // ch:线程状态 | en:Thread state
        Thread m_hGrabThread = null;          // ch:取流线程 | en:Grabbing thread

        private static Object m_LockForSaveImage = new Object();              // ch:存图锁 | en:Lock for saving image
        IntPtr m_pDataBuf = IntPtr.Zero;                       // ch:数据缓存 | en:Data buffer
        uint m_nDataBufSize = 0;                             // ch:数据缓存大小 | en:Length of data buffer
        IntPtr m_pSaveImageBuf = IntPtr.Zero;                  // ch:图像缓存 | en:Image buffer
        uint m_nSaveImageBufSize = 0;                        // ch:图像缓存大小 | en:Length of image buffer
        MV_FG_INPUT_IMAGE_INFO m_stImageInfo = new MV_FG_INPUT_IMAGE_INFO();   // ch:图像信息 | en:Image info

        delegate void ShowDisplayError(int nRet);


        public 通讯设置()
        {
            InitializeComponent();
        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        Point downPoint;
        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            downPoint = new Point(e.X, e.Y);
        }

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - downPoint.X,
                    this.Location.Y + e.Y - downPoint.Y);
            }
        }

       
        private void ParamSettings_Load(object sender, EventArgs e)
        {
            txt_PlcIpAddress.Text = Parameters.commministion.PlcIpAddress;
            num_PLCPort.Value = Parameters.commministion.PlcIpPort;
            txt_PlcDevice.Text = Parameters.commministion.PlcDevice;
            txt_PlcType.Text = Parameters.commministion.PlcType;
            chk_PLCEnabled.Checked = Parameters.commministion.PlcEnable;

            txt_ImageSavePath.Text = Parameters.commministion.ImageSavePath;
            txt_ServerIP.Text = Parameters.commministion.TcpServerIpAddress;
            num_ServerPort.Value = Parameters.commministion.TcpServerIpPort;
            chk_ServerEnabled.Checked = Parameters.commministion.TcpServerEnable;

            txt_ClientIP.Text = Parameters.commministion.TcpClientIpAddress;
            num_ClientPort.Value = Parameters.commministion.TcpClientIpPort;
            chk_ClientEnabled.Checked = Parameters.commministion.TcpClientEnable;
            num_LogSaveDays.Value = Parameters.commministion.LogFileExistDay;

            
            txt_HeartBeat_Add.Text = Parameters.plcParams.HeartBeatAdd;
            txt_StartAdd.Text = Parameters.plcParams.StartAdd;
            txt_Trigger_Detection0.Text = Parameters.plcParams.Trigger_Detection0;
            txt_Completion_Add0.Text = Parameters.plcParams.Completion0;
            txt_Trigger_Detection1.Text = Parameters.plcParams.Trigger_Detection1;
            txt_Completion_Add1.Text = Parameters.plcParams.Completion1;
            txt_Trigger_Detection2.Text = Parameters.plcParams.Trigger_Detection2;
            txt_Completion_Add2.Text = Parameters.plcParams.Completion2;

			ImageUploadPath1.Text = Parameters.commministion.ImageUploadOne;
			ImageUploadPath2.Text = Parameters.commministion.ImageUploadTwo;
			ImageUploadPath3.Text = Parameters.commministion.ImageUploadThree;

			check_ImagePath1.Checked = Parameters.commministion.UploadOneEnable;
			check_ImagePath1.Checked = Parameters.commministion.UploadTwoEnable;
			check_ImagePath1.Checked = Parameters.commministion.UploadThreeEnable;

			
			check_ImageTestPath1.Checked = Parameters.commministion.UploadOneTestEnable;
			check_ImageTestPath1.Checked = Parameters.commministion.UploadTwoTestEnable;
			check_ImageTestPath1.Checked = Parameters.commministion.UploadThreeTestEnable;

			uiTextBox25.Text = Parameters.plcParams.预留地址[0];
			uiTextBox24.Text = Parameters.plcParams.预留地址[1];
			uiTextBox23.Text = Parameters.plcParams.预留地址[2];
			uiTextBox22.Text = Parameters.plcParams.预留地址[3];
			uiTextBox21.Text = Parameters.plcParams.预留地址[4];

            uiTextBox13.Text = Parameters.plcParams.预留地址[5] ;
            uiTextBox15.Text = Parameters.plcParams.预留地址[6] ;
            uiTextBox14.Text = Parameters.plcParams.预留地址[7] ;
            uiTextBox12.Text = Parameters.plcParams.预留地址[8] ;
            uiTextBox11.Text = Parameters.plcParams.预留地址[9] ;
            uiTextBox10.Text = Parameters.plcParams.预留地址[10] ;
            uiTextBox6.Text = Parameters.plcParams.预留地址[11] ;
            uiTextBox1.Text = Parameters.plcParams.预留地址[12];
            uiTextBox20.Text = Parameters.plcParams.预留地址[13];
            uiTextBox19.Text = Parameters.plcParams.预留地址[14];

            uiTextBox26.Text = Parameters.plcParams.预留地址[15];
            uiTextBox27.Text = Parameters.plcParams.预留地址[16];
            uiTextBox28.Text = Parameters.plcParams.预留地址[17];
            uiTextBox29.Text = Parameters.plcParams.预留地址[18];
            uiTextBox30.Text = Parameters.plcParams.预留地址[19];
            uiTextBox35.Text = Parameters.plcParams.预留地址[20];
            uiTextBox34.Text = Parameters.plcParams.预留地址[21];
            uiTextBox33.Text = Parameters.plcParams.预留地址[22];
            uiTextBox32.Text = Parameters.plcParams.预留地址[23];
            uiTextBox31.Text = Parameters.plcParams.预留地址[24];

            uiTextBox51.Text = Parameters.plcParams.预留地址[25];
            uiTextBox50.Text = Parameters.plcParams.预留地址[26];
            uiTextBox49.Text = Parameters.plcParams.预留地址[27];
            uiTextBox48.Text = Parameters.plcParams.预留地址[28];
            uiTextBox47.Text = Parameters.plcParams.预留地址[29];
            uiTextBox46.Text = Parameters.plcParams.预留地址[30];
            uiTextBox45.Text = Parameters.plcParams.预留地址[31];
            uiTextBox44.Text = Parameters.plcParams.预留地址[32];
            uiTextBox43.Text = Parameters.plcParams.预留地址[33];
            uiTextBox42.Text = Parameters.plcParams.预留地址[34];


            uiTextBox18.Text = Parameters.plcParams.预留地址[35] ;
            uiTextBox17.Text = Parameters.plcParams.预留地址[36] ;
            uiTextBox38.Text = Parameters.plcParams.预留地址[37] ;
            uiTextBox37.Text = Parameters.plcParams.预留地址[38] ;
            uiTextBox41.Text = Parameters.plcParams.预留地址[39] ;
            uiTextBox40.Text = Parameters.plcParams.预留地址[40] ;

			uiTextBox16.Text = Parameters.plcParams.预留地址[41];
			uiTextBox36.Text = Parameters.plcParams.预留地址[42];

			uiTextBox25.Text = Parameters.plcParams.预留地址[0];
			uiTextBox39.Text = Parameters.plcParams.预留地址[43];
		}

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_ChangePassword_Click(object sender, EventArgs e)
        {
            txt_PlcIpAddress.Enabled = true;
            num_PLCPort.Enabled = true;
            txt_PlcDevice.Enabled = true;
            txt_PlcType.Enabled = true;
            chk_PLCEnabled.Enabled = true;

            txt_ServerIP.Enabled = true;
            num_ServerPort.Enabled = true;
            chk_ServerEnabled.Enabled = true;

            txt_ClientIP.Enabled = true;
            num_ClientPort.Enabled = true;
            chk_ClientEnabled.Enabled = true;

            txt_Trigger_Detection0.Enabled = true;
            txt_HeartBeat_Add.Enabled = true;
            txt_StartAdd.Enabled = true;
            txt_Completion_Add0.Enabled = true;
            num_LogSaveDays.Enabled = true;

			txt_Trigger_Detection1.Enabled = true;
			txt_Completion_Add1.Enabled = true;

			txt_Trigger_Detection2.Enabled = true;
			txt_Completion_Add2.Enabled = true;

			uiTextBox25.Enabled = true;
			uiTextBox24.Enabled = true;
			uiTextBox23.Enabled = true;
			uiTextBox22.Enabled = true;
			uiTextBox21.Enabled = true;

            uiTextBox13.Enabled = true;
            uiTextBox15.Enabled = true;
            uiTextBox14.Enabled = true;
            uiTextBox12.Enabled = true;
            uiTextBox11.Enabled = true;
            uiTextBox10.Enabled = true;
            uiTextBox6.Enabled = true;
            uiTextBox1.Enabled = true;
            uiTextBox20.Enabled = true;
            uiTextBox19.Enabled = true;

            uiTextBox26.Enabled = true;
            uiTextBox27.Enabled = true;
            uiTextBox28.Enabled = true;
            uiTextBox29.Enabled = true;
            uiTextBox30.Enabled = true;
            uiTextBox35.Enabled = true;
            uiTextBox34.Enabled = true;
            uiTextBox33.Enabled = true;
            uiTextBox32.Enabled = true;
            uiTextBox31.Enabled = true;

            uiTextBox51.Enabled = true;
            uiTextBox50.Enabled = true;
            uiTextBox49.Enabled = true;
            uiTextBox48.Enabled = true;
            uiTextBox47.Enabled = true;
            uiTextBox46.Enabled = true;
            uiTextBox45.Enabled = true;
            uiTextBox44.Enabled = true;
            uiTextBox43.Enabled = true;
            uiTextBox42.Enabled = true;

            uiTextBox18.Enabled = true;
            uiTextBox17.Enabled = true;
            uiTextBox38.Enabled = true;
            uiTextBox37.Enabled = true;
            uiTextBox41.Enabled = true;
            uiTextBox40.Enabled = true;

			uiTextBox16.Enabled = true;
			uiTextBox36.Enabled = true;

			txt_ImageSavePath.Enabled = true;
            btn_Save.Enabled = true;

			ImageUploadPath1.Enabled = true;
			ImageUploadPath2.Enabled = true;
			ImageUploadPath3.Enabled = true;

			check_ImagePath1.Enabled = true;
			check_ImagePath2.Enabled = true;
			check_ImagePath3.Enabled = true;

			uiTextBox39.Enabled = true;

			check_ImageTestPath1.Enabled = true;
			check_ImageTestPath2.Enabled = true;
			check_ImageTestPath3.Enabled = true;
		}

        private void btn_Save_Click(object sender, EventArgs e)
        {
            Parameters.commministion.PlcIpAddress = txt_PlcIpAddress.Text;
            Parameters.commministion.PlcIpPort = (int)num_PLCPort.Value;
            Parameters.commministion.PlcDevice = txt_PlcDevice.Text;
            Parameters.commministion.PlcType = txt_PlcType.Text;
            Parameters.commministion.PlcEnable = chk_PLCEnabled.Checked;

            Parameters.commministion.ImageSavePath = txt_ImageSavePath.Text; ;
            Parameters.commministion.TcpServerIpAddress = txt_ServerIP.Text;
            Parameters.commministion.TcpServerIpPort = (int)num_ServerPort.Value;
            Parameters.commministion.TcpServerEnable = chk_ServerEnabled.Checked;

            Parameters.commministion.TcpClientIpAddress = txt_ClientIP.Text;
            Parameters.commministion.TcpClientIpPort = (int)num_ClientPort.Value;
            Parameters.commministion.TcpClientEnable = chk_ClientEnabled.Checked;
            Parameters.commministion.LogFileExistDay = (int)num_LogSaveDays.Value;

			Parameters.commministion.ImageUploadOne = ImageUploadPath1.Text;
			Parameters.commministion.ImageUploadTwo = ImageUploadPath2.Text;
			Parameters.commministion.ImageUploadThree = ImageUploadPath3.Text;

			Parameters.commministion.UploadOneEnable = check_ImagePath1.Checked;
			Parameters.commministion.UploadTwoEnable = check_ImagePath2.Checked;
			Parameters.commministion.UploadThreeEnable = check_ImagePath3.Checked;

			Parameters.commministion.UploadOneTestEnable = check_ImageTestPath1.Checked;
			Parameters.commministion.UploadTwoTestEnable = check_ImageTestPath2.Checked;
			Parameters.commministion.UploadThreeTestEnable = check_ImageTestPath3.Checked;


			XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");

            Parameters.plcParams.HeartBeatAdd = txt_HeartBeat_Add.Text;
            Parameters.plcParams.StartAdd = txt_StartAdd.Text;
            Parameters.plcParams.Trigger_Detection0 = txt_Trigger_Detection0.Text;
            Parameters.plcParams.Completion0 = txt_Completion_Add0.Text;
            Parameters.plcParams.Trigger_Detection1 = txt_Trigger_Detection1.Text;
            Parameters.plcParams.Completion1 = txt_Completion_Add1.Text;
            Parameters.plcParams.Trigger_Detection2 = txt_Trigger_Detection2.Text;
            Parameters.plcParams.Completion2 = txt_Completion_Add2.Text;

			
			Parameters.plcParams.预留地址[1] = uiTextBox24.Text;
			Parameters.plcParams.预留地址[2] = uiTextBox23.Text;
			Parameters.plcParams.预留地址[3] = uiTextBox22.Text;
			Parameters.plcParams.预留地址[4] = uiTextBox21.Text;

            Parameters.plcParams.预留地址[5] = uiTextBox13.Text;
            Parameters.plcParams.预留地址[6] = uiTextBox15.Text;
            Parameters.plcParams.预留地址[7] = uiTextBox14.Text;
            Parameters.plcParams.预留地址[8] = uiTextBox12.Text;
            Parameters.plcParams.预留地址[9] = uiTextBox11.Text;
            Parameters.plcParams.预留地址[10] = uiTextBox10.Text;
            Parameters.plcParams.预留地址[11] = uiTextBox6.Text;
            Parameters.plcParams.预留地址[12] = uiTextBox1.Text;
            Parameters.plcParams.预留地址[13] = uiTextBox20.Text;
            Parameters.plcParams.预留地址[14] = uiTextBox19.Text;

            Parameters.plcParams.预留地址[15] = uiTextBox26.Text;
            Parameters.plcParams.预留地址[16] = uiTextBox27.Text;
            Parameters.plcParams.预留地址[17] = uiTextBox28.Text;
            Parameters.plcParams.预留地址[18] = uiTextBox29.Text;
            Parameters.plcParams.预留地址[19] = uiTextBox30.Text;
            Parameters.plcParams.预留地址[20] = uiTextBox35.Text;
            Parameters.plcParams.预留地址[21] = uiTextBox34.Text;
            Parameters.plcParams.预留地址[22] = uiTextBox33.Text;
            Parameters.plcParams.预留地址[23] = uiTextBox32.Text;
            Parameters.plcParams.预留地址[24] = uiTextBox31.Text;

            Parameters.plcParams.预留地址[25] = uiTextBox51.Text;
            Parameters.plcParams.预留地址[26] = uiTextBox50.Text;
            Parameters.plcParams.预留地址[27] = uiTextBox49.Text;
            Parameters.plcParams.预留地址[28] = uiTextBox48.Text;
            Parameters.plcParams.预留地址[29] = uiTextBox47.Text;
            Parameters.plcParams.预留地址[30] = uiTextBox46.Text;
            Parameters.plcParams.预留地址[31] = uiTextBox45.Text;
            Parameters.plcParams.预留地址[32] = uiTextBox44.Text;
            Parameters.plcParams.预留地址[33] = uiTextBox43.Text;
            Parameters.plcParams.预留地址[34] = uiTextBox42.Text;


            Parameters.plcParams.预留地址[35] = uiTextBox18.Text;
            Parameters.plcParams.预留地址[36] = uiTextBox17.Text;
            Parameters.plcParams.预留地址[37] = uiTextBox38.Text;
            Parameters.plcParams.预留地址[38] = uiTextBox37.Text;
            Parameters.plcParams.预留地址[39] = uiTextBox41.Text;
            Parameters.plcParams.预留地址[40] = uiTextBox40.Text;

			Parameters.plcParams.预留地址[41] = uiTextBox16.Text;
			Parameters.plcParams.预留地址[42] = uiTextBox36.Text;

			Parameters.plcParams.预留地址[0] = uiTextBox25.Text;
			Parameters.plcParams.预留地址[43] = uiTextBox39.Text;

			XMLHelper.serialize<Parameters.PLCParams>(Parameters.plcParams, "Parameter/PLCParams.xml");



           
            XMLHelper.serialize<Parameters.CameraParam>(Parameters.cameraParam, "Parameter/CameraParam.xml");


            MessageBox.Show("系统参数修改，请重启软件");
            this.Close();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label44_Click(object sender, EventArgs e)
        {

        }

		private void panel3_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}
