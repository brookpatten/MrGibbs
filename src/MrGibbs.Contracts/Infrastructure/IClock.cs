using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Contracts.Infrastructure
{
    public interface IClock
    {
        DateTime GetUtcTime();
        DateTime GetLocalTime(TimeZoneInfo timeZone);
        void SetUtcTime(DateTime time);
        DateTime? LastSetAt { get; }
    }
}
