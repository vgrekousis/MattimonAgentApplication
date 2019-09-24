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
    public class AuthenticationRequest
    {
        /// <summary>
        /// 
        /// </summary>
        private HttpClient client = new HttpClient();
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationRequest()
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
        /// <param name="ua"></param>
        /// <returns></returns>
        private async Task<UserAuthentication> AuthenticateAsync(UserAuthentication ua)
        {
            UserAuthentication auth = null;
            try
            {

                HttpResponseMessage response = await client.PostAsJsonAsync(
                    Constants.AUTHENTICATION_DIR, ua);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    auth = await response.Content.ReadAsAsync<UserAuthentication>();
                }
            }
            catch (HttpRequestException httprex)
            {
                return new UserAuthentication { HttpRequestException = httprex };
            }

            return auth;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ua"></param>
        /// <returns></returns>
        public UserAuthentication GetUserAuthentication(UserAuthentication ua)
        {
            return AuthenticateAsync(ua).Result;
        }
    }
}
