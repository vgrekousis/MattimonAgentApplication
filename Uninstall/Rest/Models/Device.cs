using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall.Rest.Models
{
    public class Device : IRequestModel
    {
        public long Device_Id { get; set; }
        public long User_Id { get; set; }
        public long Company_Id { get; set; }
        public long Device_Type_Id { get; set; }

        public Boolean UseAgent { get; set; }
        public double AgentReportInterval { get; set; }
        public Boolean NotificationEmails { get; set; }
        public int Port { get; set; }

        public string ComputerName { get; set; }
        public string Model { get; set; }
        public double LastCheckedTimestamp { get; set; }
        public int Status { get; set; }
        public double Latency { get; set; }
        public double Rtt { get; set; }

        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string OperatingSystem { get; set; }
        public string OperatingSystemSerialNumber { get; set; }
        public string BIOSSerialNumber { get; set; }

        public double CpuUtilization { get; set; }
        public double CpuCurrentClockSpd { get; set; }
        public double CpuMaximumClockSpd { get; set; }
        public int CpuNumberOfCores { get; set; }
        public int CpuSockets { get; set; }
        public int CpuLogicalProcessors { get; set; }
        public Boolean CpuVirtualization { get; set; }
        public int NumberOfProcesses { get; set; }

        public double FreePhysicalMemory { get; set; }
        public double TotalVisibleMemorySize { get; set; }
        public double MemoryPercentageUsed { get; set; }
        public int MemorySlotsUsed { get; set; }
        public int MemoryMaxSlots { get; set; }
        public double MemorySpeed { get; set; }
        public double MemoryFreeVirtual { get; set; }
        public double MemoryTotalVirtual { get; set; }

        public bool IsVistualMachine { get; set; }

        public Disk[] DeviceDisks { get; set; }

        public Exception Exception { get; set; }
        public String MySqlExceptionMessage { get; set; }
        public HttpRequestException HttpRequestException { get; set; }
        public Boolean RequestSuccess { get; set; }
        public String Tag { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
