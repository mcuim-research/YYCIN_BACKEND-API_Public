using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Identity;
using test3.Models;

public interface ISmartBoxRepo{
    void AddSmartBox(SmartBox smartBox);
    void UpdateSmartBox(SmartBox smartBox);
    void DeleteSmartBox(int id);
    SmartBox GetSmartBox(int id);
    List<SmartBox> GetAllSmartBoxes();
    SmartBox GetSmartBoxByPatient(int patientID);
}
public class SmartBoxRepo : ISmartBoxRepo
{
    public string _connectionString;
    public SmartBoxRepo(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DevConnection");
    }
    public void checkConnOpen(DbConnection conn)
    {
        if(conn.State == ConnectionState.Closed)
        {
            conn.Open();
        }
    }
    public void AddSmartBox(SmartBox smartBox)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);

            string sql = @"SET IDENTITY_INSERT SmartBox ON
            INSERT INTO SmartBox (SmartBoxID, Location, BindingID) VALUES (@SmartBoxID, @Location, @BindingID)
            SET IDENTITY_INSERT SmartBox OFF
            ";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Location", smartBox.Location);
            param.Add("@BindingID", smartBox.BindingID);
            param.Add("@SmartBoxID", smartBox.SmartBoxID);
        
            conn.Query(sql,param);
        }
        
    }

    public void DeleteSmartBox(int id)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            string sql = @"DELETE FROM SmartBox WHERE SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", id);
            conn.Query(sql,param);

        }
    }

    public List<SmartBox> GetAllSmartBoxes()
    {
        List<SmartBox> res = new List<SmartBox>();
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            string sql = @"SELECT SmartBoxID, Location, BindingID,Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.AccountID, Account.Email, Account.Password, Account.IsValid FROM SmartBox 
            LEFT JOIN Patient ON SmartBox.PatientID = Patient.PatientID
            LEFT JOIN Account ON Patient.AccountID = Account.AccountID";
            res = conn.Query<SmartBox, Patient, Account, SmartBox>(sql,(smartbox, patient, account)=>{
                smartbox.PatientID = patient;
                try{
                    patient.Account = account;
                    patient.Account.Password = null;
                }catch(Exception e){
                    Console.WriteLine(e.Message);
                }
                return smartbox;
            }
            ,splitOn:"PatientID, AccountID").ToList();
        }
        return res;
    }
    public SmartBox GetSmartBoxByPatient(int patientID)
    {
        SmartBox res = new SmartBox();
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            string sql = @"SELECT SmartBoxID, Location, BindingID,Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.AccountID, Account.Email, Account.Password, Account.IsValid FROM SmartBox 
            LEFT JOIN Patient ON SmartBox.PatientID = Patient.PatientID
            LEFT JOIN Account ON Patient.AccountID = Account.AccountID
            WHERE Patient.PatientID = @PatientID";
            res = conn.Query<SmartBox, Patient, Account, SmartBox>(sql,(smartbox, patient, account)=>{
                try{
                    patient.Account = account;
                    patient.Account.Password = null;
                }catch(Exception e){
                    Console.WriteLine(e.Message);
                }
                smartbox.PatientID = patient;
                return smartbox;
            }, new {PatientID = patientID}, splitOn:"PatientID, AccountID"
            ).FirstOrDefault();
        }
        return res;
    }
    public SmartBox GetSmartBox(int id)
    {
        SmartBox res = new SmartBox();
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);
            string sql = @"SELECT SmartBoxID, Location, BindingID,Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.AccountID, Account.Email, Account.Password, Account.IsValid FROM SmartBox 
            LEFT JOIN Patient ON SmartBox.PatientID = Patient.PatientID
            LEFT JOIN Account ON Patient.AccountID = Account.AccountID
            WHERE SmartBoxID = @SmartBoxID";
            res = conn.Query<SmartBox, Patient, Account, SmartBox>(sql,(smartbox, patient, account)=>{
                try{
                    patient.Account = account;
                    patient.Account.Password = null;
                }catch(Exception e){
                    Console.WriteLine(e.Message);
                }
                smartbox.PatientID = patient;
                return smartbox;
            }, new {SmartBoxID = id}, splitOn:"PatientID, AccountID"
            ).FirstOrDefault();
        }
        return res;
    }

    public void UpdateSmartBox(SmartBox smartBox)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnOpen(conn);

            string sql = @"UPDATE SmartBox SET Location = @Location, PatientID = @PatientID, BindingID='' WHERE SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Location", smartBox.Location);
            try{
                param.Add("@PatientID", smartBox.PatientID.PatientID);
            }catch{
                param.Add("PatientID",null);
            }
            param.Add("@SmartBoxID", smartBox.SmartBoxID);
            conn.Query(sql,param );
        }
        
    }

}