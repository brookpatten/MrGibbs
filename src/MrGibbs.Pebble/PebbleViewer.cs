using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

using PebbleSharp.Core;
using PebbleSharp.Core.NonPortable.AppMessage;
using PebbleSharp.Core.Bundles;

namespace MrGibbs.Pebble
{
    /// <summary>
    /// used to map a given line on the dashboard to a function to retrieve its data, its caption
    /// and optionally an action to invoke if the button next to it is pushed
    /// </summary>
    internal class LineStateMap
    {
        //func that given the state, returns a string that will be displayed on the dashboard
        public Func<State, string> Get { get; private set; }
        //the label that will appear on the dashboard for this map
        public string Caption { get; private set; }
        //an option action that cwill be taken when the user pushes the button next to the line on the dashboard (up or down only)
        public Action Action { get; private set; }

        public LineStateMap(Func<State, string> f, string caption)
        {
            Get = f;
            Caption = caption;
            Action = null;
        }

        public LineStateMap(Func<State, string> f, string caption, Action action)
        {
            Get = f;
            Caption = caption;
            Action = action;
        }
    }

	/// <summary>
	/// viewer for a specific pebble.
	/// </summary>
	public class PebbleViewer : IViewer
	{
		/// <summary>
		/// commands that the pebble may send
		/// </summary>
		private enum UICommand : byte { Button = 0, Dash = 1, Course = 2, Mark = 3, NewRace = 4, Calibrate = 5, Restart = 6, Reboot = 7, Shutdown = 8 }
		/// <summary>
		/// buttons that the pebble may send from the dashboard
		/// </summary>
		private enum Button : byte { Up = 0, Select = 1, Down = 2 }

		private ILogger _logger;
		private PebblePlugin _plugin;
		private PebbleSharp.Core.Pebble _pebble;
		private byte _transactionId;
		private UUID _uuid;

		private int _lineCount;//number of rows shown on the pebble UI
		private volatile IList<int> _lineValueIndexes;//index of which map we're currently showing on each line
		private static IList<LineStateMap> _lineStateMaps;

		private static Dictionary<UICommand, Action<AppMessagePacket>> _commandMaps;

		private Action<Action<ISystemController, IRaceController>> _queueCommand;

		private TimeSpan _sendTimeout = new TimeSpan (0, 0, 0, 5);
		private Task _lastSend;
		private DateTime? _lastSendAt;

		public PebbleViewer (ILogger logger, PebblePlugin plugin, PebbleSharp.Core.Pebble pebble, IZip appBundleZip, Action<Action<ISystemController, IRaceController>> queueCommand)
		{
			_queueCommand = queueCommand;
			_plugin = plugin;
			_logger = logger;
			_pebble = pebble;

			_pebble.ConnectAsync ().Wait ();
			_logger.Info ("Connected to pebble " + _pebble.PebbleID);
			
			_transactionId = 255;

			var progress = new Progress<ProgressValue> (pv => _logger.Debug ("Installing app on pebble " + pebble.PebbleID + ", " + pv.ProgressPercentage + "% complete. " + pv.Message));
			var bundle = new AppBundle ();
			bundle.Load (appBundleZip, _pebble.Firmware.HardwarePlatform.GetPlatform ());
			_uuid = bundle.AppMetadata.UUID;
			_pebble.InstallClient.InstallAppAsync (bundle, progress).Wait ();
			_logger.Info ("Installed app on pebble " + pebble.PebbleID);

			_pebble.RegisterCallback<AppMessagePacket> (Receive);

			InitializeViewer ();
		}

