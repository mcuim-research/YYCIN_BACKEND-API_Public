using System.Data;
using System.Data.SqlClient;
using Dapper;

public interface ITestRemindRepo{
    TestRemind GetTestRemind(int id);
    List<TestRemind> GetAllTestReminds();
    void AddTestRemind(TestRemind testRemind);
    void DeleteTestRemind(int id);
    void UpdateTestRemind(TestRemind testRemind);
}

public class TestRemindRepo: ITestRemindRepo{
    private string _connectionString;
    public TestRemindRepo(IConfiguration conn){
        _connectionString = conn.GetConnectionString("DevConnection");
    }

    public void AddTestRemind(TestRemind testRemind)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            conn.Open();
            string sql = "INSERT INTO TestRemind (Remind, Text) VALUES (@Remind, @Text)";
            conn.Execute(sql, testRemind);
        }
    }

    public void DeleteTestRemind(int id)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            conn.Open();
            string sql = "DELETE FROM TestRemind WHERE ID = @id";
            conn.Execute(sql, new {id});
        }
    }

    public List<TestRemind> GetAllTestReminds()
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            conn.Open();
            string sql = "SELECT * FROM TestRemind";
            return conn.Query<TestRemind>(sql).ToList();
        }
    }

    public TestRemind GetTestRemind(int id)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            conn.Open();
            string sql = "SELECT * FROM TestRemind WHERE ID = @id";
            return conn.QueryFirstOrDefault<TestRemind>(sql, new {id});
        }
    }

    public void UpdateTestRemind(TestRemind testRemind)
    {
        using(IDbConnection conn = new SqlConnection(_connectionString)){
            conn.Open();
            string sql = "UPDATE TestRemind SET Remind = @Remind, Text = @Text WHERE ID = @ID";
            conn.Execute(sql, testRemind);
        }
    }
}