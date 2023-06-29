using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WY_App.Utility;

namespace WY_App
{
    public partial class 登陆界面 : Form
    {
        public delegate void TransfDelegate(String value);
        public event TransfDelegate TransfEvent;
        List<Users> userList = new List<Users>();
        class Users
        {
            private string name;
            /// <summary>
            /// 用户名
            /// </summary>
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            private string password;
            /// <summary>
            /// 密码
            /// </summary>
            public string Password
            {
                get { return password; }
                set { password = value; }
            }
            private string permission;
            /// <summary>
            /// 权限等级
            /// </summary>
            public string Permission
            {
                get { return permission; }
                set { permission = value; }
            }
        }


        public 登陆界面()
        {
            InitializeComponent();
            //加载指定路径的xml文件
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true; //忽略文档里面的注释
            XmlReader reader = XmlReader.Create("Parameter/User.xml");
            xmlDoc.Load(reader);
            //得到根节点
            XmlNode xn = xmlDoc.SelectSingleNode("Users");
            //得到根节点的所有子节点
            XmlNodeList xnl = xn.ChildNodes;
            
            foreach (XmlNode item in xnl)
            {
                Users user = new Users();
                //将节点转换为元素，便于得到节点的属性值
                XmlElement xe = (XmlElement)item;
                //得到Name和Password两个属性的属性值
                XmlNodeList xmlnl = xe.ChildNodes;
                user.Name = xmlnl.Item(0).InnerText;
                cmb_UserName.Items.Add(user.Name);
                user.Password = xmlnl.Item(1).InnerText;
                user.Permission = xmlnl.Item(2).InnerText;
                userList.Add(user);
            }
            cmb_UserName.SelectedIndex = 0;
            cmb_Permission.Text = userList[cmb_UserName.SelectedIndex].Permission;
            reader.Close(); //读取完数据后需关闭
        }

      
        private bool isNumOrAlp(string str)

        {
            string pattern = @"^[A-Za-z0-9]+$";  //@意思忽略转义，+匹配前面一次或多次，$匹配结尾

            Match match = Regex.Match(str, pattern);

            return match.Success;

        }

        private void btn_Close_System_Click(object sender, EventArgs e)
        {
			timer1.Enabled = false;
			this.Close();
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
           if( txt_Password.Text == userList[cmb_UserName.SelectedIndex].Password )
            {
                TransfEvent(cmb_Permission.Text);
				timer1.Enabled = false;
				this.Close();
            }
            else
            {
                MessageBox.Show("密码错误,请重新输入!");
            }
        }

        private void txt_Password_TextChanged(object sender, EventArgs e)
        {
            if(!isNumOrAlp(txt_Password.Text)&& txt_Password.Text!= null)
            {
                MessageBox.Show("只允许输入数字或字母且不能为空，请重新输入!");
            }
        }

        private void cmb_UserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = this.cmb_UserName.FindString(cmb_UserName.Text);
            cmb_Permission.Text = userList[index].Permission;
        }

        private void txt_PasswordAck_TextChanged(object sender, EventArgs e)
        {
            if (!isNumOrAlp(txt_PasswordAck.Text) && txt_PasswordAck.Text != null && txt_PasswordAck.Text.Length == txt_Password.Text.Length)
            {
                MessageBox.Show("只允许输入数字或字母且不能为空，请重新输入!", "温馨提示");
            }
        }

        private void btn_ChangePassword_Click(object sender, EventArgs e)
        {
            UpdateXml();           
        }

