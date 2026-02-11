using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrations
{
    public static class MigrationDiff
    {
        public static (string upSql, string downSql) GenerateSql(ModelSnapshot oldSnap, ModelSnapshot newSnap)
        {
            var upSql = new StringBuilder();
            var downSql = new StringBuilder();

            foreach (var tablePair in newSnap.Tables)
            {
                var tableName = tablePair.Key;
                var newTable = tablePair.Value;

                if (!oldSnap.Tables.TryGetValue(tableName, out var oldTable))
                {
                    // New table
                    upSql.AppendLine(GenerateCreateTable(newTable));
                    downSql.Insert(0, $"DROP TABLE IF EXISTS {tableName};\n"); // prepend for rollback order
                    continue;
                }

                // Compare columns
                var oldCols = oldTable.Columns;
                var newCols = newTable.Columns;

                foreach (var col in newCols.Where(c => !oldCols.Any(oc => oc.Name == c.Name)))
                {
                    upSql.AppendLine($"ALTER TABLE {tableName} ADD COLUMN {GenerateColumnSql(col)};");
                    downSql.Insert(0, $"ALTER TABLE {tableName} DROP COLUMN {col.Name};\n");
                }

                foreach (var col in oldCols.Where(c => !newCols.Any(nc => nc.Name == c.Name)))
                {
                    upSql.AppendLine($"ALTER TABLE {tableName} DROP COLUMN {col.Name};");
                    downSql.Insert(0, $"ALTER TABLE {tableName} ADD COLUMN {GenerateColumnSql(col)};\n");
                }
            }

            return (upSql.ToString(), downSql.ToString());
        }

        private static string GenerateCreateTable(TableSnapshot table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {table.Name} (");

            var colDefs = table.Columns.Select(GenerateColumnSql);
            sb.AppendLine(string.Join(",\n", colDefs));

            // Add foreign keys
            foreach (var col in table.Columns.Where(c => c.ForeignKeyTable != null))
            {
                sb.AppendLine($", FOREIGN KEY ({col.Name}) REFERENCES {col.ForeignKeyTable}({col.ForeignKeyColumn})");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        private static string GenerateColumnSql(ColumnSnapshot col)
        {

            if (col.IsPrimaryKey)
            {
                return $"{col.Name} SERIAL PRIMARY KEY"; // always NOT NULL, no extra NULL/NOT NULL
            }

            var parts = new List<string>
            {
                col.Name,
                col.Type,
                col.IsNullable ? "NULL" : "NOT NULL"
            };

            if (col.IsUnique) parts.Add("UNIQUE");

            if (col.Default != null)
            {
                string defaultSql = col.Default switch
                {
                    string s => s,
                    bool b => b ? "TRUE" : "FALSE",
                    int or long or float or double or decimal => col.Default.ToString(),
                    _ => throw new Exception($"Unsupported default value type: {col.Default.GetType().Name}")
                };

                parts.Add($"DEFAULT {defaultSql}");
            }

            if (!string.IsNullOrEmpty(col.ForeignKeyTable))
                parts.Add($"REFERENCES {col.ForeignKeyTable}({col.ForeignKeyColumn}) ON DELETE CASCADE");

            return string.Join(" ", parts);
        }
    }

}
