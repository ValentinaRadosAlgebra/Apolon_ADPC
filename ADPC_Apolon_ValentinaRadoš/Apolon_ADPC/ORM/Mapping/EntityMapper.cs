using Apolon_ADPC.ORM.Attributes;
using System.Reflection;

namespace Apolon_ADPC.ORM.Mapping
{
    public static class EntityMapper //helper that maps classes to db table
    {
        public static string GetTableName(Type type)
        {
            var table = type.GetCustomAttribute<TableAttribute>();
            return table?.Name ?? type.Name.ToLower();
        }

        public static IEnumerable<PropertyInfo> GetColumns(Type type)
        {
            return type.GetProperties()
                       .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null
                                || p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        }

        public static IEnumerable<(PropertyInfo Property, ColumnAttribute Column)> GetInsertableColumns(Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() == null) // skip PK, bc autoincrement
                .Select(p =>
                {
                    var colAttr = p.GetCustomAttribute<ColumnAttribute>();
                    return (Property: p, Column: colAttr);//property - entity, column - sql
                })
                .Where(t => t.Column != null); // only include properties with [Column]
        }
    }
}

