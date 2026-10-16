
RaceDay — Event Management System
Module: PROG6212 — Programming 2B
Portfolio of Evidence: Part 1 and Part 2
Author: Tumisang Moipolai
Student number: ST10480624
Repository: https://github.com/10480624/RaceDay_POE

1. Project Overview
RaceDay is a web-based event management system for the South African road-running, walking, and cycling community. Its intended functionality allows event organisers to create and manage events, define event categories, manage enrolments, record race results, and communicate updates to participants. Participants can browse events, enrol in an event category, view their own enrolments and results, and maintain their profile.

The project is developed in stages:

Part 1 — Planning and database design: entity relationship design, API endpoint planning, and the SQL Server database schema.
Part 2 — API development: ASP.NET Core Web API, Entity Framework Core, JWT authentication, role-based access control, automated tests, and continuous integration.
Part 3 — Planned extension: an MVC web application consuming the API, Azure Blob Storage integration, and Docker containerisation.
This README describes the planned system and the Part 2 implementation. Where a planned feature has not been implemented or verified, that limitation is stated rather than presented as completed functionality.

2. User Roles
Organiser
The intended Organiser role is responsible for managing events. Planned capabilities include:

Creating events and updating or deleting events they own.
Defining categories for events.
Viewing enrolments for their events.
Recording and updating results for eligible enrolments.
Sending event-related notifications to participants.
In the current API, event creation, updating, and deletion are restricted to the Organiser role. Event ownership checks are applied to event updates and deletions. The implemented results endpoints also apply organiser-role and event-ownership checks.

Participant
The intended Participant role is responsible for taking part in events. Planned capabilities include:

Registering and logging in.
Browsing available events.
Enrolling in an event and selecting a category.
Viewing their own enrolments and results.
Viewing and updating personal profile information.
Receiving and managing event notifications.
Public registration is restricted to the Participant role. The current enrolment controller allows participants to create and retrieve their own enrolments. The available profile and results endpoints enforce authenticated access and ownership rules as implemented by those controllers.

3. Part 1 — Planning and Database Design
Part 1 established the database structure and the planned API contract that Part 2 builds on.

Database entities
The database design contains seven core entities:

Users — account details and role information.
Profiles — additional personal and emergency-contact information.
Events — event details and organiser ownership.
Categories — categories associated with events.
Enrolments — participant registrations for event categories.
Results — finishing time and finishing position for an enrolment.
Notifications — event-related messages and read status.
Design decisions
One Users table with a role column: Organisers and Participants share account fields, so a common Users table avoids duplicating account data. A separate Profiles table stores additional personal information.
A separate Profiles entity: profile details are kept apart from core authentication information.
SQL Server BIT for Boolean values: this is appropriate for true/false flags such as a notification's read status.
Referential integrity: primary keys, foreign keys, and constraints maintain valid relationships between related records. Delete behaviour should be considered for each relationship so dependent data is handled consistently with application requirements.
Part 1 artefacts
The repository includes the planning and database artefacts at the paths present in the repository, including:

API_Endpoint_Plan.md — planned endpoints, access requirements, and expected behaviour.
RaceDay_Database.sql — SQL Server database creation script.
RaceDay_ERD.drawio.pdf — entity relationship diagram, if present at the repository root.
Check the repository tree for the current location of each artefact before submission; older Part 1 documentation may refer to a /docs folder or image format that does not match the current file layout.

4. Part 2 — API Technologies
Area	Technology
Language	C#
Framework	ASP.NET Core Web API
Target framework	.NET 10
Database	Microsoft SQL Server Express
Data access	Entity Framework Core
Authentication	JSON Web Token (JWT) bearer authentication
API documentation	Swagger / OpenAPI
Automated testing	xUnit and Entity Framework Core InMemory
Version control	Git and GitHub
Continuous integration	GitHub Actions
5. Implemented API Functionality
The API uses Entity Framework Core to access the RaceDay database. JWT bearer authentication identifies authenticated users, and role-based authorization restricts access to protected endpoints.

