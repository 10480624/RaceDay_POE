RaceDay - Event Management System  PROG6212 - Portfolio of Evidence - Part 1  Author: Tumisang Moipolai  Student Number: ST10480624  Date: 2026-09-03

System Description  RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage races, define categories, capture participant results, and communicate with entrants. Participants can browse upcoming events, enrol by selecting a category, track their personal performance history, and receive event notifications.  The system is built progressively across three parts: Part 1 (Current): System planning ERD, API endpoint plan, and SQL database schema. Part 2: RESTful API development in ASP.NET Core with Entity Framework, role-based authentication, unit testing, and CI/CD. Part 3: MVC web application consuming the API, Azure Blob Storage integration, and Docker containerisation.

User Roles
Organiser  = Registers and logs in as an Organiser.  - Creates, updates, and deletes events (name, description, date, location, distance, type).  - Defines age and distance categories for each event.  - Views all enrolments for their events.  - Captures finish times and finishing positions after an event concludes.  - Sends notifications to enrolled participants.

Participant  - Registers and logs in as a Participant.  - Browses all upcoming events with filtering options.  - Enrols in an event by selecting a category.  - Views their own enrolments and enrolment status.  - Views their personal race results and finishing positions.  - Updates their profile (date of birth, emergency contact, medical conditions, profile picture).  - Receives and manages event notifications.

Part 1 Deliverables  All planning documents are located in the /docs folder:  1. RaceDay_ERD.png — Entity Relationship Diagram showing all 7 entities, primary keys, foreign keys, and cardinalities.  2. API_Endpoint_Plan.md — Complete RESTful API endpoint specification with 28 endpoints, role requirements, request bodies, and expected responses.  3. RaceDay_Database.sql — Full SQL Server database script including CREATE TABLE statements with constraints, foreign keys, and realistic seed data.

Database Design Decisions, Why 7 entities? The brief requires a minimum of 6 entities. The core 6 are Users, Profiles, Events, Categories, Enrolments, and Results. A 7th entity "Notifications" was added to demonstrate extended system thinking without scope explosion. Notifications allow Organisers to broadcast updates to participants, addressing the brief's mention of disconnected communication channels. Why one Users table with a role column?  Both Organisers and Participants need the same login fields (email, password hash, name). Creating separate tables would duplicate these columns and violate normalisation. A single Users table with a role column keeps the design clean while a separate Profiles table stores role-agnostic personal information.  Why BIT instead of INT for boolean flags?  SQL Server's BIT type stores 0/1 in a single bit, making it the correct professional choice for true/false flags like is_read. Using INT would waste 31 bits per row.  Why ON DELETE CASCADE on all foreign keys?  Child records (Categories, Enrolments, Results, Notifications) only exist because of their parent records (Events, Users). If an Event is deleted, its Categories and Enrolments become meaningless. CASCADE automatically cleans up these dependencies, preventing orphaned records and maintaining referential integrity.

CI/CD Pipeline  This repository uses GitHub Actions to validate that all Part 1 planning documents are present before any code is merged.  ![CI/CD Green Build](docs/ci-cd-green-build.png)  The workflow file is located at .github/workflows/validate-docs.yml. It checks:  1. The /docs folder exists.  2. The ERD image (.png or .pdf) is present.  3. The API Endpoint Plan (.md) is present.  4. The SQL Database Script (.sql) is present.

Video Presentation  A walkthrough of the Part 1 planning documents, ERD design decisions, API endpoint choices, and a live demonstration of the SQL script running in SSMS.  YouTube Link: [Insert unlisted YouTube link here]  

How to Run the SQL Script  1. Open SQL Server Management Studio (SSMS).  2. Connect to your SQL Server instance.  3. Open docs/RaceDay_Database.sql.  4. Execute the script (F5 or click Execute).  5. The script creates the RaceDay database, all 7 tables, constraints, and seed data.  6. Uncomment the verification queries at the bottom to inspect the seeded data.

