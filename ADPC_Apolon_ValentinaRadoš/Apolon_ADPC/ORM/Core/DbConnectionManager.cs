using Npgsql;
using System.Data;

namespace Apolon_ADPC.ORM.Core
{
    public class DbConnectionManager : IDisposable
    {
        private readonly NpgsqlConnection _connection; //holds the connection to PostgreSQL db

        public DbConnectionManager(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        } //opens connection when called

        public NpgsqlConnection Connection => _connection; //provides access to the connection, so not cresting a new connection each time

        public void Dispose() //close the connection when done
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}
