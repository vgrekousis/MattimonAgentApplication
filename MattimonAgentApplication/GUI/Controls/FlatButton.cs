using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls
{
    public class FlatButton : Button
    {
        public FlatButton()
        {
            FlatAppearance.BorderSize = 0;
            BackColor = System.Drawing.Color.FromArgb(100, 200, 100);
            ForeColor = System.Drawing.Color.White;
            FlatStyle = FlatStyle.Flat;
            TabStop = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(false);
        }
    }
}
