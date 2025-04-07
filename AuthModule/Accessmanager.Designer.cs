
namespace AuthModule
{
    partial class Accessmanager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cboRoleSelect = new System.Windows.Forms.ComboBox();
            this.grpModules = new System.Windows.Forms.GroupBox();
            this.btnUpdatePermission = new System.Windows.Forms.Button();
            this.btnAddModule = new System.Windows.Forms.Button();
            this.trvModules = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboRoleSelect
            // 
            this.cboRoleSelect.FormattingEnabled = true;
            this.cboRoleSelect.Location = new System.Drawing.Point(192, 51);
            this.cboRoleSelect.Name = "cboRoleSelect";
            this.cboRoleSelect.Size = new System.Drawing.Size(121, 26);
            this.cboRoleSelect.TabIndex = 0;
            // 
            // grpModules
            // 
            this.grpModules.Location = new System.Drawing.Point(85, 106);
            this.grpModules.Name = "grpModules";
            this.grpModules.Size = new System.Drawing.Size(625, 298);
            this.grpModules.TabIndex = 1;
            this.grpModules.TabStop = false;
            this.grpModules.Controls.Add(this.trvModules);
            this.grpModules.Text = "功能模块";
            // 
            // btnUpdatePermission
            // 
            this.btnUpdatePermission.Location = new System.Drawing.Point(497, 40);
            this.btnUpdatePermission.Name = "btnUpdatePermission";
            this.btnUpdatePermission.Size = new System.Drawing.Size(127, 37);
            this.btnUpdatePermission.TabIndex = 2;
            this.btnUpdatePermission.Text = "更新权限";
            this.btnUpdatePermission.UseVisualStyleBackColor = true;
            // 
            // btnAddModule
            // 
            this.btnAddModule.Location = new System.Drawing.Point(340, 40);
            this.btnAddModule.Name = "btnAddModule";
            this.btnAddModule.Size = new System.Drawing.Size(138, 37);
            this.btnAddModule.TabIndex = 4;
            this.btnAddModule.Text = "添加模块";
            this.btnAddModule.UseVisualStyleBackColor = true;
            // 
            // trvModules
            // 
            this.trvModules.CheckBoxes = true;
            this.trvModules.Location = new System.Drawing.Point(6, 25);
            this.trvModules.Name = "trvModules";
            this.trvModules.Size = new System.Drawing.Size(613, 267);
            this.trvModules.TabIndex = 0;
            this.trvModules.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TrvModules_AfterCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(97, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "切换用户:";
            // 
            // Accessmanager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Controls.Add(this.btnAddModule);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUpdatePermission);
            this.Controls.Add(this.grpModules);
            this.Controls.Add(this.cboRoleSelect);
            this.Name = "Accessmanager";
            this.Text = "用户权限分配";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboRoleSelect;
        private System.Windows.Forms.GroupBox grpModules;
        private System.Windows.Forms.Button btnUpdatePermission;
        private System.Windows.Forms.Button btnAddModule;
        private System.Windows.Forms.TreeView trvModules;
        private System.Windows.Forms.Label label1;
    }
}