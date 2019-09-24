using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.WMI
{
    public class NetworkCard
    {
        private string description;
        private string name;
        private NetworkInterfaceType networkCardType;
        private double bytesSent;
        private double bytesReceived;
        private double bytesTotal;
        private double currentBandwidth;
        private double percentUtil;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="networkCardType"></param>
        /// <param name="bytesSent"></param>
        /// <param name="bytesReceived"></param>
        /// <param name="bytesTotal"></param>
        /// <param name="percentUtil"></param>
        public NetworkCard(String description, String name, NetworkInterfaceType networkCardType, double bytesSent, double bytesReceived, double bytesTotal, double currentBandwidth, double percentUtil)
        {
            this.description = description;
            this.name = name;
            this.networkCardType = networkCardType;
            this.bytesSent = bytesSent;
            this.bytesReceived = bytesReceived;
            this.bytesTotal = bytesTotal;
            this.currentBandwidth = currentBandwidth;
            this.percentUtil = percentUtil;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetBytesSent()
        {
            return bytesSent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetBytesReceived()
        {
            return bytesReceived;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetBytesTotal()
        {
            return bytesTotal;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetCurrentBandwidth()
        {
            return currentBandwidth;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetPercentUtilization()
        {
            return percentUtil;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetNetworkCardType()
        {
            return networkCardType.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetName()
        {
            return name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetDescription()
        {
            return description;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NetworkCard[] GetActiveNetworkCards()
        {
            NetworkInterface[] activeNetworkInterfaces = (from nic in
                          NetworkInterface.GetAllNetworkInterfaces()
                                                          where nic.OperationalStatus == OperationalStatus.Up &&
                                                          (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                                                          select nic).ToArray();

            NetworkCard[] networkCards = new NetworkCard[activeNetworkInterfaces.Length];

            for (int i = 0; i < activeNetworkInterfaces.Length; i++)
            {

                double bs = activeNetworkInterfaces[i].GetIPv4Statistics().BytesSent;
                double br = activeNetworkInterfaces[i].GetIPv4Statistics().BytesReceived;
                double bt = bs + br;
                double bw = 0;
                double ut = GetNetworkUtilization(activeNetworkInterfaces[i].Description, out bw);


                networkCards[i] = new NetworkCard(
                    activeNetworkInterfaces[i].Description,
                    activeNetworkInterfaces[i].Name,
                    activeNetworkInterfaces[i].NetworkInterfaceType,
                    bs,
                    br,
                    bt,
                    bw,
                    ut);
            }

            return networkCards;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static NetworkCard[] GetAllNetworkCards()
        {
            NetworkInterface[] cards = (from nic in
                 NetworkInterface.GetAllNetworkInterfaces()
                                        where (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                                        select nic).ToArray();

            NetworkCard[] networkCards = new NetworkCard[cards.Length];

            for (int i = 0; i < cards.Length; i++)
            {
                double bs = cards[i].GetIPv4Statistics().BytesSent;
                double br = cards[i].GetIPv4Statistics().BytesReceived;
                double bt = bs + br;
                double bw = 0;
                double ut = GetNetworkUtilization(cards[i].Description, out bw);

                networkCards[i] = new NetworkCard(
                    cards[i].Description,
                    cards[i].Name,
                    cards[i].NetworkInterfaceType,
                    bs,
                    br,
                    bt,
                    bw,
                    ut);
            }

            return networkCards;
        }
        /// <summary> Calculate network utilization </summary>
        /// <param name="networkCard">Description of network card from NetworkInterface</param>
        /// <returns>NetworkUtilization</returns>
        private static double GetNetworkUtilization(string networkCard, out double currentBandwidth)
        {

            PerformanceCounter bandwidthCounter = null;
            PerformanceCounter dataSentCounter = null;
            PerformanceCounter dataReceivedCounter = null;

            

            const int numberOfIterations = 10;

            // Condition networkCard;
            networkCard = networkCard.Replace("\\", "_");
            networkCard = networkCard.Replace("/", "_");
            networkCard = networkCard.Replace("(", "[");
            networkCard = networkCard.Replace(")", "]");
            networkCard = networkCard.Replace("#", "_");

            if (networkCard.Contains("Hyper-V"))
            {
                currentBandwidth = -1;
                // Hyper-V aren't supported as they can't be found in PerformanceCounter
                return -2; // Return
            }

            try
            {
                bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
            }
            catch
            {
                currentBandwidth = -1;
                return -1;
            }

            var bandwidth = bandwidthCounter.NextValue();

            try
            {
                dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);
            }
            catch
            {
                currentBandwidth = -1;
                return -1;
            }

            try
            {
                dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);
            }
            catch
            {
                currentBandwidth = -1;
                return -1;
            }

            float sendSum = 0;
            float receiveSum = 0;

            for (var index = 0; index < numberOfIterations; index++)
            {
                sendSum += dataSentCounter.NextValue();
                receiveSum += dataReceivedCounter.NextValue();
            }

            var dataSent = sendSum;
            var dataReceived = receiveSum;

            double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;

            currentBandwidth = bandwidth;
            return utilization;
        }
    }
}
