using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall
{
    public class IOTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectoryAndContent(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo baseDir = new DirectoryInfo(path);


                string[] basedirs = Directory.GetDirectories(baseDir.FullName);
                foreach (string dir in basedirs)
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
    }
}
