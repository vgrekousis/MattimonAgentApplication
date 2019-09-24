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
    public class UserInfosRequest : IDisposable
    {
        private HttpClient client = new HttpClient();
        public UserInfosRequest()
        {
            //client.BaseAddress = new Uri(Constants.DevMattimonWebApplicationBaseURL);
            client.BaseAddress = new Uri(Constants.GetActiveWebApplicationBaseURL());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<UserAuthentication> Get(long id)
        {
            UserAuthentication ua = new UserAuthentication();
            String path = Constants.API_USERINFO_DIR + "/" + id;
            HttpResponseMessage response = await client.GetAsync(path);


            try
            {
                if (response.IsSuccessStatusCode)
                {
                    ua = await response.Content.ReadAsAsync<UserAuthentication>();
                }
            }
            catch (HttpRequestException ex)
            {
                ua.HttpRequestException = ex;
            }

            return ua;
        }
        public UserAuthentication GetUserInfo(long userId)
        {
            return Get(userId).Result;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
