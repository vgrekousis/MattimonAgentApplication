using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MattimonAgentLibrary
{
    public class WinServiceInstaller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exeFilename">Installer class (not the service class!)</param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool InstallService(string exeFilename, out Exception exception)
        {
            exception = null;
            string[] commandLineOptions = new string[1] { "/LogFile=install.log" };

            System.Configuration.Install.AssemblyInstaller installer = 
                new System.Configuration.Install.AssemblyInstaller(exeFilename, commandLineOptions);

            try
            {
                installer.UseNewContext = true;
                installer.Install(null);
                installer.Commit(null);
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exeFilename">Installer class (not the service class!)</param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool UninstallService(string exeFilename, out Exception exception)
        {
            exception = null;
            string[] commandLineOptions = new string[1] { "/LogFile=uninstall.log" };

            System.Configuration.Install.AssemblyInstaller installer = 
                new System.Configuration.Install.AssemblyInstaller(exeFilename, commandLineOptions);

            try
            {
                installer.UseNewContext = true;
                installer.Uninstall(null);
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            return true;
        }

        public static bool ManagedInstallService(String exePath, out Exception exception)
        {
            exception = null;
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { exePath });
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            return true;
        }

        public static bool ManagedUninstallService(String exePath, out Exception exception)
        {
            exception = null;
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", exePath });
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            return true;
        }
    }   
}
