using MySqlConnector;

bool run = true;
while (run)
{
    Console.Clear();
    System.Console.WriteLine("Holiday Maker");
    System.Console.WriteLine("-------------\nMENY");
    System.Console.WriteLine("1. Sök lediga rum");
    System.Console.WriteLine("2. Välj kund");
    System.Console.WriteLine("3. Registrera ny kund");
    System.Console.WriteLine("0. Avsluta");

    int opChoice = Input.ReadInt("Välj: ", 0, 3); // Operator has to choose a number betweeen 0 and 3

    try
    {
        switch (opChoice)
        {
            case 1:
                await SearchAndBookAsync(null);
                Input.Pause();
                break;
            case 2:
                {
                    Customer? customer = await SelectCustomerAsync();
                    if (customer is not null)
                        await CustomerMenuAsync(customer);
                    else
                        Input.Pause();
                    break;
                }
            case 3:
                {
                    Customer? customer = await RegisterCustomerAsync();
                    if (customer is not null)
                        await CustomerMenuAsync(customer);
                    else
                        Input.Pause();
                    break;
                }
            case 0:
                System.Console.WriteLine("Programmet avslutas");
                run = false;
                break;
        }
    }
    catch (MySqlException)
    {
        Console.WriteLine("Kunde inte nå databasen. Kontrollera att MySQL är igång.");
        Input.Pause();
    }
}

