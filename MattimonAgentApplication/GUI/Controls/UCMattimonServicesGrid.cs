using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentApplication.GUI.Events;
using System.ServiceProcess;
using System.Management;

namespace MattimonAgentApplication.GUI.Controls
{
    public partial class UCMattimonServicesGrid : UserControl
    {

        public enum ColumnNames
        {
            colSvcName,
            colSvcDisplay,
            colSvcStatus,
            colSvcBtnStatus,
            colSvcCmbStart
        }

        [Browsable(true)]
        public String Caption
        {
            get { return text == String.Empty ? Name : label1.Text; }
            set { text = value; label1.Text = text; }
        }
        private string text;
        /// <summary>
        /// 
        /// </summary>
        public event ServiceStartChangedEventHandler ServiceStartChanged;
        /// <summary>
        /// 
        /// </summary>
        public event ServiceStateChangedEventHandler ServiceStateChanged;
        /// <summary>
        /// 
        /// </summary>
        /// 
        public UCMattimonServicesGrid()
        {
            InitializeComponent();
            InitializeStyles();
            InitializeDataGridColumns();
            InitializeEventHandlers();
            btnShowHide.Text = "Show";
            LoadData();
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeStyles()
        {
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
            dataGridView.Columns.Add(ColumnNames.colSvcName.ToString(), "Service Name");
            dataGridView.Columns[ColumnNames.colSvcName.ToString()].Visible = false;
            dataGridView.Columns.Add(ColumnNames.colSvcDisplay.ToString(), "Display Name");
            dataGridView.Columns.Add(ColumnNames.colSvcStatus.ToString(), "Status");

            DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Name = ColumnNames.colSvcBtnStatus.ToString()
            };
            dataGridView.Columns.Add(btnColumn);

            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            String[] list = Enum.GetValues(typeof(MattimonAgentLibrary.Tools.MyServiceController.ServiceStart)).Cast<MattimonAgentLibrary.Tools.MyServiceController.ServiceStart>().Select(x => x.ToString()).ToArray();
            cmbColumn.HeaderText = "Start Type";
            cmbColumn.Name = ColumnNames.colSvcCmbStart.ToString();
            cmbColumn.Items.AddRange(list);
            cmbColumn.FlatStyle = FlatStyle.Flat;
            cmbColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cmbColumn.ReadOnly = false;
            dataGridView.Columns.Add(cmbColumn);


            GetColumn(ColumnNames.colSvcName).Visible = false;
            GetColumn(ColumnNames.colSvcName).ReadOnly = true;


            GetColumn(ColumnNames.colSvcDisplay).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            GetColumn(ColumnNames.colSvcDisplay).MinimumWidth = 300;
            GetColumn(ColumnNames.colSvcDisplay).ReadOnly = true;

            GetColumn(ColumnNames.colSvcStatus).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSvcStatus).ReadOnly = true;

            GetColumn(ColumnNames.colSvcBtnStatus).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSvcBtnStatus).ReadOnly = true;

            GetColumn(ColumnNames.colSvcCmbStart).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GetColumn(ColumnNames.colSvcCmbStart).ReadOnly = true;

            ((DataGridViewComboBoxColumn)GetColumn(ColumnNames.colSvcCmbStart)).FlatStyle = FlatStyle.Standard;
            ((DataGridViewComboBoxColumn)GetColumn(ColumnNames.colSvcCmbStart)).DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            ((DataGridViewComboBoxColumn)GetColumn(ColumnNames.colSvcCmbStart)).ReadOnly = false;

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
        public void LoadData()
        {
            dataGridView.Rows.Clear();
            List<ServiceMetaData> services = GetMattimonServices();
            int i = 0;
           
            foreach (ServiceMetaData svc in services)
            {
                dataGridView.Rows.Add(
                    svc.Name,
                    svc.DisplayName,
                    MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svc.Name).ToString(),
                    MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svc.Name) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running ? "Stop" : "Start",
                    MattimonAgentLibrary.Tools.MyServiceController.GetServiceStart(svc.Name).ToString());


                DataGridViewButtonCell cellbtn = (DataGridViewButtonCell)GetCell(i, ColumnNames.colSvcBtnStatus);
                cellbtn.Style.ForeColor = Color.White;
                cellbtn.Style.BackColor = svc.Status == ServiceControllerStatus.Running ? Color.FromArgb(200, 100, 100) : Color.DarkSeaGreen;
                cellbtn.Style.SelectionForeColor = cellbtn.Style.ForeColor;
                cellbtn.Style.SelectionBackColor = cellbtn.Style.BackColor;
                cellbtn.FlatStyle = FlatStyle.Flat;


                DataGridViewComboBoxCell cellcmb = (DataGridViewComboBoxCell)GetCell(i, ColumnNames.colSvcCmbStart);
                cellcmb.Style.BackColor = Color.DarkSeaGreen;
                cellcmb.Style.ForeColor = Color.White;
                cellcmb.Tag = new object[] { svc.Name, svc.DisplayName, MattimonAgentLibrary.Tools.MyServiceController.GetServiceStart(svc.Name) };
                cellcmb.ToolTipText = "Click to select a Start Type";


