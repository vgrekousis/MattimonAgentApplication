using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.BitscoreForms.Controls
{
    public class BorderlessFlatButton : Button
    {
        private readonly FlatStyle mFlatStyle = FlatStyle.Flat;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle
        {
            get { return mFlatStyle; }
        }

        public BorderlessFlatButton() : base()
        {
            BackColor = Color.Black;
            ForeColor = Color.White;
            base.FlatStyle = FlatStyle;
            base.FlatAppearance.BorderSize = 0;
            base.FlatAppearance.BorderColor = BackColor;
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
