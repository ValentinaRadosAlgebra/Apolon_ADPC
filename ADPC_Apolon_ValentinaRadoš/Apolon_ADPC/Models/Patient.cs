using Apolon_ADPC.ORM.Attributes;

namespace Apolon_ADPC.Models
{
    [Table("patients")]
    public class Patient
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Column("name", IsNullable = false)]
        public string Name { get; set; }

        [Column("surname", IsNullable = false)]
        public string Surname { get; set; }

        [Column("date_of_birth", IsNullable = false)]
        public DateTime DateOfBirth { get; set; }

        [Column("gender", IsNullable = false)]
        public string Gender { get; set; }

        [Column("address", IsNullable = false)]
        public string Address { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email", IsUnique = true)]
        public string? Email { get; set; }

        [Column("emergency_contact")]
        public string? EmergencyContact { get; set; }

        [Column("profile_created", DefaultValue = "CURRENT_TIMESTAMP")]
        public DateTime ProfileCreated { get; set; }

        // Navigation properties
        [Navigation(typeof(Checkups), "patient_id")]
        public List<Checkups> Checkups { get; set; }

        [Navigation(typeof(Prescription), "patient_id")]
        public List<Prescription> Prescriptions { get; set; }
    }

    public class PatientCU //create and update
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? EmergencyContact { get; set; }
    }
}

