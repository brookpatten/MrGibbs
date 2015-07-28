using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Console
{
    public class RaceController:IRaceController
    {
        private State _state;
        private ILogger _logger;

        public RaceController(ILogger logger)
        {
            _state = new State();
            _logger = logger;
        }

        public State State
        {
            get
            {
                return _state;  
            }
        }

        public void CountdownAction()
        {
            lock (_state)
            {
                if (_state.StartTime.HasValue)
                {
                    if (_state.StartTime > _state.BestTime)
                    {
                        _logger.Info("Countdown Synced");
                        var remaining = _state.StartTime.Value - _state.BestTime;

                        if (remaining.Minutes >= 4)
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                        else if (remaining.Minutes >= 3)
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 1, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                        else
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                    }
                    else
                    {
                        _logger.Info("Countdown Reset");
                        _state.StartTime = null;
                        _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown reset");
                    }
                }
                else
                {
                    _logger.Info("Countdown Started");
                    _state.StartTime = _state.BestTime.AddMinutes(5);
                    _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown started");
                }
            }
        }

        public void SetCourseType(int index)
        {
            throw new NotImplementedException();
        }

        public void SetMarkLocation(int markIndex)
        {
            throw new NotImplementedException();
        }

        public void SetMarkBearing(int markIndex, double bearing)
        {
            throw new NotImplementedException();
        }

        public void ClearMark(int markIndex)
        {
            throw new NotImplementedException();
        }

        public void NewRace()
        {
            throw new NotImplementedException();
        }
    }
}
