using Apolon_ADPC.ORM.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Apolon_ADPC.Models
{

    [Table("checkups")]
    public class Checkups
    {
        [PrimaryKey]
        public int Id { get; set; }

        [ForeignKey("patients")]
        [Column("patient_id", IsNullable = false)]
        public int PatientId { get; set; }

        [Column("type", IsNullable = false)]
        public CheckupType Type { get; set; }

        [Column("checkup_date", IsNullable = false)]
        public DateTime CheckupDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("diagnosis")]
        public string? Diagnosis { get; set; }

        // Navigation property
        public Patient Patient { get; set; }
    }


    public class CheckupCU
    {
        public int PatientId { get; set; }
        public CheckupType Type { get; set; }
        public DateTime CheckupDate { get; set; }
        public string? Notes { get; set; }
        public string? Diagnosis { get; set; }
    }

    public enum CheckupType
    {
        GP,
        BLOOD,
        [Display(Name = "X-RAY")]
        XRAY,
        CT,
        MRI,
        ULTRA,
        EKG,
        ECHO,
        EYE,
        DERM,
        DENTA,
        MAMMO,
        EEG
    }
}
