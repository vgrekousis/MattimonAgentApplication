using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    public class DatabaseMetaData
    {
        public long DBSize { get; internal set; }
        public string MetricsDBSize { get; internal set; }

        public long DBMaxSize { get; internal set; }
        public string MetricsDBMaxSize { get; internal set; }

        public double LogSize { get; internal set; }
        public string MetricsLogSize { get; internal set; }

        public long LogMaxSize { get; internal set; }
        public string MetricsLogMaxSize { get; internal set; }

        public int LogFileSizePercentUsed { get; set; }
        public int LogFileSizePercentGrowth { get; set; }
        public int DbFileSizePercentGrowth { get; set; }
    }
}
