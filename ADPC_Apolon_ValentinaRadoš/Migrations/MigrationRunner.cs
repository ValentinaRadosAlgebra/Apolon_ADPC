using Npgsql;


namespace Migrations
{
    public class MigrationRunner
    {
        private readonly NpgsqlConnection _conn;

        public MigrationRunner(string cs)
        {
            _conn = new NpgsqlConnection(cs);
            _conn.Open();
        }

        public void Apply(Migration m)
        {
            using var tx = _conn.BeginTransaction();

            new NpgsqlCommand(m.UpSql, _conn).ExecuteNonQuery();

            var cmd = new NpgsqlCommand(
                "INSERT INTO migrations(name) VALUES(@n)", _conn);
            cmd.Parameters.AddWithValue("@n", m.Name);
            cmd.ExecuteNonQuery();

            tx.Commit();
        }

        public void RollbackLast()
        {
            var get = new NpgsqlCommand(
                "SELECT name FROM migrations ORDER BY id DESC LIMIT 1", _conn);

            var name = (string?)get.ExecuteScalar();
            if (name == null) return;

            var migration = LoadMigration(name);
            new NpgsqlCommand(migration.DownSql, _conn).ExecuteNonQuery();

            new NpgsqlCommand(
                "DELETE FROM migrations WHERE name=@n", _conn)
            { Parameters = { new("@n", name) } }.ExecuteNonQuery();
        }

        private Migration LoadMigration(string name)
        {
            return MigrationRegistry.GetByName(name);
        }
    }

}
