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
    public class ServiceStartChangedEventArgs : EventArgs
    {
        private readonly MattimonAgentLibrary.Tools.MyServiceController.ServiceStart selected;
        private readonly MattimonAgentLibrary.Tools.MyServiceController.ServiceStart before;
        private readonly string serviceName;
        private readonly string displayName;

        /// <summary>
        /// 
        /// </summary>
        public MattimonAgentLibrary.Tools.MyServiceController.ServiceStart SelectedServiceStart
        {
            get { return selected; }
        }
        /// <summary>
        /// 
        /// </summary>
        public MattimonAgentLibrary.Tools.MyServiceController.ServiceStart ServiceStartBefore
        {
            get { return before; }
        }
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
        public String DisplayName
        {
            get { return DisplayName1; }
        }

        public string DisplayName1 => displayName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="displayName"></param>
        /// <param name="before"></param>
        /// <param name="selected"></param>
        public ServiceStartChangedEventArgs(String serviceName, String displayName, MattimonAgentLibrary.Tools.MyServiceController.ServiceStart before, MattimonAgentLibrary.Tools.MyServiceController.ServiceStart selected) : base()
        {
            this.before = before;
            this.selected = selected;
            this.serviceName = serviceName;
            this.displayName = displayName;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ServiceStartChangedEventHandler(object sender, ServiceStartChangedEventArgs e);
}
