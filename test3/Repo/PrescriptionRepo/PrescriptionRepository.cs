using System.Data;
using System.Data.SqlClient;
using Dapper;
using test3.Models;
using test3.Repo.AccountRepo;

public interface IPrescriptionRepository
{
    public Prescription AddPrescription(Prescription pres);
    public void DeletePrescription(int id);
    public List<Prescription> GetPrescriptions();
    public Prescription? GetPrescription(int id);
    public Prescription UpdatePrescription(Prescription pres);
    public List<Prescription> GetPatientPrescriptions(int id);
    List<Prescription> GetPrescriptionByMedBox(int id);

}
public class PrescriptionRepository : IPrescriptionRepository
{
    string _connectionString;
    IMedicationRepository _medRepo;
    IAccountRepository _accRepo;
    ISmartBoxRepo _box;
    public PrescriptionRepository(IConfiguration conf, IMedicationRepository med, IAccountRepository acc, ISmartBoxRepo _box){
        _connectionString = conf.GetConnectionString("DevConnection");
        _medRepo = med;
        _accRepo = acc;
        this._box = _box;
    }
    public void checkConnOpen(IDbConnection conn){
        if(conn.State == ConnectionState.Closed){
            conn.Open();
        }
    }
    public Prescription AddPrescription(Prescription pres)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            //check if the patient exists
            string sqlPatient = @"select * from Patient where PatientID = @id";
            DynamicParameters paramPatient = new DynamicParameters();
            paramPatient.Add("@id",pres.Patient.PatientID);
            Patient pat = conn.Query<Patient>(sqlPatient,paramPatient).FirstOrDefault();
            if(pat == null){
                throw new PrescriptionException("Patient not found");
            }
            //check if the medications exist
            foreach(PrescriptionMedication med in pres.MedicationDetails){
                Medication m = _medRepo.GetMedicine(med.Medication.MedicationId);
                if(m == null){
                    throw new PrescriptionException(""+med.Medication.Name+" Medication not found");
                }
            }
            string sql = @"insert into Prescription (PatientID) values(@pat); SELECT CAST(SCOPE_IDENTITY() as int)";
            DynamicParameters param = new DynamicParameters();
            param.Add("@pat",pres.Patient.PatientID);
            // execute the query and get the id of the inserted prescription
            int result = conn.Query<int>(sql,param).Single();
            if(result == 0){
                throw new PrescriptionException("an error occured while adding the prescription");
            }
            // get the id of the inserted prescription
            // each patient has more then one prescriptions
            
