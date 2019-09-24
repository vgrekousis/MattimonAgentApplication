using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentApplication.GUI.BitscoreForms;
using System.Data;
using MattimonAgentLibrary.WMI;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentApplication.GUI.Events;
using MattimonSQLite;

namespace MattimonAgentApplication
{
    partial class FormDashboard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSqlSrv_AddNew_Click(object sender, EventArgs e)
        {
            while (true) {
                SQLServerInstanceDialog serverInstanceDialog = new SQLServerInstanceDialog();
                SQLServerInstanceResult result = serverInstanceDialog.Show(this);

                if (result.DialogResult == DialogResult.OK)
                {
                    bool entryExists = false;
                    try { entryExists = ConnectionStringEntryExists(result.ServerName, result.InstanceName); }
                    catch { }

                    if (entryExists)
                    {
                        if (GUI.BitscoreForms.BitscoreMessageBox.Show(this, 
                            "The connection string you've selected is already set in the Mattimon SQL Service.",
                            Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                            return;

                        continue;
                    }

                    if (GUI.BitscoreForms.BitscoreMessageBox.Show( this, "You're about to add " + result.ServerName + (result.InstanceName != "" ? "//" + result.InstanceName : "") + " in the Mattimon Service.\nIs that correct?",
                        "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        continue;
                   
                    lblStatus.Text = "Posting Server Instances...";
                    try
                    {
                        LocalWriteConnectionString(result);
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Failed";
                        GUI.BitscoreForms.BitscoreMessageBox.Show(this, Static.UserEndErrorMessages.SQLITE_ERROR_WRITE_CONNECTION_STRINGS + ex.Message + "\n\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatus.Text = "Ready";
                        return;
                    }

                    DeviceServerObjects dso = new SQLServerObjectProvider().GetDeviceServerObjects(
                        new ServerInstance[] {
                            new ServerInstance {
                                ConnectionString = result.ConnectionString
                            },
                        }, GetDeviceID(), out Exception outex);

                    if (outex != null)
                    {
                        GUI.BitscoreForms.BitscoreMessageBox.Show(this, outex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MattimonSqlServerPostDataWorker.RunWorkerAsync(dso);
                    
                    break;
                }
                break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSqlSrv_Refresh_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Refreshing...";
            btn_sqlsrv_refresh.Enabled = false;
            MattimonSqlServerDataRequestWorker.RunWorkerAsync(mFromServerDeviceServerObjects);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSqlSrv_DeleteInstances_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented. Use the \"Delete\" button on the grid instread.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonCell"></param>
        /// <param name="e"></param>
        private void InstancesGrid_DeleteInstance_Click(object buttonCell, DeleteInstanceClickEventArgs e)
        {
            if (GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                string.Format("You're about to delete your {0}\\{1} from the monitoring service. Are you sure?", e.ServerName, e.InstanceName), "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)

                DeleteSqlServerInstance(new ServerInstance[] {
                new ServerInstance
                {
                    ID = e.InstancePrimaryKey,
                    ServerName = e.ServerName,
                    InstanceName = e.InstanceName
                }});
        }
    }
}
