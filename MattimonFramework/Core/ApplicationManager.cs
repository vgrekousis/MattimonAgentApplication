using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonFramework.Core
{
    public class ApplicationManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultConnectionString()
        {
            try
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings[
                    System.Configuration.ConfigurationManager.AppSettings["defaultConnectionStringName"]].ConnectionString;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultWepAPIURL()
        {
            return System.Configuration.ConfigurationManager.AppSettings["defaultWebApi"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultWebsiteURL()
        {
            return System.Configuration.ConfigurationManager.AppSettings["defaultMattimonWebSite"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationTitle()
        {
            return System.Configuration.ConfigurationManager.AppSettings["appTitle"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUser()
        {
            return System.Environment.UserName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            return System.Environment.UserDomainName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationArea()
        {
            return System.Configuration.ConfigurationManager.AppSettings["appArea"];
        }
    }
}
