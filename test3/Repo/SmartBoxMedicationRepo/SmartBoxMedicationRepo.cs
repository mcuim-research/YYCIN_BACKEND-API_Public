using System.Data;
using System.Data.SqlClient;
using Dapper;
using test3.Models;

public interface ISmartBoxMedicationRepo{
    public SmartBoxMedication GetSmartBoxMedication(int id);
    public List<SmartBoxMedication> GetAllSmartBoxMedications();
    public void AddSmartBoxMedication(SmartBoxMedication smartBoxMedication);
    public void UpdateSmartBoxMedication(SmartBoxMedication smartBoxMedication);
    public List<SmartBoxMedication> GetAllSmartBoxMedications(int smartBoxID);
    public void DeleteSmartBoxMedication(int id);
    public void DeleteSmartBoxMedicationsUsingSmartBoxID(int smartBoxID);

}
public class SmartBoxMedicationRepo : ISmartBoxMedicationRepo
{
    private string _connectionString;
    private IMedicationRepository _medicationRepository;
    
    public void checkConnOpen(IDbConnection conn){
        if(conn.State == ConnectionState.Closed){
            conn.Open();
        }
    }
    public SmartBoxMedicationRepo(IConfiguration configuration, IMedicationRepository medicationRepository )
    {
        _connectionString = configuration.GetConnectionString("DevConnection");
        _medicationRepository = medicationRepository;
        
    }
    public void AddSmartBoxMedication(SmartBoxMedication smartBoxMedication)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"INSERT INTO SmartBoxMedication (Pos, SmartBoxID) VALUES (@Pos, @SmartBoxID)";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Pos", smartBoxMedication.Pos);
            param.Add("@SmartBoxID", smartBoxMedication.SmartBoxID);
            conn.Query(sql,param);

        }
    }

    public void DeleteSmartBoxMedication(int id)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"DELETE FROM SmartBoxMedication WHERE SmartBoxMedicationID = @SmartBoxMedicationID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxMedicationID", id);
            conn.Query(sql,param);
        }
    }

    public List<SmartBoxMedication> GetAllSmartBoxMedications(){
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"SELECT * FROM SmartBoxMedication ";
            var res1 = conn.Query<SmartBoxMedicationDB>(sql).ToList();

            List<SmartBoxMedication> res = new List<SmartBoxMedication>();
            foreach (var item in res1)
            {
                    SmartBoxMedication temp = new SmartBoxMedication();
                    temp.Pos = item.Pos;
                    temp.SmartBoxMedicationID = item.SmartBoxMedicationID;
                    temp.SmartBoxID = item.SmartBoxID;
                    temp.Split = item.Split==null?0:item.Split.Value;
                    temp.Quantity = item.Quantity==null?0:item.Quantity.Value;
                    if(item.MedicationID != null)
                    temp.Medication = _medicationRepository.GetMedicine(item.MedicationID.Value);
                    else
                    temp.Medication = null;
                    res.Add(temp);
            }
            return res;
        }
    }
    public List<SmartBoxMedication> GetAllSmartBoxMedications(int smartBoxID){
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"SELECT * FROM SmartBoxMedication where SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", smartBoxID);
            var res1 = conn.Query<SmartBoxMedicationDB>(sql, param).ToList();

            List<SmartBoxMedication> res = new List<SmartBoxMedication>();
            foreach (var item in res1)
            {
                    SmartBoxMedication temp = new SmartBoxMedication();
                    temp.Pos = item.Pos;
                    temp.SmartBoxMedicationID = item.SmartBoxMedicationID;
                    temp.SmartBoxID = item.SmartBoxID;
                    temp.Split = item.Split==null?0:item.Split.Value;
                    temp.Quantity = item.Quantity==null?0:item.Quantity.Value;
                    if(item.MedicationID != null)
                    temp.Medication = _medicationRepository.GetMedicine(item.MedicationID.Value);
                    else
                    temp.Medication = null;
                    res.Add(temp);
            }
            return res;
        }
    }

    public SmartBoxMedication GetSmartBoxMedication(int id)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"SELECT * FROM SmartBoxMedication WHERE SmartBoxMedicationID = @SmartBoxMedicationID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxMedicationID", id);
            var res1 = conn.Query<SmartBoxMedicationDB>(sql,param).FirstOrDefault();
            if(res1 != null){
                SmartBoxMedication res = new SmartBoxMedication();
                res.Pos = res1.Pos;
                res.SmartBoxMedicationID = res1.SmartBoxMedicationID;
                res.SmartBoxID = res1.SmartBoxID;
                res.Split = res1.Split==null?0:res1.Split.Value;
                res.Quantity = res1.Quantity==null?0:res1.Quantity.Value;
                if(res1.MedicationID != null)
                res.Medication = _medicationRepository.GetMedicine(res1.MedicationID.Value);
                else
                res.Medication = null;
                return res;
            }
            return null;
        }
    }

    public void UpdateSmartBoxMedication(SmartBoxMedication smartBoxMedication)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"UPDATE SmartBoxMedication SET Pos = @Pos, MedicationID = @medID, Quantity = @q, Split=@s WHERE SmartBoxMedicationID = @SmartBoxMedicationID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Pos", smartBoxMedication.Pos);
            if(smartBoxMedication.Medication == null){
                param.Add("@medID", null);}
            else
            param.Add("@medID", smartBoxMedication.Medication.MedicationId);
            param.Add("@q", smartBoxMedication.Quantity);
            param.Add("@s", smartBoxMedication.Split);
            
            param.Add("@SmartBoxMedicationID", smartBoxMedication.SmartBoxMedicationID);
            conn.Query(sql,param);
        }
    }
        public void UpdateSmartBoxMedication(int Pos, int SmartBoxID, int MedicationID)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"UPDATE SmartBoxMedication SET Pos = @Pos, MedicationID=@MedicationID  WHERE SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();

            param.Add("@Pos", Pos);
            param.Add("@SmartBoxID", SmartBoxID);
            param.Add("@MedicationID", MedicationID);

            conn.Query(sql,param);
        }
    }
    public void DeleteSmartBoxMedicationsUsingSmartBoxID(int smartBoxID){
        using (IDbConnection conn = new SqlConnection(_connectionString)){
            checkConnOpen(conn);
            string sql = @"DELETE FROM SmartBoxMedication WHERE SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", smartBoxID);
            conn.Query(sql,param);
        }
    }
}