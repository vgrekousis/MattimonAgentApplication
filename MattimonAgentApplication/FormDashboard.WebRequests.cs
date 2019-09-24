using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary.Models;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentLibrary.Rest;
using MattimonAgentLibrary.Tools;
using MattimonAgentLibrary.WMI;
using MattimonSQLite;
using RestSharp;

namespace MattimonAgentApplication
{
    partial class FormDashboard
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly bool mWriteLogToInstallDirectory = true;
        /// <summary>
        /// 
        /// </summary>
        private readonly bool mFromServerDeviceServerObjects = true;
        /// <summary>
        /// 
        /// </summary>
        private DeviceOptions DeviceOptions;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker MattimonWebDataRequestBackgroundWorker;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker MattimonWebDataUpdateBackgroundWorker;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker MattimonSqlServerDataRequestWorker;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker MattimonSqlServerPostDataWorker;
        /// <summary>
        /// 
        /// </summary>
        private bool mUpdateWorkerInit;
        /// <summary>
        /// 
        /// </summary>
        private bool mRequestWorkerInit;
        /// <summary>
        /// 
        /// </summary>
        private bool mSqlSrvRequestWrkInit;
        /// <summary>
        /// 
        /// </summary>
        private bool mSqlSrvPostWrkInit;
        /// <summary>
        /// 
        /// </summary>
        private volatile bool dvcopt_agentIntervalChanged;

