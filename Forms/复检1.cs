using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WY_App.Utility;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static WY_App.Utility.Parameters;

namespace WY_App.Forms
{
	public partial class 复检1 : Form
	{
		static HWindow[] hWindows;
		HWindow[] hWindow = new HWindow[1];
		HWindow[] hWindowFive = new HWindow[1];
		static HRect1[,] BaseReault = new HRect1[3, 4];

		int mouse_X0, mouse_X1, mouse_Y0, mouse_Y1;   //用来记录按下/抬起鼠标时的坐标位置

		public 复检1()
		{
			InitializeComponent();

			hWindow[0] = hWindowControl4.HalconWindow;
			hWindows = new HWindow[3] { hWindowControl1.HalconWindow, hWindowControl2.HalconWindow, hWindowControl3.HalconWindow};
			hWindowFive[0] = hWindowControl5.HalconWindow;
			HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格
			HOperatorSet.SetPart(hWindows[1], 0, 0, 1000, 1000);//设置窗体的规格
			HOperatorSet.SetPart(hWindows[2], 0, 0, 1000, 1000);//设置窗体的规格
			HOperatorSet.SetPart(hWindowFive[0], 0, 0, -1, -1);//设置窗体的规格
			HOperatorSet.SetPart(hWindow[0], 0, 0, -1, -1);//设置窗体的规格

			HOperatorSet.DispObj(MainForm.hImage[1], hWindows[0]);
			HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[1]);
			HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[2]);
			HOperatorSet.DispObj(MainForm.hImage[2], hWindowFive[0]);

