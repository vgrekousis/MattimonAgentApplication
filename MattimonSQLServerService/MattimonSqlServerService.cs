using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MattimonSQLite;
using MattimonAgentLibrary.WMI;
using MattimonAgentLibrary.Tools;
using System.IO;
using System.IO.Compression;
using MattimonAgentLibrary.Models;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentLibrary.Rest;
using System.Net.Http;

namespace MattimonSQLServerService
{
    public partial class MattimonSqlServerService : ServiceBase
    {
        public const int ON_START_EVENT_ID = 0;
        public const int ON_TIMER_EVENT_ID = 1;
        public const int ON_STOP_EVENT_ID = 2;
        public const int SVC_INIT_EVENT_ID = 100;
        public const int SQLSRV_UPD_EVENT_ID = 200;
        public const int REQUEST_OPTIONS_EVENT_ID = 300;

        public string MattimonAppInstallDirectory { get => _mattimonAppInstallDirectory; private set => _mattimonAppInstallDirectory = value; }
        public System.Timers.Timer Timer { get; private set; }
        public SQLiteClientDatabase SQLiteClientDatabase { get; private set; }

        private string guiExePath, thisSvcExePath;
        private ProjectAssemblyAtrributes thisSvcAssemAttr;
        private string _mattimonAppInstallDirectory;
        private long fetchedDeviceId;
        private long fetchedDeviceTypeId;
        private long fetchedUserId;
        private long fetchedCompanyId;
        private double fetchedInterval;
        private string fetchedAgentId;
        private DeviceRequests deviceRequests;
        private bool fetchedUseAgent;
        private int fetchedSqlMonit;

        public MattimonSqlServerService()
        {
            InitializeComponent();
            ServiceName = this.GetType().ToString();
            InitializeServiceComponents();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeServiceComponents()
        {
            InitializeEventLog();


            try
            {
                InitializePaths();



                if (!System.IO.File.Exists(this.guiExePath))
                {
                    EventLog.WriteEntry("Main application assembly file is missing.\n\n" +
                       "Application must be properly installed in order to access required assembly information and paths.",
                       EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                    return;
                }

                if (!System.IO.File.Exists(this.thisSvcExePath))
                {
                    EventLog.WriteEntry("Service assembly file is missing.\n\n" +
                       "Application must be properly installed in order to access required assembly information and paths.",
                       EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                    return;
                }

                MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes thisSvcAssemAttr =
                   new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                   this.thisSvcExePath);


                String message = "";
                message = String.Format(

                    "{0}->InitializeServiceComponents\n\n" +
                    "Application path: {1}\n" +
                    "Service path: {2}\n" +
                    "Service path (Win32_Services->PathName): {3}\n\n" +
                    "MattimonAgentApplication.exe assembly information:\n\n" +
                    "Title: {4}\n" +
                    "Company: {5}\n" +
                    "Product: {6}\n" +
                    "Version: {7}\n\n" +
                    "CommonAppData directory: {8}",

                    ServiceName,
                    this.guiExePath,
                    this.thisSvcExePath,
                    MattimonAgentLibrary.Tools.MyServiceController.GetWin32ServicePathName(ServiceName),
                    this.thisSvcAssemAttr.AssemblyTitle,
                    this.thisSvcAssemAttr.AssemblyCompany,
                    this.thisSvcAssemAttr.AssemblyProduct,
                    this.thisSvcAssemAttr.GetAssemblyVersion(),
                    MattimonAgentLibrary.Static.Constants.CommonAppData
                    );


                /// Initialize the local database
                SQLiteClientDatabase = new SQLiteClientDatabase(

                    MattimonAgentLibrary.Static.Constants.CommonAppData,

                    MattimonAgentLibrary.Tools.RegistryTools.GetPublisherByDisplayName(
                        MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),

                    MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName,

                    MattimonAgentLibrary.Static.Constants.LocalDatabaseName
                );

                long deviceId = SQLiteClientDatabase.GetDeviceId();
                long deviceTypeId = SQLiteClientDatabase.GetDeviceTypeId();
                long userId = SQLiteClientDatabase.GetUserId();
                long companyId = SQLiteClientDatabase.GetCompanyId();
                double interval = SQLiteClientDatabase.GetReportingInterval();

                string agentId = SQLiteClientDatabase.GetAgentId();

                // Set global variables
                this.fetchedDeviceId = deviceId;
                this.fetchedDeviceTypeId = deviceTypeId;
                this.fetchedUserId = userId;
                this.fetchedCompanyId = companyId;
                this.fetchedInterval = interval;

                this.fetchedAgentId = agentId;


                // Prepare the rest request
                this.deviceRequests =
                    new MattimonAgentLibrary.Rest.DeviceRequests();


                // Get the device Options
                MattimonAgentLibrary.Models.DeviceOptions deviceOptions =
                    RequestDeviceOptions(deviceId, userId, companyId);


                this.fetchedUseAgent = deviceOptions.UseAgent;
                this.fetchedSqlMonit = deviceOptions.MonitorSql ? 1 : 0;


                message += String.Format(
                    "User authentication keys:\n\n" +
                    "User ID: {0}\n" +
                    "Company ID: {1}\n" +
                    "Device ID: {2}\n\n" +

                    "Device Options:\n\n" +
                    "Reporting interval (local): every {3} minute(s)\n" +
                    "Reporting inteval (server): every {4} minute(s)\n" +
                    "(Reporting interval on local and server must be identical)\n" +
                    "Email notifications: {5}",
                    userId,
                    companyId,
                    deviceId,
                    MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToMinutes(interval),
                    MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToMinutes(deviceOptions.ReportingInterval),
                    (deviceOptions.NotificationEmails ? "Enabled" : "Disabled")
                    );


                /// Finally, initialize the timers
                Timer = new System.Timers.Timer { Interval = interval };
                Timer.Elapsed += OnTimer;
                Timer.Start();

                EventLog.WriteEntry(message, EventLogEntryType.Information, SVC_INIT_EVENT_ID);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                    EventLogEntryType.Error, SVC_INIT_EVENT_ID);

                if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(ServiceName) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running)
                {
                    MattimonAgentLibrary.Tools.MyServiceController.StopService(ServiceName);
                }
            }
        }


