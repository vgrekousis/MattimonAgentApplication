using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Uninstall
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Tools.RegistryTools.IsSoftwareInstalled(Static.MattimonRegistrySubkeyNames.DisplayName))
            {
                MessageBox.Show(Static.MattimonRegistrySubkeyNames.DisplayName + " is not installed.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormUninstaller());
        }
    }
}
