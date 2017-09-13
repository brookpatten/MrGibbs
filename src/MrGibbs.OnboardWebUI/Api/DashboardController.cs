using System;
using System.Web.Http;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.OnboardWebUI.Api
{
    class DashboardController
    {
        private ILogger _logger;
        private Action<Action<ISystemController, IRaceController>> _queueCommand;
        private State _state;
        public DashboardController(ILogger logger, Action<Action<ISystemController, IRaceController>> queueCommand,State state)
        {
            _logger = logger;
            _queueCommand = queueCommand;
            _state = state;
        }

        [HttpGet]
        [Route("api/v1/state")]
        public StateLite State()
        {
            var state = new StateLite();
            state.Countdown = _state.Countdown;
            state.RaceStarted = _state.RaceStarted;
            state.StateValues = ImportEntries(_state.StateValues
                , StateValue.SpeedInKnots
                , StateValue.CourseOverGroundDirection
                , StateValue.VelocityMadeGood
                , StateValue.VelocityMadeGoodOnCourse
                , StateValue.VelocityMadeGoodOnCoursePercent
                , StateValue.VelocityMadeGoodPercent);

            return state;
        }

        private Dictionary<StateValue, double> ImportEntries(IDictionary<StateValue,double> from,params StateValue[] stateValues)
        {
            var entries = new Dictionary<StateValue, double>();
            foreach(var s in stateValues)
            {
                if(from.ContainsKey(s))
                {
                    entries.Add(s, from[s]);
                }
            }
            return entries;
        }

        [HttpPost]
        [Route("api/v1/countdown")]
        public object Countdown()
        {
            _queueCommand((s, r) => r.CountdownAction());
            return 200;
        }

        [HttpPost]
        [Route("api/v1/mark/next")]
        public object NextMark()
        {
            _queueCommand((s, r) => r.NextMark());
            return 200;
        }

        [HttpPost]
        [Route("api/v1/set-location")]
        public object SetMarkLocation(MarkType markType)
        {
            _queueCommand((s, r) => r.SetMarkLocation(markType));
            return 200;
        }

        /// <summary>
		/// Given projected gps points, converts to a simple set of coordinates that can be 
		/// rendered as an image on the pebble
		/// </summary>
		/// <returns>The course.</returns>
		/// <param name="windward">Windward.</param>
		/// <param name="leeward">Leeward.</param>
		/// <param name="boat">Boat.</param>
		private void PrerenderCourse(Vector2 windward, Vector2 leeward, Vector2 boat)
        {
            //size of the pebble screen
            Vector2 targetSize = new Vector2() { X = 100, Y = 100 };

            //center everything on the origin by shifting it by the center of the course
            Vector2 courseCenter = new Vector2()
            {
                X = (windward.X + leeward.X) / 2f,
                Y = (windward.Y + leeward.Y) / 2f
            };
            windward.X = windward.X - courseCenter.X;
            windward.Y = windward.Y - courseCenter.Y;
            leeward.X = leeward.X - courseCenter.X;
            leeward.Y = leeward.Y - courseCenter.Y;
            boat.X = boat.X - courseCenter.X;
            boat.Y = boat.Y - courseCenter.Y;

            //rotate such that windward is at the top and leeward is at the bottom
            var windwardPolar = windward.CartesianToPolar();
            var theta = windwardPolar.Theta - ((float)Math.PI / 2f);

            var boatPolar = boat.CartesianToPolar();
            boatPolar.Theta = boatPolar.Theta - theta;
            boat = boatPolar.PolarToCartesian();

            windwardPolar.Theta = windwardPolar.Theta - theta;
            windward = windwardPolar.PolarToCartesian();

            var leewardPolar = leeward.CartesianToPolar();
            leewardPolar.Theta = leewardPolar.Theta - theta;
            leeward = leewardPolar.PolarToCartesian();

            //at this point the course is centered over the origin
            //with the rhumb line on the y axis
            //scale everything down to fit on the desired size
            float currentSize = Math.Abs(windward.Y - leeward.Y);
            float scaleFactor = targetSize.Y / currentSize;

            boat.X = boat.X * scaleFactor;
            boat.Y = boat.Y * scaleFactor;

            windward.X = windward.X * scaleFactor;
            windward.Y = windward.Y * scaleFactor;

            leeward.X = leeward.X * scaleFactor;
            leeward.Y = leeward.Y * scaleFactor;


            //shift the whole thing into the TR quadrant and flip it over the x axis
            //so it matches drawing coords
            Vector2 shift = new Vector2()
            {
                X = targetSize.X / 2f,
                Y = -(targetSize.Y / 2f)
            };
            boat.X = boat.X + shift.X;
            boat.Y = Math.Abs(boat.Y + shift.Y);
            windward.X = windward.X + shift.X;
            windward.Y = Math.Abs(windward.Y + shift.Y);
            leeward.X = leeward.X + shift.X;
            leeward.Y = Math.Abs(leeward.Y + shift.Y);

            //return these.... or draw an image.... or something useful
        }
    }
}
