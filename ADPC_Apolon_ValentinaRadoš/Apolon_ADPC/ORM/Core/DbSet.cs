using Apolon_ADPC.ORM.Attributes;
using Apolon_ADPC.ORM.Mapping;
using Npgsql;
using System.Reflection;

namespace Apolon_ADPC.ORM.Core
{
    public class DbSet<T> where T : class, new()
    {
        private readonly NpgsqlConnection _connection;
        private readonly string _table;

        public DbSet(NpgsqlConnection connection)
        {
            _connection = connection;
            _table = EntityMapper.GetTableName(typeof(T));
        }

        public List<T> GetAll(string? where = null, string? orderBy = null)
        {
            var sql = $"SELECT * FROM {_table}";
            if (where != null) sql += $" WHERE {where}";
            if (orderBy != null) sql += $" ORDER BY {orderBy}";

            using var cmd = new NpgsqlCommand(sql, _connection);
            using var reader = cmd.ExecuteReader();

            var result = new List<T>();

            while (reader.Read())
                result.Add(Map(reader));

            return result;
        }

        public T? GetById(int id)
        {
            var sql = $"SELECT * FROM {_table} WHERE id=@id";
            using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Insert(T entity)
        {
            var columns = EntityMapper.GetInsertableColumns(typeof(T)).ToList();

            var names = columns.Select(c => c.Column.Name);
            var parms = columns.Select(c => "@" + c.Column.Name);

            // Add RETURNING id to get generated PK
            var sql = $"""
                    INSERT INTO {_table} ({string.Join(",", names)})
                    VALUES ({string.Join(",", parms)})
                    RETURNING id
                """;

            using var cmd = new NpgsqlCommand(sql, _connection);

            foreach (var c in columns)
            {
                var rawValue = c.Property.GetValue(entity);

                object value;
                if (rawValue == null)
                {
                    value = DBNull.Value;
                }
                else if (c.Property.PropertyType.IsEnum)
                {
                    value = (int)rawValue; // enum → int
                }
                else
                {
                    value = rawValue;
                }

                cmd.Parameters.AddWithValue("@" + c.Column.Name, value);
            }

            // Get the generated id
            var idObj = cmd.ExecuteScalar();
            if (idObj == null) throw new Exception("Failed to retrieve generated ID");

            var pkProp = typeof(T).GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            if (pkProp != null)
                pkProp.SetValue(entity, Convert.ToInt32(idObj));
        }


        public void Update(int id, T entity)
        {
            var columns = EntityMapper.GetInsertableColumns(typeof(T));

            var sets = columns.Select(c => $"{c.Column.Name}=@{c.Column.Name}");

            var sql = $"""
                UPDATE {_table}
                SET {string.Join(",", sets)}
                WHERE id=@id
            """;

            using var cmd = new NpgsqlCommand(sql, _connection);

            foreach (var c in columns)
            {
                var rawValue = c.Property.GetValue(entity);

                object value;

                if (rawValue == null)
                {
                    value = DBNull.Value;
                }
                else if (rawValue.GetType().IsEnum)
                {
                    value = (int)rawValue; // 🔹 enum → int
                }
                else
                {
                    value = rawValue;
                }

                cmd.Parameters.AddWithValue("@" + c.Column.Name, value);
            }

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var cmd = new NpgsqlCommand(
                $"DELETE FROM {_table} WHERE id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private T Map(NpgsqlDataReader reader)
        {
            var entity = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                // 🔹 PRIMARY KEY
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    prop.SetValue(entity, Convert.ToInt32(reader["id"]));
                    continue;
                }

                // 🔹 NORMAL COLUMN
                var col = prop.GetCustomAttribute<ColumnAttribute>();
                if (col == null) continue;

                var val = reader[col.Name];
                if (val == DBNull.Value) continue;

                if (prop.PropertyType.IsEnum)
                    prop.SetValue(entity, Enum.ToObject(prop.PropertyType, val));
                else
                    prop.SetValue(entity, val);
            }

            return entity;
        }

    }

}
