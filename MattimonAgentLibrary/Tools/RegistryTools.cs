using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Tools
{
    public class RegistryTools
    {
        /// <summary>
        /// Updates the display version in the already existing Uninstall entry in the registry.
        /// </summary>
        /// <param name="displayName">The application's display name as shown in Uninstall Programs.</param>
        /// <param name="assemblyVersion">The version of the assembly</param>
        public static void UpdateDisplayVersion(String displayName, String assemblyVersion)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static String GetInstallLocationByDisplayName(String displayName)
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (regKey.GetValue("DisplayName").ToString() == displayName)
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                }
                catch { }
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regKeyName"></param>
        /// <param name="regKeyValue"></param>
        /// <returns></returns>
        public static String GetInstallLocation(String regKeyName, String regKeyValue)
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (regKey.GetValue(regKeyName).ToString() == regKeyValue)
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                }
                catch { }
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static String GetPublisherByDisplayName(String displayName)
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (regKey.GetValue("DisplayName").ToString() == displayName)
                    {
                        return regKey.GetValue("Publisher").ToString();
                    }
                }
                catch { }
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="softwareName">Must be equal to the <code>DisplayName</code> registry key value</param>
        /// <returns></returns>
        public static bool IsSoftwareInstalled(string softwareName)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") ??
                      Registry.LocalMachine.OpenSubKey(
                          @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

            if (key == null)
                return false;

            return key.GetSubKeyNames()
                .Select(keyName => key.OpenSubKey(keyName))
                .Select(subkey => subkey.GetValue("DisplayName") as string)
                .Any(displayName => displayName != null && displayName.Contains(softwareName));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveControlPanelProgram(String displayName)
        {
            try
            {
                string InstallerRegLoc = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

                RegistryKey homeKey = (Registry.LocalMachine).OpenSubKey(InstallerRegLoc, true);

                RegistryKey appSubKey = homeKey.OpenSubKey(
                     displayName);

                if (null != appSubKey)
                {
                    homeKey.DeleteSubKey(
                       displayName);
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }
}
