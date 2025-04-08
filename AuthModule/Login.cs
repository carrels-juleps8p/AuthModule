using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthModule
{
    /// <summary>
    /// 登录窗体类，处理用户登录逻辑和权限初始化
    /// </summary>
    public partial class Login : Form
    {
        // 定义用户密码映射表，存储不同用户类型的固定密码
        private readonly Dictionary<string, string> userPasswords = new Dictionary<string, string>()
        {
            { "操作员", "operator" },
            { "管理员", "admin" },
            { "超级管理员", "superadmin" }
        };

        /// <summary>
        /// 登录窗体构造函数
        /// </summary>
        public Login()
        {
            InitializeComponent();
            // 添加登录按钮的点击事件处理
            btnLogin.Click += BtnLogin_Click;
            // 添加密码框的按键事件处理
            txtPassword.KeyPress += TxtPassword_KeyPress;
        }

        /// <summary>
        /// 处理密码框的按键事件，实现回车键登录功能
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">按键事件参数</param>
        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 判断是否按下回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;  // 阻止回车键的默认处理（声音）
                btnLogin.PerformClick();  // 触发登录按钮的点击事件
            }
        }

        /// <summary>
        /// 处理登录按钮点击事件，验证用户身份并初始化权限
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string selectedUser = cboUserType.SelectedItem.ToString();
            string inputPassword = txtPassword.Text;

            // 验证用户密码
            if (userPasswords[selectedUser] == inputPassword)
            {
                MessageBox.Show("登录成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 根据用户类型决定后续操作
                if (selectedUser == "管理员" || selectedUser == "超级管理员")
                {
                    // 初始化用户会话并设置相应权限
                    var permissions = new HashSet<string>();
                    if (selectedUser == "管理员")
                    {
                        // 设置管理员权限
                        permissions.Add("基础功能.查看");
                        permissions.Add("基础功能.导出");
                        permissions.Add("基础功能.编辑");
                        permissions.Add("系统管理.用户管理");
                    }
                    else // 超级管理员
                    {
                        // 设置超级管理员权限
                        permissions.Add("基础功能.查看");
                        permissions.Add("基础功能.导出");
                        permissions.Add("基础功能.编辑");
                        permissions.Add("系统管理.用户管理");
                        permissions.Add("系统管理.权限管理");
                    }
                    UserSession.Instance.Initialize(selectedUser, permissions);

                    // 打开权限管理窗体
                    Accessmanager accessForm = new Accessmanager(selectedUser);
                    this.Hide();  // 隐藏登录窗体
                    accessForm.ShowDialog();  // 显示权限管理窗体
                    this.Close();  // 关闭登录窗体
                }
                else
                {
                    // 初始化操作员会话并设置基本权限
                    var permissions = new HashSet<string>
                    {
                        "基础功能.查看",
                        "基础功能.导出"
                    };
                    UserSession.Instance.Initialize(selectedUser, permissions);

                    // 操作员直接进入系统主界面
                    MessageBox.Show("欢迎操作员登录系统！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            else
            {
                // 密码错误处理
                MessageBox.Show("密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
