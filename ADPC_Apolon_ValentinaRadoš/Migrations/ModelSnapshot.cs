using Apolon_ADPC.ORM.Attributes;
using Apolon_ADPC.ORM.Mapping;
using System.Reflection;

namespace Migrations
{
    public class ColumnSnapshot //everything needed to describe a column
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "VARCHAR(255)";
        public bool IsNullable { get; set; } = true;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public object? Default { get; set; }
        public string? ForeignKeyTable { get; set; }
        public string? ForeignKeyColumn { get; set; }
    }

    public class TableSnapshot
    {
        public string Name { get; set; } = "";
        public List<ColumnSnapshot> Columns { get; set; } = new();
    }

    public class ModelSnapshot //entire database schema
    {
        public Dictionary<string, TableSnapshot> Tables { get; set; } = new();
    }

    public static class SnapshotGenerator //reads entity classes and builds ModelSnapshot
    {
        public static ModelSnapshot Generate(params Type[] entities)
        {
            var snapshot = new ModelSnapshot(); //empty snapshot

            foreach (var t in entities)
            {
                var tableName = EntityMapper.GetTableName(t);
                var columns = new List<ColumnSnapshot>();

                foreach (var p in EntityMapper.GetColumns(t))
                {
                    var pkAttr = p.GetCustomAttribute<PrimaryKeyAttribute>();
                    var colAttr = p.GetCustomAttribute<ColumnAttribute>();

                    // Name from [Column] attribute or snake_case default -> determine column name
                    var colName = colAttr?.Name ?? ToSnakeCase(p.Name);

                    columns.Add(new ColumnSnapshot
                    {
                        Name = colName,
                        Type = MapClrTypeToSqlType(p.PropertyType, pkAttr != null),
                        IsNullable = pkAttr != null ? false : colAttr?.IsNullable ?? true, // PK is NOT NULL
                        IsPrimaryKey = pkAttr != null,
                        IsUnique = colAttr?.IsUnique ?? false,
                        Default = colAttr?.DefaultValue,
                        ForeignKeyTable = p.GetCustomAttribute<ForeignKeyAttribute>()?.ReferenceTable,
                        ForeignKeyColumn = p.GetCustomAttribute<ForeignKeyAttribute>()?.ReferenceColumn
                    });
                }

                snapshot.Tables[tableName] = new TableSnapshot
                {
                    Name = tableName,
                    Columns = columns
                }; //ad table to snapshot
            }

            return snapshot;
        }

        private static string MapClrTypeToSqlType(Type t, bool isPrimaryKey)//This converts C# types to PostgreSQL types
        {
            var type = Nullable.GetUnderlyingType(t) ?? t;

            if (isPrimaryKey) return "SERIAL"; // PK - SERIAL in Postgres, ensures auto increment

            if (type == typeof(int)) return "INT";
            if (type == typeof(decimal)) return "DECIMAL";
            if (type == typeof(float) || type == typeof(double)) return "FLOAT";
            if (type == typeof(DateTime)) return "TIMESTAMP";
            if (type == typeof(bool)) return "BOOLEAN";
            if (type == typeof(string)) return "VARCHAR(255)";
            if (type.IsEnum) return "INT";

            throw new Exception($"Unsupported CLR type: {t.Name}");
        }

        // helper to convert PascalCase to snake_case
        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var sb = new System.Text.StringBuilder();
            sb.Append(char.ToLowerInvariant(name[0]));

            for (int i = 1; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
