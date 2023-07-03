using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WY_App.Utility;

namespace WY_App
{
    public partial class 切换产品 : Form
    {
        public delegate void TransfDelegate(String value);
        public event TransfDelegate TransfEvent;
        List<ProductKind> userList = new List<ProductKind>();
        class ProductKind
        {
            private string name;
            private string height;
            private string width;
            /// <summary>
            /// 用户名
            /// </summary>
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

        }
        public 切换产品()
        {
            InitializeComponent();
        }

        private void 切换产品_Load(object sender, EventArgs e)
        {
            //加载指定路径的xml文件
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true; //忽略文档里面的注释
            XmlReader reader = XmlReader.Create("Parameter/ProductList.xml");
            xmlDoc.Load(reader);
            //得到根节点
            XmlNode xn = xmlDoc.SelectSingleNode("Products");
            //得到根节点的所有子节点
            XmlNodeList xnl = xn.ChildNodes;

            foreach (XmlNode item in xnl)
            {
                ProductKind productKind = new ProductKind();
                //将节点转换为元素，便于得到节点的属性值
                XmlElement xe = (XmlElement)item;
                //得到Name和Password两个属性的属性值
                XmlNodeList xmlnl = xe.ChildNodes;
                productKind.Name = xmlnl.Item(0).InnerText;
                //productKind.Height = xmlnl.Item(1).InnerText;
                //productKind.Width = xmlnl.Item(2).InnerText;

                cmb_ProductList.Items.Add(productKind.Name);                
                userList.Add(productKind);
            }
            cmb_ProductList.SelectedItem=Parameters.commministion.productName;
            reader.Close(); //读取完数据后需关闭

            uiDoubleUp_Width.Value = Parameters.specifications.ImageWidth;
            uiDouble_Height.Value = Parameters.specifications.ImageHeigth;
        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void 保存_Click(object sender, EventArgs e)
        {
            if (HslCommunication.plc_connect_result)
            {
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[0], Parameters.specifications.ImageWidth);
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[1], Parameters.specifications.ImageHeigth);
				HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[2], Parameters.commministion.productName);
			}
            else
            {
                MessageBox.Show("PLC未连接，数据写入失败");
                //return;
            }
            Parameters.commministion.productName = cmb_ProductList.Text;
            TransfEvent(cmb_ProductList.Text);
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
                //HOperatorSet.ReadRegion(out MainForm.hoRegions[i], Parameters.commministion.productName + "/halcon/hoRegion" + i + ".tiff");
            }
            

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cmb_ProductList.Items.Contains(txt_NewProductName.Text))
            {
                MessageBox.Show("产品型号已存在，请勿重复创建！", "温馨提示");
                return;
            }
            string productName = txt_NewProductName.Text.Trim();
            //string productHeight = textBox1.Text.Trim();
            //string productWidth = textBox2.Text.Trim();

            //加载文件并选出根节点
            XmlDocument doc = new XmlDocument();
            doc.Load("Parameter/ProductList.xml");
            XmlNode root = doc.SelectSingleNode("Products");
            //创建一个结点，并设置结点的名称
            XmlElement xelKey = doc.CreateElement("Product");
            //创建子结点
            XmlElement xelUser = doc.CreateElement("Name");
            xelUser.InnerText = productName;

            //XmlElement xelUser1 = doc.CreateElement("Height");
            //xelUser1.InnerText = productHeight;

            //XmlElement xelUser2 = doc.CreateElement("Width");
            //xelUser2.InnerText = productWidth;

            //将子结点挂靠在相应的父节点
            xelKey.AppendChild(xelUser);
            //xelKey.AppendChild(xelUser1);
            //xelKey.AppendChild(xelUser2);

            //最后把book结点挂接在跟结点上，并保存整个文件
            root.AppendChild(xelKey);
            doc.Save("Parameter/ProductList.xml");
            GetFilesAndDirs("初始化", productName);
            MessageBox.Show("保存成功！", "温馨提示");
            this.Close();
        }
        private void GetFilesAndDirs(string srcDir, string destDir)
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
                    newPath = destDir +"/"+ fileInfo.Name;
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (HslCommunication.plc_connect_result)
            {
                Parameters.specifications.ImageWidth = (int)uiDoubleUp_Width.Value;
                Parameters.specifications.ImageHeigth = (int)uiDouble_Height.Value;
                Parameters.commministion.productName =(string) cmb_ProductList.SelectedItem;

				HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[0], Parameters.specifications.ImageWidth);
                HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[1], Parameters.specifications.ImageHeigth);
				HslCommunication._NetworkTcpDevice.Write(Parameters.plcParams.预留地址[2], Parameters.commministion.productName);

				XMLHelper.serialize<Parameters.Specifications>(Parameters.specifications, Parameters.commministion.productName + "/Specifications.xml");
				XMLHelper.serialize<Parameters.Commministion>(Parameters.commministion, "Parameter/Commministion.xml");
			}
            else
            {
                MessageBox.Show("PLC未连接，数据写入失败");
            }
        }

		private void cmb_ProductList_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}
