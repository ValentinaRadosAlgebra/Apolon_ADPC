using Apolon_ADPC.ORM.Attributes;
using System.Reflection;

namespace Apolon_ADPC.ORM.Mapping
{
    public static class EntityMapper
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

        //public static IEnumerable<(PropertyInfo Property, ColumnAttribute Column)>GetInsertableColumns(Type type)
        //{
        //    return type.GetProperties()
        //        .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() == null)
        //        .Select(p => (Property: p, Column: p.GetCustomAttribute<ColumnAttribute>()))
        //        .Where(t =>
        //            t.Column != null &&
        //            t.Column.DefaultValue == null
        //        );
        //}

        public static IEnumerable<(PropertyInfo Property, ColumnAttribute Column)> GetInsertableColumns(Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() == null) // skip PK
                .Select(p => (Property: p, Column: p.GetCustomAttribute<ColumnAttribute>()))
                .Where(t => t.Column != null); // remove the DefaultValue filter
        }
    }
}

