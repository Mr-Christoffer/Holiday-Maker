-- Search availeble rooms
SET
    @city = 'Malmö',
    @checkIn = '2026-12-16',
    @checkOut = '2026-12-18',
    @guests = 1,
    @needPool = FALSE,
    @needRestaurant = FALSE,
    @maxBeach = NULL,
    @maxCenter = NULL,
    @sortBy = 'rating';

SELECT a.name AS boende, c.name AS ort, r.name AS rum, r.max_capacity, r.price_per_night, a.rating
FROM
    rooms r
    JOIN accommodations a ON a.id = r.accommodation_id
    JOIN cities c ON c.id = a.city_id
WHERE
    c.name = @city
    AND r.max_capacity >= @guests
    AND NOT EXISTS (
        SELECT 1
        FROM bookings b
        WHERE
            b.room_id = r.id
            AND b.booking_start < @checkOut
            AND b.booking_end > @checkIn
    )
    AND (
        @needPool = FALSE
        OR a.has_pool = TRUE
    )
    AND (
        @needRestaurant = FALSE
        OR a.has_restaurant = TRUE
    )
    AND (
        @maxBeach IS NULL
        OR a.distance_to_beach <= @maxBeach
    )
    AND (
        @maxCenter IS NULL
        OR a.distance_to_center <= @maxCenter
    )
ORDER BY
    CASE
        WHEN @sortBy = 'price' THEN r.price_per_night
    END ASC,
    CASE
        WHEN @sortBy = 'rating' THEN a.rating
    END DESC,
    r.price_per_night ASC;