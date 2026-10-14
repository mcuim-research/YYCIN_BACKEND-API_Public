using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Dapper;

public interface IEnvLogRepo
{
    void AddEnvLog(EnvLog envLog);
    void UpdateEnvLog(EnvLog envLog);
    void DeleteEnvLog(int id);
    EnvLog GetEnvLog(int id);
    List<EnvLog> GetAllEnvLogs();
    List<EnvLog> GetEnvLogsBySmartBox(int smartBoxID);
    EnvLog GetLatestEnvLogBySmartBox(int smartBoxID);
    List<EnvLog> GetEnvLogsByDate(System.DateTime date);
    List<EnvLog> GetEnvLogsBySmartBoxAndDate(int smartBoxID, System.DateTime date);

}
public class EnvLogRepo : IEnvLogRepo
{
    string _connectionString;
    public EnvLogRepo(IConfiguration conn){
        _connectionString = conn.GetConnectionString("DevConnection");
    }
    public void CheckConnOpen(DbConnection conn)
    {
        if(conn.State == ConnectionState.Closed)
        {
            conn.Open();
        }
    }
    public void AddEnvLog(EnvLog envLog)
    {
        using (DbConnection conn = new SqlConnection(_connectionString)){
            CheckConnOpen(conn);
            string sql = @"INSERT INTO EnvLog ( Temperature, Humidity, DateTime, SmartBoxID) VALUES ( @Temperature, @Humidity, @DateTime, @SmartBoxID)";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Temperature", envLog.Temperature);
            param.Add("@Humidity", envLog.Humidity);
            param.Add("@DateTime", envLog.DateTime);
            param.Add("@SmartBoxID", envLog.SmartBox.SmartBoxID);
            conn.Query(sql,param);

        }
        
    }

    public void DeleteEnvLog(int id)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"DELETE FROM EnvLog WHERE EnvLogID = @EnvLogID";
                DynamicParameters param = new DynamicParameters();
                param.Add("@EnvLogID", id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public List<EnvLog> GetAllEnvLogs()
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT * FROM EnvLog";
            return conn.Query<EnvLog>(sql).ToList();
        }
    }

    public EnvLog GetEnvLog(int id)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT * FROM EnvLog WHERE EnvLogID = @EnvLogID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@EnvLogID", id);
            return conn.QueryFirstOrDefault<EnvLog>(sql,param);
        }
    }

    public void UpdateEnvLog(EnvLog envLog)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"UPDATE EnvLog SET Temperature = @Temperature, Humidity = @Humidity, DateTime = @DateTime, SmartBoxID = @SmartBoxID WHERE EnvLogID = @EnvLogID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Temperature", envLog.Temperature);
            param.Add("@Humidity", envLog.Humidity);
            param.Add("@DateTime", envLog.DateTime);
            param.Add("@SmartBoxID", envLog.SmartBox.SmartBoxID);
            param.Add("@EnvLogID", envLog.EnvLogID);
            conn.Query(sql,param);
        }
    }
    public List<EnvLog> GetEnvLogsBySmartBox(int smartBoxID)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT * FROM EnvLog WHERE SmartBoxID = @SmartBoxID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", smartBoxID);
            return conn.Query<EnvLog>(sql,param).ToList();
        }
    }
    public List<EnvLog> GetEnvLogsByDate(DateTime date)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT * FROM EnvLog WHERE DateTime = @DateTime";
            DynamicParameters param = new DynamicParameters();
            param.Add("@DateTime", date);
            return conn.Query<EnvLog>(sql,param).ToList();
        }
    }
    public List<EnvLog> GetEnvLogsBySmartBoxAndDate(int smartBoxID, DateTime date)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT * FROM EnvLog WHERE SmartBoxID = @SmartBoxID AND DateTime = @DateTime";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", smartBoxID);
            param.Add("@DateTime", date);
            return conn.Query<EnvLog>(sql,param).ToList();
        }
    }
    public EnvLog GetLatestEnvLogBySmartBox(int smartBoxID)
    {
        using (DbConnection conn = new SqlConnection(_connectionString))
        {
            CheckConnOpen(conn);
            string sql = @"SELECT TOP 1 * FROM EnvLog WHERE SmartBoxID = @SmartBoxID ORDER BY DateTime DESC";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SmartBoxID", smartBoxID);
            return conn.QueryFirstOrDefault<EnvLog>(sql,param);
        }

    }


}