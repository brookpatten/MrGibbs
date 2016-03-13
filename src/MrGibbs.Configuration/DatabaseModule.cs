using System;
using System.Data;
using System.IO;

using Mono.Data.Sqlite;
using Ninject;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public class DatabaseModule:NinjectModule
	{
		public enum Databases
		{
			Current,
			Today,
			Persistant
		}
		public const string SqliteConnectionStringFormat = "Data Source={0};Version=3;";

		public DatabaseModule ()
		{
		}

		public override void Load ()
		{
			string dataPath = AppConfig.DataPath;
			DateTime now = DateTime.UtcNow;
			BindDbConnection(dataPath, Databases.Current.ToString(), now, "{0:yyyyMMdd-hhmmss}.db");
			BindDbConnection (dataPath, Databases.Today.ToString(), now, "{0:yyyyMMdd}.db");
			BindDbConnection (dataPath, Databases.Persistant.ToString(), now, "persistant.db");
		}

		private void BindDbConnection(string dataPath,string bindingName, DateTime now, string nameFormat)
		{
			Kernel.Bind<IDbConnection> ()
			      .ToMethod (c => CreateConnection(dataPath,now,nameFormat))
			      .InSingletonScope ()
			      .Named (bindingName);
		}

		private IDbConnection CreateConnection (string dataPath, DateTime now, string nameFormat)
		{
			string dataFilePath = Path.Combine (dataPath, string.Format (nameFormat, now));
			string connectionString = string.Format (SqliteConnectionStringFormat, dataFilePath);

			IDbConnection connection = null;
			if (File.Exists (dataFilePath)) 
			{
				try 
				{
					connection = new SqliteConnection (connectionString);
				} 
				catch (Exception ex) 
				{
					File.Move (dataFilePath, dataFilePath + string.Format("{0:yyyyMMddhhmmss}",DateTime.UtcNow)+".bad");
				}
			}

			if (connection == null) 
			{
				SqliteConnection.CreateFile (dataFilePath);
				connection = new SqliteConnection(connectionString);
			}

			return connection;
		}
	}
}

