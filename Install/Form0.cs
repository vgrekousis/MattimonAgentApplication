using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary.Models;
namespace Install
{
    public partial class Form0 : FormCommon
    {
        /// <summary>
        /// Next Form
        /// </summary>
        private Form1 Form1;
        /// <summary>
        /// 
        /// </summary>
        public Form0() : base()
        {
            InitializeComponent();
            Form1 = new Form1();
            Text = base.Text;
            SetText(label1, "The installer will guide you through the steps required to install " + Static.ExecutingAssemblyAttributes.AssemblyProduct + " to your computer.");
            SetText(label2, "NOTE: This computer program is protected by copyright law and international treaties.\n" +
                "Unauthorized duplication or distribution of this program, or any portion of it, may result in severe civil\n" +
                "or criminal penalties, and will be prosecuted to the maximum extent possible under the law.");
            btnCancel.Click += btnCancel_Click;
            btnPrevious.Click += btnPrevious_Click;
            btnNext.Click += btnNext_Click;
        }




        new public SizeF ClientSize
        {
            get { return base.ClientSize; }
            set { }
        }

        new public String Text
        {
            get { return base.Text; }
            set { }
        }

        private void SetText(Control control, String text)
        {
            control.Text = text;
        }
        private void AppendText(Control control, String text)
        {
            control.Text += text;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Form1.FormClosed -= Form1_FormClosed;
            Form1.FormClosed += Form1_FormClosed;
            Form1.Show();
            Form1.Location = Location;
            Hide();
        }

        public void Form0_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CommonWindowsForms.PromptExit())
            {
                e.Cancel = true;
            }
            else
            {
                using (new WaitCursor())
                    // Delete the database as the installation was canceled.
                    Tools.IOTools.DeleteDatabaseDirectory();

                // Prevent these forms from handing form closing as
                // application exit is called in this form.
                foreach (Form f in Application.OpenForms)
                {
                    if (f == this) // is Form0
                    {
                        FormClosing -= Form0_FormClosing;
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
        private void Form1_FormClosed(object sender, EventArgs e)
        {
            Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {

        }
    }
}
