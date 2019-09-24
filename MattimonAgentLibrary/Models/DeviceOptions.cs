using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class DeviceOptions : IRequestModel
    {
        public Boolean UseAgent { get; set; }
        public int MonitorEventLog { get; set; }
        public Boolean MonitorSql { get; set; }
        public Boolean NotificationEmails { get; set; }
        public Boolean NotifyStatus { get; set; }
        public Boolean NotifyHealth { get; set; }

        public int MonitoringPort { get; set; }
        public double ReportingInterval { get; set; }

        public Exception Exception { get; set; }
        public String MySqlExceptionMessage { get; set; }
        public HttpRequestException HttpRequestException { get; set; }
        public String Tag { get; set; }
        public Boolean RequestSuccess { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
