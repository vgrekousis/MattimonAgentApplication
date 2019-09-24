using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class Disk
    {
        public string Drive { get; set; }
        public double Capacity { get; set; }
        public double Available { get; set; }
        public string Descrition { get; set; }
        public string VolumeName { get; set; }
    }
}
