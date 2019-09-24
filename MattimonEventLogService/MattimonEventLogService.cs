using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MattimonAgentLibrary.Rest;
using MattimonAgentLibrary.Tools;
using MattimonSQLite;

namespace MattimonEventLogService
{
    partial class MattimonEventLogService : ServiceBase
    {
        //private bool svcStoppedBySelf;
        public string MattimonAppInstallDirectory { get; private set; }
        public SQLiteClientDatabase SQLiteClientDatabase { get; private set; }
        public Timer Timer { get; private set; }
        public EventLogEntryListener EventLogEntryListener { get; private set; }

        private string guiExePath;
        private string thisSvcExePath;
        private ProjectAssemblyAtrributes thisSvcAssemAttr;
        private long fetchedDeviceId;
        private long fetchedDeviceTypeId;
        private long fetchedUserId;
        private long fetchedCompanyId;
        private double fetchedInterval;
        private string fetchedAgentId;
        private DeviceRequests deviceRequests;
        private bool fetchedUseAgent;
        private int fetchedMonitorEvtLogs;

        public MattimonEventLogService()
        {
            InitializeComponent();
            ServiceName = this.GetType().ToString();
            InitializeServiceComponents();
        }

        /// <summary>
        /// Installs (Or re-installs) the event log if it does not exist
        /// </summary>
        private void InitializeEventLog()
        {
            string src = MattimonAgentLibrary.Static.MattimonEventLogConstants.MattimonEventLogSourceName;
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
        /// Initializes required paths for this service
        /// and returns the Assembly Title of this Assembly
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
                    "MattimonEventLogService.exe");

            this.thisSvcAssemAttr =
                   new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                   this.thisSvcExePath);

