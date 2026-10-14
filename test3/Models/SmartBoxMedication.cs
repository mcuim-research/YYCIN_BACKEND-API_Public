using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class SmartBoxMedication
    {
        [Required]
        public int Pos{get;set;}
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SmartBoxMedicationID { get; set; }
        public int SmartBoxID { get; set; }
        public float Split{get;set;}
        public int Quantity { get; set; }
        public Medication? Medication{get;set;}

      
    }
    public class SmartBoxMedicationDTO{
        public int SmartBoxMedicationID{get;set;}
        public int SmartBoxID{get;set;}
        public float Split{get;set;}
        public int Quantity{get;set;}
        public int? MedicationID{get;set;}
    }
        public class SmartBoxMedicationDB{
        public int Pos{get;set;}
        public int SmartBoxMedicationID{get;set;}
        public int SmartBoxID{get;set;}
        public float? Split{get;set;}
        public int? Quantity{get;set;}
        public int? MedicationID{get;set;}
    }
}
