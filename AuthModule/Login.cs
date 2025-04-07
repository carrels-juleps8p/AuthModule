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
    public partial class Login : Form
    {
        // 定义固定密码
        private readonly Dictionary<string, string> userPasswords = new Dictionary<string, string>()
        {
            { "操作员", "operator" },
            { "管理员", "admin" },
            { "超级管理员", "superadmin" }
        };

        public Login()
        {
            InitializeComponent();
            // 添加登录按钮的点击事件
            btnLogin.Click += BtnLogin_Click;
            // 添加密码框的按键事件
            txtPassword.KeyPress += TxtPassword_KeyPress;
        }

        // 添加密码框的回车键处理事件
        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 判断是否按下回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;  // 阻止回车键的默认处理（声音）
                btnLogin.PerformClick();  // 触发登录按钮的点击事件
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string selectedUser = cboUserType.SelectedItem.ToString();
            string inputPassword = txtPassword.Text;

            if (userPasswords[selectedUser] == inputPassword)
            {
                MessageBox.Show("登录成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 根据用户类型决定是否打开权限管理窗体
                if (selectedUser == "管理员" || selectedUser == "超级管理员")
                {
                    Accessmanager accessForm = new Accessmanager(selectedUser);
                    this.Hide();  // 先隐藏登录窗体
                    accessForm.ShowDialog();  // 显示权限管理窗体
                    this.Close();  // 关闭登录窗体
                }
                else
                {
                    // 操作员直接进入系统主界面
                    MessageBox.Show("欢迎操作员登录系统！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("密码错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