		/// <summary>
		/// initializes the viewer state to the defaults
		/// TODO: have this load from whatever the last set of settings was based on pebble id?
		/// </summary>
		private void InitializeViewer ()
		{
			_lineCount = 3;
			if (_lineStateMaps == null) {
				/*
				 * This order must match https://github.com/brookpatten/MrGibbs-Pebble/blob/master/src/DashboardMapMenu.c
				 * or things will get weird
				*/
				_lineStateMaps = new List<LineStateMap> ();
				_lineStateMaps.Add (StateValueMap(StateValue.SpeedInKnots, "Speed (kn)"));
				_lineStateMaps.Add (StateValueMap(StateValue.VelocityMadeGood, "VMG (kn)",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.VelocityMadeGoodOnCourse, "VMC (kn)",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.CourseOverGroundByLocation, "Course Over Ground"));
				_lineStateMaps.Add (StateValueMap(StateValue.MagneticHeading, "Heading (Mag)"));
				_lineStateMaps.Add (StateValueMap(StateValue.MagneticHeadingWithVariation, "Heading (True)"));
				_lineStateMaps.Add (StateValueMap(StateValue.Heel, "Heel"));
				_lineStateMaps.Add (StateValueMap(StateValue.ApparentWindSpeedKnots, "Wind Speed (Apparant)",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.TrueWindSpeedKnots, "Wind Speed (True)",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.ApparentWindDirection, "Wind Direction (Aparant)"));
				_lineStateMaps.Add (StateValueMap(StateValue.TrueWindDirection, "Wind Direction (True)",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.PeakSpeedInKnotsForWind, "Nominal Speed",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.PeakSpeedPercentForWind, "% Nominal Speed",format:"{0:0}%",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.MaximumSpeedInKnots, "Top Speed (kn)"));
				_lineStateMaps.Add (new LineStateMap (s => s.Countdown.HasValue ? s.Countdown.Value.Minutes + ":" + s.Countdown.Value.Seconds.ToString ("00") : "", "Countdown", () => _queueCommand ((s, r) => r.CountdownAction ())));
				_lineStateMaps.Add (new LineStateMap (s => s.StateValues.ContainsKey(StateValue.DistanceToTargetMarkInYards) ? string.Format ("{0}{1:0}", s.TargetMark != null ? s.TargetMark.Abbreviation : "", s.StateValues[StateValue.DistanceToTargetMarkInYards]) : "?", "Distance to Mark (yds)"));
				_lineStateMaps.Add (StateValueMap(StateValue.Pitch, "Pitch"));
				_lineStateMaps.Add (StateValueMap(StateValue.VelocityMadeGoodPercent,"VMG %",format:"{0:0.0}%",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.VelocityMadeGoodOnCoursePercent,"VMC %",format:"{0:0.0}%",missing:"?"));
				_lineStateMaps.Add (StateValueMap(StateValue.CurrentTackCourseOverGroundDelta, "Current Tack Delta"));
				_lineStateMaps.Add (StateValueMap(StateValue.CourseOverGroundRelativeToCourse, "Course Relative"));
				_lineStateMaps.Add (StateValueMap(StateValue.MastHeel, "Mast Heel"));
				_lineStateMaps.Add (StateValueMap(StateValue.MastPitch, "Mast Pitch"));
				_lineStateMaps.Add (StateValueMap(StateValue.MastBendBeam, "Mast Bend Port/Starboard"));
				_lineStateMaps.Add (StateValueMap(StateValue.MastBendCenterline, "Mast Bend Fore/Aft"));
			}

			//TODO: remember what the last settings for this pebble were and use those
			_lineValueIndexes = new List<int> ();
			_lineValueIndexes.Add (0);//speed
			_lineValueIndexes.Add (3);//cog
			_lineValueIndexes.Add (14);//countdown

			//perfect world future defaults:
			//% target speed
			//VMC %
			//Current tack delta

			if (_commandMaps == null) {
				_commandMaps = new Dictionary<UICommand, Action<AppMessagePacket>> ();
				_commandMaps.Add (UICommand.Dash, ProcessDashCommand);
				_commandMaps.Add (UICommand.Button, ProcessButtonCommand);
				_commandMaps.Add (UICommand.Calibrate, m => _queueCommand ((s, r) => s.Calibrate ()));
				_commandMaps.Add (UICommand.Restart, m => _queueCommand ((s, r) => s.Restart ()));
				_commandMaps.Add (UICommand.Reboot, m => _queueCommand ((s, r) => s.Reboot ()));
				_commandMaps.Add (UICommand.Shutdown, m => _queueCommand ((s, r) => s.Shutdown ()));
				_commandMaps.Add (UICommand.Mark, ProcessMarkCommand);
				_commandMaps.Add (UICommand.NewRace, m => _queueCommand ((s, r) => r.NewRace()));
			}
		}

		private LineStateMap StateValueMap(StateValue val, string name,string format="{0:0.0}",string missing="")
		{
			return new LineStateMap (s=>s.StateValues.ContainsKey(val) ? string.Format(format,s.StateValues[val]) : missing,name);
		}

        /// <summary>
        /// invoked when data is received by the pebble
        /// </summary>
        /// <param name="response"></param>
		private void Receive(AppMessagePacket response)
        {
			if (response.Values != null)
            {
				var commandTuple = response.Values.SingleOrDefault(x => x.Key == 0);
                if(commandTuple!=null && commandTuple is AppMessageUInt8)
                {
                    UICommand command = (UICommand)((AppMessageUInt8)commandTuple).Value;
                    
                    if(_commandMaps.ContainsKey(command))
                    {
                        _commandMaps[command](response);
                    }
                    else
                    {
                        _logger.Info("Received Command " + command.ToString() + " from pebble " + _pebble.PebbleID+", but there is no map specified");
                    }
                }
            }
        }

