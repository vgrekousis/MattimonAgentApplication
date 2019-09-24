using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    /// <summary>
	/// Mattimon Table: device_sqlsrv_connections
	/// Parent Mattimon Table: device_sqlsrv_databases
	/// </summary>
	public class DatabaseConnectionInfo
    {
        /// <summary>
        /// Mattimon Database Primary Key
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// Mattimon Database Foreign Key
        /// </summary>
        public long DatabaseID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SPID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SPID2 { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string BlkBy { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public long CpuTime { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public long DiskIO { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string LastBatch { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// Entry
        /// </summary>
        public int RequestID { get; set; }
    }
}
