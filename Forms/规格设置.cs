using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using WY_App.Utility;
using static WY_App.Utility.Parameters;
using Parameters = WY_App.Utility.Parameters;

namespace WY_App
{
    public partial class 规格设置 : Form
    {
        public 规格设置()
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

        private void btn_Change_Click(object sender, EventArgs e)
        {
            num_圆直径value.Enabled = true;
            num_圆直径min.Enabled = true;
            num_圆直径max.Enabled = true;
            num_圆直径adjust.Enabled = true;

            num_矩形宽value.Enabled = true;
            num_矩形宽min.Enabled = true;
            num_矩形宽max.Enabled = true;
            num_矩形宽adjust.Enabled = true;

            num_矩形高value.Enabled = true;
            num_矩形高min.Enabled = true;
            num_矩形高max.Enabled = true;
            num_矩形高adjust.Enabled = true;

            num_边1value.Enabled = true;
            num_边1min.Enabled = true;
            num_边1max.Enabled = true;
            num_边1adjust.Enabled = true;

            num_边2value.Enabled = true;
            num_边2min.Enabled = true;
            num_边2max.Enabled = true;
            num_边2adjust.Enabled = true;

            num_边3value.Enabled = true;
            num_边3min.Enabled = true;
            num_边3max.Enabled = true;
            num_边3adjust.Enabled = true;
            btn_Save.Enabled = true;

        }

        private void ParamSettings_Load(object sender, EventArgs e)
        {
            num_圆直径value.Value = Parameters.specifications.检测规格[0].value;
            num_圆直径min.Value = Parameters.specifications.检测规格[0].min;
            num_圆直径max.Value = Parameters.specifications.检测规格[0].max;
            num_圆直径adjust.Value = Parameters.specifications.检测规格[0].adjust;

            num_矩形宽value.Value = Parameters.specifications.检测规格[1].value;
            num_矩形宽min.Value = Parameters.specifications.检测规格[1].min;
            num_矩形宽max.Value = Parameters.specifications.检测规格[1].max;
            num_矩形宽adjust.Value = Parameters.specifications.检测规格[1].adjust;

            num_矩形高value.Value = Parameters.specifications.检测规格[2].value;
            num_矩形高min.Value = Parameters.specifications.检测规格[2].min;
            num_矩形高max.Value = Parameters.specifications.检测规格[2].max;
            num_矩形高adjust.Value = Parameters.specifications.检测规格[2].adjust;

            num_边1value.Value = Parameters.specifications.检测规格[3].value;
            num_边1min.Value = Parameters.specifications.检测规格[3].min;
            num_边1max.Value = Parameters.specifications.检测规格[3].max;
            num_边1adjust.Value = Parameters.specifications.检测规格[3].adjust;

            num_边2value.Value = Parameters.specifications.检测规格[4].value;
            num_边2min.Value = Parameters.specifications.检测规格[4].min;
            num_边2max.Value = Parameters.specifications.检测规格[4].max;
            num_边2adjust.Value = Parameters.specifications.检测规格[4].adjust;

            num_边3value.Value = Parameters.specifications.检测规格[5].value;
            num_边3min.Value = Parameters.specifications.检测规格[5].min;
            num_边3max.Value = Parameters.specifications.检测规格[5].max;
            num_边3adjust.Value = Parameters.specifications.检测规格[5].adjust;

        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            Parameters.specifications.检测规格[0].value = num_圆直径value.Value;
            Parameters.specifications.检测规格[0].min = num_圆直径min.Value;
            Parameters.specifications.检测规格[0].max = num_圆直径max.Value;
            Parameters.specifications.检测规格[0].adjust = num_圆直径adjust.Value;

            Parameters.specifications.检测规格[1].value = num_矩形宽value.Value;
            Parameters.specifications.检测规格[1].min = num_矩形宽min.Value;
            Parameters.specifications.检测规格[1].max = num_矩形宽max.Value;
            Parameters.specifications.检测规格[1].adjust = num_矩形宽adjust.Value;

            Parameters.specifications.检测规格[2].value = num_矩形高value.Value;
            Parameters.specifications.检测规格[2].min = num_矩形高min.Value;
            Parameters.specifications.检测规格[2].max = num_矩形高max.Value;
            Parameters.specifications.检测规格[2].adjust = num_矩形高adjust.Value;

            Parameters.specifications.检测规格[3].value = num_边1value.Value;
            Parameters.specifications.检测规格[3].min = num_边1min.Value;
            Parameters.specifications.检测规格[3].max = num_边1max.Value;
            Parameters.specifications.检测规格[3].adjust = num_边1adjust.Value;

            Parameters.specifications.检测规格[4].value = num_边2value.Value;
            Parameters.specifications.检测规格[4].min = num_边2min.Value;
            Parameters.specifications.检测规格[4].max = num_边2max.Value;
            Parameters.specifications.检测规格[4].adjust = num_边2adjust.Value;

            Parameters.specifications.检测规格[5].value = num_边3value.Value;
            Parameters.specifications.检测规格[5].min = num_边3min.Value;
            Parameters.specifications.检测规格[5].max = num_边3max.Value;
            Parameters.specifications.检测规格[5].adjust = num_边3adjust.Value;

            XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, "Parameter/Specifications.xml");
            MessageBox.Show("系统参数修改，请重启软件");
            this.Close();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