            string sqlGet = @"select * from Prescription where PrescriptionID = @id";
            DynamicParameters paramGet = new DynamicParameters();
            paramGet.Add("@id",result);
            Prescription p = conn.Query<Prescription>(sqlGet,paramGet).FirstOrDefault();
            if(p != null){
                foreach(PrescriptionMedication med in pres.MedicationDetails){
                    string sqlPresMed = @"insert into PrescriptionMedication (PrescriptionID,MedicationID,TakePerWk,SplitMed, Dosage, StartDate, EndDate, TakeAfterBreakfast, TakeAfterLunch, TakeAfterDinner, TakeBeforeBed, TakeWhenNeeded) values(@pres,@med, @takeWk, @takeDay, @split, @dosage, @start, @end, @takeAfterBreakfast, @takeAfterLunch, @takeAfterDinner, @takeBeforeBed, @takeWhenNeeded)";
                    DynamicParameters paramPresMed = new DynamicParameters();
                    paramPresMed.Add("@pres",p.PrescriptionID);
                    paramPresMed.Add("@med",med.Medication.MedicationId);
                    paramPresMed.Add("@takeWk",med.TakePerWk);
                    paramPresMed.Add("@split",med.SplitMed);
                    paramPresMed.Add("@dosage",med.Dosage);
                    paramPresMed.Add("@start",med.StartDate);
                    paramPresMed.Add("@end",med.EndDate);
                    paramPresMed.Add("@takeAfterBreakfast",med.TakeAfterBreakfast);
                    paramPresMed.Add("@takeAfterLunch",med.TakeAfterLunch);
                    paramPresMed.Add("@takeAfterDinner",med.TakeAfterDinner);
                    paramPresMed.Add("@takeBeforeBed",med.TakeBeforeBed);
                    paramPresMed.Add("@takeWhenNeeded",med.TakeWhenNeeded);
                    int resultPresMed = conn.Execute(sqlPresMed,paramPresMed);
                    if(resultPresMed == 0){
                        throw new PrescriptionException("an error occured while adding the prescription");
                    }
                }
                return p;
            }else{
                throw new PrescriptionException("an error occured while adding the prescription");
            }
        }
    }
    public void DeletePrescription(int id)
    {
        using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            //check if the prescription exists
            string sqlCheck = @"select * from Prescription where PrescriptionID = @id";
            DynamicParameters paramCheck = new DynamicParameters();
            paramCheck.Add("@id",id);
            Prescription pres = conn.Query<Prescription>(sqlCheck,paramCheck).FirstOrDefault();
            if(pres == null){
                throw new PrescriptionException("Prescription not found");
            }

            string sql = @"delete from Prescription where PrescriptionID = @id";
            string sqlDeletePresMed = @"delete from PrescriptionMedication where PrescriptionID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",id);
            int result = conn.Execute(sql,param);
            int resultDeletePresMed = conn.Execute(sqlDeletePresMed,param);
            if(result == 0){
                throw new PrescriptionException("Prescription not found");
            }
            if(resultDeletePresMed == 0){
                throw new PrescriptionException("an error occured while deleting the prescription");
            }
        }
    }

    public List<Prescription> GetPrescriptionByMedBox(int id)
    {
        List<Prescription> pres = new List<Prescription>();
        using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            var box = _box.GetSmartBox(id);   
            if(box != null && box.PatientID == null){
                throw new PrescriptionException("SmartBox not assigned to a patient");
            }
            if(box == null){
                throw new PrescriptionException("SmartBox not found");
            }
            if(box.PatientID == null){
                throw new PrescriptionException("SmartBox not assigned to a patient");
            }
            int pat = box.PatientID.PatientID;

            string sql ="select PrescriptionID, PatientID From Prescription WHERE PatientID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",pat);
            List<dynamic> res = conn.Query(sql,param).ToList();
            foreach(dynamic r in res){
                int prescriptionId = r.PrescriptionID;
                Patient patient = _accRepo.GetPatient(r.PatientID);
                var p = GetPrescriptionMedications(prescriptionId);
                Prescription prestemp = new Prescription(){
                    PrescriptionID = prescriptionId,
                    Patient = patient,
                    MedicationDetails = p
                };
                pres.Add(prestemp);
            }

            return pres;
        }
    }
    public List<Prescription> GetPatientPrescriptions(int id)
    {
        List<Prescription> pres = new List<Prescription>();
        using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sqlPatient = @"select * from Patient where PatientID = @id";
            DynamicParameters paramPatient = new DynamicParameters();
            paramPatient.Add("@id",id);
            Patient pat = conn.Query<Patient>(sqlPatient,paramPatient).FirstOrDefault();
            if(pat == null){
                throw new PrescriptionException("Patient not found");
            }
            string sql ="select PrescriptionID, PatientID From Prescription WHERE PatientID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",id);
            List<dynamic> res = conn.Query(sql,param).ToList();
            foreach(dynamic r in res){
                int prescriptionId = r.PrescriptionID;
                Patient patient = _accRepo.GetPatient(r.PatientID);
                var p = GetPrescriptionMedications(prescriptionId);
                Prescription prestemp = new Prescription(){
                    PrescriptionID = prescriptionId,
                    Patient = patient,
                    MedicationDetails = p
                };
                pres.Add(prestemp);
            }

            return pres;
        }
    }

    public Prescription? GetPrescription(int id)
    {
        Prescription pres;
        using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select Prescription.PrescriptionID, Prescription.PatientID, Patient.Name, Age, PhoneNumber, Patient.AccountID, Account.Email, Account.Password, Account.IsValid, TakePerWk, SplitMed, PrescriptionMedication.Dosage, StartDate, EndDate, TakeAfterBreakfast, TakeAfterLunch, TakeAfterDinner, TakeBeforeBed, TakeWhenNeeded,Medication.PermitID ,PrescriptionMedication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Medication.Size, Medication.Imprint, Medication.Shape, Medication.Color, Medication.Dosage, Medication.Frequency, Medication.AddedBy, accAddedBy.Email, accAddedBy.Password, accAddedBy.IsValid from Prescription
            Left Join Patient on Prescription.PatientID = Patient.PatientID
            Left Join Account on Patient.AccountID = Account.AccountID
            Left Join PrescriptionMedication on Prescription.PrescriptionID = PrescriptionMedication.PrescriptionID
            Left Join Medication on PrescriptionMedication.MedicationID = Medication.MedicationID
            Left Join Account as accAddedBy on Medication.AddedBy = accAddedBy.AccountID
            where Prescription.PrescriptionID = @id";

            DynamicParameters param = new DynamicParameters();
            param.Add("@id",id);
            pres = conn.Query<Prescription,Patient,Account,Medication,Account,Prescription>(sql,(pres,pat,acc,med,accAddedBy)=>{
                pres.Patient = pat;
                pat.Account = acc;
                med.AddedBy = accAddedBy;
                acc.Password = null;
                accAddedBy.Password = null;
                accAddedBy.Email = null; 
                return pres;
            },param,splitOn:"Prescription.PatientID,Patient.AccountID,PrescriptionMedication.MedicationID").FirstOrDefault();
        }
        return pres;
    }

    public List<Prescription> GetPrescriptions()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            
            string sql = @"select Prescription.PrescriptionID, Prescription.PatientID, Patient.Name, Age, PhoneNumber, Patient.AccountID, Account.Email, Account.Password, Account.IsValid, TakePerWk, SplitMed, PrescriptionMedication.Dosage, StartDate, EndDate, TakeAfterBreakfast, TakeAfterLunch, TakeAfterDinner, TakeBeforeBed, TakeWhenNeeded,Medication.PermitID ,PrescriptionMedication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Medication.Size, Medication.Imprint, Medication.Shape, Medication.Color, Medication.Dosage, Medication.Frequency, Medication.AddedBy, accAddedBy.Email, accAddedBy.Password, accAddedBy.IsValid from Prescription
            Left Join Patient on Prescription.PatientID = Patient.PatientID
            Left Join Account on Patient.AccountID = Account.AccountID
            Left Join PrescriptionMedication on Prescription.PrescriptionID = PrescriptionMedication.PrescriptionID
            Left Join Medication on PrescriptionMedication.MedicationID = Medication.MedicationID
            Left Join Account as accAddedBy on Medication.AddedBy = accAddedBy.AccountID";

            List<Prescription> preses = conn.Query<Prescription,Patient,Account,Medication,Account,Prescription>(sql,(pres,pat,acc,med,accAddedBy)=>{
                pres.Patient = pat;
                pat.Account = acc;
                med.AddedBy = acc;

                pat.Account.Password = null;
                accAddedBy.Email = null;
                return pres;
            },splitOn:"PatientID,AccountID,PermitID,AddedBy").ToList();

            return preses;
        }
    }


    public Prescription UpdatePrescription(Prescription pres)
    {
       using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            //check if the prescription exists
            var p = GetPrescription(pres.PrescriptionID);
            if(p == null){
                throw new PrescriptionException("Prescription not found");
            }
            //check if the patient exists
            string sqlPatient = @"select * from Patient where PatientID = @id";
            DynamicParameters paramPatient = new DynamicParameters();
            paramPatient.Add("@id",pres.Patient.PatientID);
            Patient pat = conn.Query<Patient>(sqlPatient,paramPatient).FirstOrDefault();
            if(pat == null){
                throw new PrescriptionException("Patient not found");
            }
            //check if the medications exist
            foreach(PrescriptionMedication medDetails in pres.MedicationDetails){
                _medRepo.GetMedicine(medDetails.Medication.MedicationId);
            }
            string sql = @"update Prescription set  PatientID = @pat where PrescriptionID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@pat",pres.Patient.PatientID);
            param.Add("@id",pres.PrescriptionID);
            int result = conn.Execute(sql,param);
            if(result == 0){
                throw new PrescriptionException("an error occured while updating the prescription");
            }
            string sqlDelete = @"delete from PrescriptionMedication where PrescriptionID = @id";
            DynamicParameters paramDelete = new DynamicParameters();
            paramDelete.Add("@id",pres.PrescriptionID);
            int resultDelete = conn.Execute(sqlDelete,paramDelete);
            if(resultDelete == 0){
                throw new PrescriptionException("an error occured while updating the prescription");

            }
            foreach(PrescriptionMedication medDetail in pres.MedicationDetails){
                string sqlPresMed = @"insert into PrescriptionMedication (PrescriptionID,MedicationID, TakePerWk, SplitMed, Dosage, StartDate, EndDate, TakeAfterBreakfast, TakeAfterLunch, TakeAfterDinner, TakeBeforeBed, TakeWhenNeeded) values(@pres,@med, @takeWk, @split, @dosage, @start, @end, @takeAfterBreakfast, @takeAfterLunch, @takeAfterDinner, @takeBeforeBed, @takeWhenNeeded)";
                DynamicParameters paramPresMed = new DynamicParameters();
                paramPresMed.Add("@pres",pres.PrescriptionID);
                paramPresMed.Add("@med",medDetail.Medication.MedicationId);
                paramPresMed.Add("@takeWk",medDetail.TakePerWk);
                paramPresMed.Add("@takeAfterBreakfast",medDetail.TakeAfterBreakfast);
                paramPresMed.Add("@takeAfterLunch",medDetail.TakeAfterLunch);
                paramPresMed.Add("@takeAfterDinner",medDetail.TakeAfterDinner);
                paramPresMed.Add("@takeBeforeBed",medDetail.TakeBeforeBed);
                paramPresMed.Add("@takeWhenNeeded",medDetail.TakeWhenNeeded);
                paramPresMed.Add("@split",medDetail.SplitMed);
                paramPresMed.Add("@dosage",medDetail.Dosage);
                paramPresMed.Add("@start",medDetail.StartDate);
                paramPresMed.Add("@end",medDetail.EndDate);

                int resultPresMed = conn.Execute(sqlPresMed,paramPresMed);
                if(resultPresMed == 0){
                    throw new PrescriptionException("an error occured while updating the prescription");
                }
            }
            return GetPrescription(pres.PrescriptionID);
       }
    }
    public List<PrescriptionMedication> GetPrescriptionMedications(int prescriptionId){
        using(SqlConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select PrescriptionMedication.PrescriptionID, TakePerWk, SplitMed, PrescriptionMedication.Dosage, StartDate, EndDate, TakeAfterBreakfast, TakeAfterLunch, TakeAfterDinner, TakeBeforeBed, TakeWhenNeeded, Medication.Name, PrescriptionMedication.MedicationID, Medication.Description, Medication.SideEffects, Medication.Size, Medication.Imprint, Medication.Shape, Medication.Color, Medication.Dosage, Medication.Frequency, Account.AccountID, Account.Email, '1234LOL' AS Password, Account.isValid from PrescriptionMedication
            Left Join Medication on PrescriptionMedication.MedicationID = Medication.MedicationID
            Left Join Account on Medication.AddedBy = Account.AccountID
            where PrescriptionID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",prescriptionId);
            List<PrescriptionMedication> meds = conn.Query<PrescriptionMedication,Medication,Account,PrescriptionMedication>(sql,(med,medication,acc)=>{
                med.Medication = medication;
                medication.AddedBy = acc;
                return med;
            },param,splitOn:"PrescriptionID,MedicationID, AccountID").ToList();
            return meds;
        }
    }


}
public class PrescriptionException : Exception
{
    public PrescriptionException(string message) : base(message)
    {
    }
}