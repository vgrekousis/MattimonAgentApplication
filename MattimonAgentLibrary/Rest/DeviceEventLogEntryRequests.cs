using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MattimonAgentLibrary.Static;
using MattimonAgentLibrary.Models;

namespace MattimonAgentLibrary.Rest
{
    public class DeviceEventLogEntryRequests
    {
        private HttpClient client = new HttpClient();

        public DeviceEventLogEntryRequests()
        {
            //client.BaseAddress = new Uri(Constants.DevMattimonWebApplicationBaseURL);
            client.BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<DeviceEventLogEntry> PostDeviceEventLogEntryAsync(DeviceEventLogEntry logEntry)
        {
            Models.DeviceEventLogEntry e = null;
            try
            {

                HttpResponseMessage response = await client.PostAsJsonAsync(
                    Constants.EVTLOGS_DIR, logEntry);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    e = await response.Content.ReadAsAsync<DeviceEventLogEntry>();
                }
            }
            catch (HttpRequestException httprex)
            {
                return new DeviceEventLogEntry { HttpRequestException = httprex };
            }

            return e;
        }
        public DeviceEventLogEntry PostDeviceLongEntry(DeviceEventLogEntry logEntry)
        {
            return PostDeviceEventLogEntryAsync(logEntry).Result;
        }
    }
}
