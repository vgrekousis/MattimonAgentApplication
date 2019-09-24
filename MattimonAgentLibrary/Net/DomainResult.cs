using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Net
{
    public class DomainResult
    {
        private double _tcpConnectLatency;
        private System.Net.NetworkInformation.PingReply _reply;
        private System.Net.NetworkInformation.PingException _pingException;
        private double _rtt;
        private Boolean _status;
        private double _unixScanTS;

        private Boolean _specialContructorUsed;

        public DomainResult(double tcpConnectLatency, System.Net.NetworkInformation.PingReply reply, System.Net.NetworkInformation.PingException pingEx = null)
        {
            _tcpConnectLatency = tcpConnectLatency;
            _reply = reply;
            _pingException = pingEx;
        }

        /// <summary>
        /// Special: To use only if latency, rtt and status are finally formated
        /// to match the database needs.
        /// </summary>
        /// <param name="finalLatency"></param>
        /// <param name="finalRTT"></param>
        /// <param name="status"></param>
        public DomainResult(double finalLatency, double finalRTT, Boolean status, double unixScanTS)
        {
            _specialContructorUsed = true;
            _rtt = finalRTT;
            _tcpConnectLatency = finalLatency;
            _status = status;
            _unixScanTS = unixScanTS;
        }

        public double GetTCPConnectLatency()
        {
            return _tcpConnectLatency;
        }

        public System.Net.NetworkInformation.PingReply GetPingReply(out System.Net.NetworkInformation.PingException pingEx)
        {
            pingEx = _pingException;
            return _reply;
        }

        public double GetRoundtripTime(String format = "seconds")
        {
            if (!_specialContructorUsed)
            {
                throw new Exception("The roundtrip time of the device is not known.");
            }

            return format == "seconds" ? Math.Floor(_rtt) * 0.001 : _rtt;
        }

        public Boolean GetStatus()
        {
            if (!_specialContructorUsed)
            {
                throw new Exception("The status of the device is not known.");
            }
            return _status;
        }

        public double GetUnixScanTimestamp()
        {
            if (!_specialContructorUsed)
            {
                throw new Exception("The device has not yet been scanned.");
            }
            return _unixScanTS;
        }
    }
}
