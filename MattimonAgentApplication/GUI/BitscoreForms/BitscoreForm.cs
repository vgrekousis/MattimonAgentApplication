using MattimonAgentApplication.GUI.BitscoreForms.Controls;
using MattimonAgentApplication.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.BitscoreForms
{
    public partial class BitscoreForm : Form, IMessageFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public BitscoreForm() : base()
        {
            try
            {
                Application.AddMessageFilter(this);
                base.FormBorderStyle = mFormBorderStyle;
                DoubleBuffered = true;
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Overrided Methods
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            if (mEnableFormDragOpacity)
                Opacity = mFormDragOpacity;
            else
                Opacity = mInitialFormDragOpacity;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Opacity = mInitialFormDragOpacity;
        }
        #endregion

        #region Win32
        //private bool Drag;
        //private int MouseX;
        //private int MouseY;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_LBUTTONDOWN = 0x0201;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            if (EnableSnap)
            {
                switch (m.Msg)
                {

                    case WmEnterSizeMove:
                    case WmSize:
                        // Need to handle window size changed as well when
                        // un-maximizing the form by dragging the title bar.
                        _dragOffsetX = Cursor.Position.X - Left;
                        _dragOffsetY = Cursor.Position.Y - Top;
                        break;
                    case WmMoving:
                        LtrbRectangle boundsLtrb = Marshal.PtrToStructure<LtrbRectangle>(m.LParam);
                        Rectangle bounds = boundsLtrb.ToRectangle();
                        // This is where the window _would_ be located if snapping
                        // had not occurred. This prevents the cursor from sliding
                        // off the title bar if the snap distance is too large.
                        Rectangle effectiveBounds = new Rectangle(
                            Cursor.Position.X - _dragOffsetX,
                            Cursor.Position.Y - _dragOffsetY,
                            bounds.Width,
                            bounds.Height);
                        _snapAnchor = FindSnap(ref effectiveBounds);
                        LtrbRectangle newLtrb = LtrbRectangle.FromRectangle(effectiveBounds);
                        Marshal.StructureToPtr(newLtrb, m.LParam, false);
                        m.Result = new IntPtr(1);
                        break;
                }
            }

            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        /// <summary>
        /// 
        /// </summary>
        public HashSet<Control> ControlsToMove = new HashSet<Control>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN &&
                 ControlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        private const int LEFTEXTENDWIDTH = 1;
        /// <summary>
        /// 
        /// </summary>
        private const int RIGHTEXTENDWIDTH = 1;
        /// <summary>
        /// 
        /// </summary>
        private const int BOTTOMEXTENDWIDTH = 1;
        /// <summary>
        /// 
        /// </summary>
        private int TOPEXTENDWIDTH = 35;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(
                e.Graphics, this.DisplayRectangle,
                mFormBorderColor, LEFTEXTENDWIDTH, ButtonBorderStyle.Solid,
                mFormBorderColor, TOPEXTENDWIDTH, ButtonBorderStyle.Solid,
                mFormBorderColor, RIGHTEXTENDWIDTH, ButtonBorderStyle.Solid,
                mFormBorderColor, BOTTOMEXTENDWIDTH, ButtonBorderStyle.Solid);

            if (FormCaptionPanel != null)
                FormCaptionPanel.Height = TOPEXTENDWIDTH;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SuspendLayout();
            ResumeLayout();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            FormBorderColor = mInitialBorderColor;
            CaptionForeColor = mInitialCaptionForeColor;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            FormBorderColor = mLostFocusBorderColor;
            CaptionForeColor = mLostFocusCaptionForeColor;
        }
        #endregion

        #region Overrided and hidden attributes
        private readonly FormBorderStyle mFormBorderStyle = FormBorderStyle.None;
        private bool mMaximizeBox = true;
        private bool mMinimizeBox = true;
        public new FormBorderStyle FormBorderStyle
        {
            get { return mFormBorderStyle; }

        }
        /// <summary>
        /// 
        /// </summary>
        public new string Text
        {
            get
            {
                return CaptionTextLabel.Text;
            }
            set
            {
                CaptionTextLabel.Text = value;
                base.Text = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public new bool ShowIcon
        {
            get { return CaptionImagePanel.Visible; }
            set
            {
                CaptionImagePanel.Visible = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(true)]
        public new bool MinimizeBox
        {
            get
            {
                if (FormCaptionPanel != null)
                    return GetControlBoxButtonVisible(ControlBoxButtonAction.Minimize);
                return mMinimizeBox;
            }
            set
            {
                if (FormCaptionPanel != null)
                    SetControlBoxButtonVisible(ControlBoxButtonAction.Minimize, value);
                mMinimizeBox = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(true)]
        public new bool MaximizeBox
        {
            get
            {
                if (FormCaptionPanel != null)
                    return GetControlBoxButtonVisible(ControlBoxButtonAction.Maximize);
                return mMaximizeBox;
            }
            set
            {
                if (FormCaptionPanel != null)
                    SetControlBoxButtonVisible(ControlBoxButtonAction.Maximize, value);
                mMaximizeBox = value;
            }
        }
        #endregion

        #region New attributes
        /// <summary>
        /// 
        /// </summary>
        internal bool IsInDesignMode
        {
            get
            {
                return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Panel FormCaptionPanel = new Panel();
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private FlowLayoutPanel ImageAndTextPanel = new FlowLayoutPanel();
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Panel CaptionImagePanel = new Panel();
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Label CaptionTextLabel = new Label();
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mFormBorderColor = SystemColors.ActiveCaption;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mCaptionForeColor = SystemColors.ActiveCaptionText;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mLostFocusBorderColor = SystemColors.InactiveCaption;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mLostFocusCaptionForeColor = SystemColors.InactiveCaptionText;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mInitialBorderColor;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Color mInitialCaptionForeColor;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private double mFormDragOpacity = 0.5;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private double mInitialFormDragOpacity;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private bool mEnableFormDragOpacity = false;
        /// <summary>
        /// 
        /// </summary>
        [Category("Appearance")]
        public Color FormBorderColor
        {
            get { return mFormBorderColor; }
            set
            {
                mFormBorderColor = value;
                Invalidate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("Appearance")]
        public Color LostFocusBorderColor
        {
            get { return mLostFocusBorderColor; }
            set { mLostFocusBorderColor = value; Invalidate(); }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("Appearance")]
        public Color LostFocusCaptionForeColor
        {
            get { return mLostFocusCaptionForeColor; }
            set { mLostFocusCaptionForeColor = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("Appearance")]
        public Color CaptionForeColor
        {
            get { return mCaptionForeColor; }
            set
            {
                mCaptionForeColor = value;
                CaptionTextLabel.ForeColor = mCaptionForeColor;
            }
        }
        [Category("Appearance")]
        public int CaptionHeight
        {
            get { return TOPEXTENDWIDTH; }
            set { TOPEXTENDWIDTH = value; Invalidate(); }
        }
        /// <summary>
        ///
        /// </summary>
        [Category("Misc")]
        public double FormDragOpacity
        {
            get { return mFormDragOpacity; }
            set { mFormDragOpacity = value; }
        }
        /// <summary>
        ///
        /// </summary>
        [Category("Misc")]
        public bool EnableFormDragOpacity
        {
            get { return mEnableFormDragOpacity; }
            set { mEnableFormDragOpacity = value; }
        }
        #endregion

        #region Control Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            // Do not apply location laws for the Caption.
            // Let it dock to the form's default client zones.
            if (e.Control.Equals(FormCaptionPanel))
                return;

            base.OnControlAdded(e);
            e.Control.HandleCreated += Control_HandleCreated;
        }
        /// <summary>
        /// Handler declared in OnControlAdded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_HandleCreated(object sender, EventArgs e)
        {
            Control ctl = sender as Control;

            ControlLocation(ctl);
            CheckDocking(ctl);
            ctl.LocationChanged += Ctl_LocationChanged;
            ctl.DockChanged += Ctl_DockChanged;
            ctl.Move += Ctl_Move;
        }
        /// <summary>
        /// Handler declared in Control_HandleCreated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ctl_Move(object sender, EventArgs e)
        {
            ControlLocation(sender as Control);
        }

        /// <summary>
        /// Handler declared in Control_HandleCreated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ctl_DockChanged(object sender, EventArgs e)
        {
            Control ctl = sender as Control;
            CheckDocking(ctl);

        }

        /// <summary>
        /// Handler declared in Control_HandleCreated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ctl_LocationChanged(object sender, EventArgs e)
        {
            Control ctl = sender as Control;
            ControlLocation(ctl);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Initialized the caption and some defauts
        /// </summary>
        private void InitializeDefaults()
        {
            InitializeCaption();
            m_aeroEnabled = false;
            this.mInitialBorderColor = this.mFormBorderColor;
            this.mInitialCaptionForeColor = this.mCaptionForeColor;
            this.mInitialFormDragOpacity = this.Opacity;
            ControlsToMove.Add(FormCaptionPanel);
            ControlsToMove.Add(ImageAndTextPanel);
            ControlsToMove.Add(CaptionTextLabel);
        }

        /// <summary>
        /// Assures the control stays within defined bounds
        /// </summary>
        /// <param name="ctl"></param>
        private void ControlLocation(Control ctl)
        {
            ctl.LocationChanged -= Ctl_LocationChanged;


            if (ctl.Location.X < LEFTEXTENDWIDTH)
                ctl.Location = new Point(LEFTEXTENDWIDTH, ctl.Location.Y);

            if (ctl.Location.X > RIGHTEXTENDWIDTH + Width - ctl.Width)
                ctl.Location = new Point(Width - RIGHTEXTENDWIDTH - ctl.Width, ctl.Location.Y);

            if (ctl.Location.Y < TOPEXTENDWIDTH)
                ctl.Location = new Point(ctl.Location.X, TOPEXTENDWIDTH);

            if (ctl.Location.Y > BOTTOMEXTENDWIDTH + Height - ctl.Height)
                ctl.Location = new Point(ctl.Location.X, Height - BOTTOMEXTENDWIDTH - ctl.Height);


            ctl.LocationChanged += Ctl_LocationChanged;

            CheckDocking(ctl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void ControlFillSize(Control ctl)
        {
            Size size = new Size
            {
                Height = Height - TOPEXTENDWIDTH - BOTTOMEXTENDWIDTH,
                Width = Width - LEFTEXTENDWIDTH - RIGHTEXTENDWIDTH
            };
            ctl.Size = size;
            ctl.LocationChanged -= Ctl_LocationChanged;
            ctl.Location = new Point(LEFTEXTENDWIDTH, TOPEXTENDWIDTH);
            ctl.LocationChanged += Ctl_LocationChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void CheckDocking(Control ctl)
        {
            if (ctl.Dock == DockStyle.Left)
                DockLeft(ctl);
            if (ctl.Dock == DockStyle.Top)
                DockTop(ctl);
            if (ctl.Dock == DockStyle.Right)
                DockRight(ctl);
            if (ctl.Dock == DockStyle.Bottom)
                DockBottom(ctl);
            if (ctl.Dock == DockStyle.Fill)
                DockFill(ctl);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void DockLeft(Control ctl)
        {
            ctl.Dock = DockStyle.None;
            ctl.Width = ctl.PreferredSize.Width;
            ctl.Height = Height - TOPEXTENDWIDTH - BOTTOMEXTENDWIDTH;
            ctl.Location = new Point(LEFTEXTENDWIDTH, TOPEXTENDWIDTH);
            ctl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void DockRight(Control ctl)
        {
            ctl.Dock = DockStyle.None;
            ctl.Width = ctl.PreferredSize.Width;
            ctl.Height = Height - TOPEXTENDWIDTH - BOTTOMEXTENDWIDTH;
            ctl.Location = new Point(Width - ctl.Width - RIGHTEXTENDWIDTH, TOPEXTENDWIDTH);
            ctl.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void DockTop(Control ctl)
        {
            ctl.Dock = DockStyle.None;
            ctl.Width = Width - RIGHTEXTENDWIDTH - LEFTEXTENDWIDTH;
            ctl.Height = ctl.PreferredSize.Height;
            ctl.Location = new Point(LEFTEXTENDWIDTH, TOPEXTENDWIDTH);
            ctl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void DockBottom(Control ctl)
        {
            ctl.Dock = DockStyle.None;
            ctl.Width = Width - RIGHTEXTENDWIDTH - LEFTEXTENDWIDTH;
            ctl.Height = ctl.PreferredSize.Height;
            ctl.Location = new Point(RIGHTEXTENDWIDTH, Height - TOPEXTENDWIDTH - ctl.Height);
            ctl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctl"></param>
        private void DockFill(Control ctl)
        {
            ctl.Dock = DockStyle.None;
            ctl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            ControlFillSize(ctl);
        }
        #endregion

        #region Caption Methods
        private void InitializeCaption(String name = "FormCaptionPanel")
        {
            ///
            /// Caption Panel
            ///
            FormCaptionPanel.Name = name;
            FormCaptionPanel.BackColor = Color.Transparent;
            FormCaptionPanel.Dock = DockStyle.Top;
            FormCaptionPanel.Height = TOPEXTENDWIDTH;
            FormCaptionPanel.Margin = new Padding(0);
            FormCaptionPanel.Paint += (s, e) =>
            FormCaptionPanel.BackColor = Color.Transparent;
            ///
            /// Append global FormCaptionPanel (a Panel) into Form
            ///
            Controls.Add(FormCaptionPanel);
            ///
            /// Control Box
            ///
            FlowLayoutPanel pControlBox = new FlowLayoutPanel
            {
                Name = "ControlBox",
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = FormCaptionPanel.BackColor,
                Margin = new Padding(0),
                Padding = new Padding(0),
                Dock = DockStyle.Right
            };

            int yPadding = RIGHTEXTENDWIDTH;
            int subHeight = yPadding * 2;
            int height = FormCaptionPanel.Height - subHeight;
            int width = FormCaptionPanel.Height + 10;

            ControlBoxButton btnExit = new ControlBoxButton()
            {
                ButtonAction = ControlBoxButtonAction.Exit,
                BackColor = FormCaptionPanel.BackColor,
                Margin = new Padding(0, yPadding, RIGHTEXTENDWIDTH, yPadding),
                Height = height,
                Width = width
            };
            ControlBoxButton btnMin = new ControlBoxButton()
            {
                ButtonAction = ControlBoxButtonAction.Minimize,
                BackColor = FormCaptionPanel.BackColor,
                Margin = new Padding(0, yPadding, 0, yPadding),
                Height = height,
                Width = width,
                Visible = mMinimizeBox
            };
            ControlBoxButton btnMax = new ControlBoxButton()
            {
                ButtonAction = ControlBoxButtonAction.Maximize,
                BackColor = FormCaptionPanel.BackColor,
                Margin = new Padding(0, yPadding, 0, yPadding),
                Height = height,
                Width = width,
                Visible = mMaximizeBox
            };
            pControlBox.Controls.Add(btnMin);
            pControlBox.Controls.Add(btnMax);
            pControlBox.Controls.Add(btnExit);
            ///
            /// Caption Text and Image (FlowLayoutPanel)
            ///
            ImageAndTextPanel.Name = "ImageAndTextPanel";
            ImageAndTextPanel.FlowDirection = FlowDirection.LeftToRight;
            ImageAndTextPanel.WrapContents = false;
            ImageAndTextPanel.Margin = new Padding(0);
            ImageAndTextPanel.Padding = new Padding(0);
            ImageAndTextPanel.AutoSize = true;
            ImageAndTextPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ImageAndTextPanel.Dock = DockStyle.Fill;
            ImageAndTextPanel.BackColor = FormCaptionPanel.BackColor;
            ///
            /// Caption Image (Panel)
            ///
            CaptionImagePanel.Name = "CaptionImagePanel";
            CaptionImagePanel.BackgroundImage = Icon.ToBitmap().ResizeImage(25);
            CaptionImagePanel.BackgroundImageLayout = ImageLayout.Center;
            CaptionImagePanel.Height = FormCaptionPanel.Height;
            CaptionImagePanel.Width = FormCaptionPanel.Height;
            CaptionImagePanel.Margin = new Padding(0);
            CaptionImagePanel.Padding = new Padding(0);
            CaptionImagePanel.BackColor = FormCaptionPanel.BackColor;
            ///
            /// Caption Text (Label)
            ///
            CaptionTextLabel.Name = "CaptionTextLabel";
            CaptionTextLabel.Text = this.Text;
            CaptionTextLabel.TextAlign = ContentAlignment.MiddleLeft;
            CaptionTextLabel.Font = new Font(SystemFonts.CaptionFont.FontFamily.Name, 9.75F, FontStyle.Regular);
            CaptionTextLabel.AutoSize = false;
            CaptionTextLabel.AutoEllipsis = true;
            CaptionTextLabel.Width = 800;
            CaptionTextLabel.Height = FormCaptionPanel.Height;
            CaptionTextLabel.Margin = new Padding(0);
            CaptionTextLabel.Padding = new Padding(5, 0, 0, 0);
            CaptionTextLabel.BackColor = FormCaptionPanel.BackColor;
            CaptionTextLabel.ForeColor = mCaptionForeColor;
            ///
            /// Append CaptionImagePanel (a Panel) and CaptionTextLabel (a Label) 
            /// into ImageAndTextPanel (a FlowLayoutPanel)
            ///
            ImageAndTextPanel.Controls.Add(CaptionImagePanel);
            ImageAndTextPanel.Controls.Add(CaptionTextLabel);
            ///
            /// Append local pControlBox (a FlowLayoutPanel) and ImageAndTextPanel(a FlowLayoutPanel) 
            /// into FormCaptionPanel (a Panel)
            ///
            FormCaptionPanel.Controls.Add(pControlBox);
            FormCaptionPanel.Controls.Add(ImageAndTextPanel);
            ///
            /// Re-Apply contained item's height on resize
            ///
            FormCaptionPanel.Resize += (s, e) =>
            {
                CaptionImagePanel.Height = FormCaptionPanel.Height;
                CaptionTextLabel.Height = FormCaptionPanel.Height;
                ImageAndTextPanel.Height = FormCaptionPanel.Height;

                int _subHeight = yPadding * 2;
                int _height = FormCaptionPanel.Height - subHeight;
                int _width = FormCaptionPanel.Height + 10;

                btnExit.Width = _width;
                btnMax.Width = _width;
                btnMin.Width = _width;
                btnExit.Height = _height;
                btnMax.Height = _height;
                btnMin.Height = _height;
            };


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlBoxButton"></param>
        internal void SetControlBoxButtonVisible(ControlBoxButtonAction controlBoxButton, bool visible)
        {
            try
            {
                FlowLayoutPanel controlbox = FormCaptionPanel.Controls["ControlBox"] as FlowLayoutPanel;
                ControlBoxButton[] buttons = controlbox.Controls.Cast<ControlBoxButton>().ToArray();
                foreach (ControlBoxButton cbb in buttons)
                    if (cbb.ButtonAction == controlBoxButton)
                        cbb.Visible = visible;
            }
            catch { }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="controlBoxButton"></param>
        /// <returns></returns>
        internal bool GetControlBoxButtonVisible(ControlBoxButtonAction controlBoxButton)
        {
            try
            {
                FlowLayoutPanel controlbox = FormCaptionPanel.Controls["ControlBox"] as FlowLayoutPanel;
                ControlBoxButton[] buttons = controlbox.Controls.Cast<ControlBoxButton>().ToArray();

                foreach (ControlBoxButton cbb in buttons)
                    if (cbb.ButtonAction == controlBoxButton)
                        return cbb.Visible;

                throw (new ArgumentException("No such button type is known or existing in the control box"));
            }
            catch
            {
                if (controlBoxButton == ControlBoxButtonAction.Maximize)
                    return mMaximizeBox;
                if (controlBoxButton == ControlBoxButtonAction.Minimize)
                    return mMinimizeBox;

                throw (new ArgumentException("No such button type is known or existing in the control box"));
            }
        }
        #endregion

        private void InitializeComponent()
        {
            InitializeDefaults();
        }
    }
}
