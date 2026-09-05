# RaceDay - API Endpoint Plan
PROG6212 - Portfolio of Evidence - Part 1

Author: Tumisang Moipolai
Date: 2026-09-03

---

Overview

This document lists every RESTful API endpoint the RaceDay system will expose.

It was completed before any application code was written (Part 2) to demonstrate that the system was fully understood at the design stage.

The plan covers six core functional areas plus one additional feature:

1. Authentication
2. User Profile
3. Events
4. Categories
5. Event Enrolments
6. Results
7. Notifications

---

Role Definitions

Public — No authentication required. Anyone can call this endpoint.
Any — Any authenticated user (Organiser or Participant).
Organiser — Only users with `role = 'Organiser'` can access.
Participant — Only users with `role = 'Participant'` can access.

---

1. Authentication Endpoints

POST `/api/auth/register`

Role: Public

Description:
Registers a new user (Organiser or Participant) with email, password, and role.

Request Body:

```json
{
  "email": "string",
  "password": "string",
  "first_name": "string",
  "last_name": "string",
  "role": "Organiser | Participant"
}
```

Expected Response:

`201 Created` — Returns user object with generated `user_id`.
`409 Conflict` — Email address already registered.

---

POST `/api/auth/login`

Role: Public

Description:
Authenticates a user and establishes a session.

Request Body:

```json
{
  "email": "string",
  "password": "string"
}
```

Expected Response:

`200 OK` — Returns session token + user role.
`401 Unauthorized` — Invalid email or password.

---

POST `/api/auth/logout`

Role: Any

Description:
Ends the current user session.

Request Body:

None

Expected Response:

`200 OK` — Logout confirmation.

---

2. User Profile Endpoints

GET `/api/users/profile`

Role: Any

Description:
Retrieves the currently logged-in user's profile information.

Request Body:

None

Expected Response:

`200 OK` — Returns User + Profile object.
`404 Not Found` — Profile has not been created yet.

---

PUT `/api/users/profile`

Role: Any

Description:
Updates the currently logged-in user's profile (date of birth, emergency contact, medical info, profile picture URL).

Request Body:

```json
{
  "date_of_birth": "YYYY-MM-DD",
  "emergency_contact_name": "string",
  "emergency_contact_phone": "string",
  "medical_conditions": "string",
  "profile_picture_url": "string"
}
```

Expected Response:

`200 OK` — Returns updated profile object.
`400 Bad Request` — Invalid data submitted.

---

GET `/api/users/{id}`

Role: Any

Description:
Retrieves public information for a specific user by ID. Sensitive fields (`password_hash`) are excluded.

Request Body:

None

Expected Response:

`200 OK` — Returns user object.
`404 Not Found` — User does not exist.

---

3. Event Endpoints

GET `/api/events`

Role: Public

Description:
Lists all upcoming events with basic details.

Supports optional query parameters for filtering by `event_type`, `location`, and date range.

Request Body:

None (query parameters optional)

Expected Response:

`200 OK` — Returns array of Event objects.

---

GET `/api/events/{id}`

Role: Public

Description:
Retrieves full details for a single event, including its organiser and available categories.

Request Body:

None

Expected Response:

`200 OK` — Returns Event object with nested Categories.
`404 Not Found` — Event does not exist.

---

POST `/api/events`

Role: Organiser

Description:
Creates a new race event. The `organiser_id` is inferred from the authenticated session.

Request Body:

```json
{
  "name": "string",
  "description": "string",
  "event_date": "YYYY-MM-DD",
  "location": "string",
  "distance": "number",
  "event_type": "run | walk | cycle"
}
```

Expected Response:

`201 Created` — Returns Event object with generated `event_id`.
`400 Bad Request` — Missing required fields.

---

PUT `/api/events/{id}`

Role: Organiser

Description:
Updates an existing event. Only the organiser who created the event can modify it.

Request Body:

```json
{
  "name": "string",
  "description": "string",
  "event_date": "YYYY-MM-DD",
  "location": "string",
  "distance": "number",
  "event_type": "run | walk | cycle"
}
```

Expected Response:

