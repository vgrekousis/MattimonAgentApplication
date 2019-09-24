using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattimonAgentApplication.GUI.Extensions;
namespace MattimonAgentApplication
{
    public partial class FormError : Form, IMessageFilter
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_LBUTTONDOWN = 0x0201;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN &&
                 ControlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                return true;
            }
            return false;
        }
        public HashSet<Control> ControlsToMove = new HashSet<Control>();
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
        public FormError(String title, String message)
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            Icon = MattimonAgentApplication.Properties.Resources.MattimonIcon;
            Text = title;
            TopMost = true;
            label2.Text = title;
            label1.Text = message;
            label1.TextAlign = ContentAlignment.TopLeft;
            label2.Image = SystemIcons.Error.ToBitmap().ResizeImage(25,25);
            label2.ImageAlign = ContentAlignment.MiddleLeft;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ControlsToMove.Add(label2);
            //btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            Width = panel1.Width;
            
            btnOK.Location = new Point(Width - btnOK.Width - 5, panel1.Location.Y + panel1.Height + 5);
            Height += btnOK.Height + label1.Padding.Bottom + 5;
            MaximumSize = Size;
            MinimumSize = Size;
            System.Media.SystemSounds.Hand.Play();
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        public new void Show(IWin32Window owner)
        {
            CenterToParent();
            ShowDialog(owner);
        }

        public new void Show()
        {
            CenterToScreen();
            ShowDialog();
        }
    }
}
