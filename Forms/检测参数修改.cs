using Newtonsoft.Json.Linq;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WY_App.Utility;

namespace WY_App.Forms
{
	public partial class 检测参数修改 : Form
	{
		UIDoubleUpDown[] ThresholdLow           = new UIDoubleUpDown[28];
		UIDoubleUpDown[] ThresholdHigh          = new UIDoubleUpDown[28];
		UIDoubleUpDown[] AreaLow                = new UIDoubleUpDown[28];
		UIDoubleUpDown[] AreaHigh               = new UIDoubleUpDown[28];
		CheckBox[] DefectDetection              = new CheckBox[28];

		string[] strPara = new string[28];
		public 检测参数修改()
		{
			InitializeComponent();
			for (int i = 0; i < ThresholdLow.Length; i++)
			{
				// 根据名字查找已经创建的示例框控件
				ThresholdLow[i]    = (UIDoubleUpDown)Controls.Find("num_ThresholdLow" + (i + 1), true)[0];
				ThresholdHigh[i]   = (UIDoubleUpDown)Controls.Find("num_ThresholdHigh" + (i + 1), true)[0];
				AreaLow[i]         = (UIDoubleUpDown)Controls.Find("num_AreaLow" + (i + 1), true)[0];
				AreaHigh[i]        = (UIDoubleUpDown)Controls.Find("num_AreaHigh" + (i + 1), true)[0];
				DefectDetection[i] = (CheckBox)Controls.Find("check_DefectDetection" + (i + 1), true)[0];
			}
		}