Authentication
The authentication controller provides:

POST /api/auth/register — registers a Participant account.
POST /api/auth/login — validates credentials and returns a JWT for successful authentication.
Registration normalises email addresses and rejects duplicate accounts. Passwords are hashed rather than stored as plain text. Public registration does not allow users to create an Organiser account. A logout endpoint was listed in the Part 1 plan but is not implemented in the current controller.

Profiles
The profile controller provides:

GET /api/profile — retrieves the authenticated user's profile.
PUT /api/profile — updates the authenticated user's profile.
The controller uses the authenticated user's identity and supports profile information such as name, date of birth, emergency contact details, medical conditions, and profile-picture URL. The implemented route is /api/profile, which differs from the /api/users/profile route in the Part 1 plan.

Events
The event controller provides:

GET /api/events — lists events.
GET /api/events/{id} — retrieves an event by ID.
POST /api/events — creates an event; Organiser access is required.
PUT /api/events/{id} — updates an event; Organiser access and event ownership are checked.
DELETE /api/events/{id} — deletes an event; Organiser access and event ownership are checked.
Event creation validates required information such as the event name and location, date, distance, and event type. Deletion is prevented when enrolments exist for the event.

Categories
The category controller provides category listing and creation under the nested route:

GET /api/events/{eventId}/categories
POST /api/events/{eventId}/categories — restricted to Organisers.
The full category-management set described in the Part 1 plan, including category update and deletion endpoints, has not been confirmed as implemented.

Enrolments
The enrolment controller provides:

GET /api/enrolments — retrieves the authenticated Participant's enrolments.
POST /api/enrolments — creates a Participant enrolment.
The controller checks enrolment requests and handles duplicate enrolment attempts and invalid or missing event/category references. The controller is restricted to the Participant role. Several enrolment-management endpoints from the Part 1 plan, including organiser event-enrolment views, status updates, individual enrolment retrieval, and participant cancellation, are not present in the current controller.

Results
The results controller provides:

GET /api/results — retrieves results visible to the authenticated user.
GET /api/results/{id} — retrieves a result by ID, subject to access checks.
POST /api/results — records a result; Organiser access and event ownership are checked.
PUT /api/results/{id} — updates a result; Organiser access and event ownership are checked.
Participant result access is restricted to the participant's own results, while organisers are restricted to results associated with their events. Result creation validates the enrolment, finishing time, and finishing position, and requires the enrolment status to be Completed. Because a complete enrolment-status workflow has not been confirmed in the current API, this prerequisite should be checked during runtime verification.

Notifications
Notifications are part of the planned database design and Part 1 API plan. A complete Notifications API controller has not been confirmed in the current implementation. Notification endpoints should therefore not be treated as implemented functionality.

API plan deviations
The current implementation does not yet fully match every endpoint in the Part 1 plan. Known differences include:

The profile route is /api/profile rather than /api/users/profile.
Authentication includes registration and login, but no logout endpoint.
Category management is incomplete compared with the planned endpoint set.
Enrolment management includes listing a participant's enrolments and creating enrolments, but not every planned management route.
Notifications endpoints have not been confirmed as implemented.
The planned organiser-specific event-list route has not been confirmed as implemented.
The endpoint plan and controllers should be compared before submission, and these differences should be explained honestly in the assessment. Do not claim that all planned endpoints are complete.

6. Database Configuration
The API is configured to use Microsoft SQL Server Express with a local instance such as (local)\SQLEXPRESS. The development database is named RaceDay. The connection string is read from application configuration. The current development configuration uses a connection string in this form:

Server=(local)\SQLEXPRESS;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
Before running the API:

Install and start SQL Server Express.
Create the RaceDay database and required schema using the supplied SQL script or the database setup process used by the project.
Confirm the connection string matches the SQL Server instance on your machine.
Configure the JWT settings required by the application, including Jwt:Key, Jwt:Issuer, Jwt:Audience, and the configured expiry value.
Security: the connection string above is intended for local development. Do not commit production passwords, real secrets, or production JWT signing keys to source control. Use user secrets or an appropriate environment-specific secret-management method for sensitive configuration. Any development signing key in the repository must be replaced before deployment.

