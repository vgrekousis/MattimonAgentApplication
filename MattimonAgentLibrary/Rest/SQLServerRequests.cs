using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MattimonAgentLibrary.Models.SQL;

namespace MattimonAgentLibrary.Rest
{
    public class SQLServerRequests : IDisposable
    {
        private HttpClient client = new HttpClient();

        class HttpBaseProtocolFitler
        {
            public bool AllowUI { get; set; }
        }

        public SQLServerRequests()
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL())
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<DeviceServerObjects> PostDeviceServerObjectsAsync(DeviceServerObjects serverObjects)
        {
            DeviceServerObjects dso = null;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    Constants.SQLSRV_DIR, serverObjects);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    dso = await response.Content.ReadAsAsync<DeviceServerObjects>();
                }
            }
            catch (HttpRequestException httprex)
            {
                return new DeviceServerObjects { HttpRequestException = httprex };
            }

            return dso;
        }
        public DeviceServerObjects PostDeviceServerObjects(DeviceServerObjects serverObjects, DeviceServerObjectAction action)
        {
            serverObjects.PostAction = action.ToString();
            return PostDeviceServerObjectsAsync(serverObjects).Result;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    public enum DeviceServerObjectAction
    {
        select,
        insert,
        update,
        delete
    }
}
