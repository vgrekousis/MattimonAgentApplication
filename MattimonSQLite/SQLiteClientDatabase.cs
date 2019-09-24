using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonSQLite
{
    public class SQLiteClientDatabase : IDisposable
    {
        private SQLiteDatabaseController SQLiteDatabaseController;

        public SQLiteClientDatabase(String rootDir, String baseDir, String dir, String file,
            Boolean replaceDbFile = false)
        {
            SQLiteDatabaseController = new SQLiteDatabaseController(
                rootDir, baseDir, dir, file,
                replaceDbFile);
        }

        public bool FileExists(out String fullname)
        {
            Boolean exists = SQLiteDatabaseController.DatabaseFileExists(out fullname);
            return exists;
        }

        #region Keys Table
        public bool CreateKeysTable()
        {
            SQLiteDatabaseController.OpenConnection();

            if (SQLiteDatabaseController.TableExist("keys"))
                return true;

            bool created = SQLiteDatabaseController.CreateTable(
                "keys",
                new string[]
                {
                    "key",
                    "user_id",
                    "device_id",
                    "company_id",
                    "device_type_id",
                    "agent_id"
                },
                new string[]
                {
                    "INTEGER PRIMARY KEY AUTOINCREMENT",
                    "int",
                    "int",
                    "int",
                    "int",
                    "text",
                },
                "key",
                true
                );
            SQLiteDatabaseController.CloseConnection();
            return created;
        }
        public bool InsertKey(long userId, long deviceId, long deviceTypeId,long companyId, string agentId)
        {
            long keyId = 0;
            SQLiteDatabaseController.OpenConnection();
            keyId = SQLiteDatabaseController.Insert(
                "keys",
                new String[]
                {
                    "user_id",
                    "device_id",
                    "device_type_id",
                    "company_id",
                    "agent_id"
                },
                new object[]
                {
                    userId,
                    deviceId,
                    deviceTypeId,
                    companyId,
                    agentId
                }
                );
            SQLiteDatabaseController.CloseConnection();
            return keyId > 0;
        }
        public bool UpdateDeviceID(long id)
        {
            long keyId = 0;
            //SQLiteDatabaseController.OpenConnection();
            keyId = SQLiteDatabaseController.Update("keys", 
                new string[] { "device_id" }, new object[] { id }, "WHERE device_id=" + GetDeviceId());
            //SQLiteDatabaseController.CloseConnection();
            return keyId > 0;
        }
        private long[] GetKeys()
        {
            SQLiteDatabaseController.OpenConnection();
            DataTable dt = (DataTable)SQLiteDatabaseController.Select
                ("keys",
                new String[]
                {
                    "user_id",
                    "device_id",
                    "company_id",
                    "device_type_id"
                }
                );

            long[] keys =
                new long[]
                {
                    Convert.ToInt64(dt.Rows[0]["user_id"]),
                    Convert.ToInt64(dt.Rows[0]["device_id"]),
                    Convert.ToInt64(dt.Rows[0]["company_id"]),
                    Convert.ToInt64(dt.Rows[0]["device_type_id"])
                };
            SQLiteDatabaseController.CloseConnection();
            return keys;
        }
        /// <summary>
        /// Get the inserted user id
        /// (Data provider on Authentication---just a reminder)
        /// </summary>
        /// <returns></returns>
        public long GetUserId()
        {
            return GetKeys()[0];
        }
        /// <summary>
        /// Get the inserted device id
        /// (Data provider on Authentication---just a reminder)
        /// </summary>
        /// <returns></returns>
        public long GetDeviceId()
        {
            return GetKeys()[1];
        }
        /// <summary>
        /// Get the company id
        /// (Data provider on Authentication---just a reminder)
        /// </summary>
        /// <returns></returns>
        public long GetCompanyId()
        {
            return GetKeys()[2];
        }
        /// <summary>
        /// Get the device type id
        /// (Data provider on Authentication---just a reminder)
        /// </summary>
        /// <returns></returns>
        public long GetDeviceTypeId()
        {
            return GetKeys()[3];
        }
        /// <summary>
        /// Get the agent id
        /// (Data provider on Authentication---just a reminder)
        /// </summary>
        /// <returns></returns>
        public string GetAgentId()
        {
            SQLiteDatabaseController.OpenConnection();
            String agent = Convert.ToString(SQLiteDatabaseController.Select
                ("keys", new String[] { "agent_id" }));
            SQLiteDatabaseController.CloseConnection();
            return agent;
        }
        #endregion

        #region Config table
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CreateConfigTable()
        {
            SQLiteDatabaseController.OpenConnection();
            if (SQLiteDatabaseController.TableExist("config"))
            {
                SQLiteDatabaseController.CloseConnection();
                return true;
            }

            SQLiteDatabaseController.OpenConnection();
            Boolean created = SQLiteDatabaseController.CreateTable(
                "config",
                new String[]
                {
                    "key",
                    "interval"
                },
                new String[]
                {
                    "INTEGER PRIMARY KEY AUTOINCREMENT",
                    "double"
                },
                "key",
                true
                );
            SQLiteDatabaseController.CloseConnection();
            return created;
        }
        /// <summary>
        /// Insert Reporting Interval
        /// NOTE: Should post this interval to the mattimon api as well
        /// </summary>
        /// <param name="intervalMillis"></param>
        /// <returns></returns>
        public bool InsertConfig(double intervalMillis)
        {
            SQLiteDatabaseController.OpenConnection();
            long configId = 0;
            configId = SQLiteDatabaseController.Insert(
                "config",
                new String[] { "interval" }, new object[] { intervalMillis });
            SQLiteDatabaseController.CloseConnection();
            return configId > 0;
        }
        /// <summary>
        /// Get the reporing interval
        /// Data is provided from the installer or from the configurator---just a reminder.
        /// </summary>
        public double GetReportingInterval()
        {
            SQLiteDatabaseController.OpenConnection();
            double interval = Convert.ToDouble(SQLiteDatabaseController.Select(
                "config", new String[] { "interval" }));
            SQLiteDatabaseController.CloseConnection();
            return interval;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public double SetReportingInterval(double ms)
        {
            SQLiteDatabaseController.OpenConnection();
            SQLiteDatabaseController.Update("config", new string[] { "interval" }, new object[] { ms });
            SQLiteDatabaseController.CloseConnection();
            return ms;
        }
        #endregion

        #region SQL Server Monitoring Related
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CreateConnectionStringTable()
        {
            string name = "connectionstrings";
            SQLiteDatabaseController.OpenConnection();
            if (SQLiteDatabaseController.TableExist(name))
            {
                SQLiteDatabaseController.CloseConnection();
                return true;
            }
            SQLiteDatabaseController.OpenConnection();
            Boolean created = SQLiteDatabaseController.CreateTable(
                name,
                new String[]
                {
                    "key",
                    "connectionstring",
                    "servername",
                    "instancename",
                    "user",
                    "version",
                    "monitored",
                },
                new String[]
                {
                    "INTEGER PRIMARY KEY AUTOINCREMENT",
                    "text",
                    "text",
                    "text",
                    "text",
                    "text",
                    "int"
                },
                "key",
                true
                );
            SQLiteDatabaseController.CloseConnection();
            return created;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        /// <param name="user"></param>
        public void InsertConnectionString(string connectionString, string serverName, string instanceName, string user, string version, bool monitored)
        {
            SQLiteDatabaseController.OpenConnection();
            SQLiteDatabaseController.Insert("connectionstrings", new string[] { "connectionstring", "servername", "instancename", "user", "version", "monitored" },
                new object[] { connectionString, serverName, instanceName, user, version, monitored ? 1 : 0 });
            SQLiteDatabaseController.CloseConnection();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="instanceName"></param>
        public object SelectConnectionString(string serverName, string instanceName)
        {
            SQLiteDatabaseController.OpenConnection();
            object result = SQLiteDatabaseController.Select("connectionstrings", null,
                string.Format("where servername='{0}' and instancename='{1}'", serverName, instanceName));
            SQLiteDatabaseController.CloseConnection();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int DeleteConnectionStringEntry(string serverName, string instanceName)
        {
            SQLiteDatabaseController.OpenConnection();
            int affected = SQLiteDatabaseController.Delete(
                "connectionstrings", 
                "where servername = '" + serverName + "' and " + "instancename = '" + instanceName + "'");
            SQLiteDatabaseController.CloseConnection();
            return affected;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object SelectConnectionStrings()
        {
            SQLiteDatabaseController.OpenConnection();
            object result = SQLiteDatabaseController.Select("connectionstrings", null);
            SQLiteDatabaseController.CloseConnection();
            return result;
        }

        public bool ConnectionStringExists(string serverName, string instanceName)
        {
            SQLiteDatabaseController.OpenConnection();
            if (SQLiteDatabaseController.Select("connectionstrings", null) is DataTable table)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (Convert.ToString(row["servername"]).Equals(serverName) && Convert.ToString(row["instancename"]).Equals(instanceName))
                    {
                        return true;
                    }
                }

                return false;
            }
            SQLiteDatabaseController.OpenConnection();
            throw new Exception("The result was not a DataTable");
        }
        #endregion

        #region Other
        public void Clean()
        {
            SQLiteDatabaseController.Clean();
        }
        public void Delete()
        {
            SQLiteDatabaseController.DeleteDatabaseFileIfExists();
        }
        public void Dispose()
        {
            SQLiteDatabaseController.Dispose();
        }
        #endregion

    }
}
