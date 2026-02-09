namespace Apolon_ADPC.ORM.Tracking
{
    public class TrackedEntity
    {
        public object Entity { get; set; }
        public Dictionary<string, object?> OriginalValues { get; set; } = new();
    }
}