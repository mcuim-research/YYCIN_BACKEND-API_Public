using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test3.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountID { get; set; }
        [StringLength(30)]
        public string? Email { get; set; }
        public string? Password { get; set; }
        [DefaultValue(true)]
        public Boolean? IsValid { get; set; } 
        public Patient? Patient{get;set;}
        public Caregiver? Caregiver{get;set;}
        public Doctor? Doctor{get;set;}

    }
    public class AccountDTO: Account{
        [Required]
        public string AccountType{get;set;}

        public AccountDTO(Account acc){
            AccountID = acc.AccountID;
            Email = acc.Email;
            Password = acc.Password;
            IsValid = acc.IsValid;
            Patient = acc.Patient;
            Caregiver = acc.Caregiver;
            Doctor = acc.Doctor;
            CheckType();

        }
        public AccountDTO(){}
        public void CheckType(){
            if(Patient != null){
                this.AccountType = "Patient";
            }else if(Caregiver != null){
                AccountType = "Caregiver";
            }else if(Doctor != null){
                AccountType = "Doctor";
            }
            
        }
    }

    public class AccountJsonRecieved{
        [Required]
        public string Email {get;set;}
        public string Password {get;set;}


    }
}
