using Npgsql;
using System.Data;

namespace Apolon_ADPC.ORM.Core
{
    public class DbConnectionManager : IDisposable
    {
        private readonly NpgsqlConnection _connection;

        public DbConnectionManager(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public NpgsqlConnection Connection => _connection;

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }

}
