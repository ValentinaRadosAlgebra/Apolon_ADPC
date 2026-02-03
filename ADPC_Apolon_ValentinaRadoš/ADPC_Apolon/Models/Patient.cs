using ADPC_Apolon.ORM.Attributes;

namespace ADPC_Apolon.Models
{
    [Table("patients")]
    public class Patient
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("surname")]
        public string Surname { get; set; }

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("gender")]
        public Gender Gender { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("emergency_contact")]
        public string? EmergencyContact { get; set; }

        [Column("profile_created")]
        public DateTime ProfileCreated { get; set; }

        // Navigation properties
        public List<Checkups> Checkups { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
    }

    public class PatientCU //create and update
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string Adress { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? EmergencyContact { get; set; }
        public DateTime ProfileCreated { get; set; }
    }

    public enum Gender { F, M }
}

