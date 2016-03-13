using System;
using System.Data;
using System.Linq;

using MrGibbs.Contracts.Persistence.Repositories;
using MrGibbs.Models;
using Dapper;

namespace MrGibbs.StateLogger
{
	/// <summary>
	/// dapper wrapper over dbconnection to create table and do inserts
	/// </summary>
	public class StateRepository:IStateRepository
	{
		private IDbConnection _dbConnection;

		public StateRepository (IDbConnection connection)
		{
			_dbConnection = connection;
		}

		public void Dispose ()
		{
			_dbConnection.Close ();
			_dbConnection.Dispose ();
		}

		public void Initialize ()
		{
			
		}



		public void Save (State state)
		{
			_db
		}
	}
}

