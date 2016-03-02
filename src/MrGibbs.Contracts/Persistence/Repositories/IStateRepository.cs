using System;
using MrGibbs.Models;

namespace MrGibbs.Contracts.Persistence.Repositories
{
	public interface IStateRepository:IDisposable
	{
		void Initialize ();
		void Save(State state);
	}
}

