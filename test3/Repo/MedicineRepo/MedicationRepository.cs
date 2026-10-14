using System.Data;
using System.Data.SqlClient;
using Dapper;
using test3.Models;

public interface IMedicationRepository
{
    Medication AddMedicine(Medication med);
    void DeleteMedicine(int id);
    List<Medication> GetMedicines();
    Medication GetMedicine(int id);
    Medication GetMedicine(string name);
    Medication UpdateMedicine(Medication med);
    Medication SearchMedicineWithColorAndShape(string color, string shape);
}
public class MedicationRepository : IMedicationRepository
{
    string _connectionString;
    public MedicationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DevConnection");
    }

    public Medication AddMedicine(Medication med)
    {
        try{
            using (IDbConnection conn = new SqlConnection(_connectionString)){
                checkConnOpen(conn);

                // Test if the medicine with the same name already exists
                bool exists = false;
                try{
                    GetMedicine(med.Name);
                    exists = true;
                }catch(MedicationException){
                    exists = false;
                }
                if(exists){
                    throw new MedicationException("Medicine already exists");
                }
                string sql = @"insert into Medication (PermitID,Name,Description,SideEffects,Size,Imprint,Shape,Color,Dosage,Frequency,AddedBy)VALUES(@permit,@name,@desc,@side,@size,@imprint,@shape,@color,@dosage,@freq,@addedBy)";
                DynamicParameters param = new DynamicParameters();
                param.Add("@permit",med.PermitID);
                param.Add("@name",med.Name);
                param.Add("@desc",med.Description);
                param.Add("@side",med.SideEffects);
                param.Add("@size",med.Size);
                param.Add("@imprint",med.Imprint);
                param.Add("@shape",med.Shape);
                param.Add("@color",med.Color);
                param.Add("@dosage",med.Dosage);
                param.Add("@freq",med.Frequency);
                param.Add("@addedBy",med.AddedBy.AccountID);
                Console.WriteLine(param.Get<string>("@permit"));
                Console.WriteLine(param.Get<string>("@name"));
                Console.WriteLine(param.Get<string>("@desc"));
                Console.WriteLine(param.Get<string>("@side"));
                Console.WriteLine(param.Get<string>("@size"));
                Console.WriteLine(param.Get<string>("@imprint"));
                Console.WriteLine(param.Get<string>("@shape"));
                Console.WriteLine(param.Get<string>("@color"));
                Console.WriteLine(param.Get<string>("@dosage"));
                Console.WriteLine(param.Get<string>("@freq"));
                Console.WriteLine(param.Get<int>("@addedBy"));
                conn.Query(sql,param);
                //Print the input params

                // Get the medicine that was just added
                string sqlGet = @"select * from Medication where Name = N'@name'";
                DynamicParameters paramGet = new DynamicParameters();
                paramGet.Add("@name",med.Name);
                Medication m = GetMedicine(med.Name);
                

                if(m != null){
                    return m;
                }else{
                    throw new MedicationException("an error occured while adding the medicine");
                }


            };
        }catch(Exception e){
            Console.WriteLine(e.Message);
            throw new MedicationException(e.Message);
            
        }
    }
    public void checkConnOpen(IDbConnection conn){
    if(conn.State == ConnectionState.Closed){
        conn.Open();
    }
}

    public void DeleteMedicine(int id)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            var med = GetMedicine(id);
            if(med.Prescriptions.Count() > 0){
                throw new MedicationException("Medicine is in use");
            }

            checkConnOpen(conn);
            string sql = @"delete from Medication where MedicineID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",id);
            int result = conn.Execute(sql,param);
            if(result == 0){
                throw new MedicationException("Medicine not found");
            }

        }
    }

    public Medication GetMedicine(int id)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select PermitID, Medication.MedicationID, Name, Description, SideEffects,Size,Imprint,Shape,Color,Medication.Dosage,Frequency,AccountID,Email,'1234lol',isValid, PrescriptionMedication.PrescriptionID,PatientID, PrescriptionMedication.Dosage,Frequency, StartDate,EndDate from Medication LEFT JOIN Account on Medication.AddedBy = Account.AccountID LEFT JOIN PrescriptionMedication on Medication.MedicationID = PrescriptionMedication.MedicationID FULL OUTER JOIN Prescription on PrescriptionMedication.PrescriptionID = Prescription.PrescriptionID
            WHERE Medication.MedicationID = @id";
            DynamicParameters param = new DynamicParameters();
            param.Add("@id",id);
            Medication med = conn.Query<Medication,Account,Medication>(sql,(med,acc)=>{
                med.AddedBy = acc;
                acc.Password = null;
                acc.Email = null;
                acc.Caregiver = null;
                acc.Doctor = null;
                acc.Patient = null;
                return med;
            },param,splitOn:"AccountID").FirstOrDefault();
            if(med == null){
                throw new MedicationException("Medicine not found");
            }
            return med;
        }
    }
    public Medication GetMedicine(string name)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select PermitID, Medication.MedicationID, Name, Description, SideEffects,Size,Imprint,Shape,Color,Medication.Dosage,Frequency,AccountID,Email,'1234lol',isValid, PrescriptionMedication.PrescriptionID,PatientID, PrescriptionMedication.Dosage,Frequency, StartDate,EndDate from Medication LEFT JOIN Account on Medication.AddedBy = Account.AccountID LEFT JOIN PrescriptionMedication on Medication.MedicationID = PrescriptionMedication.MedicationID FULL OUTER JOIN Prescription on PrescriptionMedication.PrescriptionID = Prescription.PrescriptionID
            WHERE Medication.Name = @name";
            DynamicParameters param = new DynamicParameters();
            param.Add("@name",name);
            Medication med = conn.Query<Medication,Account,Medication>(sql,(med,acc)=>{
                med.AddedBy = acc;
                acc.Password = null;
                acc.Email = null;
                acc.Caregiver = null;
                acc.Doctor = null;
                acc.Patient = null;
                return med;
            },param,splitOn:"AccountID").FirstOrDefault();
            if(med == null){
                throw new MedicationException("Medicine not found");
            }
            return med;
        }
    }

    public List<Medication> GetMedicines()
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select PermitID, Medication.MedicationID, Name, Description, SideEffects,Size,Imprint,Shape,Color,Medication.Dosage,Frequency,AccountID,Email,'1234lol',isValid, PrescriptionMedication.PrescriptionID,PatientID, PrescriptionMedication.Dosage,Frequency, StartDate,EndDate from Medication LEFT JOIN Account on Medication.AddedBy = Account.AccountID LEFT JOIN PrescriptionMedication on Medication.MedicationID = PrescriptionMedication.MedicationID FULL OUTER JOIN Prescription on PrescriptionMedication.PrescriptionID = Prescription.PrescriptionID";
            List<Medication> meds = conn.Query<Medication,Account,test3.Models.Prescription,Medication>(sql,(med,acc,pres)=>{
                med.AddedBy = acc;
                if(acc != null){
                    acc.Password = null;
                    acc.Email = null;
                    acc.Caregiver = null;
                    acc.Doctor = null;
                    acc.Patient = null;
                    acc.Password = null;
                }
                med.Prescriptions = new List<test3.Models.Prescription>
                {
                    pres
                };
                
                return med;
            },splitOn:"AccountID, PrescriptionID").ToList();
            return meds;
        }
    }

    public Medication SearchMedicineWithColorAndShape(string color, string shape)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"select PermitID, Medication.MedicationID, Name, Description, SideEffects,Size,Imprint,Shape,Color,Medication.Dosage,Frequency,AccountID,Email,'1234lol',isValid, PrescriptionMedication.PrescriptionID,PatientID, PrescriptionMedication.Dosage,Frequency, StartDate,EndDate from Medication LEFT JOIN Account on Medication.AddedBy = Account.AccountID LEFT JOIN PrescriptionMedication on Medication.MedicationID = PrescriptionMedication.MedicationID FULL OUTER JOIN Prescription on PrescriptionMedication.PrescriptionID = Prescription.PrescriptionID
            WHERE Medication.Color = @color AND Medication.Shape = @shape";
            DynamicParameters param = new DynamicParameters();
            param.Add("@color",color);
            param.Add("@shape",shape);
            Medication med = conn.Query<Medication,Account,test3.Models.Prescription,Medication>(sql,(med,acc,pres)=>{
                med.AddedBy = acc;
                if(acc != null){
                    acc.Password = null;
                    acc.Email = null;
                    acc.Caregiver = null;
                    acc.Doctor = null;
                    acc.Patient = null;
                    acc.Password = null;
                }
                med.Prescriptions = new List<test3.Models.Prescription>
                {
                    pres
                };
                
                return med;
            },param,splitOn:"AccountID, PrescriptionID").FirstOrDefault();
            if(med == null){
                throw new MedicationException("Medicine not found");
            }
            return med;
        }
    }

    public Medication UpdateMedicine(Medication med)
    {
        try{
            using(IDbConnection conn = new SqlConnection(_connectionString)){
                checkConnOpen(conn);
                string sql = @"update Medication set PermitID = @permit,Name = @name,Description = @desc,SideEffects = @side,Size =@size,Imprint =@imprint,Shape = @shape,Color = @color,Dosage = @dosage,Frequency = @freq where MedicineID = @id";
                DynamicParameters param = new DynamicParameters();
                param.Add("@permit",med.PermitID);
                param.Add("@name",med.Name);
                param.Add("@desc",med.Description);
                param.Add("@side",med.SideEffects);
                param.Add("@size",med.Size);
                param.Add("@imprint",med.Imprint);
                param.Add("@shape",med.Shape);
                param.Add("@color",med.Color);
                param.Add("@dosage",med.Dosage);
                param.Add("@freq",med.Frequency);
                param.Add("@id",med.MedicationId);
                int result = conn.Execute(sql,param);
                if(result == 0){
                    throw new MedicationException("Medicine not found");
                }
                return GetMedicine(med.MedicationId);
            }
        }catch(Exception){
            throw new MedicationException("an error occured while updating the medicine");
        }
    }
}
public class MedicationException: System.SystemException{
    public MedicationException():base("Account not found"){

    }
    public MedicationException(string message, Exception innerException) : base(message, innerException) { }
    public MedicationException(string message):base(message){}

}