using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Models;

namespace MrGibbs.Test
{
    public class StateTest
    {
        public void Run()
        {
            State state = new State();
            state.SystemTime = DateTime.UtcNow;

            System.Diagnostics.Debug.Assert(state.Message == null);
            state.AddMessage(MessageCategory.System, MessagePriority.High, 5, "High");
            state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Normal");
            state.CycleMessages();
            System.Diagnostics.Debug.Assert(state.Message.Priority == MessagePriority.High);
            System.Diagnostics.Debug.Assert(state.MessageCount == 1);

            System.Threading.Thread.Sleep(6000);
            state.SystemTime = DateTime.UtcNow;
            state.CycleMessages();
            System.Diagnostics.Debug.Assert(state.Message.Priority == MessagePriority.Normal);
            System.Diagnostics.Debug.Assert(state.MessageCount == 0);


        }
    }
}
