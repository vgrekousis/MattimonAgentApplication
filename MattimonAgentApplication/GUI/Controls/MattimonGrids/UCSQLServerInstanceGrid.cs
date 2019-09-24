using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentLibrary.MattimonEnum;

namespace MattimonAgentApplication.GUI.Controls.MattimonGrids
{
    public partial class UCSQLServerInstanceGrid : UserControl
    {
        public enum ColumnNames
        {
            colMattimonPK,
            colSqlSrvName,
            colSqlSrvInstance,
            colSqlSrvUser,
            colSqlSrvDatabases,
            colSqlSrvClustered,
            colSqlSrvVersion,
            colSqlSrvDelete,
            colSqlSrvEnabled
        }

        private bool allowDisableMonitorInstance = false;
        [Browsable(true)]
        [DefaultValue(false)]
        public bool AllowDisableInstanceMonitoring
        {
            get => GetColumn(ColumnNames.colSqlSrvEnabled).Visible; 
            set
            {
                allowDisableMonitorInstance = value;
                GetColumn(ColumnNames.colSqlSrvEnabled).Visible = allowDisableMonitorInstance;
            }
        }

        [Browsable(true)]
        public string Title
        {
            get { return lbTitle.Text; }
            set { lbTitle.Text = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public UCSQLServerInstanceGrid() : base()
        {
            InitializeComponent();
            InitializeStyles();
            InitializeDataGridColumns();
            InitializeEventHandlers();
            btnShowHide.Text = "Show";
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeStyles()
        {
            dataGridView.Columns.Clear();
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
        private void InitializeDataGridColumns()
        {

            dataGridView.Columns.Add(ColumnNames.colMattimonPK.ToString(), "mattimon_pk");
            dataGridView.Columns.Add(ColumnNames.colSqlSrvName.ToString(), "Server");
            dataGridView.Columns.Add(ColumnNames.colSqlSrvInstance.ToString(), "Instance");
            dataGridView.Columns.Add(ColumnNames.colSqlSrvClustered.ToString(), "Clustered");
            dataGridView.Columns.Add(ColumnNames.colSqlSrvVersion.ToString(), "Version");
            dataGridView.Columns.Add(ColumnNames.colSqlSrvUser.ToString(), "User");

            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn
            {
                HeaderText = "Status",
                Name = ColumnNames.colSqlSrvEnabled.ToString()
            };

            string[] list = (string[])Enum.GetNames(typeof(MonitorSwitch));
            cmbColumn.Items.AddRange(list);
            cmbColumn.FlatStyle = FlatStyle.Flat;
            cmbColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cmbColumn.ReadOnly = false;
            dataGridView.Columns.Add(cmbColumn);

            DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Name = ColumnNames.colSqlSrvDelete.ToString()
            };

            dataGridView.Columns.Add(btnColumn);

            GetColumn(ColumnNames.colMattimonPK).ReadOnly = true;
            GetColumn(ColumnNames.colMattimonPK).Visible = false;

            GetColumn(ColumnNames.colSqlSrvName).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            GetColumn(ColumnNames.colSqlSrvName).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvInstance).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSqlSrvInstance).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvUser).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSqlSrvUser).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvClustered).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSqlSrvClustered).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvVersion).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSqlSrvVersion).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvDelete).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSqlSrvDelete).ReadOnly = true;

            GetColumn(ColumnNames.colSqlSrvEnabled).AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            GetColumn(ColumnNames.colSqlSrvEnabled).MinimumWidth = GetColumn(ColumnNames.colSqlSrvEnabled).Width;
            GetColumn(ColumnNames.colSqlSrvEnabled).MinimumWidth = GetColumn(ColumnNames.colSqlSrvEnabled).Width;
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeEventHandlers()
        {
            dataGridView.EditingControlShowing += DataGridView_EditingControlShowing;
            dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            dataGridView.CellClick += Cell_Click;
            dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            dataGridView.CellContentClick += DataGridView_CellContentClick;
            dataGridView.DataError += (s, e) => {

                MessageBox.Show(this.FindForm(), e.Exception.Message + "\n\n" +
                    e.Exception.StackTrace + "\n\nValue: " +
                    this.dataGridView[e.ColumnIndex, e.RowIndex].Value, "Data Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); FindForm().Close(); Application.Exit();
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal DataGridViewColumn GetColumn(ColumnNames name)
        {
            return dataGridView.Columns[name.ToString()];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal DataGridViewCell GetCell(int rowIndex, ColumnNames column)
        {
            return dataGridView.Rows[rowIndex].Cells[column.ToString()];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        public void SetDataSource(DataTable dataTable)
        {
            this.dataGridView.DataSource = dataTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="autoGenerateColumns"></param>
        /// <param name="dataMember">Should be the name of the most top DataTable. Keep it null to set the first table in the dataset tables</param>
        public void SetDataSet(DataSet dataSet, bool autoGenerateColumns, string dataMember = null)
        {


           if (autoGenerateColumns) dataGridView.Columns.Clear();
            
            dataGridView.DataBindingComplete += (s, e) =>
            {
               
                foreach (DataGridViewColumn c in dataGridView.Columns)
                {
                    if (c.HeaderText == "servername")
                    {
                        c.HeaderText = "Server";
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        c.DisplayIndex = 0;
                    }
                    else if (c.HeaderText == "instancename")
                    {
                        c.HeaderText = "Instance";
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    }
                    else if (c.HeaderText == "clustered")
                    {
                        c.HeaderText = "Clustered";
                        c.Visible = false;
                    }
                    else if (c.HeaderText == "version")
                    {
                        c.HeaderText = "Version";
                        c.Visible = false;
                    }
                    else if (c.HeaderText == "user")
                    {
                        c.HeaderText = "Login";
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        c.Visible = false;
                    }
                    else if (c.HeaderText == "numberdatabases")
                    {
                        c.HeaderText = "Databases";
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        c.DisplayIndex = 3;
                        c.Visible = false;
                    }
                    else if (c.HeaderText == "lastreported")
                    {
                        c.HeaderText = "Last Reported";
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    }
                    else if (c.Name == ColumnNames.colSqlSrvDelete.ToString())
                    {
                        c.DisplayIndex = dataGridView.ColumnCount - 1;
                        c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        c.Width = 100;

                        ApplyStyles();
                    }
                }
            };

            if (dataSet.Tables != null && dataSet.Tables.Count > 0)
            {

                dataGridView.AutoGenerateColumns = autoGenerateColumns;
                dataGridView.DataSource = dataSet;
                dataGridView.DataMember = dataMember ?? dataSet.Tables[0].TableName;

                string[] list = (string[])Enum.GetNames(typeof(MonitorSwitch));
                dataGridView.Columns.Add(new DataGridViewComboBoxColumn
                {
                    Name = ColumnNames.colSqlSrvEnabled.ToString(),
                    DataSource = list,
                    HeaderText = "Monitored",
                    DataPropertyName = "monitored",
                    ReadOnly = false,
                    FlatStyle = FlatStyle.Standard,
                    Visible = false,
                });


                


                dataGridView.Columns["pk"].Visible = false;
                dataGridView.Columns["pk"].Name = ColumnNames.colMattimonPK.ToString();

                dataGridView.Columns["monitored"].Visible = false;

                dataGridView.Columns["servername"].ReadOnly = true;
                dataGridView.Columns["servername"].Name = ColumnNames.colSqlSrvName.ToString();

                dataGridView.Columns["instancename"].ReadOnly = true;
                dataGridView.Columns["instancename"].Name = ColumnNames.colSqlSrvInstance.ToString();

                dataGridView.Columns["clustered"].ReadOnly = true;
                dataGridView.Columns["clustered"].Name = ColumnNames.colSqlSrvClustered.ToString();

                dataGridView.Columns["version"].ReadOnly = true;
                dataGridView.Columns["version"].Name = ColumnNames.colSqlSrvVersion.ToString();

                dataGridView.Columns["user"].ReadOnly = true;
                dataGridView.Columns["user"].Name = ColumnNames.colSqlSrvUser.ToString();

                dataGridView.Columns["numberdatabases"].ReadOnly = true;

                dataGridView.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = ColumnNames.colSqlSrvDelete.ToString(),
                    HeaderText = "Action",
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        NullValue = "Delete",
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                    }
                    
                });

                ApplyStyles();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ApplyStyles()
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DataGridViewButtonCell btnCell = (DataGridViewButtonCell)row.Cells[ColumnNames.colSqlSrvDelete.ToString()];
                btnCell.FlatStyle = FlatStyle.Flat;
                btnCell.Style.BackColor = Color.FromArgb(200, 100, 100);
                btnCell.Style.ForeColor = Color.FromArgb(255, 255, 255);
                btnCell.Style.SelectionBackColor = btnCell.Style.BackColor;
                btnCell.Style.SelectionForeColor = btnCell.Style.ForeColor;
                btnCell.ToolTipText = "Delete this instance from the monitoring service";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public bool ServerInstanceInTable(string serverName, string instanceName)
        {
            foreach (DataGridViewRow row in this.dataGridView.Rows)
            {
                if (Convert.ToString(row.Cells[ColumnNames.colSqlSrvName.ToString()].Value).Equals(serverName) &&
                    Convert.ToString(row.Cells[ColumnNames.colSqlSrvInstance.ToString()].Value).Equals(instanceName))
                    return true;
            }
            return false;
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
