using System;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// a generic plugin component, provides a reference to the parent plugin
    /// </summary>
    public interface IPluginComponent:IDisposable
    {
        IPlugin Plugin { get; }
    }
}
