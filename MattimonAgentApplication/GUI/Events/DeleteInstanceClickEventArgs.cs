using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentApplication.GUI.Events
{
    public class DeleteInstanceClickEventArgs : EventArgs
    {
        private readonly int inspk;
        private readonly string srvname;
        private readonly string insname;

        public int InstancePrimaryKey
        {
            get => inspk;
        }
        
        public string ServerName
        {
            get => srvname;
        }
        public string InstanceName
        {
            get => insname;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instancePrimaryKey"></param>
        public DeleteInstanceClickEventArgs(int instancePrimaryKey, string serverName, string instanceName)
        {
            inspk = instancePrimaryKey;
            srvname = serverName;
            insname = instanceName;
        }
    }

    public delegate void DeleteInstanceClickEventHandler(object sender, DeleteInstanceClickEventArgs e);
}
