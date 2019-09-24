using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonUpdateService.Tools
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
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
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
