using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Net
{
    public class PingRequests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DomainResult PingDomain(String host, int port, int timeout = 12000, String format = "seconds")
        {
            System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.SendTimeout = timeout;
            double status;
            System.Net.NetworkInformation.PingReply reply = null;
            System.Net.NetworkInformation.PingException pingEx = null;

            try
            {
                double start = Tools.DateUtils.GetUnixTimestamp(DateTime.Now);
                tcpClient.Connect(host, port);
                double end = Tools.DateUtils.GetUnixTimestamp(DateTime.Now);
                status = (end - start) * 1000;
                status = Math.Floor(status);
                double tcpConnectLatency = format == "seconds" ? status * 0.001 : status;

                System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
                try { reply = pingSender.Send(host); }
                catch (System.Net.NetworkInformation.PingException pe) { pingEx = pe; }
                catch { }

                return new DomainResult(tcpConnectLatency, reply, pingEx);
            }
            catch
            {
                return new DomainResult(-1, reply);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DomainResult Scan(String host, int port, EventLog eventLog)
        {
            DomainResult result = Net.PingRequests.PingDomain(host, port);
            int unix_ts = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);

            double latency = result.GetTCPConnectLatency();
            double rtt;
            System.Net.NetworkInformation.PingException pingEx = null;
            System.Net.NetworkInformation.PingReply reply = result.GetPingReply(out pingEx);

            // IMPORTANT:
            // Set rtt to -2 if an error occurred during ping request
            if (pingEx != null) { rtt = -2; eventLog.WriteEntry("Unable to get a ping reply from the host: " + host + ":" + port + ".\n" + "Ping Exception message: \n" + pingEx.Message + "\n\nStack trace:\n" + pingEx.StackTrace, EventLogEntryType.Warning); }

            // IMPORTANT:
            // Set rtt to -1 is there is not any reply.
            // This will allow us indicating to the website that information aren't available as 
            // probably the host blocks it
            else if (reply == null) { rtt = -1; }

            // In any other case, we have an actual round trip time.
            else { rtt = reply.RoundtripTime; }
            // End do ping

            Boolean sts = latency != -1;

            // This is when we use the special contructor to return the formated statuses.
            return new DomainResult(latency, rtt, sts, unix_ts);
        }
    }
}
