using Microsoft.AspNetCore.Mvc;

namespace test3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnvLogController:ControllerBase{
        IEnvLogRepo _envLogRepo;
        ISmartBoxRepo _smartBoxRepo;
        public EnvLogController(IEnvLogRepo envLogRepo, ISmartBoxRepo smartBoxRepo)
        {
            _envLogRepo = envLogRepo;
            _smartBoxRepo = smartBoxRepo; 
        }
        [HttpGet("admin")]
        public ActionResult GetEnvLogs(){
            return Ok(_envLogRepo.GetAllEnvLogs());
        }
        [HttpGet("{id}/admin")]
        public ActionResult GetEnvLog(int id){
            return Ok(_envLogRepo.GetEnvLog(id));
        }
        [HttpPut("admin")]
        public ActionResult UpdateEnvLog(EnvLog envLog){
            _envLogRepo.UpdateEnvLog(envLog);
            return Ok();
        }
        [HttpGet("{smartBoxID}")]
        public ActionResult GetEnvLogsBySmartBox(int smartBoxID){
            return Ok(_envLogRepo.GetEnvLogsBySmartBox(smartBoxID));
        }
        [HttpGet("{smartBoxID}/latest")]
        public ActionResult GetLatestEnvLogBySmartBox(int smartBoxID){
            return Ok(_envLogRepo.GetLatestEnvLogBySmartBox(smartBoxID));
        }
        [HttpGet("date/{date}/smartBox/{smartBoxID}")]
        public ActionResult GetEnvLogsByDateAndSmartBox(DateTime date, int smartBoxID){
            return Ok(_envLogRepo.GetEnvLogsBySmartBoxAndDate(smartBoxID, date));
        }
        [HttpGet("date/{date}")]
        public ActionResult GetEnvLogsByDate(DateTime date){
            return Ok(_envLogRepo.GetEnvLogsByDate(date));
        }

        [HttpPost("admin")]
        public ActionResult AddEnvLogAdmin(EnvLog envLog){
            
            _envLogRepo.AddEnvLog(envLog);
            return Ok();
        }
        
        [HttpPost("")]
        public ActionResult AddEnvLog(EnvLogDTO dto){
            Models.SmartBox smartBox = _smartBoxRepo.GetSmartBox(dto.smartBoxID);
            EnvLog envLog = new EnvLog{
                Temperature = dto.Temperature,
                Humidity = dto.Humidity,
                DateTime = DateTime.Now,
                SmartBox = smartBox
            };
            _envLogRepo.AddEnvLog(envLog);
            return Ok();
        } 


    }
}