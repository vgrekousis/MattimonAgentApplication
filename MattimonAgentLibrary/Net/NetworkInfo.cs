using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Net
{
    public class NetworkInfo
    {
        //public static bool InternetConnectionAvailable()
        //{
        //    foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
        //    }
        //    return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        //}

        //public static void EnsureConnectionAvailable()
       // {
       //     if (!InternetConnectionAvailable())
       //         throw new NetworkNotAvailableException();
       // }
    }

    public class NetworkNotAvailableException : Exception
    {
        public NetworkNotAvailableException() : base("Internet connection not available. Check internet connection.")
        {

        }

        public NetworkNotAvailableException(String message) : base(message)
        {

        }
    }
}
