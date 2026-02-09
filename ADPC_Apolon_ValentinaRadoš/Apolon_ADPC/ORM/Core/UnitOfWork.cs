using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Attributes;
using Apolon_ADPC.ORM.Tracking;
using Npgsql;
using System.Reflection;

namespace Apolon_ADPC.ORM.Core
{
    public class UnitOfWork : IDisposable
    {
        private readonly DbConnectionManager _manager;
        private readonly NpgsqlTransaction _tx;

        public ChangeTracker Tracker { get; } = new();

        public DbSet<Patient> Patients { get; }
        public DbSet<Checkups> Checkups { get; }
        public DbSet<Medication> Medications { get; }
        public DbSet<Prescription> Prescriptions { get; }

        public UnitOfWork(string conn)
        {
            _manager = new DbConnectionManager(conn);
            _tx = _manager.Connection.BeginTransaction();

            Patients = new DbSet<Patient>(_manager.Connection, this);
            Checkups = new DbSet<Checkups>(_manager.Connection, this);
            Medications = new DbSet<Medication>(_manager.Connection, this);
            Prescriptions = new DbSet<Prescription>(_manager.Connection, this);
        }

        //public void Commit() => _tx.Commit();
        public void Rollback() => _tx.Rollback();

        public NpgsqlTransaction Transaction => _tx;

        public void Dispose() => _manager.Dispose();

        public void LoadNavigation(object entity, string propertyName)
        {
            var prop = entity.GetType().GetProperty(propertyName);
            if (prop == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found on type '{entity.GetType().Name}'");

            var nav = prop.GetCustomAttribute<NavigationAttribute>();
            if (nav == null)
                throw new InvalidOperationException($"Property '{propertyName}' does not have NavigationAttribute");

            var dbSetType = typeof(DbSet<>).MakeGenericType(nav.TargetType);
            var method = dbSetType.GetMethod(
                "GetAll",
                new Type[] { typeof(string), typeof(string) } // string where, string orderBy
            );

            var fkProp = entity.GetType().GetProperty("Id");
            if (fkProp == null)
                throw new InvalidOperationException($"Entity '{entity.GetType().Name}' does not have an 'Id' property");

            var fkValue = fkProp.GetValue(entity);

            // FIX: find the property of type DbSet<targetType>
            var dbSetInstanceProp = this.GetType()
                .GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType
                                     && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                                     && p.PropertyType.GenericTypeArguments[0] == nav.TargetType);

            if (dbSetInstanceProp == null)
                throw new InvalidOperationException($"UnitOfWork does not contain DbSet<{nav.TargetType.Name}> property");

            var dbSetInstance = dbSetInstanceProp.GetValue(this);

            var result = method.Invoke(dbSetInstance, new object[] { $"{nav.ForeignKey}={fkValue}", null });

            prop.SetValue(entity, result);
        }



        public void Commit()
        {
            foreach (var change in Tracker.DetectChanges())
            {
                var entity = change.Entity;
                var type = entity.GetType();

                var id = (int)type.GetProperty("Id").GetValue(entity);

                var dbSet = this.GetType()
                    .GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .First(p => p.PropertyType.GenericTypeArguments[0] == type)
                    .GetValue(this);

                dbSet.GetType()
                    .GetMethod("Update")
                    .Invoke(dbSet, new[] { id, entity });
            }

            _tx.Commit();
        }
    }

}
