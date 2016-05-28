using System;
using System.Data;
using System.Transactions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		private DateTime? _lastRaceStart;
		private long? _lastRaceId { get; set; }

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
			//using (var transaction = new TransactionScope()) 
			{
				if (!_connection.Query<string> ("SELECT name FROM sqlite_master WHERE type='table' and name='StateLog';").Any ()) {
					//TODO: implement schema versioning somehow, migrations?

					//if the table doesn't exist, create it
					_connection.Execute ("create table StateLog(" +
										  "time DATETIME," +
										  "latitude NUMERIC," +
										  "longitude NUMERIC" +
										  ")");

					_connection.Execute ("create table StateValue(" +
										   "time DATETIME," +
										   "key integer," +
										   "value NUMERIC" +
										   ")");

					_connection.Execute ("create table StateKey(" +
										 "key integer," +
										 "name text" +
										 ")");

					_connection.Execute ("create table Race(" +
										 "id INTEGER PRIMARY KEY," +
										 "start DATETIME," +
										 "end DATETIME" +
										 ")");

					_connection.Execute ("create table Tack(" +
					                     "id INTEGER PRIMARY KEY," +
					                     "start DATETIME," +
					                     "endcourseoverground NUMERIC" +
					                     ")");

					var values = Enum.GetValues (typeof (StateValue)).Cast<StateValue> ().ToList ();
					foreach (var val in values) {
						_connection.Execute ("insert into StateKey(key,name) values(@key,@name)", new { key = (int)val, name = val.ToString () });
					}
				}
				//transaction.Complete();
			}
		}

		public void RecordTack (Tack tack)
		{
			_connection.Execute ("insert into Tack (start,endcourseoverground) values (@At,@CourseOverGround)", tack);
			//TODO: there has to be a better way to do this
			tack.Id = (long)_connection.ExecuteScalar ("select max(id) from Tack");
		}

		public void RecordRace (State state)
		{
			//TODO: need a better way to communicate race events from the supervisor to the recorder
			if (state.StartTime != _lastRaceStart) 
			{
				if (!state.StartTime.HasValue || state.StartTime < state.BestTime) 
				{
					if (_lastRaceId.HasValue) 
					{
						//if there is a previous race, set the end time
						_connection.Execute ("update Race set end=@end where id=@id", new { end = state.BestTime, id = _lastRaceId.Value });
						_lastRaceId = null;
					}

					if (state.StartTime.HasValue) 
					{
						_connection.Execute ("insert into Race (start) values (@start)", new { start = state.StartTime.Value });
						//TODO: there has to be a better way to do this
						_lastRaceId = (long)_connection.ExecuteScalar ("select max(id) from Race");
					}
				} 
				else if(state.StartTime.HasValue)
				{
					if (_lastRaceId.HasValue) 
					{
						//if there is an existing but unstarted race just change the start time
						_connection.Execute ("update Race set start=@start where id=@id", new { start = state.StartTime.HasValue, id = _lastRaceId.Value });
					}
					else
					{
						_connection.Execute ("insert into Race (start) values (@start)", new { start = state.StartTime.Value });
						//TODO: there has to be a better way to do this
						_lastRaceId = (long)_connection.ExecuteScalar ("select max(id) from Race");
					}
				}


			}
			_lastRaceStart = state.StartTime;
		}

		private void RecordState (State state)
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
				LocationLatitudeValue = state.Location!=null && state.Location.Latitude!=null ? state.Location.Latitude.Value : 0,
				LocationLongitudeValue = state.Location!=null && state.Location.Longitude!=null ? state.Location.Longitude.Value : 0,
			});
		}

		private void RecordStateValues (State state)
		{
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

		public void Record (State state)
		{
			//using (var transaction = new TransactionScope()) 
			{
				RecordRace (state);
				RecordState (state);
				RecordStateValues (state);

				//make sure all tacks are recorded by walking the list backwards and looking for zero ids
				//(once they are recorded they get real ids)
				if (state.Tacks != null) {
					for (int i = state.Tacks.Count - 1; i >= 0 && state.Tacks[i].Id==0; i--) {
						RecordTack (state.Tacks [i]);
					}
				}
				//transaction.Complete();
			}
		}
	}
}

