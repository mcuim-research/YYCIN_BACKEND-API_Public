using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class Medication
    {
        public string PermitID{get;set;}
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicationId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string SideEffects { get; set; }
        public string Size{get;set;}
        public string Imprint{get;set;}
        public string Shape{get;set;}
        public string Color{get;set;}
        public string Dosage { get; set; }
        public string Frequency { get; set; } 
        public List<Prescription> Prescriptions{get;set;}
        public Account AddedBy{get;set;}    
      
    }
    public class MedicationDTO{
        public string PermitID{get;set;}
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicationId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string SideEffects { get; set; }
        public string Size{get;set;}
        public string Imprint{get;set;}
        public string Shape{get;set;}
        public string Color{get;set;}
        public string Dosage { get; set; }
        public string Frequency { get; set; } 
    }
    public class MedicationWithDate{
        public Medication Medication{get;set;}
        public int TakePerWk{get;set;}
        public int TakePerDay{get;set;}
        public float SplitMed{get;set;}
        public string Dosage{get;set;} // Original dosage text 
    }
}
