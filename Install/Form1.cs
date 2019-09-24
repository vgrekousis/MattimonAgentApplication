using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Install.Static;
using MattimonAgentLibrary.Rest;
using MattimonAgentLibrary.Models;
using MattimonAgentLibrary.Tools;
using System.Net.Http;

namespace Install
{
    public partial class Form1 : FormCommon
    {
        /// <summary>
        /// 
        /// </summary>
        DeviceOptions DeviceOptions = new DeviceOptions();
        /// <summary>
        /// 
        /// </summary>
        UserAuthentication UserAuthentication;
        /// <summary>
        /// 
        /// </summary>
        LoadingPanel loadingPanel = new LoadingPanel();
        /// <summary>
        /// 
        /// </summary>
        IntervalsPanel intervalsPanel = new IntervalsPanel();
        /// <summary>
        /// 
        /// </summary>
        private static Form globalForm;
        /// <summary>
        /// Next Form
        /// </summary>
        private Form2 Form2;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker authenticator = new BackgroundWorker();
        /// <summary>
        /// 
        /// </summary>
        public double SelectedInterval
        {
            get
            {
                return
                  this.intervalsPanel.SelectedInterval;

            }
        }
        public Form1()
        {
            InitializeComponent();
            ModeDev(false);

            globalForm = this;
            btnPrevious.Enabled = false;
            TextboxTextChanged(null, null);
            btnNext.Click += btnNext_Click;
            btnCancel.Click += btnCancel_Click;
            authenticator.DoWork += Authenticator_DoWork;
            intervalsPanel.ValueChanged += IntervalsPanel_ValueChanged;

            // Provoke  IntervalsPanel_ValueChanged just in case the user left it as it is
            // Other wise not value will be set in DeviceOptions.ReportingInterval.
            IntervalsPanel_ValueChanged(intervalsPanel, EventArgs.Empty); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntervalsPanel_ValueChanged(object sender, EventArgs e)
        {
            DeviceOptions.ReportingInterval = intervalsPanel.SelectedInterval;
            DeviceOptions.MonitorSql = intervalsPanel.SQLMonitoringSelected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modeDev"></param>
        private void ModeDev(Boolean modeDev)
        {
            if (modeDev)
            {
                textBox1.Text = "vasgr@vaswebsolutions.com";
                textBox2.Text = "MA_5ac2bf14e131f";
                //txtAuthEmail.Text = "gkorakakis@bitscoretechnologies.com";
                //txtAuthPwrd.Text = "KTjnVegDGPykSbLNt0VoYYGIc1qj5iXy6Nu5xxq82lA=";
                textBox1.Enabled = false;
                textBox2.Enabled = textBox1.Enabled;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Authenticator_DoWork(object sender, DoWorkEventArgs e)
        {
            MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(btnNext, "Enabled", false);

            try
            {
                LoadingScreenThreadSafe(true);
                AuthenticationRequest authenticationRequest = new AuthenticationRequest();
                UserAuthentication item = authenticationRequest.GetUserAuthentication(new UserAuthentication { User_email = textBox1.Text.Trim(), User_Agent_ID = textBox2.Text.Trim() });
                

                Boolean proceed = true;
                if (item.MySqlExceptionErrno > 0)
                {
                    MessageBox.Show("Internal Server Error\n\n" + item.MySqlExceptionError,
                        "Internet Server Error (MySql)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    proceed = false;
                }


                if (item.Exception != null)
                {
                    MessageBox.Show("Internal Server Error\n\n" + item.Exception.Message + "\n\n" + item.Exception.StackTrace,
                        "Internet Server Error (500)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    proceed = false;
                }


                if (item.HttpRequestException != null)
                {
                    MessageBox.Show("Http Request Exception\n\n" + item.HttpRequestException.Message + "\n\n" + item.HttpRequestException.StackTrace,
                        "Http Request Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    proceed = false;
                }


                if (proceed)
                {
                    if (!item.Auth_Ok)
                    {
                        LoadingScreenThreadSafe(false);
                        MessageBox.Show("The credentials you provided are invalid. Please try again.\n\nIf you beleive this is an error, please contact us.",
                            "Authentication Failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        LoadingScreenThreadSafe(false);
                        MessageBox.Show("Your credentials are ok. You may now proceed with the installation!\n\n" +
                            "Company: " + item.Company_Name, "Authentication Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SelectIntervalsScreenThreadSafe(true);

                        // Enable next button
                        GUITools.SetControlPropertyThreadSafe(btnNext, "Enabled", true);

                        //this.NextWindow(item);
                        this.UserAuthentication = item;

                        SelectIntervalsScreenThreadSafe(true);
                    }
                }
                else
                {
                    MessageBox.Show("The Installer will now exit.", "Message",
                        MessageBoxButtons.OK, MessageBoxIcon.None);
                    Application.Exit();
                }
            }
            catch (HttpRequestException httprex)
            {
                LoadingScreenThreadSafe(false);
                MessageBox.Show(httprex.ToString(), "Http Request Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(btnNext, "Enabled", true);
            }

            catch (Exception ex)
            {
                LoadingScreenThreadSafe(false);
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(btnNext, "Enabled", true);
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (UserAuthentication == null)
                this.authenticator.RunWorkerAsync();
            else
            {
                this.NextWindow();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ua"></param>
        private void NextWindow()
        {
            Form2 = new Form2(this, UserAuthentication, DeviceOptions);
            globalForm.Invoke((MethodInvoker)delegate () { Form2.Show(); Form2.Location = Location; });
            Form2.FormClosed += (s, e) => this.Show();
            Hide();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_FormClosed(object sender, EventArgs e)
        {
            Show();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
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
                    if (f == this) // is Form1
                    {
                        FormClosing -= Form1_FormClosing;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextboxTextChanged(object sender, EventArgs e)
        {
            int i1 = textBox1.TextLength;
            int i2 = textBox2.TextLength;
            btnNext.Enabled = i1 != 0 && i2 != 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loading"></param>
        private void LoadingScreen(Boolean loading)
        {
            
            if (loading)
            {
                DockStyle dock = this.flMainPanel.Dock;
                this.loadingPanel.Dock = dock;
                this.loadingPanel.Bounds = this.flMainPanel.Bounds;
                this.loadingPanel.ClientSize = this.flMainPanel.ClientSize;
                this.loadingPanel.Font = this.flMainPanel.Font;
                this.flMainPanel.Hide();
                this.loadingPanel.Show();
                this.Controls.Add(this.loadingPanel);

            }
            else
            {
                this.loadingPanel.Hide();
                this.Controls.Remove(this.loadingPanel);
                this.flMainPanel.Show();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acivate"></param>
        private void SelectIntervalsScreenThreadSafe(Boolean acivate)
        {
            DockStyle dock = this.flMainPanel.Dock;
            Rectangle bounds = this.flMainPanel.Bounds;
            Size clientSize = this.flMainPanel.ClientSize;
            Font font = this.flMainPanel.Font;

            if (acivate)
            {
                GUITools.SetControlPropertyThreadSafe(this.intervalsPanel, "Dock", dock);
                GUITools.SetControlPropertyThreadSafe(this.intervalsPanel, "Bounds", bounds);
                GUITools.SetControlPropertyThreadSafe(this.intervalsPanel, "ClientSize", clientSize);
                GUITools.SetControlPropertyThreadSafe(this.intervalsPanel, "Font", font);

                globalForm.Invoke((MethodInvoker)delegate ()
                {
                    this.flMainPanel.Hide();
                    this.intervalsPanel.Show();
                    this.mainContainer.Controls.Add(this.intervalsPanel);
                });
            }
            else
            {
                globalForm.Invoke((MethodInvoker)delegate ()
                {
                    this.intervalsPanel.Hide();
                    this.mainContainer.Controls.Remove(this.intervalsPanel);
                    this.flMainPanel.Show();
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loading"></param>
        private void LoadingScreenThreadSafe(Boolean loading)
        {
            DockStyle dock = this.flMainPanel.Dock;
            Rectangle bounds = this.flMainPanel.Bounds;
            Size clientSize = this.flMainPanel.ClientSize;
            Font font = this.flMainPanel.Font;

            if (loading)
            {
                GUITools.SetControlPropertyThreadSafe(this.loadingPanel, "Dock", dock);
                GUITools.SetControlPropertyThreadSafe(this.loadingPanel, "Bounds", bounds);
                GUITools.SetControlPropertyThreadSafe(this.loadingPanel, "ClientSize", clientSize);
                GUITools.SetControlPropertyThreadSafe(this.loadingPanel, "Font", font);

                globalForm.Invoke((MethodInvoker)delegate ()
                {
                    this.flMainPanel.Hide();
                    this.loadingPanel.Show();
                    this.mainContainer.Controls.Add(this.loadingPanel);
                });
            }
            else
            {
                globalForm.Invoke((MethodInvoker)delegate ()
                {
                    this.loadingPanel.Hide();
                    this.mainContainer.Controls.Remove(this.loadingPanel);
                    this.flMainPanel.Show();
                });
            }
         }

        private void Form0_Load(object sender, EventArgs e)
        {
        }
    }















    /// <summary>
    /// 
    /// </summary>
    public class LoadingPanel : Panel
    {
        private Label lblMessage = new Label();
        public new String Text
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }
        public LoadingPanel()
        {
            Text = "Loading...";
            animatedImage = Install.Properties.Resources.LoadingGif;
            lblMessage.Text = Text;
            lblMessage.Dock = DockStyle.None;
            lblMessage.AutoSize = false;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Padding = new Padding(0, 15, 0, 15);
            lblMessage.Font = this.Font;
            this.Controls.Add(lblMessage);
            GUITools.CenterControlInParent(lblMessage);
            Refresh();
        }
        #region Amination
        /// <summary>
        /// 
        /// </summary>
        private Image animatedImage;
        /// <summary>
        /// 
        /// </summary>
        bool currentlyAnimating = false;

        /// <summary>
        /// 
        /// </summary>
        private void AnimateImage()
        {
            if (!currentlyAnimating)
            {
                //Begin the animation only once.
                ImageAnimator.Animate(animatedImage, new EventHandler(this.OnFrameChanged));
                currentlyAnimating = true;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnFrameChanged(object o, EventArgs e)
        {
            //Force a call to the Paint event handler.
            this.Invalidate();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Begin the animation.
            AnimateImage();
            //Get the next frame ready for rendering.
            ImageAnimator.UpdateFrames();
            //Draw the next frame in the animation.
            e.Graphics.DrawImage(this.animatedImage,
                (this.Width - 50) / 2,
                ((this.Height + lblMessage.Height) - 50) / 2, 50, 50);
        }
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public class IntervalsPanel : FlowLayoutPanel
    {
        public event EventHandler ValueChanged;
        private GroupBox groupBox1 = new GroupBox();
        private GroupBox groupBox2 = new GroupBox();
        private FlowLayoutPanel container = new FlowLayoutPanel();
        private FlowLayoutPanel container2 = new FlowLayoutPanel();
        private RadioButton rdo15mins = new RadioButton();
        private RadioButton rdo10mins = new RadioButton();
        private RadioButton rdo5mins = new RadioButton();
        private RadioButton rdo1min = new RadioButton();
        private CheckBox chkMonitSql = new CheckBox();

        private double interval;
        private bool monitSql;
        public double SelectedInterval
        {
            get
            {
                return this.interval;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool SQLMonitoringSelected
        {
            get
            {
                return this.monitSql;
            }
        }

        /// <summary>
        /// Assigns the font to all its components
        /// </summary>
        public Font ControlFont
        {
            get { return Font; }
            set
            {
                this.Font = Font;
                rdo10mins.Font = value;
                rdo15mins.Font = value;
                rdo5mins.Font = value;
                rdo1min.Font = value;
                container.Font = value;
                groupBox1.Font = value;
                groupBox2.Font = value;
                container2.Font = value;
                chkMonitSql.Font = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IntervalsPanel()
        {
            this.rdo15mins.AutoSize = true;
            this.rdo15mins.Name = "rdo15mins";
            this.rdo15mins.TabIndex = 3;
            this.rdo15mins.Tag = "15"; // Minutes
            this.rdo15mins.Text = "By an interval of 15 minutes";
            this.rdo15mins.UseVisualStyleBackColor = true;
            this.rdo15mins.CheckedChanged += new System.EventHandler(this.CheckedChanged);

            this.rdo10mins.AutoSize = true;
            this.rdo10mins.Name = "rdo10mins";
            this.rdo10mins.TabIndex = 2;
            this.rdo10mins.Tag = "10"; // Minutes
            this.rdo10mins.Text = "By an interval of 10 minutes";
            this.rdo10mins.UseVisualStyleBackColor = true;
            this.rdo10mins.CheckedChanged += new System.EventHandler(this.CheckedChanged);

            this.rdo5mins.AutoSize = true;
            this.rdo5mins.Name = "rdo5mins";
            this.rdo5mins.TabIndex = 1;
            this.rdo5mins.TabStop = true;
            this.rdo5mins.Tag = "5"; // Minutes
            this.rdo5mins.Text = "By an interval of 5 minutes";
            this.rdo5mins.UseVisualStyleBackColor = true;
            this.rdo5mins.CheckedChanged += new System.EventHandler(this.CheckedChanged);

            this.rdo1min.AutoSize = true;
            this.rdo1min.Checked = true;
            this.rdo1min.Name = "rdo1min";
            this.rdo1min.TabIndex = 0;
            this.rdo1min.TabStop = true;
            this.rdo1min.Tag = "1"; // Minutes
            this.rdo1min.Text = "By an interval of 1 minute";
            this.rdo1min.UseVisualStyleBackColor = true;
            this.rdo1min.CheckedChanged += new System.EventHandler(this.CheckedChanged);


            this.container.Size = new System.Drawing.Size(672, 129);
            this.container.FlowDirection = FlowDirection.TopDown;
            this.container.Dock = DockStyle.Fill;

            this.container.Controls.Add(rdo1min);
            this.container.Controls.Add(rdo5mins);
            this.container.Controls.Add(rdo10mins);
            this.container.Controls.Add(rdo15mins);

            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "How often you\'d like your device to report its status to us?";
            this.groupBox1.Controls.Add(container);
            this.groupBox1.Size = new System.Drawing.Size(672, 129);

            this.chkMonitSql.Checked = false;
            this.chkMonitSql.AutoSize = true;
            this.chkMonitSql.Name = "chkMonitSql";
            this.chkMonitSql.Text = "SQL Monitoring";
            this.chkMonitSql.UseVisualStyleBackColor = true;
            this.chkMonitSql.CheckedChanged += new EventHandler(this.CheckedChanged);

            this.container2.Size = new System.Drawing.Size(672, 129);
            this.container2.FlowDirection = FlowDirection.TopDown;
            this.container2.Dock = DockStyle.Fill;
            this.container2.Controls.Add(chkMonitSql);

            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Which services you\'d like to enable?";
            this.groupBox2.Dock = DockStyle.Fill;
            this.groupBox2.Controls.Add(container2);
            this.groupBox2.Size = new System.Drawing.Size(672, 129);

            this.Dock = DockStyle.Fill;
            this.Controls.Add(groupBox1);
            this.Controls.Add(groupBox2);
            this.FlowDirection = FlowDirection.TopDown;
            this.Refresh();

            this.CheckedChanged(rdo1min, EventArgs.Empty); // Default 1 minute
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                this.interval =
                    TimeSpanUtil.ConvertMinutesToMilliseconds(
                        Convert.ToDouble(((RadioButton)sender).Tag));
            }
            else
            {
                if (((CheckBox)sender) == chkMonitSql)
                    this.monitSql = ((CheckBox)sender).Checked;
            }

            if (ValueChanged != null)
            {
                Delegate[] subscribers = ValueChanged.GetInvocationList();
                foreach (EventHandler target in subscribers)
                {
                    target(this, new EventArgs());
                }
            }
        }
    }
}
