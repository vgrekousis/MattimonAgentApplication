using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Net
{
    public class PingResult
    {
        private string host;
        private int port;
        private double rttms, rtts;
        private double latms, lats;

        /// <summary>
        /// Get the roundtrip time in milliseconds format
        /// Returns -1 if ping fails (Most lickely a PingException)
        /// </summary>
        public double RoundtripTimeMillis
        {
            get { return rttms; }
        }
        /// <summary>
        /// Get the roundtriptime in seconds format
        /// Returns -1 if ping fails (Most lickely a PingException)
        /// </summary>
        public double RoundtripTimeSeconds
        {
            get { return rtts; }
        }
        /// <summary>
        /// Get the tcp connect latency in milliseconds format
        /// Returns -1 if Tcp connection fails
        /// </summary>
        public double TcpLatencyMillis
        {
            get { return latms; }
        }
        /// <summary>
        /// Get the tcp connect larency in seconds format
        /// Returns -1 if Tcp connection fails
        /// </summary>
        public double TcpLatencySeconds
        {
            get { return lats; }
        }

        /// <summary>
        /// Holds the hostname
        /// </summary>
        public String Host
        {
            get { return host; }
        }
        /// <summary>
        /// Holds the port
        /// </summary>
        public int Port
        {
            get { return port; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public PingResult(String host, int port, out List<Exception> exceptions)
        {
            
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                exceptions = new List<Exception>();
                exceptions.Add(new Exception("Network is not available"));
                return;
            }
            this.host = host; this.port = port;
            exceptions = null;

            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingReply pingReply = null;
            Boolean pingsuccess = true;
            try { pingReply = ping.Send(host); }
            catch (Exception ex)
            {
                pingsuccess = false;
                if (exceptions == null) exceptions = new List<Exception>();
                exceptions.Add(ex);
            }

            System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
            bool tcpsuccess = true;
            double start = Tools.DateUtils.GetUnixTimestamp(DateTime.Now);
            try { tcpClient.Connect(host, port); }
            catch (Exception ex)
            {
                tcpsuccess = false;
                if (exceptions==null) exceptions = new List<Exception>();
                exceptions.Add(ex);
            }
            double end = Tools.DateUtils.GetUnixTimestamp(DateTime.Now);

            if (pingsuccess)
            {
                rttms = Math.Floor((double)pingReply.RoundtripTime);
                rtts = Math.Round(rttms * 0.001, 3);
            }
            else { rttms = -1; rtts = -1; }

            if (tcpsuccess)
            {
                double status = 0;
                status = (end - start) * 1000;
                status = Math.Floor(status);

                latms = Math.Round(status, 3);
                lats = Math.Round(status * 0.001, 3);
            }
            else { latms = -1; lats = -1; }
        }
    }
}
