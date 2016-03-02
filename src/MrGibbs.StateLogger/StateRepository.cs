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
				                      "longitude NUMERIC," +
				                      "courseoverground NUMERIC," +
				                      "speed NUMERIC," +
				                      "heel NUMERIC,"+
				                      "pitch NUMERIC,"+
				                      "heading NUMERIC"+
				                      ")");
			}
		}

		public void Save (State state)
		{
			_dbConnection.Execute ("insert into StateLog(" +
								   "time," +
								   "latitude," +
								   "longitude," +
								   "courseoverground," +
								   "speed," +
								   "heel," +
								   "pitch," +
			                       "heading" +
								   ") values (" +
								   "@BestTime," +
								   "@LocationLatitudeValue," +
								   "@LocationLongitudeValue," +
								   "@CourseOverGroundByLocation," +
								   "@SpeedInKnots," +
								   "@Heel," +
								   "@Pitch," +
			                       "@Heading" + 
								   ")",
			   new {
				    BestTime = state.BestTime,
				    LocationLatitudeValue = state.Location.Latitude.Value,
				    LocationLongitudeValue = state.Location.Longitude.Value,
				    CourseOverGroundByLocation = state.CourseOverGroundByLocation,
				    SpeedInKnots = state.SpeedInKnots,
				    Heel = state.Heel,
					Pitch=state.Pitch,
					Heading=state.MagneticHeadingWithVariation,
				});
		}
	}
}

