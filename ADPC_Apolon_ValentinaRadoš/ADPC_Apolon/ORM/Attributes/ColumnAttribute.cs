namespace ADPC_Apolon.ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsNullable { get; set; } = true;
        public bool IsUnique { get; set; }
        public object? DefaultValue { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
