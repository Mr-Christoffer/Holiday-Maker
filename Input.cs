using System.Globalization;
public static class Input
{
    // Non-optional text. Trim() deletes spaces. 
    // Used for e.g. name and phonenumber when creating new customer or municipality "ort" when searching for availeble rooms.
    public static string ReadString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(text))
                return text;

            Console.WriteLine("Fältet får inte vara tomt.");
        }
    }

    // Optional string. No text = null.
    public static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        string? text = Console.ReadLine()?.Trim();

        return string.IsNullOrEmpty(text) ? null : text;
    }

    // Int within an intervall. E.g. Menu-selection, select costumer.
    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Ange ett heltal mellan {min} och {max}.");
        }
    }

    // Optional int. E.g. distance to beach.
    // Empty answer = null = no filter.
    public static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(text))
                return null;

            if (int.TryParse(text, out int value) && value >= 0)
                return value;

            Console.WriteLine("Ange ett heltal som är 0 eller större, eller lämna tomt.");
        }
    }

    // Date in the format YYYY-MM-DD.
    public static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine()?.Trim();

            if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date)) //CultureInfo... No matter the computers region-settings.
                return date;

            Console.WriteLine("Ange datum som ÅÅÅÅ-MM-DD, till exempel 2026-12-13.");
        }
    }

    // Check-in and check-out
    public static (DateOnly CheckIn, DateOnly CheckOut) ReadPeriod()
    {
        while (true)
        {
            DateOnly checkIn = ReadDate("Incheckning (ÅÅÅÅ-MM-DD): ");
            DateOnly checkOut = ReadDate("Utcheckning (ÅÅÅÅ-MM-DD): ");

            if (checkOut > checkIn)
                return (checkIn, checkOut);

            Console.WriteLine("Utcheckningen måste vara efter incheckningen. Försök igen.");
        }
    }

    // Yes/No - question. E.g. Is pool required?
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (j/n): ");
            string? answer = Console.ReadLine()?.Trim().ToLower();

            if (answer == "j")
                return true;

            if (answer == "n")
                return false;

            Console.WriteLine("Svara j eller n.");
        }
    }
    public static void Pause() // Pause
{
    Console.WriteLine();
    Console.Write("Tryck Enter för att fortsätta...");
    Console.ReadKey();
}
}