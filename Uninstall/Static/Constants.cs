using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Static
{
    public class Constants
    {
        public static string ApplicationBaseDirectory
        {
            get { return EntryAssemblyAttributes.AssemblyCompany; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string DatabaseBaseDirectory
        {
            get { return ApplicationBaseDirectory; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string ApplicationDirectory
        {
            get { return EntryAssemblyAttributes.AssemblyCompany + @"\" +
                    EntryAssemblyAttributes.AssemblyProduct; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string DatabaseDirectory
        {
            get { return ApplicationDirectory; }
        }


        /// <summary>
        /// 
        /// </summary>
        public static string ProgramFiles
        {
            get { return Environment.ExpandEnvironmentVariables("%ProgramW6432%"); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string CommonAppData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string ApplicationBaseDirectoryPath
        {
            get { return ProgramFiles + @"\" + ApplicationBaseDirectory; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string DatabaseBaseDirectoryPath
        {
            get { return CommonAppData + @"\" + DatabaseBaseDirectory; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string ApplicationDirectoryPath
        {
            get
            {
                return ProgramFiles + @"\" +
                    EntryAssemblyAttributes.AssemblyCompany + @"\" +
                    EntryAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string DatabaseDirectoryPath
        {
            get
            {
                return CommonAppData + @"\" +
                    EntryAssemblyAttributes.AssemblyCompany + @"\" +
                    EntryAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// Database filename only
        /// </summary>
        public static string LocalDatabaseName
        {
            get { return "mattimon.sqlite"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string LocalDatabasePath
        {
            get { return DatabaseDirectoryPath + @"\" + LocalDatabaseName; }
        }  
    }


    /// <summary>
    /// 
    /// </summary>
    public class MattimonEventLogConstants
    {
        public const string MainLogName = "Mattimon Services";
        public const string UpdaterSourceName = "MattimonUpdateSvc";
        public const string AgentSourceName = "MattimonAgentSvc";
        public const string SQLServerSourceName = "MattimonSQLServerSvc";
    }
}
