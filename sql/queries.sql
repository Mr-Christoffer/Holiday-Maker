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

-- Register customer
SET @name = 'Test Testsson', @telnr = '070-999 99 99', @email = NULL;

INSERT INTO
    customers (name, telnr, email)
VALUES (@name, @telnr, @email);

SELECT LAST_INSERT_ID();

-- Search customer
SET @search = 'Olsson';

SELECT id, name, telnr, email
FROM customers
WHERE
    name LIKE CONCAT('%', @search, '%')
    OR telnr LIKE CONCAT('%', @search, '%')
ORDER BY name;

-- Create a booking
SET @customerId = 1, @roomId = 2,
    @checkIn = '2026-12-13', @checkOut = '2026-12-15', @guests = 2;

INSERT INTO bookings (customer_id, room_id, booking_start, booking_end, guest_count)
SELECT @customerId, r.id, @checkIn, @checkOut, @guests
FROM rooms r
WHERE r.id = @roomId
  AND r.max_capacity >= @guests
  AND NOT EXISTS (
    SELECT 1 FROM bookings b
    WHERE b.room_id = r.id
      AND b.booking_start < @checkOut
      AND b.booking_end   > @checkIn
);

SELECT ROW_COUNT();

-- Booking confirmation
SELECT b.id AS bokningsnr, a.name AS boende, r.name AS rum,
       b.booking_start, b.booking_end, b.guest_count,
       DATEDIFF(b.booking_end, b.booking_start) AS natter,
       DATEDIFF(b.booking_end, b.booking_start) * r.price_per_night AS totalpris
FROM bookings b
         JOIN rooms r          ON r.id = b.room_id
         JOIN accommodations a ON a.id = r.accommodation_id
WHERE b.id = LAST_INSERT_ID();

-- Show a customers bookings
SET @customerId = 1;

SELECT b.id AS bokningsnr, a.name AS boende, r.name AS rum,
       b.booking_start AS incheckning, b.booking_end AS utcheckning,
       b.guest_count AS personer,
       DATEDIFF(b.booking_end, b.booking_start) * r.price_per_night AS totalpris
FROM bookings b
         JOIN rooms r          ON r.id = b.room_id
         JOIN accommodations a ON a.id = r.accommodation_id
WHERE b.customer_id = @customerId
ORDER BY b.booking_start;

-- Change amount of persons
SET @customerId = 1, @bookingId = 1, @newGuests = 1;

UPDATE bookings b
    JOIN rooms r ON r.id = b.room_id
SET b.guest_count = @newGuests
WHERE b.id = @bookingId
  AND b.customer_id = @customerId
  AND r.max_capacity >= @newGuests;

SELECT ROW_COUNT();   

-- Cancel booking
SET @customerId = 1, @bookingId = 1;

DELETE FROM bookings
WHERE id = @bookingId
  AND customer_id = @customerId;

SELECT ROW_COUNT();   