        private void btn_SignUp_Click(object sender, EventArgs e)
        {
            if(cmb_UserName.Items.Contains(cmb_UserName.Text))
            {
                MessageBox.Show("用户名已存在，请勿重复创建！", "温馨提示");
                return;
            }
            if (  txt_PasswordAck.Text == txt_Password.Text)
            {
                string userName = cmb_UserName.Text.Trim();
                string password = txt_Password.Text.Trim();
                string permission = cmb_Permission.Text.Trim();

                //加载文件并选出根节点
                XmlDocument doc = new XmlDocument();
                doc.Load("Parameter/User.xml");
                XmlNode root = doc.SelectSingleNode("Users");

                //创建一个结点，并设置结点的名称
                XmlElement xelKey = doc.CreateElement("User");

                //创建子结点
                XmlElement xelUser = doc.CreateElement("Name");
                xelUser.InnerText = userName;

                XmlElement xelPassword = doc.CreateElement("Password");
                xelPassword.InnerText = password;

                XmlElement xelPermission = doc.CreateElement("Permission");
                xelPermission.InnerText = permission;

                //将子结点挂靠在相应的父节点
                xelKey.AppendChild(xelUser);
                xelKey.AppendChild(xelPassword);
                xelKey.AppendChild(xelPermission);
                //最后把book结点挂接在跟结点上，并保存整个文件
                root.AppendChild(xelKey);
                doc.Save("Parameter/User.xml");
                MessageBox.Show("保存成功！", "温馨提示");
                this.Close();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void 登陆界面_Load(object sender, EventArgs e)
        {
            if (MainForm.Permission == "管理员")
            {
                btn_SignUp.Visible = true;
                btn_ChangePassword.Visible = true;
                label4.Visible = true;
                txt_PasswordAck.Visible = true;
            }
			timer1.Enabled = true;
		}



        /// <summary>
        /// 创建Xml文件
        /// </summary>
        public void CreateXmlFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            //创建Xml根节点  
            XmlNode root = xmlDoc.CreateElement("Users");
            xmlDoc.AppendChild(root);

            XmlNode root1 = xmlDoc.CreateElement("User");
            root.AppendChild(root1);

            //创建子节点
            CreateNode(xmlDoc, root1, "Name", "开发员1");
            CreateNode(xmlDoc, root1, "Password", "1234");
            CreateNode(xmlDoc, root1, "Permission", "开发员");

            XmlNode root2 = xmlDoc.CreateElement("User");
            root.AppendChild(root2);

            //创建子节点
            CreateNode(xmlDoc, root2, "Name", "管理员1");
            CreateNode(xmlDoc, root2, "Password", "1234");
            CreateNode(xmlDoc, root2, "Permission", "管理员");

            XmlNode root3 = xmlDoc.CreateElement("User");
            root.AppendChild(root3);

            //创建子节点
            CreateNode(xmlDoc, root3, "Name", "操作员1");
            CreateNode(xmlDoc, root3, "Password", "1234");
            CreateNode(xmlDoc, root3, "Permission", "操作员");

            XmlNode root4 = xmlDoc.CreateElement("User");
            root.AppendChild(root4);

            //创建子节点
            CreateNode(xmlDoc, root4, "Name", "访客");
            CreateNode(xmlDoc, root4, "Password", "");
            CreateNode(xmlDoc, root4, "Permission", "访客");
            //将文件保存到指定位置
            xmlDoc.Save("Parameter/User.xml");
        }

        /// <summary>    
        /// 创建节点    
        /// </summary>    
        /// <param name="xmlDoc">xml文档</param>    
        /// <param name="parentNode">Xml父节点</param>    
        /// <param name="name">节点名</param>    
        /// <param name="value">节点值</param>    
        ///   
        public void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            //创建对应Xml节点元素
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        private void UpdateXml()
        {
            if (cmb_UserName.Text.Contains("开发员"))
            {
                MessageBox.Show("您无权修改开发员密码,请联系软件开发人员！", "温馨提示");
                this.Close();
                return;
            }
            int index = this.cmb_UserName.FindString(cmb_UserName.Text);
            if (txt_Password.Text== userList[index].Password)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Parameter/User.xml");//加载Xml文件
                XmlNode xns = xmlDoc.SelectSingleNode("Users/User");//查找要修改的节点
                XmlNodeList xmlNodeList = xns.ChildNodes;//取出book节点下所有的子节点

                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    XmlElement xmlElement = (XmlElement)xmlNode;//将节点转换一下类型
                    if (xmlElement.Name == "Password")//判断该子节点是否是要查找的节点
                    {
                        xmlElement.InnerText = txt_PasswordAck.Text;//设置新值
                        break;
                    }
                }
                xmlDoc.Save("Parameter/User.xml");//保存修改的Xml文件内容
                MessageBox.Show("密码修改成功！", "温馨提示");
            }        
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			Jurisdiction();
		}

		private void Jurisdiction()
		{
			Int16 PlcSignal = HslCommunication._NetworkTcpDevice.ReadInt16(Parameters.plcParams.预留地址[3]).Content;
			String sPlcSignal = HslCommunication._NetworkTcpDevice.ReadString(Parameters.plcParams.预留地址[4], 20).Content;

			MainForm.UserName = sPlcSignal;
			LogHelper.WriteWarn(" " + MainForm.UserName + "登录");
			if (PlcSignal == 1)
			{

				cmb_UserName.Text = userList[1].Name;
				cmb_Permission.Text = userList[1].Permission;
				txt_Password.Text = userList[1].Password;
				TransfEvent(cmb_Permission.Text);
				this.Close();

			}
			else if (PlcSignal == 2)
			{
				cmb_UserName.Text = userList[0].Name;
				cmb_Permission.Text = userList[0].Permission;
				txt_Password.Text = userList[0].Password;
				TransfEvent(cmb_Permission.Text);
				this.Close();
			}
			else if (PlcSignal == 3)
			{
				cmb_UserName.Text = userList[2].Name;
				cmb_Permission.Text = userList[2].Permission;
				txt_Password.Text = userList[2].Password;
				TransfEvent(cmb_Permission.Text);
				this.Close();
			}
			else
			{
				cmb_UserName.Text = userList[3].Name;
				cmb_Permission.Text = userList[3].Permission;
				txt_Password.Text = userList[3].Password;
				TransfEvent(cmb_Permission.Text);
				this.Close();
			}
		}
	}
}
