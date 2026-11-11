public sealed class Customer
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Phone { get; init; } = "";
    public string? Email { get; init; }          // null = no email
}

public enum SortOrder { Price, Rating }

public sealed class SearchCriteria
{
    public string City { get; init; } = "";
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public int Guests { get; init; }
    public bool NeedPool { get; init; }
    public bool NeedRestaurant { get; init; }
    public int? MaxBeach { get; init; }          // null = no filter
    public int? MaxCenter { get; init; }         // null = no filter
    public SortOrder SortBy { get; init; }
}

public sealed class AvailableRoom
{
    public int RoomId { get; init; }
    public string Accommodation { get; init; } = "";
    public string City { get; init; } = "";
    public string Room { get; init; } = "";
    public int MaxCapacity { get; init; }
    public decimal PricePerNight { get; init; }
    public int Rating { get; init; }
}

public sealed class Booking
{
    public int Id { get; init; }
    public string Accommodation { get; init; } = "";
    public string Room { get; init; } = "";
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public int Guests { get; init; }
    public decimal TotalPrice { get; init; }
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;
}