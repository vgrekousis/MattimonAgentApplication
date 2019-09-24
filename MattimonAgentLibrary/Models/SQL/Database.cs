using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    /// <summary>
	/// Mattimon Table: device_sqlsrv_databases
	/// Parent Mattimon Table: device_sqlsrv_instances
	/// </summary>
	public class Database
    {
        /// <summary>
        /// Mattimon Primary Key
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Mattimon Foreign Key
        /// </summary>
        public int ServerInstanceID { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public bool Monitored { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Field (DO NOT use as primery key on mattimon end).
        /// </summary>
        public long ClientDatabaseID { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string CollationName { get; set; }
        /// <summary>
		/// Field
		/// 0 = Online
		/// 1 = Restoring
		/// 2 = Recovering
		/// 3 = Recovery pending
		/// 4 = Suspect
		/// 5 = Emergency
		/// 6 = Offline
		/// </summary>
        public int State { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public int CreateDateTimestamp { get; set; }
        /// <summary>
        /// Entries to Dispatch to Mattimon Table: device_sqlsrv_connections
        /// </summary>
        public DatabaseConnectionInfo[] Connections { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public DatabaseMetaData MetaData { get; set; }
    }
}
