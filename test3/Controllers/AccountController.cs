using System.Text.RegularExpressions;
using Google.Api;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
namespace test3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _acc;
        private readonly IMailingHelper _mail;
        private readonly IConfiguration _configuration;
        // private readonly IAccountFirebaseRepo _accFire;
        

        public AccountController(IAccountRepository _acc, IMailingHelper _mail, IConfiguration configuration)
        {
            this._acc = _acc;   
            this._mail = _mail;
            _configuration = configuration;
            // _accFire = accFire;
        }
        /// <summary>
        /// 使用ID獲取使用者資料
        /// </summary>
        /// <param name="id">使用者帳號id</param>
        /// <returns></returns> <summary>
        /// 
        /// <returns>AccountDTO物件</returns>
        [HttpGet("{id}/admin")]
        public ActionResult GetUserByID(int id){
            try{
                AccountDTO res = _acc.GetAccount(id);
            }catch(AccountNotFoundException e){
                StatusCode(404);
                return NotFound(e.Message);
            } 
            return Ok(_acc.GetAccount(id));

        }
        [HttpGet("admin")]
        public ActionResult GetAllUsers(){
            List<AccountDTO> accs = _acc.GetAllAccounts().ToList();
            foreach(AccountDTO acc in accs){
                acc.Password = null;
            }
            return Ok(accs);
        }
        /// <summary>
        /// 管理員工具，直接新增User
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("admin")]
        public ActionResult AddUser([FromBody] AccountJsonRecieved account){
            Account acc = new Account();
            acc.Email = account.Email;

            if(acc.Email == null || acc.Password == null || acc.Email == "" || acc.Password == ""){
                return BadRequest("Email or Password is missing");
            }

            // use regex to test email valid
            if(!EmailIsValid(acc.Email)){
                return BadRequest("Email is not valid");
            }
            if(acc.Password.Length < 8){
                return BadRequest("Password is too short");
            }
            
            acc.Password = Argon2.Hash(account.Password);
            
            try{
                string authCode = "";
                (acc,authCode) = _acc.AddAccount(acc);
                _mail.SendMail(acc.Email, "藥藥Check it Now帳號認證","藥藥Check it Now帳號確認",string.Format("您好：<br>請點擊以下連結以完成帳號註冊。<br><a href='{0}'>{0}</a>",BuildAuthenticationUrl(authCode)),"");
                acc.Password = null;
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok(acc);
        }
        [HttpDelete("{id}/admin")]
        public ActionResult DeleteUser(string id){
            
            if(_acc.DeleteAccount(id)){
                return Ok("Account deleted");
            }
            return NotFound("Account not found");
        }
        
        /// <summary>
        /// 直接更改低層的用戶資料
        /// </summary>
        /// <param name="id">用戶的id</param>
        /// <param name="account"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, [FromBody] AccountJsonRecieved account){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in");
            }
            if(HttpContext.Session.GetInt32("id")== null || HttpContext.Session.GetInt32("id") != id){
                return Unauthorized("You cannot modify other's account information");
            }
            Account acc = new Account();
            acc.Email = account.Email;
        
            acc.Password = Argon2.Hash(account.Password);
            acc.AccountID = id;
            return Ok(_acc.UpdateAccount(acc));
        }
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="account">帳戶名、密碼等</param>
        /// <returns>AccountDTO物件</returns> 
        [HttpPost("login")]
        public ActionResult Login([FromBody] AccountJsonRecieved account){
            if(account.Email == null || account.Password == null || account.Email == "" || account.Password == ""){
                return BadRequest("Email or Password is missing");
            }
            try{
                AccountDTO res = _acc.GetAccount(account.Email);
                if(res == null){
                    return NotFound("Account not found");
                }
                if(res.Doctor!= null){
                    return Unauthorized("Doctor is not an account");
                }
                if(Argon2.Verify(res.Password, account.Password)){

                    // Set the session
                    HttpContext.Session.SetString("Email", account.Email);  
                    HttpContext.Session.SetInt32("id",res.AccountID);
                    res.CheckType();
                    HttpContext.Session.SetString("AccountType",res.AccountType);


                    return Ok("Login successful");
                }
            }catch(Exception e){
                return Unauthorized(e.Message);
            }
            return Unauthorized("Login failed");

        }


        [HttpGet("logout")]
        public ActionResult Logout(){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in inorder to logout");
            }
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("id");
            HttpContext.Session.Remove("AccountType");
            

            return Ok("Logged out");
        }
        [HttpGet("session")]
        public ActionResult CheckSession(){
            if(HttpContext.Session.GetString("Email") != null){
                try{
                var acc= _acc.GetAccount(HttpContext.Session.GetString("Email"));
                if(acc != null){
                    acc.Password = null;
                    return Ok(acc);
                }
                }finally{
                }
            }
                    return Unauthorized("Not logged in");
        }

        [HttpGet("Patient/admin")]
        public ActionResult GetPatients(){
            return Ok(_acc.GetPatients());
        }
        [HttpGet("Patient/{id}/admin")]
        public ActionResult GetPatient(int id){
            Patient pat;
            try{
                pat = _acc.GetPatient(id);
            }catch(Exception e){
                return NotFound("Patient not found");
            }
            if(pat == null){
                return NotFound("Patient not found");
            }
            return Ok(pat);
        }
        [HttpPut("Patient")]
        public ActionResult UpdatePatient([FromBody] Patient pat){
            if(_acc.GetPatient(pat.PatientID).Account.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You cannot modify other's account information");
            }
            try{
                pat = _acc.UpdatePatient(pat);
            }catch(Exception){
                return StatusCode(500);
            }
           
            return Ok(pat);
        }
        [HttpPost("Patient/admin")]
        public ActionResult AddPatient([FromBody] Patient pat){
            pat = _acc.AddPatient(pat,false).Item1;
            return Ok(pat);

        }
        [HttpPost("Patient")]
        public ActionResult AddPatient([FromBody] PatientRecieved patReceived){
            Patient pat = new Patient();
            try{
                    if(patReceived.Account.Password == null){
                        return BadRequest("Password is missing");
                    }
                    if(patReceived.Account.Email == null){
                        return BadRequest("Email is missing");
                    }
                    patReceived.Account.Password = Argon2.Hash(patReceived.Account.Password);
                    string authCode = "";
                    pat.Name = patReceived.Name;
                    pat.Age = patReceived.Age;
                    pat.PhoneNumber = patReceived.PhoneNumber;
                    pat.Account = new Account();
                    pat.Account.Email = patReceived.Account.Email;
                    pat.Account.Password = patReceived.Account.Password;
                
                    (pat, authCode) = _acc.AddPatient(pat, true); 
                    _mail.SendMail(pat.Account.Email, "藥藥Check it Now帳號認證","藥藥Check it Now帳號確認",string.Format("您好：<br>請點擊以下連結以完成帳號註冊。<br><a href='{0}'>{0}</a>",BuildAuthenticationUrl(authCode)),"");

            }catch(Exception){
                return StatusCode(500);
            }
            return Ok(pat);
        }
        [HttpDelete("Patient/{id}/{deleteAccount}")]
        public ActionResult DeletePatient(int id, short deleteAccount){
            if(_acc.GetPatient(id).Account.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You cannot delete other's account information");
            }
            try{
                _acc.DeletePatient(id, deleteAccount == 1);
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok("Patient deleted");
        }
        [HttpDelete("Patient/{id}/{deleteAccount}/admin")]
        public ActionResult DeletePatientAdmin(int id, short deleteAccount){
            try{
                _acc.DeletePatient(id, deleteAccount == 1);
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok("Patient deleted");
        }
        [HttpGet("Caregiver/admin")]
        public ActionResult GetCaregivers(){
            return Ok(_acc.GetCaregivers());
        }
        [HttpGet("Caregiver/{id}/admin")]
        public ActionResult GetCaregiver(int id){
            Caregiver cg;
            try{
                cg = _acc.GetCaregiver(id);
            }catch(Exception e){
                return NotFound("Caregiver not found");
            }
            if(cg == null){
                return NotFound("Caregiver not found");
            }
            return Ok(cg);
        }
        [HttpPut("Caregiver/admin")]
        public ActionResult UpdateCaregiverAdmin([FromBody] Caregiver cg){
            try{
                cg = _acc.UpdateCaregiver(cg);
            }catch(Exception){
                return StatusCode(500);
            }
           
            return Ok(cg);
        }
        [HttpPut("Caregiver")]
        public ActionResult UpdateCaregiver([FromBody] Caregiver cg){
            if(_acc.GetCaregiver(cg.CaregiverId).Account.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You cannot modify other's account information");
            }
            try{
                cg = _acc.UpdateCaregiver(cg);
            }catch(Exception){
                return StatusCode(500);
            }
           
            return Ok(cg);
        }
        [HttpPost("Caregiver")]
        public ActionResult AddCaregiver([FromBody] CaregiverReceived cgr){

            try{
                if(cgr.Account != null){
                    if(cgr.Account.Password == null){
                        return BadRequest("Password is missing");
                    }
                    if(cgr.Account.Email == null){
                        return BadRequest("Email is missing");
                    }
                    cgr.Account.Password = Argon2.Hash(cgr.Account.Password);
                    Caregiver cg = new Caregiver();
                    cg.Name = cgr.Name;
                    cg.PhoneNumber = cgr.PhoneNumber;
                    cg.Account = new Account();
                    cg.Account.Email = cgr.Account.Email;
                    cg.Account.Password = cgr.Account.Password;

                    string authCode = "";

                    (cg, authCode) = _acc.AddCaregiver(cg, true);
                    _mail.SendMail(cg.Account.Email, "藥藥Check it Now帳號認證","藥藥Check it Now帳號確認",string.Format("您好：<br>請點擊以下連結以完成帳號註冊。<br><a href='{0}'>{0}</a>",BuildAuthenticationUrl(authCode)),"");
                    cg.Account.Password = null;
                    return Ok(cg);
                }
            }catch(Exception e){
                Console.WriteLine(e.Message);
                return StatusCode(500);
            }
            // return error if account is null
            return BadRequest("Account is missing");
        }
        [HttpDelete("Caregiver/{id}/{deleteAccount}")]
        public ActionResult DeleteCaregiver(int id, short deleteAccount){
            if(_acc.GetCaregiver(id).Account.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You cannot delete other's account information");
            }
            try{
                _acc.DeleteCaregiver(id, deleteAccount == 1);
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok("Caregiver deleted");
        }

        [HttpDelete("Caregiver/{id}/{deleteAccount}/admin")]
        public ActionResult DeleteCaregiverAdmin(int id, short deleteAccount){
            try{
                _acc.DeleteCaregiver(id, deleteAccount == 1);
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok("Caregiver deleted");
        }
        [HttpGet("doctor/admin")]
        public ActionResult GetDoctors(){
            return Ok(_acc.GetDoctors());
        }
        [HttpGet("doctor/{id}/admin")]
        public ActionResult GetDoctor(int id){
            Doctor doc;
            try{
                doc = _acc.GetDoctor(id);
            }catch(Exception){
                return NotFound("Doctor not found");
            }
            if(doc == null){
                return NotFound("Doctor not found");
            }
            return Ok(doc);
        }

        [HttpPut("doctor")]
        public ActionResult UpdateDoctor([FromBody] Doctor doc){
            try{
                doc = _acc.UpdateDoctor(doc);
            }catch(Exception){
                return StatusCode(500);
            }
           
            return Ok(doc);
        }
        [HttpPut("doctor/admin")]
        public ActionResult UpdateDoctorAdmin([FromBody] Doctor doc){
            if(_acc.GetDoctor(doc.DoctorID).Account.AccountID != HttpContext.Session.GetInt32("id")){
                return Unauthorized("You cannot modify other's account information");
            }
            try{
                doc = _acc.UpdateDoctor(doc);
            }catch(Exception){
                return StatusCode(500);
            }
           
            return Ok(doc);
        }
        [HttpPost("doctor")]
        public ActionResult AddDoctor([FromBody] Doctor doc){
            try{
                string randomPassword = System.Guid.NewGuid().ToString();
                doc.Account.Password = Argon2.Hash(randomPassword);
                doc = _acc.AddDoctor(doc, true);

                doc.Account.Password = randomPassword;
            }catch(Exception){
                return StatusCode(500);
            }
            return Ok(doc);
        }


        /// <summary>
        /// GetGiver
        /// Gets the doctor and caregiver of a given AccountID accoring to SESSION/// 
        /// </summary>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Giver")]
        public ActionResult GetGiver(){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in inorder to see your doctors and caregivers");
            }
            
            int accId = (int)HttpContext.Session.GetInt32("id");
            if(HttpContext.Session.GetInt32("id") == null){
                return Unauthorized("You have to be logged in inorder to see your doctors and caregivers, we cannot find your id in the server");
            }
            return Ok(_acc.GetAccount(accId));
        }
        [HttpGet("authenticate/{code}")]
        public ActionResult Authenticate(string code){
            if(_acc.AuthenticateAccount(code)){
                return Ok("Code is correct, account is now authenticated");
            }
            return Unauthorized("Code is not correct");
        }
        [HttpPost("Bind/caregiver/{caregiverid}")]
        public ActionResult BindPatientCaregiver(int caregiverid){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in");
            }
            if(HttpContext.Session.GetString("AccountType") != "Patient"){
                return Unauthorized("You have to be a patient to bind caregiver");
            }
            var patient = _acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Patient;
            var caregiver = _acc.GetCaregiver(caregiverid);
            if(caregiver == null){
                return NotFound("Caregiver not found");
            }
            if(patient == null){
                return NotFound("Patient not found");
            }
            
            _acc.BindCaregiver(patient, caregiver);
            return Ok("成功");

        }
        [HttpDelete("Bind/caregiver/{caregiverid}")]
        public ActionResult UnbindPatientCaregiver(int caregiverid){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in");
            }
            if(HttpContext.Session.GetString("AccountType") != "Patient"){
                return Unauthorized("You have to be a patient to unbind caregiver");
            }
            var patient = _acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Patient;
            var caregiver = _acc.GetCaregiver(caregiverid);
            if(caregiver == null){
                return NotFound("Caregiver not found");
            }
            if(patient == null){
                return NotFound("Patient not found");
            }
            _acc.UnbindCaregiver(patient, caregiver);
    
            return Ok("成功");

        }

        // [HttpGet("Bind/caregivers/accepted")]
        // public ActionResult GetAcceptedCaregivers(){
        //     if(HttpContext.Session.GetString("Email") == null){
        //         return Unauthorized("You have to be logged in");
        //     }
        //     if(HttpContext.Session.GetString("AccountType") != "Patient"){
        //         return Unauthorized("You have to be a patient to see your caregivers");
        //     }
        //     int patientid = _acc.GetPatient((int)HttpContext.Session.GetInt32("id")).PatientID;
        //     return Ok(_acc.GetAcceptedBindingRequestFromCaregivers(patientid));
        // }
        // [HttpGet("Bind/caregivers/pending")]
        // public ActionResult GetPendingCaregivers(){
        //     if(HttpContext.Session.GetString("Email") == null){
        //         return Unauthorized("You have to be logged in");
        //     }
        //     if(HttpContext.Session.GetString("AccountType") != "Patient"){
        //         return Unauthorized("You have to be a patient to see your caregivers");
        //     }
        //     int patientid = _acc.GetPatient((int)HttpContext.Session.GetInt32("id")).PatientID;
        //     return Ok(_acc.GetBindingRequestFromCaregivers(patientid));
        // }
        // 因為永豐要求，accepted跟pending的功能不需要
        // [HttpGet("Bind/patients/accepted")]
        // public ActionResult GetAcceptedPatients(){
        //     if(HttpContext.Session.GetString("Email") == null){
        //         return Unauthorized("You have to be logged in");
        //     }
        //     if(HttpContext.Session.GetString("AccountType") != "Caregiver"){
        //         return Unauthorized("You have to be a caregiver to see your patients");
        //     }
        //     int caregiverid = _acc.GetCaregiver((int)HttpContext.Session.GetInt32("id")).CaregiverId;
        //     return Ok(_acc.GetAcceptedBindingRequestFromPatients(caregiverid));
        // }
        // [HttpGet("Bind/patients/pending")]
        // public ActionResult GetPendingPatients(){
        //     if(HttpContext.Session.GetString("Email") == null){
        //         return Unauthorized("You have to be logged in");
        //     }
        //     if(HttpContext.Session.GetString("AccountType") != "Caregiver"){
        //         return Unauthorized("You have to be a caregiver to see your patients");
        //     }
        //     int caregiverid = _acc.GetCaregiver((int)HttpContext.Session.GetInt32("id")).CaregiverId;
        //     return Ok(_acc.GetBindingRequestFromPatients(caregiverid));
        // }
        [HttpGet("Bind")]
        public ActionResult GetBinded(){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You have to be logged in");
            }
            if(HttpContext.Session.GetString("AccountType") == "Caregiver"){
                int caregiverId = _acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Caregiver.CaregiverId;
                return Ok(_acc.GetBindingRequestFromPatients(caregiverId));
            }
            if(HttpContext.Session.GetString("AccountType") == "Patient"){
                int patientId = _acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Patient.PatientID;
                return Ok(_acc.GetBindingRequestFromCaregivers(patientId));
            }
            return Unauthorized("You have to be a caregiver or patient to see your binded users");

        }


        private string BuildAuthenticationUrl(string authCode)
        {
            return test3.Configuration.PublicUrls.Create(
                _configuration, "account/authenticate/" + Uri.EscapeDataString(authCode)).AbsoluteUri;
        }

        //Test Email Validitiy
        private static bool EmailIsValid(string email)
        { 
            string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

            return Regex.IsMatch(email, regex, RegexOptions.IgnoreCase);
        }
        



    };
}
