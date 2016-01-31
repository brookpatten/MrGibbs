using System;
using MrGibbs.Contracts;

namespace MrGibbs.Contracts
{
	public interface ICommandable
	{
		void QueueCommand(Action<ISystemController,IRaceController> command);
	}
}