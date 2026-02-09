using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Mapping;

namespace Migrations
{
    public class ColumnSnapshot
    {
        public string Name { get; set; }
        public string Type { get; set; } = "VARCHAR(255)";
        public bool IsNullable { get; set; } = true;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public string? Default { get; set; }
        public string? ForeignKeyTable { get; set; }
        public string? ForeignKeyColumn { get; set; }
    }

    public class TableSnapshot
    {
        public string Name { get; set; } = "";
        public List<ColumnSnapshot> Columns { get; set; } = new();
    }

    public class ModelSnapshot
    {
        public Dictionary<string, TableSnapshot> Tables { get; set; } = new();
    }

    public static class SnapshotGenerator
    {
        public static ModelSnapshot Generate(params Type[] entities)
        {
            var snapshot = new ModelSnapshot();

            foreach (var t in entities)
            {
                var tableName = EntityMapper.GetTableName(t);
                var columns = EntityMapper.GetColumns(t) 
                    .Select(p => new ColumnSnapshot
                    {
                        Name = p.Name,
                        Type = MapClrTypeToSqlType(p.PropertyType),
                        IsNullable = !Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute)),
                        IsPrimaryKey = Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)),
                        IsUnique = false,
                    }).ToList();

                snapshot.Tables[tableName] = new TableSnapshot
                {
                    Name = tableName,
                    Columns = columns
                };
            }

            return snapshot;
        }

        private static string MapClrTypeToSqlType(Type t)
        {
            if (t == typeof(int)) return "INT";
            if (t == typeof(decimal)) return "DECIMAL";
            if (t == typeof(float) || t == typeof(double)) return "FLOAT";
            if (t == typeof(DateTime)) return "TIMESTAMP";
            return "VARCHAR(255)";
        }
    }
}
