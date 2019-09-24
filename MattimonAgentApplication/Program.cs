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
using System.Management;

namespace MattimonAgentApplication
{
    static class Program
    {
        private static readonly bool PreventRunningIfNotInstalled = true;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Boolean MattimonWebApiReplies()
        {
            APIInfoRequest aPIInfoRequest = new APIInfoRequest();
            APIInfo info = aPIInfoRequest.GetAPIInfo();
            if (info.HttpStatusCode != HttpStatusCode.OK)
                return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool APIResponds()
        {
            try
            {
                if (!MattimonWebApiReplies())
                {
                    new FormError(MattimonAgentApplicationAssemblyAttributes.AssemblyProduct,
                        "We're sorry. Our server is down or the web service is currently unavailable.\n" +
                        "Please, try again later.").Show();
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
        private static bool ApplicationRegistryKeyExists()
        {
            if (PreventRunningIfNotInstalled && (!IOTools.IsSoftwareInstalled(
                MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName)))
            {
                MessageBox.Show(MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName +
                    " is not installed.", MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static bool DatabaseFileExists(out MattimonSQLite.SQLiteClientDatabase db)
        {
            try
            {
                db = new MattimonSQLite.SQLiteClientDatabase(
                    MattimonAgentLibrary.Static.Constants.CommonAppData,
                    MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.Publisher,
                    MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName,
                    MattimonAgentLibrary.Static.Constants.LocalDatabaseName);
            }
            catch (Exception ex)
            {
                db = null;
                GUI.BitscoreForms.BitscoreMessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Local Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!db.FileExists(out string foundDatabaseFilePath))
            {
                db = null;
                new FormError(MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName,
                    "The program can't find the database file on your machine. Please re-install the application.");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Rest Request
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private static bool DeviceEntryExist(long deviceId)
        {
            DeviceRequests deviceRequests = new DeviceRequests();
            Boolean deviceIdExisting = deviceRequests.DeviceEntryExist(deviceId);

            if (!deviceIdExisting)
            {
                DialogResult dr = GUI.BitscoreForms.BitscoreMessageBox.Show(
                    String.Format("The program can't find the device (#{0}) entry on the server.\n", deviceId) +
                    "Click \"OK\" to fix this issue by re-posting a new device entry on our server.\n\n" +
                    "Note: previous history will not longer be available.",
                    MattimonAgentApplicationAssemblyAttributes.AssemblyProduct,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                if (dr != DialogResult.OK)
                {
                    return false;
                }
                else
                {
                    FormBackgroundWorker worker = new FormBackgroundWorker(null, DoWork, "Re-posting device")
                    {
                        RunWorkerCompletedEventHandler = DoWorkCompleted
                    };
                    worker.DoWork();
                }
                return false;
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = Helper.RepostDevice();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DoWorkCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            if (e.Result != null && e.Result is bool ok == true)
                GUI.BitscoreForms.BitscoreMessageBox.Show(
                           "Your device have been posted.\nPlease restart the application to continue.",
                           "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool IsRunning()
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                new FormError(MattimonAgentApplicationAssemblyAttributes.AssemblyProduct,
                        "Another instance of " + MattimonAgentApplicationAssemblyAttributes.AssemblyProduct + " is currently running.\nClose the running instance then try again.").Show();
                return true;
            }
            return false;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (IsRunning())
                    return;

                //String version = new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(
                //    Application.ExecutablePath, true).GetAssemblyVersion().ToString();

                //MessageBox.Show(version);

                if (!APIResponds())
                    return;

                if (!ApplicationRegistryKeyExists())
                    return;

                if (!DatabaseFileExists(out MattimonSQLite.SQLiteClientDatabase db))
                    return;

                if (db != null)
                    if (!DeviceEntryExist(db.GetDeviceId()))
                        return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormDashboard());
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show(ex.ToString());
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());//Tools.ExceptionHelper.GetFormatedExceptionMessage(ex));
                //GUI.BitscoreForms.BitscoreMessageBox.Show(Tools.ExceptionHelper.GetFormatedExceptionMessage(ex), "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


    public static class Helper
    {
        private static MattimonSQLite.SQLiteClientDatabase GetLocalDatabase()
        {
            return new MattimonSQLite.SQLiteClientDatabase(
                MattimonAgentLibrary.Static.Constants.CommonAppData,
                new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Application.ExecutablePath).AssemblyCompany,
                new MattimonAgentLibrary.Tools.ProjectAssemblyAtrributes(Application.ExecutablePath).AssemblyProduct,
                MattimonAgentLibrary.Static.Constants.LocalDatabaseName);
        }

        /// <summary>
        /// Use this only if the application is installed but
        /// the device entry is not longer found on the server.
        /// </summary>
        public static bool RepostDevice()
        {
            MattimonSQLite.SQLiteClientDatabase db = GetLocalDatabase();
            bool ok = PostDevice(db, out string errmsg);

            if (errmsg != "")
                throw (new Exception(errmsg));

            // Refresh the services
            List<GUI.Controls.UCMattimonServicesGrid.ServiceMetaData> services = GetMattimonServices();
            foreach (GUI.Controls.UCMattimonServicesGrid.ServiceMetaData s in services)
            {
                MyServiceController.StopService(s.Name);
                while (MyServiceController.GetServiceStatus(s.Name) != MyServiceController.ServiceState.Stopped) ;
                MyServiceController.StartService(s.Name);
            }
            return ok;
        }

        private static Boolean PostDevice(MattimonSQLite.SQLiteClientDatabase db, out string errorMessage)
        {
            MattimonAgentLibrary.WMI.WMIProvider provider = new MattimonAgentLibrary.WMI.WMIProvider();
            Device device = new Device(); DeviceOptions deviceOptions = new DeviceOptions();
            /// If Operating System string contains 'server', we'll suppose that the device is a server
            /// Otherwise, we'll suppose that the device is a workstation.
            Boolean isServer = provider.GetOperatingSystemString().ToLower().Contains("server");
            /// Device type id should match psm_device_types primary key
            /// '2' means Server and '3' means Workstation, according to the Mattimon database on server.
            int definedDeviceTypeId = isServer ? 2 : 3;

            //device.Port = 80;  // Port should not be defined in this context---the user should select an active port from his own machine using MattimonAgentApplication.
            device.BIOSSerialNumber = provider.GetBIOSSerialNumber();
            device.Company_Id = db.GetCompanyId();
            device.ComputerName = provider.GetComputerName();
            device.IpAddress = provider.GetIPAddress();
            device.MacAddress = provider.GetMacAddress();
            device.Model = provider.GetModel();
            device.OperatingSystem = provider.GetOperatingSystemString();
            device.OperatingSystemSerialNumber = provider.GetOSSerialNumber();
            device.User_Id = db.GetUserId();
            device.Device_Type_Id = definedDeviceTypeId;
            device.AgentReportInterval = db.GetReportingInterval();
            device.MonitorSql = 0;

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

                db.UpdateDeviceID(device.Device_Id);
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

        private static List<GUI.Controls.UCMattimonServicesGrid.ServiceMetaData> GetMattimonServices()
        {
            String svcParentDirectoryPath = MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName); //System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location.ToString()).FullName;

            List<GUI.Controls.UCMattimonServicesGrid.ServiceMetaData> foundControllers = null;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();
                foundControllers = new List<GUI.Controls.UCMattimonServicesGrid.ServiceMetaData>();

                foreach (ManagementObject obj in collection)
                {
                    string name = obj["Name"] as string;
                    string pathName = obj["PathName"] as string;

                    if (!System.IO.File.Exists(pathName)) continue;

                    string parent = System.IO.Directory.GetParent(pathName).FullName;
                    if (parent != svcParentDirectoryPath) continue;

                    try
                    {
                        System.ServiceProcess.ServiceController svcCtrl = System.ServiceProcess.ServiceController.GetServices(System.Environment.MachineName).
                            Where(svc => svc.ServiceName == name).FirstOrDefault();

                        if (svcCtrl != null)
                        {
                            GUI.Controls.UCMattimonServicesGrid.ServiceMetaData data = new GUI.Controls.UCMattimonServicesGrid.ServiceMetaData(svcCtrl, pathName);
                            foundControllers.Add(data);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(ex.Message + "\n\n" + ex.StackTrace,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return foundControllers;
        }
    }
}
