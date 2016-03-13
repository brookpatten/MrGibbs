using System;
using System.Data;
using System.IO;
using System.Linq;

using Mono.Data.Sqlite;
using Dapper;

using MrGibbs.Models;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Persistence.Repositories;

namespace MrGibbs.StateLogger
{
	/// <summary>
	/// implements a simple data recorder in a single plugin
	/// TODO: possibly factor this out so that each plugin does its own persistence from the part
	/// of the state that it is responsible for
	/// </summary>
	public class StateRecorder:IRecorder
	{
		private IPlugin _plugin;
		private IDbConnection _connection;

		public StateRecorder (IDbConnection connection)
		{
			_connection = connection;

			Initialize ();
		}

		public IPlugin Plugin 
		{
			get 
			{
				return _plugin;
			}
			set 
			{
				_plugin = value;
			}
		}

		public void Dispose ()
		{
			//the connection might be used by other plugins so we do NOT dispose it
		}

		private void Initialize ()
		{
			if(!_connection.Query<string> ("SELECT name FROM sqlite_master WHERE type='table' and name='StateLog';").Any())
			{
				//if the table doesn't exist, create it
				_connection.Execute("create table StateLog(" +
				                      "time DATETIME," +
				                      "latitude NUMERIC," +
				                      "longitude NUMERIC" +
				                      ")");

				_connection.Execute ("create table StateValue(" +
				                       "time DATETIME," +
				                       "key integer," +
				                       "value NUMERIC"+
				                       ")");
			}
		}

		public void Record (State state)
		{
			_connection.Execute ("insert into StateLog(" +
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
				_connection.Execute ("insert into StateValue(" +
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

