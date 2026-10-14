using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
namespace test3.Controllers{
    [ApiController]
    [Route("[Controller]")]
    public class TestRemindController: ControllerBase{
        ITestRemindRepo _testRemind;
        public TestRemindController(ITestRemindRepo testRemind){
            _testRemind = testRemind;
        }

        [HttpGet("{id}")]
        public ActionResult GetTestRemind(int id){
            try{
                TestRemind res = _testRemind.GetTestRemind(id);
            }catch(Exception e){
                StatusCode(500);
                return BadRequest(e.Message);
            }
            return Ok(_testRemind.GetTestRemind(id));
        }
        [HttpGet("")]
        public ActionResult GetAllTestReminds(){
            return Ok(_testRemind.GetAllTestReminds());
        }
        [HttpPost("")]
        public ActionResult AddTestRemind([FromBody]TestRemind testRemind){
            _testRemind.AddTestRemind(testRemind);
            return Ok();
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteTestRemind(int id){
            _testRemind.DeleteTestRemind(id);
            return Ok();
        }
        [HttpPut("")]
        public ActionResult UpdateTestRemind([FromBody]TestRemind testRemind){
            _testRemind.UpdateTestRemind(testRemind);
            return Ok();
        }
        
    }
}