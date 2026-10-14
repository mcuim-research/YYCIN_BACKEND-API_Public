using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using Dapper;
using test3.Models;

public interface IUsageLogRepo{
    void AddUsageLog(UsageLog usageLog);
    void UpdateUsageLog(UsageLog usageLog);
    void DeleteUsageLog(int id);
    UsageLog GetUsageLog(int id);
    IEnumerable<UsageLog> GetAllUsageLogs();
    List<UsageLog> GetAllUsageLogsBySmartBox(int smartBoxID);
    List<UsageLog> GetLatest50UsageLogsBySmartBox(int smartBoxID);
    List<UsageLog> GetUsageLogsByDateAndSmartBox(int smartBoxID, System.DateTime date);
    
}

public class UsageLogRepo : IUsageLogRepo
{
    string _connectionString;
    public UsageLogRepo(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DevConnection");
    }
    public void checkConnOpen(DbConnection conn){
        if(conn.State == ConnectionState.Closed){
            conn.Open();
        }
    }
    public void AddUsageLog(UsageLog usageLog)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            using (SqlConnection sqlConn = new SqlConnection(_connectionString)){
                string sql  = @"INSERT INTO UsageLog (AccountID, MedicationID, DateTime, Action, Status, Notes, SmartBoxMedicationID) VALUES (@AccountID, @MedicationID, @DateTime, @Action, @Status, @Notes, @SmartBoxMedicationID)";
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountID", usageLog.Account.AccountID);
                param.Add("@MedicationID", usageLog.Medication.MedicationId);
                param.Add("@DateTime", DateTime.Now);
                param.Add("@Action", usageLog.Action);
                param.Add("@Status", usageLog.Status);
                param.Add("@Notes", usageLog.Notes);
                param.Add("@SmartBoxMedicationID", usageLog.SmartBoxMedication.SmartBoxMedicationID);


                conn.Execute(sql, param);
            }
        }
    }

    public void DeleteUsageLog(int id)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            using (DbCommand cmd = conn.CreateCommand()){
                cmd.CommandText = @"DELETE FROM UsageLog WHERE LogID = @LogID";
                DynamicParameters param = new DynamicParameters();
                param.Add("@LogID", id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public IEnumerable<UsageLog> GetAllUsageLogs()
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
           String query = @"Select LogID, DateTime, Action, Status, Notes, PermitID, Account.AccountID, Account.Email, Account.Password, IsValid ,Medication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Size, Imprint, Shape, Color, Dosage, Frequency, Pos, SmartBoxMedication.SmartBoxMedicationID, SmartBoxID, Split, Quantity from UsageLog
            left join Account on UsageLog.AccountID = Account.AccountID
            left join Medication on UsageLog.MedicationID = Medication.MedicationID
            left join SmartBoxMedication on UsageLog.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID";
            return conn.Query<UsageLog, Account, Medication, SmartBoxMedication, UsageLog>(query, (usageLog, account, medication, smartBoxMedication) => {
                usageLog.Account = account;
                usageLog.Medication = medication;
                usageLog.SmartBoxMedication = smartBoxMedication;
                return usageLog;
            }, splitOn: "AccountID, MedicationID, SmartBoxMedicationID");
            
        }
    }

    public UsageLog GetUsageLog(int id)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            String query = @"Select LogID, DateTime, Action, Status, Notes, PermitID, Account.AccountID, Account.Email, Account.Password, IsValid ,Medication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Size, Imprint, Shape, Color, Dosage, Frequency, Pos, SmartBoxMedication.SmartBoxMedicationID, SmartBoxID, Split, Quantity from UsageLog
            left join Account on UsageLog.AccountID = Account.AccountID
            left join Medication on UsageLog.MedicationID = Medication.MedicationID
            left join SmartBoxMedication on UsageLog.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            where LogID = @LogID";
            return conn.Query<UsageLog, Account, Medication, SmartBoxMedication, UsageLog>(query, (usageLog, account, medication, smartBoxMedication) => {
                usageLog.Account = account;
                usageLog.Medication = medication;
                usageLog.SmartBoxMedication = smartBoxMedication;
                return usageLog;
            }, new {LogID = id}, splitOn: "AccountID, MedicationID, SmartBoxMedicationID").FirstOrDefault();
        }
    }

    public void UpdateUsageLog(UsageLog usageLog)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            using (DbCommand cmd = conn.CreateCommand()){
                cmd.CommandText = @"UPDATE UsageLog SET AccountID = @AccountID, MedicationID = @MedicationID, DateTime = @DateTime, Action = @Action, Status = @Status, Notes = @Notes, SmartBoxMedicationID = @SmartBoxMedicationID WHERE LogID = @LogID";
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountID", usageLog.Account.AccountID);
                param.Add("@MedicationID", usageLog.Medication.MedicationId);
                param.Add("@DateTime", DateTime.Now);
                param.Add("@Action", usageLog.Action);
                param.Add("@Status", usageLog.Status);
                param.Add("@Notes", usageLog.Notes);
                param.Add("@SmartBoxMedicationID", usageLog.SmartBoxMedication.SmartBoxMedicationID);
                param.Add("@LogID", usageLog.LogID);

                cmd.ExecuteNonQuery();
            }
        }
    }
    //UNTESTED
    public List<UsageLog> GetAllUsageLogsBySmartBox(int smartBoxID)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            String query = @"Select LogID, DateTime, Action, Status, Notes, PermitID, Account.AccountID, Account.Email, Account.Password, IsValid ,Medication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Size, Imprint, Shape, Color, Dosage, Frequency, Pos, SmartBoxMedication.SmartBoxMedicationID, SmartBoxID, Split, Quantity from UsageLog
            left join Account on UsageLog.AccountID = Account.AccountID
            left join Medication on UsageLog.MedicationID = Medication.MedicationID
            left join SmartBoxMedication on UsageLog.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            where SmartBoxID = @SmartBoxID";
            return conn.Query<UsageLog, Account, Medication, SmartBoxMedication, UsageLog>(query, (usageLog, account, medication, smartBoxMedication) => {
                usageLog.Account = account;
                usageLog.Medication = medication;
                usageLog.SmartBoxMedication = smartBoxMedication;
                return usageLog;
            }, new {SmartBoxID = smartBoxID}, splitOn: "AccountID, MedicationID, SmartBoxMedicationID").ToList();
        }
    }
