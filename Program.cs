bool run = true;
while (run)
{
    System.Console.WriteLine("Holiday Maker");
    System.Console.WriteLine("-------------\nMENU");
    System.Console.WriteLine("1. Sök lediga rum");
    System.Console.WriteLine("2. Välj kund");
    System.Console.WriteLine("3. Registrera ny kund");
    System.Console.WriteLine("0. Avsluta");

    int opChoice = Input.ReadInt("Välj: ", 0, 3); // Operator has to choose a number betweeen 0 and 3

    switch (opChoice)
    {
        case 1:
            System.Console.WriteLine("Sök lediga rum");
            break;
        case 2: 
            Customer? customer = await SelectCustomerAsync();
            if (customer is not null)
                Console.WriteLine($"Vald kund: {customer.Name}");
            break;
        case 3:
            System.Console.WriteLine("Registrera ny kund");
            break;
        case 0:
            System.Console.WriteLine("Programmet avslutas");
            run = false;
            break;
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