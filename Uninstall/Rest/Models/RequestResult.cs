using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Rest.Models
{
    public class RequestResult
    {
        public String HttpVerb { get; set; }
        public String HttpParameters { get; set; }
        public object Result { get; set; }
        public Boolean Success { get; set; }
        public Exception Exception { get; set; }
        public String MySqlExceptionMessage { get; set; }
        public HttpRequestException HttpRequestException { get; set; }
    }
}
