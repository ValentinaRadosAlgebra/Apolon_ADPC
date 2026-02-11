using Npgsql;


namespace Migrations
{
    public class MigrationRunner //Opens db connection, create migration table, apply migrations(UP), rollback las migration (DOWN)
    {
        private readonly NpgsqlConnection _conn;

        public MigrationRunner(string cs)
        {
            _conn = new NpgsqlConnection(cs);
            _conn.Open();
        }
        public void EnsureMigrationsTable() //make sure migration exists
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS migrations (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(255) UNIQUE,
                    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.ExecuteNonQuery();
        }
        public void Apply(Migration m) //applies one migration
        {
            using var tx = _conn.BeginTransaction();

            new NpgsqlCommand(m.UpSql, _conn).ExecuteNonQuery(); //execute up

            var cmd = new NpgsqlCommand(
                "INSERT INTO migrations(name) VALUES(@n)", _conn);
            cmd.Parameters.AddWithValue("@n", m.Name);
            cmd.ExecuteNonQuery(); //update migrations table

            tx.Commit();//finalize changes
        }

        public void RollbackLast() //reverses the most recently applied migration
        {
            var get = new NpgsqlCommand(
                "SELECT name FROM migrations ORDER BY id DESC LIMIT 1", _conn); //get last migration

            var name = (string?)get.ExecuteScalar();
            if (name == null) return;

            var migration = LoadMigration(name); //load down
            new NpgsqlCommand(migration.DownSql, _conn).ExecuteNonQuery();//execute

            new NpgsqlCommand(
                "DELETE FROM migrations WHERE name=@n", _conn)
            { Parameters = { new("@n", name) } }.ExecuteNonQuery(); //remove record
        }

        private Migration LoadMigration(string name)
        {
            return MigrationRegistry.GetByName(name);
        }
    }

}
