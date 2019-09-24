using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MattimonAgentLibrary;
using MattimonAgentLibrary.Models;
using MattimonSQLite;

namespace Install
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Form3 : FormCommon
    {
        /// <summary>
        /// Device Options
        /// </summary>
        private DeviceOptions DeviceOptions;
        /// <summary>
        /// Authenticated User
        /// </summary>
        private UserAuthentication UserAuthentication;
        /// <summary>
        /// 
        /// </summary>
        new public Size ClientSize
        {
            get { return base.ClientSize; }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        new public String Text
        {
            get { return base.Text; }
            set { }
        }
        /// <summary>
        /// 
        /// </summary>
        private String InstallPath;
        /// <summary>
        /// Previous Form
        /// </summary>
        private Form2 Form2;
        /// <summary>
        /// Next Form
        /// </summary>
        private Form4 Form4;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="form2"></param>
        /// <param name="installPath"></param>
        public Form3(Form2 form2, String installPath, UserAuthentication userAuthentication, DeviceOptions options)
        {
            InitializeComponent();
            
            Form2 = form2;
            InstallPath = installPath;
            UserAuthentication = userAuthentication;
            DeviceOptions = options;

            btnCancel.Click += BtnCancel_Click;
            btnPrevious.Click += btnPrevious_Click;
            btnPrevious.Enabled = true;
            btnNext.Click += btnNext_Click;

            SetText(this, base.Text + " (" + userAuthentication.Company_Name + ")");

            SetText(label1,
                "The installer is ready to install " + Static.ExecutingAssemblyAttributes.AssemblyProduct + " on your computer.\n\n" +
                "Click \"Next\" to start the installation.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Hide();
            Form2.Show();
            Form2.Location = Location;
            Form2.Focus();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {

            Form4 = new Form4(this, InstallPath, UserAuthentication, DeviceOptions);
            Form4.FormClosed -= (s, _) => { Show(); };
            Form4.FormClosed += (s, _) => { Show(); };
            Form4.Show();
            Form4.Location = Location;
            Hide();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Form3_FormClosing(object sender, FormClosingEventArgs e)
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
                    if (f is Form2)
                    {
                        Form2 f2 = (Form2)f;
                        f2.FormClosing -= f2.Form2_FormClosing;
                    }
                    if (f == this) // is Form3
                    {
                        FormClosing -= Form3_FormClosing;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form4_FormClosed(object sender, EventArgs e)
        {
            Show();
            Location = Form4.Location;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="text"></param>
        private void SetText(Control c, String text)
        {
            c.Text = text;
        }
    }
}
