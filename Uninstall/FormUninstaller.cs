using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Uninstall.Rest.Models;

namespace Uninstall
{
    public partial class FormUninstaller : Form
    {

        #region Windows API
        [Flags]
        enum MoveFileFlags
        {

            MOVEFILE_REPLACE_EXISTING = 0x00000001,

            MOVEFILE_COPY_ALLOWED = 0x00000002,

            MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,

            MOVEFILE_WRITE_THROUGH = 0x00000008,

            MOVEFILE_CREATE_HARDLINK = 0x00000010,

            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020

        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        #endregion

        #region Core
        /// <summary>
        /// InstallLocation -- Load from Registry
        /// </summary>
        private String InstallLocation;
        /// <summary>
        /// 
        /// </summary>
        private Tools.ProjectAssemblyAtrributes ProjectAssemblyAtrributes;
        /// <summary>
        /// Returns the InstallLocation provided from the registry DisplayName
        /// and assigns <code>InstallLocation</code>
        /// </summary>
        private String GetApplicationRegistryInstallLocation()
        {
            return InstallLocation = Tools.RegistryTools.GetInstallLocationByDisplayName(
                Static.MattimonRegistrySubkeyNames.DisplayName);
        }
        /// <summary>
        /// Returns <code>true</code> if the <code>displayName</code> is found in the applications 
        /// registry, meaning that the program has been installed.
        /// </summary>
        /// <param name="displayName">The display name as specified in the registry</param>
        /// <returns><code>Boolean</code></returns>
        private Boolean ApplicationInstalled(String displayName)
        {
            return Tools.RegistryTools.IsSoftwareInstalled(displayName);
        }
        /// <summary>
        /// Get the display name as specified in <code>MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames</code>
        /// </summary>
        /// <returns></returns>
        private String GetDisplayName()
        {
            return Static.MattimonRegistrySubkeyNames.DisplayName;
        }
        /// <summary>
        /// Assigns the <code>ProjectAssemblyAtrributes</code> object
        /// and returns the fullname (path) of the .exe
        /// </summary>
        /// <param name="installLocation">The install location of the application</param>
        /// <param name="exeName">The executable filename</param>
        /// <returns></returns>
        private String LoadProjectAssemblyAtrributes(string installLocation, String exeName)
        {
            String fullpath = Path.Combine(installLocation, exeName);
            ProjectAssemblyAtrributes = new Tools.
                ProjectAssemblyAtrributes(fullpath);
            return fullpath;
        }
        /// <summary>
        /// 
        /// </summary>
        public void RemoveControlPanelProgram()
        {
            try
            {
                progressBar1.Show();
                label1.Text = "Uninstalling...";
                Tools.RegistryTools.RemoveControlPanelProgram(GetDisplayName());
                label1.Text = "Done";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace,
                     Static.MattimonRegistrySubkeyNames.DisplayName,
                     MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        #endregion

        #region Other private attributes
        #endregion


        /// <summary>
        /// Uninstaller Form Constructor
        /// </summary>
        public FormUninstaller()
        {
            InitializeComponent();
     
            progressBar1.Hide();
            LoadProjectAssemblyAtrributes(GetApplicationRegistryInstallLocation(), "MattimonAgentApplication.exe");
            Text = GetDisplayName();
            label1.Text = "You're about to uninstall " + GetDisplayName();
        }

        #region GUI Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            System.Threading.Thread uninstaller = new System.Threading.Thread(Uninstall);
            uninstaller.SetApartmentState(System.Threading.ApartmentState.STA);
            uninstaller.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion



        
        /// <summary>
        /// 
        /// </summary>
        private void Uninstall()
        {
            bool deleteEventLogs =
                MessageBox.Show(this, 
                "You're now about to uninstall " + Static.MattimonRegistrySubkeyNames.DisplayName + ".\n" +
                "Do you wish to also remove the application's event logs?", Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;


            // First, fetch the device id from the local database and delete the device from the server
            MattimonSQLite.SQLiteClientDatabase db =
                new MattimonSQLite.SQLiteClientDatabase(
                    Static.Constants.CommonAppData,
                    Static.MattimonRegistrySubkeyNames.Publisher,
                    Static.MattimonRegistrySubkeyNames.DisplayName,
                    Static.Constants.LocalDatabaseName
                    );

            long deviceID = db.GetDeviceId();
            db.Dispose();
            db = null;

            // all files related to SQLite should now be deleted.
            try
            {
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Deleting local database...");
                // Try and delete the database directory and all its content
                IOTools.DeleteDatabaseDirectory();
                // Indicate success
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");

                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Deleting SQLite source files...");
                string sqliteX64dir = Directory.GetCurrentDirectory() + "\\" + "x64";
                string sqliteX86dir = Directory.GetCurrentDirectory() + "\\" + "x86";
            }
            catch
            {

            }



            RequestResult rr = null;
            try
            {
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Deleting device entry from server...");
                rr = DeleteDeviceEntry(deviceID);
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An error occurred while attempted to delete your device entry from our server.\n" +
                    "Click \"OK\" to continue uninstalling.\n\n" + ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rr != null && !rr.Success)
            {
                // Indicate failure
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Failed");

                // Create the error message
                string msg = "We could not delete your device from our server.\n" +
                    "You may manually delete your device from the Mattimon website.\n" +
                    "Click \"OK\" to continue unistalling." +
                    "\n\n\nError details:\n";

                // Get the error details
                if (rr.HttpRequestException != null)
                    msg += rr.HttpRequestException.Message + "\n\n" + rr.HttpRequestException.ToString();

                else if (rr.MySqlExceptionMessage != null)
                    msg += rr.MySqlExceptionMessage;

                else if (rr.Exception != null)
                    msg += rr.Exception.Message + "\n\n" + rr.Exception.ToString();
                else
                    msg += "Unknown error";

                // Show the error and continue uninstalling
                MessageBox.Show(this, msg,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // Indicate success
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
            }

            
            try
            {
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Deleting local database...");
                // Try and delete the database directory and all its content
                IOTools.DeleteDatabaseDirectory();
                // Indicate success
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
            }
            catch (Exception)
            {
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Unable to delete local database directory.");
            }



            /// Get the directory of the MattimonAgentService and MattimonUpdateService
            /// WARNING: Tools.RegistryTools.GetInstallLocationByDisplayName must be called before
            /// removing the application entry from the registry and before actually
            /// deleting the directory, so we can also get Assembly Information from the service executable (.exe) file.
            String svcPath = Path.Combine(Tools.RegistryTools.GetInstallLocationByDisplayName(
                GetDisplayName()),
                "MattimonAgentService.exe");

            String svcUpdPath = Path.Combine(Tools.RegistryTools.GetInstallLocationByDisplayName(
                GetDisplayName()),
                "MattimonUpdateService.exe");

            String svcMattSqlPath = Path.Combine(Tools.RegistryTools.GetInstallLocationByDisplayName(
                GetDisplayName()),
                "MattimonSQLServerService.exe");

            String svcEvtLogsPath = Path.Combine(Tools.RegistryTools.GetInstallLocationByDisplayName(
                GetDisplayName()),
                "MattimonEventLogService.exe");



            /// Get the the assembly name of the MattimonAgentService and MattimonUpdateService as Service Name
            string svcName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcPath)).AssemblyTitle;

            string svcUpdName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcUpdPath)).AssemblyTitle;

            /// Get the assembly product of the MattimonAgentService and MattimonUpdateService as Display Name
            string svcDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcPath)).AssemblyProduct;

