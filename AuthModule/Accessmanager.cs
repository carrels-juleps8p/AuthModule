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
    /// <summary>
    /// 权限管理窗体类，负责管理用户权限和模块结构
    /// </summary>
    public partial class Accessmanager : Form
    {
        private string userType;  // 存储当前用户类型
        // 存储每个角色的权限配置，键为角色名称，值为该角色拥有的权限集合
        private Dictionary<string, HashSet<string>> rolePermissions;
        private ContextMenuStrip treeContextMenu;  // 树形控件的右键菜单
        private const string PERMISSIONS_FILE = "permissions.json";  // 权限配置文件路径
        private const string TREE_FILE = "tree.json";  // 树形结构配置文件路径
        private JavaScriptSerializer serializer = new JavaScriptSerializer();  // JSON序列化器

        /// <summary>
        /// 权限管理窗体构造函数
        /// </summary>
        /// <param name="userType">当前用户类型</param>
        public Accessmanager(string userType)
        {
            InitializeComponent();
            this.userType = userType;
            LoadPermissions();  // 加载权限配置
            InitializeAccessManager();  // 初始化权限管理器
            LoadTree();  // 加载树形结构
            InitializeContextMenu();  // 初始化右键菜单

            // 添加事件处理
            cboRoleSelect.SelectedIndexChanged += CboRoleSelect_SelectedIndexChanged;
            btnUpdatePermission.Click += BtnUpdatePermission_Click;
        }

        /// <summary>
        /// 树节点数据类，用于序列化树形结构
        /// </summary>
        private class TreeNodeData
        {
            public string Text { get; set; }  // 节点文本
            public List<TreeNodeData> Nodes { get; set; }  // 子节点列表
        }

        /// <summary>
        /// 保存树形结构到文件
        /// </summary>
        private void SaveTree()
        {
            try
            {
                // 将树形结构转换为可序列化的数据
                var rootNodes = new List<TreeNodeData>();
                foreach (TreeNode node in trvModules.Nodes)
                {
                    rootNodes.Add(ConvertToTreeNodeData(node));
                }
                // 序列化并保存到文件
                string jsonString = serializer.Serialize(rootNodes);
                File.WriteAllText(TREE_FILE, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存树形结构失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 将TreeNode转换为可序列化的TreeNodeData
        /// </summary>
        /// <param name="node">要转换的TreeNode</param>
        /// <returns>转换后的TreeNodeData</returns>
        private TreeNodeData ConvertToTreeNodeData(TreeNode node)
        {
            var data = new TreeNodeData
            {
                Text = node.Text,
                Nodes = new List<TreeNodeData>()
            };

            // 递归转换所有子节点
            foreach (TreeNode childNode in node.Nodes)
            {
                data.Nodes.Add(ConvertToTreeNodeData(childNode));
            }

            return data;
        }

        /// <summary>
        /// 从文件加载树形结构
        /// </summary>
        private void LoadTree()
        {
            try
            {
                if (File.Exists(TREE_FILE))
                {
                    // 从文件读取并反序列化树形结构
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

            trvModules.ExpandAll();  // 展开所有节点
            UpdateTreeViewCheckState();  // 更新节点选中状态
        }

        /// <summary>
        /// 将TreeNodeData转换为TreeNode
        /// </summary>
        /// <param name="data">要转换的TreeNodeData</param>
        /// <returns>转换后的TreeNode</returns>
        private TreeNode ConvertToTreeNode(TreeNodeData data)
        {
            var node = new TreeNode(data.Text);
            // 递归转换所有子节点
            foreach (var childData in data.Nodes)
            {
                node.Nodes.Add(ConvertToTreeNode(childData));
            }
            return node;
        }

        /// <summary>
        /// 初始化默认的树形结构
        /// </summary>
        private void InitializeDefaultTree()
        {
            trvModules.Nodes.Clear();
            // 添加基础功能模块
            TreeNode basicNode = trvModules.Nodes.Add("基础功能");
            basicNode.Nodes.Add("查看");
            basicNode.Nodes.Add("导出");
            basicNode.Nodes.Add("编辑");

            // 添加系统管理模块
            TreeNode systemNode = trvModules.Nodes.Add("系统管理");
            systemNode.Nodes.Add("用户管理");
            systemNode.Nodes.Add("权限管理");
        }

        /// <summary>
        /// 从文件加载权限配置
        /// </summary>
        private void LoadPermissions()
        {
            try
            {
                if (File.Exists(PERMISSIONS_FILE))
                {
                    // 从文件读取并反序列化权限配置
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

        /// <summary>
        /// 保存权限配置到文件
        /// </summary>
        private void SavePermissions()
        {
            try
            {
                // 将HashSet转换为List以便序列化
                var tempDict = new Dictionary<string, List<string>>();
                foreach (var kvp in rolePermissions)
                {
                    tempDict[kvp.Key] = kvp.Value.ToList();
                }
                
                // 序列化并保存到文件
                string jsonString = serializer.Serialize(tempDict);
                File.WriteAllText(PERMISSIONS_FILE, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存权限配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化权限管理器界面
        /// </summary>
        private void InitializeAccessManager()
        {
            // 根据用户类型设置界面控件的可用状态
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
            cboRoleSelect.SelectedIndex = 0;
        }

        /// <summary>
        /// 更新树形控件的节点选中状态
        /// </summary>
        private void UpdateTreeViewCheckState()
        {
            string selectedRole = cboRoleSelect.SelectedItem.ToString();
            var permissions = rolePermissions[selectedRole];

            // 遍历所有节点并更新选中状态
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

        /// <summary>
        /// 更新父节点的选中状态
        /// </summary>
        /// <param name="parentNode">要更新的父节点</param>
        private void UpdateParentNodeCheckState(TreeNode parentNode)
        {
            bool allChecked = true;
            bool anyChecked = false;

            // 检查所有子节点的状态
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (childNode.Checked)
                    anyChecked = true;
                else
                    allChecked = false;
            }

            // 如果所有子节点都选中，则父节点选中
            // 如果部分子节点选中，则父节点显示为不确定状态
            parentNode.Checked = allChecked;
        }

        /// <summary>
        /// 处理角色选择变化事件
        /// </summary>
        private void CboRoleSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTreeViewCheckState();
        }

        /// <summary>
        /// 处理更新权限按钮点击事件
        /// </summary>
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

        /// <summary>
        /// 处理树形控件的节点选中状态变化事件
        /// </summary>
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

        /// <summary>
        /// 初始化右键菜单
        /// </summary>
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

        /// <summary>
        /// 处理添加父模块菜单项点击事件
        /// </summary>
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

                        // 添加新节点并保存
                        TreeNode newNode = trvModules.Nodes.Add(nodeName);
                        trvModules.SelectedNode = newNode;
                        SaveTree();
                    }
                }
            }
        }

        /// <summary>
        /// 处理添加子模块菜单项点击事件
        /// </summary>
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

                        // 添加新节点并保存
                        TreeNode newNode = selectedNode.Nodes.Add(nodeName);
                        trvModules.SelectedNode = newNode;
                        SaveTree();
                    }
                }
            }
        }

        /// <summary>
        /// 处理修改模块菜单项点击事件
        /// </summary>
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

                        // 更新节点名称并保存
                        selectedNode.Text = newName;
                        SaveTree();
                    }
                }
            }
        }

        /// <summary>
        /// 检查父节点是否已存在
        /// </summary>
        /// <param name="nodeName">要检查的节点名称</param>
        /// <param name="excludeNode">要排除的节点（用于编辑时）</param>
        /// <returns>如果节点已存在则返回true，否则返回false</returns>
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

        /// <summary>
        /// 检查子节点是否已存在
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="nodeName">要检查的节点名称</param>
        /// <param name="excludeNode">要排除的节点（用于编辑时）</param>
        /// <returns>如果节点已存在则返回true，否则返回false</returns>
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

        /// <summary>
        /// 处理删除模块菜单项点击事件
        /// </summary>
        private void DeleteNode_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = trvModules.SelectedNode;
            if (selectedNode == null) return;

            if (MessageBox.Show("确定要删除该模块吗？", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                selectedNode.Remove();
                SaveTree();
            }
        }
    }

    /// <summary>
    /// 输入对话框窗体类，用于获取用户输入
    /// </summary>
    public class InputDialog : Form
    {
        private TextBox txtInput;  // 输入文本框
        private Button btnOK;  // 确定按钮
        private Button btnCancel;  // 取消按钮
        private Label lblPrompt;  // 提示标签

        /// <summary>
        /// 获取用户输入的文本
        /// </summary>
        public string InputText
        {
            get { return txtInput.Text; }
        }

        /// <summary>
        /// 输入对话框构造函数
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="prompt">提示文本</param>
        /// <param name="defaultText">默认文本</param>
        public InputDialog(string title, string prompt, string defaultText = "")
        {
            this.Text = title;
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建并设置提示标签
            lblPrompt = new Label();
            lblPrompt.Text = prompt;
            lblPrompt.Location = new Point(10, 10);
            lblPrompt.AutoSize = true;

            // 创建并设置输入文本框
            txtInput = new TextBox();
            txtInput.Location = new Point(10, 30);
            txtInput.Size = new Size(265, 25);
            txtInput.Text = defaultText;

            // 创建并设置确定按钮
            btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(110, 70);

            // 创建并设置取消按钮
            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(190, 70);

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
    }
}
