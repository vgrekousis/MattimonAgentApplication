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

namespace MattimonAgentService
{
    public partial class MattimonAgentService : ServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        public MattimonAgentService()
        {
            InitializeComponent();
            ServiceName = this.GetType().ToString();
            InitializeServiceComponents();
            LoadAssemblyVersions();
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
                    "MattimonAgentService.exe");

            this.thisSvcAssemAttr =
                   new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                   this.thisSvcExePath);

            return this.thisSvcAssemAttr.AssemblyTitle;
        }
        /// <summary>
        /// 
        /// </summary>
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

                try
                {
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
                    /// NOTE: each time the user changes related options, the service needs to be stopped and restarted
                    /// in order for the options to take effect. 
                    /// [Void if CheckOnlineOptions is called in every timer-elapsed event!]


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

                    EventLog.WriteEntry(message, EventLogEntryType.Information, SVC_INIT_EVENT_ID);

                    /// Finally, initialize the timers
                    Timer = new System.Timers.Timer
                    {
                        Interval = interval
                    };
                    Timer.Elapsed += Timer_Elapsed;
                    Timer.Start();

                    /// And the Timer that handles the update for our updater service
                    TimerUpd = new System.Timers.Timer
                    {
                        Interval = 1000 * 60 * 1
                    };
                    TimerUpd.Elapsed += TimerUpd_Elapsed;
                    TimerUpd.Start();
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                        EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                    EventLogEntryType.Error, SVC_INIT_EVENT_ID);
            }
        }


        private MattimonAgentLibrary.Models.DeviceOptions RequestDeviceOptions(long deviceId, long userId, long companyId)
        {
            return deviceRequests.GetDeviceOptions(
                        String.Format("{0};{1};{2}", deviceId, userId, companyId));
        }

        


        /// <summary>
        /// Installs (Or re-installs) the event log if it does not exist
        /// </summary>
        private void InitializeEventLog()
        {
            String src = MattimonAgentLibrary.Static.MattimonEventLogConstants.AgentSourceName;
            String log = MattimonAgentLibrary.Static.MattimonEventLogConstants.MainLogName;

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
        private string agentversion, agentServiceVersion, updateServiceVersion, mattimonSQLServiceVersion, libraryVersion;

        private void LoadAssemblyVersions()
        {
            EventLogEntryType entrytype = EventLogEntryType.Information;
            string versAssignmentMessage = string.Empty;

            try
            {
                this.agentversion = new
                    MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                    this.MattimonAppInstallDirectory + "\\" +
                    "MattimonAgentApplication.exe").GetAssemblyVersion().ToString();
                versAssignmentMessage = "UI version retrieved. Version=" + this.agentversion;
            }
            catch (Exception ex)
            {
                versAssignmentMessage += "\nCould not read UI version.\n\n" +
                    Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                entrytype = EventLogEntryType.Warning;
            }

            try
            {
                this.agentServiceVersion = new
                    MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                    this.MattimonAppInstallDirectory + "\\" + "MattimonAgentService.exe").GetAssemblyVersion().ToString();
                versAssignmentMessage += "\nAgent Service version retrieved. Version=" + this.agentServiceVersion;
            }
            catch (Exception ex)
            {
                versAssignmentMessage += "\nCould not read Agent Service version.\n\n" +
                    Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                entrytype = EventLogEntryType.Warning;
            }

            try
            {
                this.updateServiceVersion = new
                    MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                    this.MattimonAppInstallDirectory + "\\" + "MattimonUpdateService.exe").GetAssemblyVersion().ToString();
                versAssignmentMessage += "\nUpdate Service version retrieved. Version=" + this.updateServiceVersion;
            }
            catch (Exception ex)
            {
                versAssignmentMessage += "\nCould not read Mattimon Update Service version.\n\n" +
                    Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                entrytype = EventLogEntryType.Warning;
            }

            try
            {
                this.mattimonSQLServiceVersion = new
                    MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                    this.MattimonAppInstallDirectory + "\\" + "MattimonSQLServerService.exe").GetAssemblyVersion().ToString();
                versAssignmentMessage += "\nMattimon SQL Service version retrieved. Version=" + this.mattimonSQLServiceVersion;
            }
            catch (Exception ex)
            {
                versAssignmentMessage += "\nCould not read Mattimon SQL Service version.\n\n" +
                    Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                entrytype = EventLogEntryType.Warning;
            }

            try
            {
                this.libraryVersion = new
                    MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                    this.MattimonAppInstallDirectory + "\\" + "MattimonAgentLibrary.dll").GetAssemblyVersion().ToString();
                versAssignmentMessage += "\nMattimon Library version retrieved. Version=" + this.libraryVersion;
            }
            catch (Exception ex)
            {
                versAssignmentMessage += "\nCould not read Mattimon Library version.\n\n" +
                    Tools.ExceptionHelper.GetFormatedExceptionMessage(ex);
                entrytype = EventLogEntryType.Warning;
            }

            EventLog.WriteEntry(versAssignmentMessage, entrytype, SVC_INIT_EVENT_ID);
        }

        /// <summary>
        /// Gets the interval options, sql monitoring option and use agent option from the server.
        /// Interval option applied to the timer only if changed.
        /// </summary>
        /// <param name="useBackgroundWorker"></param>
        private void CheckOnlineOptions(bool useBackgroundWorker = false)
        {
            if (useBackgroundWorker)
            {
                BackgroundWorker requestOptions = new BackgroundWorker();
                requestOptions.DoWork += (s, e) =>
                {
                    MattimonAgentLibrary.Models.DeviceOptions options =
                    RequestDeviceOptions(fetchedDeviceId, fetchedUserId, fetchedCompanyId);
                    e.Result = options;
                };
                requestOptions.RunWorkerCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        EventLog.WriteEntry(ExceptionHelper.GetFormatedExceptionMessage(e.Error), 
                            EventLogEntryType.Error, DEVICE_OPTION_REQUEST_EVENT_ID);
                        return;
                    }

                    if (e.Result != null && e.Result is DeviceOptions options)
                    {
                        bool exception = false;
                        if (options.MySqlExceptionMessage != null)
                        {
                            EventLog.WriteEntry(options.MySqlExceptionMessage, EventLogEntryType.Error,
                                DEVICE_OPTION_REQUEST_EVENT_ID);
                            exception = true;
                        }
                        if (options.HttpRequestException != null)
                        {
                            EventLog.WriteEntry
                                (options.HttpRequestException.Message + "\n\n" +
                                options.HttpRequestException.StackTrace +
                                (options.HttpRequestException.InnerException != null ? "\n\nInner Exception:\n\n" +
                                options.HttpRequestException.InnerException.Message + "\n\n" +
                                options.HttpRequestException.InnerException.StackTrace : ""),
                                EventLogEntryType.Error, DEVICE_OPTION_REQUEST_EVENT_ID);
                            exception = true;
                        }
                        if (options.Exception != null)
                        {
                            EventLog.WriteEntry
                                 (options.Exception.Message + "\n\n" +
                                 options.Exception.StackTrace +
                                 (options.Exception.InnerException != null ? "\n\nInner Exception:\n\n" +
                                 options.Exception.InnerException.Message + "\n\n" +
                                 options.Exception.InnerException.StackTrace : ""),
                                 EventLogEntryType.Error, DEVICE_OPTION_REQUEST_EVENT_ID);
                            exception = true;
                        }

                        if (options.RequestSuccess && !exception)
                        {
                            this.fetchedSqlMonit = options.MonitorSql ? 1 : 0;
                            this.fetchedUseAgent = options.UseAgent;

                            if (Timer.Interval != options.ReportingInterval)
                            {
                                this.fetchedInterval = options.ReportingInterval;
                                this.Timer.Stop();
                                this.Timer.Interval = options.ReportingInterval; ;
                                this.Timer.Start();
                            }
                        }
                    }
                    requestOptions.Dispose();
                };
            }
            else
            {
                bool exception = false;

                MattimonAgentLibrary.Models.DeviceOptions options = RequestDeviceOptions(
                    fetchedDeviceId, fetchedUserId, fetchedCompanyId);

                if (options.MySqlExceptionMessage != null)
                {
                    EventLog.WriteEntry(options.MySqlExceptionMessage, EventLogEntryType.Error, 
                        DEVICE_OPTION_REQUEST_EVENT_ID);
                    exception = true;
                }
                if (options.HttpRequestException != null)
                {
                    EventLog.WriteEntry
                        (options.HttpRequestException.Message + "\n\n" + 
                        options.HttpRequestException.StackTrace + 
                        (options.HttpRequestException.InnerException != null ? "\n\nInner Exception:\n\n" +
                        options.HttpRequestException.InnerException.Message + "\n\n" +
                        options.HttpRequestException.InnerException.StackTrace : ""), 
                        EventLogEntryType.Error, DEVICE_OPTION_REQUEST_EVENT_ID);
                    exception = true;
                }
                if (options.Exception != null)
                {
                    EventLog.WriteEntry
                         (options.Exception.Message + "\n\n" +
                         options.Exception.StackTrace +
                         (options.Exception.InnerException != null ? "\n\nInner Exception:\n\n" +
                         options.Exception.InnerException.Message + "\n\n" +
                         options.Exception.InnerException.StackTrace : ""),
                         EventLogEntryType.Error, DEVICE_OPTION_REQUEST_EVENT_ID);
                    exception = true;
                }
            
                if (options.RequestSuccess && !exception)
                {
                    this.fetchedSqlMonit = options.MonitorSql ? 1 : 0;
                    this.fetchedUseAgent = options.UseAgent;

                    if (Timer.Interval != options.ReportingInterval)
                    {
                        this.fetchedInterval = options.ReportingInterval;
                        this.Timer.Stop();
                        this.Timer.Interval = options.ReportingInterval;
                        this.Timer.Start();
                    }
                }
            }
        }

        #region Update Updater
        /// <summary>
        /// 
        /// </summary>
        private String updSvcName = "MattimonUpdateService";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reqFilesize"></param>
        /// <param name="reqVersion"></param>
        /// <param name="updSvcVersion"></param>
        /// <returns></returns>
        private object DownloadBytes(out long reqFilesize, out string reqVersion, out string updSvcVersion)
        {
            Rest.RestClientRequests rest = new Rest.RestClientRequests();

            String updSvcFullpath = System.IO.Path.Combine(this.MattimonAppInstallDirectory,
                "MattimonUpdateService.exe");

            String updSvcExe = Path.Combine(this.MattimonAppInstallDirectory, "MattimonUpdateService.exe");
            updSvcVersion = new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                updSvcExe, true).GetAssemblyVersion().ToString();

            long fileSize = rest.RequestFilesize();
            string version = rest.RequestVersion();

            reqFilesize = fileSize;
            reqVersion = version;

            short sRequestedUpdaterVersion = AssemblyVersionToShort(version, true, out FormatException ex);
            short sUpdSvcVersion = AssemblyVersionToShort(updSvcVersion, false, out ex);

            EventLog.WriteEntry("DownloadBytes()\n\n" +
                "sRequestedUpdaterVersion: " + sRequestedUpdaterVersion + "\n" +
                "sUpdSvcVersion: " + sUpdSvcVersion, EventLogEntryType.Warning, UPD_DOWNLOAD_EVENT_ID);


            if (ex != null) { throw ex; }

            if (sRequestedUpdaterVersion <= sUpdSvcVersion)
                return null;

            byte[] zipBytes = rest.GetZipBytes();
            if (fileSize != zipBytes.Length)
                throw new Exception("Downloaded bytes don't correspond to the requested file size.\nThe source is maybe damaged.");

            return rest.GetZipBytes();
        }
        private bool DownloadZip(out byte[] zipBytes)
        {
            object data;
            long reqFilesize = 0;
            string reqVersion = "";
            zipBytes = null;
            string mainAppVersion = "";

            try
            {
                data = DownloadBytes(out reqFilesize, out reqVersion, out mainAppVersion);
                if (data == null)
                {
                    EventLog.WriteEntry("Checked for updater update: No update required.\n" +
                    "Marked version: " + reqVersion + "\n" +
                    "Mattimon Application Assembly version: " + mainAppVersion + "\n",
                    EventLogEntryType.Information, UPD_DOWNLOAD_EVENT_ID);

                    return false; // No update is required
                }
                zipBytes = (byte[])data;
                return true;
            }
            catch (FormatException ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error, UPD_DOWNLOAD_EVENT_ID);
                return false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Warning, UPD_DOWNLOAD_EVENT_ID);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopAgentService"></param>
        /// <returns></returns>
        private bool StopMattimonProcesses(Boolean stopAgentService = true)
        {
            String mainAppName = "MattimonAgentApplication";
            Boolean success = false;
            Process currentProc = null;
            String msg = "Stop running Processes:\n\n";
            try
            {
                foreach (var process in Process.GetProcessesByName(mainAppName))
                {
                    currentProc = process;
                    msg += process.ProcessName + " is being stopped.\n";
                    process.Kill();
                    msg += process.ProcessName + " stopped.\n";
                }

                success = true;
            }
            catch (Exception ex)
            {
                msg += "Failed to stop " + currentProc.ProcessName;
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(), EventLogEntryType.Error, UPD_DOWNLOAD_EVENT_ID);
            }

          
            if (stopAgentService)

                /// Stop the service only if is running
                if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) != MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Stopped)
                    MattimonAgentLibrary.Tools.MyServiceController.StopService(updSvcName);

            /// Check if it actually stopped 
            if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) != MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Stopped)
                success = false;

            EventLog.WriteEntry(msg, EventLogEntryType.Information, UPD_DOWNLOAD_EVENT_ID);
            return success;
        }
        private void DeployUpdateFiles(byte[] zipBytes)
        {
            EventLogEntryType eventLogEntryType = EventLogEntryType.Information;
            if (!StopMattimonProcesses())
            {
                EventLog.WriteEntry("Can't deploy the file as some resources are stil in use.",
                    EventLogEntryType.Warning, UPD_DOWNLOAD_EVENT_ID);
                return;
            }

            String message = "Creating zip file from downloaded bytes...\n";
            MemoryStream memoryStream = new MemoryStream(zipBytes);
            ZipArchive zipArchive = new ZipArchive(memoryStream);

            message += "----Content---\n";
            long totalbytes = 0;
            foreach (var zipEntry in zipArchive.Entries)
            {
                if (zipEntry.Length != 0)
                {
                    message += String.Format(String.Format("{0,-50} | {1} byte(s)\n", zipEntry.FullName, zipEntry.Length));
                }
                totalbytes += zipEntry.Length;
            }
            message += "----End----\n";

            foreach (var zipEntry in zipArchive.Entries)
            {

                // Open zip entry as stream
                Stream extractedFile = zipEntry.Open();

                // Convert stream to memory stream
                MemoryStream extractedMemoryStream = new MemoryStream();
                extractedFile.CopyTo(extractedMemoryStream);
            }



            String tmpDirPath = Path.Combine(this.MattimonAppInstallDirectory, "_tmp2");

            if (Directory.Exists(tmpDirPath))
                MattimonAgentLibrary.Tools.IOTools.DeleteDirectoryAndContent(tmpDirPath);

            DirectoryInfo tmpdir = Directory.CreateDirectory(tmpDirPath);

            bool extracted = false;
            try
            {
                message += "Extracting to " + tmpDirPath + "\n";
                zipArchive.ExtractToDirectory(tmpDirPath);
                message += "Extracted\n\n";
                extracted = true;
            }
            catch (Exception ex)
            {
                message += "\n\nExtraction failed.\nError details:\n" +
                    ex.Message + "\n\n";
            }

            if (extracted)
            {

                bool copied = false, tmpDeleted = false, success = false;
                message += "Copying tmp directory content to the active application directory\n";

                String tmpAgentDir = tmpDirPath + @"\MattimonUpdaterUpdate";

                try
                {

                    Tools.IOTools.DirectoryCopy(tmpAgentDir,
                        this.MattimonAppInstallDirectory, true);

                    copied = true;
                    Tools.IOTools.DeleteDirectoryAndContent(tmpDirPath);
                    tmpDeleted = true;
                    success = true;
                }
                catch (Exception ex)
                {
                    if (!copied && !tmpDeleted)
                    {
                        message += "\n\nFailed to copy tmp directory to the active.\n" +
                            "Failed to delete tmp directory.\n";
                        message += "Error details:\n" +
                            ex.Message + "\n\n";
                        eventLogEntryType = EventLogEntryType.Error;
                    }
                    else
                    {
                        if (!copied)
                        {
                            message += "\n\nFailed to copy tmp directory to the active.\n\n";
                            message += "Error details:\n" +
                                ex.Message + "\n\n";
                            eventLogEntryType = EventLogEntryType.Error;
                        }
                        if (!tmpDeleted)
                        {
                            message += "\n\nFailed to delete tmp directory.\n\n";
                            message += "Error details:\n" +
                                ex.Message + "\n\n";
                            eventLogEntryType = EventLogEntryType.Warning;
                        }
                    }
                }
                finally
                {

                    if (success)
                    {
                     
                        message += "Done\n\n";

                        if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Stopped)
                            MattimonAgentLibrary.Tools.MyServiceController.StartService(updSvcName);

                        if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(updSvcName) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running)
                        {
                            message += (updSvcName + " is now running.");
                        }
                        else
                        {
                            message += (updSvcName + " couldn't start");
                            eventLogEntryType = EventLogEntryType.Warning;
                        }
                    }
                    else
                    {
                        message += "Failed\n\n";
                        eventLogEntryType = EventLogEntryType.Error;
                    }

                    EventLog.WriteEntry(message, eventLogEntryType, UPD_DOWNLOAD_EVENT_ID);

                }

            } /// END: if (extracted)
        }

        private void CheckForUpdate()
        {
            if (!DownloadZip(out byte[] zipBytes))
                return;

            DeployUpdateFiles(zipBytes);
        }
        private short AssemblyVersionToShort(String assemVersionString, Boolean fromFile, out FormatException e)
        {
            try
            {
                e = null;
                String[] nums = assemVersionString.Split('.');
                String strnum = "";
                foreach (String num in nums)
                    strnum += num;
                return Convert.ToInt16(strnum);
            }
            catch (FormatException ex)
            {
                e = ex;
                EventLog.WriteEntry("AssemblyVersionToShort [FormatException]\n\n" + e.Message + "\n\n" +
                    "String source: " + (fromFile ? " Assembly File" : "Request") + "\n" +
                    "String value was: " + assemVersionString,
                    EventLogEntryType.Warning, UPD_DOWNLOAD_EVENT_ID);

                return -1;
            }
        }
        #endregion

        #region Service Events
        protected override void OnShutdown()
        {
            base.OnShutdown();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry(ServiceName + " started.");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("OnStart()\n\n" + ex.ToString(), EventLogEntryType.Error, SVC_INIT_EVENT_ID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                Timer.Stop();
                TimerUpd.Stop();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("OnStop()\n\n" + ex.ToString(), EventLogEntryType.Error, SVC_INIT_EVENT_ID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            EventLog.WriteEntry("Timer_Elapsed called.", EventLogEntryType.Information, MattimonAgentService.ON_TIMER_EVENT_ID);
            string msg = "Timer_Elapsed():\n\n";
            int eventid = MattimonAgentService.ON_TIMER_EVENT_ID + 1;
            EventLogEntryType eventLogEntryType = EventLogEntryType.Information;

            InitializeEventLog(); // Install the event log (again) if it was deleted by the user
            
            if (this.fetchedUseAgent)
            {
                msg += "Agent reporting is enabled.\n\n";
                UpdateDeviceStatuses();
            }
            else
            {
                msg += "Agent reporting is disabled by the client.\n\n";
            }


            /// Always do these checks in the end (currently disabled/commented!)
            /// /!\ CheckOnlineOptions have been moved to the constructor
            // bool useBackgroundWorker = false;
            // CheckOnlineOptions(useBackgroundWorker);
            /// NOTE: each time the user changes related options, the service needs to be stopped and restarted
            /// in order for the options to take effect.

            EventLog.WriteEntry(msg, eventLogEntryType, eventid);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerUpd_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            EventLog.WriteEntry("TimerUpd_Elapsed()\n\n",
                EventLogEntryType.Information, UPD_TIMER_EVENT_ID);

            try
            {
                CheckForUpdate();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Timer_Elapsed()\n\n" + ex.Message + "\n\n" + ex.ToString(),
                    EventLogEntryType.Error, UPD_DOWNLOAD_EVENT_ID);
            }
        }

        

        #endregion

        #region Validation Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean Validate()
        {
            if (!APIResponds())
                return false;

            if (!ApplicationRegistryKeyExists())
                return false;

            if (!DatabaseFileExists())
                return false;

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean MattimonAPIWebRequestReplies()
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create(
                MattimonAgentLibrary.Rest.Constants.MATTIMON_WEB_APP_URL + "//" +
                MattimonAgentLibrary.Rest.Constants.API_INFO_DIR);

            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean MattimonWebApiReplies()
        {
            MattimonAgentLibrary.Rest.APIInfoRequest aPIInfoRequest = new MattimonAgentLibrary.Rest.APIInfoRequest();
            MattimonAgentLibrary.Models.APIInfo info = aPIInfoRequest.GetAPIInfo();
            if (info.HttpStatusCode != System.Net.HttpStatusCode.OK)
                return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool APIResponds()
        {
            try
            {
                if (!MattimonWebApiReplies() || !MattimonAPIWebRequestReplies())
                {
                    EventLog.WriteEntry("Unable to communicate with the Mattimon Web API.", EventLogEntryType.Warning, 500);
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ApplicationRegistryKeyExists()
        {
            bool keyExist = MattimonAgentLibrary.Tools.RegistryTools.IsSoftwareInstalled(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName);

            if (!keyExist)
                EventLog.WriteEntry("Application is not found in the registry.", EventLogEntryType.Warning, 500);

            return keyExist;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private bool DatabaseFileExists()
        {
            if (!SQLiteClientDatabase.FileExists(out string foundDatabaseFilePath))
            {
                EventLog.WriteEntry("Local database filename is not longer found.", EventLogEntryType.Warning, 500);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Rest Request
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private bool DeviceEntryExist(long deviceId)
        {
            MattimonAgentLibrary.Rest.DeviceRequests deviceRequests = new MattimonAgentLibrary.Rest.DeviceRequests();
            Boolean deviceIdExisting = deviceRequests.DeviceEntryExist(deviceId);

            if (!deviceIdExisting)
            {
                EventLog.WriteEntry("Cannot find device entry on the server. Device ID: " + deviceId, EventLogEntryType.Warning, 500);
                return false;
            }

            return true;
        }
        #endregion

        #region Rest
        private WMIService WMIService;
        /// <summary>
        /// 
        /// </summary>
        private void UpdateDeviceStatuses()
        {
            try
            {
                WMIService = new WMIService(EventLog);
                
                MattimonAgentLibrary.Models.Device scanDevice = WMIService.Scan();
                string versAssignmentMessage = string.Empty;
                EventLogEntryType entrytype = EventLogEntryType.Information;

                scanDevice.AgentVersion = this.agentversion;
                scanDevice.AgentServiceVersion = this.agentServiceVersion;
                scanDevice.UpdateServiceVersion = this.updateServiceVersion;
                scanDevice.MattimonSQLServiceVersion = this.mattimonSQLServiceVersion;
                scanDevice.LibraryVersion = this.libraryVersion;

                EventLog.WriteEntry(versAssignmentMessage, entrytype, DEVICE_UPD_EVENT_ID + 1);

                /* set keys */
                scanDevice.Company_Id = fetchedCompanyId;
                scanDevice.User_Id = fetchedUserId;
                scanDevice.Device_Id = fetchedDeviceId;
                scanDevice.Device_Type_Id = fetchedDeviceTypeId;
                /* end set keys */

                EventLog.WriteEntry("Update Device statuses: \n\n" +
                    "device_id: " + scanDevice.Device_Id + "\n" +
                    "device_type_id: " + scanDevice.Device_Type_Id + "\n" +
                    "user_id: " + scanDevice.User_Id + "\n" + "" +
                    "company_id: " + scanDevice.Company_Id + "\n" +
                    "agent version: " + scanDevice.AgentVersion, 
                    EventLogEntryType.Information, DEVICE_UPD_EVENT_ID);

                MattimonAgentLibrary.Models.Device returnedDevice = deviceRequests.UpdateDeviceEntry(scanDevice);
                scanDevice = null;

                if (returnedDevice.TaskCanceledException != null)
                {
                    EventLog.WriteEntry("UpdateDeviceStatuses TaskCanceledException\n\n" + returnedDevice.TaskCanceledException.ToString(), EventLogEntryType.Error, DEVICE_UPD_EVENT_ID);
                    return;
                }
                if (returnedDevice.HttpRequestException != null)
                {
                    EventLog.WriteEntry("UpdateDeviceStatuses HttpRequestException\n\n" + returnedDevice.HttpRequestException.ToString(), EventLogEntryType.Error, DEVICE_UPD_EVENT_ID);
                    //MattimonAgentLibrary.Tools.MyServiceController.StopService(ServiceName);
                    return;
                }
                if (returnedDevice.MySqlExceptionMessage != null)
                {
                    EventLog.WriteEntry("UpdateDeviceStatuses MySqlException\n\n" + returnedDevice.MySqlExceptionMessage, EventLogEntryType.Error, DEVICE_UPD_EVENT_ID);
                    //MattimonAgentLibrary.Tools.MyServiceController.StopService(ServiceName);
                    return;
                }
                if (returnedDevice.Exception != null)
                {
                    EventLog.WriteEntry("UpdateDeviceStatuses Exception\n\n" + returnedDevice.Exception.ToString(), EventLogEntryType.Error, DEVICE_UPD_EVENT_ID);
                    //MattimonAgentLibrary.Tools.MyServiceController.StopService(ServiceName);
                    return;
                }
                if (returnedDevice.RequestSuccess)
                {
                    EventLog.WriteEntry("UpdateDeviceStatuses Success\n\n",
                        EventLogEntryType.Information, DEVICE_UPD_EVENT_ID);
                    return;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(), EventLogEntryType.Error, DEVICE_UPD_EVENT_ID);
                if (MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(ServiceName) ==
                    MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running)
                {
                    MattimonAgentLibrary.Tools.MyServiceController.StopService(ServiceName);
                }
            }
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Path of this service exe
        /// </summary>
        private string thisSvcExePath;
        /// <summary>
        /// Path of the gui application exe
        /// </summary>
        private string guiExePath;
        /// <summary>
        /// Reference of this assembly attributes
        /// </summary>
        private MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes thisSvcAssemAttr;
        /// <summary>
        /// Directory which the entire application was installed
        /// </summary>
        private String MattimonAppInstallDirectory;
        /// <summary>
        /// Service Timer
        /// </summary>
        private System.Timers.Timer Timer, TimerUpd;
        /// <summary>
        /// 
        /// </summary>
        private MattimonAgentLibrary.Rest.DeviceRequests deviceRequests;
        /// <summary>
        /// 
        /// </summary>
        private MattimonSQLite.SQLiteClientDatabase SQLiteClientDatabase;
        /// <summary>
        /// 
        /// </summary>
        private long fetchedUserId, fetchedDeviceId, fetchedDeviceTypeId, fetchedCompanyId;
        /// <summary>
        /// 
        /// </summary>
        private string fetchedAgentId;
        /// <summary>
        /// 
        /// </summary>
        private bool fetchedUseAgent;
        /// <summary>
        /// 
        /// </summary>
        private double fetchedInterval;
        /// <summary>
        /// 
        /// </summary>
        private int fetchedSqlMonit;
        #endregion

        #region Constants
        public const int SVC_INIT_EVENT_ID = 100;

        public const int ON_TIMER_EVENT_ID = 200;
        public const int DEVICE_UPD_EVENT_ID = 300;
        public const int DEVICE_SQLSRV_UPD_EVENT_ID = 310;
        public const int DEVICE_OPTION_REQUEST_EVENT_ID = 800;
        public const int DEVICE_SQLSERVEROBJECT_EVENTID = 900;

        public const int UPD_TIMER_EVENT_ID = 250;
        public const int UPD_DOWNLOAD_EVENT_ID = 350;
        #endregion
    }    
}
