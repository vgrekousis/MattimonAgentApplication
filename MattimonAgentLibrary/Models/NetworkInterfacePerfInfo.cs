using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class NetworkInterfacePerfInfo
    {
        public String NetworkCardName { get; set; }
        public String CIMName { get; set; }
        public String ServiceName { get; set; }
        public String AdapterType { get; set; }
        public String MACAddress { get; set; }
        public double Utilization { get; set; }
        public double BytesSent { get; set; }
        public double BytesReceived { get; set; }
        public double TotalBytes { get; set; }
        public double Bandwidth { get; set; }
        public Boolean NetEnabled { get; set; }
        public String ManagementExceptionMessage { get; set; }
    }
}
