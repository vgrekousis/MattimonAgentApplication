using MattimonAgentLibrary.Models.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Tools.DebugTools
{
    public class TextLogging
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverObjects"></param>
        public void LogToFile(DeviceServerObjects serverObjects, bool useInstallLocation, out Exception exception)
        {
            exception = null;
            try
            {
                string location = useInstallLocation ? RegistryTools.GetInstallLocationByDisplayName(
                        Static.MattimonRegistrySubkeyNames.DisplayName) + "\\" : "";

                FileStream rfs = new FileStream(location + serverObjects.GetType().Name + "." +
                    (serverObjects.PostAction ?? "LocalRequest") + ".log",
                            FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(rfs);
                bool hasData = sr.ReadLine() != null;
                sr.Close();
                rfs.Close();

                FileStream wfs = new FileStream(location + serverObjects.GetType().Name + "." +
                    (serverObjects.PostAction ?? "LocalRequest") + ".log",
                            FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(wfs);

                if (hasData)
                {
                    sw.WriteLine();
                    sw.WriteLine();
                    sr.Close();
                    rfs.Close();
                }

                sw.WriteLine("[{0}]", DateTime.Now);

                foreach (ServerInstance si in serverObjects.Instances)
                {
                    sw.WriteLine("[INSTANCE]");
                    sw.WriteLine("  WEBID: " + (si.ID == 0 ? "n/a" : si.ID.ToString()));
                    sw.WriteLine("  Server name: " + si.ServerName);
                    sw.WriteLine("  Instance name: " + (si.InstanceName == "" || si.InstanceName == null ? "DEFAULT" : si.InstanceName));
                    sw.WriteLine("  Last Reported: " + (si.LastReportedTimestamp <= 0 ? "Never" : DateUtils.UnixTimeStampToDateTime(si.LastReportedTimestamp).ToString()));

                    foreach (Database db in si.Databases)
                    {
                        sw.WriteLine("  [DATABASE]");
                        sw.WriteLine("    WEBID: " + (db.ID == 0 ? "n/a" : db.ID.ToString()));
                        sw.WriteLine("    Name: " + db.DatabaseName);
                        sw.WriteLine("  [/DATABASE]");

                        foreach (DatabaseConnectionInfo ci in db.Connections)
                        {
                            sw.WriteLine("   [CONNECTION]");
                            sw.WriteLine("     WEBID: " + (ci.ID == 0 ? "n/a" : ci.ID.ToString()));
                            sw.WriteLine("     Host: " + ci.Hostname);
                            sw.WriteLine("     Program: " + ci.ProgramName);
                            sw.WriteLine("   [/CONNECTION]");
                        }
                    }
                    sw.WriteLine("[/INSTANCE]");
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine();
                }
                sw.Close();
                wfs.Close();
            }
            catch (Exception ex) { exception = ex; }
        }
    }
}
