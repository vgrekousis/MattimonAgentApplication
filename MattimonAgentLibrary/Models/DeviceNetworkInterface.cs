using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class DeviceNetworkInterface
    {
        public string Name { get; set; }
        public string NetConnectionID { get; set; }
        public string ServiceName { get; set; }
        public bool Enabled { get; set; }

        public double BytesSentPerSec { get; set; }
        public double BytesReceivedPerSec { get; set; }
        /// <summary>
        /// Represent the sum of <code>BytesSentPerSec</code> + <code>BytesReceivedPerSc</code>
        /// </summary>
        public double TotalBytesPerSec { get; set; }
        /// <summary>
        /// Should carry the value of <code>TotalBytesPerSec</code> converted to kilobytes.
        /// </summary>
        public double KiloBytesPerSec { get; set; }
        public double CurrentBandwidth { get; set; }
        public double Utilization { get; set; }
    }
}
