using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class UsageLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; } 
        public Account Account{get;set;} 
        public Medication Medication{get;set;}
        public DateTime DateTime { get; set; }
        public string Action { get; set; }
        public int Status { get; set; }
        public string Notes{get;set;}
        public SmartBoxMedication SmartBoxMedication{get;set;}
    }
}
