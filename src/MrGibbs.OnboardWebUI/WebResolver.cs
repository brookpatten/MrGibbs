using System;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Collections.Generic;

using Owin;
using Microsoft.Owin;
using Ninject;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;
using MrGibbs.OnboardWebUI.Api;

namespace MrGibbs.OnboardWebUI
{
	/// <summary>
	/// simple wrapper over ninject that allows us to inject state/logger/queueaction from the 
	/// main application easily and failover to injection from ninject for everything else
	/// </summary>
	public class WebResolver:IDependencyResolver
	{
		private ILogger _logger;
		private Action<Action<ISystemController, IRaceController>> _queueCommand;
		private IKernel _kernel;
		private State _state;
		private IDependencyResolver _defaultResolver;

		public WebResolver (IDependencyResolver defaultResolver,ILogger logger,Action<Action<ISystemController, IRaceController>> queueCommand, IKernel kernel)
		{
			_defaultResolver = defaultResolver;
			_state = new State ();
			_queueCommand = queueCommand;
			_logger = logger;
			_kernel = kernel;
			_kernel.Bind<State> ().ToConstant (_state);
			_kernel.Bind<Action<Action<ISystemController, IRaceController>>> ().ToConstant (_queueCommand);
		}

		public void UpdateState(State state)
		{
			lock (_state)
			{
				_state = state;
			}
			_kernel.Rebind<State>().ToConstant(_state);
		}

		public IDependencyScope BeginScope()
		{
			return this;
		}

		public object GetService(Type serviceType)
		{
			if (serviceType.IsSubclassOf (typeof(ApiController)))
			{
				return _kernel.Get (serviceType);
			}
			else
			{
				return _defaultResolver.GetService (serviceType);
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _defaultResolver.GetServices (serviceType);
		}

		public void Dispose()
		{
			_defaultResolver.Dispose ();
		}
	}
}

