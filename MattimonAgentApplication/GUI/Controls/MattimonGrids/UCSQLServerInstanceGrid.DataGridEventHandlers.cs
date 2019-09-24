using MattimonAgentApplication.GUI.Events;
using MattimonAgentLibrary.MattimonEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls.MattimonGrids
{
    partial class UCSQLServerInstanceGrid
    {
        public event DeleteInstanceClickEventHandler DeleteInstanceClick;

        private DataGridViewCellEventArgs dataGridViewCellEventArgs;

        #region Data Grid Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView[e.ColumnIndex, e.RowIndex].OwningColumn.Name != ColumnNames.colSqlSrvEnabled.ToString()) return;

            dataGridView.CellEndEdit -= DataGridView_CellEndEdit;
            dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            DataGridViewRow editedrow = dataGridView.Rows[e.RowIndex];
            String sqlName = Convert.ToString(editedrow.Cells[ColumnNames.colSqlSrvName.ToString()].Value);
            String sqlInst = Convert.ToString(editedrow.Cells[ColumnNames.colSqlSrvInstance.ToString()].Value);
            String sqlUser = Convert.ToString(editedrow.Cells[ColumnNames.colSqlSrvUser.ToString()].Value);
            String sqlMonitStatusBefore = Convert.ToString(editedrow.Tag);
            String sqlMonitStatusSelected = Convert.ToString(editedrow.Cells[ColumnNames.colSqlSrvEnabled.ToString()].Value);
            Enum.TryParse(sqlMonitStatusSelected, out MonitorSwitch monitorSwitch);
            MessageBox.Show(Convert.ToInt32(monitorSwitch).ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmb_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            dataGridView.Rows[dataGridView.CurrentCell.RowIndex].Tag = dataGridView.CurrentCell.Value; // Save the value before
            dataGridView.CurrentCell.Value = cmb.SelectedItem; // then, replace the value
            dataGridView.EndEdit(DataGridViewDataErrorContexts.Commit);
        }
        private void Cmb_DropDown(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            cmb.BackColor = Color.WhiteSmoke;
            cmb.ForeColor = Color.FromArgb(10, 10, 10);
            cmb.FlatStyle = FlatStyle.Flat;
            //cmb.Font = new Font(Font.FontFamily.Name, 12);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridView.CurrentCell.OwningColumn == dataGridView.Columns[ColumnNames.colSqlSrvEnabled.ToString()])
            {
                if (e.Control is ComboBox cb)
                {

                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cb.DropDownClosed -= Cmb_DropDownClosed;
                    cb.DropDownClosed += Cmb_DropDownClosed;
                    cb.DropDown -= Cmb_DropDown;
                    cb.DropDown += Cmb_DropDown;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (this.dataGridView[e.ColumnIndex, e.RowIndex].OwningColumn.Name.Equals(ColumnNames.colSqlSrvDelete.ToString()) && e.RowIndex >= 0)
            {
                int srvPK = Convert.ToInt32(dataGridView[ColumnNames.colMattimonPK.ToString(), e.RowIndex].Value);
                string srvname = Convert.ToString(dataGridView[ColumnNames.colSqlSrvName.ToString(), e.RowIndex].Value);
                string insname = Convert.ToString(dataGridView[ColumnNames.colSqlSrvInstance.ToString(), e.RowIndex].Value);
                if (DeleteInstanceClick != null)
                {
                    Delegate[] delegates = DeleteInstanceClick.GetInvocationList();
                    foreach (DeleteInstanceClickEventHandler target in delegates)
                        target(((DataGridViewButtonCell)dataGridView[ColumnNames.colSqlSrvDelete.ToString(), e.RowIndex]),
                            new DeleteInstanceClickEventArgs(srvPK, srvname, insname));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            new System.Threading.Thread(() =>
            {
                //MessageBox.Show("Changed!");
            }).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_Click(System.Object sender, DataGridViewCellEventArgs e)
        {

            dataGridViewCellEventArgs = e;
            if (dataGridView.Columns[e.ColumnIndex].Name.Equals(ColumnNames.colSqlSrvEnabled.ToString()) && e.RowIndex >= 0)
            {

                if (dataGridView.Rows[e.RowIndex].Cells[ColumnNames.colSqlSrvEnabled.ToString()].Selected == true && e.ColumnIndex >= 0)
                {
                    dataGridView.BeginEdit(true);
                    ((DataGridViewComboBoxEditingControl)dataGridView.EditingControl).DroppedDown = true;
                }
            }
            else
            {
            }
        }
        #endregion
    }
}
