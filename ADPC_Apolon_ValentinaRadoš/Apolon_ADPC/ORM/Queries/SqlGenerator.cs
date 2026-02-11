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

            var definitions = new List<string>();

            foreach (var prop in columns)
            {
                // PRIMARY KEY
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    definitions.Add("id SERIAL PRIMARY KEY");
                    continue;
                }

                var col = prop.GetCustomAttribute<ColumnAttribute>();
                if (col == null) continue;

                var sqlType = MapType(prop.PropertyType, col, prop);
                var line = $"{col.Name} {sqlType}";

                // NOT NULL
                if (!col.IsNullable)
                    line += " NOT NULL";

                // UNIQUE
                if (col.IsUnique)
                    line += " UNIQUE";

                // DEFAULT
                if (col.DefaultValue != null)
                {
                    // Convert object to SQL literal
                    string defaultSql;

                    if (col.DefaultValue is string s)
                        defaultSql = s; // assume user provides proper SQL (like CURRENT_TIMESTAMP)
                    else if (col.DefaultValue is bool b)
                        defaultSql = b ? "TRUE" : "FALSE";
                    else
                        defaultSql = col.DefaultValue.ToString()!; // numbers, etc.

                    line += $" DEFAULT {defaultSql}";
                }

                // ENUM CHECK CONSTRAINT
                if (prop.PropertyType.IsEnum)
                {
                    var max = Enum.GetValues(prop.PropertyType).Length - 1;
                    line += $" CHECK ({col.Name} BETWEEN 0 AND {max})";
                }

                // FOREIGN KEY
                var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                if (fk != null)
                {
                    line += $" REFERENCES {fk.ReferenceTable}({fk.ReferenceColumn}) ON DELETE CASCADE";
                }

                definitions.Add(line);
            }

            return $"""
                CREATE TABLE IF NOT EXISTS {tableName} (
                    {string.Join(",\n    ", definitions)}
                );
                """;
        }

        private static string MapType(Type type, ColumnAttribute col, PropertyInfo prop)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;

            if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null && t == typeof(int))
                return "SERIAL"; // PK is always SERIAL

            if (t == typeof(string)) return "VARCHAR(255)";
            if (t == typeof(int)) return "INT";
            if (t == typeof(decimal)) return "DECIMAL";
            if (t == typeof(float)) return "FLOAT";
            if (t == typeof(double)) return "DOUBLE PRECISION";
            if (t == typeof(DateTime)) return "TIMESTAMP";
            if (t == typeof(bool)) return "BOOLEAN";
            if (t.IsEnum) return "INT";

            throw new Exception($"Unsupported CLR type: {type.Name}");
        }
    }
}
