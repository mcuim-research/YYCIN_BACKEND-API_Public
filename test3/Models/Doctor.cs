using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using test3.Controllers;
using test3.Models;

public class Doctor{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DoctorID{
        get;set;
    }

    public Account Account{
        get;set;
    }
    public string Name{
        get;set;
    }
    public List<Patient> Patients{
        get;set;
    }// NOT YET IMPLEMENTED INTO REPO

}