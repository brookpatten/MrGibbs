using System;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// provides a method to send commands to the core mr. Gibbs services
    /// </summary>
	public interface ICommandable
	{
        /// <summary>
        /// used by plugins to invoke actions on the systemcontroller or race controller which must be peformed in a thread-safe manner
        /// </summary>
        /// <param name="command"></param>
		void QueueCommand(Action<ISystemController,IRaceController> command);
	}
}