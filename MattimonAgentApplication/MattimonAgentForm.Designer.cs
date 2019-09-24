namespace MattimonAgentApplication
{
    partial class MattimonAgentForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MattimonAgentForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblUpdVersion = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flMattiServicesInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRestartSvc = new System.Windows.Forms.Button();
            this.btnStartSvc = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.grpboxSQLSrv = new System.Windows.Forms.GroupBox();
            this.btnNewSqlInstance = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.grpBoxPorts = new System.Windows.Forms.GroupBox();
            this.cboAvailablePorts = new System.Windows.Forms.ComboBox();
            this.lblPortName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdo1min = new System.Windows.Forms.RadioButton();
            this.rdo15mins = new System.Windows.Forms.RadioButton();
            this.rdo10mins = new System.Windows.Forms.RadioButton();
            this.rdo5mins = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rdoNotifEmailsNo = new System.Windows.Forms.RadioButton();
            this.rdoNotifEmailsYes = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblNoteMattimonAgent = new System.Windows.Forms.Label();
            this.rdoMattiAgentNo = new System.Windows.Forms.RadioButton();
            this.rdoMattiAgentYes = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkMonitorSQL = new System.Windows.Forms.CheckBox();
            this.flHeaderLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCompany = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flHeaderContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.panelHeaderContainer = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel6.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.grpboxSQLSrv.SuspendLayout();
            this.grpBoxPorts.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.flHeaderLabels.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flHeaderContainer.SuspendLayout();
            this.panelHeaderContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnApply);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel6);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 358);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(803, 36);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(681, 2);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 32);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(557, 2);
            this.btnApply.Margin = new System.Windows.Forms.Padding(2);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(120, 32);
            this.btnApply.TabIndex = 1;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel6.Location = new System.Drawing.Point(33, 3);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(519, 30);
            this.flowLayoutPanel6.TabIndex = 2;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(208, 29);
            this.flowLayoutPanel2.TabIndex = 14;
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.AutoSize = true;
            this.flowLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel5.Controls.Add(this.lblVersion);
            this.flowLayoutPanel5.Controls.Add(this.lblUpdVersion);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(6, 8);
            this.flowLayoutPanel5.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(196, 13);
            this.flowLayoutPanel5.TabIndex = 5;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(6, 0);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(86, 13);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "AssemblyVersion";
            // 
            // lblUpdVersion
            // 
            this.lblUpdVersion.AutoSize = true;
            this.lblUpdVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUpdVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdVersion.ForeColor = System.Drawing.Color.Purple;
            this.lblUpdVersion.Location = new System.Drawing.Point(104, 0);
            this.lblUpdVersion.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblUpdVersion.Name = "lblUpdVersion";
            this.lblUpdVersion.Size = new System.Drawing.Size(86, 13);
            this.lblUpdVersion.TabIndex = 4;
            this.lblUpdVersion.Text = "AssemblyVersion";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 45);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(803, 310);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flMattiServicesInfo);
            this.tabPage1.Controls.Add(this.btnRestartSvc);
            this.tabPage1.Controls.Add(this.btnStartSvc);
            this.tabPage1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(795, 281);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Mattimon Services";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // flMattiServicesInfo
            // 
            this.flMattiServicesInfo.AutoSize = true;
            this.flMattiServicesInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flMattiServicesInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flMattiServicesInfo.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flMattiServicesInfo.Location = new System.Drawing.Point(3, 278);
            this.flMattiServicesInfo.Name = "flMattiServicesInfo";
            this.flMattiServicesInfo.Size = new System.Drawing.Size(789, 0);
            this.flMattiServicesInfo.TabIndex = 4;
            // 
            // btnRestartSvc
            // 
            this.btnRestartSvc.Location = new System.Drawing.Point(7, 41);
            this.btnRestartSvc.Margin = new System.Windows.Forms.Padding(2);
            this.btnRestartSvc.Name = "btnRestartSvc";
            this.btnRestartSvc.Size = new System.Drawing.Size(279, 32);
            this.btnRestartSvc.TabIndex = 3;
            this.btnRestartSvc.Text = "Restart Services";
            this.btnRestartSvc.UseVisualStyleBackColor = true;
            this.btnRestartSvc.Click += new System.EventHandler(this.BtnRestartSvc_Click);
            // 
            // btnStartSvc
            // 
            this.btnStartSvc.Location = new System.Drawing.Point(7, 5);
            this.btnStartSvc.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartSvc.Name = "btnStartSvc";
            this.btnStartSvc.Size = new System.Drawing.Size(279, 32);
            this.btnStartSvc.TabIndex = 2;
            this.btnStartSvc.Text = "Start / Stop Services";
            this.btnStartSvc.UseVisualStyleBackColor = true;
            this.btnStartSvc.Click += new System.EventHandler(this.BtnStartSvc_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.grpboxSQLSrv);
            this.tabPage2.Controls.Add(this.grpBoxPorts);
            this.tabPage2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(795, 281);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Monitoring Settings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // grpboxSQLSrv
            // 
            this.grpboxSQLSrv.AutoSize = true;
            this.grpboxSQLSrv.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpboxSQLSrv.Controls.Add(this.btnNewSqlInstance);
            this.grpboxSQLSrv.Controls.Add(this.checkedListBox1);
            this.grpboxSQLSrv.Location = new System.Drawing.Point(8, 96);
            this.grpboxSQLSrv.Name = "grpboxSQLSrv";
            this.grpboxSQLSrv.Size = new System.Drawing.Size(275, 167);
            this.grpboxSQLSrv.TabIndex = 10;
            this.grpboxSQLSrv.TabStop = false;
            this.grpboxSQLSrv.Text = "Monitored SQL Instances";
            // 
            // btnNewSqlInstance
            // 
            this.btnNewSqlInstance.Location = new System.Drawing.Point(6, 115);
            this.btnNewSqlInstance.Margin = new System.Windows.Forms.Padding(2);
            this.btnNewSqlInstance.Name = "btnNewSqlInstance";
            this.btnNewSqlInstance.Size = new System.Drawing.Size(263, 32);
            this.btnNewSqlInstance.TabIndex = 12;
            this.btnNewSqlInstance.Text = "Add New...";
            this.btnNewSqlInstance.UseVisualStyleBackColor = true;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(7, 21);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(262, 89);
            this.checkedListBox1.TabIndex = 11;
            // 
            // grpBoxPorts
            // 
            this.grpBoxPorts.AutoSize = true;
            this.grpBoxPorts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpBoxPorts.Controls.Add(this.cboAvailablePorts);
            this.grpBoxPorts.Controls.Add(this.lblPortName);
            this.grpBoxPorts.Controls.Add(this.label2);
            this.grpBoxPorts.Location = new System.Drawing.Point(8, 6);
            this.grpBoxPorts.Name = "grpBoxPorts";
            this.grpBoxPorts.Size = new System.Drawing.Size(275, 84);
            this.grpBoxPorts.TabIndex = 9;
            this.grpBoxPorts.TabStop = false;
            this.grpBoxPorts.Text = "Monitor TCP Connections";
            // 
            // cboAvailablePorts
            // 
            this.cboAvailablePorts.DropDownHeight = 120;
            this.cboAvailablePorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAvailablePorts.FormattingEnabled = true;
            this.cboAvailablePorts.IntegralHeight = false;
            this.cboAvailablePorts.Location = new System.Drawing.Point(6, 39);
            this.cboAvailablePorts.Name = "cboAvailablePorts";
            this.cboAvailablePorts.Size = new System.Drawing.Size(121, 24);
            this.cboAvailablePorts.TabIndex = 7;
            this.cboAvailablePorts.SelectedIndexChanged += new System.EventHandler(this.CboAvailablePorts_SelectedIndexChanged);
            this.cboAvailablePorts.SelectedValueChanged += new System.EventHandler(this.CboAvailablePorts_SelectedValueChanged);
            // 
            // lblPortName
            // 
            this.lblPortName.AutoSize = true;
            this.lblPortName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.lblPortName.Location = new System.Drawing.Point(133, 42);
            this.lblPortName.Name = "lblPortName";
            this.lblPortName.Size = new System.Drawing.Size(83, 16);
            this.lblPortName.TabIndex = 8;
            this.lblPortName.Text = "lblPortName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(263, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Select one of the available ports to monitor:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.flowLayoutPanel3);
            this.tabPage3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(795, 281);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = "Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoScroll = true;
            this.flowLayoutPanel3.Controls.Add(this.groupBox1);
            this.flowLayoutPanel3.Controls.Add(this.groupBox2);
            this.flowLayoutPanel3.Controls.Add(this.groupBox3);
            this.flowLayoutPanel3.Controls.Add(this.groupBox4);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(789, 275);
            this.flowLayoutPanel3.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.rdo1min);
            this.groupBox1.Controls.Add(this.rdo15mins);
            this.groupBox1.Controls.Add(this.rdo10mins);
            this.groupBox1.Controls.Add(this.rdo5mins);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(161, 140);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reporting Intervals";
            // 
            // rdo1min
            // 
            this.rdo1min.AutoSize = true;
            this.rdo1min.Checked = true;
            this.rdo1min.Location = new System.Drawing.Point(6, 21);
            this.rdo1min.Name = "rdo1min";
            this.rdo1min.Size = new System.Drawing.Size(135, 20);
            this.rdo1min.TabIndex = 3;
            this.rdo1min.TabStop = true;
            this.rdo1min.Tag = "1";
            this.rdo1min.Text = "Interval of 1 minute";
            this.rdo1min.UseVisualStyleBackColor = true;
            this.rdo1min.CheckedChanged += new System.EventHandler(this.CheckedChanged_Interval);
            // 
            // rdo15mins
            // 
            this.rdo15mins.AutoSize = true;
            this.rdo15mins.Location = new System.Drawing.Point(6, 99);
            this.rdo15mins.Name = "rdo15mins";
            this.rdo15mins.Size = new System.Drawing.Size(149, 20);
            this.rdo15mins.TabIndex = 2;
            this.rdo15mins.Tag = "15";
            this.rdo15mins.Text = "Interval of 15 minutes";
            this.rdo15mins.UseVisualStyleBackColor = true;
            this.rdo15mins.CheckedChanged += new System.EventHandler(this.CheckedChanged_Interval);
            // 
            // rdo10mins
            // 
            this.rdo10mins.AutoSize = true;
            this.rdo10mins.Location = new System.Drawing.Point(6, 73);
            this.rdo10mins.Name = "rdo10mins";
            this.rdo10mins.Size = new System.Drawing.Size(149, 20);
            this.rdo10mins.TabIndex = 1;
            this.rdo10mins.Tag = "10";
            this.rdo10mins.Text = "Interval of 10 minutes";
            this.rdo10mins.UseVisualStyleBackColor = true;
            this.rdo10mins.CheckedChanged += new System.EventHandler(this.CheckedChanged_Interval);
            // 
            // rdo5mins
            // 
            this.rdo5mins.AutoSize = true;
            this.rdo5mins.Location = new System.Drawing.Point(6, 47);
            this.rdo5mins.Name = "rdo5mins";
            this.rdo5mins.Size = new System.Drawing.Size(142, 20);
            this.rdo5mins.TabIndex = 0;
            this.rdo5mins.Tag = "5";
            this.rdo5mins.Text = "Interval of 5 minutes";
            this.rdo5mins.UseVisualStyleBackColor = true;
            this.rdo5mins.CheckedChanged += new System.EventHandler(this.CheckedChanged_Interval);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.rdoNotifEmailsNo);
            this.groupBox2.Controls.Add(this.rdoNotifEmailsYes);
            this.groupBox2.Location = new System.Drawing.Point(170, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(262, 140);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Enable E-mail Notifications";
            // 
            // rdoNotifEmailsNo
            // 
            this.rdoNotifEmailsNo.AutoSize = true;
            this.rdoNotifEmailsNo.Location = new System.Drawing.Point(6, 51);
            this.rdoNotifEmailsNo.Name = "rdoNotifEmailsNo";
            this.rdoNotifEmailsNo.Size = new System.Drawing.Size(250, 20);
            this.rdoNotifEmailsNo.TabIndex = 1;
            this.rdoNotifEmailsNo.Tag = "10";
            this.rdoNotifEmailsNo.Text = "No, I don\'t want to receive notifications";
            this.rdoNotifEmailsNo.UseVisualStyleBackColor = true;
            this.rdoNotifEmailsNo.CheckedChanged += new System.EventHandler(this.CheckedChanged_NotificationEmails);
            // 
            // rdoNotifEmailsYes
            // 
            this.rdoNotifEmailsYes.AutoSize = true;
            this.rdoNotifEmailsYes.Checked = true;
            this.rdoNotifEmailsYes.Location = new System.Drawing.Point(6, 25);
            this.rdoNotifEmailsYes.Name = "rdoNotifEmailsYes";
            this.rdoNotifEmailsYes.Size = new System.Drawing.Size(170, 20);
            this.rdoNotifEmailsYes.TabIndex = 0;
            this.rdoNotifEmailsYes.TabStop = true;
            this.rdoNotifEmailsYes.Tag = "5";
            this.rdoNotifEmailsYes.Text = "Yes, notify me via e-mail";
            this.rdoNotifEmailsYes.UseVisualStyleBackColor = true;
            this.rdoNotifEmailsYes.CheckedChanged += new System.EventHandler(this.CheckedChanged_NotificationEmails);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.AutoSize = true;
            this.groupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox3.Controls.Add(this.lblNoteMattimonAgent);
            this.groupBox3.Controls.Add(this.rdoMattiAgentNo);
            this.groupBox3.Controls.Add(this.rdoMattiAgentYes);
            this.groupBox3.Location = new System.Drawing.Point(438, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(318, 140);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Feedback from Mattimon Agent";
            // 
            // lblNoteMattimonAgent
            // 
            this.lblNoteMattimonAgent.AutoSize = true;
            this.lblNoteMattimonAgent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoteMattimonAgent.ForeColor = System.Drawing.Color.Gray;
            this.lblNoteMattimonAgent.Location = new System.Drawing.Point(23, 76);
            this.lblNoteMattimonAgent.Name = "lblNoteMattimonAgent";
            this.lblNoteMattimonAgent.Size = new System.Drawing.Size(283, 32);
            this.lblNoteMattimonAgent.TabIndex = 2;
            this.lblNoteMattimonAgent.Text = "Mattmon Agent won\'t longer provide WMI data.\r\n(Expiremental)";
            // 
            // rdoMattiAgentNo
            // 
            this.rdoMattiAgentNo.AutoSize = true;
            this.rdoMattiAgentNo.Location = new System.Drawing.Point(6, 51);
            this.rdoMattiAgentNo.Name = "rdoMattiAgentNo";
            this.rdoMattiAgentNo.Size = new System.Drawing.Size(306, 20);
            this.rdoMattiAgentNo.TabIndex = 1;
            this.rdoMattiAgentNo.Tag = "10";
            this.rdoMattiAgentNo.Text = "No, feedback from the default Mattimon Service";
            this.rdoMattiAgentNo.UseVisualStyleBackColor = true;
            this.rdoMattiAgentNo.CheckedChanged += new System.EventHandler(this.CheckedChanged_MattimonAgent);
            // 
            // rdoMattiAgentYes
            // 
            this.rdoMattiAgentYes.AutoSize = true;
            this.rdoMattiAgentYes.Checked = true;
            this.rdoMattiAgentYes.Location = new System.Drawing.Point(6, 25);
            this.rdoMattiAgentYes.Name = "rdoMattiAgentYes";
            this.rdoMattiAgentYes.Size = new System.Drawing.Size(237, 20);
            this.rdoMattiAgentYes.TabIndex = 0;
            this.rdoMattiAgentYes.TabStop = true;
            this.rdoMattiAgentYes.Tag = "5";
            this.rdoMattiAgentYes.Text = "Yes, feedback from Mattimon Agent";
            this.rdoMattiAgentYes.UseVisualStyleBackColor = true;
            this.rdoMattiAgentYes.CheckedChanged += new System.EventHandler(this.CheckedChanged_MattimonAgent);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chkMonitorSQL);
            this.groupBox4.Location = new System.Drawing.Point(3, 149);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(318, 43);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Other Monitoring Services";
            // 
            // chkMonitorSQL
            // 
            this.chkMonitorSQL.AutoSize = true;
            this.chkMonitorSQL.Location = new System.Drawing.Point(3, 18);
            this.chkMonitorSQL.Name = "chkMonitorSQL";
            this.chkMonitorSQL.Size = new System.Drawing.Size(143, 20);
            this.chkMonitorSQL.TabIndex = 0;
            this.chkMonitorSQL.Text = "Monitor SQL Server";
            this.chkMonitorSQL.UseVisualStyleBackColor = true;
            // 
            // flHeaderLabels
            // 
            this.flHeaderLabels.AutoSize = true;
            this.flHeaderLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flHeaderLabels.Controls.Add(this.flowLayoutPanel4);
            this.flHeaderLabels.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flHeaderLabels.Location = new System.Drawing.Point(496, 3);
            this.flHeaderLabels.Name = "flHeaderLabels";
            this.flHeaderLabels.Size = new System.Drawing.Size(130, 32);
            this.flHeaderLabels.TabIndex = 11;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.flowLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel4.Controls.Add(this.lblCompany);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(6, 8);
            this.flowLayoutPanel4.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(118, 16);
            this.flowLayoutPanel4.TabIndex = 5;
            // 
            // lblCompany
            // 
            this.lblCompany.AutoSize = true;
            this.lblCompany.Location = new System.Drawing.Point(6, 0);
            this.lblCompany.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblCompany.Name = "lblCompany";
            this.lblCompany.Size = new System.Drawing.Size(106, 16);
            this.lblCompany.TabIndex = 3;
            this.lblCompany.Text = "Company Name";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(0, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(172, 36);
            this.panel1.TabIndex = 9;
            // 
            // flHeaderContainer
            // 
            this.flHeaderContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flHeaderContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flHeaderContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flHeaderContainer.Controls.Add(this.flHeaderLabels);
            this.flHeaderContainer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flHeaderContainer.Location = new System.Drawing.Point(172, 3);
            this.flHeaderContainer.Name = "flHeaderContainer";
            this.flHeaderContainer.Size = new System.Drawing.Size(631, 36);
            this.flHeaderContainer.TabIndex = 10;
            // 
            // panelHeaderContainer
            // 
            this.panelHeaderContainer.AutoSize = true;
            this.panelHeaderContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelHeaderContainer.Controls.Add(this.flHeaderContainer);
            this.panelHeaderContainer.Controls.Add(this.panel1);
            this.panelHeaderContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeaderContainer.Location = new System.Drawing.Point(0, 0);
            this.panelHeaderContainer.Name = "panelHeaderContainer";
            this.panelHeaderContainer.Size = new System.Drawing.Size(803, 42);
            this.panelHeaderContainer.TabIndex = 11;
            // 
            // MattimonAgentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 394);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.panelHeaderContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(451, 300);
            this.Name = "MattimonAgentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.This_FormClosing);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.grpboxSQLSrv.ResumeLayout(false);
            this.grpBoxPorts.ResumeLayout(false);
            this.grpBoxPorts.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.flHeaderLabels.ResumeLayout(false);
            this.flHeaderLabels.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flHeaderContainer.ResumeLayout(false);
            this.flHeaderContainer.PerformLayout();
            this.panelHeaderContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.FlowLayoutPanel flMattiServicesInfo;
        private System.Windows.Forms.Button btnRestartSvc;
        private System.Windows.Forms.Button btnStartSvc;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdo15mins;
        private System.Windows.Forms.RadioButton rdo10mins;
        private System.Windows.Forms.RadioButton rdo5mins;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rdoNotifEmailsNo;
        private System.Windows.Forms.RadioButton rdoNotifEmailsYes;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblNoteMattimonAgent;
        private System.Windows.Forms.RadioButton rdoMattiAgentNo;
        private System.Windows.Forms.RadioButton rdoMattiAgentYes;
        private System.Windows.Forms.FlowLayoutPanel flHeaderLabels;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label lblCompany;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flHeaderContainer;
        private System.Windows.Forms.Panel panelHeaderContainer;
        private System.Windows.Forms.Label lblPortName;
        private System.Windows.Forms.ComboBox cboAvailablePorts;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.RadioButton rdo1min;
        private System.Windows.Forms.Label lblUpdVersion;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkMonitorSQL;
        private System.Windows.Forms.GroupBox grpboxSQLSrv;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.GroupBox grpBoxPorts;
        private System.Windows.Forms.Button btnNewSqlInstance;
    }
}

