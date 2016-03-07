using System;

using Ninject;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public static class ModuleHelpers
	{
		public static void LoadIfNotLoaded<T> (this IKernel kernel) where T : INinjectModule,new()
		{
			if (!kernel.HasModule (typeof (T).Name)) 
			{
				kernel.Load<T>();
			}
		}
	}
}

