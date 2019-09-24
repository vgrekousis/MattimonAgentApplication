using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MattimonUpdateService
{
    public partial class MattimonUpdateService : ServiceBase
    {
        public MattimonUpdateService()
        {
            InitializeComponent();
            ServiceName = this.GetType().ToString();
            InitializeServiceComponents();
        }

        #region Core
        /// <summary>
        /// Returns a byte array containing the requested zip file.
        /// Returns <code>null</code> if the requested version is the same as the Mattimon Application Assembly version.
        /// Throws an <code>Exception</code> if the requested file size does not correspond with the downloaded file size
        /// which may mean that the requested file is damaged.
        /// </summary>
        /// <returns>*As shown in decription*</returns>
        private object DownloadBytes(out long reqFilesize, out string reqVersion, out string mainAppVersion)
        {
            Tools.RestClientRequests rest = new Tools.RestClientRequests();

            String agentSvcFullpath = System.IO.Path.Combine(this.MattimonAppInstallDirectory, 
                "MattimonAgentService.exe");

            String mainAppExe = Path.Combine(this.MattimonAppInstallDirectory, "MattimonAgentApplication.exe");
            mainAppVersion = new Tools.ProjectAssemblyAtrributes(
                mainAppExe, true).GetAssemblyVersion().ToString(); // System.Reflection.Assembly.LoadFrom(mainAppExe).GetName().Version.ToString();
            
            long fileSize = rest.RequestFilesize();
            string version = rest.RequestVersion();

            reqFilesize = fileSize;
            reqVersion = version;

            short sRequestedVersion = AssemblyVersionToShort(version, true);
            short sMainAppVersion = AssemblyVersionToShort(mainAppVersion, false);

            EventLog.WriteEntry("DownloadBytes()\n\n" +
                "sRequestedVersion: " + sRequestedVersion + "\n" +
                "sMainAppVersion: " + sMainAppVersion, EventLogEntryType.Warning, FILE_DOWNLOAD_EVENT_ID);

            if (sRequestedVersion == -1 || sMainAppVersion == -1)
            {
                EventLog.WriteEntry("An error occurred in method AssemblyVersionToShort. View event id " + (FILE_DOWNLOAD_EVENT_ID - 1) + " for details");
                return null;
            }
                


            if (sRequestedVersion <= sMainAppVersion)
                return null;

            byte[] zipBytes = rest.GetZipBytes();
            if (fileSize != zipBytes.Length)
                throw new Exception("Downloaded bytes don't correspond to the requested file size.\nThe source is maybe damaged.");

            EventLog.WriteEntry("DownloadBytes\n\nReturning zipBytes to caller. Length:" + fileSize, EventLogEntryType.Information, 901);
                

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
                    EventLog.WriteEntry("Checked for update: No update required.\n" +
                    "Marked version: " + reqVersion + "\n" +
                    "Mattimon Application Assembly version: " + mainAppVersion + "\n", 
                    EventLogEntryType.Information, FILE_DOWNLOAD_EVENT_ID);

                    return false; // No update is required
                }
                zipBytes = (byte[])data;
                return true;
            }
            catch (FormatException ex)
            {
                EventLog.WriteEntry("DownloadZip(out byte[] zipBytes) handled a FormatException:\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n\n\nDump:\n" +
                    "", 
                    EventLogEntryType.Error, FILE_DOWNLOAD_EVENT_ID);
                return false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("DownloadZip(out byte[] zipBytes) handled an Exception:\n\n" + ex.Message + "\n\n" + ex.StackTrace, EventLogEntryType.Warning, FILE_DOWNLOAD_EVENT_ID);
                return false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        // bool bAgentSvcWasStopped;
        /// <summary>
        /// 
        /// </summary>
        private readonly string agentSvcName = "MattimonAgentService";
        private readonly string evtlogSvcName = "MattimonEventLogService";
        private readonly string mattimonSqlSvcName = "MattimonSQLServerService";
        /// <summary>
        /// Returns <code>true</code> if no kill was required or if all running processes killed.
        /// Returns <code>false</code> in the case where not (or at all) required processes where killed.
        /// </summary>
        /// <param name="killerrors">Will hold all errors related to the kill action.</param>
        /// <param name="unkilled">Will hold all processes that failed the kill process where the key represents the process id and the value the process name</param>
        /// <param name="directoryName">The directory that contains the runnable processes</param>
        /// <param name="exclusions">Process names that should be exluced, i.e processes that must not be killed. Leave it to <code>null</code> to simply kill all processes available in the directory.</param>
        /// <returns></returns>
        private bool KillProcessesFromDirectory(out List<Exception> killerrors, out Dictionary<int, string> killed, out Dictionary<int, string> unkilled, out bool actionRequired, string directoryName, params string[] exclusions)
        {
            string message = "KillProcessesFromDirectory()\n";
            message += "============================";
            EventLogEntryType evtlogentrytype = EventLogEntryType.Information;

            killerrors = null;
            killed = null;
            unkilled = null;
            actionRequired = false;
            int tokill = 0;

            Dictionary<int, string> inax = new Dictionary<int, string>();
            Dictionary<int, string> inva = new Dictionary<int, string>();
            Dictionary<int, string> exce = new Dictionary<int, string>();
            
            var processes = Process.GetProcesses();
            string[] names1 = processes.ToList().Select(p => p.ProcessName).ToArray();
            string[] names2 = exclusions;
            IEnumerable<string> excluded = names1.Except(names2);

            foreach (string processName in excluded)
            {
                Process[] byNameProcesses = Process.GetProcessesByName(processName);
                foreach (Process p in byNameProcesses)
                {
                    try
                    {
                        directoryName = directoryName.Trim('\\', '/');
                        if (Path.GetDirectoryName(p.MainModule.FileName).Equals(directoryName))
                        {
                            actionRequired = true;
                            tokill++;
                            int killid = p.Id;
                            string killname = p.ProcessName;

                            try
                            {
                                message += "stopping: " + killid + " - " + killname + "\n"; 
                                p.Kill();
                                message += "done\n\n";
                                if (killed == null) killed = new Dictionary<int, string>();
                                killed.Add(killid, killname);
                            }
                            catch (Win32Exception e)
                            {
                                message += "failed (Win32Exception)" + e.Message + "\n\n";
                                if (unkilled == null) unkilled = new Dictionary<int, string>();
                                if (killerrors == null) killerrors = new List<Exception>();
                                killerrors.Add(e);
                                unkilled.Add(killid, killname);
                            }
                            catch (NotSupportedException e)
                            {
                                message += "failed (NotSupportedException)" + e.Message + "\n\n";
                                if (unkilled == null) unkilled = new Dictionary<int, string>();
                                if (killerrors == null) killerrors = new List<Exception>();
                                killerrors.Add(e);
                                unkilled.Add(killid, killname);
                            }
                            catch (InvalidOperationException e)
                            {
                                message += "failed (InvalidOperationException)" + e.Message + "\n\n";
                                if (unkilled == null) unkilled = new Dictionary<int, string>();
                                if (killerrors == null) killerrors = new List<Exception>();
                                killerrors.Add(e);
                                unkilled.Add(killid, killname);
                            }
                        }
                    }
                    catch (Win32Exception)
                    {
                        inax.Add(p.Id, p.ProcessName);
                    }
                    catch (InvalidOperationException)
                    {
                        inva.Add(p.Id, p.ProcessName);
                    }
                    catch (Exception)
                    {
                        exce.Add(p.Id, p.ProcessName);
                    }
                }
            }

            if (actionRequired)
                EventLog.WriteEntry(message, evtlogentrytype, KILL_ACTIVE_PROCESSES_EVENT_ID);
            else
                EventLog.WriteEntry("KillProcessesFromDirectory: action not required.", evtlogentrytype, KILL_ACTIVE_PROCESSES_EVENT_ID);

            return actionRequired ? killed.Count == tokill : true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        /// <param name="exception"></param>
        /// <returns>Returns <code>true</code> if the service was stopped or not currently installed</returns>
        private bool StopMattimonService(string svcName, out Exception exception)
        {
            exception = null;
            try
            {
                if (Tools.MyServiceController.ServiceIsInstalled(svcName))
                {

                    if (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                    {
                        Tools.MyServiceController.StopService(svcName);

                        // New instruction: remove is not succesfull
                        while (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                        {
                            // Block
                        }
                    }
                        

                    return Tools.MyServiceController.GetServiceStatus(svcName) == Tools.MyServiceController.ServiceState.Stopped;
                }

                // return true to assume the service as 'stopped' if it's not installed.
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        /// <param name="exception"></param>
        /// <returns>Returns <code>true</code> if the service was stopped or not currently installed</returns>
        private bool StartMattimonService(string svcName, out bool isInstalled, out Exception exception)
        {
            exception = null;
            isInstalled = true;

            try
            {
                if (Tools.MyServiceController.ServiceIsInstalled(svcName))
                {
                    if (Tools.MyServiceController.GetServiceStatus(svcName) == Tools.MyServiceController.ServiceState.Stopped)
                        Tools.MyServiceController.StartService(svcName);

                    return Tools.MyServiceController.GetServiceStatus(svcName) == Tools.MyServiceController.ServiceState.Running;
                }

                isInstalled = false;
                return false;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipBytes"></param>
        private void DeployUpdateFiles(byte[] zipBytes)
        {
            string message = "DeployUpdateFiles(byte[]zip)\n";
            message += "\n" + new string('=', message.Length) + "\n\n";
            EventLogEntryType eventLogEntryType = EventLogEntryType.Information;

            string dirname = Tools.RegistryTools.GetInstallLocationByDisplayName(
                Static.MattimonRegistrySubkeyNames.DisplayName);
            DirectoryInfo dir = new DirectoryInfo(dirname);

            bool stopped = StopMattimonService(agentSvcName, out Exception stopSvcException);
            if (!stopped)
            {
                if (stopSvcException != null)
                {
                    message += "\nCould not stop " + agentSvcName + ".\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(stopSvcException);
                }

                message += "\n" + agentSvcName + " was not stopped. Deploy update aborted.";

                // log the information
                EventLog.WriteEntry(message, EventLogEntryType.Error, FILE_DEPLOY_EVENT_ID);

                // abort
                return;
            }
            else
            {
                message += "\n" + agentSvcName + " stopped.";
            }
           
            stopped = StopMattimonService(mattimonSqlSvcName, out stopSvcException);
            if (!stopped)
            {
                if (stopSvcException != null)
                {
                    message += "\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(stopSvcException);
                }

                message += "\n" + mattimonSqlSvcName + " was not stopped. Deploy update aborted.";


                // before aborting, try and re-start the agent service
                bool mainStarted = StartMattimonService(agentSvcName, out bool isMainInstalled, out Exception svcStartException);
                if (mainStarted)
                {
                    message += "\n" + agentSvcName + " has started.";
                }
                if (svcStartException != null)
                {
                    message += "\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(svcStartException);
                }

                if (!isMainInstalled)
                {
                    message += "\n(!)" + agentSvcName + " is not installed (!)";
                }

                // log the information
                EventLog.WriteEntry(message, EventLogEntryType.Error, FILE_DEPLOY_EVENT_ID);

                // abort
                return;
            }
            else
            {
                message += "\n" + mattimonSqlSvcName + " have been stopped.\n" +
                    "Just a reminder: This service may have actually not been installed previously.";
            }


            stopped = StopMattimonService(evtlogSvcName, out stopSvcException);
            if (!stopped)
            {
                if (stopSvcException != null)
                {
                    message += "\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(stopSvcException);
                    EventLog.WriteEntry(message, EventLogEntryType.Error, FILE_DEPLOY_EVENT_ID);
                }

                message += "\n" + evtlogSvcName + " was not stopped. Deploy update aborted.";


                // before aborting, try and re-start the agent service
                bool mainStarted = StartMattimonService(agentSvcName, out bool installed, out Exception svcStartException);
                if (mainStarted)
                {
                    message += "\n" + agentSvcName + " has started.";
                }
                if (svcStartException != null)
                {
                    message += "\n" + agentSvcName + " failed to start.\n + " + Tools.ExceptionHelper.GetFormatedExceptionMessage(svcStartException);
                }

                // log the information
                EventLog.WriteEntry(message, EventLogEntryType.Warning, FILE_DEPLOY_EVENT_ID);

                // abort
                return;
            }
            else
            {
                message += "\n" + evtlogSvcName + " have been stopped.\n" +
                    "Just a reminder: This service may have actually not been installed previously.";
            }


            message += "\nChecking related mattimon processes...";
            bool atLeastOne = false;
            // Stop the GUI if running
            foreach (var process in Process.GetProcessesByName("MattimonAgentApplication"))
            {
                message += "\nProcess " + process.ProcessName + " is running. Stopping...";
                process.Kill();
                message += "\nProcess " + process.ProcessName + " stopped.";
                atLeastOne = true;
            }
        
            if (!atLeastOne)
                message += "\n" + "No related processes seem to be running. Continuing update process.";
            


            message += "\nCreating zip file from downloaded bytes...";
            MemoryStream memoryStream = new MemoryStream(zipBytes);
            ZipArchive zipArchive = new ZipArchive(memoryStream);

            message += "\n----Content---";
            long totalbytes = 0;
            foreach (var zipEntry in zipArchive.Entries)
            {
                if (zipEntry.Length != 0)
                {
                    message += "\n" + String.Format(String.Format("{0,-50} | {1} byte(s)\n", zipEntry.FullName, zipEntry.Length));
                }
                totalbytes += zipEntry.Length;
            }
            message += "\n----End----";

            foreach (var zipEntry in zipArchive.Entries)
            {

                // Open zip entry as stream
                Stream extractedFile = zipEntry.Open();

                // Convert stream to memory stream
                MemoryStream extractedMemoryStream = new MemoryStream();
                extractedFile.CopyTo(extractedMemoryStream);
            }



            string tmpDirPath = Path.Combine(this.MattimonAppInstallDirectory, "_tmp");

            if (Directory.Exists(tmpDirPath))
                Tools.IOTools.DeleteDirectoryAndContent(tmpDirPath);

            DirectoryInfo tmpdir = Directory.CreateDirectory(tmpDirPath);

            bool extracted = false;
            try
            {
                message += "\nExtracting to " + tmpDirPath;
                zipArchive.ExtractToDirectory(tmpDirPath);
                message += "\nExtracted\n\n";
                extracted = true;
            }
            catch (Exception ex)
            {
                message += "\n\nExtraction failed.\nError details:\n" +
                    ex.Message + "\n\n";
            }

            if (extracted)
            {

                bool copied = false, tmpDeleted = false;
                message += "\nCopying tmp directory content to the active application directory";

                string tmpAgentDir = tmpDirPath + @"\MattimonAgentUpdate";

                try
                {

                    Tools.IOTools.DirectoryCopy(tmpAgentDir,
                        this.MattimonAppInstallDirectory, true);
                    copied = true;
                }
                catch (Exception ex)
                {
                    message += "\n\nFailed to copy _tmp directory contents to the active: " + ex.Message;
                    eventLogEntryType = EventLogEntryType.Error;
                }
                try
                {

                    Tools.IOTools.DeleteDirectoryAndContent(tmpDirPath);
                    tmpDeleted = true;
                }
                catch (Exception ex)
                {
                    message += "\n\nFailed to delete tmp directory: " + ex.Message;
                    eventLogEntryType = EventLogEntryType.Warning;
                }
                finally
                {
                    if (copied)
                    {
                        message += "\nUpdate of the DisplayVersion key in the registry.";
                        try
                        {
                            /// Update DisplayVersion key of the program in the registry, based on the main application's executable.
                            Tools.RegistryTools.UpdateDisplayVersion(Static.MattimonRegistrySubkeyNames.DisplayName,
                                new Tools.ProjectAssemblyAtrributes(
                                    Path.Combine(this.MattimonAppInstallDirectory, "MattimonAgentApplication.exe")).GetAssemblyVersion().ToString());
                        }
                        catch (Exception ex)
                        {
                            message += "\n\nCould not update display version.\nError details:\n" + ex.ToString() + "\n\n";
                            eventLogEntryType = EventLogEntryType.Warning;
                        }
                    }
                    else
                    {
                        message += "\nFailed\n\n";
                        eventLogEntryType = EventLogEntryType.Error;
                    }

                    if (tmpDeleted)
                    {
                        message += "\nTemporary folder deleted.\n";
                    }
                }

            } /// END: if (extracted)

            bool started = StartMattimonService(agentSvcName, out bool isInstalled, out Exception startSvcException);
            if (startSvcException != null)
            {
                if (startSvcException != null)
                {
                    message += "Failed to start " + agentSvcName + ".\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(startSvcException) + "\n" +
                        "Start Mattimon Services aborted";

                    // log information before aborting
                    EventLog.WriteEntry(message, EventLogEntryType.Warning, FILE_DEPLOY_EVENT_ID);

                    // abort
                    return;
                }
            }

            // if the main service started
            if (started)
            {
                // start start the event log service
                started = StartMattimonService(evtlogSvcName, out isInstalled, out startSvcException);

                // if an exception occurred
                if (startSvcException != null)
                {
                    // set the exception message and continue with the rest
                    message += "Failed to start " + evtlogSvcName + ".\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(startSvcException);
                }

                // if event log service is not installed
                if (!isInstalled)

                    // set the message and continue with the rest
                    message += "\n" + evtlogSvcName + " is not installed.";

                // continue with mattimon sql service start
                // reset the exception to null
                startSvcException = null;

                // start the mattimon sql service
                started = StartMattimonService(mattimonSqlSvcName, out isInstalled, out startSvcException);

                // if an exception occurred
                if (startSvcException != null)
                {
                    // set the exception message and continue with the rest
                    message += "Failed to start " + mattimonSqlSvcName + ".\n" + Tools.ExceptionHelper.GetFormatedExceptionMessage(startSvcException);
                }

                // if the mattimon sql service is not installed
                if (!isInstalled)

                    // set the message and continue with the rest
                    message += "\n" + mattimonSqlSvcName + " is not installed.";
            }

            // Show details in the event log viewer.
            EventLog.WriteEntry(message, eventLogEntryType, FILE_DEPLOY_EVENT_ID);
        }
        
        private void CheckForUpdate()
        {
            string message = GetType() + ".CheckForUpdate()\n";
            message += "=====================================";

            if (!DownloadZip(out byte[] zipBytes))
            {
                message += "\nDownloadZip(out byte[]) returned false";
                EventLog.WriteEntry(message, EventLogEntryType.Information, FILE_DOWNLOAD_EVENT_ID);
                return;
            }

            message += "\nDownloadZip(out byte[]) returned true";
            DeployUpdateFiles(zipBytes);
            message += "\nDeployUpdateFiles(byte[]) accomplished";
            EventLog.WriteEntry(message, EventLogEntryType.Information, FILE_DOWNLOAD_EVENT_ID);

        }
        private short AssemblyVersionToShort(string assemVersionString, bool fromFile)
        {
            try
            {
                string[] nums = assemVersionString.Split('.');
                string strnum = "";
                foreach (String num in nums)
                    strnum += num;
                return Convert.ToInt16(strnum);
            }
            catch (FormatException ex)
            {
                EventLog.WriteEntry("AssemblyVersionToShort [FormatException]\n\n" + ex.Message + "\n\n" +
                    "String source: " + (fromFile ? " Assembly File" : "Request") + "\n" +
                    "String value was: " + assemVersionString,
                    EventLogEntryType.Warning, (FILE_DOWNLOAD_EVENT_ID - 1));

                return -1;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("AssemblyVersionToShort [Exception]\n\n" + ex.Message + "\n\n" +
                    "String source: " + (fromFile ? " Assembly File" : "Request") + "\n" +
                    "String value was: " + assemVersionString,
                    EventLogEntryType.Warning, (FILE_DOWNLOAD_EVENT_ID - 1));

                return -1;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes required paths for this service
        /// and returns the Assembly Title of this Assembly
        /// </summary>
        /// <returns></returns>
        private String InitializePaths()
        {
            try
            {
                this.MattimonAppInstallDirectory =
                       Tools.RegistryTools.GetInstallLocationByDisplayName(
                           Static.MattimonRegistrySubkeyNames.DisplayName);

                this.mainSvcExePath = System.IO.Path.Combine(
                    this.MattimonAppInstallDirectory, "MattimonAgentService.exe");


                this.guiExePath = System.IO.Path.Combine(
                    this.MattimonAppInstallDirectory, "MattimonAgentApplication.exe");

                this.thisSvcExePath = System.IO.Path.Combine(
                    this.MattimonAppInstallDirectory, "MattimonUpdateService.exe");

                this.thisSvcAssemAttr =
                   new Tools.ProjectAssemblyAtrributes(
                   this.thisSvcExePath);

                return this.thisSvcAssemAttr.AssemblyTitle;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("InitializePaths()\n\n" + ex.ToString(), EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                return "MattimonUpdateService";
            }
        }
        /// <summary>
        /// Installs (Or re-installs) the event log if it does not exist
        /// </summary>
        private void InitializeEventLog()
        {
            String src = Static.MattimonEventLogConstants.UpdaterSourceName;
            String log = Static.MattimonEventLogConstants.MainLogName;
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
        private void InitializeServiceComponents()
        {
            try
            {
                InitializeEventLog();

            } catch{ }

            try
            {
                InitializePaths();

                if (!Tools.IOTools.IsSoftwareInstalled(
                    Static.MattimonRegistrySubkeyNames.DisplayName))
                {
                    EventLog.WriteEntry("Service started but the application is not installed.\n\n" +
                        "Application must be properly installed in order to access required assembly information and paths.",
                        EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                    return;
                }

                String publisher = Tools.RegistryTools.GetPublisherByDisplayName(
                    Static.MattimonRegistrySubkeyNames.DisplayName);
                

                if (!System.IO.Directory.Exists(this.MattimonAppInstallDirectory))
                {
                    EventLog.WriteEntry("Service started but the application directory isn't found.\n\n" +
                       "Application must be properly installed in order to access required assembly information and paths.",
                       EventLogEntryType.Error, SVC_INIT_EVENT_ID);
                    return;
                }

               
                String svcPathName = "";

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
                       EventLogEntryType.Error, 100);
                    return;
                }

               

                /// Get the source path of this executable (Win32_Services->PathName)
                svcPathName = Tools.MyServiceController.GetWin32ServicePathName(
                    this.ServiceName);

                String message = "";
                message = String.Format(

                    "{0}->InitializeServiceComponents\n\n" +
                    "Application path: {1}\n" +
                    "Service path: {2}\n" +
                    "Service path (Win32_Services->PathName): {3}\n\n" +
                    
                    "MattimonUpdateService.exe assembly information:\n\n" +
                    "Title: {4}\n" +
                    "Company: {5}\n" +
                    "Product: {6}\n" +
                    "Version: {7}\n\n" +
                    
                    "CommonAppData directory: {8}",

                    ServiceName,
                    this.guiExePath,
                    this.thisSvcExePath,
                    svcPathName,

                    this.thisSvcAssemAttr.AssemblyTitle,
                    this.thisSvcAssemAttr.AssemblyCompany,
                    this.thisSvcAssemAttr.AssemblyProduct,
                    this.thisSvcAssemAttr.GetAssemblyVersion(),

                    Static.Constants.CommonAppData
                );

                EventLog.WriteEntry(message, EventLogEntryType.Information, SVC_INIT_EVENT_ID);

                /// Finally, initialize the timer
                Timer = new System.Timers.Timer
                {
                    Interval = 1000 * 60 * 1
                };
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                    EventLogEntryType.Error, SVC_INIT_EVENT_ID);

                if (Tools.MyServiceController.GetServiceStatus(ServiceName) == 
                    Tools.MyServiceController.ServiceState.Running)
                {
                    Tools.MyServiceController.StopService(ServiceName);
                }
            }
        }
       
        private Boolean CheckAutomatic(String svcName)
        {

            if (Tools.MyServiceController.GetServiceStart(svcName) != Tools.MyServiceController.ServiceStart.Automatic)
            {
                EventLog.WriteEntry("Service start was previously changed to " +
                    Tools.MyServiceController.GetServiceStart(svcName) + "\nWill revert service start to Automatic.\nSsrvice will stop and re-apply service start.",
                    EventLogEntryType.Information, 10);

               
                Tools.MyServiceController.SetServiceStart(svcName, Tools.MyServiceController.ServiceStart.Automatic);

                if (Tools.MyServiceController.GetServiceStart(svcName) == Tools.MyServiceController.ServiceStart.Automatic)
                {
                    EventLog.WriteEntry("Service have been succesfully changed to Automatic. Service will now start.",
                        EventLogEntryType.Information, 10);
                    return true;
                }

                return false;
            }
            return true;
        }
        #endregion

        #region Service Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(ServiceName + " started.");
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            InitializeEventLog(); // Install the event log (again) if it has been deleted

            EventLog.WriteEntry(ServiceName + " On Timer.", 
                EventLogEntryType.Information, ON_TIMER_EVENT_ID);

            try
            {
                CheckForUpdate();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Timer_Elapsed()\n\n" + ex.Message + "\n\n" + ex.ToString(), 
                    EventLogEntryType.Error, FILE_DOWNLOAD_EVENT_ID);
            }
        }
        protected override void OnStop()
        {
            EventLog.WriteEntry(ServiceName + " stopped.");
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Path of this service exe
        /// </summary>
        private string thisSvcExePath;
        /// <summary>
        /// Path of the main service (agent service) exe
        /// </summary>
        private string mainSvcExePath;
        /// <summary>
        /// Path of the gui application exe
        /// </summary>
        private string guiExePath;
        /// <summary>
        /// Install path -- Provided by registry key 'DisplayName'
        /// </summary>
        private String MattimonAppInstallDirectory;
        /// <summary>
        /// Timer
        /// </summary>
        private System.Timers.Timer Timer;
        /// <summary>
        /// Mattimon Update Service Assembly Attributes
        /// </summary>
        private Tools.ProjectAssemblyAtrributes thisSvcAssemAttr;
        #endregion

        #region Constants
        private const int SVC_INIT_EVENT_ID = 1000;
        private const int ON_TIMER_EVENT_ID = 2000;
        private const int FILE_DOWNLOAD_EVENT_ID = 3000;
        private const int FILE_DEPLOY_EVENT_ID = 4000;
        private const int KILL_ACTIVE_PROCESSES_EVENT_ID = 5000;
        #endregion
    }
}
