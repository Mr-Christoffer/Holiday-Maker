bool run = true;
while (run)
{
    Console.Clear();
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
            {
                Customer? customer = await SelectCustomerAsync();
                if (customer is not null)
                    await CustomerMenuAsync(customer);
                else
                    Input.Pause();
                break;
            }
        case 3:
            System.Console.WriteLine("Registrera ny kund");
            break;
        case 0:
            System.Console.WriteLine("Programmet avslutas");
            run = false;
            break;
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

        switch (choice)
        {
            case 1:
                Console.WriteLine("Visa bokningar – kommer snart");
                break;
            case 2:
                Console.WriteLine("Sök och boka – kommer snart");
                break;
            case 3:
                Console.WriteLine("Ändra antal personer – kommer snart");
                break;
            case 4:
                Console.WriteLine("Avboka – kommer snart");
                break;
            case 0:
                inCustomerMenu = false;
                break;
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

