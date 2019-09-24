using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls
{
    

    public class ControlBoxButton : Button
    {
        /// <summary>
        /// 
        /// </summary>
        private ControlBoxButtonAction eAction = ControlBoxButtonAction.None;
        /// <summary>
        /// 
        /// </summary>
        private Padding padding = new Padding(0);
        /// <summary>
        /// 
        /// </summary>
        private Padding margin = new Padding(0);
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public ControlBoxButtonAction ButtonAction
        {
            get { return eAction; }
            set { eAction = value; Set(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public new Padding Padding
        {
            get { return padding; }
            set { base.Padding = value; padding = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public new Padding Margin
        {
            get { return margin; }
            set { base.Margin = value; margin = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ControlBoxButton()
        {
            Padding = new Padding(0);
            Margin = new Padding(0);
            FlatAppearance.BorderSize = 0;
            FlatStyle = FlatStyle.Flat;
            ForeColor = Color.White;
            BackColor = Color.Transparent;
            Text = "";
            Set();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Text = "";
        }
        /// <summary>
        /// 
        /// </summary>
        private void Set()
        {
            switch (eAction)
            {
                case ControlBoxButtonAction.Exit:
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
                    break;
                case ControlBoxButtonAction.Maximize:
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, ((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                    break;
                case ControlBoxButtonAction.Minimize:
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, ((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                    break;
                default:
                    Text = Name;
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, ((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                    break;
            }
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

    public enum ControlBoxButtonAction
    {
        Exit,
        Minimize,
        Maximize,
        Restore,
        None
    }
}