7. Running the API
Run these commands from the repository root in PowerShell.

Start the API
dotnet watch --project .\RaceDay.Api\RaceDay.Api.csproj run
Keep this terminal open while using the API. dotnet watch monitors source files and rebuilds or restarts the application when applicable changes are detected.

When the application is running on the configured local port, open Swagger:

http://localhost:5170/swagger

The port may differ if the launch configuration or application settings have changed.

Use Swagger
Open the Swagger URL.
Expand an endpoint to inspect its request and response formats.
Register a Participant account using the registration endpoint.
Log in and copy the returned JWT.
Select Authorize in Swagger and enter the bearer token as required by the Swagger configuration.
Call protected endpoints using an account and role that are permitted to access them.
Build without running
Stop the API watcher with Ctrl+C, then run:

dotnet build .\RaceDay.Api\RaceDay.Api.csproj
If a build is attempted while another process is using the build output, stop the running watcher first and retry.

8. Automated Testing
Automated tests are located in RaceDay.Tests. The test project uses xUnit and Entity Framework Core InMemory to test selected controller behaviour without changing the development SQL Server database.

The current suite contains 10 tests covering:

Successful Participant registration, including email normalisation and password hashing.
Rejection of duplicate email addresses.
Rejection of public Organiser registration.
Rejection of login attempts with an unknown email or incorrect password.
Successful login with valid credentials and token generation.
Event listing and handling of a missing event.
Rejection of event creation when the organiser identity claim is missing.
Prevention of duplicate Participant enrolments.
Run the tests from the repository root:

dotnet test .\RaceDay.Tests\RaceDay.Tests.csproj
The last recorded local run reported 10 passed, 0 failed, and 0 skipped. These are controller-level unit tests; they do not prove that every endpoint, HTTP middleware rule, role restriction, database integration, or end-to-end workflow has been tested.

9. Continuous Integration
The repository has a GitHub Actions workflow named RaceDay Build and Tests. Its purpose is to restore dependencies, build the project, and run the automated test suite when the configured branch or pull-request triggers are met.

The last recorded GitHub Actions run for commit df19431 (Add profile and results endpoints and expand tests) completed successfully. Verify the latest workflow run on GitHub before submitting, especially after any subsequent code or workflow changes.

CI evidence screenshot: add the screenshot of the successful GitHub Actions run to the repository and link or embed it here once the screenshot has been saved in the repository. Do not leave an image link pointing to a file that has not been committed.

10. Video Demonstrations
Part 1
The Part 1 demonstration should explain the planning documents, ERD decisions, API endpoint plan, and execution of the SQL database script.

Unlisted YouTube URL: Add the actual Part 1 video URL before submission.

Part 2
The Part 2 demonstration should show the API running, Swagger endpoint exploration, registration and login, use of a JWT for protected endpoints, representative event/enrolment/profile/results behaviour, automated tests, and the successful CI workflow. Demonstrate only functionality that works in the submitted version, and explain any known limitations.

Unlisted YouTube URL: Add the actual Part 2 video URL before submission.

11. Commit History
The assessment requires at least 20 meaningful commits per part. Commits should represent genuine incremental development, such as implementation changes, test additions, documentation improvements, and fixes. The commit history should be checked before submission to confirm that the required number of meaningful commits exists for each part.

12. Academic Integrity and AI Disclosure
AI tools were used to support parts of the development and documentation process. The author is responsible for reviewing, understanding, testing, and validating the submitted implementation and for disclosing AI assistance in accordance with the assessment brief and institutional requirements. Any final disclosure should accurately describe the assistance used; it should not imply that AI-generated work was independently authored without assistance.

13. Author
Name: Tumisang Moipolai
Student number: ST10480624
Module: PROG6212 — Programming 2B
Institution: IIE Varsity College