`200 OK` — Returns updated event object.
`403 Forbidden` — User is not the event owner.
`404 Not Found` — Event does not exist.

---

DELETE `/api/events/{id}`

Role: Organiser

Description:
Deletes an event and all its linked categories, enrolments, and notifications (`CASCADE`). Only the creator can delete.

Request Body:

None

Expected Response:

`204 No Content` — Deletion successful.
`403 Forbidden` — User is not the event owner.
`404 Not Found` — Event does not exist.

---

GET `/api/events/organiser/my-events`

Role: Organiser

Description:
Lists all events created by the currently logged-in organiser, including enrolment counts per event.

Request Body:

None

Expected Response:

`200 OK` — Returns array of Event objects with enrolment statistics.

---

4. Category Endpoints

GET `/api/events/{id}/categories`

Role: Public

Description:
Lists all categories defined for a specific event.

Request Body:

None

Expected Response:

`200 OK` — Returns array of Category objects.
`404 Not Found` — Event does not exist.

---

POST `/api/categories`

Role: Organiser

Description:
Creates a new category for an event. The event must belong to the logged-in organiser.

Request Body:

```json
{
  "event_id": "number",
  "category_name": "string",
  "age_range": "string",
  "distance": "number"
}
```

Expected Response:

`201 Created` — Returns Category object.
`403 Forbidden` — User is not the event owner.
`404 Not Found` — Event does not exist.

---

PUT `/api/categories/{id}`

Role: Organiser

Description:
Updates an existing category (name, age range, distance).

Request Body:

```json
{
  "category_name": "string",
  "age_range": "string",
  "distance": "number"
}
```

Expected Response:

`200 OK` — Returns updated category object.
`403 Forbidden` — User is not the event owner.
`404 Not Found` — Category does not exist.

---

DELETE `/api/categories/{id}`

Role: Organiser

Description:
Deletes a category. Only possible if no enrolments are linked to it (enforced at application level).

Request Body:

None

Expected Response:

`204 No Content` — Deletion successful.
`403 Forbidden` — User is not the event owner.
`409 Conflict` — Category has active enrolments.

---

5. Enrolment Endpoints

GET `/api/enrolments`

Role: Any

Description:
For Participants: lists all their own enrolments with event and category details.

For Organisers: lists all enrolments for events they created.

**Request Body:**

None

Expected Response:

`200 OK` — Returns array of Enrolment objects with nested Event and Category data.

---

POST `/api/enrolments`

Role: Participant

Description:
Allows a Participant to enrol in an event by selecting a category.

**Request Body:**

```json
{
  "event_id": "number",
  "category_id": "number"
}
```

Expected Response:

`201 Created` — Returns Enrolment object with status `Pending`.
`409 Conflict` — User is already enrolled in this event.
`404 Not Found` — Event or Category does not exist.

---

GET `/api/enrolments/{id}`

Role: Any

Description:
Retrieves details of a specific enrolment.

Participants can only view their own. Organisers can view any enrolment for their events.

Request Body:

None

Expected Response:

`200 OK` — Returns Enrolment object.
`403 Forbidden` — Access denied.
`404 Not Found` — Enrolment does not exist.

---

PUT `/api/enrolments/{id}/status`

Role: Organiser

**Description:**
Updates the status of an enrolment (e.g., Pending to Confirmed, or Confirmed to Cancelled).

Request Body:

```json
{
  "status": "Pending | Confirmed | Cancelled | Completed | Archived"
}
```

Expected Response:

`200 OK` — Returns updated enrolment object.
`400 Bad Request` — Invalid status value.
`403 Forbidden` — User is not the event owner.

---

DELETE `/api/enrolments/{id}`

Role: Participant

Description:
Allows a Participant to cancel their own enrolment. Removes the enrolment and any linked result.

Request Body:

None

Expected Response:

`204 No Content` — Cancellation successful.
`403 Forbidden` — Not the enrolment owner.
`404 Not Found` — Enrolment does not exist.

---

GET `/api/enrolments/event/{event_id}`

Role: Organiser

Description:
Lists all enrolments for a specific event. Only accessible to the organiser who created the event.

Request Body:

None

Expected Response:

