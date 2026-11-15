USE holiday_maker;

INSERT INTO cities (id, name)
VALUES
    (1, 'Malmö'),
    (2, 'Ystad'),
    (3, 'Båstad');

INSERT INTO accommodations
    (id, name, city_id, has_pool, has_restaurant, distance_to_beach, distance_to_center, rating)
VALUES
    (1, 'Hotell Havsutsikt',       1, TRUE,  TRUE,   150, 1200, 5),
    (2, 'Pensionat Lillgatan',     1, FALSE, FALSE, 2500,  300, 3),
    (3, 'Strandhotellet Ribban',   1, FALSE, TRUE,    80, 2800, 4),
    (4, 'Ystads Strandpensionat',  2, FALSE, FALSE,  300,  900, 2),
    (5, 'Hotell Sandskogen',       2, TRUE,  TRUE,   600, 1500, 4),
    (6, 'Hotell Bjärehus',         3, TRUE,  FALSE, 1200,  400, 3);

INSERT INTO rooms (id, accommodation_id, name, max_capacity, price_per_night)
VALUES
    ( 1, 1, '101', 2, 1450.00),
    ( 2, 1, '102', 2, 1450.00),
    ( 3, 1, '103', 4, 2400.00),
    ( 4, 1, '201', 1,  990.00),
    ( 5, 2, '1',   1,  550.00),
    ( 6, 2, '2',   2,  750.00),
    ( 7, 2, '3',   2,  790.00),
    ( 8, 3, '11',  2, 1250.00),
    ( 9, 3, '12',  4, 1950.00),
    (10, 3, '13',  6, 2900.00),
    (11, 4, 'A',   1,  650.00),
    (12, 4, 'B',   2,  850.00),
    (13, 4, 'C',   4, 1300.00),
    (14, 5, '101', 2, 1100.00),
    (15, 5, '102', 2, 1150.00),
    (16, 5, '103', 4, 1800.00),
    (17, 5, '104', 1,  800.00),
    (18, 6, '1',   2,  950.00),
    (19, 6, '2',   4, 1500.00),
    (20, 6, '3',   2,  990.00),
    (21, 6, '4',   1,  600.00);

INSERT INTO customers (id, name, phone, email)
VALUES
    (1, 'Anna Svensson',  '070-111 11 11', 'anna.svensson@example.com'),
    (2, 'Erik Lindqvist', '070-222 22 22', 'erik.lindqvist@example.com'),
    (3, 'Fatima Hassan',  '070-333 33 33', 'fatima.hassan@example.com'),
    (4, 'Jonas Berg',     '070-444 44 44', NULL),
    (5, 'Maria Olsson',   '070-555 55 55', 'maria.olsson@example.com');

INSERT INTO bookings (id, customer_id, room_id, booking_start, booking_end, guest_count, price_per_night)
VALUES
    (1, 1, 1, '2026-12-10', '2026-12-13', 2, 1450.00),
    (2, 2, 1, '2026-12-13', '2026-12-15', 2, 1450.00),
    (3, 3, 2, '2026-12-10', '2026-12-13', 1, 1450.00),
    (4, 4, 3, '2026-12-05', '2026-12-20', 4, 2400.00),
    (5, 5, 4, '2026-12-01', '2026-12-04', 1,  990.00),
    (6, 1, 5, '2026-12-18', '2026-12-22', 1,  550.00),
    (7, 2, 6, '2026-12-02', '2026-12-09', 2,  750.00),
    (8, 4, 6, '2026-12-19', '2026-12-23', 1,  750.00),
    (9, 3, 7, '2026-12-24', '2026-12-27', 2,  790.00),
    (10, 5, 7, '2026-12-05', '2026-12-08', 2,  790.00),
    (11, 4, 8, '2026-12-12', '2026-12-14', 2, 1250.00),
    (12, 1, 9, '2026-12-01', '2026-12-04', 4, 1950.00),
    (13, 5, 9, '2026-12-27', '2026-12-31', 3, 1950.00),
    (14, 1, 10, '2026-12-06', '2026-12-08', 5, 2900.00),
    (15, 2, 11, '2026-12-15', '2026-12-16', 1,  650.00),
    (16, 3, 12, '2026-12-01', '2026-12-07', 2,  850.00),
    (17, 4, 13, '2026-12-20', '2026-12-26', 4, 1300.00),
    (18, 5, 14, '2026-12-03', '2026-12-06', 2, 1100.00),
    (19, 1, 14, '2026-12-06', '2026-12-10', 2, 1100.00),
    (20, 2, 15, '2026-12-22', '2026-12-29', 2, 1150.00),
    (21, 3, 16, '2026-12-11', '2026-12-18', 3, 1800.00),
    (22, 4, 17, '2026-12-09', '2026-12-10', 1,  800.00),
    (23, 5, 18, '2026-12-14', '2026-12-21', 2,  950.00),
    (24, 1, 19, '2026-12-20', '2026-12-27', 4, 1500.00),
    (25, 2, 20, '2026-12-01', '2026-12-03', 2,  990.00),
    (26, 3, 21, '2026-12-28', '2026-12-31', 1,  600.00);

SELECT a.id AS bokning_a, b.id AS bokning_b, a.room_id
FROM bookings AS a
INNER JOIN bookings AS b
    ON a.room_id = b.room_id
   AND a.id < b.id
   AND a.booking_start < b.booking_end
   AND a.booking_end > b.booking_start;

SELECT bk.id, bk.guest_count, r.max_capacity
FROM bookings AS bk
INNER JOIN rooms AS r
    ON r.id = bk.room_id
WHERE bk.guest_count > r.max_capacity;
