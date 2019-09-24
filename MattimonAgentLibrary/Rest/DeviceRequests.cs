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
    public class DeviceRequests : IDisposable
    {
        private HttpClient client = new HttpClient();

        class HttpBaseProtocolFitler
        {
            public bool AllowUI { get; set; }
        }
        public DeviceRequests()
        {
            client = new HttpClient();


            client = new HttpClient
            {
                //client.BaseAddress = new Uri(Constants.DevMattimonWebApplicationBaseURL);
                BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL())
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task<Device> CreateDeviceEntryAsync(Device device)
        {
            Device d = null;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    Constants.DEVICE_DIR, device);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    d = await response.Content.ReadAsAsync<Device>();
                }
            }
            catch (HttpRequestException httprex)
            {
                return new Device { HttpRequestException = httprex };
            }

            return d;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<RequestResult> GetDeviceAsync(long id)
        {
            RequestResult requestObject = new RequestResult();
            String path = Constants.DEVICE_DIR + "/" + id;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                requestObject = await response.Content.ReadAsAsync<RequestResult>();
            }
            return requestObject;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task<Device> UpdateDeviceAsync(Device device)
        {
            try
            {
                HttpResponseMessage response = await client.PutAsJsonAsync(
                    Constants.DEVICE_DIR + $"/{device.Device_Id}", device);

                response.EnsureSuccessStatusCode();

                // Deserialize the updated product from the response body.
                device = await response.Content.ReadAsAsync<Device>();
                return device;
            }
            catch (HttpRequestException ex)
            {
                device.HttpRequestException = ex;
                return device;
            }
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
        /// <param name="parameters">Parapeters should be splitted by a ';'. Integers only.</param>
        /// <returns></returns>
        private async Task<DeviceOptions> GetDeviceOptionsAsync(String parameters)
        {
            Device device = new Device();
            String path = Constants.DEVICE_OPTIONS_DIR + "/" + parameters;
            HttpResponseMessage response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                device = await response.Content.ReadAsAsync<Device>();
            }

            return new DeviceOptions
            {
                UseAgent = device.UseAgent,
                NotificationEmails = device.NotificationEmails,
                NotifyHealth = device.NotifyHealth,
                NotifyStatus = device.NotifyStatus,
                MonitoringPort = device.Port,
                MonitorEventLog = device.MonitorEventLog,
                MonitorSql = device.MonitorSql > 0,
                ReportingInterval = device.AgentReportInterval,
                HttpRequestException = device.HttpRequestException,
                MySqlExceptionMessage = device.MySqlExceptionMessage,
                Exception = device.Exception,
                RequestSuccess = device.RequestSuccess
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task<Device> PostDeviceOptionsAsync(Device device)
        {
            Device returnDevice = new Device();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                Constants.DEVICE_OPTIONS_DIR, device);


            if (response.IsSuccessStatusCode)
            {
                returnDevice = await response.Content.ReadAsAsync<Device>();
            }

            returnDevice.HttpStatusCode = response.StatusCode;
            return returnDevice;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDeviceEntry"></param>
        /// <returns></returns>
        public Device CreateDeviceEntry(Device newDeviceEntry)
        {
            Device returnDevice = CreateDeviceEntryAsync(newDeviceEntry).Result;
            return returnDevice;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public Device UpdateDeviceEntry(Device device)
        {
            bool done = false; int passes = 0;
            while (!done)
            {
                try
                {
                    Device d = UpdateDeviceAsync(device).Result;
                    done = true;
                    return d;
                }
                catch (TaskCanceledException ex)
                {
                    passes++;
                    if (passes == 3)
                    {
                        device.TaskCanceledException = ex;
                        break;
                    }
                    System.Threading.Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    passes++;
                    if (passes == 3)
                    {
                        device.Exception = ex;
                        break;
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }
            return device;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myId"></param>
        /// <returns></returns>
        public Boolean DeviceEntryExist(long myId)
        {
            RequestResult requestObject = GetDeviceAsync(myId).Result;

            if (requestObject.Exception != null)
                throw requestObject.Exception;

            if (requestObject.MySqlExceptionMessage != null)
                throw new Exception(requestObject.MySqlExceptionMessage);

            return Convert.ToInt64(requestObject.Result) > 0;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myId"></param>
        /// <returns></returns>
        public RequestResult GetDevice(long myId)
        {


            return GetDeviceAsync(myId).Result;

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters">Parapeters should be splitted by a ';'. Integers only.</param>
        /// <returns></returns>
        public DeviceOptions GetDeviceOptions(String parameters)
        {
            return GetDeviceOptionsAsync(parameters).Result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public DeviceOptions PostDeviceOptions(Device device)
        {
            device = PostDeviceOptionsAsync(device).Result;
            return new DeviceOptions
            {
                UseAgent = device.UseAgent,
                NotificationEmails = device.NotificationEmails,
                MonitoringPort = device.Port,
                Exception = device.Exception,
                HttpRequestException = device.HttpRequestException,
                MySqlExceptionMessage = device.MySqlExceptionMessage,
                HttpStatusCode = device.HttpStatusCode,
                RequestSuccess = device.RequestSuccess,
                ReportingInterval = device.AgentReportInterval
            };
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
