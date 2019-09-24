using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MattimonAgentLibrary.Models.SQL;
using MattimonAgentLibrary.Tools;
using System.Text.RegularExpressions;
using System.ServiceProcess;

namespace MattimonAgentLibrary.WMI
{
    public class SQLServerObjectProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly string mConnectionString;
        public SQLServerObjectProvider()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SQLServerObjectProvider(string connectionString)
        {
            mConnectionString = connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public List<Database> GetInstanceDatabases(out Exception exception)
        {
            exception = null;
            List<Database> instanceDatabases = new List<Database>();
            using (SqlConnection sqlConnection = new SqlConnection(mConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }

                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.CommandText = @"select [name], database_id, create_date, collation_name, [state] from sys.databases;";
                    DataTable databases = GetDataTable(cmd);

                    try
                    {
                        foreach (DataRow db in databases.Rows)
                        {
                            instanceDatabases.Add(new Database
                            {
                                DatabaseName = Convert.ToString(db["name"]),
                                ClientDatabaseID = Convert.ToInt64(db["database_id"]),
                                CreateDateTimestamp = (int)DateUtils.GetUnixTimestamp(Convert.ToDateTime(db["create_date"])),
                                CollationName = Convert.ToString(db["collation_name"]),
                                State = Convert.ToInt32(db["state"])
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }
            }

            return instanceDatabases;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public List<Database> GetInstanceDatabases(string connectionString, out Exception exception)
        {
            exception = null;
            List<Database> instanceDatabases = new List<Database>();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }

                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.CommandText = @"select [name], database_id, create_date, collation_name, [state] from sys.databases;";
                    DataTable databases = GetDataTable(cmd);

                    try
                    {
                        foreach (DataRow db in databases.Rows)
                        {
                            instanceDatabases.Add(new Database
                            {
                                DatabaseName = Convert.ToString(db["name"]),
                                ClientDatabaseID = Convert.ToInt64(db["database_id"]),
                                CreateDateTimestamp = (int)DateUtils.GetUnixTimestamp(Convert.ToDateTime(db["create_date"])),
                                CollationName = Convert.ToString(db["collation_name"]),
                                State = Convert.ToInt32(db["state"])
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }
            }

            return instanceDatabases;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public List<DatabaseConnectionInfo> GetConnections(string dbname, out Exception exception)
        {
            exception = null;
            List<DatabaseConnectionInfo> databaseConnectionInfos = new List<DatabaseConnectionInfo>();
            using (SqlConnection sqlConnection = new SqlConnection(mConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch(Exception ex)
                {
                    exception = ex;
                    return null;
                }

                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.CommandText = @"
                    declare @tbl_sp_who table (
                    spid int,
                    [status] nvarchar(max),
                    [login] nvarchar(max),
                    hostname nvarchar(max),
                    blkby nvarchar(max),
                    dbname nvarchar(max),
                    command nvarchar(max),
                    cputime bigint,
                    diskio bigint,
                    lastbatch nvarchar(max),
                    programname nvarchar(max),
                    [spid2] int,
                    requestid int); 
                    insert into @tbl_sp_who exec sp_who2;
                    select * from @tbl_sp_who where dbname=@dbname";
                    cmd.Parameters.Add("@dbname", SqlDbType.NVarChar).Value = dbname;
                    DataTable connections = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(connections);
                    adapter.Dispose();
                    cmd.Dispose();
                    sqlConnection.Close();

                    try
                    {
                        if (connections.Rows.Count > 0)
                        {
                            foreach (DataRow connection in connections.Rows)
                            {
                                databaseConnectionInfos.Add(new DatabaseConnectionInfo
                                {
                                    SPID = Convert.ToInt32(connection["spid"]),
                                    Status = Convert.ToString(connection["status"]),
                                    Login = Convert.ToString(connection["login"]),
                                    Hostname = Convert.ToString(connection["hostname"]),
                                    BlkBy = Convert.ToString(connection["blkby"]),
                                    DatabaseName = Convert.ToString(connection["dbname"]),
                                    Command = Convert.ToString(connection["command"]),
                                    CpuTime = Convert.ToInt64(connection["cputime"]),
                                    DiskIO = Convert.ToInt64(connection["diskio"]),
                                    LastBatch = Convert.ToString(connection["lastbatch"]),
                                    ProgramName = Convert.ToString(connection["programname"]),
                                    SPID2 = Convert.ToInt32(connection["spid2"]),
                                    RequestID = Convert.ToInt32(connection["requestid"])
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }
            }
           return databaseConnectionInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbname"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public List<DatabaseConnectionInfo> GetConnections(string connectionString, string dbname, out Exception exception)
        {
            exception = null;
            List<DatabaseConnectionInfo> databaseConnectionInfos = new List<DatabaseConnectionInfo>();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }

                try
                {
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    cmd.CommandText = @"
                    declare @tbl_sp_who table (
                    spid int,
                    [status] nvarchar(max),
                    [login] nvarchar(max),
                    hostname nvarchar(max),
                    blkby nvarchar(max),
                    dbname nvarchar(max),
                    command nvarchar(max),
                    cputime bigint,
                    diskio bigint,
                    lastbatch nvarchar(max),
                    programname nvarchar(max),
                    [spid2] int,
                    requestid int); 
                    insert into @tbl_sp_who exec sp_who2;
                    select * from @tbl_sp_who where dbname=@dbname";
                    cmd.Parameters.Add("@dbname", SqlDbType.NVarChar).Value = dbname;
                    DataTable connections = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(connections);
                    adapter.Dispose();
                    cmd.Dispose();
                    sqlConnection.Close();

                    try
                    {
                        if (connections.Rows.Count > 0)
                        {
                            foreach (DataRow connection in connections.Rows)
                            {
                                databaseConnectionInfos.Add(new DatabaseConnectionInfo
                                {
                                    SPID = Convert.ToInt32(connection["spid"]),
                                    Status = Convert.ToString(connection["status"]),
                                    Login = Convert.ToString(connection["login"]),
                                    Hostname = Convert.ToString(connection["hostname"]),
                                    BlkBy = Convert.ToString(connection["blkby"]),
                                    DatabaseName = Convert.ToString(connection["dbname"]),
                                    Command = Convert.ToString(connection["command"]),
                                    CpuTime = Convert.ToInt64(connection["cputime"]),
                                    DiskIO = Convert.ToInt64(connection["diskio"]),
                                    LastBatch = Convert.ToString(connection["lastbatch"]),
                                    ProgramName = Convert.ToString(connection["programname"]),
                                    SPID2 = Convert.ToInt32(connection["spid2"]),
                                    RequestID = Convert.ToInt32(connection["requestid"])
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return null;
                }
            }
            return databaseConnectionInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public DeviceServerObjects GetDeviceServerObjects(ServerInstance[] instances, long deviceID, out Exception exception)
        {
            exception = null;

            List<string> connectionStrings = instances.ToList().Select(si => si.ConnectionString).ToList();
            List<ServerInstance> serverInstances = GetServerInstances(connectionStrings, out exception);
            
            if (serverInstances != null)
            return new DeviceServerObjects
            {
                DeviceID = deviceID,
                Instances = serverInstances.ToArray()
            };
            
            return null;
        }

        /// <summary>
        /// Retrieves associated Server Instances. In the case where failed to connect to the instance
        /// The included <code>ServerInstance</code> will carry the data as provided in the associated connection string.
        /// </summary>
        /// <param name="connectionStrings"></param>
        /// <param name="exception"></param>
        /// <returns>Returns a <code>List</code> of <code>ServerInstance</code> or <code>null</code> if exceptions occurred during proccess.</returns>
        private List<ServerInstance> GetServerInstances(List<string> connectionStrings, out Exception exception)
        {
            exception = null;
            List<ServerInstance> serverInstances = new List<ServerInstance>();

            foreach (string cs in connectionStrings)
            {
                SqlConnection sqlConnection = new SqlConnection(cs);
                
                ServerInstance thisServerInstance = new ServerInstance
                {
                    ConnectStatus = 0
                };

                try
                {
                    sqlConnection.Open();
                    thisServerInstance.ConnectStatus = 1;
                }
                catch { /* TODO: Simply let the ConnectStatus to 0 */ }

                // if the connection was successful
                if (thisServerInstance.ConnectStatus == 1)
                {
                    List<Database> databases = new List<Database>();

                    // We can now proceed with further querying.
                    try
                    {
                        // Step 1: Request sql server instance information

                        // create the command
                        SqlCommand cmd = sqlConnection.CreateCommand();
                        cmd.CommandText =
                            "SELECT SERVERPROPERTY('ServerName') as 'server_name'," +
                            "CAST(CASE WHEN SERVERPROPERTY('InstanceName') is not null THEN SERVERPROPERTY('InstanceName') " +
                            "ELSE @@SERVICENAME END AS nvarchar(max)) as 'instance_name'," + "" +
                            "SERVERPROPERTY('ProductVersion') as 'version'," + "" +
                            "SERVERPROPERTY('IsClustered') as 'clustered'," +
                            "SERVERPROPERTY('IsSingleUser') as 'is_single_user'," +
                            "@@SERVICENAME as 'service_name'," +
                            "SYSTEM_USER as 'user'";

                        // Store the results in the memory
                        DataTable serverInfoDataTable = new DataTable();
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.Fill(serverInfoDataTable);
                        sqlDataAdapter.Dispose();

                        // dispose the command
                        cmd.Dispose();

                        // assign values to the server instance object
                        thisServerInstance.ServerName = Convert.ToString(serverInfoDataTable.Rows[0]["server_name"]);
                        thisServerInstance.InstanceName = Convert.ToString(serverInfoDataTable.Rows[0]["instance_name"]);
                        thisServerInstance.Clustered = Convert.ToInt32(serverInfoDataTable.Rows[0]["clustered"]) == 1;
                        thisServerInstance.User = Convert.ToString(serverInfoDataTable.Rows[0]["user"]);
                        thisServerInstance.Version = Convert.ToString(serverInfoDataTable.Rows[0]["version"]);
                        thisServerInstance.ConnectionString = cs;

                        // collect the main sql service and statuses
                        string instancename = new SqlServerCredentials(cs).Instance, svcname = string.Empty, displayname = string.Empty;
                        int starttype = -1; // assume -1 as if service was not found
                        bool isrunning = false; // assume false by default

                        svcname = instancename != string.Empty ?
                            // named instance, assume that the service name is formatted as MSSQL$<instancename>
                            string.Format("MSSQL${0}", instancename) :
                            // unnamed instance, assume the default instance name as service name
                            "MSSQLSERVER";

                        try
                        {
                            ServiceController svcctl = new ServiceController(svcname);
                            displayname = svcctl.DisplayName;
                            starttype = (int)svcctl.StartType;
                            isrunning = svcctl.Status == ServiceControllerStatus.Running;
                        }
                        catch
                        {
                            // starttype keeps its default value -1.
                            // this will help us debug our svcname as passed in ServiceController
                            // where most lickely the svcname was invalid.
                        }


                        // set the service information on the server instance object
                        thisServerInstance.SQLService = new SQLService
                        {
                            Name = svcname,
                            DisplayName = displayname,
                            IsMainService = true,
                            IsRunning = isrunning,
                            StartType = starttype
                        };


                        // Dispose the datatable
                        serverInfoDataTable.Dispose();

                        // Step 2: Request databases belonging to this sql server instance

                        // create the command
                        cmd = sqlConnection.CreateCommand();
                        cmd.CommandText = @"select [name], database_id, create_date, collation_name, [state] from sys.databases;";
                        sqlDataAdapter = new SqlDataAdapter(cmd);
                        DataTable databasesDataTable = new DataTable();
                        sqlDataAdapter.Fill(databasesDataTable);
                        sqlDataAdapter.Dispose();

                        // dispose the command
                        cmd.Dispose();

                        // for each database row in the datatable
                        foreach (DataRow dbrow in databasesDataTable.Rows)
                        {
                            // Initialize the DatabaseConnectionInfo List
                            List<DatabaseConnectionInfo> connections = new List<DatabaseConnectionInfo>();

                            // create a new Database object
                            Database thisDatabase = new Database()
                            {
                                ClientDatabaseID = Convert.ToInt64(dbrow["database_id"]),
                                CollationName = Convert.ToString(dbrow["collation_name"]),
                                CreateDateTimestamp = (int)DateUtils.GetUnixTimestamp(Convert.ToDateTime(dbrow["create_date"])),
                                DatabaseName = Convert.ToString(dbrow["name"]),
                                State = Convert.ToInt32(dbrow["state"])
                            };

                            // Step 3: Request connections belonging to this database.
                            
                            // create the command
                            cmd = sqlConnection.CreateCommand();
                            cmd.CommandText = @"
                            declare @tbl_sp_who table (
                            spid int,
                            [status] nvarchar(max),
                            [login] nvarchar(max),
                            hostname nvarchar(max),
                            blkby nvarchar(max),
                            dbname nvarchar(max),
                            command nvarchar(max),
                            cputime bigint,
                            diskio bigint,
                            lastbatch nvarchar(max),
                            programname nvarchar(max),
                            [spid2] int,
                            requestid int); 
                            insert into @tbl_sp_who exec sp_who2;
                            select * from @tbl_sp_who where dbname=@dbname";

                            cmd.Parameters.Add("@dbname", SqlDbType.NVarChar).Value = thisDatabase.DatabaseName;
                            sqlDataAdapter = new SqlDataAdapter(cmd);
                            DataTable connectionsDataTable = new DataTable();
                            sqlDataAdapter.Fill(connectionsDataTable);

                            // dispose the adapter
                            sqlDataAdapter.Dispose();

                            // Dispose the command
                            cmd.Dispose();

                            // foreach connection in the datatable
                            foreach (DataRow conrow in connectionsDataTable.Rows)
                            {
                                // create a new DatabaseConnectionInfo object
                                DatabaseConnectionInfo thisConnection = new DatabaseConnectionInfo
                                {
                                    SPID = Convert.ToInt32(conrow["spid"]),
                                    Status = Convert.ToString(conrow["status"]),
                                    Login = Convert.ToString(conrow["login"]),
                                    Hostname = Convert.ToString(conrow["hostname"]),
                                    BlkBy = Convert.ToString(conrow["blkby"]),
                                    DatabaseName = Convert.ToString(conrow["dbname"]),
                                    Command = Convert.ToString(conrow["command"]),
                                    CpuTime = Convert.ToInt64(conrow["cputime"]),
                                    DiskIO = Convert.ToInt64(conrow["diskio"]),
                                    LastBatch = Convert.ToString(conrow["lastbatch"]),
                                    ProgramName = Convert.ToString(conrow["programname"]),
                                    SPID2 = Convert.ToInt32(conrow["spid2"]),
                                    RequestID = Convert.ToInt32(conrow["requestid"])
                                };

                                // add the object in the connectins collection
                                connections.Add(thisConnection);
                            }

                            // dispose the datatable 
                            connectionsDataTable.Dispose();

                            // set the connections collection to the database object connections
                            thisDatabase.Connections = connections.ToArray();


                            // Step 4: Request database meta data

                            // load and set the database meta data
                            thisDatabase.MetaData = GetDatabaseMetaData(sqlConnection, thisDatabase.DatabaseName);

                            // add the database object to the databases collection
                            databases.Add(thisDatabase);
                        }

                        // dispose the datatable
                        databasesDataTable.Dispose();

                        // set the databases collection to the server instance object databases
                        thisServerInstance.Databases = databases.ToArray();
                    }

                    // if any error possible occurred during the entire proccess
                    catch (Exception ex)
                    {
                        sqlConnection.Close();
                        // assign the out parameter
                        exception = ex;

                        // end the process and return null
                        return null;
                    }

                    // close the connection
                    sqlConnection.Close();

                } // end of server instance connection check

                // Could not connect to the sql server instance
                else
                {
                    // TODO: The case where could not connect to the sql server instance
                    // Set the the data from the provided connection string.
                    SqlServerCredentials credentials = new SqlServerCredentials(cs);
                    thisServerInstance.ServerName = credentials.Server;
                    thisServerInstance.InstanceName = credentials.Instance;
                    thisServerInstance.User = credentials.User;
                    thisServerInstance.Version = "n/a";
                    thisServerInstance.Databases = new Database[0];

                    string svcname = string.Empty, displayname = string.Empty;
                    int starttype = -1; // by default, we assume service not found
                    bool isrunning = false;

                    // Try and get the instance service statuses
                    // We need to know if the service was at least running while the connection with the instance couldn't be established.

                    if (credentials.Instance == string.Empty) // The instance is a default instance
                    {
                        svcname = "MSSQLSERVER"; // assume the default mssql service name

                        try
                        {
                            ServiceController svcc = new ServiceController(svcname);
                            starttype = (int)svcc.StartType;
                            displayname = svcc.DisplayName;
                            isrunning = svcc.Status == ServiceControllerStatus.Running;
                            
                        }
                        catch (Exception ex) { displayname = "Error: " + ex.Message; }
                    }

                    
                    else // The instance was a named instance
                    {
                        svcname = string.Format("MSSQL${0}", credentials.Instance); // format the named instance service name

                        try
                        {
                            // Try and get the named instance service statuses
                            ServiceController svcc = new ServiceController(svcname);
                            starttype = (int)svcc.StartType;
                            displayname = svcc.DisplayName;
                            isrunning = svcc.Status == ServiceControllerStatus.Running;
                        }
                        catch { }
                    }

                    // Whatever the result was, set the service information to the server instance object.
                    thisServerInstance.SQLService = new SQLService
                    {
                        Name = svcname,
                        DisplayName = displayname,
                        IsMainService = true,
                        StartType = starttype, // keeps default value of -1 if ServiceController thrown an exception (Service may not exist)
                        IsRunning = isrunning
                    };
                }

                // add the server instance object to the server instances collection
                serverInstances.Add(thisServerInstance);

            } // end foreach connection string (server instance)

            // return the server instances
            return serverInstances;
        }
        private DatabaseMetaData GetDatabaseMetaData(SqlConnection activeConnection, string dbname)
        {
            DatabaseMetaData databaseMetaData = new DatabaseMetaData();

            SqlCommand cmd = activeConnection.CreateCommand();
            cmd.CommandText = "sp_helpdb";
            cmd.Parameters.Add("@dbname", SqlDbType.NVarChar).Value = dbname;
            cmd.CommandType = CommandType.StoredProcedure;
            DataSet dataset = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dataset);
            adapter.Dispose();
            cmd.Dispose();

            if (dataset.Tables != null && dataset.Tables.Count == 2)
            {
                DataRow primaryinfo = dataset.Tables[0].Rows[0];
                string metrics = string.Empty;
                databaseMetaData.DBSize = DbSizeStringToLong(dataset.Tables[0].Rows[0], "db_size", out metrics);
                databaseMetaData.MetricsDBSize = metrics;
                DataTable dbfileinfos = dataset.Tables[1];

                foreach (DataRow dbfi in dbfileinfos.Rows)
                {
                    // if database file
                    if (Convert.ToInt32(dbfi["fileid"]) == 0)
                    {
                        string metricsMaxSize;
                        databaseMetaData.DBMaxSize = DbSizeStringToLong(dbfi, "maxsize", out metricsMaxSize);
                        databaseMetaData.MetricsDBMaxSize = metricsMaxSize;
                        databaseMetaData.DbFileSizePercentGrowth = GetInt32FromString(Convert.ToString(dbfi["growth"]));
                    }

                    // if log file
                    if (Convert.ToInt32(dbfi["fileid"]) == 1)
                    {
                        //System.Windows.Forms.MessageBox.Show(Convert.ToString(dbfi["size"]), "Debug");
                        string metricsMaxSize;
                        databaseMetaData.LogMaxSize = DbSizeStringToLong(dbfi, "maxsize", out metricsMaxSize);
                        databaseMetaData.MetricsLogMaxSize = metricsMaxSize;
                        databaseMetaData.LogFileSizePercentGrowth = GetInt32FromString(Convert.ToString(dbfi["growth"]));
                    }
                }
            }

            cmd = activeConnection.CreateCommand();
            cmd.CommandText = "declare @tblSqlPerfLogspace table" +
                "(" +
                "dbname nvarchar(max)," +
                "mb_logsize float," +
                "percent_logspace_used float," +
                "[status] int" +
                ");" +
                "insert into @tblSqlPerfLogspace exec('dbcc sqlperf (logspace)');" +
                "select * from @tblSqlPerfLogspace where[dbname] = @dbname;";

            cmd.Parameters.Add("@dbname", SqlDbType.NVarChar).Value = dbname;
            DataTable datatable = new DataTable();
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(datatable);
            adapter.Dispose();
            cmd.Dispose();

            if (datatable.Rows != null && datatable.Rows.Count > 0)
            {
                databaseMetaData.LogSize = Convert.ToDouble(datatable.Rows[0]["mb_logsize"]);
                databaseMetaData.MetricsLogSize = "MB";
                databaseMetaData.LogFileSizePercentUsed = Convert.ToInt32(datatable.Rows[0]["percent_logspace_used"]);
            }

            return databaseMetaData;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="sizename"></param>
        /// <returns>Returns -1 if the <paramref name="sizename"/> is not found in the datarow column</returns>
        private long DbSizeStringToLong(DataRow dr, string sizename, out string metrics)
        {
            metrics = string.Empty;
            string strval = null; long longval = 0;

            try { strval = Convert.ToString(dr[sizename]).ToLower(); }
            catch { longval = -1; }

            if (longval != -1)
            {
                if (strval.Equals(string.Empty) || strval.Equals(null))
                    longval = -2;

                if (strval.Equals("unlimited"))
                    longval = -3;
            }

            if (longval == 0)
            {
                metrics = Regex.Match(Convert.ToString(dr[sizename]), @"[A-Z][A-Z]").Value;
                string strDecimal = Regex.Match(Convert.ToString(dr[sizename]), @"\d+").Value;
                longval = Convert.ToInt64(strDecimal);
            }

            return longval;
        }
        private int GetInt32FromString(string value)
        {
            try
            {
                return Convert.ToInt32(Regex.Match(value, @"\d+").Value);
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="disposeResources"></param>
        /// <returns></returns>
        private DataTable GetDataTable(SqlCommand cmd, bool disposeResources = true)
        {
            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(table);

            if (disposeResources)
            {
                cmd.Connection.Close();
                adapter.Dispose();
                cmd.Dispose();
            }

            return table;
        }
    }

    public class SqlServerCredentials
    {
        private readonly string user;
        private readonly string pwrd;
        private readonly string server;
        private readonly string instance;
        public string User { get => user; }
        public string Server { get => server; }
        public string Password { get => pwrd; }
        public string Instance { get => instance; }

        public SqlServerCredentials(string connectionString)
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(connectionString);
            string[] tokens = csb.DataSource.Split('\\');
            int tokenCount = tokens.Length;
            if (tokenCount == 2)
            {
                server = tokens[0];
                instance = tokens[1];
            }
            else if (tokenCount == 1)
            {
                server = tokens[0];
            }

            user = csb.UserID;
            pwrd = csb.Password;
        }
    }
}
