using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using MattimonAgentLibrary.Static;
using System.Data;

namespace MattimonSQLite
{
    public class SQLiteDatabaseController : IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        private static class Connection
        {

            private static SQLiteConnection dbConnection;
            private static String dbfilename;
            public static String DbFileName { get { return dbfilename; } set { dbfilename = value; } }
            private static readonly String dbversion = "3";

            /// <summary>
            /// 
            /// </summary>
            public static bool CreateDatabaseFile(String rootDir, String baseDir, String dir, String file,
                out String errMessage, Boolean replace=false)
            {
                errMessage = "";

                String path = rootDir + @"\" + baseDir + @"\" + dir;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += @"\" + file;
                dbfilename = path;

                if (!replace)
                {
                    if (File.Exists(path))
                    {
                        errMessage = "Database file " + path + " already exists";
                        return false;
                    }
                    else
                    {
                        try
                        {
                            File.Delete(path);
                            SQLiteConnection.CreateFile(path);
                            errMessage = null;
                            return true;
                        }
                        catch (Exception ex)
                        {
                            errMessage = ex.Message + "\n\n" + ex.StackTrace;
                            return false;
                        }
                    }
                }
                else
                {
                    try
                    {
                        //if (Connection.GetConnectionState() != ConnectionState.Closed)
                        //{
                        //    Connection.CloseConnection(out errMessage);
                        //    Connection.DeleteDatabaseIfExists(out errMessage);
                        //}

                        //if (errMessage != null)
                        //    throw new Exception(errMessage);

                        SQLiteConnection.CreateFile(dbfilename);
                        errMessage = null;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        errMessage = ex.Message + "\n\n" + ex.StackTrace;
                        return false;
                    }
                }
            }





