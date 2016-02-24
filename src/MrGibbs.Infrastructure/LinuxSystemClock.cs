using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Infrastructure
{
    struct rtc_time
    {
        int tm_sec;
        int tm_min;
        int tm_hour;
        int tm_mday;
        int tm_mon;
        int tm_year;
        int tm_wday; /* unused */
        int tm_yday; /* unused */
        int tm_isdst;/* unused */
    };

    /// <summary>
    /// intended to get/set time on a posix system, not implemented
    /// </summary>
    public class LinuxSystemClock:IClock
    {
        public DateTime GetUtcTime()
        {
            return DateTime.UtcNow;
        }

        public DateTime GetLocalTime(TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(GetUtcTime(), timeZone);
        }

        public void SetUtcTime(DateTime time)
        {
            throw new NotImplementedException();
        }

        public DateTime? LastSetAt
        {
            get { throw new NotImplementedException(); }
        }
    }
}
