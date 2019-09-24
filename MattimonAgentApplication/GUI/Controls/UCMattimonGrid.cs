using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls
{
    public partial class UCMattimonGrid : UserControl
    {
        [Browsable(true)]
        public string Title
        {
            get { return lbTitle.Text; }
            set { lbTitle.Text = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public UCMattimonGrid() : base()
        {
            InitializeComponent();
            InitializeStyles();
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeStyles()
        {
            BorderStyle = BorderStyle.FixedSingle;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridView.BackgroundColor = SystemColors.Window;
            dataGridView.GridColor = SystemColors.Window;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.RowTemplate.Height = 40;
            dataGridView.MultiSelect = false;
            dataGridView.RowsDefaultCellStyle.ForeColor = Color.DarkGray;
            dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.WhiteSmoke;//dataGridView.DefaultCellStyle.BackColor;
            dataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 10, 10);//dataGridView.DefaultCellStyle.ForeColor;


            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(100, 200, 100);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 255, 255);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView.RowHeadersVisible = false;

            dataGridView.AllowDrop = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToOrderColumns = false;
            dataGridView.AllowUserToResizeRows = false;

            dataGridView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        public void SetDataSource(DataTable dataTable)
        {
            this.dataGridView.DataSource = dataTable;
        }

        #region Control Animation
        int mDir = 0;
        int mHeight;
        private Timer animtimer = new Timer();
        void Timer1_Tick(object sender, EventArgs e)
        {
            new System.Threading.Thread(() =>
            {
                Invoke((MethodInvoker)delegate ()
                {
                    int height = pGridHolder.Height + mDir;
                    if (height >= 161)
                    {
                        height = mHeight;
                        animtimer.Enabled = false;
                    }
                    else if (height < Math.Abs(mDir))
                    {
                        height = 0;
                        animtimer.Enabled = false;
                        pGridHolder.Visible = false;
                    }
                    pGridHolder.Height = height;
                    btnShowHide.Text = pGridHolder.Visible ? "Hide" : "Show";
                });
            }).Start();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            animtimer.Interval = 5;
            animtimer.Tick += new EventHandler(Timer1_Tick);
            mHeight = this.pGridHolder.Height;
            btnShowHide.Text = pGridHolder.Visible ? "Hide" : "Show";
        }


        private void BtnShowHide_Click(object sender, EventArgs e)
        {
            mDir = pGridHolder.Visible ? -18 : 18;
            pGridHolder.Visible = true;
            animtimer.Enabled = true;

        }

        #endregion
    }
}
