using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentLibrary.Tools
{
    public class DateUtils
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static double GetUnixTimestamp()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetUnixTimestamp(DateTime dt)
        {
            return ((Int32)(dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ConvertToUnixTime(DateTime dateTime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return (long)(dateTime - sTime).TotalSeconds;
        }
    }
}
