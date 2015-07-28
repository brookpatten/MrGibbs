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
        void SetMarkLocation(int markIndex);
        void SetMarkBearing(int markIndex, double bearing);
        void ClearMark(int markIndex);
        void NewRace();

        State State { get; }
    }

    public interface ISystemController
    {
        void Calibrate();
        void Restart();
        void Reboot();
    }
}
