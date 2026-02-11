namespace Apolon_ADPC.ORM.Tracking
{
    public class TrackedEntity
    {
        public object Entity { get; set; } //property name
        public Dictionary<string, object?> OriginalValues { get; set; } = new(); //property value at the time of tracking
    }
}