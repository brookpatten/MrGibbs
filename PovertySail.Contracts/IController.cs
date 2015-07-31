using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Models;

namespace PovertySail.Contracts
{
    public interface IRaceController
    {
        void CountdownAction();

        void SetCourseType(int index);
        void SetMarkLocation(MarkType markType);
        void SetMarkBearing(MarkType markType, double bearing, bool magneticBearing);
        void ClearMark(int markIndex);
        void NewRace();

        void ProcessMarkRoundings();
        void NextMark();

        State State { get; }
    }

    public interface ISystemController
    {
        void Calibrate();
        void Restart();
        void Reboot();
        void Shutdown();
    }
}
