using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentApplication.GUI.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceStateChangedEventArgs : EventArgs
    {
        private readonly String serviceName;
        /// <summary>
        /// 
        /// </summary>
        public String ServiceName
        {
            get { return serviceName; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="before"></param>
        /// <param name="selected"></param>
        public ServiceStateChangedEventArgs(String serviceName) : base()
        {
            this.serviceName = serviceName;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ServiceStateChangedEventHandler(object sender, ServiceStateChangedEventArgs e);
}
