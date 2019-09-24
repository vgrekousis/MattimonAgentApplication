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
using MattimonAgentApplication.GUI.BitscoreForms;

namespace MattimonAgentApplication.GUI.BitscoreForms
{
    internal partial class InternalMessageBox : BitscoreForm
    {
        
        private System.Media.SystemSound mSysSound;

        public InternalMessageBox() : base()
        {
            InitializeComponent();
            BackColor = Color.WhiteSmoke;
            EnableFormDragOpacity = true;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            foreach (Button b in fpButtons.Controls.Cast<Button>())
                b.Click += MessageBoxButton_Click;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        internal DialogResult Show(IWin32Window owner, String message, String title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Text = title;
            label1.Text = message;
            SetIcon(icon);
            SetButtons(buttons);
            StartPosition = FormStartPosition.CenterParent;
            if (mSysSound != null) mSysSound.Play();
            ShowDialog(owner);
            return DialogResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        internal DialogResult Show(String message, String title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Text = title;
            label1.Text = message;
            SetIcon(icon);
            SetButtons(buttons);
            StartPosition = FormStartPosition.CenterScreen;
            if (mSysSound != null) mSysSound.Play();
            ShowDialog();
            return DialogResult;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            MaximumSize = new Size(0, 0);
            panel1.AutoScroll = false;
            base.OnHandleCreated(e);
            label1.MaximumSize = new Size(0, 0);
            label1.Padding = new Padding(4, 14, 4, 14);

            // DO NOT MODIFY //
            label1.MaximumSize = new Size(PreferredSize.Width, 0);
            Height = (CaptionHeight + panel1.Height + fpButtons.Height + 2);
            Width = (panel1.Width + 5);
            fpButtons.Location = new Point(999, 999);
            // END //
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        private void SetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    Icon = SystemIcons.Error;
                    mSysSound = System.Media.SystemSounds.Hand;
                    break;
                case MessageBoxIcon.Information:
                    Icon = SystemIcons.Information;
                    mSysSound = System.Media.SystemSounds.Asterisk;
                    break;
                case MessageBoxIcon.None:
                    ShowIcon = false;
                    break;
                case MessageBoxIcon.Question:
                    Icon = SystemIcons.Question;
                    mSysSound = System.Media.SystemSounds.Question;
                    break;
                case MessageBoxIcon.Warning:
                    Icon = SystemIcons.Warning;
                    mSysSound = System.Media.SystemSounds.Exclamation;
                    break;
                default:
                    throw new ArgumentException("Invalid icon option especified for " + this.GetType().ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttons"></param>
        private void SetButtons(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    btnAbort.Visible = true;
                    btnRetry.Visible = true;
                    btnIgnore.Visible = true;
                    break;
                case MessageBoxButtons.OK:
                    btnOK.Visible = true;
                    break;
                case MessageBoxButtons.OKCancel:
                    btnOK.Visible = true;
                    btnCancel.Visible = true;
                    break;
                case MessageBoxButtons.RetryCancel:
                    btnRetry.Visible = true;
                    btnCancel.Visible = true;
                    break;
                case MessageBoxButtons.YesNo:
                    btnYes.Visible = true;
                    btnNo.Visible = true;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    btnYes.Visible = true;
                    btnNo.Visible = true;
                    btnCancel.Visible = true;
                    break;
                default:
                    throw new ArgumentException("Invalid button option specified for " + this.GetType().ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageBoxButton_Click(object sender, EventArgs e)
        {
            if (sender == btnAbort)
                DialogResult = DialogResult.Abort;
            else if (sender == btnCancel)
                DialogResult = DialogResult.Cancel;
            else if (sender == btnIgnore)
                DialogResult = DialogResult.Ignore;
            else if (sender == btnNo)
                DialogResult = DialogResult.No;
            else if (sender == btnOK)
                DialogResult = DialogResult.OK;
            else if (sender == btnRetry)
                DialogResult = DialogResult.Retry;
            else if (sender == btnYes)
                DialogResult = DialogResult.Yes;
            else
                DialogResult = DialogResult.None;
            Close();
        }
    }

    public class GrowLabel : Label
    {
        private bool mGrowing;
        public GrowLabel()
        {
            this.AutoSize = false;
        }
        private void ResizeLabel()
        {
            if (mGrowing)
                return;
            try
            {
                mGrowing = true;
                Size sz = new Size(this.Width, Int32.MaxValue);
                sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
                this.Height = sz.Height;
            }
            finally
            {
                mGrowing = false;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ResizeLabel();
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ResizeLabel();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ResizeLabel();
        }
    }
}
