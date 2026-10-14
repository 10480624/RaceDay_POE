-- Profiles (one per user).
INSERT INTO Profiles
    (user_id, date_of_birth, emergency_contact_name, emergency_contact_phone, medical_conditions)
VALUES
    (1, '1985-03-15', 'Mrs Mokoena', '0821112222', 'None'),
    (2, '1990-07-22', 'Mr Khumalo', '0833334444', 'Asthma'),
    (3, '1995-11-08', 'Mr Dlamini', '0845556666', 'None'),
    (4, '1988-01-30', 'Mrs Naidoo', '0857778888', 'Penicillin allergy');

-- Events (parody names of South African races).
INSERT INTO Events
    (organiser_id, name, description, event_date, location, distance, event_type)
VALUES
    (1, 'Comrades Clash Marathon', 'The ultimate test of endurance between two great cities.',
        '2026-10-18', 'Pietermaritzburg to Durban', 89.00, 'run'),
    (1, 'Cape Coast Circuit Tour', 'A scenic cycle around the peninsula with ocean views.',
        '2026-11-08', 'Cape Town', 109.00, 'cycle'),
    (2, 'Soweto Showdown Marathon', 'A race through the historic streets of Soweto.',
        '2026-09-20', 'Soweto, Johannesburg', 42.20, 'run'),
    (2, 'Twin Valleys Marathon', 'A challenging route through two lush valleys.',
        '2026-12-06', 'Cape Town', 56.00, 'run');

-- Categories for Event 1: Comrades Clash Marathon.
INSERT INTO Categories (event_id, category_name, age_range, distance)
VALUES
    (1, 'Senior Men', '20-34', 89.00),
    (1, 'Senior Women', '20-34', 89.00),
    (1, 'Veteran Men', '35-49', 89.00);

-- Categories for Event 2: Cape Coast Circuit Tour.
INSERT INTO Categories (event_id, category_name, age_range, distance)
VALUES
    (2, 'Elite Cyclists', 'Open', 109.00),
    (2, 'Amateur Cyclists', 'Open', 109.00);

-- Categories for Event 3: Soweto Showdown Marathon.
INSERT INTO Categories (event_id, category_name, age_range, distance)
VALUES
    (3, 'Full Marathon', 'Open', 42.20),
    (3, 'Half Marathon', 'Open', 21.10);

-- Categories for Event 4: Twin Valleys Marathon.
INSERT INTO Categories (event_id, category_name, age_range, distance)
VALUES
    (4, 'Ultra Distance', 'Open', 56.00),
    (4, 'Short Course', 'Open', 21.00);

-- Enrolments (sample sign-ups).
INSERT INTO Enrolments
    (user_id, event_id, category_id, enrolment_date, status)
VALUES
    (3, 1, 1, '2026-08-01 10:30:00', 'Confirmed'),
    (3, 2, 5, '2026-08-05 14:15:00', 'Pending'),
    (4, 3, 7, '2026-08-10 09:00:00', 'Confirmed'),
    (4, 4, 9, '2026-08-12 16:45:00', 'Pending');

-- Results (finish data for completed events).
INSERT INTO Results (enrolment_id, finish_time, finishing_position)
VALUES
    (1, '05:42:18', 247),
    (3, '03:15:22', 89);

-- Notifications (system messages).
INSERT INTO Notifications (user_id, event_id, message, is_read)
VALUES
    (3, 1, 'Your enrolment for Comrades Clash Marathon has been confirmed.', 1),
    (3, 2, 'Reminder: Cape Coast Circuit Tour starts at 06:00 AM.', 0),
    (4, 3, 'Soweto Showdown Marathon route map has been updated.', 1),
    (4, NULL, 'Welcome to RaceDay! Complete your profile to get started.', 0);

GO

/*
============================================================
VERIFICATION QUERIES
Uncomment these to verify your data after running the script.
============================================================
*/

-- SELECT * FROM Users;
-- SELECT * FROM Profiles;
-- SELECT * FROM Events;
-- SELECT * FROM Categories;
-- SELECT * FROM Enrolments;
-- SELECT * FROM Results;
-- SELECT * FROM Notifications;

-- Show all enrolments with user and event details.
-- SELECT
--     e.enrolment_id,
--     u.first_name + ' ' + u.last_name AS participant_name,
--     ev.name AS event_name,
--     c.category_name,
--     e.status
-- FROM Enrolments e
-- JOIN Users u ON e.user_id = u.user_id
-- JOIN Events ev ON e.event_id = ev.event_id
-- JOIN Categories c ON e.category_id = c.category_id;

-- Show results with participant and event details.
-- SELECT
--     r.result_id,
--     u.first_name + ' ' + u.last_name AS participant_name,
--     ev.name AS event_name,
--     r.finish_time,
--     r.finishing_position
-- FROM Results r
-- JOIN Enrolments e ON r.enrolment_id = e.enrolment_id
-- JOIN Users u ON e.user_id = u.user_id
-- JOIN Events ev ON e.event_id = ev.event_id;
