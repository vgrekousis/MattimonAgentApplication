using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary;
using MattimonAgentLibrary.Models;

namespace Install
{
    public partial class Form2 : FormCommon
    {
        /// <summary>
        /// Device Options
        /// </summary>
        private DeviceOptions DeviceOptions;
        /// <summary>
        /// Authenticated user
        /// </summary>
        private UserAuthentication UserAuthentication;
        /// <summary>
        /// Previous Form
        /// </summary>
        private Form1 Form1;
        /// <summary>
        /// Next Form
        /// </summary>
        private Form3 Form3;

        public Form2(Form1 form1, UserAuthentication userAuthentication, DeviceOptions deviceOptions) : base()
        {
            InitializeComponent();

            Form1 = form1;
            UserAuthentication = userAuthentication;
            DeviceOptions = deviceOptions;

            btnCancel.Click += btnCancel_Click;
            btnPrevious.Click += btnPrevious_Click;
            btnPrevious.Enabled = true;
            btnNext.Click += btnNext_Click;
            txtPath.Text = Static.Constants.ApplicationDirectoryPath;
            SetText(label1,
                "The installer will install " + Static.ExecutingAssemblyAttributes.AssemblyProduct +
                " to the following folder.\nTo install in this folder, click \"Next\"." +
                " To install to a different folder, click \"Browse\".");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Form2_FormClosing(object sender, FormClosingEventArgs e)
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
                    if (f == this) // is Form2
                    {
                        FormClosing -= Form2_FormClosing;
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
        private void Form3_FormClosed(object sender, EventArgs e)
        {
            Show();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close(); // Form2_FormClosing
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Hide();
            Form1.Show();
            Form1.Location = Location;
            Form1.Focus();
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (Form3 == null)
                Form3 = new Form3(this, txtPath.Text, UserAuthentication, DeviceOptions);

            Form3.FormClosed -= Form3_FormClosed;
            Form3.FormClosed += Form3_FormClosed;
            Form3.Show();
            Form3.Location = Location;
            Hide();
        }
        private void SetText(Control c, String text)
        {
            c.Text = text;
        }

        new public Size ClientSize
        {
            get { return base.ClientSize; }
            set { }
        }

        new public String Text
        {
            get { return base.Text; }
            set { }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (Directory.GetFiles(fbd.SelectedPath).Length != 0 || Directory.GetDirectories(fbd.SelectedPath).Length != 0)
                    {
                        MessageBox.Show("This directory isn't empty.\nPlease, select an empty directory or create a new.",
                            Static.ExecutingAssemblyAttributes.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        String path = fbd.SelectedPath + @"\";
                        txtPath.Text = path + Static.Constants.ApplicationDirectory;
                    }
                }
            }
        }
    }
}
