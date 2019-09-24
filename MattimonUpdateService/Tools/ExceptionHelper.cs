using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonUpdateService.Tools
{
    public class ExceptionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="exceptio"></param>
        /// <returns></returns>
        public static string GetFormatedExceptionMessage(Exception e, StringBuilder extraMessage = null)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(e.GetType().Name);
            message.AppendLine(new String('-', e.GetType().Name.Length));
            message.AppendLine("Message:");
            message.AppendLine(e.Message);
            message.AppendLine("Stack Trace:");
            message.AppendLine(e.StackTrace);

            if (e.InnerException != null)
            {
                message.AppendLine("Inner Exception");
                message.AppendLine("---------------");
                message.AppendLine("Inner Exception Message:");
                message.AppendLine();
                message.AppendLine(e.InnerException.Message);
                message.AppendLine("Inner Exception Stack Trace:");
                message.AppendLine();
                message.AppendLine(e.InnerException.StackTrace);
            }

            if (extraMessage != null)
            {
                message.AppendLine("Extra Message");
                message.AppendLine("-------------");
                message.AppendLine(extraMessage.ToString());
            }
            return message.ToString();
        }
    }
}
