
using MattimonAgentApplication.GUI.Events;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls
{
    partial class UCMattimonServicesGrid
    {
        private DataGridViewCellEventArgs dataGridViewCellEventArgs;
        #region Data Grid Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView[e.ColumnIndex, e.RowIndex].OwningColumn.Name != ColumnNames.colSvcCmbStart.ToString()) return;

            dataGridView.CellEndEdit -= DataGridView_CellEndEdit;
            dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            DataGridViewRow editedrow = dataGridView.Rows[e.RowIndex];
            String svcName = Convert.ToString(editedrow.Cells[ColumnNames.colSvcName.ToString()].Value);
            String svcDisp = Convert.ToString(editedrow.Cells[ColumnNames.colSvcDisplay.ToString()].Value);
            MattimonAgentLibrary.Tools.MyServiceController.ServiceStart before = (MattimonAgentLibrary.Tools.MyServiceController.ServiceStart)Enum.Parse(typeof(MattimonAgentLibrary.Tools.MyServiceController.ServiceStart), Convert.ToString(editedrow.Tag));
            MattimonAgentLibrary.Tools.MyServiceController.ServiceStart selected = (MattimonAgentLibrary.Tools.MyServiceController.ServiceStart)Enum.Parse(typeof(MattimonAgentLibrary.Tools.MyServiceController.ServiceStart), Convert.ToString(editedrow.Cells[ColumnNames.colSvcCmbStart.ToString()].Value));
            if (before != selected && ServiceStartChanged != null)
            {
                Delegate[] delegates = ServiceStartChanged.GetInvocationList();
                foreach (ServiceStartChangedEventHandler handler in delegates)
                    handler(this, new ServiceStartChangedEventArgs(svcName, svcDisp, before, selected));
            }
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
            if (dataGridView.CurrentCell.OwningColumn == dataGridView.Columns[ColumnNames.colSvcCmbStart.ToString()])
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
            if (this.dataGridView[e.ColumnIndex, e.RowIndex].OwningColumn.Name.Equals(ColumnNames.colSvcBtnStatus.ToString()) && e.RowIndex >= 0)
            {
                DataGridViewRow currentrow = dataGridView.CurrentRow;
                String svcName = Convert.ToString(currentrow.Cells[ColumnNames.colSvcName.ToString()].Value);
                String displayedSvcState = Convert.ToString(currentrow.Cells[ColumnNames.colSvcDisplay.ToString()].Value);
                MattimonAgentLibrary.Tools.MyServiceController.ServiceState querySvcState = MattimonAgentLibrary.Tools.MyServiceController.GetServiceStatus(svcName);

                // Update the grid if the sercice's state have actually changed
                // And do not trigger any ServiceStateChanged event.
                if (displayedSvcState.Equals(querySvcState.ToString()))
                {
                    RefreshDataGridViewEntry(svcName);
                    return;
                }

                // The service state is not actually updated here.
                // Let the Parent decide what to do with this by triggering a ServiceStateChanged event.
                if (ServiceStateChanged != null)
                {
                    Delegate[] delegates = ServiceStateChanged.GetInvocationList();
                    foreach (ServiceStateChangedEventHandler handle in delegates)
                        handle(this, new ServiceStateChangedEventArgs(svcName));
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
            if (dataGridView.Columns[e.ColumnIndex].Name.Equals(ColumnNames.colSvcCmbStart.ToString()) && e.RowIndex >= 0)
            {

                if (dataGridView.Rows[e.RowIndex].Cells[ColumnNames.colSvcCmbStart.ToString()].Selected == true && e.ColumnIndex >= 0)
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
