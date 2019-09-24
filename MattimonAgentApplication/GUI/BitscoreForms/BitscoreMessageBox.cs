using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.BitscoreForms
{
    public class BitscoreMessageBox
    {
        public static DialogResult Show(IWin32Window owner, String message, String title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (InternalMessageBox imb = new InternalMessageBox())
            {
                return imb.Show(owner, message, title, buttons, icon);
            }
        }
        public static DialogResult Show(String message, String title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (InternalMessageBox imb = new InternalMessageBox())
            {
                return imb.Show(message, title, buttons, icon);
            }
        }
        public static DialogResult Show(String message, String title, MessageBoxButtons buttons)
        {
            using (InternalMessageBox imb = new InternalMessageBox())
            {
                return imb.Show(message, title, buttons, MessageBoxIcon.None);
            }
        }

        public static DialogResult Show(String message, String title)
        {
            using (InternalMessageBox imb = new InternalMessageBox())
            {
                return imb.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }
    }
}
