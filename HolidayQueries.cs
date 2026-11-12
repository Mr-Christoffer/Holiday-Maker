using MySqlConnector;

public static class HolidayQueries
{
    // Search for customer with name or phonenumber
    public static async Task<List<Customer>> SearchCustomersAsync(string search)
    {
        const string sql = """
            SELECT id, name, phone, email
            FROM customers
            WHERE name LIKE CONCAT('%', @search, '%')
               OR phone LIKE CONCAT('%', @search, '%')
            ORDER BY name, id;
            """;

        await using var conn = await Database.OpenAsync(); // Connects to database, closes after method is done.
        await using var cmd = new MySqlCommand(sql, conn); 
        cmd.Parameters.AddWithValue("@search", search); // @search instead of raw string, works as protection againnst SQL-injections.

        var customers = new List<Customer>();
        await using var reader = await cmd.ExecuteReaderAsync();  

        while (await reader.ReadAsync())
        {
            customers.Add(new Customer
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Phone = reader.GetString("phone"),
                Email = reader.IsDBNull(reader.GetOrdinal("email"))
                    ? null
                    : reader.GetString("email")
            });
        }

        return customers;
    }
    // Create a new customer, returns ID if new customer was created, otherwise NULL
    public static async Task<int?> AddCustomerAsync(string name, string phone, string? email)
    {
        const string sql = """
            INSERT INTO customers (name, phone, email)
            VALUES (@name, @phone, @email);
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@phone", phone);
        cmd.Parameters.AddWithValue("@email", (object?)email ?? DBNull.Value);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            // In case customer info already exists.
            return null;
        }
    }

    // Search for availeble rooms

    public static async Task<List<AvailableRoom>> SearchRoomsAsync(SearchCriteria criteria)
    {
        const string sql = """
            SELECT
                r.id AS room_id,
                a.name AS accommodation,
                c.name AS city,
                r.name AS room,
                r.max_capacity,
                r.price_per_night,
                a.rating
            FROM rooms AS r
            INNER JOIN accommodations AS a
                ON a.id = r.accommodation_id
            INNER JOIN cities AS c
                ON c.id = a.city_id
            WHERE c.name = @city
              AND r.max_capacity >= @guests
              AND NOT EXISTS (
                  SELECT 1
                  FROM bookings AS b
                  WHERE b.room_id = r.id
                    AND b.booking_start < @checkOut
                    AND b.booking_end > @checkIn
              )
              AND (@needPool = FALSE OR a.has_pool = TRUE)
              AND (@needRestaurant = FALSE OR a.has_restaurant = TRUE)
              AND (@maxBeach IS NULL OR a.distance_to_beach <= @maxBeach)
              AND (@maxCenter IS NULL OR a.distance_to_center <= @maxCenter)
            ORDER BY
                CASE WHEN @sortBy = 'price' THEN r.price_per_night END ASC,
                CASE WHEN @sortBy = 'rating' THEN a.rating END DESC,
                r.price_per_night ASC,
                r.id ASC;
            """;

        string sortBy = criteria.SortBy == SortOrder.Price ? "price" : "rating";

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@city", criteria.City);
        cmd.Parameters.AddWithValue("@checkIn", criteria.CheckIn);
        cmd.Parameters.AddWithValue("@checkOut", criteria.CheckOut);
        cmd.Parameters.AddWithValue("@guests", criteria.Guests);
        cmd.Parameters.AddWithValue("@needPool", criteria.NeedPool);
        cmd.Parameters.AddWithValue("@needRestaurant", criteria.NeedRestaurant);
        cmd.Parameters.AddWithValue("@maxBeach", (object?)criteria.MaxBeach ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@maxCenter", (object?)criteria.MaxCenter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sortBy", sortBy);

        var rooms = new List<AvailableRoom>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rooms.Add(new AvailableRoom
            {
                RoomId = reader.GetInt32("room_id"),
                Accommodation = reader.GetString("accommodation"),
                City = reader.GetString("city"),
                Room = reader.GetString("room"),
                MaxCapacity = reader.GetInt32("max_capacity"),
                PricePerNight = reader.GetDecimal("price_per_night"),
                Rating = reader.GetInt32("rating")
            });
        }

        return rooms;
    }

    // Bookings
    // Creates a booking and returns bookingnumber (id)
    public static async Task<int?> CreateBookingAsync(
        int customerId, int roomId, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        const string sql = """
            INSERT INTO bookings
                (customer_id, room_id, booking_start, booking_end, guest_count, price_per_night)
            SELECT @customerId, r.id, @checkIn, @checkOut, @guests, r.price_per_night
            FROM rooms AS r
            WHERE r.id = @roomId
              AND r.max_capacity >= @guests
              AND NOT EXISTS (
                  SELECT 1
                  FROM bookings AS b
                  WHERE b.room_id = r.id
                    AND b.booking_start < @checkOut
                    AND b.booking_end > @checkIn
              );
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", customerId);
        cmd.Parameters.AddWithValue("@roomId", roomId);
        cmd.Parameters.AddWithValue("@checkIn", checkIn);
        cmd.Parameters.AddWithValue("@checkOut", checkOut);
        cmd.Parameters.AddWithValue("@guests", guests);

        int affectedRows = await cmd.ExecuteNonQueryAsync();

        if (affectedRows == 0)
            return null;

        return (int)cmd.LastInsertedId;
    }

    private const string BookingSelect = """
        SELECT
            b.id AS booking_id,
            a.name AS accommodation,
            r.name AS room,
            b.booking_start,
            b.booking_end,
            b.guest_count,
            DATEDIFF(b.booking_end, b.booking_start) * b.price_per_night AS total_price
        FROM bookings AS b
        INNER JOIN rooms AS r
            ON r.id = b.room_id
        INNER JOIN accommodations AS a
            ON a.id = r.accommodation_id
        """;

    public static async Task<Booking?> GetBookingAsync(int bookingId)
    {
        string sql = BookingSelect + """
 
            WHERE b.id = @bookingId;
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return ReadBooking(reader);

        return null;
    }

    // All bookings a specific customer has
    public static async Task<List<Booking>> GetBookingsAsync(int customerId)
    {
        string sql = BookingSelect + """
 
            WHERE b.customer_id = @customerId
            ORDER BY b.booking_start, b.id;
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", customerId);

        var bookings = new List<Booking>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            bookings.Add(ReadBooking(reader));

        return bookings;
    }

    private static Booking ReadBooking(MySqlDataReader reader)
    {
        return new Booking
        {
            Id = reader.GetInt32("booking_id"),
            Accommodation = reader.GetString("accommodation"),
            Room = reader.GetString("room"),
            CheckIn = reader.GetDateOnly("booking_start"),
            CheckOut = reader.GetDateOnly("booking_end"),
            Guests = reader.GetInt32("guest_count"),
            TotalPrice = reader.GetDecimal("total_price")
        };
    }

    // Change amount of persons in a booking. Returns false if not possible
    public static async Task<bool> ChangeGuestsAsync(int customerId, int bookingId, int newGuests)
    {
        const string sql = """
            UPDATE bookings AS b
            INNER JOIN rooms AS r
                ON r.id = b.room_id
            SET b.guest_count = @newGuests
            WHERE b.id = @bookingId
              AND b.customer_id = @customerId
              AND r.max_capacity >= @newGuests;
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", customerId);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);
        cmd.Parameters.AddWithValue("@newGuests", newGuests);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    // Cancel a booking. Returns false if not possible
    public static async Task<bool> CancelBookingAsync(int customerId, int bookingId)
    {
        const string sql = """
            DELETE FROM bookings
            WHERE id = @bookingId
              AND customer_id = @customerId;
            """;

        await using var conn = await Database.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", customerId);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
}