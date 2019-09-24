using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class DeviceProcess
    {
        public Int32 ProcessId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Boolean IsApplication { get; set; }
        public Double PrivateMemorySize { get; set; }
        public Double CpuUtil { get; set; }
        public Double DiskUtil { get; set; }
        public Double NetUtil { get; set; }
        public Double BytesSent { get; set; }
        public Double BytesReceived { get; set; }
        public Double TotalBytes { get; set; }
        public ManagementException[] ManagementExceptions { get; set; }
        public Exception[] Exceptions { get; set; }
    }
}
