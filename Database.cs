using MySqlConnector;

static class Database
{
    public static async Task<MySqlConnection> OpenAsync()
    {
        string? connectionString = Environment.GetEnvironmentVariable("HOLIDAY_MAKER_DB");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                "Miljövariabeln HOLIDAY_MAKER_DB är inte satt. Se README.");

        var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        return conn;
    }
}