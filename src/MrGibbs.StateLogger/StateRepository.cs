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
			if(!_dbConnection.Query<string> ("SELECT name FROM sqlite_master WHERE type='table' and name='StateLog';").Any())
			{
				//if the table doesn't exist, create it
				_dbConnection.Execute("create table StateLog(" +
				                      "time DATETIME," +
				                      "latitude NUMERIC," +
				                      "longitude NUMERIC" +
				                      ")");

				_dbConnection.Execute ("create table StateValue(" +
									  "time DATETIME," +
									  "key integer," +
				                       "value NUMERIC"+
									  ")");
			}
		}

		private double? CoalesceStateValue (State state, StateValue val)
		{
			if (state.StateValues.ContainsKey (val)) 
			{
				return state.StateValues [val];
			} 
			else 
			{
				return null;
			}
		}

		public void Save (State state)
		{
			_dbConnection.Execute ("insert into StateLog(" +
								   "time," +
								   "latitude," +
								   "longitude" +
								   ") values (" +
								   "@BestTime," +
								   "@LocationLatitudeValue," +
								   "@LocationLongitudeValue" +
								   ")",
			   new {
				    BestTime = state.BestTime,
				    LocationLatitudeValue = state.Location.Latitude.Value,
				    LocationLongitudeValue = state.Location.Longitude.Value,
				});

			foreach (var key in state.StateValues.Keys) 
			{
				_dbConnection.Execute ("insert into StateValue(" +
				                       "time," +
				                       "key," +
				                       "value" +
				                       ") values (" +
				                       "@BestTime," +
				                       "@Key," +
				                       "@Value" +
				                       ")",
				                       new {
					BestTime = state.BestTime,
					Key = (int)key,
					Value = state.StateValues[key],
				});
			}
		}
	}
}

