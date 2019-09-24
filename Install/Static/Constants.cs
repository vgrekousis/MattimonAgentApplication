using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Install.Static
{
    public class Constants
    {
        /// <summary>
        /// Application base directory name
        /// </summary>
        public static String ApplicationBaseDirectory
        {
            get { return ExecutingAssemblyAttributes.AssemblyCompany; }
        }
        /// <summary>
        /// Database base directory name
        /// </summary>
        public static String DatabaseBaseDirectory
        {
            get { return ApplicationBaseDirectory; }
        }
        /// <summary>
        /// Application directory name
        /// </summary>
        public static String ApplicationDirectory
        {
            get { return ExecutingAssemblyAttributes.AssemblyCompany + @"\" + 
                    ExecutingAssemblyAttributes.AssemblyProduct; }
        }
        /// <summary>
        /// Database directory name
        /// </summary>
        public static String DatabaseDirectory
        {
            get { return ApplicationDirectory; }
        }


        /// <summary>
        /// Program files full path
        /// </summary>
        public static String ProgramFiles
        {
            get { return Environment.ExpandEnvironmentVariables("%ProgramW6432%"); }
        }
        /// <summary>
        /// Common app data full path
        /// </summary>
        public static String CommonAppData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
        }

        /// <summary>
        /// Application base directory full path
        /// </summary>
        public static String ApplicationBaseDirectoryPath
        {
            get { return ProgramFiles + @"\" + ApplicationBaseDirectory; }
        }
        /// <summary>
        /// Database base directory full path
        /// </summary>
        public static String DatabaseBaseDirectoryPath
        {
            get { return CommonAppData + @"\" + DatabaseBaseDirectory; }
        }

        /// <summary>
        /// Application directory full path
        /// </summary>
        public static String ApplicationDirectoryPath
        {
            get
            {
                return ProgramFiles + @"\" + 
                    ExecutingAssemblyAttributes.AssemblyCompany + @"\" + 
                    ExecutingAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// Database directory full path
        /// </summary>
        public static String DatabaseDirectoryPath
        {
            get
            {
                return CommonAppData + @"\" +
                    ExecutingAssemblyAttributes.AssemblyCompany + @"\" +
                    ExecutingAssemblyAttributes.AssemblyProduct;
            }
        }
        /// <summary>
        /// Database filename
        /// </summary>
        public static String LocalDatabaseName
        {
            get { return "mattimon.sqlite"; }
        }
        /// <summary>
        /// Database filename (full path)
        /// </summary>
        public static String LocalDatabasePath
        {
            get { return DatabaseDirectoryPath + @"\" + LocalDatabaseName; }
        }  
    }

    public class MattimonRegistrySubkeyNames
    {
        /// <summary>
        /// 
        /// </summary>
        public static String DisplayName
        {
            get { return "Mattimon Agent"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String Publisher
        {
            get { return "Bitscore Technologies"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static String Version
        {
            get { return "1.0.0.0"; }
        }
    }
}
