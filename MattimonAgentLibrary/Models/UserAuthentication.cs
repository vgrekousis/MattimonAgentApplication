using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Models
{
    public class UserAuthentication
    {
        public long User_Id { get; set; }
        public long Company_Id { get; set; }
        public string User_email { get; set; }
        public string User_Agent_ID { get; set; }
        public string Company_Name { get; set; }
        public bool Auth_Ok { get; set; }

        public string MySqlExceptionError { get; set; }
        public int MySqlExceptionErrno { get; set; }
        public Exception Exception { get; set; }
        public HttpRequestException HttpRequestException { get; set; }
        public WebException WebException { get; set; }
    }
}
