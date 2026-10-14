using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class Prescription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PrescriptionID { get; set; }
        [Required]
        public Patient Patient { get; set; }

        [Required]
        public List<PrescriptionMedication> MedicationDetails{get;set;}
    }
    public class PrescriptionDTO{
        [Key]
        [Required]
        public int PrescriptionID { get; set; }
        public int PatientID{get;set;}
        public List<int> Medications{get;set;}
        public PrescriptionDTO(Prescription pres){
            PrescriptionID = pres.PrescriptionID;
            PatientID = pres.Patient.PatientID;
            Medications = new List<int>();
            foreach(PrescriptionMedication med in pres.MedicationDetails){
                Medications.Add(med.Medication.MedicationId);
            }
        }
        public PrescriptionDTO(){}
    }
}
