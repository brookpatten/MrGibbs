using System;

using MrGibbs.Models;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
	/// <summary>
	/// calculates mast deflection by comparing the angles of the hull with the angles of the mast
	/// </summary>
	public class MastBendCalculator:ICalculator
	{
		private ILogger _logger;
		private IPlugin _plugin;

		public MastBendCalculator(ILogger logger, IPlugin plugin)
		{
			_plugin = plugin;
			_logger = logger;
		}

		/// <inheritdoc />
		public void Calculate(State state)
		{
			if (state.StateValues.ContainsKey(StateValue.Heel)
			    && state.StateValues.ContainsKey(StateValue.MastHeel))
			{
				state.StateValues [StateValue.MastBendBeam] = state.StateValues [StateValue.MastHeel] - state.StateValues [StateValue.Heel];
			}

			if (state.StateValues.ContainsKey(StateValue.Pitch)
			    && state.StateValues.ContainsKey(StateValue.MastPitch))
			{
				state.StateValues [StateValue.MastBendCenterline] = state.StateValues [StateValue.MastPitch] - state.StateValues [StateValue.Pitch];
			}
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}
	}
}