        /// <summary>
        /// received when a button is pushed on the pebble dashboard
        /// </summary>
        /// <param name="response"></param>
		private void ProcessButtonCommand(AppMessagePacket response)
        {
            var lineTuple = response.Values.SingleOrDefault(x=>x.Key==1);
            if(lineTuple!=null && lineTuple is AppMessageUInt8)
            {
                var line = ((AppMessageUInt8)lineTuple).Value;
                var action = _lineStateMaps[_lineValueIndexes[line]].Action;
                if(action!=null)
                {
                    _logger.Info("Received button press for line " + line + ", executing action for " + _lineStateMaps[_lineValueIndexes[line]].Caption);
                    action();
                }
                else
                {
                    _logger.Info("Received button press for line " + line+", but there is no action defined for "+_lineStateMaps[_lineValueIndexes[line]].Caption);
                }
            }
        }

        /// <summary>
        /// invoked when the user wants to change out a line on the dashboard
        /// </summary>
        /// <param name="response"></param>
		private void ProcessDashCommand(AppMessagePacket response)
        {
            //which line are we changing?
            var line = ((AppMessageUInt8)response.Values.SingleOrDefault(x => x.Key == 1)).Value;
            //change it to which map?
            var map = ((AppMessageUInt8)response.Values.SingleOrDefault(x => x.Key == 2)).Value;

            _logger.Info("Pebble "+_pebble.PebbleID+" Has requested Dashboard Row "+line+" to show "+_lineStateMaps[map].Caption);

            lock (_lineValueIndexes)
            {
                _lineValueIndexes[(int) line] = (int) map;
            }
        }

        /// <summary>
        /// invoked when the user is attempting to tell us something about the course
        /// </summary>
        /// <param name="response"></param>
		private void ProcessMarkCommand(AppMessagePacket response)
        {
            var mark = (MarkType)((AppMessageUInt8)response.Values.SingleOrDefault(x => x.Key == 1)).Value;

            var bearingTuple = response.Values.SingleOrDefault(x=>x.Key==2);
            if (bearingTuple!=null)
            {
                //bearing
                int pebbleBearing = ((AppMessageInt32)bearingTuple).Value;
                //convert to double
                double bearingToNorth = ((double)pebbleBearing/65536.0)*360.0;

                //pebble reports bearing to NORTH, we need to figure out what we're pointed at
                double bearingToMark = 360 - bearingToNorth;

                

                _queueCommand((s, r) => r.SetMarkBearing(mark, bearingToMark, true));
            }
            else
            {
                //location
                _queueCommand((s, r) => r.SetMarkLocation(mark));
            
            }
        }

        /// <summary>
        /// update the the pebble with data from the current race/boat state
        /// </summary>
        /// <param name="state"></param>
        public void Update(State state)
        {

            //don't send anything until the last send has completed or errored
            if (_lastSend == null || _lastSend.IsCanceled || _lastSend.IsCompleted || _lastSend.IsFaulted 
                //or if it has exceeded the send timeout
                || !_lastSendAt.HasValue || state.SystemTime - _lastSendAt.Value > _sendTimeout)
            {
                _transactionId--;
				AppMessagePacket message = new AppMessagePacket();
                message.ApplicationId = _uuid;
                message.TransactionId = _transactionId;
                message.Command = (byte) Command.Push;

                string captions = "";

                for (int i = 0; i < _lineCount; i++)
                {
                    LineStateMap map = null;
                    lock (_lineValueIndexes)
                    {
                        map = _lineStateMaps[_lineValueIndexes[i]];
                    }
                    message.Values.Add(new AppMessageString() {Key = (uint) message.Values.Count, Value = map.Caption});
                    captions = captions + map.Caption + ",";
                    message.Values.Add(new AppMessageString()
                    {
                        Key = (uint) message.Values.Count,
                        Value = map.Get(state)
                    });
                }

                if (state.Message != null)
                {
                    message.Values.Add(new AppMessageString()
                    {
                        Key = (uint) message.Values.Count,
                        Value = state.Message.Text
                    });
                }

                _lastSend = _pebble.SendApplicationMessage(message);
                _lastSendAt = state.SystemTime;
                _logger.Debug("Sent state to pebble " + _pebble.PebbleID+" ("+captions+")");
            }
            else
            {
                //_logger.Debug("Skipped send to pebble, previous send has not completed yet");
            }
        }

		/// <summary>
		/// Given projected gps points, converts to a simple set of coordinates that can be 
		/// rendered as an image on the pebble
		/// </summary>
		/// <returns>The course.</returns>
		/// <param name="windward">Windward.</param>
		/// <param name="leeward">Leeward.</param>
		/// <param name="boat">Boat.</param>
		private void PrerenderCourse (Vector2 windward, Vector2 leeward, Vector2 boat)
		{
			//size of the pebble screen
			Vector2 targetSize = new Vector2() { X = 144, Y = 168 };

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
			var windwardPolar = windward.CartesianToPolar ();
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
			float scaleFactor = targetSize.Y/ currentSize ;

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

        /// <inheritdoc />
        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _pebble.Disconnect();

        }
    }
}
