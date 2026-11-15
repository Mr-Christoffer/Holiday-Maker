
DROP DATABASE IF EXISTS holiday_maker;

CREATE DATABASE holiday_maker
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE holiday_maker;

CREATE TABLE cities (
    id INTEGER PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE accommodations (
    id INTEGER PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(200) NOT NULL,
    city_id INTEGER NOT NULL,
    has_pool BOOLEAN NOT NULL,
    has_restaurant BOOLEAN NOT NULL,
    distance_to_beach INTEGER NOT NULL,
    distance_to_center INTEGER NOT NULL,
    rating INTEGER NOT NULL,
    UNIQUE (name, city_id),
    CHECK (distance_to_beach >= 0),
    CHECK (distance_to_center >= 0),
    CHECK (rating BETWEEN 1 AND 5),
    FOREIGN KEY (city_id) REFERENCES cities(id)
);

CREATE TABLE rooms (
    id INTEGER PRIMARY KEY AUTO_INCREMENT,
    accommodation_id INTEGER NOT NULL,
    name VARCHAR(50) NOT NULL,
    max_capacity INTEGER NOT NULL,
    price_per_night DECIMAL(10, 2) NOT NULL,
    UNIQUE (accommodation_id, name),
    CHECK (max_capacity > 0),
    CHECK (price_per_night >= 0),
    FOREIGN KEY (accommodation_id) REFERENCES accommodations(id)
);

CREATE TABLE customers (
    id INTEGER PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(200) NOT NULL,
    phone VARCHAR(30) NOT NULL UNIQUE,
    email VARCHAR(200) UNIQUE
);

CREATE TABLE bookings (
    id INTEGER PRIMARY KEY AUTO_INCREMENT,
    customer_id INTEGER NOT NULL,
    room_id INTEGER NOT NULL,
    booking_start DATE NOT NULL,
    booking_end DATE NOT NULL,
    guest_count INTEGER NOT NULL,
    price_per_night DECIMAL(10, 2) NOT NULL,
    CHECK (booking_end > booking_start),
    CHECK (guest_count > 0),
    CHECK (price_per_night >= 0),
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);