            return thisSvcAssemAttr.AssemblyTitle;
        }
        private void InitializeServiceComponents()
        {
            try
            {
                InitializeEventLog();
            }
            catch { }

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


                try
                {
                    // Prepare the EventLongEntryListeners
                    // WARNING: The listener posts any event log entry that occurs.
                    // Service intervals do NOT apply in this case --- just a friendly reminder.
                    EventLogEntryListener = new EventLogEntryListener(
                        EventLogEntryListener_EntryWritten,
                        "Application", "Security", "System")
                    {
                        DebugLog = EventLog
                    };

                    // EDITED: The following statement has been removed from OnStart()
                    if (this.EventLogEntryListener != null)
                        this.EventLogEntryListener.StartListening();
                    // END EDITED;
                }
                catch (Exception ex)
                {
                    message += "\nError while initializing EventLogsEntryListeners.\n\n" + ex.Message + "\n\n" + ex.ToString() + "\n";
                }

                // Get the device Options
                MattimonAgentLibrary.Models.DeviceOptions deviceOptions =
                    RequestDeviceOptions(deviceId, userId, companyId);

                this.fetchedUseAgent = deviceOptions.UseAgent;
                this.fetchedMonitorEvtLogs = deviceOptions.MonitorEventLog;

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

                EventLog.WriteEntry(message, EventLogEntryType.Information, SVC_INIT_EVENT_ID);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                    EventLogEntryType.Error, SVC_INIT_EVENT_ID);
            }
        }


        /// <summary>
        /// Forwarded event from EventLogEntryListener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EventLogEntryListener_EntryWritten(object sender, EventLogEntryWrittenArgs e)
        {
            if (fetchedMonitorEvtLogs == 0)
            {
                string message = "EventLogEntryListener_EntryWritten\n\n";
                message += "User have not enabled the event log monitoring feature.\n" +
                    "Skipping.\n";
                EventLog.WriteEntry(message, EventLogEntryType.Information, EVENT_LOG_LISTENER_EVENT_ID);
                return;
            }

            String msg = "[MattimonEventLogService->EventLogEntryListener_EntryWritten]\n\nAn entry was written.\n\n";
            bool error = false;

            // Only notify for FailureAudit, Error and Warning.
            if (e.EventLogEntry.EntryType == EventLogEntryType.FailureAudit ||
                e.EventLogEntry.EntryType == EventLogEntryType.Error ||
                e.EventLogEntry.EntryType == EventLogEntryType.Warning)
            {
                string logname = e.AssociatedEventLog.Log;
                long entries = e.AssociatedEventLog.Entries.Count;

                msg += "Entry Instance ID: " + e.EventLogEntry.InstanceId + "\n";
                msg += "Entry Type: " + e.EventLogEntry.EntryType + "\n";

                msg += "Associated event logname: " + logname +
                    "\nAssociated event log entries: " + entries + "\n";

                MattimonAgentLibrary.Rest.DeviceEventLogEntryRequests eventLogEntryRequests = null;
                MattimonAgentLibrary.Models.DeviceEventLogEntry deviceEventLogEntry = null;

                try
                {
                    msg += "Posting entry started on " + DateTime.Now + "\n";

                    eventLogEntryRequests = new MattimonAgentLibrary.Rest.DeviceEventLogEntryRequests();
                    deviceEventLogEntry = eventLogEntryRequests.PostDeviceLongEntry(
                        new MattimonAgentLibrary.Models.DeviceEventLogEntry
                        {
                            DeviceId = this.fetchedDeviceId,
                            EntryType = (int)e.EventLogEntry.EntryType,
                            TimeGenerated = DateUtils.GetUnixTimestamp(e.EventLogEntry.TimeGenerated),
                            TimeWritten = DateUtils.GetUnixTimestamp(e.EventLogEntry.TimeWritten),
                            Source = e.EventLogEntry.Source,
                            Category = e.EventLogEntry.Category,
                            Message = e.EventLogEntry.Message,
                            InstanceId = e.EventLogEntry.InstanceId,
                            Index = e.EventLogEntry.Index,
                            EventLogName = logname
                        });

                    if (deviceEventLogEntry != null)
                    {
                        msg += "Posted entry returned.\n";
                    }


                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry
                        (ex.Message + "\n\n" + ex.ToString(), EventLogEntryType.Error, EVENT_LOG_LISTENER_EVENT_ID);
                    return;
                }

                if (deviceEventLogEntry.HttpRequestException != null)
                {
                    error = true;
                    msg += "An HttpRequestException occured:\n" +
                        deviceEventLogEntry.HttpRequestException.ToString() + " on " + DateTime.Now + "\n";

                    // Let us know that the operation couldn't complete.
                    String message = "An HTTP Request Exception occurred in " + this.GetType().ToString() + "->" + "EvtLog_EntryWritten\n\n";
                    message += deviceEventLogEntry.HttpRequestException.Message + "\n\n";
                    message += "Error details:\n" + deviceEventLogEntry.HttpRequestException.ToString();
                    EventLog.WriteEntry(message, EventLogEntryType.Warning, EVENT_LOG_LISTENER_EVENT_ID + 1);

                    // Store the entry in temporary table
                    // For now, do nothing.
                }
                else if (deviceEventLogEntry.MySqlExceptionMessage != null)
                {
                    error = true;
                    msg += "An SQL exception occured server side:\n" + deviceEventLogEntry.MySqlExceptionMessage + " on " + DateTime.Now + "\n";

                    String message = "An SQL Exception occurred in " + this.GetType().ToString() + "->" + "EvtLog_EntryWritten\n\n";
                    message += deviceEventLogEntry.MySqlExceptionMessage;
                    EventLog.WriteEntry(message, EventLogEntryType.Warning, EVENT_LOG_LISTENER_EVENT_ID + 1);

                    // Store the entry in temporary table
                    // For now, do nothing.
                }
                else if (deviceEventLogEntry.Exception != null)
                {
                    error = true;
                    msg += "An exception occured server side:\n" + deviceEventLogEntry.Exception.ToString() + " on " + DateTime.Now + "\n";

                    String message = "An Exception occurred in " + this.GetType().ToString() + "->" + "EvtLog_EntryWritten\n\n";
                    message += deviceEventLogEntry.Exception.Message + "\n\n" + deviceEventLogEntry.Exception.ToString();

                    EventLog.WriteEntry(message, EventLogEntryType.Warning, EVENT_LOG_LISTENER_EVENT_ID + 1);

                    // Store the entry in temporary table
                    // For now, do nothing.
                }
                else
                {
                    if (deviceEventLogEntry.MySqlInsertStatus > 0)
                    {
                        msg += "Posted and inserted on mattimon server's database on " + DateTime.Now + "\n";

                        EventLog.WriteEntry("[EventLogEntryListener->EvtLog_EntryWritten]\n\nPost DeviceEventLogEntry success\n\n" +
                            "LogName: " + logname + "\n" +
                            "Entry Type: " + e.EventLogEntry.EntryType + "\n" +
                            "Entry Event ID: " + e.EventLogEntry.InstanceId + "\n" +
                            "Entry Source: " + e.EventLogEntry.Source + "\n",
                            EventLogEntryType.Information, EVENT_LOG_LISTENER_EVENT_ID);
                    }
                    else
                    {
                        error = true;
                        msg += "Posted event log entry but could't insert on the" +
                            "mattimon server's database on " + DateTime.Now + "\n";

                        EventLog.WriteEntry("[EventLogEntryListener->EvtLog_EntryWritten]\n\nPost DeviceEventLogEntry was not succesfull\n\n" +
                            "LogName: " + logname + "\n" +
                            "Entry Type: " + e.EventLogEntry.EntryType + "\n" +
                            "Entry Event ID: " + e.EventLogEntry.InstanceId + "\n" +
                            "Entry Source: " + e.EventLogEntry.Source + "\n",
                            EventLogEntryType.Warning, EVENT_LOG_LISTENER_EVENT_ID);
                    }
                }
            }
            else
            {
                // An entry was written, but reporting was not necessary.
                // Handle that case here, if needed.
                msg = "[MattimonAgentService->EventLogEntryListener_EntryWritten]\n\nAn entry was written.\n\n";

                msg += "Entry Instance ID: " + e.EventLogEntry.InstanceId + "\n";
                msg += "Entry Type: " + e.EventLogEntry.EntryType + "\n";

                msg += "Associated event logname: " + e.AssociatedEventLog.Log +
                    "\nAssociated event log entries: " + e.AssociatedEventLog.Entries.Count + "\n";

                msg += "This entry type is skipped by the agent.\n\n";
            }


            EventLog.WriteEntry(msg,
                    (error == true ? EventLogEntryType.Error : EventLogEntryType.Information),
                        EVENT_LOG_LISTENER_EVENT_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private MattimonAgentLibrary.Models.DeviceOptions RequestDeviceOptions(long deviceId, long userId, long companyId)
        {
            MattimonAgentLibrary.Rest.DeviceRequests deviceRequests = new MattimonAgentLibrary.Rest.DeviceRequests();
            return deviceRequests.GetDeviceOptions(
                        String.Format("{0};{1};{2}", deviceId, userId, companyId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            EventLogEntryType eventLogEntryType = EventLogEntryType.Information;
            string message = "Service Started.\n";

            if (fetchedMonitorEvtLogs == 0)
            {
                try
                {
                    if (Timer != null && Timer.Enabled)
                        Timer.Stop();

                    message += "User have not enabled the event log monitoring feature.\n" +
                    "The service will now stop.\n";
                }

                catch (Exception ex) {
                    eventLogEntryType = EventLogEntryType.Warning;
                    message += "\n\nAn error occurred while attempted to stop the timer:\n\n" + 
                        Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                }
            }

            EventLog.WriteEntry(message, eventLogEntryType);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            // The Service is stopping, stop/dispose resources.
            if (this.EventLogEntryListener != null)
                this.EventLogEntryListener.StopListening();
            EventLog.WriteEntry("Service Stopped");
        }

        #region Constants
        public const int SVC_INIT_EVENT_ID = 100;
        public const int ON_TIMER_EVENT_ID = 200;
        public const int EVENT_LOG_LISTENER_EVENT_ID = 300;
        public const int DEVICE_OPTION_REQUEST_EVENT_ID = 400;
        #endregion
    }
}
