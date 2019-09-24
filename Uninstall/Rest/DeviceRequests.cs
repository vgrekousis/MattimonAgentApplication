using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Uninstall.Rest;
using Uninstall.Rest.Models;

namespace Uninstall.Rest
{
    public class DeviceRequests
    {
        private HttpClient client = new HttpClient();

        public DeviceRequests()
        {
            client.BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<RequestResult> DeleteDeviceAsync(long id)
        {
            HttpResponseMessage response = await client.DeleteAsync(
                Constants.DEVICE_DIR + $"/{id}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<RequestResult>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="myId"></param>
        /// <returns></returns>
        public RequestResult DeleteDevice(long myId)
        {
            return DeleteDeviceAsync(myId).Result;
        }
    }
}
