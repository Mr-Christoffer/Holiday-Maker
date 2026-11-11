using MySqlConnector;

const string ListCustomersSql = """
    SELECT id, name, telnr, email
    FROM customers
    ORDER BY name;
    """;

await using var conn = await Database.OpenAsync();
await using var cmd = new MySqlCommand(ListCustomersSql, conn);
await using var reader = await cmd.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    int id = reader.GetInt32("id");
    string name = reader.GetString("name");
    string telnr = reader.GetString("telnr");
    string email = reader.IsDBNull(reader.GetOrdinal("email"))
        ? "-"
        : reader.GetString("email");

    Console.WriteLine($"{id}. {name}, {telnr}, {email}");
}