Technologies Used (Planned for Parts 2 and 3)  Backend API: ASP.NET Core Web API (C#)  ORM: Entity Framework Core (Code-First)  Database: Microsoft SQL Server  Authentication: Session-based with password hashing  API Documentation: Swagger / OpenAPI  Frontend: ASP.NET Core MVC  Cloud Storage: Azure Blob Storage  **Containerisation: Docker  CI/CD: GitHub Actions

Commit History  A minimum of 20 meaningful commits is required per part. Commits reflect real development work — planning decisions, document updates, and iterative refinements.

Academic Integrity  This project was planned and designed with the support of AI tools for brainstorming and structure. All design decisions, entity relationships, and endpoint logic were reviewed, understood, and validated by the author before submission. AI assistance is disclosed here in accordance with the assignment requirements.

Part 2

Part 2: API Development

Part 2 of the RaceDay project focuses on developing the backend API for the event management system. The API provides endpoints for managing event-related information and supports authentication and role-based access control.

The application is developed using ASP.NET Core Web API, C#, Entity Framework Core, and Microsoft SQL Server. Swagger is used to document the API endpoints and support manual testing.

The implementation builds on the database design and planning completed in Part 1.

Part 2 Objectives

- Develop REST API endpoints for the RaceDay application.
- Connect the API to the SQL Server database using Entity Framework Core.
- Implement JWT bearer authentication.
- Apply role-based authorization to protected endpoints.
- Validate incoming requests and handle common errors.
- Implement and test event enrolment functionality.
- Create automated tests for selected API behaviour.
- Configure a GitHub Actions workflow to build the project and run automated tests.


Technologies and Tools

The following technologies are used in the Part 2 implementation:

- **Programming language:** C#
- **Framework:** ASP.NET Core Web API
- **Target framework:** .NET 10
- **Database:** Microsoft SQL Server Express
- **Object-relational mapping:** Entity Framework Core
- **Authentication:** JSON Web Token (JWT) bearer authentication
- **API documentation and testing:** Swagger / OpenAPI
- **Automated testing:** .NET test project with Entity Framework Core InMemory for the enrolment unit test
- **Version control:** Git and GitHub
- **Continuous integration:** GitHub Actions


API Functionality

The RaceDay API provides backend functionality for the event management system. Its implementation includes authentication and protected operations for event enrolments.

Authentication and Authorization

The API uses JWT bearer authentication to identify authenticated users. Role-based authorization restricts access to endpoints according to the user's role.

The enrolment controller restricts access to participants. Organisers and other unauthorized roles cannot access participant-only enrolment operations.

Event Enrolments

The enrolment functionality supports creating an enrolment and retrieving the authenticated participant's enrolments.

The API validates enrolment requests and handles relevant error conditions, including:

- Successful enrolment creation.
- Duplicate enrolment attempts.
- Invalid event and category combinations.
- References to events or categories that do not exist.
- Unauthorized access to participant-only operations.

The API returns appropriate HTTP status codes for these outcomes, including `201 Created`, `200 OK`, `400 Bad Request`, `403 Forbidden`, `404 Not Found`, and `409 Conflict`, depending on the request and its result.

Other functionality should be documented according to the endpoints implemented and verified in the project.


Database Configuration

The application uses Microsoft SQL Server and Entity Framework Core to access the RaceDay database.

Before running the application, ensure that:

1. SQL Server is installed and running.
2. The RaceDay database has been created using the supplied database script or the applicable project setup process.
3. The database connection string is configured for the local SQL Server instance.
4. The required JWT configuration values are supplied.

The application reads its database connection string and JWT settings from configuration. These include the database connection string, `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience`.

**Security note:** Do not commit real passwords, production connection strings, or JWT signing keys to the repository. Use local development configuration or an appropriate secrets-management method.

Running the API

Open a terminal in the repository directory and run:

```powershell
dotnet restore
dotnet build
dotnet run --project .\RaceDay.Api\RaceDay.Api.csproj
```

When the application is running on the configured local port, open Swagger in a browser:

http://localhost:5170/swagger

The actual address may differ if the application uses another configured port.

Swagger can be used to inspect available endpoints and manually test requests. For endpoints requiring authentication, provide a valid JWT using the Swagger Authorize function.


Testing

The project includes a .NET test project for automated testing of selected application behaviour.

The enrolment unit test checks that an attempt to enrol a participant in the same event more than once returns a conflict response and does not create another enrolment.

Run the automated tests from the repository directory using:

```powershell
dotnet test .\RaceDay.Tests\RaceDay.Tests.csproj
```

The tests should complete successfully before changes are considered ready for submission.

Continuous Integration

GitHub Actions is configured to run a build-and-test workflow when changes are pushed to the `main` branch or when a pull request targets that branch.

The workflow performs the following steps:

1. Checks out the repository.
2. Sets up the .NET SDK.
3. Restores project dependencies.
4. Builds the test project and its referenced application project.
5. Runs the automated tests.

This workflow provides an automated check that the project can build and that its configured tests pass in the CI environment.

The latest workflow result should be checked on GitHub before submission.



Video Presentation

**Presentation link:** [Replace this text with the actual unlisted YouTube video URL]

The video presentation should demonstrate the implemented functionality and explain the relevant technical decisions. The demonstration should reflect what is working in the submitted version of the project.

Academic Integrity and AI Disclosure

AI tools were used to support parts of the development and documentation process. The submitted implementation and explanations should be reviewed and understood by the author, and any AI-assisted code or content should be disclosed in the manner required by the assessment brief.

The final submission should accurately distinguish between functionality that has been implemented and tested and functionality that remains incomplete.


Author  Name: Tumisang Moipolai Student Number: ST10480624  Module: PROG6212 - Programming 2B  Institution: IIE Varsity College  