                i++;
            }

            if (this.dataGridView.Rows.Count > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = 0;
                dataGridView.Rows[0].Selected = true;
            }

            //dataGridView.Size = new Size(dataGridView.Size.Width, (dataGridView.Rows.Count * dataGridView.RowTemplate.Height + dataGridView.ColumnHeadersHeight + dataGridView.Rows.Count));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcParentDirectoryPath"></param>
        /// <returns></returns>
        private List<ServiceMetaData> GetMattimonServices()
        {
            String svcParentDirectoryPath = MattimonAgentLibrary.Tools.RegistryTools.GetInstallLocationByDisplayName(MattimonAgentLibrary.Static.MattimonRegistrySubkeyNames.DisplayName); //System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location.ToString()).FullName;

            List<ServiceMetaData> foundControllers = null;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();
                foundControllers = new List<ServiceMetaData>();

                foreach (ManagementObject obj in collection)
                {
                    string name = obj["Name"] as string;
                    string pathName = obj["PathName"] as string;

                    if (!System.IO.File.Exists(pathName)) continue;

                    string parent = System.IO.Directory.GetParent(pathName).FullName;
                    if (parent != svcParentDirectoryPath) continue;

                    try
                    {
                        ServiceController svcCtrl = ServiceController.GetServices(System.Environment.MachineName).
                            Where(svc => svc.ServiceName == name).FirstOrDefault();

                        if (svcCtrl != null)
                        {
                            ServiceMetaData data = new ServiceMetaData(svcCtrl, pathName);
                            foundControllers.Add(data);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.StackTrace,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return foundControllers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="svcName"></param>
        public void RefreshDataGridViewEntry(string svcName)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[ColumnNames.colSvcName.ToString()].Value.ToString().Equals(svcName))
                    {
                        row.Cells[ColumnNames.colSvcStatus.ToString()].Value = MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName).ToString();
                        row.Cells[ColumnNames.colSvcBtnStatus.ToString()].Value = MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running ? "Stop" : "Start";
                        ((DataGridViewButtonCell)(row.Cells[ColumnNames.colSvcBtnStatus.ToString()])).Style.BackColor = MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName) == MattimonAgentLibrary.Tools.MyServiceController.ServiceState.Running ? Color.FromArgb(200, 100, 100) : Color.FromArgb(100, 200, 100);
                        ((DataGridViewButtonCell)(row.Cells[ColumnNames.colSvcBtnStatus.ToString()])).Style.SelectionBackColor = ((DataGridViewButtonCell)(row.Cells[ColumnNames.colSvcBtnStatus.ToString()])).Style.BackColor;
                        ((DataGridViewComboBoxCell)(row.Cells[ColumnNames.colSvcCmbStart.ToString()])).Value = MattimonAgentLibrary.Tools.MyServiceController.GetServiceStart(svcName).ToString();
                        return;
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReloadDataGridView()
        {
            LoadData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal DataGridView GetDataGridView()
        {
            return dataGridView;
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
            pGridHolder.Visible = false;
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

        internal class ServiceMetaData
        {
            private readonly string svcname;
            private readonly string svcdisplay;
            private readonly string parentdirname;
            private readonly string fulldirname;
            private readonly ServiceStartMode svcmode;
            private readonly ServiceControllerStatus svcstatus;
            /// <summary>
            /// 
            /// </summary>
            public String Name
            {
                get { return svcname; }
            }
            /// <summary>
            /// 
            /// </summary>
            public String DisplayName
            {
                get { return svcdisplay; }
            }
            /// <summary>
            /// 
            /// </summary>
            public ServiceStartMode StartType
            {
                get { return svcmode; }
            }
            /// <summary>
            /// 
            /// </summary>
            public ServiceControllerStatus Status
            {
                get { return svcstatus; }
            }
            /// <summary>
            /// 
            /// </summary>
            public String ParentDirectoryFullName
            {
                get { return parentdirname; }
            }
            /// <summary>
            /// 
            /// </summary>
            public String DirectoryFullName
            {
                get { return fulldirname; }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="controller"></param>
            /// <param name="path"></param>
            public ServiceMetaData(ServiceController controller, String path)
            {
                svcname = controller.ServiceName;
                svcdisplay = controller.DisplayName;
                svcstatus = controller.Status;
                svcmode = controller.StartType;

                if (System.IO.File.Exists(path))
                {
                    fulldirname = new System.IO.DirectoryInfo(path).FullName;
                    parentdirname = System.IO.Directory.GetParent(path).FullName;
                }
                else if (System.IO.Directory.Exists(path))
                {
                    fulldirname = new System.IO.DirectoryInfo(path).FullName;
                    parentdirname = System.IO.Directory.GetParent(path).FullName;
                }
                else
                {
                    fulldirname = null;
                    parentdirname = null;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public String GetParentDirectoryName()
            {
                return parentdirname;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public String GetFullDirectoryName()
            {
                return fulldirname;
            }
        }
    }
}
