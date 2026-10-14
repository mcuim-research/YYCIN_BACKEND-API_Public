using System.Data;
using System.Data.SqlClient;
using Dapper;
using test3.Models;

public interface IAccountFirebaseRepo
{
    List<AccountFirebase> GetAllAccountFirebase();
    void AddAccountFirebase(AccountFirebase accountFirebase);
    void DeleteAccountFirebase(int id);
    void DeleteAccountFirebaseByToken(string token);
    void UpdateAccountFirebase(AccountFirebase accountFirebase);
    string[] GetTokensByAccount(int accountID);
    bool checkTokenExisits(string token);

}
public class AccountFirebaseRepo: IAccountFirebaseRepo{
    private string _connectionString;
    public AccountFirebaseRepo(IConfiguration conn){
        _connectionString = conn.GetConnectionString("DevConnection");
    }

    public void AddAccountFirebase(AccountFirebase accountFirebase)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@AccountID", accountFirebase.AccountID);
            parameters.Add("@Token", accountFirebase.Token);
            conn.Query("INSERT INTO AccountFirebase (AccountID, Token) VALUES (@AccountID, @Token)", parameters);
        }
    }

    public void checkConnectionOpen(IDbConnection conn){
        if(conn.State == ConnectionState.Closed){
            conn.Open();
        }
    }

    public void DeleteAccountFirebase(int id)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@AccountID", id);
            conn.Query("DELETE FROM AccountFirebase WHERE AccountID = @AccountID", parameters);
        }
    }

    public void DeleteAccountFirebaseByToken(string token)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Token", token);
            conn.Query("DELETE FROM AccountFirebase WHERE Token = @Token", parameters);
        }
    }

    public List<AccountFirebase> GetAllAccountFirebase()
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            return conn.Query<AccountFirebase>("SELECT * FROM AccountFirebase").ToList();
        }
    }


    public string[] GetTokensByAccount(int accountID)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@accountID", accountID);
            return conn.Query<string>("SELECT Token FROM AccountFirebase WHERE AccountID = @accountID", parameters).ToArray();
        }
    }

    public void UpdateAccountFirebase(AccountFirebase accountFirebase)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Token", accountFirebase.Token);
            parameters.Add("@AccountID", accountFirebase.AccountID);
            conn.Query("UPDATE AccountFirebase SET Token = @Token WHERE AccountID = @AccountID AND Token=@Token", parameters);
    }
    }
    public bool checkTokenExisits(string token)
    {
        using (IDbConnection conn = new SqlConnection(_connectionString))
        {
            checkConnectionOpen(conn);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Token", token);
            return conn.Query<bool>("SELECT COUNT(*) FROM AccountFirebase WHERE Token = @Token", parameters).FirstOrDefault();
        }
    }
}