using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Diagnostics;
using System.Net.Http;
using System.Diagnostics;

namespace MattimonAgentLibrary.Models
{
    public class DeviceEventLogEntry
    {
        /// <summary>
        /// Mattimon (psm_devices.device_id)
        /// </summary>
        public long DeviceId { get; set; }

        public String EventLogName { get; set; }
        public long InstanceId { get; set; }
        public int EntryType { get; set; }
        /// <summary>
        /// Unix Timestamp
        /// </summary>
        public double TimeWritten { get; set; }
        /// <summary>
        /// Unix Timestamp
        /// </summary>
        public double TimeGenerated { get; set; }
        public int Index { get; set; }
        public String Message { get; set; }
        public String Category { get; set; }
        public String Source { get; set; }

        public Exception Exception { get; set; }
        public HttpRequestException HttpRequestException { get; set; }
        public String MySqlExceptionMessage { get; set; }
        public int MySqlInsertStatus { get; set; }
    }
}
