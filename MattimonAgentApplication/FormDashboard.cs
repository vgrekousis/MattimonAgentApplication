using MattimonAgentApplication.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using MattimonAgentLibrary.Models;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentLibrary.Models.SQL.Tools;
using System.IO;

namespace MattimonAgentApplication
{
    public partial class FormDashboard : Form, IMessageFilter
    {
        /// <summary>
        /// 
        /// </summary>
        internal MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes ApplicationProjectAssemblyAtrributes;
        /// <summary>
        /// 
        /// </summary>
        private FormBackgroundWorker FormBackgroundWorker;
        /// <summary>
        /// 
        /// </summary>
        private bool initialBalloonTipShown;
        /// <summary>
        ///
        /// </summary>
        private bool shouldExit;
        /// <summary>
        /// 
        /// </summary>
        public FormDashboard()
        {
            //MessageBox.Show(MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName("Mattimon Agent"));

            Visible = false;
            ///
            ///
            ///
            Load += (s, e) => { if (shouldExit) { if (Visible) Close(); Application.Exit(); } };
            //
            //
            //
            try
            {
                ///
                /// Add a Message filder to handle Win32 Messages
                ///
                Application.AddMessageFilter(this);
                ///
                /// Assign 'MattimonAgentApplication' Assembly Attributes
                ///
                ApplicationProjectAssemblyAtrributes = 
                    new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Application.ExecutablePath);
                ///
                /// Designer call
                ///
                InitializeComponent();
                ///
                /// Fill Reporting Interval Comobox (No Index selected yet)
                ///
                InitializeReportingIntervalComboBox();
                ///
                /// Extra event handlers that needed to be assigned out of the designer context.
                /// Rather using this method because .NET designer is being retarded when I 
                /// add event handlers located in another partial class. 
                ///
                AssignExtraEventHandlers();
                ///
                /// Refresh the Mattimon Services Gata Grid every X milliseconds
                ///
                InitializeServiceCheckTimer(1000);
                ///
                /// Initialize the background worker that will be used for refreshing the UI from the server
                ///
                InitializeMattimonWebDataRequestWorker();
                ///
                /// Initialize the background worker that will be used for updating data on the server
                ///
                InitializeMattimonWebDataUpdateWorker();
                ///
                /// Initialize the background worker that will be used for requesting DeviceServerObjects
                ///
                InitializeMattimonSqlServerDataRequestWorker();
                ///
                /// Initialize the background worker that will be used for posting DeviceServer Objects
                ///
                InitializeMattimonSqlServerPostWorker();
                ///
                /// Initialize the Notify Icon (Notifications on the desktop)
                ///
                InitializeNotifyIcon();
                ///
                /// Make sure the forms header always on top
                ///
                pTop.BringToFront();
                ///
                /// Assign Bitscore Icon
                ///
                Icon = MattimonAgentApplication.Properties.Resources.bitscore_icon_64;
                ///
                /// Set DoubleBuffered to true to succesfully redraw form components in case of form resize
                ///
                DoubleBuffered = true;
                ///
                /// Set Control Styles
                ///
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldExit = true;

            }
        }

