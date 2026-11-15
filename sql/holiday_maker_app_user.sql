
CREATE USER IF NOT EXISTS 'holiday_app'@'%'
IDENTIFIED BY 'byt-till-eget-losenord';

REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'holiday_app'@'%';

GRANT SELECT ON holiday_maker.cities         TO 'holiday_app'@'%';
GRANT SELECT ON holiday_maker.accommodations TO 'holiday_app'@'%';
GRANT SELECT ON holiday_maker.rooms          TO 'holiday_app'@'%';

GRANT SELECT, INSERT ON holiday_maker.customers TO 'holiday_app'@'%';

GRANT SELECT, INSERT, DELETE ON holiday_maker.bookings TO 'holiday_app'@'%';

GRANT UPDATE (guest_count) ON holiday_maker.bookings TO 'holiday_app'@'%';
