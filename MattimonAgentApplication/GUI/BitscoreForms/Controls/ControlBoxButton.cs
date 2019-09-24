using MattimonAgentApplication.GUI.Extensions;
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
    public enum ControlBoxButtonStyle
    {
        Thick,
        Normal
    }
    public enum ControlBoxButtonSize
    {
        Pixels16,
        Pixels32,
        MaxAvailable
    }
    public class ControlBoxButton : Button
    {
        /// <summary>
        /// 
        /// </summary>
        private ControlBoxButtonAction eAction = ControlBoxButtonAction.None;
        /// <summary>
        /// 
        /// </summary>
        private readonly ControlBoxButtonSize mControlBoxButtonSize = ControlBoxButtonSize.Pixels16;
        /// <summary>
        /// 
        /// </summary>
        private readonly ControlBoxButtonStyle mControlBoxButtonStyle = ControlBoxButtonStyle.Normal;
        /// <summary>
        /// 
        /// </summary>
        private readonly ToolTip toolTip = new ToolTip();
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
        private int mSize = 40;
        private int mControlBoxImgSize = 16;
        public new int Size
        {
            get { return mSize; }
            set { mSize = value; Height = value; Width = Height; }
        }
        public int ImageSize
        {
            get { return mControlBoxImgSize; }
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
            Font = SystemFonts.CaptionFont;
            Height = mSize;
            Width = mSize;
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            Set();
            base.OnHandleCreated(e);
            Form f = FindForm();

            f.Resize += (s, evt) => {

                if (eAction == ControlBoxButtonAction.Maximize)
                {
                    Image imgMaximize = GetControBoxImages()[1];
                    Image imgRestore = GetControBoxImages()[2];

                    BackgroundImageLayout = ImageLayout.Center;
                    BackgroundImage = f.WindowState == FormWindowState.Maximized ?

                       imgRestore.ResizeImage(mControlBoxImgSize) :
                       imgMaximize.ResizeImage(mControlBoxImgSize);

                    toolTip.SetToolTip(this, f.WindowState == FormWindowState.Maximized ? "Restore" : "Maximize");

                }
            };
        }
        /// <summary>
        /// Index 0: Close
        /// Index 1: Maximize
        /// Index 2: Restore
        /// Index 3: Minimize
        /// </summary>
        /// <returns></returns>
        private Image[] GetControBoxImages()
        {
            Image[] img = new Image[4];

            switch (mControlBoxButtonSize)
            {
                ///
                /// 16 Pixels
                ///
                case ControlBoxButtonSize.Pixels16:

                    switch (mControlBoxButtonStyle)
                    {
                        ///
                        /// Use regular non-thick style
                        ///
                        case ControlBoxButtonStyle.Normal:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_white;
                            break;
                        ///
                        /// Use thick style
                        ///
                        case ControlBoxButtonStyle.Thick:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_thick_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_thick_white;
                            break;
                        ///
                        /// Use regular non-thick style for invalid values
                        ///
                        default:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_white;
                            break;
                    }
                    break;
                ///
                /// 32 Pixels
                ///
                case ControlBoxButtonSize.Pixels32:

                    switch (mControlBoxButtonStyle)
                    {
                        ///
                        /// Use regular non-thick style
                        ///
                        case ControlBoxButtonStyle.Normal:
                        ///
                        /// Use regular non-thick style for any other value
                        /// There is no Thick Style for this ControlBoxButtonSize.Pixels32
                        ///
                        default:
                            img[0] = MattimonAgentApplication.Properties.Resources.close32_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize32_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore32_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize32_white;
                            break;
                    }
                    break;
                ///
                /// Load maximum available
                ///
                case ControlBoxButtonSize.MaxAvailable:

                    switch (mControlBoxButtonStyle)
                    {
                        ///
                        /// Use regular non-thick style
                        ///
                        case ControlBoxButtonStyle.Normal:
                        ///
                        /// Use regular non-thick style for any other value
                        /// There is no Thick Style for this ControlBoxButtonSize.MaxAvailable
                        ///
                        default:
                            img[0] = MattimonAgentApplication.Properties.Resources.close_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize_white;
                            break;
                    }
                    break;

                ///
                /// Use 16 pixel
                /// 
                default:
                    switch (mControlBoxButtonStyle)
                    {
                        case ControlBoxButtonStyle.Normal:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_white;
                            break;
                        case ControlBoxButtonStyle.Thick:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_thick_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_thick_white;
                            break;
                        default:
                            img[0] = MattimonAgentApplication.Properties.Resources.close16_white;
                            img[1] = MattimonAgentApplication.Properties.Resources.maximize16_white;
                            img[2] = MattimonAgentApplication.Properties.Resources.restore16_white;
                            img[3] = MattimonAgentApplication.Properties.Resources.minimize16_white;
                            break;
                    }
                    break;
            }

            return img;
        }
        /// <summary>
        /// 
        /// </summary>
        private void Set()
        {
            switch (eAction)
            {
                case ControlBoxButtonAction.Exit:
                    Image imgClose = GetControBoxImages()[0];
                    toolTip.SetToolTip(this, "Close");
                    BackgroundImageLayout = ImageLayout.Center;
                    BackgroundImage = imgClose.ResizeImage(mControlBoxImgSize);
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
                    break;

                case ControlBoxButtonAction.Maximize:
                    Image imgMaximize = GetControBoxImages()[1];
                    toolTip.SetToolTip(this, "Maximize");
                    BackgroundImageLayout = ImageLayout.Center;
                    BackgroundImage = imgMaximize.ResizeImage(mControlBoxImgSize);
                    FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, ((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                    break;

                case ControlBoxButtonAction.Minimize:
                    Image imgMinimize = GetControBoxImages()[3];
                    toolTip.SetToolTip(this, "Minimize");
                    BackgroundImageLayout = ImageLayout.Center;
                    BackgroundImage = imgMinimize.ResizeImage(mControlBoxImgSize);
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

        protected override void OnClick(EventArgs e)
        {
            Form f = FindForm();
            if (f is BitscoreForm && ((BitscoreForm)f).IsInDesignMode)
                return;

            switch (eAction)
            {
                case ControlBoxButtonAction.Exit:
                    f.Close();
                    break;
                case ControlBoxButtonAction.Maximize:
                    f.WindowState = f.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    break;
                case ControlBoxButtonAction.Minimize:
                    f.WindowState = FormWindowState.Minimized;
                    break;
                case ControlBoxButtonAction.Restore:
                    f.WindowState = FormWindowState.Normal;
                    break;
                default:
                    break;
            }
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
