using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientID { get; set; }
        [Required]
        public string Name { get; set; }
        public int Age { get; set; }
        public string PhoneNumber{get; set;}
        public int Gender{get;set;}
        [Required]
        public Account Account { get; set; }
        public List<Caregiver> Caregivers{get;set;}
        public List<Doctor> Doctors{get;set;}
        // NOT YET IMPLEMENTED INTO REPO
    }
    public class PatientRecieved{
        [Required]
        public string Name { get; set; }
        public int Age { get; set; }
        public string PhoneNumber{get; set;}
        public int Gender{get;set;}
        [Required]
        public AccountJsonRecieved Account { get; set; }
        
    }
}