        /// <summary>
        /// Installs (Or re-installs) the event log if it does not exist
        /// </summary>
        private void InitializeEventLog()
        {
            string src = MattimonAgentLibrary.Static.MattimonEventLogConstants.SQLServerSourceName;
            string log = MattimonAgentLibrary.Static.MattimonEventLogConstants.MainLogName;

            try
            {
                AutoLog = false;
                if (!System.Diagnostics.EventLog.SourceExists(src))

                    System.Diagnostics.EventLog.CreateEventSource(
                       src,
                       log);

                EventLog.Source = src;
                EventLog.Log = log;
            }
            catch (Exception ex) { AutoLog = true; throw ex; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private String InitializePaths()
        {
            this.MattimonAppInstallDirectory =
                MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                    MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName);

            this.guiExePath = System.IO.Path.Combine(this.MattimonAppInstallDirectory,
                   "MattimonAgentApplication.exe");

            this.thisSvcExePath = System.IO.Path.Combine(this.MattimonAppInstallDirectory,
                    "MattimonSqlServerService.exe");

            this.thisSvcAssemAttr =
                   new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                   this.thisSvcExePath);

            return thisSvcAssemAttr.AssemblyTitle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private DeviceOptions RequestDeviceOptions(long deviceId, long userId, long companyId)
        {
            //EventLog.WriteEntry(string.Format("RequestDeviceOptions({0},{1},{2})", deviceId, userId, companyId), EventLogEntryType.Warning, 999);
            return deviceRequests.GetDeviceOptions(
                string.Format("{0};{1};{2}", deviceId, userId, companyId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ServerInstance[] LocalReadServerInstances()
        {
            List<ServerInstance> lst = new List<ServerInstance>();
            MattimonSQLite.SQLiteClientDatabase db = SQLiteClientDatabase;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnTimer(object sender, EventArgs e)
        {
            InitializeEventLog(); // Install the event log (again) if it was deleted by the user

            StringBuilder message = new StringBuilder();
            EventLogEntryType entryType = EventLogEntryType.Information;
            
            message.AppendLine("OnTimer");
            message.AppendLine("=======");

            bool ok = RequestOptions();
            message.AppendLine((ok ? "Request Options passed." : "Request Options failed.") + " Monitor event id " +  REQUEST_OPTIONS_EVENT_ID + " for inner details.");
            entryType = ok ? EventLogEntryType.Information : EventLogEntryType.Warning;

            if (fetchedSqlMonit == 1)
            {
                ReportDeviceServerObjects();
                message.AppendLine("ReportDeviceServerObjects has finished. Monitor event id " + SQLSRV_UPD_EVENT_ID + " for inner details.");
            }
            else
            {
                try
                {
                    message.AppendLine("SQL Monitoring feature is currently suspended. Reporting Device Server Objects have been skipped.\nStopping Service.");
                    Stop();
                }
                catch (Exception ex)
                {
                    message.AppendLine("\nAn error occurred while attempted to stop the service.\n" +
                        Tools.ExceptionHelper.GetFormatedExceptionMessage(ex));
                }
            }

            EventLog.WriteEntry(message.ToString(), entryType, ON_TIMER_EVENT_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(ServiceName + " on start.", EventLogEntryType.Information, ON_START_EVENT_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            StringBuilder message = new StringBuilder();
            EventLogEntryType entryType = EventLogEntryType.Information;
            message.AppendLine(ServiceName + " on stop.");

            try { if (Timer != null && Timer.Enabled) Timer.Stop(); }
            catch (Exception ex)
            {
                entryType = EventLogEntryType.Warning;
                message.AppendLine();
                message.AppendLine(GetFormatedExceptionMessage(ex));
            }

            EventLog.WriteEntry(message.ToString(), entryType, ON_STOP_EVENT_ID);
        }

        /// <summary>
        /// Assigns this.fetched* attributes.
        /// </summary>
        protected bool RequestOptions(bool writeEventLogEntry = true)
        {
            MattimonAgentLibrary.Models.DeviceOptions deviceOptions = null;
            StringBuilder message = new StringBuilder();
            EventLogEntryType entryType = EventLogEntryType.Information;
            
            message.AppendLine("RequestOptions");
            message.AppendLine("==============");

            try
            {
                var watch = Stopwatch.StartNew();
                deviceOptions = RequestDeviceOptions(fetchedDeviceId, fetchedUserId, fetchedCompanyId);
                watch.Stop();

                if (deviceOptions.RequestSuccess)
                {
                    this.fetchedUseAgent = deviceOptions.UseAgent;
                    this.fetchedSqlMonit = deviceOptions.MonitorSql ? 1 : 0;
                    message.AppendLine("Fetched UseAgent: " + this.fetchedUseAgent.ToString());
                    message.AppendLine("Fetched MonitorSql: " + this.fetchedSqlMonit.ToString());
                    message.AppendLine();
                    message.AppendLine("Completed in " + TimeSpanUtil.ConvertMillisecondsToSeconds(watch.ElapsedMilliseconds) + " seconds.");
                }
                else
                {
                    entryType = EventLogEntryType.Error;
                    if (deviceOptions.HttpRequestException != null)
                    {
                        HttpRequestException e = deviceOptions.HttpRequestException;
                        message.AppendLine("HttpRequestException?");
                        message.AppendLine(GetFormatedExceptionMessage(e));
                        message.AppendLine();
                    }
                    if (deviceOptions.MySqlExceptionMessage != null)
                    {
                        Exception e = new Exception(deviceOptions.MySqlExceptionMessage);
                        message.AppendLine("SqlExceptionMessage?");
                        message.AppendLine(GetFormatedExceptionMessage(e));
                        message.AppendLine();
                    }
                    if (deviceOptions.Exception != null)
                    {
                        Exception e = deviceOptions.Exception;
                        message.AppendLine("Exception?");
                        message.AppendLine(GetFormatedExceptionMessage(e));
                        message.AppendLine();
                    }
                }
            }
            catch (Exception e)
            {
                entryType = EventLogEntryType.Error;
                message.AppendLine(GetFormatedExceptionMessage(e));
                message.AppendLine();
            }

            if (writeEventLogEntry)
                EventLog.WriteEntry(message.ToString(), entryType, REQUEST_OPTIONS_EVENT_ID);

            return entryType != EventLogEntryType.Error;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReportDeviceServerObjects()
        {

            string message = GetType() + ".ReportDeviceServerObjects\n";
            message += "\n" + new string('=', message.Length);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            ///
            /// Request from server all Instance Objects associated to this device
            /// The reason why we're doing this: As we request local data to post back for update, primery/foreign keys are not existing.
            ///
            SQLServerRequests requests = new SQLServerRequests();
            message += "\nRequesting DeviceServerObjects from the server in order to assign update primary/foreign keys to locally retreived DeviceServerObjects.";
            DeviceServerObjects reqDeviceServerObjects = requests.PostDeviceServerObjects(new DeviceServerObjects { DeviceID = fetchedDeviceId },
                DeviceServerObjectAction.select);
            if (GetErrorMessageIfAny(reqDeviceServerObjects) != null)
            {
                watch.Stop();
                EventLog.WriteEntry(message += GetErrorMessageIfAny(reqDeviceServerObjects), EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                return;
            }
            if (reqDeviceServerObjects.Instances == null || reqDeviceServerObjects.Instances.Length < 1)
            {
                watch.Stop();
                message += "\nRequested DeviceServerObjects instances are null or empty. No action taken.";
                EventLog.WriteEntry(message, EventLogEntryType.Warning, SQLSRV_UPD_EVENT_ID);
                return;
            }
            ///
            /// Query Local by SQLServerObjectProvider to assign optained primary/foreign keys for update.
            ///
            message += "\nQuerying local DeviceServerObjects from SQLServerObjectProvider holding the potentially new data.";
            message += "\n  This is where the requested keys from the server will be assigned to.";
            DeviceServerObjects updDeviceServerObjects = new SQLServerObjectProvider().GetDeviceServerObjects(LocalReadServerInstances(out Exception exeptionLocalRead), fetchedDeviceId, out Exception exception);
            if (exeptionLocalRead != null)
            {
                watch.Stop();
                message += "\nAn error occurred while attempting to read locally stored instances..\n\n" + exeptionLocalRead.Message + "\n" + exeptionLocalRead.StackTrace + "\n\n" +
                    (exception.InnerException != null ? "Inner Exception:" + exeptionLocalRead.InnerException.Message + "\n" + exeptionLocalRead.InnerException.StackTrace : "");
                EventLog.WriteEntry(message, EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                return;
            }
            if (exception != null)
            {
                watch.Stop();
                message += "\nException occurred.\n\n" + exception.Message + "\n" + exception.StackTrace + "\n\n" +
                    (exception.InnerException != null ? "Inner Exception:" + exception.InnerException.Message + "\n" + exception.InnerException.StackTrace : "");
                EventLog.WriteEntry(message, EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                return;
            }
            if (GetErrorMessageIfAny(updDeviceServerObjects) != null)
            {
                watch.Stop();
                EventLog.WriteEntry(message += GetErrorMessageIfAny(reqDeviceServerObjects), EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                return;
            }
            if (updDeviceServerObjects.Instances == null || updDeviceServerObjects.Instances.Length < 1)
            {
                watch.Stop();
                message += "\nSQLServerObjectProvider returned instances are null or empty. No action taken.";
                EventLog.WriteEntry(message, EventLogEntryType.Warning, SQLSRV_UPD_EVENT_ID);
                return;
            }
            ///
            /// Assign Update Primary/Foreign keys
            ///
            try
            {
                message += "\nAssigning update keys to ServerInstances and ServerDatabases for update.";
                foreach (ServerInstance reqsi in reqDeviceServerObjects.Instances)
                {
                    for (int i = 0; i < updDeviceServerObjects.Instances.Length; i++)
                    {
                        if (updDeviceServerObjects.Instances[i].ConnectStatus == 0) // In this case local instance name could not be retrieved
                            updDeviceServerObjects.Instances[i].InstanceName = reqsi.InstanceName; // assign it so we can still pass through the next if statement.

                        if (reqsi.ServerName == updDeviceServerObjects.Instances[i].ServerName && reqsi.InstanceName == updDeviceServerObjects.Instances[i].InstanceName)
                        {

                            updDeviceServerObjects.Instances[i].ID = reqsi.ID;
                            updDeviceServerObjects.Instances[i].Monitored = reqsi.Monitored;

                            EventLog.WriteEntry("ConnectStatus: " + updDeviceServerObjects.Instances[i].ConnectStatus + "" +
                                "\nInstance Name: " + updDeviceServerObjects.Instances[i].InstanceName + "\n" +
                                "Monitored: " + updDeviceServerObjects.Instances[i].Monitored, EventLogEntryType.Warning, SQLSRV_UPD_EVENT_ID + 10);

                            updDeviceServerObjects.Instances[i].LastReportedTimestamp =

                                updDeviceServerObjects.Instances[i].ConnectStatus == 1 ?
                                // connection between agent and instance ok, we're good to set the LastReportedTimestamp to now.
                                DateUtils.GetUnixTimestamp(DateTime.Now) :
                                // in this case, use the LastReportedTimestamp already found on-server
                                // as the last succesfull report datetime (timestamp).
                                reqsi.LastReportedTimestamp;

                            message += "\n" + (updDeviceServerObjects.Instances[i].ConnectStatus == 1 ?
                               "\nConnection agent-instance: passed (LastReportedTimestamp set to now)" :
                               "\nConnection agent-instance: failed (LastReportedTimestamp set to last reported timestamp as found on the server)");

                            message += "\nMSSQL: " + (updDeviceServerObjects.Instances[i].SQLService.Name) + " - " +
                                (updDeviceServerObjects.Instances[i].SQLService.DisplayName) + " (" +
                                (updDeviceServerObjects.Instances[i].SQLService.IsRunning ? "Is Running" : "Is not running") + ")" +
                                "\nStartTypeCode: " + (updDeviceServerObjects.Instances[i].SQLService.StartType);

                            message += "\nServerInstance: Assigned " + reqsi.ID + " on " + updDeviceServerObjects.Instances[i].ServerName + ".";
                        }

                        foreach (Database reqdb in reqsi.Databases)
                        {
                            for (int j = 0; j < updDeviceServerObjects.Instances[i].Databases.Length; j++)
                            {
                                if (reqdb.DatabaseName == updDeviceServerObjects.Instances[i].Databases[j].DatabaseName)
                                {
                                    updDeviceServerObjects.Instances[i].Databases[j].ID = reqdb.ID;
                                    updDeviceServerObjects.Instances[i].Databases[j].Monitored = reqdb.Monitored;

                                    message += "\nInstanceDatabase: Assigned " + reqdb.ID + " on " + updDeviceServerObjects.Instances[i].Databases[j].DatabaseName + ".";
                                    message += " \n\tMetaData.DbSize=" + updDeviceServerObjects.Instances[i].Databases[j].MetaData.DBSize + " " + 
                                        updDeviceServerObjects.Instances[i].Databases[j].MetaData.MetricsDBSize;
                                    message += " \n\tMetaData.DBMaxSize=" + updDeviceServerObjects.Instances[i].Databases[j].MetaData.DBMaxSize + " " +
                                        updDeviceServerObjects.Instances[i].Databases[j].MetaData.MetricsDBMaxSize;
                                    message += " \n\tMetaData.LogSize=" + updDeviceServerObjects.Instances[i].Databases[j].MetaData.LogSize + " " +
                                        updDeviceServerObjects.Instances[i].Databases[j].MetaData.MetricsLogSize;
                                    message += " \n\tMetaData.LogMaxSize=" + updDeviceServerObjects.Instances[i].Databases[j].MetaData.LogMaxSize + " " +
                                        updDeviceServerObjects.Instances[i].Databases[j].MetaData.MetricsLogMaxSize;
                                }
                            }
                        }
                    }
                }

                message += "\nAttempting to post the local DeviceServerObjects containing requested primary/foreign keys.";
                updDeviceServerObjects = requests.PostDeviceServerObjects(updDeviceServerObjects, DeviceServerObjectAction.update);

                if (GetErrorMessageIfAny(updDeviceServerObjects) != null)
                {
                    watch.Stop();
                    EventLog.WriteEntry(message += GetErrorMessageIfAny(updDeviceServerObjects), EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                    return;
                }
            }
            catch (Exception ex)
            {
                message += "\nException occurred.\n\n" + ex.Message + "\n" + ex.StackTrace + "\n\n" + ex.ToString() + "\n\n" + 
                    (ex.InnerException != null ? "Inner Exception:" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace : "");
                watch.Stop();
                EventLog.WriteEntry(message, EventLogEntryType.Error, SQLSRV_UPD_EVENT_ID);
                return;
            }

            watch.Stop();
            object[] result = new object[2];
            result[0] = updDeviceServerObjects;
            result[1] = MattimonAgentLibrary.Tools.TimeSpanUtil.ConvertMillisecondsToSeconds(watch.ElapsedMilliseconds);
            message += "\nPost update call accomplished in " + Convert.ToDouble(result[1]) + " seconds.";
            EventLog.WriteEntry(message, EventLogEntryType.Information, SQLSRV_UPD_EVENT_ID);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dso"></param>
        /// <returns></returns>
        private Exception GetExceptionIfAny(DeviceServerObjects dso)
        {
            if (dso != null && dso.Exception != null)
                return dso.Exception;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool LocalDatabaseExistsInRequestedDatabases(Database updDatabase, Database[] reqDatabases)
        {
            foreach (Database reqdb in reqDatabases)
                if (updDatabase.DatabaseName == reqdb.DatabaseName)
                    return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dso"></param>
        /// <returns></returns>
        private HttpRequestException GetHttpRequestExceptionIfAny(DeviceServerObjects dso)
        {
            if (dso != null && dso.HttpRequestException != null)
                return dso.HttpRequestException;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dso"></param>
        /// <returns></returns>
        private string GetErrorMessageIfAny(DeviceServerObjects dso)
        {
            string message = null;
            if (GetExceptionIfAny(dso) != null || GetHttpRequestExceptionIfAny(dso) != null)
            {
                if (GetHttpRequestExceptionIfAny(dso) != null)
                {
                    message += "\n\n" + ("An HttpRequestException occured while attemping to update SQL Server Objects on the server.\n\n" +
                         dso.HttpRequestException.Message + "\n\n" +
                         dso.HttpRequestException.StackTrace +
                         (dso.HttpRequestException.InnerException != null ?
                         dso.HttpRequestException.InnerException.Message + "\n\n" +
                         dso.HttpRequestException.InnerException.StackTrace : ""));
                }
                if (GetExceptionIfAny(dso) != null)
                {
                    message += "\n\n" + ("An Exception occured while attemping to update SQL Server Objects on the server.\n\n" +
                        dso.Exception.Message + "\n\n" +
                        dso.Exception.StackTrace +
                        (dso.Exception.InnerException != null ?
                        dso.Exception.InnerException.Message + "\n\n" +
                        dso.Exception.InnerException.StackTrace : ""));
                }
            }
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ServerInstance[] LocalReadServerInstances(out Exception exception)
        {
            exception = null;
            List<ServerInstance> lst = new List<ServerInstance>();
            try
            {
                MattimonSQLite.SQLiteClientDatabase db = SQLiteClientDatabase;
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
            catch (Exception ex)
            {
                exception = ex;
                return lst.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="exceptio"></param>
        /// <returns></returns>
        private string GetFormatedExceptionMessage(Exception e, StringBuilder extraMessage = null)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(e.GetType().Name);
            message.AppendLine(new String('-', e.GetType().Name.Length));
            message.AppendLine("Message:");
            message.AppendLine(e.Message);
            message.AppendLine("Stack Trace:");
            message.AppendLine(e.StackTrace);

            if (e.InnerException != null)
            {
                message.AppendLine("Inner Exception");
                message.AppendLine("---------------");
                message.AppendLine("Inner Exception Message:");
                message.AppendLine();
                message.AppendLine(e.InnerException.Message);
                message.AppendLine("Inner Exception Stack Trace:");
                message.AppendLine();
                message.AppendLine(e.InnerException.StackTrace);
            }

            if (extraMessage != null)
            {
                message.AppendLine("Extra Message");
                message.AppendLine("-------------");
                message.AppendLine(extraMessage.ToString());
            }
            return message.ToString();
        }
    }
}
