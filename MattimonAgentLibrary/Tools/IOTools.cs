using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Tools
{
    public class IOTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectoryAndContent(String path)
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
        public static void DeleteDatabaseDirectory()
        {
            if (System.IO.File.Exists(Static.Constants.LocalDatabasePath))
            {
                DeleteDirectoryAndContent(
                    Static.Constants.DatabaseBaseDirectoryPath);
            }
        }
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
    }
}
