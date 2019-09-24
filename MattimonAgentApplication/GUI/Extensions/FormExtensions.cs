using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MattimonAgentApplication.GUI.Extensions
{
    public static class FormExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootControl"></param>
        /// <returns></returns>
        private static List<Control> controls = new List<Control>();
        /// <summary>
        /// 
        /// </summary>
        private static List<Control> tmpctls = new List<Control>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootControl"></param>
        /// <returns></returns>
        public static List<Control> LoopControls(this Control rootControl)
        {
            foreach (Control c in rootControl.Controls)
            {
                if (c.HasChildren)
                {
                    LoopControls(c);
                }

                controls.Add(c);
            }

            tmpctls = controls;

            //controls.Clear();
            return tmpctls;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> en, Action<T> action)
        {
            foreach (var obj in en) action(obj);
        }
    }
}
