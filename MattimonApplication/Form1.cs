using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentLibrary;

namespace MattimonApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            MattimonAgentLibrary.Rest.DeviceRequests deviceRequests = new 
                MattimonAgentLibrary.Rest.DeviceRequests();
            deviceRequests.DeviceEntryExist(84);
        }
    }
}
