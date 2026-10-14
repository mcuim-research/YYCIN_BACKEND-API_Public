using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using test3.Models;
namespace test3.Repo.AccountRepo
{
    public interface IAccountRepository
    {
        public AccountDTO GetAccount(int accountId);
        public IEnumerable<AccountDTO> GetAllAccounts();
        public (Account, string) AddAccount(Account account);
        public Account UpdateAccount(Account account);
        public bool DeleteAccount(string accountId);
        public AccountDTO GetAccount(string email);
        public Patient? GetPatient(int patientId);
        public IEnumerable<Patient> GetPatients();
        public Patient UpdatePatient(Patient patient);
        public bool DeletePatient(int patientId, bool deleteAccount);
        public (Patient,string) AddPatient(Patient patient, bool AddAccount);
        public Caregiver GetCaregiver(int caregiverId);
        public IEnumerable<Caregiver> GetCaregivers();
        public Caregiver UpdateCaregiver(Caregiver caregiver);
        public bool DeleteCaregiver(int caregiver, bool deleteAccount);
        public (Caregiver,string) AddCaregiver(Caregiver caregiver, bool AddAccount);
        public Doctor GetDoctor(int doctorId);
        public IEnumerable<Doctor> GetDoctors();
        public Doctor UpdateDoctor(Doctor doctor);
        public bool DeleteDoctor(Doctor doctor, bool deleteAccount);
        public Doctor AddDoctor(Doctor doctor, bool AddAccount);
        public List<Patient> GetPatients(Caregiver caregiver, bool agreed);
        public void BindCaregiver(Patient pat, Caregiver car);
        public void BindCaregiver(Caregiver car, Patient pat);
        public List<Caregiver> GetBindingRequestFromCaregivers(int patientId);
        public List<Patient> GetBindingRequestFromPatients(int caregiverId);
        public List<Caregiver> GetAcceptedBindingRequestFromCaregivers(int patientId);
        public List<Patient> GetAcceptedBindingRequestFromPatients(int caregiverId);

        public bool AuthenticateAccount(string code);
        public void UnbindCaregiver(Patient pat, Caregiver car);

        public bool ReauthenticicateAccount(string email);


    }
    public class AccountRepository : IAccountRepository
    {
        public void checkConnOpen(IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }
        public string _connStr = "";
        /** 
        CONTEXT: Session
        Session contains: 
        UserID,
        Username,
        Role
        **/
        public AccountRepository(IConfiguration conf)
        {
            _connStr = conf.GetConnectionString("DevConnection");
        }

        // This is just for Administrator, for regular users, they'll have to go through Patient or Caregiver registration steps.
        public (Account, string) AddAccount(Account account)
        {
            string authenticationString;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {


                checkConnOpen(conn);
                //check email exists
                DynamicParameters param = new DynamicParameters();

                param.Add("@m", account.Email);
                string sql = @"SELECT AccountID FROM Account WHERE Email = @m";

                int accId = conn.Query<int>(sql,param).FirstOrDefault();
                Console.WriteLine(account.Email);
                Console.WriteLine(accId);
                if (accId != 0) throw new System.SystemException("Email already exists");
                if (account.Email == null)
                    sql = @"INSERT INTO Account (Email, Password, IsValid) VALUES (null, null, 0)";
                else
                {
                    sql = @"INSERT INTO Account (Email, Password, IsValid, Authentication) VALUES (@m, @password, 0, @auth)";
                }
                param = new DynamicParameters();
                param.Add("@m", account.Email);
                param.Add("@password", account.Password);
                authenticationString = Guid.NewGuid().ToString();
                //test collision
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@auth", authenticationString);
                var res = conn.Query<int>(@"SELECT AccountID FROM Account WHERE Authentication = @auth", param1);
                while (res.Count() != 0)
                {
                    authenticationString = Guid.NewGuid().ToString();
                    param1 = new DynamicParameters();
                    param1.Add("@auth", authenticationString);
                    res = conn.Query<int>(@"SELECT AccountID FROM Account WHERE Authentication = @auth", param1);
                }

                param.Add("@auth", authenticationString);

                conn.Execute(sql, param);

                // Get the AccountID
                sql = @"SELECT AccountID FROM Account WHERE Email = @m";
                int accountId = conn.Query<int>(sql, param).FirstOrDefault();
                account.AccountID = accountId;
            }
            return (account, authenticationString);
        }

