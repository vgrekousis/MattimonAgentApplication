using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class APIInfo
    {
        public String ApplicationName { get; set; }
        public String ApplicationVersion { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
