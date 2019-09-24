using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace MattimonAgentLibrary.WMI
{
    public class PublicIPClient 
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly string mPrefix;

        /// <summary>
        /// 
        /// </summary>
        public PublicIPClient()
        {
            mPrefix = "https://monitoring.mattimon.com/pip/";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUrl"></param>
        public PublicIPClient(string serverUrl)
        {
            mPrefix = serverUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public string MyPublicIp(out Exception exception)
        {
            exception = null;
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("X-Requested-With", "XMLHttpRequest");
                return client.DownloadString(mPrefix);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
    }
}
