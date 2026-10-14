using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using test3.Models;
namespace test3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsageLogController : ControllerBase
    {
        //whole class is untested!!!
        IUsageLogRepo _usageLogRepo;
        public UsageLogController(IUsageLogRepo usageLogRepo)
        {
            _usageLogRepo = usageLogRepo;
        }

        [HttpGet("admin")]
        public ActionResult GetUsageLogs()
        {
            return Ok(_usageLogRepo.GetAllUsageLogs());
        }
        [HttpGet("{id}/admin")]
        public ActionResult GetUsageLog(int id)
        {
            return Ok(_usageLogRepo.GetUsageLog(id));
        }
        [HttpPut("admin")]
        public ActionResult UpdateUsageLog(UsageLog usageLog)
        {
            _usageLogRepo.UpdateUsageLog(usageLog);
            return Ok();
        }
        [HttpGet("{smartBoxID}")]
        public ActionResult GetUsageLogsBySmartBox(int smartBoxID)
        {
            return Ok(_usageLogRepo.GetAllUsageLogsBySmartBox(smartBoxID));
        }
        [HttpGet("{smartBoxID}/latest50")]
        public ActionResult GetTop50UsageLogsBySmartBox(int smartBoxID)
        {
            return Ok(_usageLogRepo.GetLatest50UsageLogsBySmartBox(smartBoxID));
        }
        [HttpGet("date/{date}/smartBox/{smartBoxID}")]
        public ActionResult GetUsageLogsByDateAndSmartBox(DateTime date, int smartBoxID)
        {
            return Ok(_usageLogRepo.GetUsageLogsByDateAndSmartBox(smartBoxID, date));
        }
        [HttpPost("admin")]
        public ActionResult AddUsageLogAdmin(UsageLog usageLog)
        {
            _usageLogRepo.AddUsageLog(usageLog);
            return Ok();
        }
        [HttpDelete("{id}/admin")]
        public ActionResult DeleteUsageLog(int id)
        {
            _usageLogRepo.DeleteUsageLog(id);
            return Ok();
        }
    }
}