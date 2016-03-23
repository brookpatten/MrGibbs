using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using MrGibbs.Models;

namespace MrGibbs.Test
{
	[TestFixture]
    public class StateTest
    {
		private State _state;
		private DateTime _startTime;

		[SetUp]
		public void Setup ()
		{
			_startTime = new DateTime (2016, 1, 1, 0, 0, 0);
			_state = new State();
			_state.SystemTime = _startTime;
		}

		[Test]
        public void HighPriorityMessagesShouldShowBeforeNormalPriority()
        {
			Assert.IsNull(_state.Message);
            
			_state.AddMessage(MessageCategory.System, MessagePriority.High, 5, "High");
            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Normal");
            _state.CycleMessages();
			Assert.AreEqual(MessagePriority.High,_state.Message.Priority );
			Assert.AreEqual(1,_state.MessageCount);
		}

		[Test]
		public void MessagesShouldExpire()
		{
			_state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Normal");

			_state.SystemTime = _startTime.Add (new TimeSpan (0, 0, 6));
			_state.CycleMessages();
			Assert.AreEqual(0,_state.MessageCount);
		}
    }
}
