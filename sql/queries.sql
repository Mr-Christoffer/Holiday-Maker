
SET @city = 'Malmö',
    @checkIn = '2026-12-16',
    @checkOut = '2026-12-18',
    @guests = 1,
    @needPool = FALSE,
    @needRestaurant = FALSE,
    @maxBeach = NULL,
    @maxCenter = NULL,
    @sortBy = 'rating';

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

SET @name = 'Test Testsson', @phone = '070-999 99 99', @email = NULL;

INSERT INTO customers (name, phone, email)
VALUES (@name, @phone, @email);

SELECT LAST_INSERT_ID();

SET @search = 'Olsson';

SELECT id, name, phone, email
FROM customers
WHERE name LIKE CONCAT('%', @search, '%')
   OR phone LIKE CONCAT('%', @search, '%')
ORDER BY name, id;

SET @customerId = 1, @roomId = 2,
    @checkIn = '2026-12-13', @checkOut = '2026-12-15', @guests = 2;

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

SELECT ROW_COUNT();

SET @bookingId = LAST_INSERT_ID();

SELECT
    b.id AS booking_id,
    a.name AS accommodation,
    r.name AS room,
    b.booking_start,
    b.booking_end,
    b.guest_count,
    DATEDIFF(b.booking_end, b.booking_start) AS nights,
    DATEDIFF(b.booking_end, b.booking_start) * b.price_per_night AS total_price
FROM bookings AS b
INNER JOIN rooms AS r
    ON r.id = b.room_id
INNER JOIN accommodations AS a
    ON a.id = r.accommodation_id
WHERE b.id = @bookingId;

SET @customerId = 1;

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
WHERE b.customer_id = @customerId
ORDER BY b.booking_start, b.id;

SET @customerId = 1, @bookingId = 1, @newGuests = 1;

UPDATE bookings AS b
INNER JOIN rooms AS r
    ON r.id = b.room_id
SET b.guest_count = @newGuests
WHERE b.id = @bookingId
  AND b.customer_id = @customerId
  AND r.max_capacity >= @newGuests;

SELECT ROW_COUNT();

SET @customerId = 1, @bookingId = 1;

DELETE FROM bookings
WHERE id = @bookingId
  AND customer_id = @customerId;

SELECT ROW_COUNT();
