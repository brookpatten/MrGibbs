using System;
using System.Collections.Generic;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// A Mr. Gibbs Plugin
    /// Initialize method should add relevant components to the PluginConfiguration and populate the Components proprty
    /// </summary>
    public interface IPlugin:IDisposable
    {
        /// <summary>
        /// Initializes the plugin
        /// The plugin will handle initialization of each of its components and add them to the appropriate array in the configuration
        /// It will also keep track of its own components in the Components property
        /// </summary>
        /// <param name="configuration">Configuration to add the components to</param>
        /// <param name="queueCommand">A way for the plugin to send commands back to the supervisor to be executed.  Commands are always executed AFTER all plugins have executed ina given tick</param>
        void Initialize(PluginConfiguration configuration, Action<Action<ISystemController,IRaceController>> queueCommand);
        bool Initialized { get; }
        IList<IPluginComponent> Components { get; } 
    }
}
