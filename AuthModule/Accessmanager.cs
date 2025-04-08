using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Web.Script.Serialization;

namespace AuthModule
{
    public partial class Accessmanager : Form
    {
        private string userType;  // 存储用户类型
        // 存储每个角色的权限配置
        private Dictionary<string, HashSet<string>> rolePermissions;
        private ContextMenuStrip treeContextMenu;  // 右键菜单
        private const string PERMISSIONS_FILE = "permissions.json";
        private const string TREE_FILE = "tree.json";
        private JavaScriptSerializer serializer = new JavaScriptSerializer();

        public Accessmanager(string userType)
        {
            InitializeComponent();
            this.userType = userType;
            LoadPermissions();  // 加载权限配置
            InitializeAccessManager();
            LoadTree();  // 加载树形结构
            InitializeContextMenu();  // 初始化右键菜单

            // 添加事件处理
            cboRoleSelect.SelectedIndexChanged += CboRoleSelect_SelectedIndexChanged;
            btnUpdatePermission.Click += BtnUpdatePermission_Click;
        }

        private class TreeNodeData
        {
            public string Text { get; set; }
            public List<TreeNodeData> Nodes { get; set; }
        }

        private void SaveTree()
        {
            try
            {
                var rootNodes = new List<TreeNodeData>();
                foreach (TreeNode node in trvModules.Nodes)
                {
                    rootNodes.Add(ConvertToTreeNodeData(node));
                }
                string jsonString = serializer.Serialize(rootNodes);
                File.WriteAllText(TREE_FILE, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存树形结构失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TreeNodeData ConvertToTreeNodeData(TreeNode node)
        {
            var data = new TreeNodeData
            {
                Text = node.Text,
                Nodes = new List<TreeNodeData>()
            };

            foreach (TreeNode childNode in node.Nodes)
            {
                data.Nodes.Add(ConvertToTreeNodeData(childNode));
            }

            return data;
        }

        private void LoadTree()
        {
            try
            {
                if (File.Exists(TREE_FILE))
                {
                    string jsonString = File.ReadAllText(TREE_FILE);
                    var rootNodes = serializer.Deserialize<List<TreeNodeData>>(jsonString);
                    trvModules.Nodes.Clear();
                    foreach (var nodeData in rootNodes)
                    {
                        trvModules.Nodes.Add(ConvertToTreeNode(nodeData));
                    }
                }
                else
                {
                    // 如果文件不存在，使用默认节点
                    InitializeDefaultTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载树形结构失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                InitializeDefaultTree();
            }

            trvModules.ExpandAll();
            UpdateTreeViewCheckState();
        }

        private TreeNode ConvertToTreeNode(TreeNodeData data)
        {
            var node = new TreeNode(data.Text);
            foreach (var childData in data.Nodes)
            {
                node.Nodes.Add(ConvertToTreeNode(childData));
            }
            return node;
        }

        private void InitializeDefaultTree()
        {
            trvModules.Nodes.Clear();
            TreeNode basicNode = trvModules.Nodes.Add("基础功能");
            basicNode.Nodes.Add("查看");
            basicNode.Nodes.Add("导出");
            basicNode.Nodes.Add("编辑");

            TreeNode systemNode = trvModules.Nodes.Add("系统管理");
            systemNode.Nodes.Add("用户管理");
            systemNode.Nodes.Add("权限管理");
        }

        private void LoadPermissions()
        {
            try
            {
                if (File.Exists(PERMISSIONS_FILE))
                {
                    string jsonString = File.ReadAllText(PERMISSIONS_FILE);
                    var tempDict = serializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
                    rolePermissions = new Dictionary<string, HashSet<string>>();
                    foreach (var kvp in tempDict)
                    {
                        rolePermissions[kvp.Key] = new HashSet<string>(kvp.Value);
                    }
                }
                else
                {
                    // 如果文件不存在，使用默认配置
                    rolePermissions = new Dictionary<string, HashSet<string>>()
                    {
                        { "操作员", new HashSet<string>() { "基础功能.查看", "基础功能.导出" } },
                        { "管理员", new HashSet<string>() { "基础功能.查看", "基础功能.导出", "基础功能.编辑", "系统管理.用户管理" } },
                        { "超级管理员", new HashSet<string>() { "基础功能.查看", "基础功能.导出", "基础功能.编辑", "系统管理.用户管理", "系统管理.权限管理" } }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载权限配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 使用默认配置
                rolePermissions = new Dictionary<string, HashSet<string>>()
                {
                    { "操作员", new HashSet<string>() { "基础功能.查看", "基础功能.导出" } },
                    { "管理员", new HashSet<string>() { "基础功能.查看", "基础功能.导出", "基础功能.编辑", "系统管理.用户管理" } },
                    { "超级管理员", new HashSet<string>() { "基础功能.查看", "基础功能.导出", "基础功能.编辑", "系统管理.用户管理", "系统管理.权限管理" } }
                };
            }
        }

        private void SavePermissions()
        {
            try
            {
                // 将 HashSet 转换为 List 以便序列化
                var tempDict = new Dictionary<string, List<string>>();
                foreach (var kvp in rolePermissions)
                {
                    tempDict[kvp.Key] = kvp.Value.ToList();
                }
                
                string jsonString = serializer.Serialize(tempDict);
                File.WriteAllText(PERMISSIONS_FILE, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存权限配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeAccessManager()
        {
            // 根据用户类型设置界面
            if (userType == "超级管理员")
            {
                // 超级管理员可以看到和操作所有内容
                btnAddModule.Enabled = true;
                btnUpdatePermission.Enabled = true;
                cboRoleSelect.Enabled = true;
            }
            else if (userType == "管理员")
            {
                // 管理员只能查看，不能添加新模块
                btnAddModule.Enabled = false;
                btnUpdatePermission.Enabled = true;
                cboRoleSelect.Enabled = true;
            }

            // 清空并设置角色下拉框选项
            cboRoleSelect.Items.Clear();
            cboRoleSelect.Items.Add("操作员");  // 第一个选项
            cboRoleSelect.Items.Add("管理员");

            if (userType == "超级管理员")
            {
                cboRoleSelect.Items.Add("超级管理员");
            }

            // 设置默认选中"操作员"
            cboRoleSelect.SelectedIndex = 0;  // 选中第一项（操作员）
            // 或者也可以直接用文本设置：
            // cboRoleSelect.SelectedItem = "操作员";
        }

        private void UpdateTreeViewCheckState()
        {
            string selectedRole = cboRoleSelect.SelectedItem.ToString();
            var permissions = rolePermissions[selectedRole];

            foreach (TreeNode moduleNode in trvModules.Nodes)
            {
                foreach (TreeNode functionNode in moduleNode.Nodes)
                {
                    string permissionKey = $"{moduleNode.Text}.{functionNode.Text}";
                    functionNode.Checked = permissions.Contains(permissionKey);
                }

                // 更新父节点的选中状态
                UpdateParentNodeCheckState(moduleNode);
            }
        }

        private void UpdateParentNodeCheckState(TreeNode parentNode)
        {
            bool allChecked = true;
            bool anyChecked = false;

            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (childNode.Checked)
                    anyChecked = true;
                else
                    allChecked = false;
            }

            parentNode.Checked = allChecked;
        }

        private void CboRoleSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTreeViewCheckState();
        }

        private void BtnUpdatePermission_Click(object sender, EventArgs e)
        {
            string selectedRole = cboRoleSelect.SelectedItem.ToString();
            var newPermissions = new HashSet<string>();

            // 收集选中的权限
            foreach (TreeNode moduleNode in trvModules.Nodes)
            {
                foreach (TreeNode functionNode in moduleNode.Nodes)
                {
                    if (functionNode.Checked)
                    {
                        newPermissions.Add($"{moduleNode.Text}.{functionNode.Text}");
                    }
                }
            }

            // 更新权限配置
            rolePermissions[selectedRole] = newPermissions;
            
            // 保存权限配置到文件
            SavePermissions();

            // 如果当前选中的角色是当前登录用户，更新用户会话中的权限
            if (selectedRole == UserSession.Instance.UserType)
            {
                UserSession.Instance.Initialize(selectedRole, newPermissions);
            }

            MessageBox.Show("权限更新成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 处理TreeView的节点选中状态变化
        private void TrvModules_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // 阻止事件递归
            if (e.Action == TreeViewAction.Unknown) return;

            // 更新子节点
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            }
            // 更新父节点
            else if (e.Node.Parent != null)
            {
                UpdateParentNodeCheckState(e.Node.Parent);
            }
        }

        private void InitializeContextMenu()
        {
            treeContextMenu = new ContextMenuStrip();

            // 添加菜单项
            ToolStripMenuItem addParentItem = new ToolStripMenuItem("添加父模块");
            ToolStripMenuItem addItem = new ToolStripMenuItem("添加子模块");
            ToolStripMenuItem editItem = new ToolStripMenuItem("修改模块");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("删除模块");

            // 添加事件处理
            addParentItem.Click += AddParentNode_Click;
            addItem.Click += AddNode_Click;
            editItem.Click += EditNode_Click;
            deleteItem.Click += DeleteNode_Click;

            // 将菜单项添加到右键菜单
            treeContextMenu.Items.AddRange(new ToolStripItem[] { addParentItem, addItem, editItem, deleteItem });

            // 将右键菜单绑定到TreeView
            trvModules.ContextMenuStrip = treeContextMenu;
        }

        private void AddParentNode_Click(object sender, EventArgs e)
        {
            using (var inputDialog = new InputDialog("添加父模块", "请输入父模块名称:"))
            {
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    string nodeName = inputDialog.InputText;
                    if (!string.IsNullOrWhiteSpace(nodeName))
                    {
                        // 检查父节点是否已存在
                        if (IsParentNodeExists(nodeName))
                        {
                            MessageBox.Show("该父模块已存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        TreeNode newNode = trvModules.Nodes.Add(nodeName);
                        trvModules.SelectedNode = newNode;
                        SaveTree();  // 保存树形结构
                    }
                }
            }
        }

        private void AddNode_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = trvModules.SelectedNode;
            if (selectedNode == null) return;

            using (var inputDialog = new InputDialog("添加模块", "请输入模块名称:"))
            {
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    string nodeName = inputDialog.InputText;
                    if (!string.IsNullOrWhiteSpace(nodeName))
                    {
                        // 检查当前父节点下是否已存在同名子节点
                        if (IsChildNodeExists(selectedNode, nodeName))
                        {
                            MessageBox.Show("该模块在当前父模块下已存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        TreeNode newNode = selectedNode.Nodes.Add(nodeName);
                        trvModules.SelectedNode = newNode;
                        SaveTree();  // 保存树形结构
                    }
                }
            }
        }

        private void EditNode_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = trvModules.SelectedNode;
            if (selectedNode == null) return;

            using (var inputDialog = new InputDialog("修改模块", "请输入新的模块名称:", selectedNode.Text))
            {
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    string newName = inputDialog.InputText;
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        // 如果是父节点，检查新名称是否与其他父节点重复
                        if (selectedNode.Parent == null)
                        {
                            if (IsParentNodeExists(newName, selectedNode))
                            {
                                MessageBox.Show("该父模块名称已存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        // 如果是子节点，检查新名称是否与当前父节点下的其他子节点重复
                        else
                        {
                            if (IsChildNodeExists(selectedNode.Parent, newName, selectedNode))
                            {
                                MessageBox.Show("该模块在当前父模块下已存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        selectedNode.Text = newName;
                        SaveTree();  // 保存树形结构
                    }
                }
            }
        }

        private bool IsParentNodeExists(string nodeName, TreeNode excludeNode = null)
        {
            foreach (TreeNode node in trvModules.Nodes)
            {
                if (excludeNode != null && node == excludeNode) continue;
                if (node.Text.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsChildNodeExists(TreeNode parentNode, string nodeName, TreeNode excludeNode = null)
        {
            foreach (TreeNode node in parentNode.Nodes)
            {
                if (excludeNode != null && node == excludeNode) continue;
                if (node.Text.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void DeleteNode_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = trvModules.SelectedNode;
            if (selectedNode == null) return;

            if (MessageBox.Show("确定要删除该模块吗？", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                selectedNode.Remove();
                SaveTree();  // 保存树形结构
            }
        }
    }

    // 输入对话框窗体
    public class InputDialog : Form
    {
        private TextBox txtInput;
        private Button btnOK;
        private Button btnCancel;
        private Label lblPrompt;

        public string InputText
        {
            get { return txtInput.Text; }
        }

        public InputDialog(string title, string prompt, string defaultText = "")
        {
            this.Text = title;
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblPrompt = new Label();
            lblPrompt.Text = prompt;
            lblPrompt.Location = new Point(10, 10);
            lblPrompt.AutoSize = true;

            txtInput = new TextBox();
            txtInput.Location = new Point(10, 30);
            txtInput.Size = new Size(265, 25);
            txtInput.Text = defaultText;

            btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(110, 70);

            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(190, 70);

            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
    }
}