        #region Override Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ///
            /// btnMaximize
            ///
            btnMaximize.Hide();
            ///
            /// pCompanyImg
            ///
            pCompanyImg.BackgroundImageLayout = ImageLayout.Center;
            pCompanyImg.BackgroundImage = Icon.ToBitmap().ResizeImage(25, 25);
            ///
            /// pForeImg
            ///
            pForeImg.BackgroundImageLayout = ImageLayout.Center;
            pForeImg.BackgroundImage = MattimonAgentApplication.Properties.Resources.mattimon_banner_resize.ResizeImage(200, 35);
            pForeImg.Width = 200;
            ///
            /// fpControls
            ///
            fpControls.BringToFront();
            ///
            /// pImgCaption
            ///
            pImgCaption.Dock = DockStyle.Fill;
            ///
            /// lblTitle
            ///
            lblTitle.Location = new Point(lblTitle.Location.X, ((pImgCaption.Height - lblTitle.Height) / 2) + 1);
            lblTitle.Size = new Size(pImgCaption.Width - lblTitle.Location.X - fpControls.DisplayRectangle.Width, lblTitle.Height);
            ///
            /// Form Delta Event Handlers
            ///
            Resize += (s, r) => { bool ellipsis = lblTitle.PreferredWidth > lblTitle.Width;  /* handle caption label ellipsis if needed */ };
            ///
            /// tabControl1
            ///
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.Margin = new Padding(0);
            tabControl1.BringToFront();
            ///
            /// Controls that will handle form dragging (Requires Application.AddMessageFilter(this)) 
            ///
            ControlsToMove.Add(pForeImg);
            ControlsToMove.Add(pImgCaption);
            ControlsToMove.Add(lblTitle);
            ControlsToMove.Add(pTop);
            ControlsToMove.Add(statusStrip);
            ///
            /// Initialize default Label text here
            ///
            lblTitle.Text = MattimonFramework.Core.ApplicationManager.GetApplicationTitle() + " - " + MattimonFramework.Core.ApplicationManager.GetComputerName();
            lbl_about_cversion.Text = ApplicationProjectAssemblyAtrributes.GetAssemblyVersion().ToString();
            lbl_about_cdeviceid.Text = GetDeviceID().ToString();
            ///
            /// Apply extra layout fixes here if needed
            ///
            ucMattimonServicesGrid1.Width = pPageTitleHolder.Width;
            ucsqlServerInstanceGrid1.Width = pSqlSrvTitleHolder.Width;
            ///
            /// Next
            ///
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            Opacity = 0.5;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Opacity = 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(
                e.Graphics, ClientRectangle,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 0, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 0, ButtonBorderStyle.Solid
                );
            base.OnPaint(e);
        }
        #endregion

        #region Primary Data Request
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormDashboard_Load(object sender, EventArgs e)
        {
            FormBackgroundWorker = new FormBackgroundWorker(this, DoWork, "Gathering user settings")
            {
                RunWorkerCompletedEventHandler = DoWorkCompleted
            };
            FormBackgroundWorker.DoWork();


            MattimonSqlServerDataRequestWorker.RunWorkerAsync(mFromServerDeviceServerObjects);
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            object[] result = new object[3];
            result[0] = RequestAuthentication();
            result[1] = RequestDeviceOptions();
            result[2] = RequestVersion();
            e.Result = result;
        }

        private void DoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this,e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                Application.Exit();
                return;
            }

            UserAuthentication ua = (UserAuthentication)((object[])e.Result)[0];
            DeviceOptions options = (DeviceOptions)((object[])e.Result)[1];
            string version = ((object[])e.Result)[2] as string;
            UpdateGUIUserSettings(ua, options, version);
            Show();
        }
        #endregion

        #region GUI Data Update
        private void UpdateGUIUserSettings(UserAuthentication ua, DeviceOptions options, String version)
        {
            // Aside Menu
            btnMenu_SqlSrv.Visible = options.MonitorSql;
            // End

            // About page
            lbl_about_ccompany.Text = ua.Company_Name;
            lbl_about_cemail.Text = ua.User_email;
            lbl_about_ccurrversion.Text = version;
            // End

            // Settings page
            chk_settings_enableEmail.Checked = options.NotificationEmails;
            chk_settings_enabeEvtLogMonitoring.Checked = options.MonitorEventLog > 0;
            chk_settings_enableSQLFeature.Checked = options.MonitorSql;
            chk_settings_notifyStatus.Checked = options.NotifyStatus;
            chk_settings_notifyHealth.Checked = options.NotifyHealth;
            cbo_settings_reportInterval.SelectedItem = (int)MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToMinutes(DeviceOptions.ReportingInterval);
            btn_settings_refreshSettings.Enabled = true;
            
            // End

            // Status Strip
            lblStatus.Text = "Ready";
            // End

            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="version"></param>
        private void UpdateGUIUserSettings(DeviceOptions options, String version)
        {
            // Aside Menu
            btnMenu_SqlSrv.Visible = options.MonitorSql;
            // End

            // About page
            lbl_about_ccurrversion.Text = version;
            // End

            // Settings page
            chk_settings_enableEmail.Checked = options.NotificationEmails;
            chk_settings_enabeEvtLogMonitoring.Checked = options.MonitorEventLog > 0;
            chk_settings_enableSQLFeature.Checked = options.MonitorSql;
            chk_settings_notifyStatus.Checked = options.NotifyStatus;
            chk_settings_notifyHealth.Checked = options.NotifyHealth;
            cbo_settings_reportInterval.SelectedItem = (int)MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToMinutes(DeviceOptions.ReportingInterval);
            btn_settings_refreshSettings.Enabled = true;
            // End

            // Status Strip
            lblStatus.Text = "Ready";
            // End
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="version"></param>
        private void UpdateGUIUserSettings(DeviceOptions options)
        {
            // Aside Menu
            btnMenu_SqlSrv.Visible = options.MonitorSql;
            // End

            // Settings page
            chk_settings_enableEmail.Checked = options.NotificationEmails;
            chk_settings_enabeEvtLogMonitoring.Checked = options.MonitorEventLog > 0;
            chk_settings_enableSQLFeature.Checked = options.MonitorSql;
            chk_settings_notifyStatus.Checked = options.NotifyStatus;
            chk_settings_notifyHealth.Checked = options.NotifyHealth;
            cbo_settings_reportInterval.SelectedItem = (int)MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToMinutes(DeviceOptions.ReportingInterval);
            btn_settings_refreshSettings.Enabled = true;
            // End

            // Status Strip
            lblStatus.Text = "Ready";
            // End
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ua"></param>
        private void UpdateGUIUserSettings(UserAuthentication ua)
        {
            // About page
            lbl_about_ccompany.Text = ua.Company_Name;
            lbl_about_cemail.Text = ua.User_email;
            // End

            // Status Strip
            lblStatus.Text = "Ready";
            // End
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverObjects"></param>
        private void UpdateGUISQLServer(DeviceServerObjects serverObjects, DataGridView gridView, bool local)
        {
            lblStatus.Text = "Ready";
            bool hasInstances = serverObjects.Instances != null && serverObjects.Instances.Length > 0;
            ucsqlServerInstanceGrid1.Visible = hasInstances;
            btn_sqlsrv_refresh.Visible = hasInstances;
            btn_sqlsrv_deleteInstances.Visible = hasInstances;
            btn_sqlsrv_refresh.Enabled = true;
            
            if (hasInstances)
            {
                int dbcount = 0;
                int dccount = 0;
                foreach (ServerInstance si in serverObjects.Instances)
                {
                    dbcount += si.Databases.Length;
                    foreach (Database db in si.Databases)
                        dccount += db.Connections.Length;
                }
                if (dbcount > 0) lbl_sqlsrv_hint.Text += "\tDatabases: " + dbcount;

                lbl_sqlsrv_hint.Text = string.Format("Instances: {0} - Databases: {1} - Connections: {2}", serverObjects.Instances.Length, dbcount, dccount);
            }
            else
            {
                lbl_sqlsrv_hint.Text = "You're not monitoring any SQL Server instances. Click\"Add New\" to get started!";
            }
            try
            {
                DataSet serverObjectsDataSet = DataHelper.CreateDataSet(serverObjects, local);
                ucsqlServerInstanceGrid1.SetDataSet(serverObjectsDataSet, true);
            }
            catch (Exception ex)
            {
                MenuButtonClick(btnMenu_SqlSrv, EventArgs.Empty);
                GUI.BitscoreForms.BitscoreMessageBox.Show
                    (this, "An error occurred while attempting to parse SQL Server Data.\n\nError details:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        #endregion

        #region Private methods
        /// <summary>
        ///
        /// </summary>
        private void AssignExtraEventHandlers()
        {
            ///
            /// Located in FormDashboard.SQLServer
            ///
            btn_sqlsrv_newServer.Click += BtnSqlSrv_AddNew_Click;
            btn_sqlsrv_refresh.Click += BtnSqlSrv_Refresh_Click;
            btn_sqlsrv_deleteInstances.Click += BtnSqlSrv_DeleteInstances_Click;
            ucsqlServerInstanceGrid1.DeleteInstanceClick += InstancesGrid_DeleteInstance_Click;
            ///
            /// Located in FormDashboard.ServiceHanding
            ///
            this.ucMattimonServicesGrid1.ServiceStartChanged += new MattimonAgentApplication.GUI.Events.ServiceStartChangedEventHandler(this.ServicesDataGrid1_ServiceStartChanged);
            this.ucMattimonServicesGrid1.ServiceStateChanged += new MattimonAgentApplication.GUI.Events.ServiceStateChangedEventHandler(this.ServicesDataGrid1_ServiceStateChanged);
            ///
            /// Located in FormDashboard.WebRequests
            ///
            btn_settings_postSettings.Click += Btn_settings_postSettings_Click;
            btn_settings_refreshSettings.Click += Btn_settings_refreshSettings_Click;
        }

        private void InitializeReportingIntervalComboBox()
        {
            int[] minutes = new int[] { 1, 5, 10, 15 };
            foreach (int m in minutes)
                cbo_settings_reportInterval.Items.Add(m);
        }

        

        /// <summary>
        /// 
        /// </summary>
        private void InitializeNotifyIcon()
        {
            notifyIcon1.Icon = MattimonAgentApplication.Properties.Resources.bitscore_icon_64;
            notifyIcon1.Text = Text;
            notifyIcon1.ContextMenu = CreateNotifyIconContextMenu();
            notifyIcon1.MouseDoubleClick += (s, e) =>
            {
                Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateNotifyIconContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Show"));
            menu.MenuItems[0].Click += (s, e) =>
            {
                Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            };
            menu.MenuItems.Add(new MenuItem("Exit"));
            menu.MenuItems[1].Click += (s, e) =>
            {
                Application.Exit();
            };
            return menu;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="timeout"></param>
        private void ShowBalloonTip(String text, String title, int timeout = 2500)
        {
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.ShowBalloonTip(timeout);
        }
        #endregion

        #region GUI Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            Hide();
            notifyIcon1.Visible = true;
            if (!initialBalloonTipShown) ShowBalloonTip(Text + " minimized to tray.", Text);
            initialBalloonTipShown = true;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
            Application.Exit();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="e"></param>
        private void MenuButtonClick(object button, EventArgs e)
        {
            if (button == btnMenu_Dashboard)
                tabControl1.SelectedTab = tabPage0;
            if (button == btnMenu_WebService)
                tabControl1.SelectedTab = tabPage2;
            if (button == btnMenu_MonitPorts)
                tabControl1.SelectedTab = tabPage3;
            if (button == btnMenu_SqlSrv)
                tabControl1.SelectedTab = tabPage4;
            if (button == btnMenu_SystemNotifications)
                tabControl1.SelectedTab = tabPage5;
            if (button == btnMenu_Settings)
                tabControl1.SelectedTab = tabPage6;
            if (button == btnMenu_About)
                tabControl1.SelectedTab = tabPage7;
        }
        private void FormDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MattimonServiceCheckTimer != null)
                try { MattimonServiceCheckTimer.Stop(); }
                catch { }

            if (MattimonWebDataRequestBackgroundWorker.IsBusy || MattimonWebDataUpdateBackgroundWorker.IsBusy 
                || MattimonSqlServerDataRequestWorker.IsBusy || MattimonSqlServerPostDataWorker.IsBusy)
            {
                e.Cancel = true;
                lblStatus.Text = "Waiting for background tasks to complete...";
                new Thread(() =>
                {
                    while (MattimonWebDataRequestBackgroundWorker.IsBusy) ;
                    while (MattimonWebDataUpdateBackgroundWorker.IsBusy) ;
                    while (MattimonSqlServerDataRequestWorker.IsBusy) ;
                    while (MattimonSqlServerPostDataWorker.IsBusy) ;
                    Invoke((MethodInvoker)delegate () { Close(); });
                }).Start();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAboutClientInfoHide_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (pAboutClientInfoHolder.Height != 0)
            {
                b.Text = "Show";
                pAboutClientInfoHolder.AutoSize = false;
                pAboutClientInfoHolder.Height = 0;
            }

            else
            {
                pAboutClientInfoHolder.AutoSize = true;
                b.Text = "Hide";
            }
        }
        #endregion
    }
}