        public bool DeleteAccount(string accountId)
        {
            bool success = false;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {

                try
                {
                    checkConnOpen(conn);
                    string sql = @"SELECT AccountID FROM Account WHERE AccountID = @accountId";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@accountId", accountId);
                    int accId = conn.Query<int>(sql, param).FirstOrDefault();
                    if (accId == 0) throw new AccountNotFoundException();

                    sql = @"DELETE FROM Account WHERE AccountID = @accountId";
                    param.Add("@accountId", accountId);
                    conn.Execute(sql, param);
                    success = true;
                }
                catch (Exception e)
                {
                    success = false;
                }

            }
            return success;


        }
        public AccountDTO GetAccount(string email)
        {
            AccountDTO acc = new AccountDTO();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                DynamicParameters param = new DynamicParameters();
                param.Add("@email", email);
                string sql = @"select Account.AccountID, Email, Password, IsValid, PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, DoctorID, Doctor.Name FROM Account
                    FULL JOIN Patient ON Account.AccountID = Patient.AccountID
                    FULL JOIN Caregiver ON Caregiver.AccountID = Account.AccountID
                    FULL JOIN Doctor ON Doctor.AccountID = Account.AccountID
                    WHERE Account.Email = @email
                ";

                acc = conn.Query<Models.Account, Patient, Caregiver, Doctor, test3.Models.AccountDTO>(sql, (account, patient, caregiver, doctor) =>
                {
                    AccountDTO acc = new AccountDTO(account);
                    acc.Patient = patient;
                    acc.Caregiver = caregiver;
                    acc.Doctor = doctor;

                    acc.CheckType();
                    return acc;
                }, param, splitOn: "PatientID,CaregiverID,DoctorID").FirstOrDefault();
            }
            if (acc == null) throw new AccountNotFoundException();
            return acc;
        }

