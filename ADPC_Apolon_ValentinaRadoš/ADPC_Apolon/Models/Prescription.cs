using ADPC_Apolon.ORM.Attributes;

namespace ADPC_Apolon.Models
{
    [Table("prescriptions")]
    public class Prescription
    {
        [PrimaryKey]
        public int Id { get; set; }

        [ForeignKey("patients")]
        [Column("patient_id", IsNullable = false)]
        public int PatientId { get; set; }

        [ForeignKey("medications")]
        [Column("medication_id", IsNullable = false)]
        public int MedicationId { get; set; }

        [Column("dosage", IsNullable = false)]
        public string Dosage { get; set; }

        [Column("start_date", IsNullable = false)]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        // Navigation properties
        public Patient Patient { get; set; }
        public Medication Medication { get; set; }
    }

    public class PrescriptionCU
    {
        public int PatientId { get; set; }
        public int MedicationId { get; set; }

        public string Dosage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
