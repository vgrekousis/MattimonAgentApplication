using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary.Tools;
using MattimonAgentLibrary.Rest;
using MattimonAgentLibrary.Static;
using MattimonAgentLibrary.Models;
using System.Drawing;
using System.Net.Http;
using System.ComponentModel;
using MattimonAgentLibrary.WMI;

namespace MattimonAgentApplication
{
    public partial class MattimonAgentForm : Form
    {
        private long
            fetchedDeviceId,
            fetchedCompanyId,
            fetchedUserId;

        private double
            fetchedIntervals;

        private bool 
            fetchedSQLMonitoring;

        private string
            srvFetchedCompanyName,
            srvFetchedUserEmail;

        private DeviceOptions
            srvFetchedDeviceOptions;

        private double 
            selectedIntervalRadioButton;
        
        private FormBackgroundWorker 
            worker;

        private bool
            loop = true;

        public MattimonAgentForm()
        {
            InitializeComponent();
            DefineVersionLabelText(GetMattimonUpdateServiceStatus());
            DefineUpdVersionLabelText();

            Text = new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                Application.ExecutablePath).AssemblyTitle;

            Icon = MattimonAgentApplication.Properties.Resources.MattimonIcon;

            BackgroundWorkerFetchAll();
            ThreadLoop();

        }