            /// <summary>
            /// 
            /// </summary>
            /// <param name="errMessage"></param>
            /// <returns></returns>
            public static bool CreateDatabaseFileIfNotExists(String rootDir, String baseDir, String dir, String file, out String errMessage)
            {
                String path = rootDir + @"\" + baseDir + @"\" + dir;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += @"\" + file;
                dbfilename = path;

                try
                {
                    errMessage = null;
                    if (File.Exists(path)) { return false; }

                    SQLiteConnection.CreateFile(path);
                    dbfilename = path;
                    return true;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message + "\n\n" + ex.StackTrace;
                    return false;
                }
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="errMessage"></param>
            /// <returns></returns>
            public static bool DeleteDatabase(out String errMessage)
            {
                if (dbfilename == null)
                {
                    errMessage = "Database filename is not initialized";
                    return false;
                }

                if (!File.Exists(dbfilename))
                {
                    errMessage = "Database file " + dbfilename + " does not exist";
                    return false;
                }

                if (dbConnection != null)
                {
                    if (dbConnection.State != ConnectionState.Closed)
                    {
                        errMessage = "Database file cannot be deleted because the connection is open";
                        return false;
                    }
                }

                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.Delete(dbfilename);
                    errMessage = null;
                    return true;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message + "\n\n" + ex.StackTrace;
                    return false;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="errMessage"></param>
            /// <returns></returns>
            public static bool DeleteDatabaseIfExists(out String errMessage)
            {
                if (dbfilename == null)
                {
                    errMessage = "Database filename is not initialized";
                    return false;
                }

                errMessage = null;
                if (File.Exists(dbfilename))
                {
                    return DeleteDatabase(out errMessage);
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static bool DatabaseFileExists(out String fullname)
            {
                if (dbfilename == "")
                    throw new Exception("[SQLiteDatabaseController->Connection->DatabaseFileExists]\n" +
                        "Database filename is not initialized");

                fullname = dbfilename;
                return File.Exists(dbfilename);
            }

            /// <summary>
            /// Creates the <code>SQLiteConnection</code> object.
            /// </summary>
            public static void Initialize()
            {
                if (dbfilename == null)
                {
                    throw new Exception("Database filename is not initialized");
                }

                dbConnection = new SQLiteConnection();
                dbConnection = new SQLiteConnection(String.Format("Data Source={0};Version={1};",
                    dbfilename, dbversion));
            }




            /// <summary>
            /// Initializes SQLiteConnection object if its null and
            /// attempts to establish connection.
            /// </summary>
            /// <param name="errMessage"></param>
            /// <returns></returns>
            public static bool OpenConnection(out String errMessage)
            {
                errMessage = null;
                if (dbConnection == null)
                {
                    Initialize();
                }

                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();

                    DateTime timeout = DateTime.Now.AddSeconds(5);

                    while (dbConnection.State != ConnectionState.Closed)
                    {
                        if (DateTime.Now > timeout)
                        {
                            errMessage = "Connection was already open but look more that 5 seconds to close";
                            break;
                        }
                    }
                }

                if (errMessage != null)
                {
                    return false;
                }

                try
                {
                    dbConnection.Open();
                    errMessage = null;
                    return true;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message + "\n\n" + ex.StackTrace;
                    return false;
                }
            }




            /// <summary>
            /// 
            /// </summary>
            /// <param name="errMessage"></param>
            /// <returns></returns>
            public static bool CloseConnection(out String errMessage)
            {
                if (dbConnection == null)
                {
                    errMessage = "SQLiteConnection is not defined";
                    return false;
                }

                if (dbConnection.State == ConnectionState.Closed)
                {
                    errMessage = "SQLiteConnection is already closed";
                    return false;
                }

                try
                {
                    dbConnection.Close();
                    SQLiteConnection.ClearAllPools();
                    errMessage = null;
                    return true;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message + "\n\n" + ex.ToString();
                    return false;
                }
            }




            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static ConnectionState GetConnectionState()
            {
                if (dbConnection == null)
                    throw new Exception("SQLiteConnection is not defined");
                else
                    return dbConnection.State;
            }


            public static SQLiteConnection GetSQLiteConnection()
            {
                return dbConnection;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static SQLiteCommand CreateCommand(out String errMessage)
            {
                if (dbConnection == null)
                {
                    errMessage = "SQLiteConnection is not defined";
                    return null;
                }

                if (dbConnection.State != ConnectionState.Open)
                {
                    errMessage = "SQLiteConnection is not open";
                    return null;
                }

                errMessage = null;
                return dbConnection.CreateCommand();
            }


            public static void DisposeConnection()
            {
                if (dbConnection != null)
                {
                    if (dbConnection.State != ConnectionState.Closed)
                        try
                        {
                            dbConnection.Clone();
                        } catch { }

                    dbConnection.Dispose();
                }
            }

        } // END static class Connection




        /// <summary>
        /// SQLiteDatabaseController main constructor.
        /// Initializes a <code>SQLiteConnection</code> object.
        /// </summary>
        public SQLiteDatabaseController(String rootDir, String baseDir, String dir, String file,
            Boolean replaceDbFile = false)
        {
            Initialize(rootDir, baseDir, dir, file,
                replaceDbFile);
        }



        /// <summary>
        /// Creates a <code>SQLiteConnection object</code>
        /// Note: To open the connection, use <code>OpenConnection()</code>
        /// </summary>
        private void Initialize(String rootDir, String baseDir, String dir, String file,
            Boolean replaceDbFile = false)
        {
            if (replaceDbFile == false)
                CreateDatabaseFileIfNotExists(rootDir, baseDir, dir, file);
            else
                CreateDatabaseFile(rootDir, baseDir, dir, file, replaceDbFile);

            Connection.Initialize();
        }


        public Boolean DatabaseFileExists(out String fullname)
        {
            return Connection.DatabaseFileExists(out fullname);
        }


        /// <summary>
        /// Creates the database file in the application's root folder.
        /// </summary>
        private void CreateDatabaseFile(String rootDir, String baseDir, String dir, String file, Boolean replace)
        {
            bool created = Connection.CreateDatabaseFile(rootDir, baseDir, dir, file, out string errMessage, replace);
            if (errMessage != null && !created) { throw new Exception(errMessage); }
        }


        /// <summary>
        /// Create the database file if the file does not exist already.
        /// Note: An <code>Exception</code> is thrown if the operation fails.
        /// </summary>
        private void CreateDatabaseFileIfNotExists(String rootDir, String baseDir, String dir, String file)
        {
            Connection.CreateDatabaseFileIfNotExists(rootDir, baseDir, dir, file, out string errMessage);

            if (errMessage != null) { throw new Exception(errMessage); }
        }




        /// <summary>
        /// Delete the database file from the applications's root folder.
        /// </summary>
        public bool DeleteDatabaseFile()
        {
            bool deleted = Connection.DeleteDatabase(out string errMessage);
            if (errMessage != null && !deleted) { throw new Exception(errMessage); }
            return deleted;
        }


        /// <summary>
        /// Delete the database file if it exists
        /// Note: Throws an <code>Exception</code> if the database file is in use.
        /// </summary>
        /// <returns></returns>
        public bool DeleteDatabaseFileIfExists()
        {
            bool deleted = Connection.DeleteDatabaseIfExists(out string errMessage);
            if (errMessage != null)
                throw (new Exception(errMessage));
            return deleted;
        }




        /// <summary>
        /// Create a SQLiteConnection object and attempts to connect.
        /// </summary>
        public bool OpenConnection()
        {
            if (Connection.GetSQLiteConnection() == null)
                Connection.Initialize();

            if (GetConnectionState() == ConnectionState.Closed)
            {
                Boolean connected = Connection.OpenConnection(out string errMessage);
                if (errMessage != null)
                    throw (new Exception(errMessage));
                return connected;
            }
            return true;
        }


        /// <summary>
        /// Attempts to close the connection.
        /// NOTE: The SQLiteConnection object is disposed if success.
        /// Use <code>OpenConnection()</code> to create a SQLiteConnection object and attempt connection
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
        {
            try
            {
                Boolean disconnected = Connection.CloseConnection(out string errMessage);
                SQLiteConnection.ClearAllPools();
                if (errMessage != null)
                    throw (new Exception(errMessage));
                return disconnected;
            }
            catch
            { return true; }
        }


        /// <summary>
        /// Closes, Cancels, Disposes database Connection,
        /// Calls GC.Collect and GC.WaitForPendingFinalizers
        /// </summary>
        public void CloseConnection_GC()
        {
            if (Connection.GetSQLiteConnection() != null)
            {
                if (Connection.GetConnectionState() != ConnectionState.Closed)
                {
                    Connection.GetSQLiteConnection().Close();
                    Connection.GetSQLiteConnection().Cancel();
                    Connection.GetSQLiteConnection().Dispose();
                    SQLiteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        public void Clean()
        {
            SQLiteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns>Retrusn a <code>ConnectionState</code> representing the current SQLiteConnection's state.
        /// Throws <code>Exception</code> if SQLiteConnection is null</returns>
        public ConnectionState GetConnectionState()
        {
            return Connection.GetConnectionState();
        }




        /// <summary>
        /// Creates a SQL update statement
        /// Note: fields and values count must match, otherwise an Exception will be thrown
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// <param name="fields">Field names</param>
        /// <param name="values">Values</param>
        /// <param name="sql">SQL Conditions (such a WHERE clause etc.)</param>
        /// <returns>SQL statement</returns>
        private String CreateUpdateStatement(String table, String[] fields, object[] values, String sql)
        {
            if (fields.Length != values.Length)
            {
                throw new Exception("Field and values count mismatch");
            }

            String query = "UPDATE " + table + " SET ";
            for (int i = 0; i <= fields.Length - 1; i++)
            {
                query += fields[i] + " = ";

                if (values[i] is int ||
                    values[i] is double ||
                    values[i] is float)
                {
                    query += values[i];
                }
                else if (values[i] is bool)
                {
                    query += (Convert.ToBoolean(values[i]) ? "1" : "0");
                }
                else
                {
                    query += "'" + values[i] + "'";
                }

                if (i != fields.Length - 1)
                {
                    query += ", ";
                }
            }
            query += " " + sql;

            return query;
        }


        /// <summary>
        /// Create a SQL insert statement
        /// Note: fields and values count must match, otherwise an Exception will be thrown
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// <param name="fields">Field names</param>
        /// <param name="values">Values</param>
        /// <returns>SQL statement</returns>
        private String CreateInsertStatement(String table, String[] fields, object[] values)
        {
            if (fields.Length != values.Length)
            {
                throw new Exception("Field and values count mismatch");
            }

            // INSERT INTO table (fields) VALUES (values)
            String query = "INSERT INTO " + table + " (";
            for (int i = 0; i < fields.Length; i++)
            {
                query += fields[i];

                if (i != fields.Length - 1)
                {
                    query += ", ";
                }
                else
                {
                    query += ") VALUES (";
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is int ||
                    values[i] is double ||
                    values[i] is float)
                {
                    query += values[i];
                }
                else if (values[i] is bool)
                {
                    query += (Convert.ToBoolean(values[i]) ? "1" : "0");
                }
                else
                {
                    query += "'" + values[i] + "'";
                }

                if (i != fields.Length - 1)
                {
                    query += ", ";
                }
            }

            query += ") ";

            return query;
        }





        /// <summary>
        /// Create a SQL SELECT statement
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// <param name="fields">Fields to select from the table</param>
        /// <param name="sql">Extra SQL statements such as WHERE clause, ORDER BY etc.</param>
        /// <returns>SQL statement</returns>
        private String CreateSelectStatement(String table, String[] fields = null, String sql = "")
        {
            String query = "SELECT ";
            int i = 0;

            if (fields != null)
            {
                foreach (String field in fields)
                {
                    query += field;


                    if (i != fields.Length - 1)
                        query += ", ";

                    i++;
                }
            }
            else
            {
                query += " * ";
            }

            return query += " FROM " + table + " " + sql;
        }

        /// <summary>
        /// Create a SQL create table statament
        /// Note: fields and types count must match, otherwise an Exception will be thrown
        /// </summary>
        /// <param name="tblName">Name of the table to create</param>
        /// <param name="fieldNames">Field names</param>
        /// <param name="sqlFieldTypes">SQL data types</param>
        /// <param name="primaryKeyField">Primary key</param>
        /// <param name="autoIncrement">Primary key is auto increment (INT only)</param>
        /// <returns>SQL statement</returns>
        private String GetCreateTableStatement(String tblName, String[] fieldNames, String[] sqlFieldTypes, String primaryKeyField, Boolean autoIncrementPrimaryKey)
        {
            if (fieldNames.Length != sqlFieldTypes.Length)
                throw new Exception("Field and types count mismatch");

            if (!fieldNames.Contains(primaryKeyField) && (primaryKeyField != null && primaryKeyField != ""))
                throw new Exception("Primary key " + primaryKeyField + " is not listed in field names");

            String query = "CREATE TABLE '" + tblName + "' (";

            for (int i = 0; i < sqlFieldTypes.Length; i++)
            {

                if (fieldNames[i].Equals(primaryKeyField) && (primaryKeyField != null && primaryKeyField != ""))
                {
                    if (autoIncrementPrimaryKey && (!sqlFieldTypes[i].ToUpper().Equals("INTEGER PRIMARY KEY AUTOINCREMENT")))
                    {
                        throw new Exception("AUTOINCREMENT can only by applied to INTEGER PRIMARY KEY AUTOINCREMENT. " + sqlFieldTypes[i] + " was given.");
                    }
                }

                query += fieldNames[i] + " " + sqlFieldTypes[i];
                

                if (i != fieldNames.Length - 1)
                {
                    query += ", ";
                }
            }
            return query + ");";
        }





        /// <summary>
        /// Counts all rows from the specified table based on this query.
        /// </summary>
        /// <param name="tblName">The table's name</param>
        /// <param name="sql">SQL Conditions (such a WHERE clause etc.)</param>
        /// <returns>Number of rows based on this query</returns>
        public long QueryCountRows(String tblName, String sql = "")
        {
            string query = String.Format("SELECT COUNT(*) FROM {0} {1}", tblName, sql);

            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);
            if (errMessage != null && sQLiteCommand == null)
            {
                sQLiteCommand.Dispose();
                CloseConnection();
                throw new Exception(errMessage);
            }

            sQLiteCommand.CommandText = query;
            long count = Convert.ToInt64(sQLiteCommand.ExecuteScalar());
            sQLiteCommand.ExecuteNonQuery();
            sQLiteCommand.Dispose();
            return count;
        }





        /// <summary>
        /// Executes an update statement
        /// Note: fieldNames and values count must match, otherwise an Exception will be thrown
        /// </summary>
        /// <param name="tblName">The name of the table</param>
        /// <param name="fieldNames">The field names</param>
        /// <param name="values">The values</param>
        /// <param name="sql">SQL Conditions (such a WHERE clause etc.)</param>
        /// <returns>Number of rows affected by this statement / query</returns>
        public int Update(string tblName, string[] fieldNames, object[] values, string sql = "")
        {
            OpenConnection();
            string query = CreateUpdateStatement(tblName, fieldNames, values, sql);


            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
            {
                try
                {
                    sQLiteCommand.Dispose();
                }
                catch { }
                CloseConnection();
                throw (new Exception(errMessage));
            }


            sQLiteCommand.CommandText = query;
            int rows = sQLiteCommand.ExecuteNonQuery();
            sQLiteCommand.ExecuteNonQuery();
            sQLiteCommand.Dispose();
            return rows;
        }





        /// <summary>
        /// Deletes rows from the specified table based on this query
        /// </summary>
        /// <param name="tblName">The name of the table</param>
        /// <param name="sql">SQL Conditions (such a WHERE clause etc.)</param>
        /// <returns>Number of rows affected by this statement / query</returns>
        public int Delete(String tblName, String sql = "")
        {
            String query = String.Format("DELETE FROM {0} {1}", tblName, sql);

            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
            {
                sQLiteCommand.Dispose();
                CloseConnection();
                throw (new Exception(errMessage));
            }

            sQLiteCommand.CommandText = query;
            int rows = sQLiteCommand.ExecuteNonQuery();
            sQLiteCommand.Dispose();
            return rows;
        }





        /// <summary>
        /// Insert a row in the specified table based on this query
        /// </summary>
        /// <param name="tblName"><The name of the table/param>
        /// <param name="fieldNames">The field names</param>
        /// <param name="values">The values</param>
        /// <returns>Generated Insert ID (usefull only is the table has auto-increment primary key)</returns>
        public long Insert(String tblName, String[] fieldNames, Object[] values)
        {
            String query = CreateInsertStatement(tblName, fieldNames, values);

            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
                throw (new Exception(errMessage));

            sQLiteCommand.CommandText = query;
            // Execute the insert statement
            sQLiteCommand.ExecuteNonQuery();

            sQLiteCommand.CommandText = "SELECT last_insert_rowid()";
            long insertId = Convert.ToInt64(sQLiteCommand.ExecuteScalar());

            sQLiteCommand.Dispose();

            return insertId;
        }





        /// <summary>
        /// Selects specified fields from the specified table
        /// </summary>
        /// <param name="tblName">The name of the table</param>
        /// <param name="fieldNames">
        /// The field names to select. 
        /// Note: to return all the fields, set to <code>null</code></param>
        /// <param name="sql"></param>
        /// <returns>
        /// An object if only one field is specified. 
        /// Otherwise, a <code>System.Data.DataTable</code> is returned
        /// </returns>
        public object Select(String tblName, String[] fieldNames = null, String sql = "")
        {
            String query = CreateSelectStatement(tblName, fieldNames, sql);
            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
            {
                sQLiteCommand.Dispose();
                CloseConnection();
                throw (new Exception(errMessage));
            }

            sQLiteCommand.CommandText = query;

            // A simple object should be returned
            if (fieldNames != null && fieldNames.Length == 1)
            {
                object value = sQLiteCommand.ExecuteScalar();
                sQLiteCommand.Dispose();
                return value;
            }

            // Or, return a System.Data.DataTable
            DataTable dataTable = new DataTable();
            SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter(sQLiteCommand);
            sQLiteDataAdapter.Fill(dataTable);

            sQLiteDataAdapter.Dispose();
            sQLiteCommand.Dispose();

            if (dataTable.Rows.Count > 0)
                return dataTable;
            return null;
        }





        /// <summary>
        /// Create a SQL create table statament
        /// Note: fields and types count must match, otherwise an Exception will be thrown
        /// </summary>
        /// <param name="tblName">Name of the table to create</param>
        /// <param name="fieldNames">Field names</param>
        /// <param name="sqlFieldTypes">SQL data types</param>
        /// <param name="primaryKeyField">Primary key</param>
        /// <param name="autoIncrement">Primary key is auto increment (INT only)</param>
        /// <returns>Returns <code>true</code>if the command was succesfull. An exception is thrown is not.</returns>
        public bool CreateTable(String table, String[] fields, String[] types, String primaryKey, Boolean autoIncrement)
        {
            string query = GetCreateTableStatement(table, fields, types, primaryKey, autoIncrement);

            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
            {
                sQLiteCommand.Dispose();
                CloseConnection();
                throw (new Exception(errMessage));
            }

            sQLiteCommand.CommandText = query;
            sQLiteCommand.ExecuteNonQuery();

            sQLiteCommand.Dispose();
            return true;
        }

        /// <summary>
        /// Checks if the given table exists
        /// </summary>
        /// <param name="tblName">The name of the table</param>
        /// <returns></returns>
        public Boolean TableExist(string tblName)
        {
            string query = string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';", tblName);

            SQLiteCommand sQLiteCommand = Connection.CreateCommand(out string errMessage);

            if (errMessage != null && sQLiteCommand == null)
            {
                sQLiteCommand.Dispose();
                CloseConnection();
                throw (new Exception(errMessage));
            }

            sQLiteCommand.CommandText = query;
            Boolean exists = Convert.ToString(sQLiteCommand.ExecuteScalar()) != "";

            sQLiteCommand.Dispose();

            return exists;
        }


        public String GetDatabaseFullName()
        {
            return Connection.DbFileName;
        }

        

        public void Dispose()
        {
            Connection.DisposeConnection();
            Clean();
        }
    }




    public static class Common
    {
        /// <summary>
        /// Disposes a TableAdapter generated by SQLite Designer
        /// </summary>
        /// <param name="disposing"></param>
        /// <param name="adapter"></param>
        /// <param name="commandCollection"></param>
        /// <remarks>You must dispose all the command,
        /// otherwise the file remains locked and cannot be accessed
        /// (for example, for reading or deletion)</remarks>
        public static void DisposeTableAdapter(
            bool disposing,
            System.Data.SQLite.SQLiteDataAdapter adapter,
            IEnumerable<System.Data.SQLite.SQLiteCommand> commandCollection)
        {
            if (disposing)
            {
                DisposeSQLiteTableAdapter(adapter);

                foreach (SQLiteCommand currentCommand in commandCollection)
                {
                    currentCommand.Dispose();
                }
            }
        }

        public static void DisposeSQLiteTableAdapter(SQLiteDataAdapter adapter)
        {
            if (adapter != null)
            {
                DisposeSQLiteTableAdapterCommands(adapter);

                adapter.Dispose();
            }
        }

        public static void DisposeSQLiteTableAdapterCommands(SQLiteDataAdapter adapter)
        {
            SQLiteCommand[] commands = new SQLiteCommand[]
            {
                    adapter.DeleteCommand,
                    adapter.InsertCommand,
                    adapter.SelectCommand,
                    adapter.UpdateCommand
            };
            foreach (SQLiteCommand currentCommand in commands)
            {
                if (currentCommand != null)
                    currentCommand.Dispose();
            }
        }
    }
}
