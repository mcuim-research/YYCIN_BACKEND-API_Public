using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
namespace test3.Controllers{
    [ApiController]
    [Route("[Controller]")]
    public class SmartBoxController: ControllerBase{
    ISmartBoxRepo _smartbox;
    IAccountRepository _acc;
    ISmartBoxMedicationRepo _smartBoxMedication;
    public SmartBoxController(ISmartBoxRepo smartBox, IAccountRepository _a, ISmartBoxMedicationRepo smartBoxMedication){
        _smartbox = smartBox;
        _acc = _a;
        _smartBoxMedication = smartBoxMedication;
    }

    [HttpGet("{id}")]
    public ActionResult GetSmartBox(int id){
        try{
            SmartBox res = _smartbox.GetSmartBox(id);
        }catch(Exception e){
            StatusCode(500);
            return BadRequest(e.Message);
        }
        return Ok(_smartbox.GetSmartBox(id));
    

    }
    [HttpGet("")]
    public ActionResult GetAllSmartBoxes(){
        return Ok(_smartbox.GetAllSmartBoxes());
    }
    /// <summary>
    /// 將藥盒綁定到病人中/// 
    ///使用者在藥盒上註冊，出現QRCode跟綁定碼，之後手機掃描QRCode完成綁定智慧藥盒/// 
    /// </summary>
    /// <param name="smartbox">
    /// int SmartBoxID藥盒本身的ID
    /// string location 一般是放在哪裡？/// 
    /// string BindingID///
    /// /// 
    /// /// }/// </param>
    /// <returns></returns>
    ///
    ///  
    [HttpPost("registerbox")]
    public ActionResult Register([FromBody]SmartBoxInit init){
        if(init.Location == null){
            return BadRequest("Location is required");
        }
        if(_smartbox.GetSmartBox(init.SmartBoxID) != null){
            return BadRequest("SmartBox already exists");
        }
        SmartBox s = new SmartBox();
        s.Location = init.Location;
        s.SmartBoxID = init.SmartBoxID;


        // ramdomly generate a binding string
        s.BindingID = Guid.NewGuid().ToString();
        _smartbox.AddSmartBox(s);
        SmartBoxBindingRegistration reg = new SmartBoxBindingRegistration();
        reg.SmartBoxID = s.SmartBoxID;
        reg.BindingID = s.BindingID;
        return Ok(reg);
    }
    [HttpPost("unregisterbox")]
    public ActionResult Unregister([FromBody]SmartBoxDTO smartbox){
        SmartBox s = _smartbox.GetSmartBox(smartbox.SmartBoxID);
        if(s == null){
            return BadRequest("SmartBox does not exist");
        }
        s.PatientID = null;
        s.BindingID = "";
        _smartbox.DeleteSmartBox(s.SmartBoxID);
        _smartBoxMedication.DeleteSmartBoxMedicationsUsingSmartBoxID(s.SmartBoxID);
        return Ok("Unregistered SmartBox");
    }
    [HttpPost("checkboxbinding")]
    public ActionResult CheckBinding([FromBody]SmartBoxInit init){
        var res = _smartbox.GetSmartBox(init.SmartBoxID);
        if(res == null){
            return BadRequest("SmartBox does not exist");
        }
        if(res.PatientID == null){
            return Ok("SmartBox is not bound to any patient");
        }
        return Ok("SmartBox is bound to a patient");
    }
    [HttpPost("bind/{forpatientid}")]
    public ActionResult BindSmartBox([FromBody] SmartBoxDTO smartbox, int forpatientid){
        if(HttpContext.Session.GetString("Email") == null){
            return Unauthorized("You need to be logged in to bind a smartbox");
        }
        if(HttpContext.Session.GetInt32("id") == null){
            return Unauthorized("You need to be logged in to bind a smartbox");
        }
        SmartBox orig = _smartbox.GetSmartBox(smartbox.SmartBoxID);
        AccountDTO accountDTO = _acc.GetAccount((int)HttpContext.Session.GetInt32("id"));
        accountDTO.CheckType();
        if(accountDTO.AccountType == "Patient" && accountDTO.AccountID != _acc.GetPatient(forpatientid).Account.AccountID){
            return Unauthorized("You do not have permission to bind this smartbox, for more information please contact the administrator");
        }
        if(orig == null){
            return BadRequest("SmartBox does not exist");
        }
        if(smartbox.BindingID == null){
            return BadRequest("BindingID is required");
        }
        if(orig.BindingID != smartbox.BindingID){
            return BadRequest("BindingID is incorrect");
        }

        orig.PatientID = _acc.GetPatient(forpatientid);
        if(orig.PatientID == null){
            return BadRequest("Patient does not exist");
        }
        _smartbox.UpdateSmartBox(orig);
        return Ok("Added SmartBox");
    }
    [HttpGet("patient/{patientid}")]
    public ActionResult GetSmartBoxByPatient(int patientid){
        return Ok(_smartbox.GetSmartBoxByPatient(patientid));
    }
}
}