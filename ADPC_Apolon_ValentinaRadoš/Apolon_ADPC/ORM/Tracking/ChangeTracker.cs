using System.Reflection;

namespace Apolon_ADPC.ORM.Tracking
{
    public class ChangeTracker
    {
        private readonly List<TrackedEntity> _tracked = new();

        public void Track(object entity)
        {
            var snapshot = new TrackedEntity
            {
                Entity = entity
            };

            foreach (var prop in entity.GetType().GetProperties())
                snapshot.OriginalValues[prop.Name] = prop.GetValue(entity);

            _tracked.Add(snapshot);
        }

        public IEnumerable<(object Entity, List<string> ChangedProps)> DetectChanges()
        {
            foreach (var tracked in _tracked)
            {
                var changed = new List<string>();

                foreach (var prop in tracked.Entity.GetType().GetProperties())
                {
                    var current = prop.GetValue(tracked.Entity);
                    var original = tracked.OriginalValues[prop.Name];

                    if (!Equals(current, original))
                        changed.Add(prop.Name);
                }

                if (changed.Any())
                    yield return (tracked.Entity, changed);
            }
        }
    }
}
