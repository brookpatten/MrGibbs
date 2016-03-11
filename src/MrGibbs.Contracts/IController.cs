using MrGibbs.Models;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// bouy racing specific logic controller
    /// </summary>
    public interface IRaceController
    {
        /// <summary>
        /// performs the contextually relevant stopwatch action
        /// </summary>
        void CountdownAction();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        void SetCourseType(int index);
        /// <summary>
        /// Tell the controller about a mark via the current State location
        /// </summary>
        /// <param name="markType">The type of mark that is at the present location</param>
        void SetMarkLocation(MarkType markType);
        /// <summary>
        /// Tell the controller about a mark at a given bearing from the current location
        /// Approximate location of the mark will be estimated using great circle intersection
        /// from the most recent 2 mark bearings of matching type
        /// </summary>
        /// <param name="markType">the type of mark to which the bearing is being taken</param>
        /// <param name="bearing">the bearing to the mark from the present (in state) location</param>
        /// <param name="magneticBearing">whether the bearing is magnetic or absolute</param>
        void SetMarkBearing(MarkType markType, double bearing, bool magneticBearing);
        void ClearMark(int markIndex);
        void NewRace();
        /// <summary>
        /// attempts to determine if the current location is within the pre-set distance to the current target mark
        /// if it is, we advance the target mark to the next mark
        /// </summary>
        void ProcessMarkRoundings();
        /// <summary>
        /// used to manually advance the target mark to the next mark in the course, should the auto-detection
        /// distance fail to do so.
        /// </summary>
        void NextMark();
        /// <summary>
        /// the current state of the race, boat etc
        /// </summary>
        State State { get; }
    }

    /// <summary>
    /// core system functions
    /// </summary>
    public interface ISystemController
    {
        /// <summary>
        /// invokes the calibrate method on all sensors
        /// </summary>
        void Calibrate();
        /// <summary>
        /// instructs the supervisor to exit the main thread and restart
        /// </summary>
        void Restart();
        /// <summary>
        /// reboot the entire system (OS and all)
        /// </summary>
        void Reboot();
		/// <summary>
		/// Exit the app
		/// </summary>
		void Exit ();
        /// <summary>
        /// shutdown the entire system (OS and all)
        /// </summary>
        void Shutdown();
    }
}
