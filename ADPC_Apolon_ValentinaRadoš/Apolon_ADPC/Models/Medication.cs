using Apolon_ADPC.ORM.Attributes;

namespace Apolon_ADPC.Models
{
    [Table("medications")]
    public class Medication
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Column("name", IsNullable = false)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("manufacturer")]
        public string? Manufacturer { get; set; }
    }

    public class MedicationCU
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
    }
}
