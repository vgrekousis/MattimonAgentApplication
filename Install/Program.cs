using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Install
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Tools.IOTools.IsSoftwareInstalled(Static.ExecutingAssemblyAttributes.AssemblyProduct))
            {
                Boolean mainExeFound = false;

                String installDirectory = Tools.RegistryTools.GetInstallLocationByDisplayName(Static.ExecutingAssemblyAttributes.AssemblyProduct);
                String exeFullPath = System.IO.Path.Combine(installDirectory, "MattimonAgentApplication.exe");
                mainExeFound = System.IO.File.Exists(exeFullPath);

                if (mainExeFound)
                {
                    Tools.ProjectAssemblyAtrributes projectAssembly = new Tools.ProjectAssemblyAtrributes(exeFullPath, true);
                    short installedVersion = projectAssembly.AssemblyVersionToShort();
                    string strInstalledVersion = projectAssembly.GetAssemblyVersion().ToString();
                    projectAssembly = new Tools.ProjectAssemblyAtrributes(Application.ExecutablePath, true);
                    short installerVersion = projectAssembly.AssemblyVersionToShort();
                    string strInstallerVersion = projectAssembly.GetAssemblyVersion().ToString();

                    if (installerVersion < installedVersion)
                    {
                        MessageBox.Show(
                            "Seems like your installer's version is out of date.\n\n" +
                            "Installer version: " + strInstallerVersion + "\n" +
                            "Installed version: " + strInstalledVersion + "\n\n" +
                            "Please, download our latest installer from the Mattimon Monitoring website.",
                            "Version Conflit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (installedVersion == installerVersion)
                    {
                        DialogResult result = MessageBox.Show(
                            Static.ExecutingAssemblyAttributes.AssemblyProduct + " is already installed. " +
                            "Re-installing will overwrite the existing version of the program.\nIf this is what you want, " +
                            "click \"Yes\" to continue.", Static.ExecutingAssemblyAttributes.AssemblyTitle,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                        if (result != DialogResult.Yes)
                            return;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show(
                           "Seems like a previous version of " + Static.ExecutingAssemblyAttributes.AssemblyProduct + " is already installed." +
                           "Re-installing will overwrite the existing version of the program.\n" +
                           "If this is what you want, click \"Yes\" to continue.", Static.ExecutingAssemblyAttributes.AssemblyTitle,
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result != DialogResult.Yes)
                            return;
                    }

                }
               
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form0());
        }
    }
}
