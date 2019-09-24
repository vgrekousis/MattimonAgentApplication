namespace MattimonAgentApplication
{
    partial class SQLServerInstanceDialog
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
            this.pnlBackground = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCredsAndLoading = new System.Windows.Forms.Panel();
            this.pnlSqlCred = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.cboServers = new System.Windows.Forms.ComboBox();
            this.btnRefreshSources = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPwrd = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnTestConnection = new MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton();
            this.btnAccept = new MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton();
            this.btnCancel = new MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton();
            this.pnlBackground.SuspendLayout();
            this.pnlCredsAndLoading.SuspendLayout();
            this.pnlSqlCred.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.AutoSize = true;
            this.pnlBackground.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBackground.Controls.Add(this.pnlCredsAndLoading);
            this.pnlBackground.Controls.Add(this.pnlButtons);
            this.pnlBackground.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlBackground.Location = new System.Drawing.Point(0, 38);
            this.pnlBackground.Margin = new System.Windows.Forms.Padding(0, 0, 1, 1);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(354, 207);
            this.pnlBackground.TabIndex = 14;
            // 
            // pnlCredsAndLoading
            // 
            this.pnlCredsAndLoading.AutoSize = true;
            this.pnlCredsAndLoading.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlCredsAndLoading.Controls.Add(this.pnlSqlCred);
            this.pnlCredsAndLoading.Location = new System.Drawing.Point(3, 3);
            this.pnlCredsAndLoading.Name = "pnlCredsAndLoading";
            this.pnlCredsAndLoading.Size = new System.Drawing.Size(347, 159);
            this.pnlCredsAndLoading.TabIndex = 12;
            // 
            // pnlSqlCred
            // 
            this.pnlSqlCred.AutoSize = true;
            this.pnlSqlCred.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSqlCred.Controls.Add(this.label2);
            this.pnlSqlCred.Controls.Add(this.flowLayoutPanel2);
            this.pnlSqlCred.Controls.Add(this.label1);
            this.pnlSqlCred.Controls.Add(this.txtUser);
            this.pnlSqlCred.Controls.Add(this.label3);
            this.pnlSqlCred.Controls.Add(this.txtPwrd);
            this.pnlSqlCred.Controls.Add(this.lblStatus);
            this.pnlSqlCred.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlSqlCred.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlSqlCred.Location = new System.Drawing.Point(3, 3);
            this.pnlSqlCred.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSqlCred.Name = "pnlSqlCred";
            this.pnlSqlCred.Size = new System.Drawing.Size(344, 156);
            this.pnlSqlCred.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Server name:";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.cboServers);
            this.flowLayoutPanel2.Controls.Add(this.btnRefreshSources);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 16);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(342, 30);
            this.flowLayoutPanel2.TabIndex = 7;
            // 
            // cboServers
            // 
            this.cboServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboServers.FormattingEnabled = true;
            this.cboServers.ItemHeight = 16;
            this.cboServers.Location = new System.Drawing.Point(3, 3);
            this.cboServers.Name = "cboServers";
            this.cboServers.Size = new System.Drawing.Size(242, 24);
            this.cboServers.TabIndex = 0;
            this.cboServers.SelectedIndexChanged += new System.EventHandler(this.CboServers_SelectedIndexChanged);
            // 
            // btnRefreshSources
            // 
            this.btnRefreshSources.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRefreshSources.Location = new System.Drawing.Point(251, 3);
            this.btnRefreshSources.Name = "btnRefreshSources";
            this.btnRefreshSources.Size = new System.Drawing.Size(88, 23);
            this.btnRefreshSources.TabIndex = 9;
            this.btnRefreshSources.Text = "Refresh";
            this.btnRefreshSources.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Username:";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(3, 65);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(338, 22);
            this.txtUser.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password:";
            // 
            // txtPwrd
            // 
            this.txtPwrd.Location = new System.Drawing.Point(3, 109);
            this.txtPwrd.Name = "txtPwrd";
            this.txtPwrd.Size = new System.Drawing.Size(338, 22);
            this.txtPwrd.TabIndex = 5;
            this.txtPwrd.UseSystemPasswordChar = true;
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(3, 134);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(338, 22);
            this.lblStatus.TabIndex = 12;
            this.lblStatus.Text = "label4";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.Controls.Add(this.btnTestConnection);
            this.pnlButtons.Controls.Add(this.btnAccept);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Location = new System.Drawing.Point(3, 168);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(348, 36);
            this.pnlButtons.TabIndex = 11;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(100)))));
            this.btnTestConnection.ForeColor = System.Drawing.Color.White;
            this.btnTestConnection.Location = new System.Drawing.Point(3, 3);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(182, 30);
            this.btnTestConnection.TabIndex = 9;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = false;
            // 
            // btnAccept
            // 
            this.btnAccept.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(100)))));
            this.btnAccept.ForeColor = System.Drawing.Color.White;
            this.btnAccept.Location = new System.Drawing.Point(191, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(77, 30);
            this.btnAccept.TabIndex = 8;
            this.btnAccept.Text = "OK";
            this.btnAccept.UseVisualStyleBackColor = false;
            this.btnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(100)))));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(274, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(71, 30);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // SQLServerInstanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionForeColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(389, 270);
            this.Controls.Add(this.pnlBackground);
            this.EnableFormDragOpacity = true;
            this.FormBorderColor = System.Drawing.Color.Black;
            this.LostFocusBorderColor = System.Drawing.Color.Silver;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SQLServerInstanceDialog";
            this.Text = "SQL Server Instance Dialog";
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            this.pnlCredsAndLoading.ResumeLayout(false);
            this.pnlCredsAndLoading.PerformLayout();
            this.pnlSqlCred.ResumeLayout(false);
            this.pnlSqlCred.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel pnlBackground;
        private System.Windows.Forms.Panel pnlCredsAndLoading;
        private System.Windows.Forms.FlowLayoutPanel pnlSqlCred;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.ComboBox cboServers;
        private System.Windows.Forms.Button btnRefreshSources;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPwrd;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton btnTestConnection;
        private MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton btnAccept;
        private MattimonAgentApplication.GUI.BitscoreForms.Controls.BorderlessFlatButton btnCancel;
    }
}