			HOperatorSet.SetPart(hWindow[0], 0, 0, -1, -1);//设置窗体的规格
			HOperatorSet.DispObj(MainForm.hImage[0], hWindow[0]);
		}

		private void groupBox2_Enter(object sender, EventArgs e)
		{

		}

		private void 复检1_Load(object sender, EventArgs e)
		{
			HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格            
			HOperatorSet.DispObj(MainForm.hImage[1], hWindows[0]);
			HOperatorSet.SetPart(hWindow[0], 0, 0, -1, -1);//设置窗体的规格
			HOperatorSet.DispObj(MainForm.hImage[0], hWindow[0]);
			HOperatorSet.SetPart(hWindowFive[0], 0, 0, -1, -1);//设置窗体的规格            
			HOperatorSet.DispObj(MainForm.hImage[2], hWindowFive[0]);

			DateTime dtNow = System.DateTime.Now;
			string strDateDay = dtNow.ToString("yyyy-MM-dd");
			text_TestDate.Text = strDateDay;
			text_SheetNums.Text = HslCommunication.STRproduct;

			string targetPath = Parameters.commministion.ImageSavePath + strDateDay + "\\" + HslCommunication.STRproduct; // 替换为你的目标路径

			if (Directory.Exists(targetPath))
			{
				string[] folderNames = Directory.GetDirectories(targetPath);

				foreach (string folderPath in folderNames)
				{
					// 获取文件夹名称，并将其添加到下拉框
					string folderName = Path.GetFileName(folderPath);
					com_SheetNum.Items.Add(folderName);
				}
				com_SheetNum.SelectedIndex = 0;
			}
			else
			{
				MessageBox.Show("目标路径不存在！");
			}


		}


		List<DetectionResult> detectionResults;

		HObject AreahObject = new HObject();

		private void hWindowControl4_HMouseDown(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Clicks == 1)
				{
					int tempNum = 0;
					hWindow[0].GetMposition(out mouse_X0, out mouse_Y0, out tempNum);  //记录按下鼠标时的位置
				}
				
			}
			else if(e.Button == MouseButtons.Right)
			{
				if (e.Clicks == 1)
				{
					HTuple X = new HTuple();
					HTuple Y = new HTuple();
					HTuple row = new HTuple();
					HTuple col = new HTuple();
					HTuple grayval = new HTuple();
					hWindows[1].SetPart(0, 0, 400, 400);
					HOperatorSet.GetImageSize(MainForm.hImage[0], out X, out Y);
					//hWindowControl1.Size = new System.Drawing.Size(X, Y);
					hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, X, Y);
					if ((int)e.X < 200)
					{
						col = 200;
					}
					else if ((int)e.X > Halcon.hv_Width[0] - 200)
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
					else if ((int)e.Y > Halcon.hv_Height[0] - 200)
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
						HOperatorSet.CropPart(MainForm.hImage[0], out AreahObject, row - 200, col - 200, 400, 400);
						HOperatorSet.DispObj(AreahObject, hWindows[1]);
					}
					catch (Exception ex)
					{

					}
					messageShow3.lab_Column.Text = (col * Parameters.detectionSpec[0].PixelResolutionColum - Parameters.detectionSpec[0].ColumBase[0] + Parameters.detectionSpec[0].ColumBase[1]).ToString();
					messageShow3.lab_Row.Text = (row * Parameters.detectionSpec[0].PixelResolutionRow - Parameters.detectionSpec[0].RowBase[0] + Parameters.detectionSpec[0].RowBase[1]).ToString();
					//HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[1]);
					X.Dispose();
					Y.Dispose();
					row.Dispose();
					col.Dispose();
					grayval.Dispose();
				}
				if (e.Clicks == 2)
				{
					DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
					MainForm.strDateTime = dtNow.ToString("HHmmss");
					MainForm.strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
					相机检测设置.set_display_font(hWindow[0], 10, "mono", "true", "false");
					detectionResults = new List<DetectionResult>();
					相机检测设置.Detection(0, hWindow, MainForm.hImage[0], ref detectionResults,false);
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
						if (detectionResults.Count == 0)
						{
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindow[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindow[0], "OK", "window", 12, 12, "black", "true");
						}
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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindow[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindow[0], "NG", "window", 12, 12, "black", "true");

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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindow[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindow[0], "NG", "window", 12, 12, "black", "true");

						}
					});
				}
			}
		}

		private void hWindowControl1_HMouseDown(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Clicks == 1)
				{
					int tempNum = 0;
					hWindows[0].GetMposition(out mouse_X0, out mouse_Y0, out tempNum);  //记录按下鼠标时的位置
				}

				
			}
			else if(e.Button == MouseButtons.Right)
			{
				if (e.Clicks == 1)
				{
					HTuple X = new HTuple();
					HTuple Y = new HTuple();
					HTuple row = new HTuple();
					HTuple col = new HTuple();
					HTuple grayval = new HTuple();
					hWindows[1].SetPart(0, 0, 400, 400);
					HOperatorSet.GetImageSize(MainForm.hImage[1], out X, out Y);
					//hWindowControl1.Size = new System.Drawing.Size(X, Y);
					hWindowControl4.ImagePart = new System.Drawing.Rectangle(0, 0, X, Y);
					if ((int)e.X < 200)
					{
						col = 200;
					}
					else if ((int)e.X > Halcon.hv_Width[1] - 200)
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
					else if ((int)e.Y > Halcon.hv_Height[1] - 200)
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
						HOperatorSet.CropPart(MainForm.hImage[1], out AreahObject, row - 200, col - 200, 400, 400);
						HOperatorSet.DispObj(AreahObject, hWindows[1]);
					}
					catch (Exception ex)
					{

					}
					messageShow3.lab_Column.Text = (col * Parameters.detectionSpec[1].PixelResolutionColum - Parameters.detectionSpec[1].ColumBase[0] + Parameters.detectionSpec[1].ColumBase[1]).ToString();
					messageShow3.lab_Row.Text = (row * Parameters.detectionSpec[1].PixelResolutionRow - Parameters.detectionSpec[1].RowBase[0] + Parameters.detectionSpec[1].RowBase[1]).ToString();
					//HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[1]);
					X.Dispose();
					Y.Dispose();
					row.Dispose();
					col.Dispose();
					grayval.Dispose();
				}
				if (e.Clicks == 2)
				{
					DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
					MainForm.strDateTime = dtNow.ToString("HHmmss");
					MainForm.strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
					MainForm.CamNum = 1;
					相机检测设置.set_display_font(hWindows[0], 10, "mono", "true", "false");
					detectionResults = new List<DetectionResult>();
					相机检测设置.Detection(1, hWindows, MainForm.hImage[1], ref detectionResults,false);
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
						if (detectionResults.Count == 0)
						{
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindows[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindows[0], "OK", "window", 12, 12, "black", "true");
						}
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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindows[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindows[0], "NG", "window", 12, 12, "black", "true");
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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindows[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindows[0], "NG", "window", 12, 12, "black", "true");
						}
					});
				}
			}

			
		}

		private void hWindowControl5_HMouseDown(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Clicks == 1)
				{
					int tempNum = 0;
					hWindowFive[0].GetMposition(out mouse_X0, out mouse_Y0, out tempNum);  //记录按下鼠标时的位置
				}
				
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (e.Clicks == 1)
				{
					HTuple X = new HTuple();
					HTuple Y = new HTuple();
					HTuple row = new HTuple();
					HTuple col = new HTuple();
					HTuple grayval = new HTuple();
					hWindows[1].SetPart(0, 0, 400, 400);
					HOperatorSet.GetImageSize(MainForm.hImage[2], out X, out Y);
					//hWindowControl1.Size = new System.Drawing.Size(X, Y);
					hWindowControl4.ImagePart = new System.Drawing.Rectangle(0, 0, X, Y);
					if ((int)e.X < 200)
					{
						col = 200;
					}
					else if ((int)e.X > Halcon.hv_Width[1] - 200)
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
					else if ((int)e.Y > Halcon.hv_Height[1] - 200)
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
						HOperatorSet.CropPart(MainForm.hImage[2], out AreahObject, row - 200, col - 200, 400, 400);
						HOperatorSet.DispObj(AreahObject, hWindows[1]);
					}
					catch (Exception ex)
					{

					}
					messageShow3.lab_Column.Text = (col * Parameters.detectionSpec[2].PixelResolutionColum - Parameters.detectionSpec[2].ColumBase[0] + Parameters.detectionSpec[2].ColumBase[1]).ToString();
					messageShow3.lab_Row.Text = (row * Parameters.detectionSpec[2].PixelResolutionRow - Parameters.detectionSpec[2].RowBase[0] + Parameters.detectionSpec[2].RowBase[1]).ToString();
					//HOperatorSet.DispObj(MainForm.hImage[MainForm.CamNum], hWindows[1]);
					X.Dispose();
					Y.Dispose();
					row.Dispose();
					col.Dispose();
					grayval.Dispose();
				}
				if (e.Clicks == 2)
				{
					DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
					MainForm.strDateTime = dtNow.ToString("HHmmss");
					MainForm.strDateTimeDay = dtNow.ToString("yyyy-MM-dd");
					MainForm.CamNum = 2;
					相机检测设置.set_display_font(hWindowFive[0], 10, "mono", "true", "false");
					detectionResults = new List<DetectionResult>();
					相机检测设置.Detection(2, hWindowFive, MainForm.hImage[2], ref detectionResults,false);
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
						if (detectionResults.Count == 0)
						{
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindowFive[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindowFive[0], "OK", "window", 12, 12, "black", "true");
						}
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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindowFive[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindowFive[0], "NG", "window", 12, 12, "black", "true");
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
							HOperatorSet.SetWindowAttr("background_color", "black");
							相机检测设置.set_display_font(hWindowFive[0], 67, "mono", "true", "false");
							相机检测设置.disp_message(hWindowFive[0], "NG", "window", 12, 12, "black", "true");
						}
					});
				}
			}
		}

		private void hWindowControl5_HMouseWheel(object sender, HMouseEventArgs e)
		{
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(LMap_MouseWheel5);

		}

		public void LMap_MouseWheel5(object sender, MouseEventArgs e)
		{
			//当e.Delta > 0时鼠标滚轮是向上滚动，e.Delta < 0时鼠标滚轮向下滚动
			if (e.Delta > 0)//滚轮向上
			{
				Halcon.ImgZoomDouble(MainForm.hImage[2], hWindowFive[0], 1);
			}
			else
			{
				Halcon.ImgZoomDouble(MainForm.hImage[2], hWindowFive[0], 0);
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
				Halcon.ImgZoomDouble(MainForm.hImage[1], hWindows[0], 1);
			}
			else
			{
				Halcon.ImgZoomDouble(MainForm.hImage[1], hWindows[0], 0);
			}

		}
		private void hWindowControl4_HMouseWheel(object sender, HMouseEventArgs e)
		{
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(LMap_MouseWheel4);
		}

		public void LMap_MouseWheel4(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)//滚轮向上
			{
				Halcon.ImgZoomDouble(MainForm.hImage[0], hWindow[0], 1);

			}
			else
			{
				Halcon.ImgZoomDouble(MainForm.hImage[0], hWindow[0], 0);
			}

		}
				
		private void hWindowControl2_HMouseDown(object sender, HMouseEventArgs e)
		{
			if (e.Clicks == 2)
			{
				HOperatorSet.SetPart(hWindow[0], 0, 0, -1, -1);//设置窗体的规格
				HOperatorSet.DispObj(MainForm.hImage[0], hWindow[0]);//显示图片

				HOperatorSet.SetPart(hWindows[0], 0, 0, -1, -1);//设置窗体的规格
				HOperatorSet.DispObj(MainForm.hImage[1], hWindows[0]);//显示图片

				HOperatorSet.SetPart(hWindowFive[0], 0, 0, -1, -1);//设置窗体的规格
				HOperatorSet.DispObj(MainForm.hImage[2], hWindowFive[0]);//显示图片
			}
		}

		private void btn_Close_System_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btn_Detection_Click(object sender, EventArgs e)
		{
			string TestDate = text_TestDate.Text;
			string SheetNum = text_SheetNum.Text;
			string SheetNums = text_SheetNums.Text;

			string ImagePath = Parameters.commministion.ImageSavePath + TestDate + "\\" + SheetNums + "\\" + SheetNum + "\\" + "CAM-0" + "\\" + "IN-" + "\\" + SheetNum + ".jpeg";
			string ImagePathTwo = Parameters.commministion.ImageSavePath + TestDate + "\\" + SheetNums + "\\" + SheetNum + "\\" + "CAM-1" + "\\" + "IN-" + "\\" + SheetNum + ".jpeg";
			string ImagePathThree = Parameters.commministion.ImageSavePath + TestDate + "\\" + SheetNums + "\\" + SheetNum + "\\" + "CAM-2" + "\\" + "IN-" + "\\" + SheetNum + ".jpeg";

			FileInfo fImagePath = new FileInfo(ImagePath);
			FileInfo fImagePathTwo = new FileInfo(ImagePathTwo);
			FileInfo fImagePathThree = new FileInfo(ImagePathThree);

			if (fImagePath.Exists)
			{
				Halcon.ImgDisplayAlone(0, ImagePath, hWindow[0]);
			}
			else
			{
				MessageBox.Show("路径不存在检测结果1", "错误提示");
			}

			if (fImagePathTwo.Exists)
			{
				Halcon.ImgDisplayAlone(1, ImagePathTwo, hWindows[0]);
			}
			else
			{
				MessageBox.Show("路径不存在检测结果2", "错误提示");
			}

			if (fImagePathThree.Exists)
			{
				Halcon.ImgDisplayAlone(2, ImagePathThree, hWindowFive[0]);
			}
			else
			{
				MessageBox.Show("路径不存在检测结果3", "错误提示");
			}
		}

		private void com_SheetNum_SelectedIndexChanged(object sender, EventArgs e)
		{
			text_SheetNum.Text = com_SheetNum.Text;
		}

		private void btn_PathConfirm_Click(object sender, EventArgs e)
		{
			com_SheetNum.Items.Clear();
			string targetPath = Parameters.commministion.ImageSavePath + text_TestDate.Text + "\\" + text_SheetNums.Text; // 替换为你的目标路径

			if (Directory.Exists(targetPath))
			{
				string[] folderNames = Directory.GetDirectories(targetPath);

				foreach (string folderPath in folderNames)
				{
					// 获取文件夹名称，并将其添加到下拉框
					string folderName = Path.GetFileName(folderPath);
					com_SheetNum.Items.Add(folderName);
				}
				com_SheetNum.SelectedIndex = 0;
			}
			else
			{
				MessageBox.Show("目标路径不存在！");
			}
		}

		private void hWindowControl4_HMouseUp(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int row1, col1, row2, col2;
				if (mouse_X0 == 0 || mouse_Y0 == 0)
					return;

				int tempNum = 0;
				hWindow[0].GetMposition(out mouse_X1, out mouse_Y1, out tempNum);

				double dbRowMove, dbColMove;
				dbRowMove = mouse_X0 - mouse_X1;//计算光标在X轴拖动的距离
				dbColMove = mouse_Y0 - mouse_Y1;//计算光标在Y轴拖动的距离

				hWindow[0].GetPart(out row1, out col1, out row2, out col2);//计算HWindow控件在当前状态下显示图像的位置
				hWindow[0].SetPart((int)(row1 + dbRowMove), (int)(col1 + dbColMove), (int)(row2 + dbRowMove), (int)(col2 + dbColMove));//根据拖动距离调整HWindows控件显示图像的位置
				RefreshImage(hWindow[0], MainForm.hImage[0]);//刷新图像
			}
		}

		private void hWindowControl1_HMouseUp(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int row1, col1, row2, col2;
				if (mouse_X0 == 0 || mouse_Y0 == 0)
					return;

				int tempNum = 0;
				hWindows[0].GetMposition(out mouse_X1, out mouse_Y1, out tempNum);

				double dbRowMove, dbColMove;
				dbRowMove = mouse_X0 - mouse_X1;//计算光标在X轴拖动的距离
				dbColMove = mouse_Y0 - mouse_Y1;//计算光标在Y轴拖动的距离

				hWindows[0].GetPart(out row1, out col1, out row2, out col2);//计算HWindow控件在当前状态下显示图像的位置
				hWindows[0].SetPart((int)(row1 + dbRowMove), (int)(col1 + dbColMove), (int)(row2 + dbRowMove), (int)(col2 + dbColMove));//根据拖动距离调整HWindows控件显示图像的位置
				RefreshImage(hWindows[0], MainForm.hImage[1]);//刷新图像
			}
		}

		private void hWindowControl3_HMouseDown(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Clicks == 2)
				{
					try
					{
						相机检测设置.DetectionBase(1, hWindows, MainForm.hImage[1]);
						相机检测设置.DetectionBase(0, hWindow, MainForm.hImage[0]);
						相机检测设置.DetectionBase(2, hWindowFive, MainForm.hImage[2]);
					}
					catch
					{
						MessageBox.Show("基准线查找异常", "严重错误提示");
					}
				}
			}
				
		}

		private void text_SheetNum_TextChanged(object sender, EventArgs e)
		{

		}

		private void hWindowControl5_HMouseMove(object sender, HMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int row1, col1, row2, col2;
				if (mouse_X0 == 0 || mouse_Y0 == 0)
					return;

				int tempNum = 0;
				hWindowFive[0].GetMposition(out mouse_X1, out mouse_Y1, out tempNum);

				double dbRowMove, dbColMove;
				dbRowMove = mouse_X0 - mouse_X1;//计算光标在X轴拖动的距离
				dbColMove = mouse_Y0 - mouse_Y1;//计算光标在Y轴拖动的距离

				hWindowFive[0].GetPart(out row1, out col1, out row2, out col2);//计算HWindow控件在当前状态下显示图像的位置
				hWindowFive[0].SetPart((int)(row1 + dbRowMove), (int)(col1 + dbColMove), (int)(row2 + dbRowMove), (int)(col2 + dbColMove));//根据拖动距离调整HWindows控件显示图像的位置
				RefreshImage(hWindowFive[0], MainForm.hImage[2]);//刷新图像
			}
		}

		private void hWindowControl5_HMouseUp(object sender, HMouseEventArgs e)
		{

		}




		private void RefreshImage(HWindow hWindows, HObject hImage)
		{
			hWindows.ClearWindow();//清空HWindow控件
			hWindows.DispObj(hImage);//重新显示图像
		}
	}
}
