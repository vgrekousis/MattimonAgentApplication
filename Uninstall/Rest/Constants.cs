﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Rest
{
    public class Constants
    {
        private const Boolean devURLActive = false;
        public static String GetActiveWebAppURL()
        {
            return devURLActive ?
                DEV_MATTIMON_WEB_APP_URL : MATTIMON_WEB_APP_URL;
        }
        public static String GetActiveWebAppUpdateURL()
        {
            return devURLActive ?
                DEV_MATTIMON_UPDATE_WEB_APP_URL : MATTIMON_UPDATE_WEB_APP_URL;
        }
        public static String GetActiveWebAppUpdaterURL()
        {
            return devURLActive ?
                DEV_MATTIMON_UPDATER_WEB_APP_URL : MATTIMON_UPDATER_WEB_APP_URL;
        }
        public static String GetActiveWebsiteURL()
        {
            return devURLActive ?
                DEV_MATTIMON_WEBSITE_URL : MATTIMON_WEBSITE_URL;
        }
        public static String GetActiveDeviceControllerURL()
        {
            return devURLActive ?
                DevDeviceURL : DeviceURL;
        }
        public static String GetActiveDeviceOptionsControllerURL()
        {
            return devURLActive ?
                DevDeviceOptionsURL : DeviceOptionsURL;
        }
        public static String GetActiveAuthenticationContollerURL()
        {
            return devURLActive ?
                DevAuthenticationURL : AuthenticationURL;
        }
        public static String GetActiveEventLogsURL()
        {
            return devURLActive ?
                DevEventLogsURL : EventLogsURL;
        }
        public static String GetActiveSQLServerURL()
        {
            return devURLActive ?
                DevSQLServerURL : SQLServerURL;
        }
        public static String GetActiveWebApplicationBaseURL()
        {
            return devURLActive ?
                DevMattimonWebApplicationBaseURL : MattimonWebApplicationBaseURL;
        }
        /// <summary>
        /// Dev Base app url 
        /// </summary>
        /// 
        public const String DEV_MATTIMON_WEB_APP_URL = "https://devapi02.mattimon.com";
        /// <summary>
        /// Production Base app url
        /// </summary>
        public const String MATTIMON_WEB_APP_URL = "https://api01.mattimon.com";
        /// <summary>
        /// 
        /// </summary>
        public const String DEV_MATTIMON_HOSTNAME = "devapi02.mattimon.com";
        /// <summary>
        /// 
        /// </summary>
        public const String MATTIMON_WEBSITE_HOSTNAME = "monitoring.mattimon.com";
        /// <summary>
        /// 
        /// </summary>
        public const String DEV_MATTIMON_WEBSITE_URL = "https://mattidev01.mattimon.com";
        public const String MATTIMON_WEBSITE_URL = "https://monitoring.mattimon.com";
        /// <summary>
        /// 
        /// </summary>
        public const String DEV_MATTIMON_UPDATE_WEB_APP_URL = DEV_MATTIMON_WEBSITE_URL + "/" + "MattimonAgent/index.php";
        public const String MATTIMON_UPDATE_WEB_APP_URL = MATTIMON_WEBSITE_URL + "/" + "MattimonAgent/index.php";


        public const String DEV_MATTIMON_UPDATER_WEB_APP_URL = DEV_MATTIMON_WEBSITE_URL + "/" + "MattimonUpdater/index.php";
        public const String MATTIMON_UPDATER_WEB_APP_URL = MATTIMON_WEBSITE_URL + "/" + "MattimonUpdater/index.php";
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
        /// <summary>
        /// 
        /// </summary>
        public static String DevDeviceURL { get { return DEV_MATTIMON_WEB_APP_URL + DEVICE_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DeviceURL { get { return MATTIMON_WEB_APP_URL + DEVICE_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DevDeviceOptionsURL { get { return DEV_MATTIMON_WEB_APP_URL + DEVICE_OPTIONS_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DeviceOptionsURL { get { return MATTIMON_WEB_APP_URL + DEVICE_OPTIONS_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DevAuthenticationURL { get { return DEV_MATTIMON_WEB_APP_URL + AUTHENTICATION_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String AuthenticationURL { get { return MATTIMON_WEB_APP_URL + AUTHENTICATION_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String EventLogsURL { get { return MATTIMON_WEB_APP_URL + EVTLOGS_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DevEventLogsURL { get { return DEV_MATTIMON_WEB_APP_URL + EVTLOGS_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String SQLServerURL { get { return MATTIMON_WEB_APP_URL + SQLSRV_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DevSQLServerURL { get { return DEV_MATTIMON_WEB_APP_URL + SQLSRV_DIR; } }
        /// <summary>
        /// 
        /// </summary>
        public static String DevMattimonWebApplicationBaseURL { get { return DEV_MATTIMON_WEB_APP_URL; } }
        /// <summary>
        /// 
        /// </summary>
        public static String MattimonWebApplicationBaseURL { get { return MATTIMON_WEB_APP_URL; } }
    }
}
