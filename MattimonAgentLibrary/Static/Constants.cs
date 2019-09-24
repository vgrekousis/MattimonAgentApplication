using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Static
{
    public class Constants
    {
        public static String ApplicationBaseDirectory
        {
            get { return MattimonAgentApplicationAssemblyAttributes.AssemblyCompany; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String DatabaseBaseDirectory
        {
            get { return ApplicationBaseDirectory; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String ApplicationDirectory
        {
            get { return MattimonAgentApplicationAssemblyAttributes.AssemblyCompany + @"\" + 
                    MattimonAgentApplicationAssemblyAttributes.AssemblyProduct; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String DatabaseDirectory
        {
            get { return ApplicationDirectory; }
        }


        /// <summary>
        /// 
        /// </summary>
        public static String ProgramFiles
        {
            get { return Environment.ExpandEnvironmentVariables("%ProgramW6432%"); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String CommonAppData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static String ApplicationBaseDirectoryPath
        {
            get { return ProgramFiles + @"\" + ApplicationBaseDirectory; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String DatabaseBaseDirectoryPath
        {
            get { return CommonAppData + @"\" + DatabaseBaseDirectory; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static String ApplicationDirectoryPath
        {
            get
            {
                return ProgramFiles + @"\" + 
                    MattimonAgentApplicationAssemblyAttributes.AssemblyCompany + @"\" + 
                    MattimonAgentApplicationAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String DatabaseDirectoryPath
        {
            get
            {
                return CommonAppData + @"\" +
                    MattimonAgentApplicationAssemblyAttributes.AssemblyCompany + @"\" +
                    MattimonAgentApplicationAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// Database filename only
        /// </summary>
        public static String LocalDatabaseName
        {
            get { return "mattimon.sqlite"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String LocalDatabasePath
        {
            get { return DatabaseDirectoryPath + @"\" + LocalDatabaseName; }
        }

        // MattimonAgentLibrary.Static
        /////////////////////////////// API CONSTANTS /////////////////////////////////
        private const Boolean devURLActive = false;
        /// <summary>
        /// Web API full url
        /// </summary>
        /// <returns></returns>
        public static String GetActiveWebAppURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL();
        }
        //public static String GetActiveWebAppUpdateURL()
        //{
        //    //return devURLActive ?
        //    //    DEV_MATTIMON_UPDATE_WEB_APP_URL : MATTIMON_UPDATE_WEB_APP_URL;
        //}
        public static String GetActiveWebsiteURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWebsiteURL();
        }
        public static String GetActiveDeviceControllerURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL() + DEVICE_DIR;
        }
        public static String GetActiveDeviceOptionsControllerURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL() + DEVICE_OPTIONS_DIR;
        }
        public static String GetActiveAuthenticationContollerURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL() + AUTHENTICATION_DIR;
        }
        public static String GetActiveEventLogsURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL() + EVTLOGS_DIR;
        }
        public static String GetActiveSQLServerURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWepAPIURL() + SQLSRV_DIR;
        }
        public static String GetActiveUpdateWebAppURL()
        {
            return MattimonFramework.Core.ApplicationManager.GetDefaultWebsiteURL() + WEBSITE_UPD_DIR;
        }

        /// <summary>
        /// 
        /// </summary>
        public const String WEBSITE_UPD_DIR = "MattimonAgent/index.php";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String DEVICE_DIR = "api/device";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String DEVICE_OPTIONS_DIR = "api/deviceoptions";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String AUTHENTICATION_DIR = "api/authentication";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String API_INFO_DIR = "api/apiinfo";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String API_USERINFO_DIR = "api/userinfos";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String EVTLOGS_DIR = "api/eventlogs";
        /// <summary>
        /// Generated root by the API Controller Classes
        /// </summary>
        public const String SQLSRV_DIR = "api/sqlserver";
    }

    public class MattimonEventLogConstants
    {
        public const String MainLogName = "Mattimon Services";
        public const String UpdaterSourceName = "MattimonUpdateSvc";
        public const String AgentSourceName = "MattimonAgentSvc";
        public const String MattimonEventLogSourceName = "MattimonEvtLogSvc";
        public const String SQLServerSourceName = "MattimonSQLServerSvc";
    }
}
