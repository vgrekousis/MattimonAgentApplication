using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    /// <summary>
	/// Primary
	/// Mattimon Table: device_sqlsrv_instances
	/// </summary>
	public class ServerInstance
    {
        /// <summary>
        /// Mattimon Database Primary Key
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public bool Monitored { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string InstanceName { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public bool Clustered { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Entries to Dispatch to Mattimon Table: device_sqlsrv_databases
        /// </summary>
        public Database[] Databases { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public double LastReportedTimestamp { get; set; }
        /// <summary>
        /// Local field. This field isn't reported to the web api.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
		/// Field
		/// </summary>
		public int ConnectStatus { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        public SQLService SQLService { get; set; }
    }
}
