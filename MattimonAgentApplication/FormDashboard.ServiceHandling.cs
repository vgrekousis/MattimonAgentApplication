using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using MattimonAgentLibrary.Tools;
using MattimonAgentLibrary.Models;

namespace MattimonAgentApplication
{
    partial class FormDashboard
    {
        private bool svccheckinit;
        internal System.Windows.Forms.Timer MattimonServiceCheckTimer;
        /// <summary>
        /// Initalize the timer that will be refreshing the status of the Mattimon Services on the UI.
        /// </summary>
        /// <param name="interval"></param>
        public void InitializeServiceCheckTimer(int interval)
        {
            if (svccheckinit) return;

            MattimonServiceCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = interval
            };
            MattimonServiceCheckTimer.Tick += (s, e) =>
            {
                foreach (DataGridViewRow row in ucMattimonServicesGrid1.GetDataGridView().Rows)
                {
                    try
                    {
                        string svcName = row.Cells[GUI.Controls.UCMattimonServicesGrid.ColumnNames.colSvcName.ToString()].Value.ToString();
                        ucMattimonServicesGrid1.RefreshDataGridViewEntry(svcName);
                    }
                    catch (Exception ex)
                    {
                        GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An error occurred while refreshing the service statuses.\n\n" +
                            "Error details: " + ex.Message + "\n\n" + ex.StackTrace,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            MattimonServiceCheckTimer.Enabled = true;
            MattimonServiceCheckTimer.Start();
            svccheckinit = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        /// <param name="serviceStart"></param>
        /// <returns></returns>
        public MyServiceController.ServiceStart SetServiceStart(string svcName, MyServiceController.ServiceStart serviceStart)
        {
            switch (serviceStart)
            {
                case MyServiceController.ServiceStart.Automatic:
                    MyServiceController.SetRecoveryOptions(svcName);
                    MyServiceController.SetDelayedAutoStart(svcName);
                    break;
                default:
                    MyServiceController.SetServiceStart(svcName, serviceStart);
                    break;
            }

            return MyServiceController.GetServiceStart(svcName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        /// <returns></returns>
        public MyServiceController.ServiceState SwitchServiceStatus(string svcName)
        {
            if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Running)
            {
                MyServiceController.StartService(svcName);
            }
            else
            {
                MyServiceController.StopService(svcName);
            }

            return MyServiceController.GetServiceStatus(svcName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServicesDataGrid1_ServiceStartChanged(object sender, GUI.Events.ServiceStartChangedEventArgs e)
        {
            if (e.SelectedServiceStart == e.ServiceStartBefore)
                return;

            try
            {
                MattimonAgentLibrary.Tools.MyServiceController.ServiceStart newStart;
                using (new WaitCursor())
                {
                    newStart = SetServiceStart(e.ServiceName, e.SelectedServiceStart);
                    ucMattimonServicesGrid1.RefreshDataGridViewEntry(e.ServiceName);
                }
                //GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service status changed.\nCurrent service start: " + newStart.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An error occurred while switching service start type.\n\nError details:\n" +
                    ex.Message + "\n\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServicesDataGrid1_ServiceStateChanged(object sender, GUI.Events.ServiceStateChangedEventArgs e)
        {
            try
            {
                MattimonAgentLibrary.Tools.MyServiceController.ServiceState newState;
                using (new WaitCursor())
                {
                    string svcName = e.ServiceName;
                    newState = SwitchServiceStatus(svcName);
                    ucMattimonServicesGrid1.RefreshDataGridViewEntry(svcName);
                }
                //GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service status changed.\nCurrent status: " + newState.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "An error occurred while switching service status.\n\nError details:\n" +
                    ex.Message + "\n\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstallOrUninstallMattimonSqlServerService()
        {
            string svcPath = null, svcName = null, svcProd = null, svcVers = null;

            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                          MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                          "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");

            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch { return; }

            svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();
            
            // If not installed
            if (!MyServiceController.ServiceIsInstalled(svcName))
            {
                FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                    this, new DoWorkEventHandler((s, e) =>
                    {
                        MyServiceController.InstallAndStart(svcName, svcProd, svcPath);
                        object[] result = new object[6];
                        result[0] = MyServiceController.ServiceIsInstalled(svcName);
                        result[1] = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                        result[2] = svcName;
                        result[3] = svcProd;
                        result[4] = svcVers;
                        result[5] = null;

                        if ((bool)result[0])
                        {
                            try
                            {
                                MyServiceController.SetRecoveryOptions(svcName);
                                MyServiceController.SetDelayedAutoStart(svcName);
                            }
                            catch (Exception ex)
                            {
                                result[5] = ex;
                            }
                        }

                        e.Result = result;

                    }), "Installing")
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        if (e.Result != null && e.Result is object[] result)
                        {
                            bool installed = Convert.ToBoolean(result[0]);
                            bool running = Convert.ToBoolean(result[1]);
                            string name = Convert.ToString(result[2]);
                            string prod = Convert.ToString(result[3]);
                            string vers = Convert.ToString(result[4]);
                            Exception optionException = (Exception)result[5];


                            if (installed && running)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have is now installed and running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (installed)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been installed but isn't currently running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                if (e.Error != null)
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempting to install " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not install " + prod + " at this time.\n" +
                                        "You may view Application event log entries in the event viewer for possible related errors.",
                                        "Unexpected Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            if (optionException != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempted to set options on " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    })
                };
                frmInstaller.DoWork();

            } 

            else
            {
                FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                   this, new DoWorkEventHandler((s, e) =>
                   {
                       MyServiceController.StopService(svcName);
                       if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                       {
                           MyServiceController.Uninstall(svcName);
                       }
                       object[] result = new object[4];
                       result[0] = MyServiceController.ServiceIsInstalled(svcName) == false;
                       result[1] = svcName;
                       result[2] = svcProd;
                       result[3] = svcVers;

                       e.Result = result;

                   }), "Uninstalling " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        if (e.Result != null && e.Result is object[] result)
                        {
                            bool uninstalled = Convert.ToBoolean(result[0]);
                            string name = Convert.ToString(result[1]);
                            string prod = Convert.ToString(result[2]);
                            string vers = Convert.ToString(result[3]);

                            if (uninstalled)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been uninstalled.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                if (e.Error != null)
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempting to uninstall " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not uninstall " + prod + " at this time.\n" +
                                        "You may view Application event log entries in the event viewer for possible related errors.",
                                        "Unexpected Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    })
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstallMattimonSqlServerService()
        {
            string svcPath = null, svcName = null, svcProd = null, svcVers = null;

            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                          MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                          "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");

            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception e)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
            {
                FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                    this, new DoWorkEventHandler((s, e) =>
                    {
                        MyServiceController.InstallAndStart(svcName, svcProd, svcPath);
                        object[] result = new object[6];
                        result[0] = MyServiceController.ServiceIsInstalled(svcName);
                        result[1] = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                        result[2] = svcName;
                        result[3] = svcProd;
                        result[4] = svcVers;
                        result[5] = null;

                        if ((bool)result[0])
                        {
                            try
                            {
                                MyServiceController.SetRecoveryOptions(svcName);
                                MyServiceController.SetDelayedAutoStart(svcName);
                            }
                            catch (Exception ex)
                            {
                                result[5] = ex;
                            }
                        }

                        e.Result = result;

                    }), "Installing " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        if (e.Result != null && e.Result is object[] result)
                        {
                            bool installed = Convert.ToBoolean(result[0]);
                            bool running = Convert.ToBoolean(result[1]);
                            string name = Convert.ToString(result[2]);
                            string prod = Convert.ToString(result[3]);
                            string vers = Convert.ToString(result[4]);
                            Exception optionException = (Exception)result[5];


                            if (installed && running)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " is now installed and running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (installed)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been installed but isn't currently running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                if (e.Error != null)
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempting to install " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not install " + prod + " at this time.\n" +
                                        "You may view Application event log entries in the event viewer for possible related errors.",
                                        "Unexpected Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            if (optionException != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempted to set options on " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    })
                };
                frmInstaller.DoWork();

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="exeName">Do not include '.exe'</param>
        private void InstallMattimonService(string serviceName, string exeName)
        {
            exeName = Path.Combine(MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName), 
                exeName + ".exe");

            if (!File.Exists(exeName))
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service executable not found." +
                    "\nRe-install this application over the existing one.\n", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string svcPath = null, svcName = serviceName, svcProd = null, svcVers = null;

            try
            {
                svcPath = exeName; 

                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");

            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception e)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
            {
                FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                    this, new DoWorkEventHandler((s, e) =>
                    {
                        try
                        {
                            MyServiceController.InstallAndStart(svcName, svcProd, svcPath);
                        }
                        catch (Exception ex)
                        {
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this, 
                                "An error occurred while attempted to instan and start the service.\n\n" + 
                                Tools.ExceptionHelper.GetFormatedExceptionMessage(ex), svcName + " Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        object[] result = new object[6];
                        result[0] = MyServiceController.ServiceIsInstalled(svcName);
                        result[1] = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                        result[2] = svcName;
                        result[3] = svcProd;
                        result[4] = svcVers;
                        result[5] = null;

                        if ((bool)result[0])
                        {
                            try
                            {
                                MyServiceController.SetRecoveryOptions(svcName);
                                MyServiceController.SetDelayedAutoStart(svcName);
                            }
                            catch (Exception ex)
                            {
                                result[5] = ex;
                            }
                        }

                        e.Result = result;

                    }), "Installing " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        if (e.Result != null && e.Result is object[] result)
                        {
                            bool installed = Convert.ToBoolean(result[0]);
                            bool running = Convert.ToBoolean(result[1]);
                            string name = Convert.ToString(result[2]);
                            string prod = Convert.ToString(result[3]);
                            string vers = Convert.ToString(result[4]);
                            Exception optionException = (Exception)result[5];


                            if (installed && running)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " is now installed and running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (installed)
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been installed but isn't currently running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                if (e.Error != null)
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempting to install " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not install " + prod + " at this time.\n" +
                                        "You may view Application event log entries in the event viewer for possible related errors.",
                                        "Unexpected Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            if (optionException != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "An error occurred while attempted to set options on " + prod + ".\n" +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    })
                };
                frmInstaller.DoWork();

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="exeName"></param>
        private void UninstallMattimonService(string serviceName, string exeName)
        {
            exeName = Path.Combine(MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName), 
                exeName + ".exe");

            if (!File.Exists(exeName))
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service executable not found." +
                    "\nRe-install this application over the existing one.\n", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (!MyServiceController.ServiceIsInstalled(serviceName))
            {
                return;
            }

            string svcPath = null, svcName = null, svcProd = null, svcVers = null;

            try
            {
                svcPath = exeName;

                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");

            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception e)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }

            svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();



            FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                   this, new DoWorkEventHandler((s, e) =>
                   {

                       if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
                           MyServiceController.StopService(svcName);

                       if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                       {
                           MyServiceController.Uninstall(svcName);
                           while (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.NotFound) ;
                       }
                       object[] result = new object[4];
                       result[0] = MyServiceController.ServiceIsInstalled(svcName) == false;
                       result[1] = svcName;
                       result[2] = svcProd;
                       result[3] = svcVers;

                       e.Result = result;

                   }), "Uninstalling " + svcProd)
            {
                RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                {
                    if (e.Result != null && e.Result is object[] result)
                    {
                        bool uninstalled = Convert.ToBoolean(result[0]);
                        string name = Convert.ToString(result[1]);
                        string prod = Convert.ToString(result[2]);
                        string vers = Convert.ToString(result[3]);

                        if (uninstalled)
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been uninstalled.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "An error occurred while attempting to uninstall " + prod + ".\n" +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not uninstall " + prod + " at this time.\n" +
                                    "You may view Application event log entries in the event viewer for possible related errors.",
                                    "Unexpected Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                })
            };
            frmInstaller.DoWork();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="exeName">Do not include '.exe'</param>
        /// <param name="action"></param>
        /// <param name="showSuccessMessage"></param>
        private void StopMattimonService(string serviceName, string exeName, string action= "Stopping", bool showSuccessMessage = true)
        {
            exeName = Path.Combine(MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                exeName + ".exe");

            if (!File.Exists(exeName))
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service executable not found." +
                    "\nRe-install this application over the existing one.\n", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string svcPath = null;
            try
            {
                svcPath = exeName;

                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");
            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            catch { return; }


            string svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            string svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            string svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
                return;

            if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
            {
                FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this, new DoWorkEventHandler((s, e) =>
                {
                    MyServiceController.StopService(svcName);
                    e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped;
                }), action + " " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        bool stopped = (bool)e.Result;

                        if (!stopped)
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "Could not stop " + svcProd + ".\nError details: " +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not stop " + svcProd + " at this time.\n" +
                               "You may view Application event log entries in the event viewer for possible related errors.",
                               "Unexpected Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            if (showSuccessMessage)
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    svcProd + " have been stopped.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    })
                };
                frmSvcController.DoWork();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="exeName">Do not include '.exe'</param>
        /// <param name="action"></param>
        /// <param name="showSuccessMessage"></param>
        private void StartMattimonService(string serviceName, string exeName, string action = "Starting", bool showSuccessMessage = true)
        {
            exeName = Path.Combine(MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                exeName + ".exe");

            if (!File.Exists(exeName))
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Service executable not found." +
                    "\nRe-install this application over the existing one.\n", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string svcPath = null;
            try
            {
                svcPath = exeName;

                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");
            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            catch { return; }


            string svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            string svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            string svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
                return;

            if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
            {
                FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this, new DoWorkEventHandler((s, e) =>
                {
                    MyServiceController.StartService(svcName);
                    e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                }), action + " " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        bool started = (bool)e.Result;

                        if (!started)
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "Could not start " + svcProd + ".\nError details: " +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not start " + svcProd + " at this time.\n" +
                               "You may view Application event log entries in the event viewer for possible related errors.",
                               "Unexpected Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            if (showSuccessMessage)
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    svcProd + " have been started.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    })
                };
                frmSvcController.DoWork();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UninstallMattimonSqlServerService()
        {
            string svcPath = null, svcName = null, svcProd = null, svcVers = null;

            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                          MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                          "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");

            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception e)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }

            svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
            {
                return;
            }

            FormBackgroundWorker frmInstaller = new FormBackgroundWorker(
                   this, new DoWorkEventHandler((s, e) =>
                   {

                       if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
                           MyServiceController.StopService(svcName);

                       if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                       {
                           MyServiceController.Uninstall(svcName);
                           while (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.NotFound) ;
                       }
                       object[] result = new object[4];
                       result[0] = MyServiceController.ServiceIsInstalled(svcName) == false;
                       result[1] = svcName;
                       result[2] = svcProd;
                       result[3] = svcVers;

                       e.Result = result;

                   }), "Uninstalling " + svcProd)
            {
                RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                {
                    if (e.Result != null && e.Result is object[] result)
                    {
                        bool uninstalled = Convert.ToBoolean(result[0]);
                        string name = Convert.ToString(result[1]);
                        string prod = Convert.ToString(result[2]);
                        string vers = Convert.ToString(result[3]);

                        if (uninstalled)
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            GUI.BitscoreForms.BitscoreMessageBox.Show(this, prod + " have been uninstalled.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "An error occurred while attempting to uninstall " + prod + ".\n" +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error),
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not uninstall " + prod + " at this time.\n" +
                                    "You may view Application event log entries in the event viewer for possible related errors.",
                                    "Unexpected Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                })
            };
            frmInstaller.DoWork();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopOrStartMattimonSqlServerService()
        {
            string svcPath = null;
            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                      MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                      "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");
            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }
            catch { return; }

            string svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            string svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            string svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (MyServiceController.ServiceIsInstalled(svcName))
            {
                if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
                {
                    FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this, new DoWorkEventHandler((s, e) =>
                    {
                        MyServiceController.StopService(svcName);
                        e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped;
                    }), "Stopping " + svcProd)
                    {
                        RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                        {
                            bool stopped = (bool)e.Result;

                            if (!stopped)
                            {
                                if (e.Error != null)
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        "Could not stop " + svcProd + ".\nError details: " +
                                        Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not stop " + svcProd + " at this time.\n" +
                                   "You may view Application event log entries in the event viewer for possible related errors.",
                                   "Unexpected Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                ucMattimonServicesGrid1.ReloadDataGridView();
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    svcProd + " have been stopped.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        })
                    };
                    frmSvcController.DoWork();
                }

                else
                {
                    if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
                    {
                        FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this,
                            new DoWorkEventHandler((s, e) =>
                            {
                                MyServiceController.StartService(svcName);
                                e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                            }), "Starting " + svcProd)
                        {
                            RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                            {
                                bool started = (bool)e.Result;
                                if (!started)
                                {
                                    if (e.Error != null)
                                    {
                                        GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                            "Could not start " + svcProd + ".\nError details: " +
                                            Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not start " + svcProd + " at this time.\n" +
                                       "You may view Application event log entries in the event viewer for possible related errors.",
                                       "Unexpected Error",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    ucMattimonServicesGrid1.ReloadDataGridView();
                                    GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                        svcProd + " is now running.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            })
                        };
                        frmSvcController.DoWork();
                    }
                }
            }       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="showSuccessMessage"></param>
        private void StopMattimonSQLServerService(string action = "Stopping", bool showSuccessMessage = true)
        {
            string svcPath = null;
            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                      MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                      "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");
            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            catch { return; }


            string svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            string svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            string svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
                return;

            if (MyServiceController.GetServiceStatus(svcName) != MyServiceController.ServiceState.Stopped)
            {
                FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this, new DoWorkEventHandler((s, e) =>
                {
                    MyServiceController.StopService(svcName);
                    e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped;
                }), action + " " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        bool stopped = (bool)e.Result;

                        if (!stopped)
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "Could not stop " + svcProd + ".\nError details: " +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not stop " + svcProd + " at this time.\n" +
                               "You may view Application event log entries in the event viewer for possible related errors.",
                               "Unexpected Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            if (showSuccessMessage)
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    svcProd + " have been stopped.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    })
                };
                frmSvcController.DoWork();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="showSuccessMessage"></param>
        private void StartMattimonSQLServerService(string action = "Starting", bool showSuccessMessage = true)
        {
            string svcPath = null;
            try
            {
                svcPath = Path.Combine(RegistryTools.GetInstallLocationByDisplayName(
                      MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName),
                      "MattimonSQLServerService.exe");
                if (!File.Exists(svcPath))
                    throw new IOException("Could not locate the service file: " + svcPath + ".\n" +
                    "Re-installing the application might fix this issue.");
            }
            catch (IOException ioe)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, ioe.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            catch { return; }


            string svcName = new ProjectAssemblyAtrributes(svcPath).AssemblyTitle;
            string svcProd = new ProjectAssemblyAtrributes(svcPath).AssemblyProduct;
            string svcVers = new ProjectAssemblyAtrributes(svcPath).GetAssemblyVersion().ToString();

            if (!MyServiceController.ServiceIsInstalled(svcName))
                return;

            if (MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Stopped)
            {
                FormBackgroundWorker frmSvcController = new FormBackgroundWorker(this, new DoWorkEventHandler((s, e) =>
                {
                    MyServiceController.StartService(svcName);
                    e.Result = MyServiceController.GetServiceStatus(svcName) == MyServiceController.ServiceState.Running;
                }), action + " " + svcProd)
                {
                    RunWorkerCompletedEventHandler = new RunWorkerCompletedEventHandler((s, e) =>
                    {
                        bool started = (bool)e.Result;

                        if (!started)
                        {
                            if (e.Error != null)
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    "Could not start " + svcProd + ".\nError details: " +
                                    Tools.ExceptionHelper.GetFormatedExceptionMessage(e.Error), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Could not start " + svcProd + " at this time.\n" +
                               "You may view Application event log entries in the event viewer for possible related errors.",
                               "Unexpected Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            ucMattimonServicesGrid1.ReloadDataGridView();
                            if (showSuccessMessage)
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                                    svcProd + " have been started.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    })
                };
                frmSvcController.DoWork();
            }
        }
    }
}