async Task CustomerMenuAsync(Customer customer)
{
    bool inCustomerMenu = true;

    while (inCustomerMenu)
    {
        Console.Clear();
        System.Console.WriteLine($"=== {customer.Name} ({customer.Phone}) ===");
        System.Console.WriteLine("1. Visa bokningar");
        System.Console.WriteLine("2. Sök lediga rum och boka");
        System.Console.WriteLine("3. Ändra antal personer");
        System.Console.WriteLine("4. Avboka");
        System.Console.WriteLine("0. Tillbaka till huvudmenyn");

        int choice = Input.ReadInt("Välj: ", 0, 4);
        try
        {
            switch (choice)
            {
                case 1:
                    await ShowBookingsAsync(customer);
                    Input.Pause();
                    break;
                case 2:
                    await SearchAndBookAsync(customer);
                    Input.Pause();
                    break;
                case 3:
                    await ChangeGuestCountAsync(customer);
                    Input.Pause();
                    break;
                case 4:
                    await CancelSelectedBookingAsync(customer);
                    Input.Pause();
                    break;
                case 0:
                    inCustomerMenu = false;
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("Kunde inte nå databasen. Kontrollera att MySQL är igång.");
            Input.Pause();
        }
    }
}

async Task<Customer?> SelectCustomerAsync()
{
    // Search customer
    string? search = Input.ReadOptionalString("Sök kund (namn eller telefon, tomt = alla): ");

    // Fills customer list
    List<Customer> customers = await HolidayQueries.SearchCustomersAsync(search ?? "");

    // No matches
    if (customers.Count == 0)
    {
        Console.WriteLine("Inga kunder matchade sökningen.");
        return null;
    }

    // Show results
    Console.WriteLine();
    foreach (Customer customer in customers)
    {
        Console.WriteLine($"{customer.Id}. {customer.Name}, {customer.Phone}");
    }

    // Select ID
    int id = Input.ReadInt("Ange kundens id: ", 1, int.MaxValue);

    // Find customer
    Customer? chosen = customers.Find(c => c.Id == id);

    if (chosen is null)
    {
        Console.WriteLine("Det id:t fanns inte bland sökträffarna.");
        return null;
    }

    return chosen;
}

// List the customer's bookings. Returns the list.
async Task<List<Booking>> ShowBookingsAsync(Customer customer)
{
    List<Booking> bookings = await HolidayQueries.GetBookingsAsync(customer.Id);

    if (bookings.Count == 0)
    {
        Console.WriteLine($"{customer.Name} har inga bokningar.");
        return bookings;
    }

    Console.WriteLine($"Bokningar för {customer.Name}:");
    Console.WriteLine();

    foreach (Booking booking in bookings)
    {
        Console.WriteLine($"Bokning {booking.Id}: {booking.Accommodation}, rum {booking.Room}");
        Console.WriteLine($"   {booking.CheckIn:yyyy-MM-dd} – {booking.CheckOut:yyyy-MM-dd} " +
                          $"({booking.Nights} nätter), {booking.Guests} personer");
        Console.WriteLine($"   Totalpris: {booking.TotalPrice:0.00} kr");
    }

    return bookings;
}

// Cancel booking
async Task CancelSelectedBookingAsync(Customer customer)
{
    List<Booking> bookings = await ShowBookingsAsync(customer);
    if (bookings.Count == 0)
        return;

    Console.WriteLine();
    int bookingId = Input.ReadInt("Ange bokningsnummer att avboka: ", 1, int.MaxValue);
    Booking? booking = bookings.Find(b => b.Id == bookingId);

    // Only allow bookings that exist (to that customer)
    if (booking is null)
    {
        Console.WriteLine("Det bokningsnumret tillhör inte kunden.");
        return;
    }

    // Ask for confirmation
    if (!Input.ReadYesNo($"Avboka bokning {bookingId} ({booking.Accommodation}, rum {booking.Room})?"))
    {
        Console.WriteLine("Ingen avbokning gjordes.");
        return;
    }

    bool cancelled = await HolidayQueries.CancelBookingAsync(customer.Id, bookingId);

    if (cancelled)
        Console.WriteLine($"Bokning {bookingId} är avbokad. Rummet kan nu bokas igen för perioden.");
    else
        Console.WriteLine("Bokningen kunde inte avbokas.");
}

// Search available rooms and optionally book one.
// If no customer is selected, customer == null
async Task SearchAndBookAsync(Customer? customer)
{
    // Search filters
    string city = Input.ReadString("Ort: ");
    var (checkIn, checkOut) = Input.ReadPeriod();
    int guests = Input.ReadInt("Antal personer: ", 1, 20);

    bool needPool = Input.ReadYesNo("Krävs pool?");
    bool needRestaurant = Input.ReadYesNo("Krävs restaurang?");
    int? maxBeach = Input.ReadOptionalInt("Max avstånd till strand i meter (tomt = inget krav): ");
    int? maxCenter = Input.ReadOptionalInt("Max avstånd till centrum i meter (tomt = inget krav): ");

    int sortChoice = Input.ReadInt("Sortera efter 1) pris, lägst först  2) omdöme, högst först: ", 1, 2);
    SortOrder sortBy = sortChoice == 1 ? SortOrder.Price : SortOrder.Rating;

    var criteria = new SearchCriteria
    {
        City = city,
        CheckIn = checkIn,
        CheckOut = checkOut,
        Guests = guests,
        NeedPool = needPool,
        NeedRestaurant = needRestaurant,
        MaxBeach = maxBeach,
        MaxCenter = maxCenter,
        SortBy = sortBy
    };

    // Filtering and sorting are done in SQL
    List<AvailableRoom> rooms = await HolidayQueries.SearchRoomsAsync(criteria);

    if (rooms.Count == 0)
    {
        Console.WriteLine("Inga lediga rum matchar sökningen.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Lediga rum i {city} {checkIn:yyyy-MM-dd} – {checkOut:yyyy-MM-dd}:");
    foreach (AvailableRoom room in rooms)
    {
        Console.WriteLine($"[{room.RoomId}] {room.Accommodation} ({room.City}), rum {room.Room}, " +
                          $"max {room.MaxCapacity} pers, {room.PricePerNight:0.00} kr/natt, " +
                          $"omdöme {room.Rating}/5");
    }

    // Booking
    Console.WriteLine();
    if (!Input.ReadYesNo("Vill du boka ett av rummen?"))
        return;

    int roomId = Input.ReadInt("Ange rum-id (siffran inom [ ]): ", 1, int.MaxValue);
    AvailableRoom? chosenRoom = rooms.Find(r => r.RoomId == roomId);

    // Only allow rooms from the search result
    if (chosenRoom is null)
    {
        Console.WriteLine("Det rum-id:t fanns inte bland sökträffarna.");
        return;
    }

    // Only possible to book if customer is selected.
    if (customer is null)
    {
        int customerChoice = Input.ReadInt("1) Välj befintlig kund  2) Registrera ny kund: ", 1, 2);
        customer = customerChoice == 1
            ? await SelectCustomerAsync()
            : await RegisterCustomerAsync();

        if (customer is null)
        {
            Console.WriteLine("Bokningen avbröts eftersom ingen kund valdes.");
            return;
        }
    }

    // Availebility checks in MySQL
    int? bookingId = await HolidayQueries.CreateBookingAsync(
        customer.Id, chosenRoom.RoomId, checkIn, checkOut, guests);

    if (bookingId is null)
    {
        Console.WriteLine("Rummet kunde inte bokas. Det är inte längre ledigt för perioden.");
        return;
    }

    // Confirmation
    Booking? booking = await HolidayQueries.GetBookingAsync(bookingId.Value);
    if (booking is not null)
    {
        Console.WriteLine();
        Console.WriteLine("Bokningen är klar!");
        Console.WriteLine($"Bokningsnummer: {booking.Id}");
        Console.WriteLine($"Kund:           {customer.Name}");
        Console.WriteLine($"Boende:         {booking.Accommodation}, rum {booking.Room}");
        Console.WriteLine($"Datum:          {booking.CheckIn:yyyy-MM-dd} – {booking.CheckOut:yyyy-MM-dd} ({booking.Nights} nätter)");
        Console.WriteLine($"Antal personer: {booking.Guests}");
        Console.WriteLine($"Totalpris:      {booking.TotalPrice:0.00} kr");
    }

}

// Register new customer
async Task<Customer?> RegisterCustomerAsync()
{
    string name = Input.ReadString("Namn: ");
    string phone = Input.ReadString("Telefon: ");
    string? email = Input.ReadOptionalString("E-post (tomt = ingen): ");

    // null = phone or email already exists.
    int? id = await HolidayQueries.AddCustomerAsync(name, phone, email);

    if (id is null)
    {
        Console.WriteLine("Telefonnumret eller e-postadressen finns redan hos en annan kund.");
        return null;
    }

    Console.WriteLine($"Kunden registrerades med id {id}.");
    return new Customer { Id = id.Value, Name = name, Phone = phone, Email = email };
}

// Change number of guests in a booking
async Task ChangeGuestCountAsync(Customer customer)
{
    List<Booking> bookings = await ShowBookingsAsync(customer);
    if (bookings.Count == 0)
        return;

    Console.WriteLine();
    int bookingId = Input.ReadInt("Ange bokningsnummer: ", 1, int.MaxValue);
    Booking? booking = bookings.Find(b => b.Id == bookingId);

    // Only allow bookings that belong to this customer
    if (booking is null)
    {
        Console.WriteLine("Det bokningsnumret tillhör inte kunden.");
        return;
    }

    int newGuests = Input.ReadInt("Nytt antal personer: ", 1, 20);

    // Check room capacity
    bool changed = await HolidayQueries.ChangeGuestsAsync(customer.Id, bookingId, newGuests);

    if (changed)
        Console.WriteLine($"Bokning {bookingId} gäller nu {newGuests} personer.");
    else
        Console.WriteLine("Det nya antalet ryms inte i rummet. Ingen ändring gjordes.");
}