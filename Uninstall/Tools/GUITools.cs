using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uninstall.Tools
{
    public class GUITools
    {
        public static void CenterControlInParent(Control ctrlToCenter, Boolean applytop = true)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;

            if (applytop)
                ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }
        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Red);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }


        private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);
        public static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            try
            {
                if (control != null)
                {
                    if (control.InvokeRequired)
                    {
                        control.Invoke(new SetControlPropertyThreadSafeDelegate
                        (SetControlPropertyThreadSafe),
                        new object[] { control, propertyName, propertyValue });
                    }
                    else
                    {
                        control.GetType().InvokeMember(
                            propertyName,
                            BindingFlags.SetProperty,
                            null,
                            control,
                            new object[] { propertyValue });
                    }
                }
            }
            catch (Exception) { }
            finally { }
        }
        private delegate void SetToolStripDelegate(ToolStripLabel label, string text);
        private static void SetToolStrip(ToolStripLabel label, string text)
        {
            label.Text = text;
        }

        public static void SetToolLabelStipTextThreadSafe(Form container, ToolStripLabel label, String text)
        {
            try
            {
                container.Invoke(new SetToolStripDelegate(SetToolStrip), label, text);
            }
            catch { }
        }
    }
}
