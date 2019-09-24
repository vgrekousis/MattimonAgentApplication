using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Install
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WaitCursor : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private Cursor m_oldCursor = null;





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
