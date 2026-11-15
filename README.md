# Holiday Maker

A C# console application for booking rooms from a database.

## Requirements

- .NET SDK 10
- MySQL 8.0.16 or later, for example in Docker

## 1. Create the database

Start MySQL, for example with Docker:

```bash
docker run --name holiday-mysql -e MYSQL_ROOT_PASSWORD=<root-password> -p 3306:3306 -d mysql:8.4
```

Then run the three scripts in the `sql` folder **as root, in this order**:

| Script | What it does |
| --- | --- |
| `holiday_maker_schema.sql` | Creates the `holiday_maker` database and its tables. Can be rerun to reset the database. |
| `holiday_maker_app_user.sql` | Creates the `holiday_app` account the application uses. **Replace `byt-till-eget-losenord` with your own password first.** |
| `holiday_maker_testdata.sql` | Inserts the test data. |

From the command line:

```bash
mysql -u root -p < sql/holiday_maker_schema.sql
mysql -u root -p < sql/holiday_maker_app_user.sql
mysql -u root -p --default-character-set=utf8mb4 < sql/holiday_maker_testdata.sql
```

You can also open and run the scripts in a database client such as DataGrip.

## 2. Run the application

The connection string is read from the environment variable `HOLIDAY_MAKER_DB`, so no password is stored in the code. Use the password you chose for `holiday_app`.

**Linux and macOS:**

```bash
export HOLIDAY_MAKER_DB="Server=localhost;Port=3306;Database=holiday_maker;User ID=holiday_app;Password=<password>;"
dotnet run
```

**Windows PowerShell:**

```powershell
$env:HOLIDAY_MAKER_DB="Server=localhost;Port=3306;Database=holiday_maker;User ID=holiday_app;Password=<password>;"
dotnet run
```

The variable only applies in the terminal where it was set.

## Troubleshooting

| Message | Cause |
| --- | --- |
| `Miljövariabeln HOLIDAY_MAKER_DB är inte satt` | The variable has not been set in this terminal |
| `Kunde inte nå databasen` | MySQL is not running, or the port is wrong |
| `Access denied for user 'holiday_app'` | The password does not match the one in `holiday_maker_app_user.sql` |

## Testing

The solution was tested manually in the application against the test data, using the `holiday_app` account. The database was reset with the schema and test data scripts before each round. All tests passed.

| What was checked | How | Result |
| --- | --- | --- |
| An available room is shown and a booked one is filtered out | Malmö 11–12 Dec, 2 guests. Rooms 101 and 102 at Hotell Havsutsikt are booked 10–13 Dec. | 101 and 102 are not shown, the other free rooms are |
| A room can be booked on the day the previous guest checks out | Malmö 13–14 Dec, 2 guests. Room 102 is booked 10–13 Dec. | Room 102 is shown and can be booked |
| Overlapping bookings are rejected, also when one period encloses another | Malmö 8–10 Dec, 4 guests (room 103 is booked 5–20 Dec). Booking the same room and period twice. | Room 103 is not shown; the second booking is rejected |
| A party that is too large cannot book | Ystad, 5 guests (largest room holds 4) | No results |
| Combined filters and both sort orders | Malmö 16–18 Dec with pool + restaurant, max distance to beach and to centre, sorted by price and by rating | Only matching accommodations; lowest price first, highest rating first |
| Changing the number of guests respects room capacity | Booking 1 (room for 2) changed to 3, then to 1 | 3 is rejected, 1 is accepted |
| Cancelling frees the room | Cancel booking 1 (room 101, 10–13 Dec), then search the same period | Room 101 is shown again and can be booked |
| Data persists after a restart | Book, exit, start the application again and list the customer's bookings | The booking is still there |