            string svcUpdDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcUpdPath)).AssemblyProduct;

            string svcMattSQLName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcMattSqlPath)).AssemblyTitle;

            string svcMattSQLDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcMattSqlPath)).AssemblyProduct;

            string svcEvtLogName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcEvtLogsPath)).AssemblyTitle;

            string svcEvtLogDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcEvtLogsPath)).AssemblyProduct;

            // User this variable to know if the services where uninstalled later.
            bool servicesUninstalled = true;

            /// Try Stop/Uninstall Agent service
            try
            {
                // Stop the service if it is not stopped
                if (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                    Tools.MyServiceController.StopService(svcName);

                // Uninstall the service
                Tools.MyServiceController.Uninstall(svcName);
            }
            catch (Exception ex)
            {
                String msg = "";
                if (Tools.MyServiceController.ServiceIsInstalled(new Tools.ProjectAssemblyAtrributes(
                    Path.Combine(svcPath)).AssemblyTitle))
                {
                    // Mark to false
                    servicesUninstalled = false;

                    msg = "Failed to uninstall " + new Tools.ProjectAssemblyAtrributes(
                    Path.Combine(svcPath)).AssemblyProduct;
                }
                MessageBox.Show((msg != "" ? msg + ".\n\nError details:\n" : "") + ex.Message + "\n\n" + ex.ToString(),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            /// Try Stop/Uninstall Update service
            try
            {
                // Stop the service if it is not stopped
                if (Tools.MyServiceController.GetServiceStatus(svcUpdName) != Tools.MyServiceController.ServiceState.Stopped)
                    Tools.MyServiceController.StopService(svcUpdName);

                // Uninstall the service
                Tools.MyServiceController.Uninstall(svcUpdName);
            }
            catch (Exception ex)
            {
                String msg = "";
                if (Tools.MyServiceController.ServiceIsInstalled(new Tools.ProjectAssemblyAtrributes(
                    Path.Combine(svcUpdPath)).AssemblyTitle))
                {
                    // Mark to false
                    servicesUninstalled = false;
                    msg = "Failed to uninstall " + svcUpdName;
                }
                MessageBox.Show((msg != "" ? msg + ".\n\nError details:\n" : "") + ex.Message + "\n\n" + ex.ToString(),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            /// Try Stop/Uninstall Mattimon SQL Service
            try
            {
                // Stop the service if it is not stopped
                if (Tools.MyServiceController.ServiceIsInstalled(svcMattSQLName))
                if (Tools.MyServiceController.GetServiceStatus(svcMattSQLName) != Tools.MyServiceController.ServiceState.Stopped)
                    Tools.MyServiceController.StopService(svcMattSQLName);

                // Uninstall the service
                Tools.MyServiceController.Uninstall(svcMattSQLName);
            } 
            catch (Exception ex)
            {
                String msg = "";
                if (Tools.MyServiceController.ServiceIsInstalled(new Tools.ProjectAssemblyAtrributes(
                    Path.Combine(svcMattSqlPath)).AssemblyTitle))
                {
                    // Mark to false
                    servicesUninstalled = false;
                    msg = "Failed to uninstall " + svcMattSQLName;
                }
                MessageBox.Show((msg != "" ? msg + ".\n\nError details:\n" : "") + ex.Message + "\n\n" + ex.ToString(),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            /// Try Stop/Uninstall Mattimon Event Log Service
            try
            {
                // Stop the service if it is not stopped
                if (Tools.MyServiceController.ServiceIsInstalled(svcEvtLogName))
                    if (Tools.MyServiceController.GetServiceStatus(svcEvtLogName) != Tools.MyServiceController.ServiceState.Stopped)
                        Tools.MyServiceController.StopService(svcEvtLogName);

                // Uninstall the service
                Tools.MyServiceController.Uninstall(svcEvtLogName);
            }
            catch (Exception ex)
            {
                String msg = "";
                if (Tools.MyServiceController.ServiceIsInstalled(new Tools.ProjectAssemblyAtrributes(
                    Path.Combine(svcEvtLogsPath)).AssemblyTitle))
                {
                    // Mark to false
                    servicesUninstalled = false;
                    msg = "Failed to uninstall " + svcEvtLogName;
                }
                MessageBox.Show((msg != "" ? msg + ".\n\nError details:\n" : "") + ex.Message + "\n\n" + ex.ToString(),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                // Show progress bar
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Visible", true);

                // Remove the directory content
                RemoveDirectories(Directory.GetParent(
                    this.GetApplicationRegistryInstallLocation()).FullName);

                // Progress bar not longer needed, hide it
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Visible", false);

                // Show 
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Finalizing uninstall...");

                
                // Indicate success
                Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            try
            {
                // Finally program from the control panel
                RemoveControlPanelProgram();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace, 
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            /// Delete the event logs only if the user wishes to and that
            /// both services have been uninstalled.
            if (deleteEventLogs && servicesUninstalled)
            {
                
                string mainLogName = Static.MattimonEventLogConstants.MainLogName;
                string srcName1 = Static.MattimonEventLogConstants.UpdaterSourceName;
                string srcName2 = Static.MattimonEventLogConstants.AgentSourceName;
                string srcName3 = Static.MattimonEventLogConstants.SQLServerSourceName;


                if (EventLog.SourceExists(srcName1))
                {
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Removing " + srcName1);
                    EventLog.DeleteEventSource(srcName1);
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
                }

                if (EventLog.SourceExists(srcName2))
                {
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Removing " + srcName2);
                    EventLog.DeleteEventSource(srcName2);
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
                }

                if (EventLog.SourceExists(srcName3))
                {
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Removing " + srcName3);
                    EventLog.DeleteEventSource(srcName3);
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
                }

                if (EventLog.Exists(mainLogName))
                {
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Removing " + mainLogName);
                    EventLog.Delete(mainLogName);
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Done");
                }
            }

            

            //if (rebootRequired)
            //{
            //    Tools.GUITools.SetControlPropertyThreadSafe(label1, Text, 
            //        Static.MattimonRegistrySubkeyNames.DisplayName + " has been uninstalled.");
            //    Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Visible", false);

            //    DialogResult reboot = MessageBox.Show(this,
            //          Static.MattimonRegistrySubkeyNames.DisplayName + 
            //          " has finished uninstalling.\n" +
            //         "Do you wish to reboot your system now?",
            //          Static.MattimonRegistrySubkeyNames.DisplayName,
            //         MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);

            //    if (reboot != DialogResult.Yes)
            //    {
            //        Application.Exit();
            //        return;
            //    }

            //    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            //    Application.Exit();
            //    return;
            //}

            Application.Exit();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strpath"></param>
        private volatile Boolean rebootRequired;
        private void RemoveDirectories(string strpath)
        {

            if (Directory.Exists(strpath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(strpath);
                var files = dirInfo.GetFiles();

                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Minimum", 0);
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Value", 0);
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Maximum", files.Length - 1);
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Step", 1);

                foreach (FileInfo file in files)
                {

                    // Show the file that is being proccessed
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", file.FullName);

                    try
                    {
                        // Delete this file
                        file.Delete();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Mark that reboot is required
                        rebootRequired = true;

                        // As this file could not be deleted (most lickely is in use)
                        // Tell the operating system to delete that file after reboot
                        MoveFileEx(file.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                    }

                    // I assume your code is inside a Form, else you need a control to do this invocation;
                    this.BeginInvoke(new Action(() => progressBar1.PerformStep()));

                }

                var dirs = dirInfo.GetDirectories();

                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Value", 0);
                Tools.GUITools.SetControlPropertyThreadSafe(progressBar1, "Maximum", dirs.Length);

                foreach (DirectoryInfo dir in dirs)
                {
                    Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", dir.FullName);

                    try
                    {
                        // Delete this directory
                        //dir.Delete(true);
                        RemoveDirectories(dir.FullName);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Mark that reboot is required
                        this.rebootRequired = true;

                        // As this directory could not be deleted (most lickely is in use)
                        // Tell the operating system to delete that file after reboot
                        MoveFileEx(dir.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                    }
                    this.BeginInvoke(new Action(() => progressBar1.PerformStep())); //I assume your code is inside a Form, else you need a control to do this invocation;
                }


                
                if (rebootRequired)
                {
                    // We assume that if reboot is required, some files and folders could not be
                    // fully deleted (most lickely were in use)

                    // Get the full path
                    DirectoryInfo appbase = new DirectoryInfo(
                        Static.Constants.ApplicationDirectoryPath);

                    // Tell the operating system to delete that directory after reboot
                    MoveFileEx(appbase.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                    // Get the base path
                    appbase = new DirectoryInfo(
                        Static.Constants.ApplicationBaseDirectoryPath);

                    // Tell the operating system to delete that directory after reboot
                    MoveFileEx(appbase.FullName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void DeleteDirectoryAndContent(String path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo baseDir = new DirectoryInfo(path);


                String[] basedirs = Directory.GetDirectories(baseDir.FullName);
                foreach (String dir in basedirs)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (di.GetFiles() != null)
                    {

                        foreach (FileInfo fi in di.GetFiles())
                        {

                            try
                            {
                                fi.Delete();
                            }
                            catch (UnauthorizedAccessException)
                            {

                            }
                        }
                    }

                    if (di.GetDirectories() != null)
                        DeleteDirectoryAndContent(di.FullName);
                }

                if (baseDir.GetFiles() != null)
                {
                    foreach (FileInfo fi in baseDir.GetFiles())
                    {
                        if (fi.Name.Equals("Uninstall.exe")) continue;

                        Tools.GUITools.SetControlPropertyThreadSafe(label1, "Text", "Uninstalling " + fi.Name);
                        fi.Delete();
                    }
                }

                baseDir.Delete();
            }
        }

        /// <summary>
        /// Rest Request - Delete Device Entry From Server
        /// </summary>
        /// <returns></returns>
        private Rest.Models.RequestResult DeleteDeviceEntry(long deviceId)
        {
            Rest.DeviceRequests deviceRequests = new Rest.DeviceRequests();
            Rest.Models.RequestResult rr = deviceRequests.DeleteDevice(deviceId);
            return rr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool TryDelete(FileInfo file)
        {
            try
            {
                file.Delete();
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