//UNTESTED
    public List<UsageLog> GetLatest50UsageLogsBySmartBox(int smartBoxID)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            String query = @"Select top 50 LogID, DateTime, Action, Status, Notes, PermitID, Account.AccountID, Account.Email, Account.Password, IsValid ,Medication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Size, Imprint, Shape, Color, Dosage, Frequency, Pos, SmartBoxMedication.SmartBoxMedicationID, SmartBoxID, Split, Quantity from UsageLog
            left join Account on UsageLog.AccountID = Account.AccountID
            left join Medication on UsageLog.MedicationID = Medication.MedicationID
            left join SmartBoxMedication on UsageLog.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            where SmartBoxID = @SmartBoxID order by DateTime desc";
            return conn.Query<UsageLog, Account, Medication, SmartBoxMedication, UsageLog>(query, (usageLog, account, medication, smartBoxMedication) => {
                usageLog.Account = account;
                usageLog.Medication = medication;
                usageLog.SmartBoxMedication = smartBoxMedication;
                return usageLog;
            }, new {SmartBoxID = smartBoxID}, splitOn: "AccountID, MedicationID, SmartBoxMedicationID").ToList();
        }
    }
//UNTESTED
    public List<UsageLog> GetUsageLogsByDateAndSmartBox(int smartBoxID, DateTime date)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            String query = @"Select LogID, DateTime, Action, Status, Notes, PermitID, Account.AccountID, Account.Email, Account.Password, IsValid ,Medication.MedicationID, Medication.Name, Medication.Description, Medication.SideEffects, Size, Imprint, Shape, Color, Dosage, Frequency, Pos, SmartBoxMedication.SmartBoxMedicationID, SmartBoxID, Split, Quantity from UsageLog
            left join Account on UsageLog.AccountID = Account.AccountID
            left join Medication on UsageLog.MedicationID = Medication.MedicationID
            left join SmartBoxMedication on UsageLog.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            where SmartBoxID = @SmartBoxID and CONVERT(DATE, DateTime)=CONVERT(DATE, @Date)";
            return conn.Query<UsageLog, Account, Medication, SmartBoxMedication, UsageLog>(query, (usageLog, account, medication, smartBoxMedication) => {
                usageLog.Account = account;
                usageLog.Medication = medication;
                usageLog.SmartBoxMedication = smartBoxMedication;
                return usageLog;
            }, new {SmartBoxID = smartBoxID, Date = date}, splitOn: "AccountID, MedicationID, pos").ToList();
        }
    }
}