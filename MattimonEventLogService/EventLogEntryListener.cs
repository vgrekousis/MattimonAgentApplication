using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonEventLogService
{
    public class EventLogEntryListener
    {
        EventLog debuglog;
        List<String> validLogNames = new List<String>();
        event EventLogEntryWrittenEventHandler EventLogEntryWritten;
        EventLog[] valideventlogs = null;

        public EventLog[] LoadedEventLogs { get { return valideventlogs; } }
        public String[] ValidatedLogNames { get { return validLogNames.ToArray(); } }
        public int ValidatedLogNamesCount { get { return validLogNames.Count; } }
        public EventLog DebugLog { get { return debuglog; } set { debuglog = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">External EventLogEntryWrittenEventHandler event handler method</param>
        /// <param name="lognames"></param>
        public EventLogEntryListener(EventLogEntryWrittenEventHandler callback, params String[] lognames)
        {
            EventLogEntryWritten += callback;

            foreach (EventLog checkLog in EventLog.GetEventLogs())
            {
                foreach (String name in lognames)
                {
                    if (name.Equals(checkLog.Log))
                        validLogNames.Add(name);
                }
            }

            if (validLogNames.Count > 0)
            {
                valideventlogs = new EventLog[validLogNames.Count];

                for (int i = 0; i < valideventlogs.Length; i++)
                {
                    valideventlogs[i] = new EventLog
                    {
                        Log = validLogNames[i]
                    };
                }
            }
        }
        public void StartListening()
        {
            String message = "[EventLogListener->StartListening]\n\nWill listen on the following event logs:\n";
            foreach (EventLog evtlog in valideventlogs)
            {
                message += evtlog.Log;

                evtlog.EntryWritten += EventLogEntryListener_EntryWritten;
                evtlog.EnableRaisingEvents = true;
                message += " (Started)\n";
            }

            if (debuglog != null)
                debuglog.WriteEntry(message, EventLogEntryType.Information, MattimonEventLogService.EVENT_LOG_LISTENER_EVENT_ID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logname"><code>null</code> by default. Leave <code>null</code> to stop listening on all specified logs.</param>
        public void StopListening(String logname = null)
        {
            string message = "[EventLogListener->StopListening]\n\nWill stop listen on the following event logs:\n";

            if (logname != null)
            {
                string[] names = logname.Split(',');

                foreach (string name in names)
                {
                    foreach (EventLog evtlog in valideventlogs)
                    {
                        message += name;
                        if (evtlog.Log.ToLower().Equals(name.ToLower()))
                        {
                            evtlog.EntryWritten -= EventLogEntryListener_EntryWritten;
                            evtlog.EnableRaisingEvents = false;
                            message += " (Stopped)";
                        }
                    }
                }
                if (debuglog != null)
                    debuglog.WriteEntry(message, EventLogEntryType.Information, MattimonEventLogService.EVENT_LOG_LISTENER_EVENT_ID);
                return;
            }


            foreach (EventLog evtlog in valideventlogs)
            {
                message += evtlog.Log;
                evtlog.EntryWritten -= EventLogEntryListener_EntryWritten;
                evtlog.EnableRaisingEvents = false;
                message += " (Stopped)";
            }

            if (debuglog != null)
                debuglog.WriteEntry(message, EventLogEntryType.Information, MattimonEventLogService.EVENT_LOG_LISTENER_EVENT_ID);
        }

        public int ValidLogsCount()
        {
            return validLogNames.Count;
        }

        private void EventLogEntryListener_EntryWritten(object sender, EntryWrittenEventArgs e)
        {


            int handlers = 0;

            try
            {
                string machinename = e.Entry.MachineName == "" || e.Entry.MachineName == null ? System.Environment.MachineName : e.Entry.MachineName;
                EventLog assocEventLog = new EventLog(EventLog.LogNameFromSourceName(e.Entry.Source, machinename));
                EventLogEntry entry = e.Entry;

                if (EventLogEntryWritten != null)
                {
                    Delegate[] subscribers = EventLogEntryWritten.GetInvocationList();
                    foreach (EventLogEntryWrittenEventHandler target in subscribers)
                    {
                        target(this, new EventLogEntryWrittenArgs(assocEventLog, entry));
                        handlers++;
                    }
                }
            }
            catch (Exception ex)
            {
                if (debuglog != null)
                    debuglog.WriteEntry(ex.Message + "\n\n" + ex.ToString(),
                        EventLogEntryType.Error, MattimonEventLogService.EVENT_LOG_LISTENER_EVENT_ID);
                return;
            }
        }

        public EventLog[] GetValidatedEventLogs()
        {
            return valideventlogs;
        }

        public String[] GetValidatedLogNames()
        {
            return validLogNames.ToArray();
        }
    }
    public delegate void EventLogEntryWrittenEventHandler(object sender, EventLogEntryWrittenArgs e);
    public class EventLogEntryWrittenArgs : EventArgs
    {
        readonly EventLog assocEventLog;
        readonly EventLogEntry eventLogEntry;

        public EventLog AssociatedEventLog { get { return assocEventLog; } }
        public EventLogEntry EventLogEntry { get { return eventLogEntry; } }

        public EventLogEntryWrittenArgs(EventLog assocEventLog, EventLogEntry eventLogEntry)
        {
            this.assocEventLog = assocEventLog;
            this.eventLogEntry = eventLogEntry;
        }
    }
}
