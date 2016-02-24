using Mono.Data.Sqlite;

using Dapper;

namespace MrGibbs.Persistence.Migrations
{
    public class Schema
    {
        private SqliteConnection _connection;

        public Schema(SqliteConnection connection)
        {
            _connection = connection;
        }

        public void InitializeSchema()
        {
            _connection.Execute("create table test (id integer primary key,name text);");
        }
    }
}
