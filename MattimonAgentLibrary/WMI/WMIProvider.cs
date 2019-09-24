using MattimonAgentLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.WMI
{
    public class WMIProvider : IDisposable
    {
        private NetworkWMI NetworkWMI = new NetworkWMI();

        public WMIProvider()
        {
        }

        public Boolean IsVirtualMachine()
        {
            String model = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem").Get().Cast<ManagementObject>()
                            select x.GetPropertyValue("Model")).FirstOrDefault().ToString();
            //Console.WriteLine(model != null ? model.ToString() : "Unknown");
            return model.ToLower().Equals("virtual machine");
        }

        public String GetOperatingSystemString()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        public String GetDomain()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT  Domain FROM win32_computersystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Domain")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        public String GetComputerName()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Name FROM win32_computersystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Name")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        public String GetModel()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Model FROM win32_computersystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Model")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetOSSerialNumber()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("SerialNumber")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetMacAddress()
        {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
            return macAddr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetBIOSSerialNumber()
        {
            var serialnum = (from x in new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS").Get().Cast<ManagementObject>()
                             select x.GetPropertyValue("SerialNumber")).FirstOrDefault();
            return serialnum != null ? serialnum.ToString() : "Unknown";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetIPAddress()
        {
            String ipAddr = "";

            IPHostEntry Host = default(IPHostEntry);
            string Hostname = null;
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddr = Convert.ToString(IP);
                }
            }
            return ipAddr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPublicIPAddress(out Exception exception)
        {
            PublicIPClient publicIPClient = new PublicIPClient();
            return publicIPClient.MyPublicIp(out exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetCurrentCPUUsage()
        {
            return new CPUInfo().PercentageUsed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public double GetMaxClockSpeed(String format = "ghz")
        {
            return new CPUInfo(format).MaximumClockSpeed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public double GetCurrentClockSpeed(String format = "ghz")
        {
            return new CPUInfo(format).CurrentClockSpeed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CPUInfo GetCPUInfo()
        {
            return new CPUInfo();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MemoryInfo GetMemoryInfo()
        {
            return new MemoryInfo();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveType"></param>
        /// <returns></returns>
        public Models.Disk[] GetDisks(String driveType = "")
        {
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("root\\CIMV2",
                String.Format("SELECT * FROM Win32_LogicalDisk {0}", (driveType.Equals("") ? "" : "WHERE DriveType=" + driveType)));

            Models.Disk[] disks = new Models.Disk[searcher.Get().Count];
            int index = 0;
            foreach (ManagementObject queryObj in searcher.Get())
            {
                Models.Disk dsk = new Models.Disk
                {
                    Drive = Convert.ToString(queryObj["DeviceID"]), // Or "Caption"
                    Capacity = Convert.ToDouble(queryObj["Size"]),
                    Available = Convert.ToDouble(queryObj["FreeSpace"]),
                    Descrition = Convert.ToString(queryObj["Description"]),
                    VolumeName = Convert.ToString(queryObj["VolumeName"])
                };

                disks[index] = dsk;
                index++;
            }

            return disks;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Models.DeviceNetworkInterface[] GetDeviceNetworkInterfaces()
        {
            return new CPUInfo.NetworkInfo().GetNetworkInterfaces();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="overall"></param>
        /// <returns></returns>
        public Models.NetworkInterfacePerfInfo[] GetNetworkInterfacePerfInfos(out double overall)
        {
            CPUInfo.NetworkInterfaceInfo networkInterfaceInfo = new CPUInfo.NetworkInterfaceInfo();
            Models.NetworkInterfacePerfInfo[] infos = networkInterfaceInfo.GetNetworkInterfacePerfInfos(out overall);
            return infos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Models.NetworkInterfacePerfInfo[] GetNetworkInterfacePerfInfos2()
        {
            NetworkCard[] activeNetCards = new NetworkWMI().GetActiveNetworkCards();
            NetworkInterfacePerfInfo[] perfInfos = new NetworkInterfacePerfInfo[activeNetCards.Length];

            for (int i = 0; i < activeNetCards.Length; i++)
            {
                NetworkInterfacePerfInfo ncpi = new NetworkInterfacePerfInfo();

                try
                {
                    ncpi.CIMName = activeNetCards[i].GetDescription();
                }
                catch (Exception)
                {
                    ncpi.CIMName = "n/a";
                }
                try
                {
                    ncpi.BytesReceived = activeNetCards[i].GetBytesReceived();
                }
                catch (Exception)
                {
                    ncpi.BytesReceived = -1;
                }
                try
                {
                    ncpi.BytesSent = activeNetCards[i].GetBytesSent();
                }
                catch (Exception)
                {
                    ncpi.BytesSent = -1;
                }
                try
                {
                    ncpi.NetworkCardName = activeNetCards[i].GetDescription();
                }
                catch (Exception)
                {
                    ncpi.NetworkCardName = "n/a";
                }
                if (ncpi.BytesReceived > -1 && ncpi.BytesSent > -1)
                {
                    ncpi.TotalBytes = ncpi.BytesReceived + ncpi.BytesSent;
                }
                else
                {
                    ncpi.TotalBytes = -1;
                }

                ncpi.NetEnabled = true;

                try
                {
                    ncpi.Utilization = activeNetCards[i].GetPercentUtilization();
                } 
                catch (Exception)
                {
                    ncpi.Utilization = -1;
                }


                ncpi.ServiceName = "n/a";
                ncpi.MACAddress = "n/a";
                try
                {
                    ncpi.AdapterType = activeNetCards[i].GetNetworkCardType();
                }
                catch (Exception)
                {
                    ncpi.AdapterType = "n/a";
                }

                try
                {
                    ncpi.Bandwidth = activeNetCards[i].GetCurrentBandwidth();
                }
                catch (Exception)
                {
                    ncpi.Bandwidth = -1;
                }
                perfInfos[i] = ncpi;
            }

            return perfInfos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetNetworkOvlUtilization()
        {
            return NetworkWMI.GetOverallNetworkUtilization();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetNetworkOvlBytesReceived()
        {
            return NetworkWMI.OverallBytesReceived();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetNetworkOvlBytesSent()
        {
            return NetworkWMI.OverallBytesSent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Models.DeviceProcess[] GetDeviceProcesses()
        {
            try
            {
                return new ProcessPerfInfo().GetDeviceProcessesList().ToArray();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ProcessPorts
    {
        /// <summary>
        /// A list of ProcesesPorts that contain the mapping of processes and the ports that the process uses.
        /// </summary>
        public static List<ProcessPort> ProcessPortMap
        {
            get
            {
                return GetNetStatPorts();
            }
        }


        /// <summary>
        /// This method distills the output from netstat -a -n -o into a list of ProcessPorts that provide a mapping between
        /// the process (name and id) and the ports that the process is using.
        /// </summary>
        /// <returns></returns>
        private static List<ProcessPort> GetNetStatPorts()
        {
            List<ProcessPort> ProcessPorts = new List<ProcessPort>();

            try
            {
                using (Process Proc = new Process())
                {

                    ProcessStartInfo StartInfo = new ProcessStartInfo
                    {
                        FileName = "netstat.exe",
                        Arguments = "-a -n -o",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    Proc.StartInfo = StartInfo;
                    Proc.Start();

                    StreamReader StandardOutput = Proc.StandardOutput;
                    StreamReader StandardError = Proc.StandardError;

                    string NetStatContent = StandardOutput.ReadToEnd() + StandardError.ReadToEnd();
                    string NetStatExitStatus = Proc.ExitCode.ToString();

                    if (NetStatExitStatus != "0")
                    {
                        Console.WriteLine("NetStat command failed.   This may require elevated permissions.");
                    }

                    string[] NetStatRows = Regex.Split(NetStatContent, "\r\n");

                    foreach (string NetStatRow in NetStatRows)
                    {
                        string[] Tokens = Regex.Split(NetStatRow, "\\s+");
                        if (Tokens.Length > 4 && (Tokens[1].Equals("UDP") || Tokens[1].Equals("TCP")))
                        {
                            string IpAddress = Regex.Replace(Tokens[2], @"\[(.*?)\]", "1.1.1.1");
                            try
                            {
                                ProcessPorts.Add(new ProcessPort(
                                    Tokens[1] == "UDP" ? GetProcessName(Convert.ToInt16(Tokens[4])) : GetProcessName(Convert.ToInt16(Tokens[5])),
                                    Tokens[1] == "UDP" ? Convert.ToInt16(Tokens[4]) : Convert.ToInt16(Tokens[5]),
                                    IpAddress.Contains("1.1.1.1") ? String.Format("{0}v6", Tokens[1]) : String.Format("{0}v4", Tokens[1]),
                                    Convert.ToInt32(IpAddress.Split(':')[1])
                                ));
                            }
                            catch
                            {
                                Console.WriteLine("Could not convert the following NetStat row to a Process to Port mapping.");
                                Console.WriteLine(NetStatRow);
                            }
                        }
                        else
                        {
                            if (!NetStatRow.Trim().StartsWith("Proto") && !NetStatRow.Trim().StartsWith("Active") && !String.IsNullOrWhiteSpace(NetStatRow))
                            {
                                Console.WriteLine("Unrecognized NetStat row to a Process to Port mapping.");
                                Console.WriteLine(NetStatRow);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ProcessPorts;
        }

        /// <summary>
        /// Private method that handles pulling the process name (if one exists) from the process id.
        /// </summary>
        /// <param name="ProcessId"></param>
        /// <returns></returns>
        private static string GetProcessName(int ProcessId)
        {
            string procName = "UNKNOWN";

            try
            {
                procName = Process.GetProcessById(ProcessId).ProcessName;
            }
            catch { }

            return procName;
        }
    }
    /// <summary>
    /// A mapping for processes to ports and ports to processes that are being used in the system.
    /// </summary>
    public class ProcessPort
    {
        private readonly string _ProcessName = string.Empty;
        private readonly int _ProcessId;
        private readonly string _Protocol = string.Empty;
        private readonly int _PortNumber;

        /// <summary>
        /// Internal constructor to initialize the mapping of process to port.
        /// </summary>
        /// <param name="ProcessName">Name of process to be </param>
        /// <param name="ProcessId"></param>
        /// <param name="Protocol"></param>
        /// <param name="PortNumber"></param>
        internal ProcessPort(string ProcessName, int ProcessId, string Protocol, int PortNumber)
        {
            _ProcessName = ProcessName;
            _ProcessId = ProcessId;
            _Protocol = Protocol;
            _PortNumber = PortNumber;
        }

        public string ProcessPortDescription
        {
            get
            {
                return String.Format("{0} ({1} port {2} pid {3})", _ProcessName, _Protocol, _PortNumber, _ProcessId);
            }
        }
        public string ProcessName
        {
            get { return _ProcessName; }
        }
        public int ProcessId
        {
            get { return _ProcessId; }
        }
        public string Protocol
        {
            get { return _Protocol; }
        }
        public int PortNumber
        {
            get { return _PortNumber; }
        }
    }
    /// <summary>
    /// Management Class
    /// </summary>
    public class MemoryInfo
    {
        public double FreePhysicalMemory { get { return free; } }
        public double TotalVisibleMemorySize { get { return total; } }
        public double PercentageUsed { get { return percentUsed; } }
        public double TotalVirtualMemory { get { return totalvm; } }
        public double FreeVirtualMemory { get { return freevm; } }
        public double TotalPhysicalMemoryGB { get { return gbtotalphysmem; } }
        public double TotalPhysicalMemoryMB { get { return mbtotalphysmem; } }
        public double CommittedVirtualMemory { get { return totalvm - freevm; } }
        public double MemorySpeed { get { return memspeed; } }
        public Int64 TotalRAMGigabyte { get { return totalRamGb; } }
        public Int64 TotalRAM { get { return totalRam; } }
        public int Slots { get { return slots; } }
        public int MaxSlots { get { return maxslots; } }
        public List<ManagementException> ManagementExceptions { get { return managementExceptions; } }
        public List<Exception> Exceptions { get { return exceptions; } }

        private readonly int slots;
        private readonly int maxslots;
        private readonly long totalRam;
        private readonly long totalRamGb;
        private readonly double free;
        private readonly double total;
        private readonly double percentUsed;
        private readonly double totalvm;
        private readonly double freevm;
        private readonly double memspeed;
        private double gbtotalphysmem;
        private double mbtotalphysmem;

        /// <summary>
        /// 
        /// </summary>
        private List<ManagementException> managementExceptions = new List<ManagementException>();
        /// <summary>
        /// 
        /// </summary>
        private List<Exception> exceptions = new List<Exception>();

        /// <summary>
        /// 
        /// </summary>
        public MemoryInfo()
        {
            try
            {
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    try { free = Double.Parse(queryObj["FreePhysicalMemory"].ToString()); }
                    catch { free = -1; }


                    try { total = Double.Parse(queryObj["TotalVisibleMemorySize"].ToString()); }
                    catch { total = -1; }

                    try { totalvm = Double.Parse(queryObj["TotalVirtualMemorySize"].ToString()); }
                    catch { totalvm = -1; }

                    try { freevm = Double.Parse(queryObj["FreeVirtualMemory"].ToString()); }
                    catch { freevm = -1; }

                    if (total != -1 && free != -1)
                        percentUsed = Math.Round(((total - free) / total * 100), 2);
                    else
                        percentUsed = -1;
                }

                try { memspeed = Win32_PhysicalMemory_Speed(); }
                catch { memspeed = -1; }

                try { totalRam = Win32_PhysicalMemory_TotalRAM(false, out this.slots, out this.maxslots); }
                catch { totalRam = -1; }

                try { totalRamGb = Win32_PhysicalMemory_TotalRAM(true, out this.slots, out this.maxslots); }
                catch { totalRamGb = -1; }

                try { gbtotalphysmem = Win32_PhysicalMemory_RAM("gb"); }
                catch { gbtotalphysmem = -1; }
            }
            catch (ManagementException manex)
            {
                memspeed = -1;
                totalRam = -1;
                totalRamGb = -1;
                managementExceptions.Add(manex);
            }
            catch (Exception ex)
            {
                memspeed = -1;
                totalRam = -1;
                totalRamGb = -1;
                exceptions.Add(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="convertToGB"></param>
        /// <param name="banks"></param>
        /// <param name="maxslots"></param>
        /// <returns></returns>
        private Int64 Win32_PhysicalMemory_TotalRAM(Boolean convertToGB, out int banks, out int maxslots)
        {
            maxslots = Win32_PhysicalMemoryArray_MemoryDevices();

            try
            {
                ManagementObjectSearcher search = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
                Int64 total = 0;
                banks = search.Get().Count;
                foreach (ManagementObject ram in search.Get())
                {
                    try
                    {
                        total += (Int64)ram.GetPropertyValue("Capacity");
                    }
                    catch (ManagementException manex)
                    {
                        total = -1;
                        managementExceptions.Add(manex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        total = -1;
                        exceptions.Add(ex);
                        break;
                    }
                }

                if (total == -1) return -1;
                else
                {
                    if (convertToGB)
                        return total / 1073741824;

                    return total;
                }
            }
            catch (ManagementException manex) { managementExceptions.Add(manex); banks = -1; return -1; }
            catch (Exception ex) { exceptions.Add(ex); banks = -1; return -1; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double Win32_PhysicalMemory_Speed()
        {
            try
            {
                double speed = 0;
                ManagementObjectSearcher search = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
                foreach (ManagementObject ram in search.Get())
                {
                    try
                    {
                        speed = Double.Parse(ram.GetPropertyValue("Speed").ToString());
                    }
                    catch (ManagementException manex) { speed = -1; managementExceptions.Add(manex); }
                    catch (Exception ex) { speed = -1; exceptions.Add(ex); }
                }
                return speed;
            }
            catch (ManagementException manex) { managementExceptions.Add(manex); return -1; }
            catch (Exception ex) { exceptions.Add(ex); return -1; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int Win32_PhysicalMemoryArray_MemoryDevices()
        {
            try
            {
                string Query = "SELECT * FROM Win32_PhysicalMemoryArray";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
                int memorydevices = 0;
                foreach (ManagementObject WniPART in searcher.Get())
                {
                    try
                    {
                        memorydevices = Int32.Parse(WniPART.Properties["MemoryDevices"].Value.ToString());
                    }
                    catch (ManagementException manex) { managementExceptions.Add(manex); memorydevices = -1; }
                    catch (Exception ex) { exceptions.Add(ex); memorydevices = -1; }
                }
                return memorydevices;
            }
            catch (ManagementException manex) { managementExceptions.Add(manex); return -1; }
            catch (Exception ex) { exceptions.Add(ex); return -1; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        private double Win32_PhysicalMemory_RAM(string metrics = "gb")
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    double dblMemory;
                    if (double.TryParse(Convert.ToString(queryObj["TotalPhysicalMemory"]), out dblMemory))
                    {
                        mbtotalphysmem =  Convert.ToInt32(dblMemory / (1024 * 1024));
                        gbtotalphysmem = Convert.ToInt32(dblMemory / (1024 * 1024 * 1024));
                    }
                }
            }
            catch (ManagementException e)
            {
                mbtotalphysmem = -1;
                gbtotalphysmem = -1;
            }

            if (metrics == "gb")
            {
                return gbtotalphysmem;
            }
            else
            {
                return mbtotalphysmem;
            }
        }
    }
    /// <summary>
    /// Management Class
    /// </summary>
    public class CPUInfo
    {
        private List<Exception> exceptions = new List<Exception>();
        private List<ManagementException> managementExceptions = new List<ManagementException>();
        private readonly double percentageUsed;
        private readonly double maxClockSpd;
        private readonly double curClockSpd;
        private readonly int cores;
        private readonly int logicalprocessors;
        private readonly int numProcesses;
        private readonly Boolean virtualization;

        public List<Exception> Exceptions { get { return exceptions; } }
        public List<ManagementException> ManagementExceptions { get { return managementExceptions; } }
        public double MaximumClockSpeed { get { return maxClockSpd; } }
        public double CurrentClockSpeed { get { return curClockSpd; } }
        public double PercentageUsed { get { return percentageUsed; } }
        public int Cores { get { return cores; } }
        public int LogicalProcessors { get { return logicalprocessors; } }
        public Boolean VirtualizationFirmwareEnabled { get { return virtualization; } }
        public int NumberOfProcesses { get { return numProcesses; } }

        public CPUInfo(String format = "ghz")
        {
            ManagementObjectSearcher searcher = null;
            try
            {
                searcher =
                        new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'");

                foreach (ManagementObject obj in searcher.Get())
                {
                    try { percentageUsed = Convert.ToDouble(obj["PercentProcessorTime"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); percentageUsed = -1; }
                    catch (Exception ex) { exceptions.Add(ex); percentageUsed = -1; }
                }
            }
            catch (ManagementException ex)
            {
                percentageUsed = -1;
                managementExceptions.Add(ex);
            }
            catch (Exception ex)
            {
                percentageUsed = -1;
                exceptions.Add(ex);
            }

            try
            {
                searcher = new ManagementObjectSearcher(
                "select * from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    try { cores = Convert.ToInt32(obj["NumberOfCores"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); cores = -1; }
                    catch (Exception ex) { exceptions.Add(ex); cores = -1; }

                    try { logicalprocessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); logicalprocessors = -1; }
                    catch (Exception ex) { exceptions.Add(ex); logicalprocessors = -1; }

                    try { curClockSpd = Convert.ToDouble(obj["CurrentClockSpeed"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); curClockSpd = -1; }
                    catch (Exception ex) { exceptions.Add(ex); curClockSpd = -1; }

                    if (curClockSpd != -1)
                        curClockSpd = format == "ghz" ? +(curClockSpd * 0.001) : curClockSpd;

                    try { maxClockSpd = Convert.ToDouble(obj["MaxClockSpeed"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); maxClockSpd = -1; }
                    catch (Exception ex) { exceptions.Add(ex); maxClockSpd = -1; }

                    if (maxClockSpd != -1)
                        maxClockSpd = format == "ghz" ? +(maxClockSpd * 0.001) : maxClockSpd;

                    try { virtualization = Convert.ToBoolean(obj["VirtualizationFirmwareEnabled"]); }
                    catch (ManagementException manex) { managementExceptions.Add(manex); virtualization = false; }
                    catch (Exception ex) { exceptions.Add(ex); virtualization = false; }
                }
            }
            catch (ManagementException ex)
            {
                cores = -1;
                logicalprocessors = -1;
                curClockSpd = -1;
                maxClockSpd = -1;
                managementExceptions.Add(ex);
            }
            catch (Exception ex)
            {
                cores = -1;
                logicalprocessors = -1;
                curClockSpd = -1;
                maxClockSpd = -1;
                exceptions.Add(ex);
            }

            try
            {
                searcher = new ManagementObjectSearcher(
                    "select * from Win32_Process"
                    );
                numProcesses = searcher.Get().Count;
            }
            catch (ManagementException manex) { numProcesses = -1; managementExceptions.Add(manex); }
            catch (Exception wmiex) { numProcesses = -1; exceptions.Add(wmiex); }
        }

        public class NetworkInfo
        {
            private List<ManagementException> managementExceptions = new List<ManagementException>();
            private List<Exception> exceptions = new List<Exception>();

            public List<ManagementException> ManagementExceptions { get { return managementExceptions; } }
            public List<Exception> Exceptions { get { return exceptions; } }

            public NetworkInfo()
            {

            }

           
            public Models.DeviceNetworkInterface[] GetNetworkInterfaces()
            {
                String connId = "n/a";
                String serviceName = "n/a";
                double bytesReceivedPerSec = -1;
                double bytesSentPerSec = -1;
                double currentBandwidth = -1;
                double bytesTotalPerSec = -1, kbps = -1;
                double totalUtilisation = -1;

                try
                {
                    ManagementObjectSearcher searcher =
                             new ManagementObjectSearcher("root\\CIMV2",
                              //"SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled='true'");
                              "SELECT * FROM Win32_NetworkAdapter");


                    ManagementObjectCollection networkAdapters = searcher.Get();

                    Models.DeviceNetworkInterface[] deviceNetworkInterfaces =
                        new Models.DeviceNetworkInterface[networkAdapters.Count];

                    int counter = 0;
                    foreach (ManagementObject obj in networkAdapters)
                    {
                        String name = obj["Name"].ToString();
                        Boolean enabled = Convert.ToBoolean(obj["NetEnabled"]);

                        try { connId = obj["NetConnectionID"].ToString(); }
                        catch (ManagementException manex) { managementExceptions.Add(manex); }
                        catch (Exception ex) { exceptions.Add(ex); }

                        try { serviceName = obj["ServiceName"].ToString(); }
                        catch (ManagementException manex) { managementExceptions.Add(manex); }
                        catch (Exception ex) { exceptions.Add(ex); }

                        ManagementObjectSearcher innerSearcher =
                            new ManagementObjectSearcher("root\\CIMV2",
                            "SELECT * FROM Win32_PerfFormattedData_Tcpip_NetworkInterface WHERE Name = '" + name + "'");

                        ManagementObjectCollection innerManObjCol = innerSearcher.Get();
                        int innerManObjColCount = innerManObjCol.Count;


                        if (innerManObjColCount > 0)
                        {
                            foreach (ManagementObject innerObj in innerManObjCol)
                            {
                                try { bytesReceivedPerSec = Convert.ToDouble(innerObj["BytesReceivedPerSec"]); }
                                catch (ManagementException manex) { managementExceptions.Add(manex); }
                                catch (Exception ex) { exceptions.Add(ex); }

                                try { bytesSentPerSec = Convert.ToDouble(innerObj["BytesSentPerSec"]); }
                                catch (ManagementException manex) { managementExceptions.Add(manex); }
                                catch (Exception ex) { exceptions.Add(ex); }

                                try { bytesTotalPerSec = Convert.ToDouble(innerObj["BytesTotalPerSec"]); }
                                catch (ManagementException manex) { managementExceptions.Add(manex); }
                                catch (Exception ex) { exceptions.Add(ex); }

                                try { currentBandwidth = Convert.ToDouble(innerObj["CurrentBandwidth"]); }
                                catch (ManagementException manex) { managementExceptions.Add(manex); }
                                catch (Exception ex) { exceptions.Add(ex); }

                                try { totalUtilisation = ((bytesTotalPerSec * 8) / currentBandwidth) * 100; }
                                catch (ManagementException manex) { managementExceptions.Add(manex); }
                                catch (Exception ex) { exceptions.Add(ex); }
                            }

                            if (currentBandwidth > 0 && bytesTotalPerSec != -1 && totalUtilisation >= 0)
                            {
                                kbps = bytesTotalPerSec / 1024;
                            }
                        }

                        if ((Double.IsNaN(totalUtilisation)) || totalUtilisation >= Double.MaxValue)
                            totalUtilisation = -1;

                        deviceNetworkInterfaces[counter] = new Models.DeviceNetworkInterface
                        {
                            Name = name,
                            NetConnectionID = connId,
                            ServiceName = serviceName,
                            Enabled = enabled,
                            BytesReceivedPerSec = bytesReceivedPerSec,
                            BytesSentPerSec = bytesSentPerSec,
                            CurrentBandwidth = currentBandwidth,
                            TotalBytesPerSec = bytesTotalPerSec,
                            KiloBytesPerSec = kbps,
                            Utilization = totalUtilisation
                        };

                        counter++;
                    }

                    return deviceNetworkInterfaces;
                }
                catch (ManagementException e)
                {
                    managementExceptions.Add(e);
                    return null;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    return null;
                }
            }
        }

        public class NetworkInterfaceInfo
        {
            private String[] GetNetworkCards(Boolean printConsole = false)
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
                String[] instancename = category.GetInstanceNames();

                if (printConsole)
                    foreach (string name in instancename)
                        Console.WriteLine(name);

                return instancename;
            }
            private double GetNetworkUtilization(string networkCard,
               out double bandwidth,
               out double dataSent,
               out double dataReceived)
            {

                const int numberOfIterations = 10;

                PerformanceCounter bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
                bandwidth = bandwidthCounter.NextValue();//valor fixo 10Mb/100Mn/

                PerformanceCounter dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);

                PerformanceCounter dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

                double sendSum = 0;
                double receiveSum = 0;

                for (int index = 0; index < numberOfIterations; index++)
                {
                    sendSum += Math.Round(dataSentCounter.NextValue());
                    receiveSum += Math.Round(dataReceivedCounter.NextValue());
                }

                dataSent = sendSum;
                dataReceived = receiveSum;


                double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
                return Math.Round(utilization, 2);
            }
            public Models.NetworkInterfacePerfInfo[] GetNetworkInterfacePerfInfos(out double overall)
            {

                String name = "n/a";
                String serviceName = "n/a";
                String adapterType = "n/a";
                String macAddress = "n/a";
                Boolean netEnabled = false;
                ManagementException managementException = null;

                String[] cards = GetNetworkCards();
                Models.NetworkInterfacePerfInfo[] networkInterfacePerfInfos = new Models.NetworkInterfacePerfInfo[cards.Length];
                double[] util = new double[cards.Length];
                overall = 0;
                double dataSent = 0, dataReceived = 0, bandwidth = 0;

                for (int i = 0; i < cards.Length; i++)
                {
                    util[i] = Double.IsNaN(
                        GetNetworkUtilization(cards[i], out bandwidth, out dataSent, out dataReceived)) ? 0 :
                        GetNetworkUtilization(cards[i], out bandwidth, out dataSent, out dataReceived);

                    try
                    {
                        ManagementObjectSearcher searcher =
                            new ManagementObjectSearcher("root\\CIMV2",
                             //"SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled='true'");
                             String.Format("SELECT * FROM CIM_NetworkAdapter WHERE Name = '{0}'", cards[i].Replace("[R]", "(R)")));
                        //String.Format("SELECT * FROM CIM_NetworkAdapter WHERE Name = '{0}'", cards[i]));

                        foreach (ManagementObject obj in searcher.Get())
                        {
                            name = Convert.ToString(obj["Name"]);
                            serviceName = Convert.ToString(obj["ServiceName"]);
                            adapterType = Convert.ToString(obj["AdapterType"]);
                            macAddress = Convert.ToString(obj["MACAddress"]);
                            netEnabled = Convert.ToBoolean(obj["NetEnabled"]);
                        }
                    }
                    catch (ManagementException ex) { managementException = ex; }


                    networkInterfacePerfInfos[i] = new Models.NetworkInterfacePerfInfo
                    {
                        NetworkCardName = cards[i],
                        CIMName = name,
                        ServiceName = serviceName == String.Empty ? "n/a" : serviceName,
                        AdapterType = adapterType == String.Empty ? "n/a" : adapterType,
                        MACAddress = macAddress == String.Empty ? "n/a" : macAddress,
                        BytesReceived = dataReceived,
                        BytesSent = dataSent,
                        TotalBytes = dataSent + dataReceived,
                        Bandwidth = bandwidth,
                        Utilization = util[i],
                        NetEnabled = netEnabled,
                        ManagementExceptionMessage = managementException?.Message
                    };
                }

                foreach (double value in util)
                    overall += value;

                overall = Math.Round(overall / util.Length, 2);
                return networkInterfacePerfInfos;

            }
        }
    }

    public class ProcessPerfInfo
    {

        public List<DeviceProcess> GetDeviceProcessesList()
        {
            List<Exception> exceptions = new List<Exception>();
            List<ManagementException> managementExceptions = new List<ManagementException>();
            List<DeviceProcess> deviceProcesses = new List<DeviceProcess>();

            try
            {
                Process[] processes = Process.GetProcesses();

                foreach (Process p in processes)
                {
                    int pid = -1;
                    string pname = "n/a";
                    double memory = -1;
                    double cpu = -1;
                    double bs = -1;
                    double br = -1;
                    double net = -2;
                    double disk = -2;
                    bool isapp = false;

                    if (p.MainWindowHandle != null)
                    {
                        isapp = p.MainWindowHandle != IntPtr.Zero;
                    }

                    ManagementObjectSearcher mos = null;
                    try
                    {
                        mos = new ManagementObjectSearcher("" +
                        "SELECT * FROM Win32_PerfFormattedData_PerfProc_Process WHERE IDProcess = " + p.Id);
                    }
                    catch (ManagementException ex)
                    {
                        pid = -1;
                        pname = "n/a";
                        memory = -1;
                        cpu = -1;
                        bs = -1;
                        br = -1;
                        managementExceptions.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        pid = -1;
                        pname = "n/a";
                        memory = -1;
                        cpu = -1;
                        bs = -1;
                        br = -1;
                        exceptions.Add(ex);
                    }

                    try { pid = p.Id; }
                    catch { }

                    try { pname = p.ProcessName; }
                    catch { }

                    NetworkTraffic networkTraffic = null;
                    try { networkTraffic = new NetworkTraffic(p.Id); }
                    catch (Exception ex) { bs = -1; br = -1; exceptions.Add(ex); }

                    try { if (networkTraffic != null) bs = networkTraffic.GetBytesSent(); }
                    catch (Exception ex) { bs = -1; exceptions.Add(ex); }
                    try { if (networkTraffic != null) br = networkTraffic.GetBytesReceived(); }
                    catch (Exception ex) { br = -1; exceptions.Add(ex); }

                    try
                    {
                        ManagementObjectCollection col = mos.Get();
                        foreach (ManagementObject obj in col)
                        {
                            try { memory = p.PrivateMemorySize64; }
                            catch (ManagementException ex) { memory = -1; managementExceptions.Add(ex); }
                            catch (Exception ex) { memory = -1; exceptions.Add(ex); }

                            try { cpu = Convert.ToDouble(obj["PercentProcessorTime"]) / Environment.ProcessorCount; }
                            catch (ManagementException ex) { cpu = -1; managementExceptions.Add(ex); }
                            catch (Exception ex) { cpu = -1; exceptions.Add(ex); }
                        }
                    }
                    catch (ManagementException ex)
                    {
                        memory = -1;
                        cpu = -1;
                        managementExceptions.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        memory = -1; cpu = -1;
                        exceptions.Add(ex);
                    }

                    DeviceProcess deviceProcess = new DeviceProcess
                    {
                        BytesReceived = br,
                        BytesSent = bs,
                        CpuUtil = cpu,
                        Description = "",
                        IsApplication = isapp,
                        Name = pname,
                        PrivateMemorySize = memory / 1024 / 1024,
                        ProcessId = pid,
                        TotalBytes = br + bs,
                        DiskUtil = disk,
                        NetUtil = net
                    };
                    deviceProcesses.Add(deviceProcess);
                }
                return deviceProcesses;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
