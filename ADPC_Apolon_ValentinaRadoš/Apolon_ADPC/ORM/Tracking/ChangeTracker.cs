using System.Reflection;

namespace Apolon_ADPC.ORM.Tracking
{
    public class ChangeTracker
    {
        private readonly List<TrackedEntity> _tracked = new(); //entities currently tracked

        public void Track(object entity) //shows which entity to track and save original values in snapshot
        {
            var snapshot = new TrackedEntity
            {
                Entity = entity
            };

            foreach (var prop in entity.GetType().GetProperties())
                snapshot.OriginalValues[prop.Name] = prop.GetValue(entity);

            _tracked.Add(snapshot);
        }

        public IEnumerable<(object Entity, List<string> ChangedProps)> DetectChanges()//compares the current object with its original snapshot.
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
