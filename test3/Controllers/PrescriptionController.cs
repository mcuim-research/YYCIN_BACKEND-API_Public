using Microsoft.AspNetCore.Mvc;
using test3.Models;
using test3.Repo.AccountRepo;
using OpenAI;
using OpenAI.Assistants;
using System.ClientModel;

using System.Text.Json;
using Google.Cloud.Vision.V1;
using Google.Api;
namespace test3.Controllers{
    [ApiController]
    [Route("[controller]")]
    public class PrescriptionController: ControllerBase{
        public IPrescriptionRepository _pres;
        public IAccountRepository _acc;
        public PrescriptionController(IPrescriptionRepository _pres, IAccountRepository _acc){
            this._pres = _pres;
            this._acc = _acc;
        }
        [HttpGet("{id}")]
        public ActionResult GetPrescription(int id){
            try{
                Prescription res = _pres.GetPrescription(id);
            }catch(PrescriptionException e){
                StatusCode(404);
                return NotFound(e.Message);
            }
            return Ok(_pres.GetPrescription(id));
        }
        [HttpGet("medbox/{medboxid}")]
        public ActionResult GetPrescriptionsByMedBox(int medboxid){
            try{
                List<Prescription> res = _pres.GetPrescriptionByMedBox(medboxid);
            }catch(PrescriptionException e){
                StatusCode(404);
                return NotFound(e.Message);
            }
            return Ok(_pres.GetPrescriptionByMedBox(medboxid));
        }
        [HttpGet("")]
        public ActionResult GetAllUserPrescriptions(){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to view prescriptions");
            }
            string accountType = HttpContext.Session.GetString("AccountType");
            if(accountType == "Patient"){
                return Ok(_pres.GetPatientPrescriptions(_acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Patient.PatientID));
            }
            if(accountType == "Doctor"){
                List<Patient> doctorPatients = _acc.GetDoctor(_acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Doctor.DoctorID).Patients;
                List<Prescription> doctorPrescriptions = new();
                foreach(Patient p in doctorPatients){
                    doctorPrescriptions.AddRange(_pres.GetPatientPrescriptions(p.PatientID));
                } 
                return Ok(doctorPrescriptions);
            }
            if(accountType == "Caregiver"){
                var caregiverPatients = _acc.GetBindingRequestFromPatients(_acc.GetAccount((int)HttpContext.Session.GetInt32("id")).Caregiver.CaregiverId);
                List<Prescription> caregiverPrescriptions = new();
                foreach(Patient p in caregiverPatients){
                    caregiverPrescriptions.AddRange(_pres.GetPatientPrescriptions(p.PatientID));
                }
                return Ok(caregiverPrescriptions);
            }
            return NotFound("No prescriptions found");
        }
        [HttpGet("admin")]
        public ActionResult GetAllPrescriptionsAdmin(){
            return Ok(_pres.GetPrescriptions());
        }
        [HttpPost("")]
        public ActionResult AddPrescription([FromBody] Prescription pres){
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to add a prescription");
            }
            int accid = (int)HttpContext.Session.GetInt32("id");
            Account account = _acc.GetAccount(accid);
            AccountDTO dto = new AccountDTO(account);
            dto.CheckType();
            if(dto.AccountType == "Doctor"){
                Patient patient = _acc.GetPatient(pres.Patient.PatientID);
                if(patient.Doctors.Any(d=>d.DoctorID == account.Doctor.DoctorID)){
                    return Ok(_pres.AddPrescription(pres));
                }
            }
            if(dto.AccountType == "Patient" && pres.Patient.PatientID == accid){
                return Ok(_pres.AddPrescription(pres));
            }
            if(dto.AccountType == "Caregiver"){
                Patient patient = _acc.GetPatient(pres.Patient.PatientID);
                if(patient.Caregivers.Any(c=>c.CaregiverId == account.Caregiver.CaregiverId)){
                    return Ok(_pres.AddPrescription(pres));
                }
            }
            return NotFound();
        }
        [HttpDelete("{id}")]
        public ActionResult DeletePrescription(int id){
            int accid = (int)HttpContext.Session.GetInt32("id");
            Account account = _acc.GetAccount(accid);
            AccountDTO dto = new AccountDTO(account);
            dto.CheckType();
            if(CheckPermission(id)){
                _pres.DeletePrescription(id);
                return Ok("Prescription deleted");
            }
            return Unauthorized("You do not have permission to delete this prescription");
        }
        [HttpPut("")]
        public ActionResult UpdatePrescription(int id, [FromBody] Prescription pres){
            if(CheckPermission(id)){
                _pres.UpdatePrescription(pres);
                return Ok("Prescription updated");
            }
            return Unauthorized("You do not have permission to update this prescription");
        }

        [HttpPost("classifications/{patientid}")]
        public async Task<ActionResult> Classifications([FromForm]IFormFile ufile,[FromRoute]int patientid){
            
            if(HttpContext.Session.GetString("Email") == null){
                return Unauthorized("You need to be logged in to classify a prescription");
            }
            if(ufile == null){
                return BadRequest("No file uploaded");
            }
            if(!CheckAccountPermission(patientid)){
                return Unauthorized("You do not have permission to classify this prescription");
            }
            //test uploaded file format
            if(ufile.ContentType != "image/jpeg" && ufile.ContentType != "image/png" && ufile.ContentType != "image/jpg" && ufile.ContentType != "image/gif" && ufile.ContentType != "image/png" && ufile.ContentType != "image/webp" ){
                return BadRequest("File must be a jpeg, png, webp, gif image");
            }            


            
            using(var stream = new FileStream("wwwroot/"+ufile.FileName, FileMode.Create)){
                await ufile.CopyToAsync(stream);
                #pragma warning disable OPENAI001
                OpenAIClient client = new(Environment.GetEnvironmentVariable("OPENAPI_API_KEY"));
                // FileClient fileClient = client.GetFileClient();
                AssistantClient assistantClient = client.GetAssistantClient();

                Console.WriteLine("\r\r\r\r\r\n\n\n\n\n\n\n\n\n\n\n\r\r\r\r\r");
                Console.WriteLine(ufile.FileName);
                Console.WriteLine("\r\r\r\r\r\n\n\n\n\n\n\n\n\n\n\n\r\r\r\r\r");
                
                // OpenAIFileInfo fileInfo = fileClient.UploadFile("wwwroot/"+ufile.FileName, FileUploadPurpose.Vision);
                // string url = "https://platform.openai.com/fictitious-files/"+ufile.FileName;
                //Uri link = new(url);

                var googleClient = ImageAnnotatorClient.Create();
            
                var image = Image.FromFile("wwwroot/"+ufile.FileName);
                var response = googleClient.DetectText(image);
                string txt = "";
                foreach(var annotation in response){
                    if(annotation.Description != null){
                        txt += annotation.Description;
                    }
                }
                Assistant assistant = assistantClient.CreateAssistant(model:"gpt-6-luna");
                string dateTimeNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                // string prmpt = "請條列式藥物名稱、用量、並且輸出用藥時間：（如：一天三次、或需要時），需要時如果藥單有提供可以寫什麼情況下需要如腹瀉需要時。此外，是否需要分成兩半服用？劑量？如果有提供，要服用幾天？";
                string prmpt1 = "請輸出藥物名稱、並且輸出用藥時間：早TakeAfterBreakfast、中TakeAfterLunch、晚TakeAfterDinner、睡前TakeBeforeBed，一天三次就是TakeAfterBreakfast=true、TakeAfterLunch=true、TakeAfterDinner=True、需要時TakeWhenNeeded：如果藥單有提供可以寫什麼情況下需要如腹瀉需要時，如果沒有，請輸出空值。splitHalf為是否需要分成兩半服用";
                // var ocr = new IronTesseract();
                
                // ocr.Language=OcrLanguage.ChineseTraditionalBest;
                // using (var ocrinput = new OcrInput())
                // {
                    // ocrinput.LoadImage("wwwroot/"+ufile.FileName);

                    // txt = ocr.Read(ocrinput).Text;
                // }

                // Console.WriteLine(txt);
                // AssistantThread thread = assistantClient.CreateThread(new ThreadCreationOptions()
                // {
                //     InitialMessages = {
                //         new ThreadInitializationMessage(MessageRole.User,new List<MessageContent>{
                //                 prmpt,MessageContent.FromImageFileId(fileInfo.Id)
                //             }), 
                        
                //     }
                // });

                // CollectionResult<StreamingUpdate> streamingUpdates = assistantClient.CreateRunStreaming(thread,assistant);


                // string txt = "";
                // foreach(StreamingUpdate update in streamingUpdates){
                //     if(update.UpdateKind == StreamingUpdateReason.RunCreated){
                //         Console.WriteLine("Run created");

                //     }
                //     if(update is MessageContentUpdate contentUpdate){
                //         Console.Write(contentUpdate.Text);
                //         txt += contentUpdate.Text;ㄇ
                //         //meds.Add(update.Message.Content);
                //     }

                // }
                AssistantThread th = assistantClient.CreateThread(new ThreadCreationOptions()
                {
                    InitialMessages = {
                        new ThreadInitializationMessage(MessageRole.User,new List<MessageContent>{
                                prmpt1,txt
                            }), 
                        
                    }
                });
                CollectionResult<StreamingUpdate> streamingUpdates1 = assistantClient.CreateRunStreaming(th.Id,assistant.Id,  new RunCreationOptions(){
                    ResponseFormat = AssistantResponseFormat.CreateJsonSchemaFormat(
                                                name:"meds",
                        jsonSchema: BinaryData.FromString(
                            """
                            {
                                "type": "object",
                                "properties": {
                                    "medications": {
                                        "type": "array",
                                        "items": {
                                            "type": "object",
                                            "properties": {
                                                "medication": {
                                                    "type": "string"
                                                },
                                                "HowManyTakePerWeek": { 
                                                    "type": "integer"
                                                },
                                                "takeAfterBreakfast": {
                                                    "type": "boolean"
                                                },
                                                "takeAfterLunch": {
                                                    "type": "boolean"
                                                },
                                                "takeAfterDinner": {
                                                    "type": "boolean"
                                                },
                                                "takeBeforeBed": {
                                                    "type": "boolean"
                                                },
                                                "takeWhenNeeded": {
                                                    "type": "string"
                                                },
                                                "splitHalf":{
                                                    "type": "boolean"
                                                
                                                }

                                            },
                                            "required": ["medication","HowManyTakePerWeek","takeAfterBreakfast","takeAfterLunch","takeAfterDinner","takeBeforeBed","splitHalf"],
                                            "additionalProperties": false

                                        },
                                    "required": ["medications"],
                                    "additionalProperties": false
                                    }


                                }
                            }
                            """
                        )

                    )
                });
string jsonStr = "";
Console.WriteLine($"Raw text content: {txt}"); // 檢查原始文本

foreach(StreamingUpdate update in streamingUpdates1)
{
    if(update.UpdateKind == StreamingUpdateReason.RunCreated)
    {
        Console.WriteLine("Run2 created");
    }
    if(update is MessageContentUpdate contentUpdate)
    {
        Console.WriteLine($"Content update received: {contentUpdate.Text}"); // 檢查每個更新
        jsonStr += contentUpdate.Text;
    }
}

Console.WriteLine($"Final jsonStr: {jsonStr}"); // 檢查最終字符串

if (string.IsNullOrEmpty(jsonStr))
{
    Console.WriteLine("streamingUpdates1 content:"); // 檢查完整的更新列表
    foreach(var update in streamingUpdates1)
    {
        Console.WriteLine($"Update type: {update.GetType()}");
        Console.WriteLine($"Update content: {update}");
        jsonStr += update.ToString();
    }
    return BadRequest("No JSON content received");
}
                var json = JsonSerializer.Deserialize<InnerPrescription>(jsonStr);
                if(json != null){
                    Prescription pres = new Prescription();
                    pres.MedicationDetails = new List<PrescriptionMedication>();
                    pres.Patient = _acc.GetPatient(patientid);
                    pres.Patient.Account.Password = null;
                    if(json.medications == null || json.medications.Count == 0){
                        return BadRequest("No medications found in prescription");
                    }
                        foreach(InnerPrescriptionMedication med in json.medications){
                        PrescriptionMedication pmed = new PrescriptionMedication();
                        pmed.Medication = new Medication
                        {
                            Name = med.medication,
                            AddedBy = _acc.GetAccount((int)HttpContext.Session.GetInt32("id"))
                            
                        
                        };
                        pmed.Medication.AddedBy.Password = null;
                        pmed.TakePerWk = med.HowManyTakePerWeek;
                        pmed.TakeAfterBreakfast = med.takeAfterBreakfast;
                        pmed.TakeAfterLunch = med.takeAfterLunch;
                        pmed.TakeAfterDinner = med.takeAfterDinner;
                        pmed.TakeBeforeBed = med.takeBeforeBed;
                        pmed.TakeWhenNeeded = med.takeWhenNeeded;
                        pmed.SplitMed = med.splitHalf;
                        pres.MedicationDetails.Add(pmed);
                    }
                    return Ok(pres);
                }

               return NoContent();


            }
        }
        class InnerPrescription{
            public List<InnerPrescriptionMedication> medications{get;set;}
        }
        class InnerPrescriptionMedication{
            public string medication{get;set;}
            public int HowManyTakePerWeek{get;set;}
            public bool takeAfterBreakfast{get;set;}
            public bool takeAfterLunch{get;set;}
            public bool takeAfterDinner{get;set;}
            public bool takeBeforeBed{get;set;}
            public string takeWhenNeeded{get;set;}
            public bool splitHalf{get;set;}
        }

        
        bool CheckAccountPermission(int patientId){
            if(HttpContext.Session.GetInt32("id") == null){
                return false;
            }
            int accid = (int)HttpContext.Session.GetInt32("id");
            Account account = _acc.GetAccount(accid);
            AccountDTO dto = new AccountDTO(account);
            dto.CheckType();
            if(_acc.GetPatient(patientId) == null){
                return false;
            }
            Account patientToCheck = _acc.GetAccount(_acc.GetPatient(patientId).Account.AccountID);
            AccountDTO dtoToCheck = new AccountDTO(patientToCheck);
            dtoToCheck.CheckType();
            if(dto.AccountType == "Patient" && dtoToCheck.AccountType == "Patient" && dto.AccountID == dtoToCheck.AccountID){
                return true;
            }

            if(dto.AccountType == "Doctor" && _acc.GetPatient(patientId).Doctors.Any(d=>_acc.GetDoctor(d.DoctorID).Account.AccountID == HttpContext.Session.GetInt32("id")) ||
             (dto.AccountType=="Caregiver" &&
              _acc.GetPatient(patientId).Caregivers != null &&
              _acc.GetPatient(patientId).Caregivers.Any(c=>c != null &&c.Account !=null &&
               c.Account.AccountID == HttpContext.Session.GetInt32("id"))) ){ 
                return true;
            }
            return false;
 
        }
        bool CheckPermission(int id){
            if(HttpContext.Session.GetString("Email") == null){
                return false;
            }
            Prescription pres = _pres.GetPrescription(id);
            if(pres == null){
                return false;
            }
            if(HttpContext.Session.GetInt32("id") == null){
                return false;
            }
            int accid = (int)HttpContext.Session.GetInt32("id");
            Account account = _acc.GetAccount(accid);
            AccountDTO dto = new AccountDTO(account);
            dto.CheckType();
            Console.WriteLine(dto.AccountType);
            if(( dto.AccountType == "Patient" && 
            pres.Patient.PatientID != _acc.GetAccount((int)HttpContext.Session.GetInt32("id") ).Patient.PatientID)||
             dto.AccountType == "Doctor" && _acc.GetPatient(pres.Patient.PatientID).Doctors.Any(d=>_acc.GetDoctor(d.DoctorID).Account.AccountID == HttpContext.Session.GetInt32("id")) ||
             (dto.AccountType=="Caregiver" && _acc.GetPatient(pres.Patient.PatientID).Caregivers.Any(c=>c.Account.AccountID == HttpContext.Session.GetInt32("id"))) ){ 
                return false;
            }
            return true;
        }
    }
}