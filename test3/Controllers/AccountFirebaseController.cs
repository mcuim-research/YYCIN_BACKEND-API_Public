using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;

namespace test3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountFirebaseController : ControllerBase
    {
        private readonly IAccountFirebaseRepo _accFire;
        private readonly IAccountRepository _accRepo;
        public AccountFirebaseController(IAccountFirebaseRepo accFire, IAccountRepository accRepo)
        {
            _accFire = accFire;
            _accRepo = accRepo;
        }

        [HttpPost("")]
        public IActionResult AddAccountFirebase([FromBody] dynamic token)
        {
            string tokenString = token.Token;
            AccountFirebase accFire = new AccountFirebase();

            //Check if user is logged in and if the token is valid
            if (tokenString == null)
            {
                return BadRequest("Token is null");
            }
            if (_accFire.checkTokenExisits(tokenString))
            {
                return BadRequest("Token has already been used");
            }
            if(HttpContext.Session.GetInt32("id") == null){
                return BadRequest("User is not logged in");
            }
            accFire.AccountID = (int)HttpContext.Session.GetInt32("id");
            accFire.Token = tokenString;
            _accFire.AddAccountFirebase(accFire);
            return Ok("Token added");

        }
    }
}