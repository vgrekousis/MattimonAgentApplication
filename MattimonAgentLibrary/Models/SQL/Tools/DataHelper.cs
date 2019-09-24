using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattimonAgentLibrary.Models.SQL;
using System.Data;
using MattimonAgentLibrary.MattimonEnum;
using MattimonAgentLibrary.Tools;

namespace MattimonAgentLibrary.Models.SQL.Tools
{
    public class DataHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverObjects"></param>
        /// <returns></returns>
        public static DataSet CreateDataSet(DeviceServerObjects serverObjects, bool local)
        {
            DataTable instances = new DataTable("instances");
            instances.Columns.Add("pk", typeof(Int32));
            instances.Columns.Add("servername", typeof(String));
            instances.Columns.Add("instancename", typeof(String));
            instances.Columns.Add("clustered", typeof(String));
            instances.Columns.Add("version", typeof(String));
            instances.Columns.Add("user", typeof(String));
            instances.Columns.Add("monitored", typeof(String));
            instances.Columns.Add("numberdatabases", typeof(Int32));
            instances.Columns.Add("lastreported", typeof(DateTime));
            if (!local)
                instances.PrimaryKey = new DataColumn[] { instances.Columns[0] };
            ///
            /// Databases
            ///
            DataTable databases = new DataTable("databases");
            databases.Columns.Add("pk", typeof(Int32));
            databases.Columns.Add("fk_instances", typeof(Int32));
            databases.Columns.Add("collationname", typeof(String));
            databases.Columns.Add("state", typeof(Int32));
            databases.Columns.Add("createdate", typeof(Int32));
            databases.Columns.Add("monitored", typeof(String));
            if (!local)
                databases.PrimaryKey = new DataColumn[] { databases.Columns[0] };
            ///
            /// Connections
            ///
            DataTable connections = new DataTable("connections");
            connections.Columns.Add("pk", typeof(Int32));
            connections.Columns.Add("fk_databases", typeof(Int32));
            connections.Columns.Add("spid", typeof(Int32));
            connections.Columns.Add("spid2", typeof(Int32));
            connections.Columns.Add("status", typeof(String));
            connections.Columns.Add("login", typeof(String));
            connections.Columns.Add("host", typeof(String));
            connections.Columns.Add("blkby", typeof(String));
            connections.Columns.Add("dbname", typeof(String));
            connections.Columns.Add("command", typeof(String));
            connections.Columns.Add("CPU Time", typeof(Int64));
            connections.Columns.Add("diskio", typeof(Int64));
            connections.Columns.Add("lastbatch", typeof(String));
            connections.Columns.Add("programname", typeof(String));
            connections.Columns.Add("requestid", typeof(Int32));
            ///
            /// DataSet
            ///
            DataSet serverObjectsDataSet = new DataSet();
            serverObjectsDataSet.Tables.Add(instances);
            serverObjectsDataSet.Tables.Add(databases);
            serverObjectsDataSet.Tables.Add(connections);

            if (!local)
            {
                serverObjectsDataSet.Relations.Add(new DataRelation("instances-databases", instances.Columns[0], databases.Columns[1]));
                serverObjectsDataSet.Relations.Add(new DataRelation("databases-connections", databases.Columns[0], connections.Columns[1]));
            }

            // If there are instances
            if (serverObjects.Instances != null && serverObjects.Instances.Length > 0)
            {
                foreach (ServerInstance si in serverObjects.Instances)
                {
                    // Add instances in instances DataTable
                    int dblen = si.Databases != null ? si.Databases.Length : 0;
                    instances.Rows.Add(si.ID, si.ServerName, si.InstanceName, si.Clustered ? "Yes" : "No", si.Version, si.User, si.Monitored ? MonitorSwitch.Enabled.ToString() : MonitorSwitch.Disabled.ToString(), dblen, DateUtils.UnixTimeStampToDateTime(si.LastReportedTimestamp));

                    // Then if there are databases
                    if (si.Databases != null && si.Databases.Length > 0)
                    {

                        foreach (Database db in si.Databases)
                        {

                            // Add databases in databases DataTable
                            databases.Rows.Add(db.ID, db.ServerInstanceID, db.CollationName, db.State, db.CreateDateTimestamp, db.Monitored ? MonitorSwitch.Enabled.ToString() : MonitorSwitch.Disabled.ToString());

                            // Then if there are connections
                            if (db.Connections != null && db.Connections.Length > 0)
                            {
                                foreach (DatabaseConnectionInfo ci in db.Connections)
                                {
                                    // Add connections in connections DataTable
                                    connections.Rows.Add(
                                        ci.ID, ci.DatabaseID, ci.SPID,
                                        ci.SPID2, ci.Status, ci.Login,
                                        ci.Hostname, ci.BlkBy, ci.DatabaseName,
                                        ci.Command, ci.CpuTime, ci.DiskIO,
                                        ci.LastBatch, ci.ProgramName, ci.RequestID);

                                } // end foreach DatabaseConnectionInfo

                            } // end if Connections

                        } // end foreach Database

                    } // end if Databases

                } // end foreach ServerInstance
            }

            return serverObjectsDataSet;
        }
    }
}
