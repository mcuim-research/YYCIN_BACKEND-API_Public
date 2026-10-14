using System.Xml;
using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
namespace test3.Controllers{
    [ApiController]
    [Route("[Controller]")]
    public class SmartBoxMedicationController: ControllerBase{
    ISmartBoxMedicationRepo _smartboxM;
    IAccountRepository _acc;
    IMedicationRepository _med;
    IUsageLogRepo _usage;
    ISmartBoxRepo _smartbox;
    public SmartBoxMedicationController(ISmartBoxMedicationRepo smartBoxM, IAccountRepository _a, IMedicationRepository _m, IUsageLogRepo _u, ISmartBoxRepo smartBoxRepo){
        this._smartboxM = smartBoxM;
        _acc = _a;
        _med = _m;
        _usage = _u;
        _smartbox = smartBoxRepo;
    }

    [HttpGet("medboxid/{id}")]
    public ActionResult GetSmartBoxMedicationBox(int id){
        SmartBoxMedication res;
       try{
              res = _smartboxM.GetSmartBoxMedication(id);

       }catch(Exception e){
                StatusCode(500);
                return BadRequest(e.Message);
       } 
       return Ok(res);
    }
    [HttpGet("admin")]
    public ActionResult GetAllSmartBoxMedicationsAdmin(){
        return Ok(_smartboxM.GetAllSmartBoxMedications());
    }
    [HttpGet("smartboxid/{id}")]
    public ActionResult GetAllSmartBoxMedications(int id){
        return Ok(_smartboxM.GetAllSmartBoxMedications(id));
    }

    [HttpPut("")]
    public ActionResult UpdateSmartBoxMedication(SmartBoxMedicationDTO dto){
        System.Diagnostics.Debug.WriteLine("UpdateSmartBoxMedication Recieved DTO:");
        System.Diagnostics.Debug.WriteLine(dto);
        SmartBoxMedication trulyOrig = _smartboxM.GetSmartBoxMedication(dto.SmartBoxMedicationID);
        SmartBoxMedication? orig = _smartboxM.GetSmartBoxMedication(dto.SmartBoxMedicationID);

            int status = -1;
        if(orig == null){
            return NotFound();
        }

        try{
            // Not implemented
            if(dto.Quantity < orig.Quantity){
                status = 1;
            }else if(dto.Quantity > orig.Quantity){
                status = 0;
            }
            if(dto.MedicationID == 0){
                orig.Medication = null;
            }else{
                orig.Medication = _med.GetMedicine((int)dto.MedicationID);
            }
            System.Diagnostics.Debug.WriteLine(orig.Medication);



            orig.Quantity = dto.Quantity;
            orig.Split = dto.Split;
            _smartboxM.UpdateSmartBoxMedication(orig);
            UsageLog ul = new UsageLog();
            if(orig.Medication != null & trulyOrig.Medication!= null && orig.Medication.MedicationId == trulyOrig.Medication.MedicationId){
                if(orig.Quantity == trulyOrig.Quantity){
                    return Ok();
                }
            }

            if(orig.Medication == null && trulyOrig.Medication == null){
                return Ok();
            }
            if(orig.Medication == null && trulyOrig.Medication != null){
                status = 1;
                ul.Medication = trulyOrig.Medication;
 
            }else if(orig.Medication != null && trulyOrig.Medication == null){
                status = 0;
                ul.Medication = orig.Medication;
            }else{
                ul.Medication = orig.Medication;
            }
            ul.SmartBoxMedication = orig;
            ul.DateTime = DateTime.Now;
            ul.Status = status;
            if(orig.Medication != null & trulyOrig.Medication!= null && orig.Medication.MedicationId != trulyOrig.Medication.MedicationId){
                status = 1;
                ul.Medication = trulyOrig.Medication;
            }
            //TODO: Change this to actual action
            ul.Action = "中午";
            ul.Account = _acc.GetPatient(_smartbox.GetSmartBox(dto.SmartBoxID).PatientID.PatientID).Account;
            if(ul.Account == null){
                return BadRequest("Account not found");
            }
            _usage.AddUsageLog(ul);
                if(orig.Medication != null & trulyOrig.Medication!= null && orig.Medication.MedicationId != trulyOrig.Medication.MedicationId){
                    status = 0;
                    ul.Medication = orig.Medication;
                    _usage.AddUsageLog(ul);
                }
        
        }catch(Exception e){
            return BadRequest(e.Message);

        }
        return Ok();
    }   
    [HttpPost("")]
    public ActionResult AddSmartBoxMedication(SmartBoxMedicationAddingDTO dto)
    {
        SmartBoxMedication res = new SmartBoxMedication();
        try{
            res.Pos = dto.Pos;
            res.SmartBoxID = dto.SmartBoxID;
            _smartboxM.AddSmartBoxMedication(res);
        }catch(Exception e){
            return BadRequest(e.Message);
        }
        return Ok();
    }
    
    public class SmartBoxMedicationAddingDTO{
        public int SmartBoxID{get;set;}
        public int Pos {get;set;}
    }
}
}