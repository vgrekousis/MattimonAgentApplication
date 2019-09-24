using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.WMI
{
    public class NetworkWMI
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool NetworkAvailable()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double OverallBytesSent()
        {
            double bytes = -1;

            if (!NetworkInterface.GetIsNetworkAvailable())
                return bytes;

            try
            {
                NetworkInterface[] interfaces
                    = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    bytes += ni.GetIPv4Statistics().BytesSent;
                }
            }
            catch (Exception)
            {
                return -2;
            }

            return bytes;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double OverallBytesReceived()
        {
            double bytes = -1;

            if (!NetworkInterface.GetIsNetworkAvailable())
                return bytes;

            try
            {
                NetworkInterface[] interfaces
                    = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    bytes += ni.GetIPv4Statistics().BytesReceived;
                }
            }
            catch (Exception)
            {
                return -2;
            }

            return bytes;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetFormatedNICStatuses()
        {
            string sts = "";
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in networkInterfaces)
            {

                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    string cn = nic.Name;
                    string ct = nic.NetworkInterfaceType.ToString();
                    string bs = nic.GetIPv4Statistics().BytesSent.ToString();
                    string br = nic.GetIPv4Statistics().BytesReceived.ToString();
                    string bt = (Convert.ToDouble(bs) + Convert.ToDouble(br)).ToString();

                    sts +=
                        String.Format(
                            "Name: {0}@" +
                            "Type: {1}@" +
                            "Bytes Sent: {2}@" +
                            "Bytes Received: {3}@" +
                            "Bytes Total: {4}@@@",
                            cn, ct, bs, br, bt);
                }

            }

            return sts.Replace("@", System.Environment.NewLine);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetOverallNetworkUtilization()
        {
            ManagementObjectSearcher searcher =

            new ManagementObjectSearcher("root\\CIMV2",

            "SELECT CurrentBandwidth,  BytesTotalPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface");// WHERE Name = '" + description + "'");


            double cbw = 0;
            double bt = 0;
            double u = 0;

            foreach (ManagementObject queryObj in searcher.Get())
            {
                cbw += Convert.ToDouble(queryObj["CurrentBandwidth"]);
                bt += Convert.ToDouble(queryObj["BytesTotalPerSec"]);
            }

            u = (8 * (bt)) / (cbw * 1) * 100;
            return u;
        }
        public NetworkCard[] GetNetworkCards()
        {
            return NetworkCard.GetAllNetworkCards();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NetworkCard[] GetActiveNetworkCards()
        {
            return NetworkCard.GetActiveNetworkCards();
        }
    }
}
