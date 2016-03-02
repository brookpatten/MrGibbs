using System;
using System.Collections.Generic;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Contracts.Persistence;

namespace MrGibbs.StateLogger
{
	/// <summary>
	/// Plugin to write important state fields to sqlite table
	/// </summary>
	public class StateLoggerPlugin:IPlugin
	{
		private ILogger _logger;
		private bool _initialized = false;
		private IList<IPluginComponent> _components;
		private StateRecorder _recorder;
		private string _dataPath;

		public StateLoggerPlugin(ILogger logger,string dataPath)
		{
			_logger = logger;
			_dataPath = dataPath;
		}

		/// <inheritdoc />
		public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
		{
			if (_recorder != null) 
			{
				_recorder.Dispose ();
			}

			_components = new List<IPluginComponent>();

			_initialized = false;

			var recorder = new StateRecorder (this, _dataPath);
			configuration.Recorders.Add (recorder);
			_components.Add (recorder);

			_initialized = true;
		}

		/// <inheritdoc />
		public bool Initialized
		{
			get { return _initialized; }
		}

		/// <inheritdoc />
		public IList<IPluginComponent> Components
		{
			get { return _components; }
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_components != null)
			{
				foreach (var component in _components)
				{
					component.Dispose();
				}
			}
		}
	}
}

