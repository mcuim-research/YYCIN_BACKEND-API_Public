using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using test3.Models;

public class Caregiver
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public  int CaregiverId { get; set; }  
    [Required]
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public int Age{get;set;}
    public int Gender{get;set;}
    [Required]
    public Account Account{ get; set; } 
    public List<Patient> Patients{get;set;}
    
}

public class CaregiverReceived{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public int Age{get; set;}
    public int Gender{get;set;}
    public AccountJsonRecieved Account{get;set;}
}