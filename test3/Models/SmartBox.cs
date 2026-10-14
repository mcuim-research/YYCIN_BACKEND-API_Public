using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class SmartBox
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SmartBoxID { get; set; }
        public string Location { get; set; }
        public Patient? PatientID { get; set; }
        public string BindingID{get;set;}
        public IEnumerable<SmartBoxMedication>? SmartBoxMedications{get;set;}
    }
    public class SmartBoxDTO{
        public int SmartBoxID{get;set;}
        public string BindingID{get;set;}
    }
    public class SmartBoxInit{
        public int SmartBoxID{get;set;}
        public string Location{get;set;}
    }
    public class SmartBoxBindingRegistration{
        public int SmartBoxID{get;set;}
        public string BindingID{get;set;}
    }
}