`200 OK` — Returns array of Enrolment objects with Participant details.
`403 Forbidden` — User is not the event owner.

---

6. Result Endpoints

GET `/api/results`

**Role:** Any

Description:
For Participants: lists all their personal results across events.

For Organisers: lists all results for events they created.

Request Body:

None

Expected Response:**

`200 OK` — Returns array of Result objects with nested Event and Enrolment data.

---

GET `/api/results/{id}`

Role: Any

Description:
Retrieves a single result by ID. Participants can only view their own results.

Request Body:

None

Expected Response:**

`200 OK` — Returns Result object with finish time and position.
`403 Forbidden` — Access denied.
`404 Not Found` — Result does not exist.

---

POST `/api/results`

Role:** Organiser

Description:
Records a finish time and finishing position for a completed enrolment. Only the event organiser can record results.

**Request Body:**

```json
{
  "enrolment_id": "number",
  "finish_time": "HH:MM:SS",
  "finishing_position": "number"
}
```

Expected Response:

`201 Created` — Returns Result object.
`409 Conflict` — Result already exists for this enrolment.
`404 Not Found` — Enrolment does not exist.

---

PUT `/api/results/{id}`

Role: Organiser

**Description:**
Updates an existing result (e.g., correcting a finish time or position).

**Request Body:**

```json
{
  "finish_time": "HH:MM:SS",
  "finishing_position": "number"
}
```

Expected Response:**

`200 OK` — Returns updated result object.
`403 Forbidden` — User is not the event owner.
`404 Not Found` — Result does not exist.

---

7. Notification Endpoints

GET `/api/notifications`

Role: Any

Description:
Lists all notifications for the currently logged-in user, ordered by most recent.

Request Body:

None

Expected Response:

`200 OK` — Returns array of Notification objects.

---

POST `/api/notifications`

Role: Organiser

Description:
Sends a notification to all enrolled participants of a specific event (e.g., route update, weather alert).

Request Body:

```json
{
  "event_id": "number",
  "message": "string"
}
```

Expected Response:

`201 Created` — Returns Notification object.
`404 Not Found` — Event does not exist.

---

PUT `/api/notifications/{id}/read`

Role: Any

Description:
Marks a specific notification as read by the current user.

Request Body:

None

Expected Response:

`200 OK` — Returns updated notification object.
`403 Forbidden` — Not the notification recipient.
`404 Not Found` — Notification does not exist.

---

DELETE `/api/notifications/{id}`

Role: Any

Description:
Deletes a notification from the user's inbox.

Request Body:

None

Expected Response:

`204 No Content` — Deletion successful.
`403 Forbidden` — Not the notification recipient.

---

Summary

Total Endpoints: 28

By Role

Public: 3
Any (Authenticated): 9
Organiser Only: 12
Participant Only: 4

By HTTP Method

GET: 14
POST: 8
PUT: 5
DELETE: 4

---

Design Notes

Why These Endpoints?

1. Authentication Separation

Authentication is separated from Users under the `/api/auth` prefix.

This groups all security-related endpoints together, making the API structure predictable and Swagger documentation cleaner.

2. Event-Centric Routing

Categories are accessed via `/api/events/{id}/categories` for public reads, but created via `/api/categories` for organiser management.

This balances readability with RESTful design.

3. Role Enforcement at the API Level

Every endpoint that returns `403 Forbidden` enforces role-based access.

This is a core requirement of the brief and will be implemented using session-based middleware in Part 2.

4. Results Linked to Enrolments

Results link to Enrolments rather than directly to Users.

A result belongs to an enrolment, which already links the user, event, and category. This avoids redundant foreign keys and maintains the 1:1 relationship defined in the ERD.

5. Notifications as a Lightweight Feature

Notifications contain only four endpoints.

This adds value without the scope explosion of a full messaging system.

Organisers broadcast; participants receive and mark notifications as read.

---

Deviations from the Brief

None.

This plan covers all minimum requirements:

* Authentication
* User Profile
* Events
* Categories
* Enrolments
* Results

It also adds Notifications as an extra feature.

Any deviations in Part 2 implementation will be documented in the README with justification.
