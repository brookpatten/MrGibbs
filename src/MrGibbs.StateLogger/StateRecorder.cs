using System;
using System.Data;
using System.IO;

using Mono.Data.Sqlite;

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
		private IStateRepository _stateRepository;
		private string _dataPath;
		private const string _sqliteConnectionStringFormat = "Data Source={0};Version=3;";

		public StateRecorder (IPlugin plugin,string dataPath)
		{
			_dataPath = dataPath;
			_plugin = plugin;

			Initialize ();
		}

		public IPlugin Plugin 
		{
			get 
			{
				return _plugin;
			}
		}

		public void Dispose ()
		{
			_stateRepository.Dispose ();
		}

		private void Initialize ()
		{
			if (_stateRepository != null) 
			{
				_stateRepository.Dispose ();
			}

			var now = DateTime.UtcNow;
			string dataFilePath = Path.Combine (_dataPath, string.Format ("{0:0000}{1:00}{2:00}-{3:00}{4:00}.db",now.Year,now.Month,now.Day,now.Hour,now.Minute));

			IDbConnection connection = null;
			if (File.Exists (dataFilePath)) 
			{
				try 
				{
					connection = new SqliteConnection (string.Format (_sqliteConnectionStringFormat,dataFilePath));
				} 
				catch (Exception ex) 
				{
					File.Move (dataFilePath, dataFilePath + ".bad");
				}
			}

			if (connection == null) 
			{
				SqliteConnection.CreateFile (dataFilePath);
				connection = new SqliteConnection (string.Format (_sqliteConnectionStringFormat,dataFilePath));
			}

			_stateRepository = new StateRepository (connection);

			_stateRepository.Initialize ();
		}

		public void Record (State state)
		{
			//TODO: store the id of the last time it was saved?
			//TODO: add some logic to break out races into another table?
			_stateRepository.Save(state);
		}
	}
}

