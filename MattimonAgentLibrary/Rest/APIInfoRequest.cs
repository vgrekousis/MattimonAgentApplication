using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MattimonAgentLibrary.Models;

namespace MattimonAgentLibrary.Rest
{
    public class APIInfoRequest : IDisposable
    {
        private HttpClient client = new HttpClient();
        public APIInfoRequest()
        {
            //client.BaseAddress = new Uri(Constants.DevMattimonWebApplicationBaseURL);
            client.BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<APIInfo> GetApiInfoAsync()
        {
            APIInfo apiinfo = new APIInfo();
            String path = Constants.API_INFO_DIR;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                apiinfo = await response.Content.ReadAsAsync<APIInfo>();
            }
            apiinfo.HttpStatusCode = response.StatusCode;
            return apiinfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public APIInfo GetAPIInfo()
        {
            return this.GetApiInfoAsync().Result;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
