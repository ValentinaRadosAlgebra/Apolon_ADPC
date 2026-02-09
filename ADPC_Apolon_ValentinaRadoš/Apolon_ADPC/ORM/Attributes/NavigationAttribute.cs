namespace Apolon_ADPC.ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NavigationAttribute : Attribute
    {
        public Type TargetType { get; }
        public string ForeignKey { get; }

        public NavigationAttribute(Type targetType, string foreignKey)
        {
            TargetType = targetType;
            ForeignKey = foreignKey;
        }
    }
}
