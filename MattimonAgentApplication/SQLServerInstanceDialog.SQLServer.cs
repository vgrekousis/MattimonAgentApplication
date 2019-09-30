using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication
{
    partial class SQLServerInstanceDialog
    {
        private readonly bool LOCAL_INSTANCES = true;
        private readonly string DEFAULT_INSTANCE_NAME = "MSSQLSERVER";
        /// <summary>
        /// 
        /// </summary>
        private List<string> selections = new List<string>();
        /// <summary>
        /// Assigned in ComboBox Selected Index Changed event handler.
        /// Should hold the selection parameters splitted by a ';'.
        /// </summary>
        private string mServerInstanceSelection;

        /// <summary>
        /// 
        /// </summary>
        private String mSelectedConnectionString;
        /// <summary>
        /// 
        /// </summary>
        private SqlConnectionStringBuilder mSqlConnectionStringBuilder;
        /// <summary>
        /// 
        /// </summary>
        private SQLServerInstanceResult mSqlserverInstanceResult;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorkerEnumerateSQLInstances_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            if (e.Result != null)
            {
                DataTable sources = (DataTable)e.Result;
                EnumerateDataSources(sources, cboServers);
                return;
            }

            MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Data sources could not load.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorkerEnumerateSQLInstances_DoWork(object sender, DoWorkEventArgs e)
        {
            bool useloadscreen = e.Argument == null ? true : (Convert.ToBoolean(e.Argument));

            try
            {
                Invoke((MethodInvoker)delegate () { cboServers.Items.Clear(); });
                Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.AppStarting); });
                Invoke((MethodInvoker)delegate () { LoadScreenEnabled(useloadscreen); });
                Invoke((MethodInvoker)delegate () { btnAccept.Enabled = false; });
                Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = false; });
                Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = false; });
            }
            catch { }
            e.Result = LOCAL_INSTANCES ? EnumLocalInstanceNames() : SqlDataSourceEnumerator.Instance.GetDataSources();
            try
            {
                Invoke((MethodInvoker)delegate () { btnAccept.Enabled = true; });
                Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = true; });
                Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = true; });
                Invoke((MethodInvoker)delegate () { LoadScreenEnabled(false); });
                Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
            }
            catch { }
        }
        private DataTable EnumLocalInstanceNames()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("serverName");
            dataTable.Columns.Add("instanceName");
            dataTable.Columns.Add("fullname");
            dataTable.Columns.Add("IsClustered");
            dataTable.Columns.Add("Version");
            dataTable.Columns.Add("PathLevel");
            dataTable.Columns.Add("value");
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        var value = instanceKey.GetValue(instanceName);

                        RegistryKey innerKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\" + value + "\\Setup", false);
                        string version = string.Empty, patchLevel = string.Empty;

                        foreach (var inner in innerKey.GetValueNames())
                        {
                            if (inner.ToLower() == "version")
                            {
                                version = Convert.ToString(innerKey.GetValue(inner));
                            }
                            if (inner.ToLower() == "patchlevel")
                            {
                                patchLevel = Convert.ToString(innerKey.GetValue(inner));
                            }
                        }

                        var instancename = instanceName == DEFAULT_INSTANCE_NAME ? "" : instanceName;

                        dataTable.Rows.Add(

                            Environment.MachineName,

                            instancename, 

                            String.Format("{0}\\{1}", Environment.MachineName, instancename),

                            null,

                            version,

                            patchLevel,

                            value);
                    }
                }
            }
            return dataTable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlInstancesDialog_HandleCreated(object sender, EventArgs e)
        {
            BackgroundWorkerEnumerateSQLInstances.RunWorkerAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toControl"></param>
        private void EnumerateDataSources(DataTable source, Control toControl = null)
        {
            //Dictionary<string, string> sources = new Dictionary<string, string>();
            List<string> instances = new List<string>();

            foreach (DataRow dr in source.Rows)
            {
                String srv = Convert.ToString(dr["ServerName"]);
                String ins = Convert.ToString(dr["InstanceName"]);
                Boolean clr = Convert.ToString(dr["IsClustered"]).ToLower().Equals("yes");
                String ver = Convert.ToString(dr["Version"]);
                String formated = String.Format("{0};{1};{2};{3}", srv, ins, clr, ver);
                selections.Add(formated);

                instances.Add(srv + "\\" + ins);
            }

            
            if (instances.Count < 1)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this,
                    "SqlDataSourceEnumerator could not locate any visible SQL Server instances within your network.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            if (toControl == null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (String src in instances)
                {
                    sb.AppendLine(src);
                    sb.AppendLine();
                }
                MessageBox.Show(this, sb.ToString().Equals(String.Empty) ? "No sources where loaded" : sb.ToString(), "Available sources", MessageBoxButtons.OK);
                return;
            }

            if (toControl is ComboBox comboBox)
            {
                comboBox.Items.Add("<Select>");

                foreach (String src in instances)
                    cboServers.Items.Add(src);

                cboServers.SelectedIndex = 0;
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server">Example: DT-DBSRV01\\MSSQLSRV01</param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="catalogue"></param>
        /// <returns></returns>
        private String CreateConnectionString(String server, String user, String password, String catalogue)
        {
            mSqlConnectionStringBuilder = new SqlConnectionStringBuilder(String.Format(
                "Server={0};Database={1};UID={2};Pwd={3}", server, catalogue, user, password));
            return mSqlConnectionStringBuilder.ConnectionString;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (BackgroundWorkerEnumerateSQLInstances.IsBusy)
            {
                BackgroundWorkerEnumerateSQLInstances.CancelAsync();
                BackgroundWorkerEnumerateSQLInstances.Dispose();
            }
            mDialogResult = DialogResult.Cancel;
            mSqlserverInstanceResult = SQLServerInstanceResult.CancelResult;
            Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            Accept();
        }
        /// <summary>
        /// Thread
        /// </summary>
        /// <returns></returns>
        private void Accept()
        {
            if (cboServers.SelectedIndex < 1 || txtPwrd.TextLength < 1 || txtUser.TextLength < 1)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Required information not provided.", Text, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Hide();
            String cs = CreateConnectionString(
                   cboServers.SelectedItem.ToString(),
                   txtUser.Text.Trim(),
                   txtPwrd.Text,
                   "master");

            new System.Threading.Thread(() =>
            {
                Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.AppStarting); });
                Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = false; });
                Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = false; });
                Invoke((MethodInvoker)delegate () { btnAccept.Enabled = false; });

                using (SqlConnection sqlconnection = new SqlConnection(cs))
                {
                    try
                    {
                        sqlconnection.Open();
                        sqlconnection.Close();

                        // Acceptable connection close the dialogue -- thread safe.
                        Invoke((MethodInvoker)delegate ()
                        {
                            mSelectedConnectionString = cs;
                           
                            string srvname, insname, srvversion;
                            bool clustered;
                            try
                            {
                                string[] parameters = mServerInstanceSelection.Split(';');
                                srvname = parameters[0];
                                insname = parameters[1];
                                clustered = Convert.ToBoolean(parameters[2]);
                                srvversion = parameters[3];
                                mDialogResult = DialogResult.OK;
                                mSqlserverInstanceResult = new SQLServerInstanceResult(mDialogResult,
                                    srvname, insname, clustered, srvversion, mSqlConnectionStringBuilder);
                                
                            }
                            catch (Exception ex)
                            {
                                Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
                                GUI.BitscoreForms.BitscoreMessageBox.Show(this, 
                                    "An error occurred while parsing the server instance selection.\n\n" +
                                    "Error details:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Error", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Close();
                                return;
                            }

                            Close();
                        });
                    }
                    catch (SqlException)
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
                            MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show("Unable to connect to SQL Server.\nMake sure that the provided credentials are valid and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch { }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
                            MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show("Unable to connect to SQL Server.\nMake sure that the provided credentials are valid and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch { }
                    }
                    finally
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
                            Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = true; });
                            Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = true; });
                            Invoke((MethodInvoker)delegate () { btnAccept.Enabled = true; });
                        }
                        catch { }
                    }
                }
            }).Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestConnection_Click1(object sender, EventArgs e)
        {
            if (cboServers.SelectedIndex < 1 || txtPwrd.TextLength < 1 || txtUser.TextLength < 1)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Required information not provided.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult innerResult = DialogResult.Retry;
            while (innerResult == DialogResult.Retry)
            {
                using (SqlConnection sqlconnection = new SqlConnection())
                {
                    try
                    {
                        sqlconnection.Open();
                        sqlconnection.Close();
                        MessageBox.Show(this, "Connection successful.", Text, MessageBoxButtons.OK);
                        innerResult = DialogResult.OK;
                    }
                    catch (SqlException)
                    {
                        innerResult = MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Connection not successful.", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    catch (Exception)
                    {
                        innerResult = MattimonAgentApplication.GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Connection not successful.", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestConnection_Click2(object sender, EventArgs e)
        {
            if (cboServers.SelectedIndex < 1 || txtPwrd.TextLength < 1 || txtUser.TextLength < 1)
            {
                GUI.BitscoreForms.BitscoreMessageBox.Show(this, "Required information not provided.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            String cs = CreateConnectionString(
                   cboServers.SelectedItem.ToString(),
                   txtUser.Text.Trim(),
                   txtPwrd.Text,
                   "master");


            new System.Threading.Thread(() =>
            {
                Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.AppStarting); });
                Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = false; });
                Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = false; });
                Invoke((MethodInvoker)delegate () { btnAccept.Enabled = false; });

                using (SqlConnection sqlconnection = new SqlConnection(cs))
                {
                    try
                    {
                        sqlconnection.Open();
                        sqlconnection.Close();

                        try
                        {
                            Invoke((MethodInvoker)delegate () { lblStatus.ForeColor = Color.FromArgb(34, 177, 76); });
                            Invoke((MethodInvoker)delegate () { lblStatus.Text = "Connection successful"; });
                        }
                        catch { }
                    }
                    catch (SqlException)
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { lblStatus.ForeColor = Color.FromArgb(255, 100, 100); });
                            Invoke((MethodInvoker)delegate () { lblStatus.Text = "Connection not successful."; });
                        }
                        catch { }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { lblStatus.ForeColor = Color.FromArgb(255, 100, 100); });
                            Invoke((MethodInvoker)delegate () { lblStatus.Text = "Connection not successful."; });
                        }
                        catch { }
                    }
                    finally
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate () { ApplyCursorToControlTree(Cursors.Default); });
                            Invoke((MethodInvoker)delegate () { pnlSqlCred.Enabled = true; });
                            Invoke((MethodInvoker)delegate () { btnTestConnection.Enabled = true; });
                            Invoke((MethodInvoker)delegate () { btnAccept.Enabled = true; });
                            Invoke((MethodInvoker)delegate () { lblStatus.Show(); });
                        }
                        catch { }

                    }
                }
            }).Start();
        }
    }
}
