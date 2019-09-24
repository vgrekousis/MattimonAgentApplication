using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class DeviceProcesses
    {
        public long DeviceId { get; set; }
        public DeviceProcess[] DeviceProcess { get; set; }
    }
}
