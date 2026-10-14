using System.Data;
using System.Data.SqlClient;
using Dapper;
using test3.Models;

public interface IReminderRepo
{
    Reminder GetReminder(int id);
    List<Reminder> GetAllReminders();
    void AddReminder(Reminder reminder);
    void DeleteReminder(int id);
    void UpdateReminder(Reminder reminder);
    List<Reminder> GetRemindersByPatient(int patientID);
    List<Reminder> GetRemindersBySmartBoxMedication(int smartBoxMedicationID);
    List<Reminder> GetRemindersThisMinute();
    
}
public class ReminderRepo: IReminderRepo{
    private string _connectionString;
    private IPrescriptionRepository _prescription;
    public ReminderRepo(IConfiguration conn, IPrescriptionRepository _prescription){
        this._prescription = _prescription;
        _connectionString = conn.GetConnectionString("DevConnection");
    }

    public void AddReminder(Reminder reminder)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            string sql = "INSERT INTO Reminder (PrescriptionID, Time, Status, SmartBoxMedicationID) VALUES (@PrescriptionID, @Time, @Status, @SmartBoxMedicationID)";
            var param = new DynamicParameters();
            param.Add("@PrescriptionID", reminder.Prescription.PrescriptionID);
            param.Add("@Time", reminder.Time);
            param.Add("@Status", reminder.Status);
            param.Add("@SmartBoxMedicationID", reminder.SmartBoxMedication.SmartBoxMedicationID);
            conn.Query(sql,param);
        }
    }

    public void checkConnectionOpen(IDbConnection conn){
        if(conn.State == ConnectionState.Closed){
            conn.Open();
        }
    }

    public void DeleteReminder(int id)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            string sql = "DELETE FROM Reminder WHERE ReminderID = @id";
            var param = new DynamicParameters();
            param.Add("@id", id);
            conn.Query(sql,param);
        }
    }

    public List<Reminder> GetAllReminders()
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnectionOpen(conn);
            string sql = @"SELECT ReminderID, Time, Status, SmartBoxMedicationID, SmartBoxMedication.SmartBoxID, SmartBoxMedication.Split, SmartBoxMedication.Quantity, PrescriptionID, Prescription.PatientID,null FROM Reminder
            LEFT JOIN Prescription ON Reminder.PrescriptionID = Prescription.PrescriptionID
            LEFT JOIN SmartBoxMedication ON Reminder.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID";
            var res = conn.Query<Reminder, Prescription, SmartBoxMedication,Reminder>(sql,(reminder, prescription, smartboxmedi)=>{
                reminder.Prescription = _prescription.GetPrescription(prescription.PrescriptionID);
                reminder.SmartBoxMedication = smartboxmedi;
                return reminder;
            },splitOn:"PrescriptionID,SmartBoxMedicationID");
            return res.ToList();
        }
    }

    public Reminder GetReminder(int id){
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnectionOpen(conn);
            string sql = @"SELECT ReminderID, Time, Status, SmartBoxMedicationID, SmartBoxMedication.SmartBoxID, SmartBoxMedication.Split, SmartBoxMedication.Quantity, PrescriptionID, Prescription.PatientID,null FROM Reminder
            LEFT JOIN Prescription ON Reminder.PrescriptionID = Prescription.PrescriptionID
            LEFT JOIN SmartBoxMedication ON Reminder.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            WHERE ReminderID = @id";
            var res = conn.Query<Reminder, Prescription, SmartBoxMedication,Reminder>(sql,(reminder, prescription, smartboxmedi)=>{
                reminder.Prescription = _prescription.GetPrescription(prescription.PrescriptionID);
                reminder.SmartBoxMedication = smartboxmedi;
                return reminder;
            },splitOn:"PrescriptionID,SmartBoxMedicationID");
            return res.FirstOrDefault();
        }
    }

    public List<Reminder> GetRemindersByPatient(int patientID)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnectionOpen(conn);
            string sql = @"SELECT ReminderID, Time, Status, SmartBoxMedicationID, SmartBoxMedication.SmartBoxID, SmartBoxMedication.Split, SmartBoxMedication.Quantity, PrescriptionID, Prescription.PatientID,null FROM Reminder
            LEFT JOIN Prescription ON Reminder.PrescriptionID = Prescription.PrescriptionID
            LEFT JOIN SmartBoxMedication ON Reminder.SmartBoxMedicationID = SmartBoxMedication.SmartBoxMedicationID
            WHERE ReminderID.PatientID = @patientID";
            var res = conn.Query<Reminder, Prescription, SmartBoxMedication,Reminder>(sql,(reminder, prescription, smartboxmedi)=>{
                reminder.Prescription = _prescription.GetPrescription(prescription.PrescriptionID);
                reminder.SmartBoxMedication = smartboxmedi;
                return reminder;
            },splitOn:"PrescriptionID,SmartBoxMedicationID");
            return res.ToList();
        }
        
    }

    public List<Reminder> GetRemindersBySmartBoxMedication(int smartBoxMedicationID)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            string sql = "SELECT ReminderID, PrescriptionID, Time, Status, SmartBoxMedicationID FROM Reminder WHERE SmartBoxMedicationID = @smartBoxMedicationID";
            var param = new DynamicParameters();
            param.Add("@smartBoxMedicationID", smartBoxMedicationID);
            var res = conn.Query<Reminder, Prescription, SmartBoxMedication,Reminder>(sql,(reminder, prescription, smartboxmedi)=>{
                reminder.Prescription = _prescription.GetPrescription(prescription.PrescriptionID);
                reminder.SmartBoxMedication = smartboxmedi;
                return reminder;
            },param);
            return res.ToList();
        }
    }

    public void UpdateReminder(Reminder reminder)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            string sql = "UPDATE Reminder SET Time = @Time, Status = @Status WHERE ReminderID = @ReminderID";
            var param = new DynamicParameters();
            param.Add("@Time", reminder.Time);
            param.Add("@Status", reminder.Status);
            param.Add("@ReminderID", reminder.ReminderID);
            conn.Query(sql,param);
        }
    }
    public List<Reminder> GetRemindersThisMinute()
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            string sql = "SELECT ReminderID, PrescriptionID, Time, Status, SmartBoxMedicationID FROM Reminder WHERE DATEPART(HOUR, Time) = DATEPART(HOUR, @Time)";
            var param = new DynamicParameters();
            param.Add("@Time", DateTime.Now);
            var res = conn.Query<Reminder, Prescription, SmartBoxMedication,Reminder>(sql,(reminder, prescription, smartboxmedi)=>{
                reminder.Prescription = _prescription.GetPrescription(prescription.PrescriptionID);
                reminder.SmartBoxMedication = smartboxmedi;
                return reminder;
            },param);
            return res.ToList();
        }
    }
}
