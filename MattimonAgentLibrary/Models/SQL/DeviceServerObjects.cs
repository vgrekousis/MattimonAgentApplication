using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    /// <summary>
	/// Disparcher Data Object
	/// </summary>
    public class DeviceServerObjects
    {
        /// <summary>
        /// Mattimon Database Foreign Key
        /// </summary>
        public long DeviceID { get; set; }
        /// <summary>
        /// Entries to Dispatch to Mattimon Table: device_sqlsrv_instances
        /// </summary>
        public ServerInstance[] Instances { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public HttpRequestException HttpRequestException { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String PostAction { get; set; }

    }
}
