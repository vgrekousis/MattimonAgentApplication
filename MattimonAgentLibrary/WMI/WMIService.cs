using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.WMI
{
    public class WMIService : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public const int SVC_INIT_EVENT_ID = 100;
        /// <summary>
        /// 
        /// </summary>
        public const int ON_TIMER_EVENT_ID = 200;
        /// <summary>
        /// 
        /// </summary>
        public const int DEVICE_UPD_EVENT_ID = 300;
        /// <summary>
        /// 
        /// </summary>
        public const int DEVICE_SQLSRV_UPD_EVENT_ID = 310;
        /// <summary>
        /// 
        /// </summary>
        public const int EVENT_LOG_LISTENER_EVENT_ID = 400;
        /// <summary>
        /// 
        /// </summary>
        private EventLog evtLog;
        /// <summary>
        /// 
        /// </summary>
        private WMIProvider provider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evtLog"></param>
        public WMIService(EventLog evtLog)
        {
            provider = new MattimonAgentLibrary.WMI.WMIProvider();
            this.evtLog = evtLog;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            provider.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Models.Device Scan()
        {
            MattimonAgentLibrary.Models.DeviceProcess[] deviceProcesses = provider.GetDeviceProcesses();
            MattimonAgentLibrary.Models.Device device = new MattimonAgentLibrary.Models.Device
            {
                BIOSSerialNumber = provider.GetBIOSSerialNumber(),
                ComputerName = provider.GetComputerName(),

                CpuCurrentClockSpd = provider.GetCurrentClockSpeed(),
                CpuMaximumClockSpd = provider.GetMaxClockSpeed(),
                CpuUtilization = provider.GetCurrentCPUUsage(),
                CpuLogicalProcessors = provider.GetCPUInfo().LogicalProcessors,
                CpuNumberOfCores = provider.GetCPUInfo().Cores,
                //device.CpuSockets = provider.GetCPUInfo().;
                CpuVirtualization = provider.GetCPUInfo().VirtualizationFirmwareEnabled,
                NumberOfProcesses = provider.GetCPUInfo().NumberOfProcesses
            };


            AlertWMIErrors(device);


            device.MemoryPercentageUsed = provider.GetMemoryInfo().PercentageUsed;
            device.FreePhysicalMemory = provider.GetMemoryInfo().FreePhysicalMemory;
            device.TotalVisibleMemorySize = provider.GetMemoryInfo().TotalVisibleMemorySize;
            device.MemoryFreeVirtual = provider.GetMemoryInfo().FreeVirtualMemory;
            device.MemoryTotalVirtual = provider.GetMemoryInfo().TotalVirtualMemory;
            device.MemorySlotsUsed = provider.GetMemoryInfo().Slots;
            device.MemoryMaxSlots = provider.GetMemoryInfo().MaxSlots;
            device.MemorySpeed = provider.GetMemoryInfo().MemorySpeed;
            device.MemoryTotalPhysicalGB = provider.GetMemoryInfo().TotalPhysicalMemoryGB;
            device.MemoryTotalPhysicalMB = provider.GetMemoryInfo().TotalPhysicalMemoryMB;

            device.NetworkOvlBytesReceived = provider.GetNetworkOvlBytesReceived();
            device.NetworkOvlBytesSent = provider.GetNetworkOvlBytesSent();
            device.NetworkOvlUtilization = provider.GetNetworkOvlUtilization();

            AlertWMIErrors(device);

            device.IpAddress = provider.GetIPAddress();

            // public IP section
            string pip = provider.GetPublicIPAddress(out Exception e);

            // if an exception occurred then
            // the provider returned null
            if (e != null)
            {
                // log the exception
                evtLog.WriteEntry(Tools.ExceptionHelper.GetFormatedExceptionMessage(e), EventLogEntryType.Warning, 300);

                // assign the error message instead of null public ip
                device.PublicIp = e.Message;
            }

            // if the public ip is not null
            // then no exception occurred nor the server actually returned null
            if (pip != null)
                // assign the device's public ip:
                device.PublicIp = pip;

            else // the provider returned null
            {
                if (e == null)  // but if it isn't due to an exception, 
                                // let's assume that the public ip server
                                // returned null:
                {
                    device.PublicIp = "Error: Public IP Server returned null";
                }
            }
            // end of public ip section
            

            device.MacAddress = provider.GetMacAddress();
            device.Model = provider.GetModel();
            device.OperatingSystem = provider.GetOperatingSystemString();
            device.OperatingSystemSerialNumber = provider.GetOSSerialNumber();
            device.BIOSSerialNumber = provider.GetBIOSSerialNumber();
            device.IsVistualMachine = provider.IsVirtualMachine();

            device.DeviceDisks = provider.GetDisks("3");


           //double overallNetworkUtil = -1;
           //device.NetworkInterfacePerfInfos = provider.GetNetworkInterfacePerfInfos(out overallNetworkUtil);
            device.NetworkInterfacePerfInfos = provider.GetNetworkInterfacePerfInfos2();
            //device.OverallNetworkUtilization = overallNetworkUtil;


            AlertWMIErrors(device);


            device.DeviceProcesses = deviceProcesses;
            AlertDeviceProcessExceptions(deviceProcesses);

            new System.Threading.Thread(() =>
            {
                foreach (MattimonAgentLibrary.Models.NetworkInterfacePerfInfo nipi in device.NetworkInterfacePerfInfos)
                {
                    evtLog.WriteEntry(
                        String.Format("Device Network Interfaces [Debug]\n" +
                        "Interface name: {0} ({1})\n" +
                        "Service Name: {2}\n" +
                        "Bytes Sent: {3}\n" +
                        "Bytes Received: {4}\n" +
                        "Total Bytes: {5} ({6} kbps)\n" +
                        "Bandwidth: {7}\n" +
                        "Utilization: {8} %",
                        nipi.NetworkCardName, // 0
                        nipi.NetEnabled ? "Enabled" : "Disabled", // 1
                        nipi.ServiceName, // 2
                        nipi.BytesSent, //3
                        nipi.BytesReceived, //4
                        nipi.TotalBytes, // 5
                        nipi.TotalBytes / 1024, // 6
                        nipi.Bandwidth, // 7
                        nipi.Utilization // 8
                        ),
                        EventLogEntryType.Warning, 300);

                }
            });//.Start();

            Net.PingResult pingResult = new Net.PingResult(
                MattimonAgentLibrary.Rest.Constants.MATTIMON_WEBSITE_HOSTNAME, 80, out List<Exception> relatedPingExceptions);

            if (relatedPingExceptions != null)
            {
                String message = "Some exceptions occurred in MattimonAgentLibrary.Net.PingResult()\n\n";
                foreach (Exception ex in relatedPingExceptions)
                {
                    message += ex.Message + "\n------------------\n" + ex.ToString() + "\n\n\n";
                }
                evtLog.WriteEntry(message, EventLogEntryType.Warning, 301);
            }

            device.Latency = pingResult.TcpLatencySeconds;
            device.Rtt = pingResult.RoundtripTimeSeconds;
            device.Status = 1;
            //device.LastCheckedTimestamp = MattimonAgentLibrary.Tools.DateUtils.GetUnixTimestamp();

            return device;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceProcesses"></param>
        private void AlertDeviceProcessExceptions(MattimonAgentLibrary.Models.DeviceProcess[] deviceProcesses)
        {
            if (deviceProcesses == null) return;

            new System.Threading.Thread(() =>
            {
                foreach (MattimonAgentLibrary.Models.DeviceProcess dp in deviceProcesses)
                {
                    String message = "";
                    if (dp.ManagementExceptions != null)
                    {
                        message = String.Format("One or more Management Exceptions occurred while querying the process #{0} ({1})\n\n", dp.ProcessId, dp.Name);
                        message += String.Format("Number of exceptions: {0}", dp.ManagementExceptions.Length);
                        int order = 0;
                        foreach (ManagementException ex in dp.ManagementExceptions)
                        {
                            message += String.Format("{0}[{1}]\nError details:\n{2}", ex.GetType().ToString(), order, ex.ToString());
                            order++;
                        }

                        evtLog.WriteEntry(message, EventLogEntryType.Warning, DEVICE_UPD_EVENT_ID);
                    }
                    if (dp.Exceptions != null)
                    {
                        message = String.Format("One or more Exceptions occurred while querying the process #{0} ({1})\n\n", dp.ProcessId, dp.Name);
                        message += String.Format("Number of exceptions: {0}", dp.ManagementExceptions.Length);
                        int order = 0;
                        foreach (ManagementException ex in dp.ManagementExceptions)
                        {
                            message += String.Format("{0}[{1}]\nError details:\n{2}", ex.GetType().ToString(), order, ex.ToString());
                            order++;
                        }

                        evtLog.WriteEntry(message, EventLogEntryType.Warning, DEVICE_UPD_EVENT_ID);
                    }
                }
            }).Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        private void AlertWMIErrors(MattimonAgentLibrary.Models.Device device)
        {
            String deviceWMIErrorMessage = null;
            if (device.ManagementExceptions != null)
            {
                if (device.ManagementExceptions.Count > 0)
                {
                    String message = "Some Management Exceptions occurred.\n\n";
                    int counter = 0;

                    foreach (System.Management.ManagementException ex in device.ManagementExceptions)
                    {
                        message += "==================================\n";
                        message += "Management Exception (" + counter + ")\n";
                        message += "------------------------------------\n";
                        message += "Error code: " + ex.ErrorCode + "\n";
                        message += "Error Information: " + ex.ErrorInformation + "\n";
                        message += "Error Message: " + ex.Message + "\n";
                        message += "\nDetails:\n" + ex.ToString() + "\n";
                        message += "==================================\n\n\n";
                        counter++;
                    }
                    deviceWMIErrorMessage += message + "\n\n\n";

                    // Reset this field for the next ManagementException check
                    device.ManagementExceptions = new List<System.Management.ManagementException>();
                }
            }
            if (device.WMIExceptions != null)
            {
                if (device.WMIExceptions.Count > 0)
                {
                    String message = "Some Exceptions occurred.\n\n";
                    int counter = 0;

                    foreach (Exception ex in device.WMIExceptions)
                    {
                        message += "==================================\n";
                        message += "Exception (" + counter + ")\n";
                        message += "------------------------------------\n";
                        message += "Error Message: " + ex.Message + "\n";
                        message += "\nDetails:\n" + ex.ToString() + "\n";
                        message += "==================================\n\n\n";
                        counter++;
                    }
                    deviceWMIErrorMessage += message + "\n\n\n";

                    // Reset this field for the next Exception check
                    device.WMIExceptions = new List<Exception>();
                }
            }
            if (deviceWMIErrorMessage != null)
            {
                evtLog.WriteEntry(deviceWMIErrorMessage, EventLogEntryType.Warning, 300);
            }
        }
    }
}