        private void DefineVersionLabelText(MattimonAgentLibrary.Tools.MyServiceController.ServiceState svcState)
        {
            try
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblVersion, "Text",
                    "App version: " + new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(

                    Application.ExecutablePath).GetAssemblyVersion().ToString() +
                     String.Format(" (Automatic update service {0}",
                     svcState == MyServiceController.ServiceState.NotFound ? "unavailable" :
                     (svcState == MyServiceController.ServiceState.Running ? "is running" : "is not running - Status: " + svcState)) + ")"
                     );
            }
            catch
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblVersion, "Text", "Assembly not found!");
            }
        }
        private void DefineUpdVersionLabelText() //(MattimonAgentLibrary.Tools.MyServiceController.ServiceState svcState)
        {
            try
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblUpdVersion, "Text",

                   "Updater version: " + new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(

                        System.IO.Path.Combine(
                            System.IO.Directory.GetCurrentDirectory(), "MattimonUpdateService.exe")).
                            GetAssemblyVersion().ToString());
            }
            catch
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblUpdVersion, "Text", "Assembly not found!");
            }
        }
        private String GetMattimonAgentServiceAssemblyTitle()
        {
            String installDirectory = MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName);

            String svcExe = System.IO.Path.Combine(installDirectory, "MattimonAgentService.exe");
            return new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(svcExe).AssemblyTitle;
        }
        private String GetMattimonUpdateServiceAssemblyTitle()
        {
            String installDirectory = MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName);

            String svcExe = System.IO.Path.Combine(installDirectory, "MattimonUpdateService.exe");
            return new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(svcExe).AssemblyTitle;
        }
        private void StartMattimonAgentService()
        {
            String svcName = GetMattimonAgentServiceAssemblyTitle();
            String updSvcName = GetMattimonUpdateServiceAssemblyTitle();

            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) == MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StartService(updSvcName);

            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StartService(svcName);
        }
        private void StopMattimonAgentService()
        {

            String svcName = GetMattimonAgentServiceAssemblyTitle();
            String updSvcName = GetMattimonUpdateServiceAssemblyTitle();

            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) != MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StopService(updSvcName);

            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StopService(svcName);
        }
        private MattimonAgentLibrary.Tools.MyServiceController.ServiceState GetMattimonAgentServiceStatus()
        {
            String svcName = GetMattimonAgentServiceAssemblyTitle();
            return MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName);
        }

        private void StartMattimonUpdateService()
        {
            String svcName = GetMattimonUpdateServiceAssemblyTitle();
            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StartService(svcName);
        }
        private void StopMattimonUpdateService()
        {
            String svcName = GetMattimonUpdateServiceAssemblyTitle();
            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
                MattimonAgentLibrary.Tools.MyServiceController.StopService(svcName);
        }

        private MattimonAgentLibrary.Tools.MyServiceController.ServiceState GetMattimonUpdateServiceStatus()
        {
            String svcName = GetMattimonUpdateServiceAssemblyTitle();
            return MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName);
        }

        private Timer refreshFormTimer;
        private void DefineForm()
        {
            String lblVersionText = lblVersion.Text;
            DefineVersionLabelText(GetMattimonUpdateServiceStatus());

            if (GetMattimonAgentServiceStatus() == MyServiceController.ServiceState.NotFound)
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Enabled", false);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Text", "Not installed");

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Enabled", false);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Text", "Not installed");
            }

            if (GetMattimonAgentServiceStatus() == MyServiceController.ServiceState.Running)
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                   (btnStartSvc, "Enabled", true);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Text", "Stop Service");

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Tag", false);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Enabled", true);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Text", "Restart");
            }
            if (GetMattimonAgentServiceStatus() == MyServiceController.ServiceState.Stopped)
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                  (btnStartSvc, "Enabled", true);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Text", "Start Service");

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnStartSvc, "Tag", true);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Enabled", false);

                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe
                    (btnRestartSvc, "Text", "Restart");
            }
        }
        private void Tick(Object sender, EventArgs e)
        {
            DefineForm();
        }
        private void Loop()
        {
            while (loop)
            {
                DefineForm();
            }
        }
        private void ThreadLoop()
        {
            DefineForm();
            refreshFormTimer = new Timer
            {
                Interval = 1200
            };
            refreshFormTimer.Tick += Tick;
            refreshFormTimer.Start();
            Console.WriteLine("Form refresher: " + (refreshFormTimer.Enabled ? "enabled" : "disabled"));
            //new System.Threading.Thread(Loop).Start();
        }
        private void BtnStartSvc_Click(object sender, EventArgs e)
        {
            try
            {
                using (new WaitCursor())
                {
                    Boolean shouldStart = Convert.ToBoolean(((Button)sender).Tag);
                    if (shouldStart)
                        StartMattimonAgentService();
                    else
                        StopMattimonAgentService();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        private void BtnRestartSvc_Click(object sender, EventArgs e)
        {
            try
            {
                using (new WaitCursor())
                {
                    StopMattimonAgentService();
                    StartMattimonAgentService();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        private void This_FormClosing(object sender, FormClosingEventArgs e)
        {
            /// Setting loop back to false prevents the Loop()'s loop to remain active on application exit.
            this.loop = false;

            if (refreshFormTimer != null && refreshFormTimer.Enabled)
            {
                refreshFormTimer.Stop();
                refreshFormTimer.Dispose();
            }
        }
        private void BtnApply_Click(object sender, EventArgs e)
        {
            WorkUpdateDeviceOptions();
        }
        private void CheckedChanged_Interval(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            this.selectedIntervalRadioButton = TimeSpan.FromMinutes(Convert.ToInt32(rb.Tag)).TotalMilliseconds;
            AutoEnableApplyButton();
        }
        private void CheckedChanged_SQLMonitoring(object sender, EventArgs e)
        {
            AutoEnableApplyButton();
        }

        private void CboAvailablePorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            Boolean none = cboAvailablePorts.SelectedIndex == 0;

            if (!none)
            {
                using (new WaitCursor())
                {
                    foreach (ProcessPort p in ProcessPorts.ProcessPortMap.FindAll(
                        x => x.PortNumber == Convert.ToInt32(cboAvailablePorts.Items[cboAvailablePorts.SelectedIndex])))
                    {
                        MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(
                            lblPortName, "Text", p.ProcessPortDescription);
                    }
                }
            }
            else
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblPortName, "Text", "");
        }
        
        private void CboAvailablePorts_SelectedValueChanged(object sender, EventArgs e)
        {
            AutoEnableApplyButton();
        }
        private void CheckedChanged_NotificationEmails(object sender, EventArgs e)
        {
            Boolean useAgent = rdoMattiAgentYes.Checked;
            Boolean notifEmails = rdoNotifEmailsYes.Checked;
            AutoEnableApplyButton();
        }
        private void CheckedChanged_MattimonAgent(object sender, EventArgs e)
        {
            Boolean useAgent = rdoMattiAgentYes.Checked;
            Boolean notifEmails = rdoNotifEmailsYes.Checked;
            AutoEnableApplyButton();
        }
        private void AutoEnableApplyButton()
        {
            try
            {
                Boolean useAgent = rdoMattiAgentYes.Checked;
                Boolean notifEmails = rdoNotifEmailsYes.Checked;
                int selectionport = Convert.ToString(cboAvailablePorts.SelectedItem).Equals("None") ? 0 : Convert.ToInt32(cboAvailablePorts.SelectedItem);
                Boolean sqlmonitEnabled = fetchedSQLMonitoring;
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(btnApply, "Enabled",
                    useAgent != srvFetchedDeviceOptions.UseAgent ||
                    notifEmails != srvFetchedDeviceOptions.NotificationEmails ||
                    selectedIntervalRadioButton != fetchedIntervals ||
                    chkMonitorSQL.Checked != sqlmonitEnabled ||
                    selectionport != srvFetchedDeviceOptions.MonitoringPort);
            }
            catch (Exception ex)
            {
                worker.CancelAsyncClose();
                MessageBox.Show(this, ex.ToString());
            }
        }
        private void SetReportingIntervalRadioButton(double value)
        {
            rdo5mins.Checked = TimeSpan.FromMilliseconds(value).TotalMinutes == 5;
            rdo10mins.Checked = TimeSpan.FromMilliseconds(value).TotalMinutes == 10;
            rdo15mins.Checked = TimeSpan.FromMilliseconds(value).TotalMinutes == 15;

            if (rdo5mins.Checked)
                CheckedChanged_Interval(rdo5mins, EventArgs.Empty);
            if (rdo10mins.Checked)
                CheckedChanged_Interval(rdo5mins, EventArgs.Empty);
            if (rdo15mins.Checked)
                CheckedChanged_Interval(rdo5mins, EventArgs.Empty);

            AutoEnableApplyButton();
        }
        private void InitializeMonitoringPorts()
        {
            cboAvailablePorts.Items.Add("None");
            foreach (ProcessPort p in ProcessPorts.ProcessPortMap)
            {
                cboAvailablePorts.Items.Add(p.PortNumber);
            }
        }


        private void WorkUpdateDeviceOptions()
        {
            worker = new FormBackgroundWorker(this, DoWorkUpdateDeviceOptions, "Posting options");
            worker.DoWork();
        }
        private bool debug_DoWorkUpdateDeviceOptions = true;
        private void DoWorkUpdateDeviceOptions(object sender, DoWorkEventArgs e)
        {
            if (debug_DoWorkUpdateDeviceOptions)
            {
                worker.CancelAsyncClose();
                MessageBox.Show("Will post device options for:\n" +
                    "device_id:     " + fetchedDeviceId + "\n" +
                    "user_id:       " + fetchedUserId + "\n" +
                    "company_id:    " + fetchedCompanyId
                    ); return;
            }



            DeviceOptions tmp = null;
            DeviceRequests deviceRequests = new DeviceRequests();
            using (new WaitCursor())
            {
                int port = cboAvailablePorts.SelectedIndex == 0 ? 0 : Convert.ToInt32(cboAvailablePorts.SelectedItem);

                tmp = deviceRequests.PostDeviceOptions(
                       new Device
                       {
                           /* keys */
                           Device_Id = fetchedDeviceId,
                           User_Id = fetchedUserId,
                           Company_Id = fetchedCompanyId,
                           /* end keys */

                           /* options */
                           Port = port,
                           UseAgent = rdoMattiAgentYes.Checked,
                           MonitorSql = rdoMattiAgentNo.Checked ? 0 : ( chkMonitorSQL.Checked ? 1 : 0),
                           AgentReportInterval = selectedIntervalRadioButton,
                           NotificationEmails = rdoNotifEmailsYes.Checked
                           /* end options */
                       }
                       );

            }
            worker.CancelAsyncClose();

            if (tmp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(this, "We couldn't proccess your request at this time.\nThe application needs to close.\nPlease, re-run the program and try again.",
                    "Error (" + (int)tmp.HttpStatusCode + ")",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            if (tmp.MySqlExceptionMessage != null)
            {
                MessageBox.Show(this, "We couldn't proccess your request at this time. " +
                    "Please, let us know about this error.\n\n" + tmp.MySqlExceptionMessage,
                    "Internal Server Error (MySQL)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (tmp.Exception != null)
            {
                MessageBox.Show(this, "We couldn't proccess your request at this time. " +
                    "Please, let us know about this error.\n\n" + tmp.Exception.Message + "\n\n" +
                    tmp.Exception.StackTrace, "Error (" + tmp.Exception.Source + ")",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (tmp.RequestSuccess)
            {
                if (!tmp.UseAgent)
                {
                    using (new WaitCursor())
                    {
                        // We stop the service if settings are applied succesfully 
                        // and the user selected "default mattimon service" 
                        // (if the service is already running).
                        // Note: btnStopResume and btnRestart are enabled or disabled accordingly in RunThread()
                        StopMattimonAgentService();
                    }
                }
                else
                {
                    // Starts only if its stopped
                    StartMattimonAgentService();
                }

                // Show or hide sql monitor group box
                grpboxSQLSrv.Visible = tmp.MonitorSql;
                

                ///MessageBox.Show("Selected reporting interval is now " + selectedIntervalRadioButton);
                ///
                // Save the interval in the local database
                this.GetLocalDatabase().SetReportingInterval(selectedIntervalRadioButton);

                // Assign the new options to the fetchedDeviceOptions
                srvFetchedDeviceOptions = tmp;
                fetchedIntervals = srvFetchedDeviceOptions.ReportingInterval;
                AutoEnableApplyButton();
                MessageBox.Show(this, "Your options have been received and applied", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #region Work Fetch
        private void BackgroundWorkerFetchAll()
        {
            worker = new FormBackgroundWorker(this, DoWorkFetchDeviceOptions, "Fetching");
            worker.DoWork();
        }
        private void DoWorkFetchDeviceOptions(object sender, DoWorkEventArgs e)
        {
            try
            {
                FetchKeys();
                FetchIntervals();
                FetchDeviceOptions();
                FetchUserInfos();

                SetReportingIntervalRadioButton(fetchedIntervals);
                
                // Show or hide the sql server group box
                grpboxSQLSrv.Visible = srvFetchedDeviceOptions.MonitorSql;
                InitializeMonitoringPorts();
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(
                    cboAvailablePorts, "SelectedItem", srvFetchedDeviceOptions.MonitoringPort == 0 ? (object)"None" : (object)Convert.ToString(srvFetchedDeviceOptions.MonitoringPort));
                AutoEnableApplyButton();
                worker.CancelAsyncClose();
            }
            catch (Exception ex)
            {
                worker.CancelAsyncClose();
                MessageBox.Show(this, ex.Message + "\n\n" + ex.ToString(), 
                    "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Local Fetch Methods
        /// <summary>
        /// 
        /// </summary>
        private void FetchIntervals()
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            selectedIntervalRadioButton = db.GetReportingInterval();
            fetchedIntervals = db.GetReportingInterval();
        }
        
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        private void FetchKeys()
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            fetchedUserId = db.GetUserId();
            fetchedDeviceId = db.GetDeviceId();
            fetchedCompanyId = db.GetCompanyId();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MattimonSQLite.SQLiteClientDatabase GetLocalDatabase()
        {
            return new MattimonSQLite.SQLiteClientDatabase(
                MattimonAgentLibrary.Static.Constants.CommonAppData,
                new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Application.ExecutablePath).AssemblyCompany,
                new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Application.ExecutablePath).AssemblyProduct,
                MattimonAgentLibrary.Static.Constants.LocalDatabaseName);
        }
        #endregion

        #region Server Fetch Methods
        private void FetchDeviceOptions()
        {
            try
            {
                DeviceRequests requests = new DeviceRequests();
                DeviceOptions deviceOptions = requests.GetDeviceOptions(
                    String.Format("{0};{1};{2}", fetchedDeviceId, fetchedUserId, fetchedCompanyId));

                this.fetchedSQLMonitoring = deviceOptions.MonitorSql;

                if (deviceOptions.Exception != null)
                {
                    worker.CancelAsyncClose();
                    MessageBox.Show(this,deviceOptions.Exception.ToString(), "FetchDeviceOptions Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (deviceOptions.MySqlExceptionMessage != null)
                {
                    worker.CancelAsyncClose();
                    MessageBox.Show(deviceOptions.MySqlExceptionMessage, "Server Error (SQL)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (deviceOptions.HttpRequestException != null)
                {
                    worker.CancelAsyncClose();
                    MessageBox.Show(deviceOptions.MySqlExceptionMessage, "Http Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                srvFetchedDeviceOptions = deviceOptions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }
        }
        private void FetchUserInfos()
        {
            UserInfosRequest userInfosRequest = new UserInfosRequest();
            UserAuthentication ua = userInfosRequest.GetUserInfo(fetchedUserId);

            if (ua.Exception != null)
            {
                worker.CancelAsyncClose();
                MessageBox.Show(this, ua.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            if (ua.HttpRequestException != null)
            {
                worker.CancelAsyncClose();
                MessageBox.Show(this, ua.HttpRequestException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            if (ua.MySqlExceptionErrno > 0)
            {
                worker.CancelAsyncClose();
                MessageBox.Show(this, ua.HttpRequestException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            srvFetchedCompanyName = ua.Company_Name;
            srvFetchedUserEmail = ua.User_email;
            MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblCompany, "Text", srvFetchedCompanyName + "  (Device: #" + fetchedDeviceId + ")");

        }
        #endregion
    }
}
