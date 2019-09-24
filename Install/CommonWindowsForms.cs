using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary;

namespace Install
{
    public static class CommonWindowsForms
    {
        public static Boolean PromptExit()
        {
            if (MessageBox.Show(
                Form.ActiveForm,
                "Are you sure you'd like to abort " +
                Static.ExecutingAssemblyAttributes.AssemblyProduct + " installation?",
                Static.ExecutingAssemblyAttributes.AssemblyTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return false;

            return true;
        }

        public static void ApplicationExit()
        {
            // Prevent these forms from handing form closing as
            // application exist is called in this form.
            foreach (Form f in Application.OpenForms)
            {
                if (f is Form0)
                {
                    Form0 f0 = (Form0)f;
                    f0.FormClosing -= f0.Form0_FormClosing;
                }
                if (f is Form1)
                {
                    Form1 f1 = (Form1)f;
                    f1.FormClosing -= f1.Form1_FormClosing;
                }
                if (f is Form2)
                {
                    Form2 f2 = (Form2)f;
                    f2.FormClosing -= f2.Form2_FormClosing;
                }
                if (f is Form3)
                {
                    Form3 f3 = (Form3)f;
                    f3.FormClosing -= f3.Form3_FormClosing;
                }
                if (f is Form4)
                {
                    Form4 f4 = (Form4)f;
                    f4.FormClosing -= f4.Form4_FormClosing;
                }
            }
            Application.Exit();
        }
    }
}
