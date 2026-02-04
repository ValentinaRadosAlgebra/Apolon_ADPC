using Apolon_ADPC.ORM.Attributes;
using Apolon_ADPC.ORM.Mapping;
using System.Reflection;

namespace Apolon_ADPC.ORM.Queries
{
    public static class SqlGenerator
    {
        public static string GenerateCreateTable<T>()
        {
            var type = typeof(T);
            var tableName = EntityMapper.GetTableName(type);
            var columns = EntityMapper.GetColumns(type);

            var sql = $"CREATE TABLE IF NOT EXISTS {tableName} (\n";
            var defs = new List<string>();

            foreach (var prop in columns)
            {
                // PRIMARY KEY
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    defs.Add("id SERIAL PRIMARY KEY");
                    continue;
                }

                var col = prop.GetCustomAttribute<ColumnAttribute>();
                var sqlType = MapType(prop.PropertyType);

                var line = $"{col.Name} {sqlType}";

                // NULL / NOT NULL
                if (!col.IsNullable)
                    line += " NOT NULL";

                // UNIQUE
                if (col.IsUnique)
                    line += " UNIQUE";

                // DEFAULT
                if (col.DefaultValue != null)
                    line += $" DEFAULT {col.DefaultValue}";

                // FOREIGN KEY
                var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                if (fk != null)
                {
                    line += $" REFERENCES {fk.ReferenceTable}({fk.ReferenceColumn})";
                }

                defs.Add(line);
            }

            sql += string.Join(",\n", defs);
            sql += "\n);";

            return sql;
        }

        private static string MapType(Type t)
        {
            var underlyingType = Nullable.GetUnderlyingType(t) ?? t;

            if (underlyingType == typeof(string)) return "VARCHAR(255)";
            if (underlyingType == typeof(int)) return "INT";
            if (underlyingType == typeof(decimal)) return "DECIMAL";
            if (underlyingType == typeof(float)) return "FLOAT";
            if (underlyingType == typeof(DateTime)) return "TIMESTAMP";
            if (underlyingType == typeof(DateOnly)) return "DATE";
            if (underlyingType.IsEnum) return "INT";

            throw new Exception($"Unsupported type {t}");
        }

    }
}
