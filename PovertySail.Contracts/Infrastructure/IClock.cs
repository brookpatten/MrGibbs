using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts.Infrastructure
{
    public interface IClock
    {
        DateTime GetUtcTime();
        DateTime GetLocalTime(TimeZoneInfo timeZone);
    }
}