        public AccountDTO GetAccount(int accountId)
        {
            AccountDTO acc = new AccountDTO();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                DynamicParameters param = new DynamicParameters();
                param.Add("@userId", accountId);
                string sql = @"select Account.AccountID, Email, Password, IsValid, PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, DoctorID, Doctor.Name FROM Account
                    FULL JOIN Patient ON Account.AccountID = Patient.AccountID
                    FULL JOIN Caregiver ON Caregiver.AccountID = Account.AccountID
                    FULL JOIN Doctor ON Doctor.AccountID = Account.AccountID
                    WHERE Account.AccountID = @userId
                ";

                acc = conn.Query<Models.Account, Patient, Caregiver, Doctor, test3.Models.AccountDTO>(sql, (account, patient, caregiver, doctor) =>
                {
                    AccountDTO acc = new AccountDTO(account);
                    acc.Patient = patient;
                    acc.Caregiver = caregiver;
                    acc.Doctor = doctor;

                    acc.CheckType();
                    return acc;
                }, param, splitOn: "PatientID,CaregiverID,DoctorID").FirstOrDefault();
            }
            if (acc == null) throw new AccountNotFoundException();
            return acc;
        }

        public IEnumerable<AccountDTO> GetAllAccounts()
        {
            IEnumerable<AccountDTO> accounts = new List<AccountDTO>();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                DynamicParameters param = new DynamicParameters();
                string sql = @"select Account.AccountID, Email, Password, IsValid, PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, DoctorID, Doctor.Name FROM Account
                    FULL JOIN Patient ON Account.AccountID = Patient.AccountID
                    FULL JOIN Caregiver ON Caregiver.AccountID = Account.AccountID
                    FULL JOIN Doctor ON Doctor.AccountID = Account.AccountID
                ";

                accounts = conn.Query<Models.Account, Patient, Caregiver, Doctor, test3.Models.AccountDTO>(sql, (account, patient, caregiver, doctor) =>
                {
                    AccountDTO acc = new AccountDTO(account);
                    acc.Patient = patient;
                    acc.Caregiver = caregiver;
                    acc.Doctor = doctor;

                    acc.CheckType();
                    return acc;
                }, param, splitOn: "PatientID,CaregiverID,DoctorID");
            }
            return accounts;
        }

        public Account UpdateAccount(Account account)
        {
            using (IDbConnection conn = new SqlConnection(_connStr))
            {


                checkConnOpen(conn);
                string sql = @"UPDATE Account SET Email = @email, Password = @password WHERE AccountID = @accountId";
                DynamicParameters param = new DynamicParameters();
                param.Add("@accountId", account.AccountID);
                param.Add("@email", account.Email);
                param.Add("@password", account.Password);
                conn.Execute(sql, param);
            }
            return account;
        }

        public Patient? GetPatient(int patientId)
        {
            Patient pat;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.Gender ,Account.AccountID, Account.Email, Account.Password, Account.IsValid, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, Caregiver.AccountID, Doctor.DoctorID, Doctor.Name, Doctor.AccountID FROM Patient
                 FULL JOIN Account
                 on Patient.AccountID = Account.AccountID
                 FULL JOIN PatientCaregiver
                 on Patient.PatientID = PatientCaregiver.PatientID
                 FULL JOIN Caregiver
                 on PatientCaregiver.CaregiverID = Caregiver.CaregiverID AND PatientCaregiver.CaregiverAgreed = 1 AND PatientCaregiver.PatientAgreed = 1
                    FULL JOIN PatientDoctor
                    on Patient.PatientID = PatientDoctor.PatientID
                    FULL JOIN Doctor
                    on PatientDoctor.DoctorID = Doctor.DoctorID
                 WHERE Patient.PatientID = @patientId";
                DynamicParameters param = new DynamicParameters();
                param.Add("@patientId", patientId);
                pat = conn.Query<Patient, Account, Caregiver, Doctor, Patient>(sql, (patient, account, caregiver, doctor) =>
                {
                    patient.Account = account;
                    if(patient.Caregivers == null){
                        patient.Caregivers = new List<Caregiver> { caregiver };
                    }else{
                        patient.Caregivers.Add(caregiver);
                    }
                    patient.Doctors = new List<Doctor> { doctor };


                    return patient;
                }, param, splitOn: "AccountID, CaregiverID, DoctorID").FirstOrDefault();
            };
            return pat;
        }
        public IEnumerable<Patient> GetPatients()
        {
            IEnumerable<Patient> pat = new List<Patient>();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.Gender,Account.AccountID, Account.Email, Account.Password, Account.IsValid, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, Caregiver.AccountID, Doctor.DoctorID, Doctor.Name, Doctor.AccountID FROM Patient
                 FULL JOIN Account
                 on Patient.AccountID = Account.AccountID
                 FULL JOIN PatientCaregiver
                 on Patient.PatientID = PatientCaregiver.PatientID
                 FULL JOIN Caregiver
                 on PatientCaregiver.CaregiverID = Caregiver.CaregiverID 
                    FULL JOIN PatientDoctor
                    on Patient.PatientID = PatientDoctor.PatientID
                    FULL JOIN Doctor
                    on PatientDoctor.DoctorID = Doctor.DoctorID
                    ";

                pat = conn.Query<Patient, Account, Caregiver, Doctor, Patient>(sql, (patient, account, caregiver, doctor) =>
                {
                    patient.Account = account;
                    patient.Caregivers = new List<Caregiver> { caregiver };
                    patient.Doctors = new List<Doctor> { doctor };

                    return patient;
                }, splitOn: "AccountID, CaregiverID, DoctorID");
            };
            return pat;
        }

        public Patient UpdatePatient(Patient patient)
        {
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"UPDATE Patient SET Name = @name, Age = @age, PhoneNumber = @phone, Gender = @gender WHERE PatientID = @pat";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", patient.Name);
                param.Add("@age", patient.Age);
                param.Add("@phone", patient.PhoneNumber);
                param.Add("@pat", patient.PatientID);
                param.Add("@gender", patient.Gender);
                conn.Execute(sql, param);
            }
            return patient;
        }

        public bool DeletePatient(int patientID, bool deleteAccount)
        {
            bool success = false;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {

                try
                {
                    checkConnOpen(conn);
                    string sql = @"SELECT PatientID FROM Patient WHERE PatientID = @pat";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@pat", patientID);
                    int patId = conn.Query<int>(sql, param).FirstOrDefault();
                    if (patId == 0) throw new AccountNotFoundException();
                    int accountID = conn.Query<int>(@"SELECT AccountID FROM Patient WHERE PatientID = @pat", param).FirstOrDefault();
                    sql = @"DELETE FROM Patient WHERE PatientID = @pat";
                    param.Add("@pat", patientID);
                    conn.Execute(sql, param);
                    if (deleteAccount)
                    {
                        DeleteAccount(accountID.ToString());
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    success = false;
                }

            }
            return success;
        }


        public (Patient,string) AddPatient(Patient patient, bool addAccount)
        {
            try{
            string authenticationString ="";
            if (addAccount)
            {
                var acc = AddAccount(patient.Account);
                patient.Account = acc.Item1;
                authenticationString = acc.Item2;
            }
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"INSERT INTO Patient (Name, Age, PhoneNumber, Gender, AccountID) VALUES (@name, @age, @phone, @gender, @acc)";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", patient.Name, DbType.String);
                param.Add("@age", patient.Age);
                param.Add("@phone", patient.PhoneNumber);
                param.Add("@gender", patient.Gender);
                param.Add("@acc", patient.Account.AccountID);
                conn.Execute(sql, param);
                sql = @"SELECT PatientID FROM Patient WHERE AccountID = @acc";
                int patId = conn.Query<int>(sql, param).FirstOrDefault();
                patient.PatientID = patId;
            }
            return (patient, authenticationString);
            }catch(Exception e){
                Console.WriteLine(e);
                throw e;
            }
        }

        public Caregiver GetCaregiver(int caregiverId)
        {
            Caregiver cg;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, Caregiver.Gender ,Account.AccountID, Account.Email, Password, IsValid FROM Caregiver
                 FULL JOIN Account
                 on Caregiver.AccountID = Account.AccountID
                 WHERE CaregiverID = @cg";
                DynamicParameters param = new DynamicParameters();
                param.Add("@cg", caregiverId);
                cg = conn.Query<Caregiver, Account, Caregiver>(sql, (caregiver, account) =>
                {
                    caregiver.Account = account;
                    return caregiver;
                }, splitOn: "AccountID",param:param).FirstOrDefault();
            };
            return cg;
        }

        public IEnumerable<Caregiver> GetCaregivers()
        {
            IEnumerable<Caregiver> cg = new List<Caregiver>();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select CaregiverID, Caregiver.Name, Caregiver.PhoneNumber,Caregiver.Age, Caregiver.Gender , Account.AccountID, Account.Email, Password, IsValid FROM Caregiver
                 FULL JOIN Account
                 on Caregiver.AccountID = Account.AccountID";

                cg = conn.Query<Caregiver, Account, Caregiver>(sql, (caregiver, account) =>
                {
                    caregiver.Account = account;
                    return caregiver;
                }, splitOn: "AccountID");
            };
            return cg;
        }

        public Caregiver UpdateCaregiver(Caregiver caregiver)
        {
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"UPDATE Caregiver SET Name = @name, PhoneNumber = @phone, Age = @age, Gender = @gender WHERE CaregiverID = @cg";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", caregiver.Name);
                param.Add("@phone", caregiver.PhoneNumber);
                param.Add("@cg", caregiver.CaregiverId);
                param.Add("@age", caregiver.Age);
                param.Add("@gender",caregiver.Gender);
                conn.Execute(sql, param);
            }
            return caregiver;
        }

        public bool DeleteCaregiver(int caregiverId, bool deleteAccount)
        {
            bool success = false;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {

                try
                {
                    checkConnOpen(conn);
                    Caregiver caregiver = GetCaregiver(caregiverId);
                    string sql = @"SELECT CaregiverID FROM Caregiver WHERE CaregiverID = @cg";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@cg", caregiverId);
                    int cgId = conn.Query<int>(sql, param).FirstOrDefault();
                    if (cgId == 0) throw new AccountNotFoundException();

                    sql = @"DELETE FROM Caregiver WHERE CaregiverID = @cg";
                    param.Add("@cg", caregiverId);
                    conn.Execute(sql, param);
                    if (deleteAccount)
                    {
                        DeleteAccount(caregiver.Account.AccountID.ToString());
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    success = false;
                }

            }
            return success;
        }

        public (Caregiver, string) AddCaregiver(Caregiver caregiver, bool addAccount)
        {
            string authenticationString = "";
            if (addAccount)
            {
                var acc = AddAccount(caregiver.Account);
                caregiver.Account = acc.Item1;
                authenticationString = acc.Item2;
            }
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"INSERT INTO Caregiver (Name, Age, PhoneNumber, Gender, AccountID) VALUES (@name, @age,@phone,@gender ,@acc)";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", caregiver.Name);
                param.Add("@phone", caregiver.PhoneNumber);
                param.Add("@age", caregiver.Age);
                param.Add("@gender",caregiver.Gender);
                param.Add("@acc", caregiver.Account.AccountID);
                conn.Execute(sql, param);
                sql = @"SELECT CaregiverID FROM Caregiver WHERE AccountID = @acc";
                int cgId = conn.Query<int>(sql, param).FirstOrDefault();
                caregiver.CaregiverId = cgId;
            }
            return (caregiver, authenticationString);
        }

        public Doctor GetDoctor(int doctorId)
        {
            Doctor doc;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select DoctorID, Doctor.Name, Account.AccountID, Account.Email, Password, IsValid FROM Doctor
                 FULL JOIN Account
                 on Doctor.AccountID = Account.AccountID
                 WHERE DoctorID = @doc";
                DynamicParameters param = new DynamicParameters();
                param.Add("@doc", doctorId);
                doc = conn.Query<Doctor, Account, Doctor>(sql, (doctor, account) =>
                {
                    doctor.Account = account;
                    return doctor;
                }, splitOn: "AccountID").FirstOrDefault();
            };
            return doc;
        }

        public IEnumerable<Doctor> GetDoctors()
        {
            IEnumerable<Doctor> doc = new List<Doctor>();
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select DoctorID, Doctor.Name, Account.AccountID, Account.Email, Password, IsValid FROM Doctor
                 FULL JOIN Account
                 on Doctor.AccountID = Account.AccountID";

                doc = conn.Query<Doctor, Account, Doctor>(sql, (doctor, account) =>
                {
                    doctor.Account = account;
                    return doctor;
                }, splitOn: "AccountID");
            };
            return doc;
        }

        public Doctor UpdateDoctor(Doctor doctor)
        {
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"UPDATE Doctor SET Name = @name WHERE DoctorID = @doc";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", doctor.Name);
                param.Add("@doc", doctor.DoctorID);
                conn.Execute(sql, param);
            }
            return doctor;
        }

        public bool DeleteDoctor(Doctor doctor, bool deleteAccount)
        {
            bool success = false;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {

                try
                {
                    checkConnOpen(conn);
                    string sql = @"SELECT DoctorID FROM Doctor WHERE DoctorID = @doc";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@doc", doctor.DoctorID);
                    int docId = conn.Query<int>(sql, param).FirstOrDefault();
                    if (docId == 0) throw new AccountNotFoundException();

                    sql = @"DELETE FROM Doctor WHERE DoctorID = @doc";
                    param.Add("@doc", doctor.DoctorID);
                    conn.Execute(sql, param);
                    if (deleteAccount)
                    {
                        DeleteAccount(doctor.Account.AccountID.ToString());
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    success = false;
                }

            }
            return success;
        }

        public Doctor AddDoctor(Doctor doctor, bool addAccount)
        {
            if (addAccount)
            {
                var acc = AddAccount(doctor.Account);
                doctor.Account = acc.Item1;
            }
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"INSERT INTO Doctor (Name, AccountID) VALUES (N@name, @acc)";
                DynamicParameters param = new DynamicParameters();
                param.Add("@name", doctor.Name);
                param.Add("@acc", doctor.Account.AccountID);
                conn.Execute(sql, param);
                sql = @"SELECT DoctorID FROM Doctor WHERE AccountID = @acc";
                int docId = conn.Query<int>(sql, param).FirstOrDefault();
                doctor.DoctorID = docId;
            }
            return doctor;
        }
        public List<Patient> GetPatients(Caregiver caregiver, bool agreed = false)
        {
            List<Patient> pats;
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = "";
                if (agreed){
                    sql = @"select Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Patient.Gender ,Account.AccountID, Account.Email, Account.Password, Account.IsValid, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, Caregiver.AccountID, Doctor.DoctorID, Doctor.Name, Doctor.AccountID FROM Patient
                    FULL JOIN Account
                    on Patient.AccountID = Account.AccountID
                    FULL JOIN PatientCaregiver
                    on Patient.PatientID = PatientCaregiver.PatientID
                    FULL JOIN Caregiver
                    on PatientCaregiver.CaregiverID = Caregiver.CaregiverID
                        FULL JOIN PatientDoctor
                        on Patient.PatientID = PatientDoctor.PatientID
                        FULL JOIN Doctor
                        on PatientDoctor.DoctorID = Doctor.DoctorID
                    WHERE Caregiver.CaregiverID = @cg AND PatientCaregiver.CaregiverAgreed = 1 AND PatientCaregiver.PatientAgreed = 1";
                }else{
                                       sql = @"select Patient.PatientID, Patient.Name, Patient.Age, Patient.PhoneNumber, Account.AccountID, Account.Email, Account.Password, Account.IsValid, Caregiver.CaregiverID, Caregiver.Name, Caregiver.PhoneNumber, Caregiver.AccountID, Doctor.DoctorID, Doctor.Name, Doctor.AccountID FROM Patient
                    FULL JOIN Account
                    on Patient.AccountID = Account.AccountID
                    FULL JOIN PatientCaregiver
                    on Patient.PatientID = PatientCaregiver.PatientID
                    FULL JOIN Caregiver
                    on PatientCaregiver.CaregiverID = Caregiver.CaregiverID
                        FULL JOIN PatientDoctor
                        on Patient.PatientID = PatientDoctor.PatientID
                        FULL JOIN Doctor
                        on PatientDoctor.DoctorID = Doctor.DoctorID
                    WHERE Caregiver.CaregiverID = @cg"; 
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@cg", caregiver.CaregiverId);
                pats = conn.Query<Patient, Account, Caregiver, Doctor, Patient>(sql, (patient, account, caregiver, doctor) =>
                {
                    patient.Account = account;
                    patient.Caregivers = new List<Caregiver> { caregiver };
                    patient.Doctors = new List<Doctor> { doctor };
                    return patient;
                }).ToList();

            }
            return pats;
        }
        public bool AuthenticateAccount(string code){
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"SELECT AccountID FROM Account WHERE Authentication = @code";
                DynamicParameters param = new DynamicParameters();
                param.Add("@code", code);
                int accId = conn.Query<int>(sql, param).FirstOrDefault();
                if (accId == 0) return false;
                sql = @"UPDATE Account SET IsValid = 1, Authentication=null WHERE Authentication = @code";
                conn.Execute(sql, param);
                return true;
            }
        }
        public bool ReauthenticicateAccount(string email){
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"SELECT AccountID FROM Account WHERE Email = @email";
                DynamicParameters param = new DynamicParameters();
                param.Add("@email", email);
                int accId = conn.Query<int>(sql, param).FirstOrDefault();
                if (accId == 0) return false;
                string authenticationString = Guid.NewGuid().ToString();
                sql = @"UPDATE Account SET Authentication = @auth WHERE AccountID = @acc";
                param.Add("@auth", authenticationString);
                conn.Execute(sql, param);
                return true;
            }
        }

        /// <summary>
        /// 绑定caregiver和patient，
        /// </summary>
        /// <param name="pat">發起人patient</param>
        /// <param name="car"></param>欲徵求同意的caregiver<summary>
        public void BindCaregiver(Patient pat, Caregiver car){
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select PatientID, CaregiverID from PatientCaregiver WHERE PatientID = @pat AND CaregiverID = @car";
                DynamicParameters param = new DynamicParameters();
                param.Add("@pat", pat.PatientID);
                param.Add("@car", car.CaregiverId);
                int res = conn.Query<int>(sql, param).FirstOrDefault();
                if (res != 0){
                    sql = @"update PatientCaregiver set PatientAgreed=1 WHERE PatientID = @pat AND CaregiverID = @car";
                }else{
                    sql = @"insert into PatientCaregiver (PatientID, CaregiverID, PatientAgreed, CaregiverAgreed) VALUES (@pat, @car, 1, 0)";
                }
                conn.Query(sql, param);
            }
        }
        public void BindCaregiver(Caregiver car, Patient pat){
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"select PatientID, CaregiverID from PatientCaregiver WHERE PatientID = @pat AND CaregiverID = @car";
                DynamicParameters param = new DynamicParameters();
                param.Add("@pat", pat.PatientID);
                param.Add("@car", car.CaregiverId);
                int res = conn.Query<int>(sql, param).FirstOrDefault();
                if (res != 0){
                    sql = @"update PatientCaregiver set CaregiverAgreed=1 WHERE PatientID = @pat AND CaregiverID = @car";
                }else{
                    sql = @"insert into PatientCaregiver (PatientID, CaregiverID, PatientAgreed, CaregiverAgreed) VALUES (@pat, @car, 0, 1)";
                }
                conn.Query(sql, param);
            }
        }
        public void UnbindCaregiver(Patient pat, Caregiver car){
            using (IDbConnection conn = new SqlConnection(_connStr))
            {
                checkConnOpen(conn);
                string sql = @"DELETE FROM PatientCaregiver WHERE PatientID = @pat AND CaregiverID = @car";
                DynamicParameters param = new DynamicParameters();
                param.Add("@pat", pat.PatientID);
                param.Add("@car", car.CaregiverId);
                conn.Query(sql, param);
            }
        }

        public List<Caregiver> GetBindingRequestFromCaregivers(int patientId)
        {
            using (IDbConnection conn = new SqlConnection(_connStr)){
                checkConnOpen(conn);
                string sql = @"select PatientCaregiver.CaregiverID from Patient 
                INNER JOIN PatientCaregiver ON Patient.PatientID = PatientCaregiver.PatientID 
                 WHERE PatientCaregiver.PatientID = @pat";                
                 DynamicParameters param = new DynamicParameters();
                param.Add("@pat", patientId);
                var res = conn.Query<int>(sql, param).ToList();
                if(res.Count == 0) return new List<Caregiver>();
                List<Caregiver> caregivers = new List<Caregiver>();
                foreach(int i in res){
                    caregivers.Add(GetCaregiver(i));
                }
                return caregivers;
            }
        }
        public List<Patient> GetBindingRequestFromPatients(int caregiverId){
            using (IDbConnection conn = new SqlConnection(_connStr)){
                checkConnOpen(conn);
                string sql = @"select Patient.PatientID from Patient 
                INNER JOIN PatientCaregiver ON Patient.PatientID = PatientCaregiver.PatientID 
                 WHERE PatientCaregiver.CaregiverID = @car";
                DynamicParameters param = new DynamicParameters();
                param.Add("@car", caregiverId);
                var res = conn.Query<int>(sql, param).ToList();
                if(res.Count == 0) return new List<Patient>();
                List<Patient> patients = new List<Patient>();
                foreach(int i in res){
                    patients.Add(GetPatient(i));
                }
                return patients;
            }
        }

        public List<Caregiver> GetAcceptedBindingRequestFromCaregivers(int patientId)
        {
            using (IDbConnection conn = new SqlConnection(_connStr)){
                checkConnOpen(conn);
                string sql = @"select Caregiver.CaregiverID from Caregiver INNER JOIN PatientCaregiver ON Caregiver.CaregiverID = PatientCaregiver.CaregiverID WHERE PatientCaregiver.PatientID = @pat AND PatientCaregiver.PatientAgreed = 1";
                DynamicParameters param = new DynamicParameters();
                param.Add("@pat", patientId);
                var res = conn.Query<int>(sql, param).ToList();
                if(res.Count == 0) return new List<Caregiver>();
                List<Caregiver> caregivers = new List<Caregiver>();
                foreach(int i in res){
                    caregivers.Add(GetCaregiver(i));
                }
                return caregivers;
            }
        }

        public List<Patient> GetAcceptedBindingRequestFromPatients(int caregiverId)
        {
            using (IDbConnection conn = new SqlConnection(_connStr)){
                checkConnOpen(conn);
                string sql = @"select Patient.PatientID from Patient INNER JOIN PatientCaregiver ON Patient.PatientID = PatientCaregiver.PatientID WHERE PatientCaregiver.CaregiverID = @car AND PatientCaregiver.CaregiverAgreed = 1";
                DynamicParameters param = new DynamicParameters();
                param.Add("@car", caregiverId);
                var res = conn.Query<int>(sql, param).ToList();
                if(res.Count == 0) return new List<Patient>();
                List<Patient> patients = new List<Patient>();
                foreach(int i in res){
                    patients.Add(GetPatient(i));
                }
                return patients;
            }
        }
    }
}
public class AccountNotFoundException : System.SystemException
{
    public AccountNotFoundException() : base("Account not found")
    {

    }

}