using Apolon_ADPC.Models;
using Npgsql;

namespace Apolon_ADPC.ORM.Core
{
    public class UnitOfWork : IDisposable
    {
        private readonly DbConnectionManager _manager;
        private readonly NpgsqlTransaction _tx;

        public DbSet<Patient> Patients { get; }
        public DbSet<Checkups> Checkups { get; }
        public DbSet<Medication> Medications { get; }
        public DbSet<Prescription> Prescriptions { get; }

        public UnitOfWork(string conn)
        {
            _manager = new DbConnectionManager(conn);
            _tx = _manager.Connection.BeginTransaction();

            Patients = new DbSet<Patient>(_manager.Connection);
            Checkups = new DbSet<Checkups>(_manager.Connection);
            Medications = new DbSet<Medication>(_manager.Connection);
            Prescriptions = new DbSet<Prescription>(_manager.Connection);
        }

        public void Commit() => _tx.Commit();
        public void Rollback() => _tx.Rollback();

        public void Dispose() => _manager.Dispose();
    }

}
