using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Infrastructure
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
