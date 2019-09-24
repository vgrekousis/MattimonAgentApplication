using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace MattimonAgentService.Rest
{
    public class RestClientRequests
    {
        /// <summary>
        /// Gets the version string from mattimon agent php file
        /// </summary>
        /// <returns></returns>
        public String RequestVersion()
        {
            RestClient restClient = new RestClient(MattimonAgentLibrary.Rest.Constants.GetActiveWebAppUpdaterURL());
            restClient.AddDefaultHeader("X-Requested-With", "Mattimon-Web-Client");
            RestSharp.RestRequest request = new RestRequest(Method.GET);
            request.AddParameter(new Parameter { Name = "version", Value = "", Type = ParameterType.GetOrPost });
            return restClient.Execute(request).Content;
        }
        /// <summary>
        /// Gets the filesize of the mattimon agent zip file from the php file
        /// </summary>
        /// <returns></returns>
        public Int64 RequestFilesize()
        {
            RestClient restClient = new RestClient(MattimonAgentLibrary.Rest.Constants.GetActiveWebAppUpdaterURL());
            restClient.AddDefaultHeader("X-Requested-With", "Mattimon-Web-Client");
            RestSharp.RestRequest request = new RestRequest(Method.GET);
            request.AddParameter(new Parameter { Name = "filesize", Value = "", Type = ParameterType.GetOrPost });
            return Convert.ToInt64(restClient.Execute(request).Content);
        }
        /// <summary>
        /// Downloads the bytes from the mattimon agent php file
        /// </summary>
        /// <returns></returns>
        public byte[] GetZipBytes()
        {
            RestClient restClient = new RestClient(MattimonAgentLibrary.Rest.Constants.GetActiveWebAppUpdaterURL());
            restClient.AddDefaultHeader("X-Requested-With", "Mattimon-Web-Client");
            RestRequest request = new RestRequest();
            return restClient.DownloadData(request, true);
        }
    }
}
