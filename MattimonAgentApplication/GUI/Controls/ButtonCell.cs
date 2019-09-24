using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Controls
{
    public class ButtonCellTemplate : DataGridViewButtonCell
    {
        /// <summary>
        /// 
        /// </summary>
        private bool focused;
        /// <summary>
        /// 
        /// </summary>
        private System.Windows.Forms.VisualStyles.PushButtonState pushState;
        /// <summary>
        /// 
        /// </summary>
        public ButtonCellTemplate()
        {
            focused = false;
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Normal;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds"></param>
        /// <param name="rowIndex"></param>
        /// <param name="elementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle"></param>
        /// <param name="advancedBorderStyle"></param>
        /// <param name="paintParts"></param>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            ButtonRenderer.DrawButton(graphics, cellBounds, formattedValue.ToString(), new Font("Segoe UI", 9.75f, FontStyle.Regular), focused, pushState);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="throughMouseClick"></param>
        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            base.OnLeave(rowIndex, throughMouseClick);
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Normal;
            focused = false;
        }
        protected override void OnEnter(int rowIndex, bool throughMouseClick)
        {
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Hot;
            focused = false;
        }
        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Normal;
            focused = false;
        }
        protected override void OnMouseEnter(int rowIndex)
        {
            base.OnMouseEnter(rowIndex);
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Hot;
            focused = false;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClick(DataGridViewCellEventArgs e)
        {
            base.OnClick(e);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseDown(e);
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Pressed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseUp(e);
            pushState = System.Windows.Forms.VisualStyles.PushButtonState.Normal;
        }
    }
}