        /// <summary>
        /// Refresh UI data from the web
        /// Initializes MattimonWebDataRequestBackgroundWorker and MattimonWebDataRequestTimer
        /// </summary>
        /// <param name="interval"></param>
        private void InitializeMattimonWebDataRequestWorker()
        {
            if (mRequestWorkerInit) return;
            ///
            /// MattimonWebDataRequestBackgroundWorker
            ///
            MattimonWebDataRequestBackgroundWorker = new BackgroundWorker();
            ///
            /// DoWork Handler
            ///
            MattimonWebDataRequestBackgroundWorker.DoWork += (s, e) => 
            {
                if (Program.MattimonWebApiReplies())
                {
                    object[] data = new object[2];
                    data[0] = RequestDeviceOptions();
                    data[1] = RequestVersion();
                    e.Result = data;
                }
            };
            ///
            /// RunWorkerCompleted Handler
            ///
            MattimonWebDataRequestBackgroundWorker.RunWorkerCompleted += (s, e) => 
            {
                if (e.Error != null)
                {
                    MessageBox.Show("An error occurred while attempted to refresh user data from the server." +
                        "\n\nError details:\n" + e.Error.Message + (e.Error.InnerException != null ? "\n\n" 
                        + e.Error.InnerException.Message : ""), 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    Application.Exit();
                }
                
                // Parse the requested data
                object[] data = e.Result as object[];
                DeviceOptions deviceOptions = data[0] as DeviceOptions;
                String version = data[1] as String;

                // Set data on the UI
                UpdateGUIUserSettings(deviceOptions, version);

            };

            mRequestWorkerInit = true;
        }
        /// <summary>
        /// Initializes MattimonWebDataUpdateBackgroundWorker that will be posting settings / data on the server
        /// </summary>
        private void InitializeMattimonWebDataUpdateWorker()
        {
            if (!mUpdateWorkerInit)
            {
                MattimonWebDataUpdateBackgroundWorker = new BackgroundWorker();
                MattimonWebDataUpdateBackgroundWorker.DoWork += (s, e) =>
                {
                    e.Result = UpdateDeviceOptions(this.DeviceOptions);
                };
                MattimonWebDataUpdateBackgroundWorker.RunWorkerCompleted += (s, e) =>
                {
                    btn_settings_postSettings.Enabled = true;

                    if (e.Error != null)
                    {
                        // reset the switch!
                        dvcopt_agentIntervalChanged = false;

                        GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                            "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                            e.Error.Message + "\n" + e.Error.StackTrace + 
                            (e.Error.InnerException != null ? "\n\nInner Exception:\n\n" + e.Error.InnerException.Message + "\n" + e.Error.InnerException.StackTrace:"")
                            , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }

                    if (e.Result != null && e.Result is DeviceOptions options)
                    {
                        if (options.HttpRequestException != null)
                        {

                            // reset the switch!
                            dvcopt_agentIntervalChanged = false;


                            //GUI.BitscoreForms.BitscoreMessageBox.Show
                            MessageBox.Show(this,
                            "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                            options.HttpRequestException.Message + "\n" + options.HttpRequestException.StackTrace +
                            (options.HttpRequestException.InnerException != null ? "\n\nInner Exception:\n\n" + options.HttpRequestException.InnerException.Message + "\n" + options.HttpRequestException.InnerException.StackTrace : "")
                            , "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                            return;
                        }
                        if (options.Exception != null)
                        {

                            // reset the switch!
                            dvcopt_agentIntervalChanged = false;

                            //GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                            MessageBox.Show(this,
                           "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                            options.Exception.Message + "\n" + options.Exception.StackTrace +
                            (options.Exception.InnerException != null ? "\n\nInner Exception:\n\n" + options.Exception.InnerException.Message + "\n" + options.Exception.InnerException.StackTrace : "")
                            , "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                            return;
                        }
                        if (options.MySqlExceptionMessage != null)
                        {
                            // reset the switch!
                            dvcopt_agentIntervalChanged = false;

                            GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                "An error occurred while attempting to post on the server due to a database error.\n\nError details:n\n" + options.MySqlExceptionMessage,
                                "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                            return;
                        }

                        // If the interval was changed, restart the Agent Service.
                        if (dvcopt_agentIntervalChanged)
                        {
                            try
                            {
                                MyServiceController.StopService("MattimonAgentService");
                                if (MyServiceController.GetServiceStatus("MattimonAgentService") != MyServiceController.ServiceState.Running)
                                    MyServiceController.StartService("MattimonAgentService");
                            }
                            catch (Exception svcRestartError)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, 
                                    ExceptionHelper.GetFormatedExceptionMessage(svcRestartError), 
                                    "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            // and reset the switch!
                            dvcopt_agentIntervalChanged = false;
                        }

                        // Assign global Device Options
                        this.DeviceOptions = options;

                        // Refresh the form to make sure the data were actually posted
                        Btn_settings_refreshSettings_Click(btn_settings_refreshSettings, EventArgs.Empty);

                        GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                            "Your settings have been succesfully posted on the server.", Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                };
                mUpdateWorkerInit = true;
            }
        }
        /// <summary>
        /// Initializes MattimonSqlServerDataRequestWorker that will be requesting DeviceServerObjects
        /// </summary>
        private void InitializeMattimonSqlServerDataRequestWorker()
        {
            if (mSqlSrvRequestWrkInit) return;

            MattimonSqlServerDataRequestWorker = new BackgroundWorker();
            MattimonSqlServerDataRequestWorker.DoWork += (s, e) =>
            {
                object[] result = new object[2];
                if (e.Argument is bool loadFromServer) {
                    if (loadFromServer)
                    {
                        // Load the selected instances that where stored on the server
                        result[0] = RequestDeviceServerObjects();
                    }
                    else
                    {
                        // Only provide the instances stored on server to retreive
                        // local sql monitoring data : that data may not be yet posted on the server
                        // this is allowing us to see current data that haven't yet been posted by the MattimonService
                        ServerInstance[] instances = LocalReadServerInstances();
                        if (instances != null)
                        {
                            result[0] = new SQLServerObjectProvider().GetDeviceServerObjects(
                                    instances, GetDeviceID(), out Exception ex);
                        }
                    }
                    result[1] = loadFromServer;
                    e.Result = result;
                }                            
            };
            MattimonSqlServerDataRequestWorker.RunWorkerCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                            "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                            e.Error.Message + "\n" + e.Error.StackTrace +
                            (e.Error.InnerException != null ? "\n\nInner Exception:\n\n" + e.Error.InnerException.Message + "\n" + e.Error.InnerException.StackTrace : "")
                            , "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ///
                /// Result acquired
                ///
                if (e.Result != null && e.Result is object[] result)
                {
                    if (!(result[0] is DeviceServerObjects serverObjects)) serverObjects = new DeviceServerObjects();

                    bool local = Convert.ToBoolean(result[1]);
                    ///
                    /// Handle Server exceptions
                    ///
                    if (serverObjects.Exception != null || serverObjects.HttpRequestException != null)
                    {
                        if (serverObjects.Exception is SqlException sqlex)
                        {
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                            "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                            sqlex.Message + "\n" + sqlex.StackTrace +
                            (sqlex.InnerException != null ? "\n\nInner Exception:\n\n" + sqlex.InnerException.Message + "\n" + sqlex.InnerException.StackTrace : "")
                            , "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (serverObjects.HttpRequestException != null)
                        {
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                "An error occurred while attempting to post on the server.\n\nError details:\n\n" +
                                serverObjects.HttpRequestException.Message + "\n" + serverObjects.HttpRequestException.StackTrace +
                                (serverObjects.HttpRequestException.InnerException != null ? "\n\nInner Exception:\n\n" + serverObjects.HttpRequestException.InnerException.Message + "\n" + serverObjects.HttpRequestException.InnerException.StackTrace : "")
                                , "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return;
                    }
                    ///
                    /// Success
                    ///
                    else
                    {
                        UpdateGUISQLServer(serverObjects, null, false);

                        if (serverObjects.Instances != null)
                        {
                            if (serverObjects.Instances.Length == 0 && MyServiceController.GetServiceStatus("MattimonSQLServerService") == MyServiceController.ServiceState.Running)
                            {
                                new Thread(() =>
                                {
                                    MyServiceController.StopService("MattimonSQLServerService");
                                }).Start();
                            }
                        }
                    }
                }
            };
            mSqlSrvRequestWrkInit = true;
        }
        /// <summary>
        /// Initializes MattimonSqlServerPostDataWorker that will be posting DeviceServerObjects
        /// </summary>
        private void InitializeMattimonSqlServerPostWorker()
        {
            if (mSqlSrvPostWrkInit) return;
            MattimonSqlServerPostDataWorker = new BackgroundWorker();
            MattimonSqlServerPostDataWorker.DoWork += (s, e) =>
            {
                DeviceServerObjects dso = (DeviceServerObjects)e.Argument;
                e.Result = PostServerObjects(dso);
            };
            MattimonSqlServerPostDataWorker.RunWorkerCompleted += (s, e) =>
            {
                lblStatus.Text = "Ready";

                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.ToString());
                    return;
                }

                if (e.Result != null && e.Result is DeviceServerObjects serverObjects)
                {
                  
                    if (serverObjects.HttpRequestException != null)
                    {
                        GUI.BitscoreForms.BitscoreMessageBox.Show(serverObjects.HttpRequestException.Message + "\n\n" + serverObjects.HttpRequestException.StackTrace +
                            (serverObjects.HttpRequestException.InnerException != null ?
                            serverObjects.HttpRequestException.InnerException.Message + "\n\n" + serverObjects.HttpRequestException.InnerException.StackTrace : ""
                            ), "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (serverObjects.Exception != null)
                    {
                        GUI.BitscoreForms.BitscoreMessageBox.Show(
                            serverObjects.Exception.Message + "\n\n" + serverObjects.Exception.StackTrace +
                            (serverObjects.Exception.InnerException != null ?
                            serverObjects.Exception.InnerException.Message + "\n\n" + serverObjects.Exception.InnerException.StackTrace : ""
                            ), "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        UpdateGUISQLServer(serverObjects, null, false);
                    }
                }
            };
            mSqlSrvPostWrkInit = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instances"></param>
        private void DeleteSqlServerInstance(ServerInstance[] instances)
        {
            string svcPath = string.Empty, svcName = string.Empty;

            FormBackgroundWorker deleteInstances = new FormBackgroundWorker(this,

                new DoWorkEventHandler((s, e) => {

                    SQLServerRequests requests = new SQLServerRequests();

                    DeviceServerObjects dso = new DeviceServerObjects();

                    dso.DeviceID = GetDeviceID();

                    dso.Instances = instances;

                    try
                    {
                        svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(

                              MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),

                              "MattimonSQLServerService.exe");

                        if (!File.Exists(svcPath))

                            throw new IOException("Could not locate the service file: " + svcPath + ".\n" +

                            "Re-installing the application might fix this issue.");
                    }

                    catch (IOException ioe) { throw ioe; }

                    catch (Exception ex) { throw ex; }

                    svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;

                    SQLiteClientDatabase db = GetLocalDatabase();
                    
                    foreach (ServerInstance si in dso.Instances)
                    {
                        string insname = si.InstanceName.ToLower().Equals("mssqlserver") ? "" : si.InstanceName;

                        db.DeleteConnectionStringEntry(si.ServerName, insname);
                    }

                    

                    e.Result = requests.PostDeviceServerObjects(dso, DeviceServerObjectAction.delete);

                }), "Deleting Instance(s)");

            deleteInstances.RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) => {

                if (e.Error != null)
                {
                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An error occurred while attempting to delete SQL Server instances.\n\n" +
                        "Error details:\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (e.Result != null && e.Result is DeviceServerObjects dso)
                {
                    if (dso.Exception != null || dso.HttpRequestException != null)
                    {
                        if (dso.Exception != null)
                        {
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An server-side error occurred.\n\n" +
                           "Error details:\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(dso.Exception),
                           "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (dso.HttpRequestException != null)
                        {
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An server-side error occurred.\n\n" +
                           "Error details:\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(dso.HttpRequestException),
                           "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        return;
                    }

                    BtnSqlSrv_Refresh_Click(btn_sqlsrv_refresh, EventArgs.Empty);


                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Instance(s) have been deleted.", 
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
            deleteInstances.DoWork();
        }

        #region Rest Requests
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private UserAuthentication RequestAuthentication()
        {
            UserInfosRequest userInfosRequest = new UserInfosRequest();
            return userInfosRequest.GetUserInfo(GetUserID());
        }
        /// <summary>
        /// Assigns the global variable/object 'DeviceOptions' and returns a reference.
        /// </summary>
        /// <returns></returns>
        private DeviceOptions RequestDeviceOptions()
        {
            DeviceOptions deviceOptions = null;
            DeviceRequests deviceRequests = new DeviceRequests();
            long deviceid = GetDeviceID();
            long userid = GetUserID();
            long companyid = GetCompanyID();
            deviceOptions = deviceRequests.GetDeviceOptions(String.Format("{0};{1};{2}", deviceid, userid, companyid));

            // Assign global DeviceOptions
            this.DeviceOptions = deviceOptions;
            return deviceOptions;
        }
        /// <summary>
        /// Updates the device options and assigns the global variable/object Device Options and
        /// returns a reference.
        /// </summary>
        /// <param name="deviceOptions"></param>
        /// <returns></returns>
        private DeviceOptions UpdateDeviceOptions(DeviceOptions deviceOptions)
        {
            /* deviceOptions = procedure to update device options here */
            DeviceRequests deviceRequests = new DeviceRequests();

            return this.DeviceOptions = deviceRequests.PostDeviceOptions(new Device
            {

                MonitorSql = deviceOptions.MonitorSql ? 1 : 0,
                MonitorEventLog = deviceOptions.MonitorEventLog,
                NotificationEmails = deviceOptions.NotificationEmails,
                NotifyHealth = deviceOptions.NotifyHealth,
                NotifyStatus = deviceOptions.NotifyStatus,
                AgentReportInterval = deviceOptions.ReportingInterval,
                Device_Id = GetDeviceID(),
                Company_Id = GetCompanyID(),
                User_Id = GetUserID()
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DeviceServerObjects RequestDeviceServerObjects()
        {
            SQLServerRequests serverRequests = new SQLServerRequests();
            DeviceServerObjects serverObjects = serverRequests.PostDeviceServerObjects(new DeviceServerObjects {
                DeviceID = GetDeviceID() }, DeviceServerObjectAction.select);
            new MattimonAgentLibrary.Tools.DebugTools.TextLogging().LogToFile(serverObjects, mWriteLogToInstallDirectory, out Exception exception);
            if (exception != null)
            {
                new System.Threading.Thread(() => {
                    Invoke((MethodInvoker)delegate () {
                        GUI.BitscoreForms.BitscoreMessageBox.Show("An error occurred while attempted to read or write to local log file.\nYou may simply ignore this error." +
                            "\n\nError details:\n" + exception.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                });
            }
            return serverObjects;

        }

        /// <summary>
        /// Posts and returns the updated DeviceServerObjects as echo.
        /// </summary>
        /// <param name="dso"></param>
        /// <returns></returns>
        private DeviceServerObjects PostServerObjects(DeviceServerObjects dso)
        {
            SQLServerRequests requests = new SQLServerRequests();
            dso = requests.PostDeviceServerObjects(dso, DeviceServerObjectAction.insert);
            return dso;

        }

        /// <summary>
        /// Gets the version string from mattimon agent php file
        /// </summary>
        /// <returns></returns>
        public String RequestVersion()
        {
            RestClient restClient = new RestClient(Constants.GetActiveUpdateWebAppURL());
            restClient.AddDefaultHeader("X-Requested-With", "Mattimon-Web-Client");
            RestSharp.RestRequest request = new RestRequest(Method.GET);
            request.AddParameter(new Parameter { Name = "version", Value = "", Type = ParameterType.GetOrPost });
            return restClient.Execute(request).Content;
        }
        #endregion
   
        #region Local Database
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetDeviceID()
        {
            SQLiteClientDatabase db = GetLocalDatabase();
            return db.GetDeviceId();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetUserID()
        {
            SQLiteClientDatabase db = GetLocalDatabase();
            return db.GetUserId();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetCompanyID()
        {
            SQLiteClientDatabase db = GetLocalDatabase();
            return db.GetCompanyId();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetAgentID()
        {
            SQLiteClientDatabase db = GetLocalDatabase();
            return db.GetAgentId();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private ServerInstance LocalWriteConnectionString(SQLServerInstanceResult result)
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            db.CreateConnectionStringTable();
            string servername = result.ServerName;
            string instancename = result.InstanceName;
            string connectionstring = result.ConnectionString;
            string username = result.Username;
            string version = result.Version;
            bool monitored = true;
            db.InsertConnectionString(connectionstring, servername, instancename, username, version, monitored);

            DataTable dt = (DataTable)db.SelectConnectionString(servername, instancename);
            return new ServerInstance
            {
                ServerName = dt.Rows[0]["servername"] as string,
                InstanceName = dt.Rows[0]["instancename"] as string,
                ConnectionString = dt.Rows[0]["connectionstring"] as string,
                User = dt.Rows[0]["user"] as string,
                Clustered = result.Clustered,
                Monitored = Convert.ToBoolean(dt.Rows[0]["monitored"]),
                Version = result.Version
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        private string LocalReadConnectionString(string serverName, string instanceName)
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            db.CreateConnectionStringTable();
            DataTable dt = (DataTable)db.SelectConnectionString(serverName, instanceName);
            return dt.Rows[0]["connectionstring"] as string;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        private bool ConnectionStringEntryExists(string serverName, string instanceName)
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            return db.ConnectionStringExists(serverName, instanceName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ServerInstance[] LocalReadServerInstances()
        {
            List<ServerInstance> lst = new List<ServerInstance>();
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            db.CreateConnectionStringTable();
            DataTable dt = (DataTable)db.SelectConnectionStrings();
            if (dt == null) return null;
            foreach (DataRow row in dt.Rows)
            {
                lst.Add(new ServerInstance
                {
                     ConnectionString = row["connectionstring"] as string,
                     ServerName = row["servername"] as string,
                     InstanceName = row["instancename"] as string,
                     User = row["user"] as string,
                     Monitored = Convert.ToInt16(row["monitored"]) == 1,
                     Version = row["version"] as string
                });
            }
            return lst.ToArray();
        }
        #endregion

        #region GUI event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_settings_refreshSettings_Click(object sender, EventArgs e)
        {
            btn_settings_refreshSettings.Enabled = false;
            lblStatus.Text = "Refreshing";
            MattimonWebDataRequestBackgroundWorker.RunWorkerAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_settings_postSettings_Click(object sender, EventArgs e)
        {
            if (DeviceOptions != null)
            {
                btn_settings_postSettings.Enabled = false;
                this.DeviceOptions.MonitorEventLog = chk_settings_enabeEvtLogMonitoring.Checked ? 1 : 0;
                this.DeviceOptions.MonitorSql = chk_settings_enableSQLFeature.Checked;
                this.DeviceOptions.NotificationEmails = chk_settings_enableEmail.Checked;
                this.DeviceOptions.NotifyHealth = chk_settings_notifyHealth.Checked;
                this.DeviceOptions.NotifyStatus = chk_settings_notifyStatus.Checked;

                if (this.DeviceOptions.ReportingInterval != MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMinutesToMilliseconds(
                        Convert.ToDouble(cbo_settings_reportInterval.SelectedItem)))
                {
                    this.DeviceOptions.ReportingInterval =
                        MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMinutesToMilliseconds(
                            Convert.ToDouble(cbo_settings_reportInterval.SelectedItem));
                    dvcopt_agentIntervalChanged = true;
                }

                lblStatus.Text = "Posting options";

                if (!this.DeviceOptions.MonitorSql)
                    UninstallMattimonSqlServerService();
                else
                    InstallMattimonSqlServerService();

                if (this.DeviceOptions.MonitorEventLog == 0)
                {
                    // No action taken if not installed
                    UninstallMattimonService("MattimonEventLogService", "MattimonEventLogService");
                }
                else
                {
                    // No action taken if already installed
                    InstallMattimonService("MattimonEventLogService", "MattimonEventLogService");
                }

                MattimonWebDataUpdateBackgroundWorker.RunWorkerAsync();
            }
        }
        #endregion
    }
}
