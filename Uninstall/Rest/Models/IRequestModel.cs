using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Rest.Models
{
    public interface IRequestModel
    {
        Exception Exception { get; set; }
        String MySqlExceptionMessage { get; set; }
        HttpRequestException HttpRequestException { get; set; }
        String Tag { get; set; }
        Boolean RequestSuccess { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
    }
}
