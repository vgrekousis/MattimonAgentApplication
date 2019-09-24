using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentApplication.GUI.BitscoreForms;
using MattimonAgentApplication.GUI.Extensions;

namespace MattimonAgentApplication
{
    public partial class SQLServerInstanceDialog : BitscoreForm
    {
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker BackgroundWorkerEnumerateSQLInstances;
        /// <summary>
        /// 
        /// </summary>
        private DialogResult mDialogResult = DialogResult.None;
        /// <summary>
        /// 
        /// </summary>
        private LoadingPanel mLoadingPanel;
        /// <summary>
        /// 
        /// </summary>
        private readonly int testConnectionMethod = 2;
        /// <summary>
        /// 
        /// </summary>
        public SQLServerInstanceDialog()
        {
            InitializeComponent();
            Icon = MattimonAgentApplication.Properties.Resources.dataicon_64;
            BackgroundWorkerEnumerateSQLInstances = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            BackgroundWorkerEnumerateSQLInstances.DoWork += BackgroundWorkerEnumerateSQLInstances_DoWork;
            BackgroundWorkerEnumerateSQLInstances.RunWorkerCompleted += BackgroundWorkerEnumerateSQLInstances_RunWorkerCompleted;
            btnTestConnection.Click += (testConnectionMethod == 1 ? (EventHandler)BtnTestConnection_Click1 : (EventHandler)BtnTestConnection_Click2);
            this.HandleCreated += SQLServerInstanceDialog_HandleCreated; ;
            lblStatus.Hide();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SQLServerInstanceDialog_HandleCreated(object sender, EventArgs e)
        {
            BackgroundWorkerEnumerateSQLInstances.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        private void InitalizeDialog(IWin32Window owner = null)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            StartPosition = owner == null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public new SQLServerInstanceResult Show(IWin32Window owner)
        {
            InitalizeDialog(owner);
            ShowDialog(owner);
            return mSqlserverInstanceResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new SQLServerInstanceResult Show()
        {
            InitalizeDialog();
            ShowDialog();
            return mSqlserverInstanceResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (mDialogResult != DialogResult.OK && mDialogResult != DialogResult.Cancel)
                mSqlserverInstanceResult = SQLServerInstanceResult.EmptyResult;
        }

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        private void ListControlTree()
        {
            IEnumerable<Control> controls = this.LoopControls();
            String output = String.Empty;
            foreach (Control c in controls)
                output += c.Name + "\n";
            MessageBox.Show(output, "Controls");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void LoopAllControls(Control ctl)
        {
            foreach (Control c in ctl.Controls)
            {
                if (c.HasChildren)
                    LoopAllControls(c);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ApplyCursorToControlTree(Cursor cursor)
        {
            this.LoopControls().ForEach(x => x.Cursor = cursor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        private void LoadScreenEnabled(bool enabled)
        {
            mLoadingPanel = new LoadingPanel
            {
                Size = pnlSqlCred.Size
            };
            pnlSqlCred.Visible = !enabled;

            if (enabled) { pnlCredsAndLoading.Controls.Add(mLoadingPanel); }
            else { pnlCredsAndLoading.Controls.Remove(mLoadingPanel); mLoadingPanel = null; }
        }
        #endregion

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CboServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboServers.SelectedIndex > 0)
            {
                foreach (String s in this.selections)
                {
                    string[] parameters = s.Split(';');

                    if (cboServers.Items[cboServers.SelectedIndex].ToString().Equals(parameters[0] + "\\" + parameters[1]))
                    {
                        this.mServerInstanceSelection = s;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class LoadingPanel : Panel
    {
        private Label label = new Label();
        public LoadingPanel()
        {
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);

            animatedImage = MattimonAgentApplication.Properties.Resources.loading_large;
            Runtext("Searching for SQL Server Instances");
            label.Dock = DockStyle.Bottom;
            label.Font = new Font(label.Font.FontFamily, 12);
            label.AutoSize = false;
            label.Width = this.Width;
            label.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(label);
            Refresh();
        }

        static string Dots(int n)
        {
            return new String('.', n);
        }

        private void Runtext(String initialText)
        {
            Timer t = new Timer
            {
                Interval = 300
            };
            int passes = 0;
            t.Tick += (s, e) =>
            {
                String toShow = initialText + Dots(passes);
                string dots = Dots(passes);
                label.Text = toShow.ToFixedString(initialText.Length + 3);
                passes = passes > 3 ? 0 : passes + 1;
            };
            t.Start();
            this.Disposed += (s, e) => { t.Stop(); t = null; };
        }



        #region Amination
        /// <summary>
        /// 
        /// </summary>
        private readonly Image animatedImage;
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
                ((this.Height) - 50) / 2, 50, 50);

            PaintString("Searching SQL Server instances", e.Graphics);
        }
        private void PaintString(String loadingString, Graphics g)
        {

        }

        public static void DrawStringCenter(Graphics g, string s, Font font, Color color, RectangleF layoutRectangle)
        {
            var brush = new SolidBrush(color);

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            g.DrawString(s, font, brush, layoutRectangle, format);
        }
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Extends the <code>String</code> class with this <code>ToFixedString</code> method.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">The prefered fixed string size</param>
        /// <param name="appendChar">The <code>char</code> to append</param>
        /// <returns></returns>
        public static String ToFixedString(this String value, int length, char appendChar = ' ')
        {
            string current = value;

            int currlen = current.Length;
            int needed = length == currlen ? 0 : (length - currlen);

            return needed == 0 ? value :
                (needed > 0 ? value + new string(appendChar, needed) :
                    new string(new string(value.ToCharArray().Reverse().ToArray()).
                        Substring(needed * -1, value.Length - (needed * -1)).ToCharArray().Reverse().ToArray()));
        }

        public static String FixedSize(this String value, int length, char appendChar = ' ')
        {
            int len = value.Length;
            int need_len = length == len ? 0 : (length - len);
            return new string(appendChar, need_len);
        }

        public static String[] FixedAll(string[] values, int length, char appendChar = ' ')
        {
            string[] fixes = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                fixes[i] = values[i].ToFixedLength(length, appendChar);
            }
            return fixes;    
        }

        public static string ToFixedLength(this string inStr, int length, char appendChar)
        {
            if (inStr.Length == length)
                return inStr;
            if (inStr.Length > length)
                return inStr.Substring(0, length);

            var blanks = Enumerable.Range(1, length - inStr.Length).Select(v => appendChar.ToString()).Aggregate((a, b) => $"{a}{b}");
            return $"{inStr}{blanks}";
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQLServerInstanceResult
    {
        /// <summary>
        /// 
        /// </summary>
        public static SQLServerInstanceResult EmptyResult
        {
            get { return new SQLServerInstanceResult(DialogResult.None, "", "", false, "", null); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static SQLServerInstanceResult CancelResult
        {
            get { return new SQLServerInstanceResult(DialogResult.Cancel, "", "", false, "", null); }
        }
        private readonly string mServerName;
        private readonly string mInstanceName;
        private readonly string mVersion;
        private readonly bool mClustered;
        private SqlConnectionStringBuilder mSqlConnectionStringBuilder;
        private readonly DialogResult mDialogResult;
        /// <summary>
        /// 
        /// </summary>
        public string ServerName
        {
            get { return mServerName; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string InstanceName
        {
            get { return mInstanceName; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Version
        {
            get { return mVersion; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool Clustered
        {
            get { return mClustered; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString
        {
            get { if (mSqlConnectionStringBuilder != null) return mSqlConnectionStringBuilder.ConnectionString; return null; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Username => mSqlConnectionStringBuilder?.UserID;
        /// <summary>
        /// 
        /// </summary>
        public string Password => mSqlConnectionStringBuilder?.Password;
        /// <summary>
        /// 
        /// </summary>
        public SqlConnectionStringBuilder SqlConnectionStringBuilder
        {
            get { return mSqlConnectionStringBuilder; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DialogResult DialogResult
        {
            get { return mDialogResult; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        /// <param name="clustered"></param>
        /// <param name="version"></param>
        /// <param name="connectionStringBuilder"></param>
        internal SQLServerInstanceResult
            (DialogResult result, string serverName, string instanceName, bool clustered,
                string version, SqlConnectionStringBuilder sqlConnectionStringBuilder)
        {
            mDialogResult = result;
            mServerName = serverName;
            mInstanceName = instanceName;
            mVersion = version;
            mClustered = clustered;
            mSqlConnectionStringBuilder = sqlConnectionStringBuilder;
        }

    }
}
