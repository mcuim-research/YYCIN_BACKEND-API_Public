using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReminderID { get; set; }

        [Required]
        public DateTime Time { get; set; }
        
        public int Status { get; set; }
        public SmartBoxMedication SmartBoxMedication { get; set; }
        public Prescription Prescription { get; set; }

        
    }
}
