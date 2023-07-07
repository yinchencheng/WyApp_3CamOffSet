using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WY_App.Utility;

namespace WY_App
{
    public partial class 卡尺工具设置 : Form
    {
        public 卡尺工具设置()
        {
            InitializeComponent();
        }

        private void 卡尺工具设置_Load(object sender, EventArgs e)
        {
            num_MeasureLength1.Value = Parameters.detectionSpec[MainForm.CamNum].MeasureLength1[MainForm.baseNum];
            num_MeasureLength2.Value = Parameters.detectionSpec[MainForm.CamNum].MeasureLength2[MainForm.baseNum];
            num_MeasureSigma.Value = (decimal)Parameters.detectionSpec[MainForm.CamNum].MeasureSigma[MainForm.baseNum];
            num_MeasureThreshold.Value = Parameters.detectionSpec[MainForm.CamNum].MeasureThreshold[MainForm.baseNum];
            num_MeasureTransition.Text = Parameters.detectionSpec[MainForm.CamNum].MeasureTransition[MainForm.baseNum];
            num_OffSet.Value= Parameters.detectionSpec[MainForm.CamNum].OffSet[MainForm.baseNum];
        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void 保存_Click(object sender, EventArgs e)
        {
			if (Parameters.detectionSpec[MainForm.CamNum].MeasureLength1[MainForm.baseNum] != (uint)num_MeasureLength1.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeasureLength1:" + Parameters.detectionSpec[MainForm.CamNum].MeasureLength1[MainForm.baseNum] + "=>" + (uint)num_MeasureLength1.Value);
				Parameters.detectionSpec[MainForm.CamNum].MeasureLength1[MainForm.baseNum] = (uint)num_MeasureLength1.Value;
			}

			if (Parameters.detectionSpec[MainForm.CamNum].MeasureLength2[MainForm.baseNum] != (uint)num_MeasureLength2.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeasureLength2:" + Parameters.detectionSpec[MainForm.CamNum].MeasureLength2[MainForm.baseNum] + "=>" + (uint)num_MeasureLength2.Value);
				Parameters.detectionSpec[MainForm.CamNum].MeasureLength2[MainForm.baseNum] = (uint)num_MeasureLength2.Value;
			}

			if (Parameters.detectionSpec[MainForm.CamNum].MeasureSigma[MainForm.baseNum] != (double)num_MeasureSigma.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeasureSigma:" + Parameters.detectionSpec[MainForm.CamNum].MeasureSigma[MainForm.baseNum] + "=>" + (double)num_MeasureSigma.Value);
				Parameters.detectionSpec[MainForm.CamNum].MeasureSigma[MainForm.baseNum] = (double)num_MeasureSigma.Value;
			}

			if (Parameters.detectionSpec[MainForm.CamNum].MeasureThreshold[MainForm.baseNum] != (uint)num_MeasureThreshold.Value)
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeasureThreshold:" + Parameters.detectionSpec[MainForm.CamNum].MeasureThreshold[MainForm.baseNum] + "=>" + (uint)num_MeasureThreshold.Value);
				Parameters.detectionSpec[MainForm.CamNum].MeasureThreshold[MainForm.baseNum] = (uint)num_MeasureThreshold.Value;
			}

			if (Parameters.detectionSpec[MainForm.CamNum].MeasureTransition[MainForm.baseNum] != num_MeasureTransition.SelectedItem.ToString())
			{
				LogHelper.WriteWarn(" " + MainForm.UserName + "MeasureTransition:" + Parameters.detectionSpec[MainForm.CamNum].MeasureTransition[MainForm.baseNum] + "=>" + num_MeasureTransition.SelectedText);
				Parameters.detectionSpec[MainForm.CamNum].MeasureTransition[MainForm.baseNum] = num_MeasureTransition.SelectedItem.ToString();
			}
            
            XMLHelper.serialize<Parameters.DetectionSpec>(Parameters.detectionSpec[MainForm.CamNum], Parameters.commministion.productName + "/DetectionSpec" + MainForm.CamNum + ".xml");
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (MainForm.baseNum)
            {
                case 0:
                    {
                        相机检测设置.showXBase(sender, e);
                        break;
                    }
                case 1:
                    {
                        相机检测设置.showY1Base(sender, e);
                        break;
                    }
                case 2:
                    {
                        相机检测设置.showY2Base(sender, e);
                        break;
                    }
            }
            
        }

        private void num_MeasureLength1_ValueChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].MeasureLength1[MainForm.baseNum] = (uint)num_MeasureLength1.Value;
        }

        private void 修改_Click(object sender, EventArgs e)
        {
            num_MeasureLength1.Enabled = true;
            num_MeasureLength2.Enabled = true;
            num_MeasureSigma.Enabled = true;
            num_MeasureThreshold.Enabled = true;
            num_MeasureTransition.Enabled = true;
            num_OffSet.Enabled = true;
        }

        private void num_MeasureLength2_ValueChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].MeasureLength2[MainForm.baseNum] = (uint)num_MeasureLength2.Value;
        }

        private void num_MeasureSigma_ValueChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].MeasureSigma[MainForm.baseNum] = (double)num_MeasureSigma.Value;
        }

        private void num_MeasureThreshold_ValueChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].MeasureThreshold[MainForm.baseNum] = (uint)num_MeasureThreshold.Value;
        }

        private void num_MeasureTransition_SelectedIndexChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].MeasureTransition[MainForm.baseNum] = num_MeasureTransition.SelectedText;
        }

        private void num_OffSet_ValueChanged(object sender, EventArgs e)
        {
            Parameters.detectionSpec[MainForm.CamNum].OffSet[MainForm.baseNum] = (uint)num_OffSet.Value;
        }
    }
}
