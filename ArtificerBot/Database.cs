using Artificer.Utility;
using Microsoft.Data.Sqlite;
#pragma warning disable CS8618

namespace Artificer.Bot;

public class Database : IAsyncInitialization
{
    private SqliteConnection _sqlConnection;
    
    public Database()
    {
	    Initialization = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
	    var databaseName = Kernel.IsDebug ? "debugbase.db" : "database.db";
	    
	    var datapasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseName);
	    
	    _sqlConnection = new SqliteConnection($"Data Source={datapasePath}; Cache=Shared;");
        await _sqlConnection.OpenAsync();
        
        await IntegrityCheck();
    }

    private async Task IntegrityCheck()
    {
	    var userTableAsync = CreateUserTableAsync();
	    //var organizationAsync = CreatePaymentTableAsync();
        var organizationTableAsync = CreateOrganizationTableAsync();
        var requestTableAsync = CreateRequestTableAsync();
        var whisperRequestTableAsync = CreateWhisperRequestTableAsync();
        var inviteTableAsync = CreateInviteTableAsync();
        var paymentTableAsync = CreatePaymentTableAsync();
        var actionsTableAsync = CreateActionsTableAsync();
        var statisticTableAsync = CreateStatisticsTableAsync();

        Debug.Log("Database integrity check...", Debug.Sender.Database, Debug.MessageStatus.INFO);
        await Task.WhenAll(userTableAsync, organizationTableAsync, requestTableAsync, whisperRequestTableAsync, 
	        inviteTableAsync, paymentTableAsync, actionsTableAsync, statisticTableAsync);
        Debug.Log("Verification completed. The integrity of the database is not broken.", 
	        Debug.Sender.Database, Debug.MessageStatus.INFO);
    }
    
    private async Task CreateUserTableAsync()
    {
        var command = _sqlConnection.CreateCommand();
        command.CommandText = $@"CREATE TABLE IF NOT EXISTS users (
	            id              NUMERIC PRIMARY KEY UNIQUE,
	            organization    INTEGER DEFAULT NULL,
	            firstname		TEXT DEFAULT NULL,
	            lastname		TEXT DEFAULT NULL,
	            telephone		TEXT DEFAULT NULL,
	            stage           INTEGER NOT NULL DEFAULT 0,
	            balance     	REAL NOT NULL DEFAULT 0,
	            trial			INTEGER NOT NULL DEFAULT 1,
	            conversation	TEXT NOT NULL DEFAULT '[]',
	            date_of_reg     TIMESTAMP NOT NULL,
	            enabled         INTEGER NOT NULL DEFAULT 1
            )";
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task CreateOrganizationTableAsync()
    {
        var command = _sqlConnection.CreateCommand();
        command.CommandText = $@"CREATE TABLE IF NOT EXISTS organizations (
	            id          INTEGER PRIMARY KEY AUTOINCREMENT,
	            manager     NUMERIC UNIQUE NOT NULL DEFAULT 0,
	            name        TEXT,
	            type        INTEGER NOT NULL DEFAULT 0,
	            stage       INTEGER NOT NULL DEFAULT 0,
	            rate 		INTEGER NOT NULL DEFAULT 0,
	            date_of_reg TIMESTAMP NOT NULL
            )";
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateRequestTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS requests (
	            id          	INTEGER PRIMARY KEY AUTOINCREMENT,
	            user_id     	NUMERIC NOT NULL DEFAULT 0,
	            organization_id INTEGER DEFAULT NULL,
	            total_tokens    INTEGER NOT NULL DEFAULT 0,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }

    private async Task CreateStatisticsTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS statistics (
	            dau				INTEGER NOT NULL,
	            wau 			INTEGER NOT NULL,
	            mau			    INTEGER NOT NULL,
	            mnu				INTEGER NOT NULL,
	            utilisation		NUMERIC NOT NULL,
	            frequency		NUMERIC NOT NULL,
	            churn_rate		NUMERIC NOT NULL,
	            arpu			NUMERIC NOT NULL,
	            arppu			NUMERIC NOT NULL,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }

    private async Task CreateActionsTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS actions (
	            user_id     	NUMERIC NOT NULL,
	            type 			INTEGER NOT NULL,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }
    
    private async Task CreateWhisperRequestTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS wrequests (
	            id          	INTEGER PRIMARY KEY AUTOINCREMENT,
	            user_id     	NUMERIC NOT NULL DEFAULT 0,
	            organization_id INTEGER DEFAULT NULL,
	            seconds    		INTEGER NOT NULL DEFAULT 0,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }

    private async Task CreateInviteTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS invites (
	            id          	INTEGER PRIMARY KEY AUTOINCREMENT,
	            organization_id INTEGER NOT NULL DEFAULT 0,
	            uuid			TEXT NOT NULL,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }
    
    private async Task CreatePaymentTableAsync()
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $@"CREATE TABLE IF NOT EXISTS payments (
	            id          	TEXT NOT NULL UNIQUE,
	            userid			NUMERIC NOT NULL,
	            amount 			REAL NOT NULL,
	            status			INTEGER NOT NULL DEFAULT 0,
	            datetime 		TIMESTAMP NOT NULL 
            )";
	    await command.ExecuteNonQueryAsync();
    }

    public List<Organization> GetAllOrganization()
    {
	    Debug.Log("Getting information about organizations...", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    var organizations = new List<Organization>();

	    using (var command = _sqlConnection.CreateCommand())
	    {
		    command.CommandText = @"SELECT * FROM organizations";

		    using (var reader = command.ExecuteReader())
		    {
			    if (reader.HasRows)
			    {
				    while (reader.Read())
				    {
					    var manager = reader.GetFieldValue<long>(1);
					    var name = reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2);
					    var type = reader.GetFieldValue<int>(3);
					    var stage = reader.GetFieldValue<int>(4);
					    var rate = reader.GetFieldValue<int>(5);
					    var date_of_reg = reader.GetFieldValue<DateTime>(6);
					    //var date_as_int = reader.GetBoolean(7);
					    
					    organizations.Add(new Organization(manager, name, (OrganizationType) type, (OrganizationStage) stage, date_of_reg, (OrganizationRate) rate));
				    }
			    }
		    }
	    }

	    Debug.Log("Information about organizations has been successfully uploaded.", Debug.Sender.Database, Debug.MessageStatus.INFO);
	    return organizations;
    }

    public List<User> GetAllUser()
    {
	    Debug.Log("Getting information about users...", Debug.Sender.Database, Debug.MessageStatus.INFO);
	    
	    var users = new List<User>();
	    
	    using (var command = _sqlConnection.CreateCommand())
	    {
		    command.CommandText = @"SELECT * FROM users";

		    using (var reader = command.ExecuteReader())
		    {
			    if (reader.HasRows)
			    {
				    while (reader.Read())
				    {
					    var id = reader.GetFieldValue<long>(0);
					    var organization = reader.IsDBNull(1) ? null : reader.GetFieldValue<long?>(1);
					    var firstname = reader.IsDBNull(2) ? null : reader.GetFieldValue<string?>(2);
					    var lastname = reader.IsDBNull(3) ? null : reader.GetFieldValue<string?>(3);
					    var telephone = reader.IsDBNull(4) ? null : reader.GetFieldValue<string?>(4);
					    var stage =  reader.GetFieldValue<int>(5);
					    var balance = reader.GetFieldValue<double>(6);
					    var trial = reader.GetFieldValue<bool>(7);
					    var conversatios = reader.GetFieldValue<string>(8);
					    var dateOfReg = reader.GetFieldValue<DateTime>(9);
					    var enabled = reader.GetFieldValue<bool>(10);
					    
					    
					    users.Add(new User(id, organization, firstname, lastname, telephone, (UserStage) stage, balance, trial,conversatios, dateOfReg, enabled));
				    }
			    }
		    }
	    }

	    Debug.Log("Information about users has been successfully uploaded.", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    return users;
    }

    public List<Invite> GetAllInvites()
    {
	    Debug.Log("Getting information about invites...", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    var invites = new List<Invite>();
	    
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = @"SELECT * FROM invites";

	    using (var reader = command.ExecuteReader())
	    {
		    while (reader.Read())
		    {
			    var id = reader.GetInt32(1);
			    var uuid = reader.GetString(2);
			    var datetime = reader.GetDateTime(3);
			    
			    invites.Add(new Invite(id, uuid ,datetime));
		    }
	    }

	    Debug.Log("Information invite users has been successfully uploaded.", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    return invites;
    }
    
    public List<Payment> GetAllPayments()
    {
	    Debug.Log("Getting information about payments...", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    var payments = new List<Payment>();
	    
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = @"SELECT * FROM payments";

	    using (var reader = command.ExecuteReader())
	    {
		    while (reader.Read())
		    {
			    var id = reader.GetString(0);
			    var userid = reader.GetInt64(1);
			    var amount = reader.GetDouble(2);
			    var status = (PaymentStatus)reader.GetInt32(3);
			    var datetime = reader.GetDateTime(4);
			    
			    
			    payments.Add(new Payment(id, userid, amount, status, datetime));
		    }
	    }

	    Debug.Log("Information payments has been successfully uploaded.", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    return payments;
    }
    
    public List<Statistic> GetMountlyStatistics()
    {
	    Debug.Log("Getting information about statistics...", Debug.Sender.Database, Debug.MessageStatus.INFO);
	    
	    var statistic = new List<Statistic>();
	    
	    using (var command = _sqlConnection.CreateCommand())
	    {
		    command.CommandText = @"SELECT * FROM statistics where datetime > datetime('now', '-1 month')";

		    using (var reader = command.ExecuteReader())
		    {
			    if (reader.HasRows)
			    {
				    while (reader.Read())
				    {
					    var dau = reader.GetFieldValue<int>(0);
					    var wau = reader.GetFieldValue<int>(1);
					    var mau = reader.GetFieldValue<int>(2);
					    var mnu = reader.GetFieldValue<int>(3);
					    var utilisation = reader.GetFieldValue<double>(4);
					    var frequency = reader.GetFieldValue<double>(5);
					    var churn_rate = reader.GetFieldValue<double>(6);
					    var arpu = reader.GetFieldValue<double>(7);
					    var arppu = reader.GetFieldValue<double>(8);
					    var datetime = reader.GetFieldValue<DateTime>(9);

					    statistic.Add(new Statistic(dau, wau, mau, mnu, utilisation, frequency, churn_rate, arpu, arppu, datetime));
				    }
			    }
		    }
	    }

	    Debug.Log("Information about users has been successfully uploaded.", Debug.Sender.Database, Debug.MessageStatus.INFO);

	    return statistic;
    }
    
    public async Task AddPaymentAsync(string id, long userId, double amount, PaymentStatus status, DateTime dateTime)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = @"INSERT INTO payments (
                id, userid, amount, status, datetime
            ) VALUES(
                $id, $userid, $amount, $status, $datetime
            );";
	    
	    command.Parameters.AddWithValue("id", id);
	    command.Parameters.AddWithValue("userid", userId);
	    command.Parameters.AddWithValue("amount", amount);
	    command.Parameters.AddWithValue("status", (int)status);
	    command.Parameters.AddWithValue("datetime", dateTime);
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task AddUserAsync(long userId)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = @"INSERT INTO users (
                id, date_of_reg
            ) VALUES(
                $id, $date_of_reg
            );";
	    
	    command.Parameters.AddWithValue("id", userId);
	    command.Parameters.AddWithValue("date_of_reg", DateTime.Now);
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task AddRequest(long userId, long? organizationId, int totalTokens)
    {
	    var command = _sqlConnection.CreateCommand();
            
	    command.CommandText = @"INSERT INTO requests (
                user_id, organization_id, total_tokens, datetime
            ) VALUES(
                $user_id, $organization_id, $total_tokens, $datetime
            );";
            
	    command.Parameters.AddWithValue("user_id", userId);
	    command.Parameters.AddWithValue("organization_id", organizationId == null ? DBNull.Value : organizationId);
	    command.Parameters.AddWithValue("total_tokens", totalTokens);
	    command.Parameters.AddWithValue("datetime", DateTime.Now);
	    
	    await command.ExecuteNonQueryAsync();
    }
    
    public async Task AddWhisperRequest(long userId, long? organizationId, int seconds)
    {
	    var command = _sqlConnection.CreateCommand();
            
	    command.CommandText = @"INSERT INTO wrequests (
                user_id, organization_id, seconds, datetime
            ) VALUES(
                $user_id, $organization_id, $seconds, $datetime
            );";
            
	    command.Parameters.AddWithValue("user_id", userId);
	    command.Parameters.AddWithValue("organization_id", organizationId == null ? DBNull.Value : organizationId);
	    command.Parameters.AddWithValue("seconds", seconds);
	    command.Parameters.AddWithValue("datetime", DateTime.Now);
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task AddOrganization(long managerId)
    {
	    var command = _sqlConnection.CreateCommand();
            
	    command.CommandText = @"INSERT INTO organizations (
                manager, date_of_reg
            ) VALUES(
                $manager, $date_of_reg
            );";
            
	    command.Parameters.AddWithValue("manager", managerId);
	    command.Parameters.AddWithValue("date_of_reg", DateTime.Now);

	    await command.ExecuteNonQueryAsync();
    }

    public async Task AddInvite(long organizationId, string uuid)
    {
	    var command = _sqlConnection.CreateCommand();
            
	    command.CommandText = @"INSERT INTO invites (
                organization_id, uuid
            ) VALUES(
                $organization_id, $uuid
            );";
	    
	    command.Parameters.AddWithValue("organization_id", organizationId);
	    command.Parameters.AddWithValue("uuid", uuid);
	    command.Parameters.AddWithValue("datetime", DateTime.Now);
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetOrganizationName(long managerId, string name)
    {
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"UPDATE organizations SET name = '{name}' WHERE manager = {managerId}";

	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetOrganizationType(long managerId, OrganizationType type)
    {
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"UPDATE organizations SET type = {(uint)type} WHERE manager = {managerId}";

	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetOrganizationStage(long managerId, OrganizationStage stage)
    {
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"UPDATE organizations SET stage = {(uint)stage} WHERE manager = {managerId}";

	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetOrganizationRate(long managerId, OrganizationRate rate)
    {
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"UPDATE organizations SET rate = {(uint)rate} WHERE manager = {managerId}";

	    await command.ExecuteNonQueryAsync();
    }
    
    public async Task SetUserFirstname(long userId, string firstname)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET firstname = '{firstname}' WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }
    
    public async Task SetUserLastname(long userId, string lastname)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET lastname = '{lastname}' WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserTelephone(long userId, string telephone)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET telephone = '{telephone}' WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserOrganization(long userId, long? organizationId)
    {
	    var command = _sqlConnection.CreateCommand();
	    var value = (organizationId == null ? "null" : organizationId.ToString())!;
	    command.CommandText = $"UPDATE users SET organization = {value} WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserStage(long userId, UserStage stage)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET stage = {(uint)stage} WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserBalance(long userId, double balance)
    {
	    var command = _sqlConnection.CreateCommand();
	    var balanceStr = balance.ToString("F").Replace(",", ".");
	    command.CommandText = $"UPDATE users SET balance = {balanceStr} WHERE id = {userId}";
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserTrial(long userId, bool trial)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET trial = {trial} WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserEnabled(long userId, bool enabled)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET enabled = {enabled} WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetUserConversation(long userId, string conversation)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE users SET conversation = '{conversation}' WHERE id = {userId}";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task SetPaymentStatus(string id, PaymentStatus status)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"UPDATE payments SET status = '{(int)status}' WHERE id = '{id}'";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveInvite(string uuid)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"DELETE FROM invites WHERE uuid = '{uuid}';";
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task<MountData> GetRequestCountAndTokensFromMountAsync(long organizationId)
    {
	    var command = _sqlConnection.CreateCommand();
	    var datetime = DateTime.Now;
	    var like = $"{datetime:yyy}-{datetime:MM}%";
	    command.CommandText = $"SELECT COUNT(*), SUM(total_tokens) from requests where organization_id = {organizationId} and datetime LIKE '{like}';";

	    var totalCount = 0;
	    var totalTokens = 0;
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    if (!reader.HasRows) return new MountData {TotalCount = totalCount, TotalTokens = totalTokens};
	    while (await reader.ReadAsync())
	    {
		    totalCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    totalTokens = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
	    }

	    return new MountData {TotalCount = totalCount, TotalTokens = totalTokens};
    }
    
    public async Task<MountData> GetWhisperRequestSecondsFromMount(long organizationId)
    {
	    var command = _sqlConnection.CreateCommand();
	    var datetime = DateTime.Now;
	    var like = $"{datetime:yyy}-{datetime:MM}%";
	    var totalSeconds = 0;

	    command.CommandText = $"SELECT SUM(seconds) from wrequests where organization_id = {organizationId} and datetime LIKE '{like}';";

	    await using var reader = await command.ExecuteReaderAsync();
	    if (!reader.HasRows) return new MountData {TotalSeconds = 0};
	    while (await reader.ReadAsync())
	    {
		    totalSeconds = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
	    }

	    return new MountData {TotalSeconds = totalSeconds};
    }

    public async Task AddAction(long userId, ActionType type)
    {
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = @"INSERT INTO actions (
                user_id, type, datetime
            ) VALUES(
                $user_id, $type, $datetime
            );";
	    
	    command.Parameters.AddWithValue("user_id", userId);
	    command.Parameters.AddWithValue("type", (int)type);
	    command.Parameters.AddWithValue("datetime", DateTime.Now);
	    
	    await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteOrganization(long managerId)
    {
	    //DELETE from sqlitedb_developers where id = 6
	    
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $"DELETE from organizations where manager = {managerId}";
	    await command.ExecuteNonQueryAsync();
    }
    
    //SELECT strftime('%m',datetime) AS formatted, COUNT(DISTINCT user_id) FROM requests where datetime like "2023%"

    /*public async Task<Dictionary<string, int>> GetMonthlyActiveUsers(int year)
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"SELECT strftime('%m',datetime) AS month, COUNT(DISTINCT user_id) as count from requests where datetime like '{year}%';";
	    
	    await using var reader = await command.ExecuteReaderAsync();

	    var keyValueList = new Dictionary<string, int>();

	    if (!reader.HasRows) return keyValueList;
	    while (await reader.ReadAsync())
	    {
		    var month = DateTime.Parse($"{1}.{reader.GetInt32(0)}.{year}").ToString("MMMM");
		    var count = reader.GetInt32(1);
			    
		    keyValueList.Add(month, count);
	    }

	    return keyValueList;
    }*/
    
    //SELECT strftime('%d.%m.%Y',datetime) AS date, COUNT(DISTINCT user_id) as count FROM requests WHERE datetime like '2023%' GROUP by date

    /*public async Task<Dictionary<string, int>> GetDailyActiveUsers()
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"SELECT strftime('%d.%m.%Y',datetime) AS date, COUNT(DISTINCT user_id) as count FROM requests WHERE datetime like '{DateTime.Now.Year}%' GROUP by date";
	    
	    await using var reader = await command.ExecuteReaderAsync();

	    var keyValueList = new Dictionary<string, int>();

	    if (!reader.HasRows) return keyValueList;
	    while (await reader.ReadAsync())
	    {
		    var date = reader.GetString(0);
		    var count = reader.GetInt32(1);
			    
		    keyValueList.Add(date, count);
	    }

	    return keyValueList;
    }*/

    public async Task<int> GetDailyActiveUsers(DateTime? date = null)
    {
	    //select count(distinct user_id) from actions where datetime like '2023-03-29%'

	    var fateString = date == null ? "now" : date.Value.ToString("yyyy-MM-dd");

	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"select count(distinct user_id) from actions where datetime > datetime('{fateString}', '-1 day') and datetime < datetime('{fateString}'))";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetMonthlyActiveUsers(DateTime? date = null)
    {
	    var fateString = date == null ? "now" : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();

	    command.CommandText = $"select count(distinct user_id) from actions where datetime > datetime('{fateString}', '-1 month') and datetime < datetime('{fateString}')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetWeeklyActiveUsers(DateTime? date = null)
    {
	    var fateString = date == null ? "now" : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    command.CommandText = $"select count(distinct user_id) from actions where datetime > datetime('{fateString}','-7 day') and datetime < datetime('{fateString}')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetMonthlyNewUsers(DateTime? date = null)
    {
	    //select count(distinct user_id) from actions where type = 24 and  datetime >= datetime('now', '-1 month')
	    
	    var fateString = date == null ? "now" : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(distinct user_id) from actions where type = 24 and  datetime > datetime('{fateString}', '-1 month')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }
    
    public async Task<int> GetMonthlyLeaveUsers(DateTime? date = null)
    {
	    //select count(distinct user_id) from actions where type = 24 and  datetime >= datetime('now', '-1 month')
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(distinct user_id) from actions where type = 30 and datetime > datetime('{fateString}', '-1 month')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }
    
    public async Task<int> GetMonthlyReturnUsers(DateTime? date = null)
    {
	    //select count(distinct user_id) from actions where type = 24 and  datetime >= datetime('now', '-1 month')
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(distinct user_id) from actions where type = 31 and datetime > datetime('{fateString}', '-1 month')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetTrialUser()
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(id) from users where trial = 1";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetConversUser()
    {
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(id) from users where trial = 0 and conversation != '[]'";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetTotalDailyActiveUsers(DateTime? date = null)
    {
	    //select count(distinct user_id) from actions where datetime > datetime('now','-1 day') /*Общее количество активных пользователей за день*/
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(distinct user_id) from actions where datetime > datetime('{fateString}','-1 day')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetTotalCountActionsPerDay(DateTime? date = null)
    {
	    //select count(datetime) from actions where datetime > datetime('now','-1 day')
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(datetime) from actions where datetime > datetime('{fateString}','-1 day')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<double> GetMonthlyProfit(DateTime? date = null)
    {
	    //select sum(amount) from payments where status = 3 and datetime > datetime('now', '-1 month')
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select sum(amount) from payments where status = 3 and datetime > datetime('{fateString}', '-1 month')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
		    return count;
	    }

	    return 0;
    }

    public async Task<int> GetPayingUsersPerMonth(DateTime? date = null)
    {
	    //select count(distinct userid) from payments where status = 3 and datetime > datetime('now', '-1 month')
	    
	    var fateString = date == null ? DateTime.Now.ToString("yyyy-MM-dd") : date.Value.ToString("yyyy-MM-dd");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = $"select count(distinct userid) from payments where status = 3 and datetime > datetime('{fateString}', '-1 month')";
	    
	    await using var reader = await command.ExecuteReaderAsync();
	    
	    if (!reader.HasRows) return 0;
	    while (await reader.ReadAsync())
	    {
		    var count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
		    return count;
	    }

	    return 0;
    }

    public async Task AddRecordToStatistics(int dau, int wau, int mau, int mnu, double utilisation, double frequency, double churnRate, double arpu, double arppu)
    {
	    var utilisationStr = utilisation.ToString("F").Replace(",", ".");
	    var frequencyStr = frequency.ToString("F").Replace(",", ".");
	    var churnRateStr = churnRate.ToString("F").Replace(",", ".");
	    var arpuStr = arpu.ToString("F").Replace(",", ".");
	    var arppuStr = arppu.ToString("F").Replace(",", ".");
	    
	    var command = _sqlConnection.CreateCommand();
	    
	    command.CommandText = @"INSERT INTO statistics (
                dau, wau, mau, mnu, utilisation, frequency, churn_rate, arpu, arppu, datetime
            ) VALUES(
                $dau, $wau, $mau, $mnu, $utilisation, $frequency, $churn_rate, $arpu, $arppu, $datetime
            );";
	    
	    command.Parameters.AddWithValue("dau", dau);
	    command.Parameters.AddWithValue("wau", wau);
	    command.Parameters.AddWithValue("mau", mau);
	    command.Parameters.AddWithValue("mnu", mnu);
	    command.Parameters.AddWithValue("utilisation", utilisationStr);
	    command.Parameters.AddWithValue("frequency", frequencyStr);
	    command.Parameters.AddWithValue("churn_rate", churnRateStr);
	    command.Parameters.AddWithValue("arpu", arpuStr);
	    command.Parameters.AddWithValue("arppu", arppuStr);
	    command.Parameters.AddWithValue("datetime", (DateTime.Now - TimeSpan.FromDays(1)).ToString("yyy-MM-ddd"));
	    
	    await command.ExecuteNonQueryAsync();
    }

    public Task Initialization { get; }
}