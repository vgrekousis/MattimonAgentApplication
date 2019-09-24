using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication
{
    public partial class FormBackgroundWorker : Form
    {
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

        private IWin32Window owner;
        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker backgroundWorker;
        /// <summary>
        /// 
        /// </summary>
        private RunWorkerCompletedEventHandler runWorkerCompletedEventHandler = null;
        /// <summary>
        /// 
        /// </summary>
        public BackgroundWorker BackgroundWorker
        {
            get { return backgroundWorker; }
        }
        /// <summary>
        /// 
        /// </summary>
        public RunWorkerCompletedEventHandler RunWorkerCompletedEventHandler
        {
            get { return runWorkerCompletedEventHandler; }
            set { runWorkerCompletedEventHandler += value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private DoWorkEventHandler callback;
        /// <summary>
        /// 
        /// </summary>
        private Image animatedImage;

        /// <summary>
        /// 
        /// </summary>
        public FormBackgroundWorker(IWin32Window owner, DoWorkEventHandler callback, String message)
        {
            InitializeComponent();
            this.owner = owner;
            this.callback = callback;
            lblMessage.Text = message;
            animatedImage = MattimonAgentApplication.Properties.Resources.loading_large;
            MattimonAgentLibrary.Tools.GUITools.CenterControlInParent(lblMessage, false);
        }


        public new void Show()
        {
            ShowDialog(owner);
        }


        public void SetMessage(String message)
        {
            try
            {
                MattimonAgentLibrary.Tools.GUITools.SetControlPropertyThreadSafe(lblMessage, "Text", message);
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void NewDoWorkEventHander(DoWorkEventHandler callback)
        {
            this.callback = callback;
        }




        /// <summary>
        /// 
        /// </summary>
        public void DoWork()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork -= this.callback;
            backgroundWorker.DoWork += callback;
            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                try
                {
                    Close();
                }
                catch { }
            };

            if (runWorkerCompletedEventHandler != null)
                backgroundWorker.RunWorkerCompleted += runWorkerCompletedEventHandler;

            backgroundWorker.RunWorkerAsync();

            if (owner == null)
                StartPosition = FormStartPosition.CenterScreen;
            ShowDialog(owner);
        }





        /// <summary>
        /// To use if the DoWork handler (method) needs to terminate before it actually terminates
        /// </summary>
        public void CancelAsyncClose()
        {
            try
            {
                backgroundWorker.CancelAsync();
                this.Invoke((MethodInvoker)delegate
                {
                    // close the form on the forms thread
                    this.Close();
                });
            }
            catch { }
        }

        public void CancelAsync()
        {
            backgroundWorker.CancelAsync();
        }

        #region Amination
        /// <summary>
        /// 
        /// </summary>
        bool currentlyAnimating = false;





        /// <summary>
        /// 
        /// </summary>
        private void AnimateImage()
        {
            if (!currentlyAnimating)
            {
                //Begin the animation only once.
                ImageAnimator.Animate(animatedImage, new EventHandler(this.OnFrameChanged));
                currentlyAnimating = true;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnFrameChanged(object o, EventArgs e)
        {
            //Force a call to the Paint event handler.
            this.Invalidate();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Begin the animation.
            AnimateImage();
            //Get the next frame ready for rendering.
            ImageAnimator.UpdateFrames();
            //Draw the next frame in the animation.
            e.Graphics.DrawImage(this.animatedImage,
                (this.Width - 50) / 2,
                ((this.Height + lblMessage.Height) - 50) / 2, 50, 50);
        }
        #endregion
    }






    /// <summary>
    /// 
    /// </summary>
    public sealed class WaitCursor : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Cursor m_oldCursor;





        /// <summary>
        ///
        /// </summary>
        public WaitCursor()
        {
            m_oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }





        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Cursor.Current = m_oldCursor;
        }
    }
}