		private void 检测参数修改_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < 28; i++)
			{
				ThresholdLow[i].Value      = Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[i];
				ThresholdHigh[i].Value     = Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[i];
				AreaLow[i].Value           = Parameters.detectionSpec[MainForm.CamNum].AreaLow[i];
				AreaHigh[i].Value          = Parameters.detectionSpec[MainForm.CamNum].AreaHigh[i];
				DefectDetection[i].Checked = Parameters.detectionSpec[MainForm.CamNum].DefectDetection[i];

				if (DefectDetection[i].Checked)
				{
					ThresholdLow[i].Enabled = true;
					ThresholdHigh[i].Enabled = true;
					AreaLow[i].Enabled = true;
					AreaHigh[i].Enabled = true;
				}
				else
				{
					ThresholdLow[i].Enabled = false;
					ThresholdHigh[i].Enabled = false;
					AreaLow[i].Enabled = false;
					AreaHigh[i].Enabled = false;
				}
			}

			
		}

		private void uiDoubleUpDown1_ValueChanged(object sender, double value)
		{

		}

		private void btn_Save_Click(object sender, EventArgs e)
		{
			ChangeParaLog();
			for (int i = 0; i < 28; i++)
			{
				Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[i] = ThresholdLow[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[i] = ThresholdHigh[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].AreaLow[i] = AreaLow[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].AreaHigh[i] = AreaHigh[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].DefectDetection[i] = DefectDetection[i].Checked;
			}
			try
			{
				XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[MainForm.CamNum], Parameters.commministion.productName + "/DetectionSpec" + MainForm.CamNum + ".xml");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

		}

		private void uiButton1_Click(object sender, EventArgs e)
		{
			ChangeParaLog();
			for (int i = 0; i < 28; i++)
			{
				Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[i]    = ThresholdLow[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[i]   = ThresholdHigh[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].AreaLow[i]         = AreaLow[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].AreaHigh[i]        = AreaHigh[i].Value;
				Parameters.detectionSpec[MainForm.CamNum].DefectDetection[i] = DefectDetection[i].Checked;
			}
			try
			{
				XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[MainForm.CamNum], Parameters.commministion.productName + "/DetectionSpec" + MainForm.CamNum + ".xml");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			MainForm.RefreshPara();
			this.Close();
		}

		private void ChangeParaLog()
		{
			for (int i = 0; i < 28; i++)
			{
				if (Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[i] != ThresholdLow[i].Value)
				{
					LogHelper.WriteWarn(" " + MainForm.UserName + "ThresholdLow:" + Parameters.detectionSpec[MainForm.CamNum].ThresholdLow[i] + "  第" + i.ToString() + "号缺陷" + "=>" + ThresholdLow[i].Value);	
				}
				if (Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[i] != ThresholdHigh[i].Value)
				{
					LogHelper.WriteWarn(" " + MainForm.UserName + "ThresholdHigh:" + Parameters.detectionSpec[MainForm.CamNum].ThresholdHigh[i] + "  第" + i.ToString() + "号缺陷" + "=>" + ThresholdHigh[i].Value);
				}
				if (Parameters.detectionSpec[MainForm.CamNum].AreaLow[i] != AreaLow[i].Value)
				{
					LogHelper.WriteWarn(" " + MainForm.UserName + "AreaLow:" + Parameters.detectionSpec[MainForm.CamNum].AreaLow[i] + "  第" + i.ToString() + "号缺陷" + "=>" + AreaLow[i].Value);
				}
				if (Parameters.detectionSpec[MainForm.CamNum].AreaHigh[i] != AreaHigh[i].Value)
				{
					LogHelper.WriteWarn(" " + MainForm.UserName + "AreaHigh:" + Parameters.detectionSpec[MainForm.CamNum].AreaHigh[i] + "  第" + i.ToString() + "号缺陷" + "=>" + AreaHigh[i].Value);
				}
				if (Parameters.detectionSpec[MainForm.CamNum].DefectDetection[i] != DefectDetection[i].Checked)
				{
					LogHelper.WriteWarn(" " + MainForm.UserName + "DefectDetection:" + Parameters.detectionSpec[MainForm.CamNum].DefectDetection[i] + "  第" + i.ToString() + "号缺陷" + "=>" + DefectDetection[i].Checked);
				}

			}
		}
		private void RefreshButton(int i)
		{
			if (DefectDetection[i].Checked)
			{
				ThresholdLow[i].Enabled = true;
				ThresholdHigh[i].Enabled = true;
				AreaLow[i].Enabled = true;
				AreaHigh[i].Enabled = true;
			}
			else
			{
				ThresholdLow[i].Enabled = false;
				ThresholdHigh[i].Enabled = false;
				AreaLow[i].Enabled = false;
				AreaHigh[i].Enabled = false;
			}
		}
		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel19_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel23_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel33_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow28_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection28_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(27);
		}

		private void uiLabel109_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh28_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel110_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow28_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel111_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh28_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel112_Click(object sender, EventArgs e)
		{

		}

		private void label4_Click(object sender, EventArgs e)
		{

		}

		private void panel24_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow27_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection27_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(26);
		}

		private void uiLabel73_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh27_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel74_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow27_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel75_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh27_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel76_Click(object sender, EventArgs e)
		{

		}

		private void panel25_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow26_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection26_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(25);
		}

		private void uiLabel77_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh26_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel78_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow26_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel79_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh26_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel80_Click(object sender, EventArgs e)
		{

		}

		private void panel26_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow25_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection25_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(24);
		}

		private void uiLabel81_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh25_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel82_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow25_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel83_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh25_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel84_Click(object sender, EventArgs e)
		{

		}

		private void panel27_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow24_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection24_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(23);
		}

		private void uiLabel85_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh24_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel86_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow24_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel87_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh24_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel88_Click(object sender, EventArgs e)
		{

		}

		private void panel28_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow23_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection23_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(22);
		}

		private void uiLabel89_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh23_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel90_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow23_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel91_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh23_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel92_Click(object sender, EventArgs e)
		{

		}

		private void panel29_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow22_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection22_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(21);
		}

		private void uiLabel93_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh22_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel94_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow22_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel95_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh22_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel96_Click(object sender, EventArgs e)
		{

		}

		private void panel4_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel32_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow21_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection21_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(20);
		}

		private void uiLabel105_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh21_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel106_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow21_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel107_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh21_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel108_Click(object sender, EventArgs e)
		{

		}

		private void label3_Click(object sender, EventArgs e)
		{

		}

		private void panel17_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow20_ValueChanged(object sender, double value)
		{
			
		}

		private void check_DefectDetection20_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(19);
		}

		private void uiLabel49_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh20_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel50_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow20_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel51_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh20_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel52_Click(object sender, EventArgs e)
		{

		}

		private void panel18_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow19_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection19_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(18);
		}

		private void uiLabel53_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh19_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel54_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow19_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel55_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh19_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel56_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdLow18_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection18_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(17);
		}

		private void uiLabel57_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh18_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel58_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow18_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel59_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh18_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel60_Click(object sender, EventArgs e)
		{

		}

		private void panel20_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow17_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection17_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(16);
		}

		private void uiLabel61_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh17_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel62_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow17_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel63_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh17_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel64_Click(object sender, EventArgs e)
		{

		}

		private void panel21_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow16_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection16_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(15);
		}

		private void uiLabel65_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh16_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel66_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow16_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel67_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh16_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel68_Click(object sender, EventArgs e)
		{

		}

		private void panel22_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow15_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection15_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(14);
		}

		private void uiLabel69_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh15_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel70_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow15_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel71_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh15_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel72_Click(object sender, EventArgs e)
		{

		}

		private void panel3_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel31_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow14_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection14_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(13);
		}

		private void uiLabel101_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh14_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel102_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow14_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel103_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh14_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel104_Click(object sender, EventArgs e)
		{

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}

		private void panel11_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow13_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection13_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(12);
		}

		private void uiLabel25_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh13_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel26_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow13_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel27_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh13_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel28_Click(object sender, EventArgs e)
		{

		}

		private void panel12_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow12_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection12_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(11);
		}

		private void uiLabel29_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh12_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel30_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow12_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel31_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh12_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel32_Click(object sender, EventArgs e)
		{

		}

		private void panel13_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow11_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection11_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(10);
		}

		private void uiLabel33_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh11_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel34_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow11_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel35_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh11_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel36_Click(object sender, EventArgs e)
		{

		}

		private void panel14_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow10_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection10_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(9);
		}

		private void uiLabel37_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh10_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel38_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow10_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel39_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh10_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel40_Click(object sender, EventArgs e)
		{

		}

		private void panel15_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow9_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection9_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(8);
		}

		private void uiLabel41_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh9_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel42_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow9_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel43_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh9_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel44_Click(object sender, EventArgs e)
		{

		}

		private void panel16_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow8_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection8_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(7);
		}

		private void uiLabel45_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh8_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel46_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow8_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel47_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh8_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel48_Click(object sender, EventArgs e)
		{

		}

		private void panel2_Paint(object sender, PaintEventArgs e)
		{

		}

		private void panel30_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow7_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection7_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(6);
		}

		private void uiLabel97_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh7_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel98_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow7_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel99_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh7_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel100_Click(object sender, EventArgs e)
		{

		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void panel10_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow6_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection6_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(5);
		}

		private void uiLabel21_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh6_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel22_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow6_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel23_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh6_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel24_Click(object sender, EventArgs e)
		{

		}

		private void panel9_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow5_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection5_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(4);
		}

		private void uiLabel17_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh5_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel18_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow5_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel19_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh5_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel20_Click(object sender, EventArgs e)
		{

		}

		private void panel8_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow4_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection4_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(3);
		}

		private void uiLabel13_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh4_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel14_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow4_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel15_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh4_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel16_Click(object sender, EventArgs e)
		{

		}

		private void panel7_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow3_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection3_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(2);
		}

		private void uiLabel7_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh3_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel8_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow3_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel11_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh3_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel12_Click(object sender, EventArgs e)
		{

		}

		private void panel6_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow2_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection2_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(1);
		}

		private void uiLabel1_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh2_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel3_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow2_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel4_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh2_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel5_Click(object sender, EventArgs e)
		{

		}

		private void panel5_Paint(object sender, PaintEventArgs e)
		{

		}

		private void num_ThresholdLow1_ValueChanged(object sender, double value)
		{

		}

		private void check_DefectDetection1_CheckedChanged(object sender, EventArgs e)
		{
			RefreshButton(0);
		}

		private void uiLabel2_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaHigh1_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel6_Click(object sender, EventArgs e)
		{

		}

		private void num_AreaLow1_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel9_Click(object sender, EventArgs e)
		{

		}

		private void num_ThresholdHigh1_ValueChanged(object sender, double value)
		{

		}

		private void uiLabel10_Click(object sender, EventArgs e)
		{

		}
	}
}
