using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models.SQL
{
    public class SQLService
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsRunning { get; set; }
        public bool IsMainService { get; set; }
        /// <summary>
        /// Automatic: 2
        /// Boot: 0
        /// Disabled: 4
        /// Manual: 3
        /// System: 1
        /// </summary>
        public int StartType { get; set; }
    }
}
