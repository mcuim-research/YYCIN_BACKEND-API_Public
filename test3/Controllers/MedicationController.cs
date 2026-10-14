using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
namespace test3.Controllers
{
[ApiController]
[Route("[controller]")]
    public class MedicationController: ControllerBase{
        public IMedicationRepository _med;
        public IAccountRepository _acc;
        public MedicationController(IMedicationRepository _med, IAccountRepository _acc){
            this._med = _med;
            this._acc = _acc;
        }
    
        [HttpGet("{id}")]
        public ActionResult GetMedicine(int id){
            try{
                Medication res = _med.GetMedicine(id);
            }catch(MedicationException e){
                StatusCode(404);
                return NotFound(e.Message);
            }
            return Ok(_med.GetMedicine(id));
        }
        [HttpGet("")]
        public ActionResult GetAllMedicines(){
            return Ok(_med.GetMedicines());
        }
        [HttpPost("")]
        public ActionResult AddMedicine([FromBody] MedicationDTO med){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to add a medicine");
            }
            Medication m = new Medication();
            m.PermitID = med.PermitID;
            m.Name = med.Name;
            m.Description = med.Description;
            m.SideEffects = med.SideEffects;
            m.Size = med.Size;
            m.Imprint = med.Imprint;
            m.Shape = med.Shape;
            m.Color = med.Color;
            m.Dosage = med.Dosage;
            m.Frequency = med.Frequency;
            Console.WriteLine(HttpContext.Session.GetString("id"));
            m.AddedBy = _acc.GetAccount((int)HttpContext.Session.GetInt32("id"));
            Medication medres;
            try{
                medres = _med.AddMedicine(m);
                if(medres == null){
                    return BadRequest("Medicine already exists");
                }
            }catch(MedicationException e){
                return BadRequest(e.Message);
            }
            return Ok(medres);
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteMedicine(int id){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to delete a medicine");
            }
            Medication med = _med.GetMedicine(id);
            if(med == null){
                return NotFound("Medicine not found");
            }
            if(med.AddedBy.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You can only delete medicines you added");
            }
            _med.DeleteMedicine(id);
            return Ok("Medicine deleted");
        }
        [HttpPut("{id}")]
        public ActionResult UpdateMedicine(int id, [FromBody] MedicationDTO med){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to update a medicine");
            }
            Medication m = _med.GetMedicine(id);
            if(m == null){
                return NotFound("Medicine not found");
            }
            if(m.AddedBy.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You can only update medicines you added");
            }
            m.Name = med.Name;
            m.Description = med.Description;
            m.SideEffects = med.SideEffects;
            m.Size = med.Size;
            m.Imprint = med.Imprint;
            m.Shape = med.Shape;
            m.Color = med.Color;
            m.Dosage = med.Dosage;
            m.Frequency = med.Frequency;
            return Ok(_med.UpdateMedicine(m));
        }
        [HttpGet("identify/{color}/{shape}")]
        public ActionResult IdentifyMedicine(string color, string shape){
            return Ok(_med.SearchMedicineWithColorAndShape(color, shape));
        }

        
    }
}