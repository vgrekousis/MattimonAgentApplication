using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MattimonAgentLibrary;
using MattimonAgentLibrary.Tools;
using IWshRuntimeLibrary;
using System.IO.Compression;
using Microsoft.Win32;
using MattimonAgentLibrary.WMI;
using MattimonAgentLibrary.Rest;
using MattimonAgentLibrary.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MattimonSQLite;
using System.Management;

namespace Install
{
    public partial class Form4 : FormCommon
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

        /// <summary>
        /// Selected Device Options
        /// </summary>
        private DeviceOptions DeviceOptions;
        /// <summary>
        /// Authenticated user
        /// </summary>
        private UserAuthentication UserAuthentication;
        /// <summary>
        /// Value is Constants.ApplicationDirectoryPath, assigned in constructor
        /// </summary>
        private String InstallPath;
        /// <summary>
        /// 
        /// </summary>
        private volatile Boolean installing;
        /// <summary>
        /// Previous Form
        /// </summary>
        private Form3 Form3;
        /// <summary>
        /// Created Local Database
        /// </summary>
        private SQLiteClientDatabase SQLiteClientDatabase;
        /// <summary>
        /// Its value is the device ID that has been generated from the server's SQL database
        /// as Inserted ID representing the device's row primary key.
        /// </summary>
        private long GeneratedPostDeviceID; private Device PostedDevice;
        /// <summary>
        /// 
        /// </summary>
        private event EventHandler InstallComplete;
        /// <summary>
        /// 
        /// </summary>
        private bool Installed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form3"></param>
        /// <param name="installPath"></param>
        /// <param name="userAuthentication"></param>
        /// <param name="deviceOptions"></param>
        public Form4(Form3 form3, String installPath, UserAuthentication userAuthentication, DeviceOptions deviceOptions)
        {
            InitializeComponent();
            TopMost = true;
            ControlBox = false;
            ShowInTaskbar = ControlBox;
            btnNext.Hide();
            btnPrevious.Hide();

            btnCancel.Click += BtnCancel_Click;

            label3.Hide();
    
            Form3 = form3;
            InstallPath = installPath;
            UserAuthentication = userAuthentication;
            DeviceOptions = deviceOptions;

            UpdateProgressBarStep(1);
            SetText(label1, Static.ExecutingAssemblyAttributes.AssemblyProduct + " is being installed.");
            SetText(label2, "Please wait...");
            SetText(label3, "Log");

            InstallComplete += Form4_InstallComplete;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form4_InstallComplete(object sender, EventArgs e)
        {
            Installed = true; // Important variable
            installing = false; // Important variable



            GUITools.SetControlPropertyThreadSafe(label1, "Text", Static.ExecutingAssemblyAttributes.AssemblyProduct + " has been installed!");
            GUITools.SetControlPropertyThreadSafe(label2, "Visible", false);
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Visible", false);

            this.Invoke((MethodInvoker)delegate () {
                DebugWriteLine("Installation Completed");
            });


            DialogResult dr = MessageBox.Show(this, 
                Static.ExecutingAssemblyAttributes.AssemblyProduct + " has finished installing.\n" +
                "Do you wish to create a shortcut on the desktop?", 
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (dr == DialogResult.Yes)
            {
                String shortcutPath = CreateShortcut(
                    Static.MattimonRegistrySubkeyNames.DisplayName, 
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                    InstallPath + @"\MattimonAgentApplication.exe");
            }
            
            btnCancel.Text = "Finish";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shortcutName"></param>
        /// <param name="shortcutPath"></param>
        /// <param name="targetFileLocation"></param>
        public String CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {

            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "Shortcut to Mattimon Agent Application";   // The description of the shortcut
            shortcut.IconLocation = InstallPath + @"\icon.ico";          // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            shortcut.Save();                                    // Save the shortcut
            return shortcutLocation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (!installing)
            {
                Hide();
                Form3.Show();
                Form3.Location = Location;
                Form3.Focus();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent the window from closing if the application is installing
            if (installing)
                e.Cancel = true;



            if (Installed)
            {
                // Close application without triggering form closing events
                // on active forms that will result the database to be deleted.
                CommonWindowsForms.ApplicationExit();
                return;
            }

            if (!CommonWindowsForms.PromptExit())
            {
                e.Cancel = true;
            }
            else
            {
                using (new WaitCursor())
                    // Delete the database as the installation was canceled.
                    Tools.IOTools.DeleteDatabaseDirectory();

                // Prevent these forms from handing form closing as
                // application exit is called in this form.
                foreach (Form f in Application.OpenForms)
                {
                    if (f is Form0 f0)
                    {
                        f0.FormClosing -= f0.Form0_FormClosing;
                    }
                    if (f is Form1 f1)
                    {
                        f1.FormClosing -= f1.Form1_FormClosing;
                    }
                    if (f is Form2 f2)
                    {
                        f2.FormClosing -= f2.Form2_FormClosing;
                    }
                    if (f is Form3 f3)
                    {
                        f3.FormClosing -= f3.Form3_FormClosing;
                    }
                    if (f == this) // is Form4
                    {
                        FormClosing -= Form4_FormClosing;
                    }
                }
                Application.Exit();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="text"></param>
        private void SetText(Control c, String text)
        { 
            GUITools.SetControlPropertyThreadSafe(c, "Text", text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void DebugWrite(String text)
        {
            //output.Text += text;
            output.AppendText(text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        private void DebugWriteLine(String text, params object[] args)
        {
            String output = String.Format(text, args);
            //this.output.Text += output + "\r\n";
            this.output.AppendText(output + "\r\n");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void UpdateProgressBar(int value)
        {
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Value", value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maximum"></param>
        private void UpdateProgressBarMaximum(int maximum)
        {
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Maximum", maximum);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="step"></param>
        private void UpdateProgressBarStep(int step)
        {
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Step", step);
        }
        
        private void Install_CreateLocalDatabase()
        {
            this.Invoke((MethodInvoker)delegate () { DebugWriteLine("Preparing local database..."); });
            if (!CreateLocalDatabase()) // Handles inner exception's and shows messages.
            {
                installing = false;
                throw (new Exception("The Installer will now close.")); // Brake the operation and reach base catch block.
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Install_PostDevice()
        {
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Posting device...");
            this.Invoke((MethodInvoker)delegate ()
            {
                DebugWriteLine("Posting device....");
            });

            if (!PostDevice(out string postDeviceErrorMsg))
            {
                // Indicate failure
                GUITools.SetControlPropertyThreadSafe(output, "Text", "Failed");

                // Show error message
                MessageBox.Show(this, postDeviceErrorMsg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Posting device failed, delete the database directory as it's not needed.
                Tools.IOTools.DeleteDatabaseDirectory();

                // Cancel the install without provoking 'Form closed' on the active forms
                CommonWindowsForms.ApplicationExit();


                installing = false;
                throw (new Exception("The Installer will now close."));
            }

            // Display related post device information
            this.Invoke((MethodInvoker)delegate ()
            {
                DebugWriteLine("Device '{0}' of type '{1}' has been posted. Device Ident: #{2}",
                    PostedDevice.ComputerName,
                    PostedDevice.Device_Type_Id == 2 ? "Server" : "Workstation",
                    PostedDevice.Device_Id);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        private void Install_CreateLocalData()
        {
            // Device has been committed on the server, create local data
            // Show
            this.Invoke((MethodInvoker)delegate ()
            {
                DebugWriteLine("Creating local data...");
            });

            try
            {
                // Insert Authentication keys
                SQLiteClientDatabase.InsertKey(
                    UserAuthentication.User_Id,
                    PostedDevice.Device_Id,
                    PostedDevice.Device_Type_Id,
                    UserAuthentication.Company_Id,
                    UserAuthentication.User_Agent_ID);

                // Insert Database Options
                SQLiteClientDatabase.InsertConfig(DeviceOptions.ReportingInterval);

                // Clean
                SQLiteClientDatabase.Clean();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An error occurred while creating local data.\n\nError details:\n" +
                    ex.Message + "\n\n" + ex.StackTrace, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Can throw exception. Handled in the base catch block
                Tools.IOTools.DeleteDatabaseDirectory();

                /// Important instruction:
                /// Pass through the base catch block
                /// to 'rollback' installation.
                throw new Exception("The Installer will now close.");
            }


            /// Reaching here,  local data have been created. Ok to create application directory.
            /// Show
            this.Invoke((MethodInvoker)delegate ()
            {
                DebugWriteLine("Creating application directory {0}", InstallPath);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        private void Install_CreateApplicationDirectory()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                DebugWriteLine("Creating application directory...");
            });
            /// Create application directoty
            Directory.CreateDirectory(InstallPath);

            /// Check if the path exists for real
            if (Directory.Exists(InstallPath))
            {
                /// Show success
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine("Directory created succesfully");
                });
            }

            /// Create a temporary directory to extract application files to.
            String tmpDirPath = Path.Combine(Directory.GetCurrentDirectory(), "_tmp");

            /// Create if it does not exist
            if (!Directory.Exists(tmpDirPath))
                Directory.CreateDirectory(tmpDirPath);

            else
            {
                /// Temporary directory already exists
                /// Delete it!
                DeleteDirectoryAndContent(tmpDirPath);

                /// Create it again
                Directory.CreateDirectory(tmpDirPath);
            }


            /// ZIP LOGIC --- Install application program files
            MemoryStream memoryStream = new MemoryStream(Install.Properties.Resources.Package);
            ZipArchive zipArchive = new ZipArchive(memoryStream);
            UpdateProgressBarMaximum(zipArchive.Entries.Count);

            int i = 1;
            long totalbytes = 0;
            foreach (var zipEntry in zipArchive.Entries)
            {
                /// Open zip entry as stream
                Stream extractedFile = zipEntry.Open();

                /// Convert stream to memory stream
                MemoryStream extractedMemoryStream = new MemoryStream();
                GUITools.SetControlPropertyThreadSafe(label2, "Text", "Extracting " + zipEntry.FullName);
                extractedFile.CopyTo(extractedMemoryStream);


                UpdateProgressBar(i);
                totalbytes += zipEntry.Length;
                i++;
            }

            ///Show
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Done");
            System.Threading.Thread.Sleep(100); // Wait a little

            /// Show
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Installing...");

            /// Extract content to tmp directory
            zipArchive.ExtractToDirectory(tmpDirPath);

            /// Copy directory content to install directory
            DirectoryCopy(tmpDirPath + @"\Package\", InstallPath, true);

            /// Delete temp path
            DeleteDirectoryAndContent(tmpDirPath);
            /// END OF ZIP LOGIC
        }
        /// <summary>
        /// 
        /// </summary>
        private void Overwrite_ReplaceFiles()
        {
            /// Create a temporary directory to extract application files to.
            String tmpDirPath = Path.Combine(Directory.GetCurrentDirectory(), "_tmp");

            /// Create if it does not exist
            if (!Directory.Exists(tmpDirPath))
                Directory.CreateDirectory(tmpDirPath);

            else
            {
                /// Temporary directory already exists
                /// Delete it!
                DeleteDirectoryAndContent(tmpDirPath);

                /// Create it again
                Directory.CreateDirectory(tmpDirPath);
            }


            /// ZIP LOGIC --- Install application program files
            MemoryStream memoryStream = new MemoryStream(Install.Properties.Resources.Package);
            ZipArchive zipArchive = new ZipArchive(memoryStream);
            UpdateProgressBarMaximum(zipArchive.Entries.Count);

            int i = 1;
            long totalbytes = 0;
            foreach (var zipEntry in zipArchive.Entries)
            {
                /// Open zip entry as stream
                Stream extractedFile = zipEntry.Open();

                /// Convert stream to memory stream
                MemoryStream extractedMemoryStream = new MemoryStream();
                GUITools.SetControlPropertyThreadSafe(label2, "Text", "Extracting " + zipEntry.FullName);
                extractedFile.CopyTo(extractedMemoryStream);


                UpdateProgressBar(i);
                totalbytes += zipEntry.Length;
                i++;
            }

            ///Show
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Done");
            System.Threading.Thread.Sleep(100); // Wait a little

            /// Show
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Installing...");

            /// Extract content to tmp directory
            zipArchive.ExtractToDirectory(tmpDirPath);

            /// Copy directory content to install directory
            DirectoryCopy(tmpDirPath + @"\Package\", InstallPath, true);

            /// Delete temp path
            DeleteDirectoryAndContent(tmpDirPath);
            /// END OF ZIP LOGIC
        }
        /// <summary>
        /// 
        /// </summary>
        private void Install_CreateRegistryEntry()
        {
            try
            {
                /// Show
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine("Creating registry entry...");
                });

                /// Register installed program to control panel;
                RegisterControlPanelProgram();

                /// Show
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine("Done");
                });
            }
            catch (Exception ex) /// An error occurred while register program
            {
                /// Show
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine("Failed. " + ex.Message + ".");
                });

                /// Show Message
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                /// Important instruction:
                /// Pass through the base catch block
                /// to 'rollback' installation.
                throw new Exception("The Installer will now close.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcFullPath"></param>
        /// <returns></returns>
        private String Install_InstallService(String svcFullPath)
        {
            #region Service Paths
            /// Get the the assembly name of the Service as Service Name
            String svcName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcFullPath)).AssemblyTitle;

            /// Get the assembly product of the Service as Display Name
            String svcDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcFullPath)).AssemblyProduct;
            #endregion

            /// Show 
            GUITools.SetControlPropertyThreadSafe(label2, "Text", "Installing and starting " + svcDisplay);
            
            try
            {
                #region Stop and uninstall if exists
                /// If the update service is still installed
                if (Tools.MyServiceController.ServiceIsInstalled(svcName))
                {
                    /// Show a warning
                    DialogResult dr = MessageBox.Show(this, "Seems like an associated Service is still installed.\n" +
                        "Service name: " + svcName + "\n\n" + "Display name: " + svcDisplay + "\n\nIt will now replaced. Click \"OK\" to continue",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    try
                    {
                        /// Stop the service if it's running
                        if (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                            Tools.MyServiceController.StopService(svcName);

                        /// Uninstall it!
                        Tools.MyServiceController.Uninstall(svcName);

                        /// Check if it was actually uninstalled
                        if (!Tools.MyServiceController.ServiceIsInstalled(svcName))
                        {
                            /// Show message if succesfully uninstalled
                            MessageBox.Show(this, svcName + " has been uninstalled.", Text,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex) /// Failed to stop or uninstall the service
                    {
                        String msg =
                            Tools.MyServiceController.ServiceIsInstalled(svcName) ?
                            "Could not uninstall the service.\n" +
                            "Therefore, the installation cannot continue.\n\nError details:" : null;
                        msg += ex.Message + "\n\n" + ex.StackTrace;
                        MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Important instruction:
                        // Pass through the base catch block
                        // to 'rollback' installation.
                        throw new Exception("The Installer will now close.");
                    }
                }
                #endregion

                /// Install and start the update service
                Install.Tools.MyServiceController.InstallAndStart(
                    svcName, svcDisplay, svcFullPath);


                Boolean installed = Install.Tools.MyServiceController.ServiceIsInstalled(svcName);

                if (installed)
                {
                    Install.Tools.MyServiceController.SetRecoveryOptions(svcName);
                    Install.Tools.MyServiceController.SetDelayedAutoStart(svcName, true);
                }
                
             
                /// Show
                this.Invoke((MethodInvoker)delegate ()
                {
                    if (installed)
                    {
                        DebugWriteLine(svcName + " installed Status: " +
                            Install.Tools.MyServiceController.GetServiceStatus(svcName));
                    }
                });

                return svcName;
            }
            catch (Exception ex) /// Error occurred while installing or starting the service
            {
                String msg = "";

                /// If not installed
                if (!Install.Tools.MyServiceController.ServiceIsInstalled(svcName))
                    /// The failure is on install
                    msg = "Failed to Install " + svcDisplay + ".";

                else
                    /// The failure is on start
                    msg = "Failed to Start " + svcDisplay + ".";

                /// Show error
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine(msg);
                });

                MessageBox.Show(this,
                    msg + ".\n\nError details:\n" +
                    ex.Message + "\n\n" + ex.StackTrace,
                    Static.ExecutingAssemblyAttributes.AssemblyTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                /// Important:
                /// Reach base catch block to do the 'rollback'
                throw new Exception("The Installer will now close.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcFullPath"></param>
        /// <returns></returns>
        private String Install_InstallService2(String svcFullPath)
        {
            #region Service Paths
            /// Get the the assembly name of the Service as Service Name
            String svcName = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcFullPath)).AssemblyTitle;

            /// Get the assembly product of the Service as Display Name
            String svcDisplay = new Tools.ProjectAssemblyAtrributes(
                Path.Combine(svcFullPath)).AssemblyProduct;
            #endregion

            #region Stop and uninstall if exists
            /// If the update service is still installed
            if (Tools.MyServiceController.ServiceIsInstalled(svcName))
            {
                /// Show a warning
                DialogResult dr = MessageBox.Show(this, "Seems like an associated Service is still installed.\n" +
                    "Service name: " + svcName + "\n\n" + "Display name: " + svcDisplay + "\n\nIt will now replaced. Click \"OK\" to continue",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                try
                {
                    /// Stop the service if it's running
                    if (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                        Tools.MyServiceController.StopService(svcName);

                    /// Uninstall it!
                    Tools.MyServiceController.Uninstall(svcName);

                    /// Check if it was actually uninstalled
                    if (!Tools.MyServiceController.ServiceIsInstalled(svcName))
                    {
                        /// Show message if succesfully uninstalled
                        MessageBox.Show(this, svcName + " has been uninstalled.", Text,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex) /// Failed to stop or uninstall the service
                {
                    String msg =
                        Tools.MyServiceController.ServiceIsInstalled(svcName) ?
                        "Could not uninstall the service.\n" +
                        "Therefore, the installation cannot continue.\n\nError details:" : null;
                    msg += ex.Message + "\n\n" + ex.StackTrace;
                    MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Important instruction:
                    // Pass through the base catch block
                    // to 'rollback' installation.
                    throw new Exception("The Installer will now close.");
                }
            }
            #endregion

            Boolean ok = MattimonAgentLibrary.WinServiceInstaller.ManagedInstallService(
                svcFullPath, out Exception exception);

            if (!ok)
            {
                String exmsg = exception != null ? exception.Message + "\n\n" + exception.StackTrace : String.Empty;
                String msg = "Could not uninstall the service " + svcName + ".\n\n" + exmsg;
                MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("The installer will now close.");
            }

            return svcName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid"></param>
        private void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        private void StopServiceIfExist(String svcName)
        {
            if (Tools.MyServiceController.ServiceIsInstalled(svcName))
                if (Tools.MyServiceController.GetServiceStatus(svcName) != Tools.MyServiceController.ServiceState.Stopped)
                    Tools.MyServiceController.StopService(svcName);
            while (Tools.MyServiceController.GetServiceStatus(svcName) == Tools.MyServiceController.ServiceState.StopPending)
            {

            }
        }
        private void BeginInstall()
        {
            if (Tools.RegistryTools.IsSoftwareInstalled(
                new ProjectAssemblyAtrributes(
                    Application.ExecutablePath, true).AssemblyProduct))
            {
                BeginOverwrite();
                return;
            }

            installing = true;
            String installedSvc1 = "", installedSvc2 = "";//, installedSvc3 = "";
            try
            {
                if (Directory.Exists(InstallPath))
                    DeleteDirectoryAndContent(InstallPath);

                Install_CreateLocalDatabase();
                Install_PostDevice();

                Install_CreateLocalData();
                Install_CreateApplicationDirectory();
                Install_CreateRegistryEntry();

                installedSvc1 = Install_InstallService(Path.Combine(InstallPath, "MattimonAgentService.exe"));
                installedSvc2 = Install_InstallService(Path.Combine(InstallPath, "MattimonUpdateService.exe"));
                //installedSvc3 = Install_InstallService(Path.Combine(InstallPath, "MattimonSQLServerService.exe"));

                /// If reached here, no Exception was thrown.
                /// File InstallComplete event
                if (InstallComplete != null)
                {
                    Delegate[] subscribers = InstallComplete.GetInvocationList();
                    foreach (EventHandler target in subscribers)
                    {
                        target(this, new EventArgs());
                    }
                    return;
                }
            }

            /// INSTALATION FAILED!
            /// BASE CATCH BLOCK (Rollback happens here) 
            catch (Exception ex)
            {

                /// Show the cause of the error
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace, Text,
                    MessageBoxButtons.OK);

                /// Show
                this.Invoke((MethodInvoker)delegate ()
                {
                    DebugWriteLine("Removing committed application components...");
                });

                /// Try to remove committed application components
                /// from the computer.
                try
                {
                    

                    /// Delete application directory if exists
                    if (Directory.Exists(InstallPath))
                    {
                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Deleting application directory and files...");
                        });

                        /// Delete the directory
                        DeleteDirectoryAndContent(InstallPath); /// May throw exception

                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Done");
                        });
                    }

                    /// Delete the database directory
                    try
                    {
                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Deleting local database directory and file...");
                        });

                        /// Delete it!
                        Tools.IOTools.DeleteDatabaseDirectory();  /// May throw exception

                    }

                    /// The file is most lickely in use
                    catch (Exception deleteDbException)
                    {

                        /// Tell the operating system to delete:

                        /// The Database file path
                        MoveFileEx(Static.Constants.DatabaseBaseDirectory,
                            null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                        // The Directory
                        MoveFileEx(Static.Constants.LocalDatabasePath,
                            null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                        // The Base directory
                        MoveFileEx(Static.Constants.DatabaseBaseDirectory,
                            null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Failed. " + deleteDbException.Message + ". Reboot is required.");
                        });

                    }


                    /// If user device have been posted
                    if (PostedDevice != null)
                    {
                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Deleting device entry from server...");
                        });

                        /// Delete that entry
                        bool entryDeleted = DeleteDevice(); /// Exceptions are supressed, only boolean is returned to specify success or not.

                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine(entryDeleted ?
                            "Done" : "Could not delete the device entry from the server. Device ID: " + PostedDevice.Device_Id);
                        });
                    }

                    /// Stop and delete the service if is defined
                    if (installedSvc1 != "")
                    {
                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Uninstalling " + installedSvc1 + "...");
                        });


                        /// Stop service if not stopped
                        if (Tools.MyServiceController.GetServiceStatus(installedSvc1) != Tools.MyServiceController.ServiceState.Stopped)
                            Tools.MyServiceController.StopService(installedSvc1);

                        /// Uninstall the servicer
                        Tools.MyServiceController.Uninstall(installedSvc1);

                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Done");
                        });
                    }

                    /// Stop and delete the second service if is defined
                    if (installedSvc2 != "")
                    {
                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Uninstalling " + installedSvc2 + "...");
                        });


                        /// Stop service if not stopped
                        if (Tools.MyServiceController.GetServiceStatus(installedSvc2) != Tools.MyServiceController.ServiceState.Stopped)
                            Tools.MyServiceController.StopService(installedSvc2);

                        /// Uninstall the servicer
                        Tools.MyServiceController.Uninstall(installedSvc2);

                        /// Show
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            DebugWriteLine("Done");
                        });
                    }

                    // Delete application entry from the registry
                    Tools.RegistryTools.RemoveControlPanelProgram(Static.MattimonRegistrySubkeyNames.DisplayName);

                    installing = false;
                }

                /// ROLLBACK FAILED
                /// An exception have been thrown in the try block 
                /// containing the 'Rollback' logic.
                catch (Exception inner) /// Deleting commited application files threw an exception
                {
                    installing = false;

                    /// Show this error
                    MessageBox.Show(this, inner.ToString(), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    /// Quit even if rollback was successfull or not.
                    CommonWindowsForms.ApplicationExit();
                }
            }
        }
        private void BeginOverwrite()
        {
            installing = true;
            String installedSvc1 = "", installedSvc2 = ""; //, installedSvc3 = "";
            Boolean filesReplaced = false;
            Boolean registryCreated = false;
            Boolean agentServiceInstalled = false;
            Boolean updateServiceInstalled = false;
            //Boolean sqlServiceInstalled = false;

            try
            {
                String svcNameAgent = null, svcNameUpd = null, svcSql = null;
                try
                {
                    /// Get the the assembly name of the Service as Service Name
                    svcNameAgent = new Tools.ProjectAssemblyAtrributes(
                        Path.Combine(Path.Combine(InstallPath, "MattimonAgentService.exe"))).AssemblyTitle;

                    svcNameUpd = new Tools.ProjectAssemblyAtrributes(
                        Path.Combine(Path.Combine(InstallPath, "MattimonUpdateService.exe"))).AssemblyTitle;

                    svcSql = new Tools.ProjectAssemblyAtrributes(
                        Path.Combine(Path.Combine(InstallPath, "MattimonSQLServerService.exe"))).AssemblyTitle;
                }
                catch (IOException)
                {
                    MessageBox.Show("Mattimon Services assemblies have been previously deleted.\nClick \"OK\" to continue installing.", "Information (IO Exception handled)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    svcNameUpd = "MattimonUpdateService";
                    svcNameAgent = "MattimonAgentService";
                    svcSql = "MattimonSQLServerService";
                }
                catch
                {
                    MessageBox.Show("Mattimon Services assemblies have been previously deleted.\nClick \"OK\" to continue installing.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    svcNameUpd = "MattimonUpdateService";
                    svcNameAgent = "MattimonAgentService";
                    svcSql = "MattimonSQLServerService";
                }

                StopServiceIfExist(svcNameUpd);
                StopServiceIfExist(svcNameAgent);
                StopServiceIfExist(svcSql);
                


                Overwrite_ReplaceFiles(); filesReplaced = true;

                Install_CreateRegistryEntry(); registryCreated = true;


                installedSvc1 = Install_InstallService(Path.Combine(InstallPath, "MattimonAgentService.exe"));
                installedSvc2 = Install_InstallService(Path.Combine(InstallPath, "MattimonUpdateService.exe"));
                //installedSvc3 = Install_InstallService(Path.Combine(InstallPath, "MattimonSQLServerService.exe"));

                agentServiceInstalled = installedSvc1 != "";
                updateServiceInstalled = installedSvc2 != "";
                //sqlServiceInstalled = installedSvc3 != "";

                /// If reached here, no Exception was thrown.
                /// File InstallComplete event
                if (InstallComplete != null)
                {
                    Delegate[] subscribers = InstallComplete.GetInvocationList();
                    foreach (EventHandler target in subscribers)
                    {
                        target(this, new EventArgs());
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                installing = false;

                if (!filesReplaced)
                {
                    MessageBox.Show(this, "Failed to replace files.\n\nError details:\n" 
                        + ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!registryCreated)
                {
                    MessageBox.Show(this, 
                        "Failed to create registry entry.\n\nError details:\n" + 
                        ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!agentServiceInstalled)
                {
                    MessageBox.Show(this, 
                        "Failed to install the Mattimon Agent service.\n\nError details:\n" + 
                        ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!updateServiceInstalled)
                {
                    MessageBox.Show(this, 
                        "Failed to install the Mattimon Updater service.\n\nError details:\n" + 
                        ex.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                CommonWindowsForms.ApplicationExit();
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
                            fi.Delete();
                        }
                    }

                    if (di.GetDirectories() != null)
                        DeleteDirectoryAndContent(di.FullName);
                }

                if (baseDir.GetFiles() != null)
                {
                    foreach (FileInfo fi in baseDir.GetFiles())
                    {
                        fi.Delete();
                    }
                }

                baseDir.Delete();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }


            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();

            // Initialize progress bar maximum value and step
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Step", 1);
            GUITools.SetControlPropertyThreadSafe(progressBar1, "Maximum", files.Length);


            int counter = 1;
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);

                // Display the file that is being copied
                GUITools.SetControlPropertyThreadSafe(label2, "Text", "Copying " + file.FullName);

                // Copy the file
                file.CopyTo(temppath, true);

                // Update progress bar value
                GUITools.SetControlPropertyThreadSafe(progressBar1, "Value", counter++);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                counter = 1;
                GUITools.SetControlPropertyThreadSafe(progressBar1, "Step", 1);
                GUITools.SetControlPropertyThreadSafe(progressBar1, "Maximum", dirs.Length);
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Display the file that is being copied
                    GUITools.SetControlPropertyThreadSafe(label2, "Text", "Copying " + subdir.FullName);

                    // Copy the directory (Recursive)
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);

                    // Update progress bar value
                    GUITools.SetControlPropertyThreadSafe(progressBar1, "Value", counter++);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CreateLocalDatabase()
        {
            try
            {
                SQLiteClientDatabase = new SQLiteClientDatabase(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    Static.ExecutingAssemblyAttributes.AssemblyCompany,
                    Static.ExecutingAssemblyAttributes.AssemblyProduct,
                    Static.Constants.LocalDatabaseName, true);

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while initializing the local database.\n\nError details:\n" + ex.Message + "\n\n" + ex.StackTrace, 
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                bool created1 = SQLiteClientDatabase.CreateKeysTable();
                bool created2 = SQLiteClientDatabase.CreateConfigTable();
                return created1 && created2;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creating the local database tables.\n\n" +
                    "Error details:\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form4_Load(object sender, EventArgs e)
        {
            System.Threading.Thread installer = new System.Threading.Thread(BeginInstall);
            installer.SetApartmentState(System.Threading.ApartmentState.STA);
            installer.Start();
        }


        #region Registry
        /// <summary>
        /// Updates the display version in the already existing Uninstall entry in the registry.
        /// </summary>
        /// <param name="displayName">The application's display name as shown in Uninstall Programs.</param>
        /// <param name="assemblyVersion">The version of the assembly</param>
        public void UpdateDisplayVersion(String displayName, String assemblyVersion)
        {
            try
            {
                string Install_Reg_Loc = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
                RegistryKey hKey = (Registry.LocalMachine).OpenSubKey(Install_Reg_Loc, true);
                RegistryKey appKey = hKey.OpenSubKey(displayName, true);
                appKey.SetValue("DisplayVersion", (object)assemblyVersion);
                appKey.Close();
                hKey.Close();
            }
            catch (Exception ex) { throw ex; }
        }
        public void RegisterControlPanelProgram()
        {
            try
            {
                string Install_Reg_Loc = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

                RegistryKey hKey = (Registry.LocalMachine).OpenSubKey(Install_Reg_Loc, true);

                RegistryKey appKey = hKey.CreateSubKey(Static.ExecutingAssemblyAttributes.AssemblyProduct);

                appKey.SetValue("DisplayName", (object)Static.MattimonRegistrySubkeyNames.DisplayName, RegistryValueKind.String);

                appKey.SetValue("Publisher", (object)Static.MattimonRegistrySubkeyNames.Publisher, RegistryValueKind.String);

                appKey.SetValue("InstallLocation", (object)this.InstallPath, RegistryValueKind.ExpandString);

                appKey.SetValue("DisplayIcon", (object)InstallPath + @"\icon.ico", RegistryValueKind.String);

                appKey.SetValue("UninstallString", (object)InstallPath + @"\Uninstall.exe", RegistryValueKind.ExpandString);

                /// Get the assembly version from the deployed application exe
                appKey.SetValue("DisplayVersion", (object)new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Path.Combine(InstallPath, "MattimonAgentApplication.exe")).GetAssemblyVersion(), RegistryValueKind.String);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Rest
        private Boolean DeleteDevice()
        {
            if (this.PostedDevice != null)
            {
                DeviceRequests deviceRequests = new DeviceRequests();
                RequestResult rr = deviceRequests.DeleteDevice(PostedDevice.Device_Id);
                if (rr.Success)
                {
                    PostedDevice = null;
                    return true;
                }
                return false;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private Boolean PostDevice(out String errorMessage)
        {
            WMIProvider provider = new WMIProvider();
            Device device = new Device(); DeviceOptions deviceOptions = new DeviceOptions();
            /// If Operating System string contains 'server', we'll suppose that the device is a server
            /// Otherwise, we'll suppose that the device is a workstation.
            Boolean isServer = provider.GetOperatingSystemString().ToLower().Contains("server");
            /// Device type id should match psm_device_types primary key
            /// '2' means Server and '3' means Workstation, according to the Mattimon database on server.
            int definedDeviceTypeId = isServer ? 2 : 3;

            //device.Port = 80;  // Port should not be defined in this context---the user should select an active port from his own machine using MattimonAgentApplication.
            device.BIOSSerialNumber = provider.GetBIOSSerialNumber();
            device.Company_Id = UserAuthentication.Company_Id;
            device.ComputerName = provider.GetComputerName();
            device.IpAddress = provider.GetIPAddress();
            device.MacAddress = provider.GetMacAddress();
            device.Model = provider.GetModel();
            device.OperatingSystem = provider.GetOperatingSystemString();
            device.OperatingSystemSerialNumber = provider.GetOSSerialNumber();
            device.User_Id = UserAuthentication.User_Id;
            device.Device_Type_Id = definedDeviceTypeId;
            device.AgentReportInterval = DeviceOptions.ReportingInterval;
            device.MonitorSql = DeviceOptions.MonitorSql ? 1 : 0;

            DeviceRequests requests = new DeviceRequests();
            device = requests.CreateDeviceEntry(device);

            Boolean postDeviceSuccess =
                device.Exception == null &&
                device.HttpRequestException == null &&
                device.MySqlExceptionMessage == null &&
                device.Device_Id > 0;

            if (postDeviceSuccess)
            {
                errorMessage = "";

                this.GeneratedPostDeviceID = device.Device_Id;
                this.PostedDevice = device;
            }
            else
            {
                errorMessage = "We could not receive your device due to the following errors.\n\n";

                if (device.HttpRequestException != null)
                {
                    errorMessage += device.HttpRequestException.Message + "\n\n" +
                        device.HttpRequestException.StackTrace;
                }
                else if (device.Exception != null)
                {
                    errorMessage += device.Exception.Message + "\n\n" +
                        device.Exception.StackTrace;
                }
                else if (device.MySqlExceptionMessage != null)
                {
                    errorMessage += device.MySqlExceptionMessage;
                }
                else
                {
                    errorMessage = "An unhandled error has occurred.";
                }
            }
            return postDeviceSuccess;
        }
        #endregion
    }
}
