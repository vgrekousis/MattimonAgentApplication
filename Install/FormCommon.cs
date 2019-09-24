using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Install
{
    public partial class FormCommon : Form
    {
        public FormCommon()
        {
            InitializeComponent();
            Text = Static.ExecutingAssemblyAttributes.AssemblyTitle;
        }
    }
}
