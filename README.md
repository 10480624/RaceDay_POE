RaceDay — Event Management System



Module: PROG6212 — Programming 2B



Portfolio of Evidence: Part 1, Part 2 and Part 3



Author: Tumisang Moipolai



Student Number: ST10480624



Repository: \[https://github.com/10480624/RaceDay\_POE](https://github.com/10480624/RaceDay\_POE)



1\. Project Overview



RaceDay is a web-based event management system designed for the South African road-running, walking and cycling community.



The system provides two user roles:



• Participants can register, log in, browse events, enrol in event categories, view their enrolments and results, and manage their profiles.

• Organisers can create and manage events, manage event categories and enrolments, and record and manage race results.



The application is developed using C#, ASP.NET Core, Entity Framework Core and Microsoft SQL Server. It consists of an ASP.NET Core Web API and an ASP.NET Core MVC web application. The MVC application communicates with the API rather than accessing the database directly.



The project was completed in three parts:



| Part | Description |

| --- | --- |

| Part 1 | Requirements, database design, ERD and API endpoint planning |

| Part 2 | ASP.NET Core Web API, database integration, authentication, authorisation and automated tests |

| Part 3 | ASP.NET Core MVC application, participant and organiser interfaces, integration with the API, Docker and CI improvements |



2\. Technologies Used



| Area | Technology |

| --- | --- |

| Programming language | C# |

| Framework | ASP.NET Core |

| Target framework | .NET 10 |

| Web API | ASP.NET Core Web API |

| Web application | ASP.NET Core MVC |

| Database | Microsoft SQL Server Express |

| Data access | Entity Framework Core |

| Authentication | JSON Web Tokens (JWT) |

| API documentation | Swagger / OpenAPI |

| Automated testing | xUnit and Entity Framework Core InMemory |

| Version control | Git and GitHub |

| Continuous integration | GitHub Actions |

| Containerisation | Docker |



3\. System Architecture



RaceDay uses a two-application architecture.



```

&#x20;           USER

&#x20;             |

&#x20;             v

&#x20;   ASP.NET Core MVC Web App

&#x20;        RaceDay.Web

&#x20;             |

&#x20;             | HTTP requests

&#x20;             | JWT authentication

&#x20;             v

&#x20;     ASP.NET Core Web API

&#x20;        RaceDay.Api

&#x20;             |

&#x20;             | Entity Framework Core

&#x20;             v

&#x20;     Microsoft SQL Server

&#x20;          RaceDay



```



The MVC application handles the user interface and sends requests to the API through the ApiClient service. The API handles business logic, authentication, authorisation and database operations.



This separation means the web application does not directly access the SQL Server database.



Main projects:



| Project | Purpose |

| --- | --- |

| RaceDay.Api | Provides API endpoints and handles database operations |

| RaceDay.Web | Provides the MVC web interface for participants and organisers |

| RaceDay.Tests | Contains automated tests for selected API functionality |



4\. User Roles and Functionality



4.1 Participant



Participants can:

• Register for a RaceDay account.

• Log in and log out.

• Browse available events.

• View event details.

• View available event categories.

• Enrol in an event.

• View their own enrolments and enrolment statuses.

• View their race results.

• View and update their profile.



Participant pages include:

• Events

• Event Details

• My Enrolments

• Results

• My Profile



4.2 Organiser



Organisers can:

• Log in to the system.

• View and manage events.

• Create new events.

• Edit existing events.

• Delete events where permitted.

• View and manage event categories.

• View enrolments associated with their events.

• Record race results.

• Edit existing results.



Organiser pages include:

• Manage Events

• Manage Categories

• Manage Enrolments

• Results

• My Profile



Organiser access is restricted through role-based authorisation. Event ownership is checked by the API for protected event-management operations.



Public registration creates Participant accounts. Organiser accounts are not created through the public registration form.



5\. Part 1 — Database Design



Part 1 established the database structure and the planned API contract.



The database design contains seven core entities:



| Entity | Purpose |

| --- | --- |

| Users | Stores account details and user roles |

| Profiles | Stores additional personal and emergency-contact information |

| Events | Stores event information and organiser ownership |

| Categories | Stores categories associated with events |

| Enrolments | Stores participant registrations for event categories |

| Results | Stores finishing times and finishing positions |

| Notifications | Stores planned event-related messages and read status |



Primary keys and foreign keys are used to maintain relationships between entities and support referential integrity.



The database creation script is included in the repository.



6\. Part 2 — API Functionality



The API uses Entity Framework Core to communicate with SQL Server. JWT bearer authentication identifies authenticated users, while role-based authorisation protects restricted operations.



6.1 Authentication



The authentication functionality includes:

• Participant registration.

• Login and JWT generation.

• Password hashing.

• Duplicate email validation.

• Restriction of public registration to the Participant role.



6.2 Profiles



Authenticated users can retrieve and update their own profile information.



The profile functionality includes personal details and additional information such as emergency contacts.



6.3 Events



The API supports:

• Listing events.

• Retrieving an event by ID.

• Creating events.

• Updating events.

• Deleting events where permitted.



Event creation, updating and deletion are restricted to Organisers. Ownership checks are applied to protected event-management operations.



6.4 Categories



The API supports retrieving categories for an event and creating categories under an event.



Category management is also available through the organiser MVC interface.



6.5 Enrolments



Participants can create enrolments and retrieve their own enrolments.



The API also provides organiser functionality for viewing and managing enrolments associated with their events.



Enrolment operations validate relevant event and category information and apply access restrictions.



6.6 Results



The results functionality supports:

• Retrieving results.

• Retrieving an individual result subject to access checks.

• Recording results as an Organiser.

• Updating existing results as an Organiser.



Participant access is restricted to their own results, while organiser access is restricted according to event ownership.



6.7 Notifications



The Notifications entity forms part of the database design. A complete notification workflow is not implemented in the submitted application.



7\. Part 3 — MVC Web Application



Part 3 extends the API with an ASP.NET Core MVC application.



The MVC application provides separate interfaces for Participants and Organisers. It communicates with the API using the ApiClient service and uses the authenticated session information to support role-specific navigation and access.



7.1 Authentication and navigation



The web application provides:

• Login.

• Participant registration.

• Logout.

• Role-aware navigation.

• Session-based handling of authentication information.



7.2 Participant interface



Participants can browse events, view event details, enrol in available categories, view their enrolments, access results and manage their profiles.



7.3 Organiser interface



Organisers can manage events, access category management, view event enrolments and manage race results.



The organiser event-management page provides access to the related category and enrolment management pages.



7.4 API integration



The MVC application uses the configured API base address to communicate with the backend.



The development configuration uses:



{

"ApiBaseUrl": "http://localhost:5170"

}



The API and MVC applications must both be running for the complete web application to function.



8\. Database Configuration



The development environment uses Microsoft SQL Server Express.



Typical local configuration:



| Setting | Value |

| --- | --- |

| SQL Server instance | .\\SQLEXPRESS |

| Database name | RaceDay |

| Authentication | Windows Authentication |



Example connection string:



Server=(local)\\SQLEXPRESS;Database=RaceDay;Trusted\_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true



Database setup:



1\. Install and start SQL Server Express.

2\. Open SQL Server Management Studio or another suitable SQL client.

3\. Execute the supplied RaceDay\_Database.sql script to create the database and its schema.

4\. Confirm that the connection string matches the local SQL Server instance.

5\. Configure the required JWT settings for the API.



Do not commit production passwords, private credentials or production JWT signing keys to source control. Use an appropriate secret-management method for sensitive configuration.



9\. Running the Application Locally



The following commands are run from the repository root in PowerShell.



9.1 Start the API



dotnet watch --project .\\RaceDay.Api\\RaceDay.Api.csproj run



The API is configured to run at:

http://localhost:5170



Swagger documentation:

http://localhost:5170/swagger



Keep the API terminal running while using the MVC application.



9.2 Start the MVC application



Open a second PowerShell terminal from the repository root:



dotnet watch --project .\\RaceDay.Web\\RaceDay.Web.csproj run



The MVC application is configured to run at:

http://localhost:5008



Both applications must be running for the complete system to work.



9.3 Build the solution



dotnet build



9.4 Run automated tests



dotnet test .\\RaceDay.Tests\\RaceDay.Tests.csproj



10\. Docker



The MVC web application is designed to run in a Docker container.



The API and SQL Server remain separate services in the local development setup. When the MVC application runs inside Docker, localhost refers to the container itself, not the host computer.



For this reason, the MVC container must use the host-accessible API address:

\[http://host.docker.internal:5170](https://www.google.com/search?q=http%3A%2F%2Fhost.docker.internal%3A5170)



Build the MVC image:

Run from the repository root:



docker build -t raceday-web .



Run the MVC container:



docker run -d --name raceday-web -p 8080:8080 -e ApiBaseUrl=\[http://host.docker.internal:5170](https://www.google.com/search?q=http%3A%2F%2Fhost.docker.internal%3A5170) raceday-web



Open the containerised MVC application:

http://localhost:8080



The following services must be available on the host computer:

• SQL Server Express.

• The RaceDay API running on port 5170.



The Docker container runs the MVC application. The API must remain running separately for the web application to retrieve and update data.



11\. Automated Testing



Automated tests are located in RaceDay.Tests.



The test project uses xUnit and Entity Framework Core InMemory to test selected API controller behaviour without modifying the development SQL Server database.



The tests cover selected authentication, event and enrolment scenarios, including:

• Successful Participant registration.

• Duplicate email rejection.

• Rejection of public Organiser registration.

• Login validation.

• Event listing and missing-event handling.

• Organiser identity validation.

• Duplicate enrolment prevention.



Run the tests using:



dotnet test .\\RaceDay.Tests\\RaceDay.Tests.csproj



These are automated tests of selected application behaviour. They do not, by themselves, prove that every API endpoint, database integration or complete user workflow has been tested.



12\. Continuous Integration



RaceDay uses GitHub Actions for continuous integration.



The workflow restores dependencies, builds the application projects and runs the automated test suite.



The workflow includes a build step for the MVC project:



\* name: Build MVC Web project

run: dotnet build RaceDay.Web/RaceDay.Web.csproj



This helps ensure that changes to the MVC application are included in the automated build process.



The successful GitHub Actions workflow run should be verified on the repository before submission.



13\. Known Limitations and Deviations



The submitted implementation does not include every feature originally considered during planning.



Known limitations include:

• Azure Blob Storage integration for profile-picture storage has not been implemented.

• The planned notification workflow is not implemented.

• The organiser dashboard with consolidated event and enrolment summaries is not implemented.

• Public registration is restricted to Participants; organiser accounts must be provisioned separately.

• Some planned API functionality may differ from the original Part 1 endpoint plan.



These limitations are acknowledged rather than presented as completed functionality.



14\. Continuous Development and Version Control



Git and GitHub were used for source control throughout development.



The repository contains the application source code, database artefacts, automated tests and GitHub Actions workflow.



Development was carried out incrementally, with commits used to record implementation changes, fixes, documentation updates and testing-related changes.



15\. Academic Integrity and AI Disclosure



AI tools were used as development and learning assistance during parts of the project. This included assistance with understanding programming concepts, troubleshooting, reviewing implementation approaches and preparing documentation.



The submitted implementation was reviewed and tested by the student. The student remains responsible for understanding the code, validating the application and ensuring that the final submission complies with the institution's academic integrity requirements.



16\. Author



Name: Tumisang Moipolai



Student Number: ST10480624



Module: PROG6212 — Programming 2B



Institution: IIE Varsity College

Part 3 Docker implementation

A multi-stage `.NET 10` Dockerfile is provided at the repository root. It publishes the MVC application into the ASP.NET Core runtime image; the API and SQL Server remain separate services.

Build from the repository root:

```powershell
docker build -t raceday-web .
```

Start the API on the host first, then run the MVC container:

```powershell
docker run --rm --name raceday-web -p 8080:8080 -e ApiBaseUrl=http://host.docker.internal:5170 raceday-web
```

Open `http://localhost:8080`. The API must be reachable from the container at `http://host.docker.internal:5170`. This Docker configuration has been added to source control; a successful image build and runtime test must still be confirmed on a machine with Docker installed.
