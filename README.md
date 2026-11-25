# Library Management API

A REST API for managing a library's books and members, built with ASP.NET Core (.NET 8) as a fresher learning assignment.

**Current status:** Level 1 and Level 2 complete. Levels 3 and 4 are not started.

## Tech stack

- .NET 8, ASP.NET Core Web API (controllers)
- Entity Framework Core 8 with SQLite
- Swagger / OpenAPI (Swashbuckle) for interactive documentation

## Project structure

```
LibraryManagement/
├── global.json                     pins the .NET SDK version
├── README.md
├── LEARNING.md                     learning journal
├── docs/                           Swagger screenshots
└── LibraryManagement.Api/
    ├── Program.cs                  app startup and service registration
    ├── appsettings.json            configuration (connection string)
    ├── Models/                     database entities (Book, Member)
    ├── Data/                       LibraryDbContext (EF Core)
    ├── Migrations/                 EF Core migrations
    ├── Dtos/                       request and response shapes
    ├── Services/                   business logic (interfaces + classes)
    └── Controllers/                HTTP endpoints
```

## Architecture

```
Controller  ->  Service  ->  DbContext  ->  library.db (SQLite)
(HTTP only)    (logic)       (EF Core)
```

- **Controllers** translate HTTP into method calls and return status codes. They contain no database code.
- **Services** hold the logic and map between entities and DTOs. They are registered with dependency injection (scoped lifetime) and used through interfaces (`IBookService`, `IMemberService`).
- **DTOs** separate what clients send (`BookRequest`, `MemberRequest`) from what the API returns (`BookDto`, `MemberDto`). Entities are never exposed directly, so clients cannot set server-controlled fields such as `Id` or `JoinedOn`.
- **Validation** uses data annotations on the request DTOs. ASP.NET Core returns 400 automatically when they fail.

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- The EF Core command-line tool (once per machine):

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

### Run

```bash
cd LibraryManagement/LibraryManagement.Api
dotnet restore
dotnet ef database update      # creates library.db from the migrations
dotnet run
```

Open the URL printed in the console and add `/swagger` to it.

### Migrations

Migrations are in `LibraryManagement.Api/Migrations`. Run these from `LibraryManagement.Api`.

```bash
dotnet ef migrations add <Name>    # after changing an entity or the DbContext
dotnet ef database update          # apply pending migrations to library.db
dotnet ef migrations list          # show applied and pending migrations
```

The database file `library.db` is created in `LibraryManagement.Api` and is not committed to git.

## API endpoints

### Books: `/api/books`

| Method | Route | Success | Errors |
| --- | --- | --- | --- |
| GET | `/api/books` | 200 | |
| GET | `/api/books/{id}` | 200 | 404 |
| POST | `/api/books` | 201 with `Location` header | 400 |
| PUT | `/api/books/{id}` | 204 | 400, 404 |
| DELETE | `/api/books/{id}` | 204 | 404 |

Example request body for POST and PUT:

```json
{
  "title": "Clean Code",
  "author": "Robert Martin",
  "isbn": "9780132350884",
  "publishedYear": 2008,
  "totalCopies": 3,
  "availableCopies": 3
}
```

### Members: `/api/members`

| Method | Route | Success | Errors |
| --- | --- | --- | --- |
| GET | `/api/members` | 200 | |
| GET | `/api/members/{id}` | 200 | 404 |
| POST | `/api/members` | 201 with `Location` header | 400 |
| PUT | `/api/members/{id}` | 204 | 400, 404 |
| DELETE | `/api/members/{id}` | 204 | 404 |

Example request body for POST and PUT:

```json
{
  "fullName": "Ada Lovelace",
  "email": "ada@example.com",
  "isActive": true
}
```

`isActive` is optional and defaults to `true`. `joinedOn` is set by the server when the member is created.

### Status codes used

| Code | Meaning |
| --- | --- |
| 200 | OK, body contains the data |
| 201 | Created, `Location` header points to the new resource |
| 204 | Success with no response body (PUT, DELETE) |
| 400 | Invalid input or duplicate ISBN / email |
| 404 | Resource not found |

## Validation rules

**Book**

| Field | Rule |
| --- | --- |
| Title | required, max 200 characters |
| Author | required, max 100 characters |
| ISBN | required, max 20 characters, unique |
| PublishedYear | between 1000 and 2100 |
| TotalCopies | at least 1 |
| AvailableCopies | at least 0 and not more than TotalCopies |

**Member**

| Field | Rule |
| --- | --- |
| FullName | required, max 100 characters |
| Email | required, valid email format, max 200 characters, unique |
| IsActive | optional, defaults to true |

Uniqueness is checked in code (returning 400 with a message) and enforced by a unique index in the database.

## Assumptions

- ISBN and email must be unique. A duplicate returns 400.
- Email uniqueness is case-sensitive (SQLite default), so `Ada@x.com` and `ada@x.com` count as different emails.
- `Id` and `JoinedOn` are set by the server. Any value for them in a request is ignored because the request DTOs do not contain them.
- `PUT` replaces the whole record. Omitting `isActive` in a member update sets it to `true`.
- `JoinedOn` is stored in UTC and is never changed by an update.
- `IsActive` is set manually through the API. Automatic deactivation after a period of inactivity is not implemented.
- Deletes are hard deletes (the row is removed).
- The `PublishedYear` range (1000 to 2100) is an arbitrary sanity limit.

## Known limitations

- If two requests with the same ISBN or email arrive at the same instant, the database unique index rejects the second, and the current code does not catch that exception, so the client would see a 500. Global exception handling is planned for Level 3.
- No authentication, paging or filtering yet.
- No loans yet, so books and members are not linked.

## Development levels

### Level 1: basic CRUD (completed)

- ASP.NET Core Web API on .NET 8 using controllers
- CRUD for `Book` with an in-memory list
- Correct status codes: 200, 201 with `Location`, 204, 400, 404
- Swagger enabled

### Level 2: persistence and structure (completed)

- EF Core 8 with SQLite, `LibraryDbContext` and migrations
- `Member` entity and CRUD endpoints
- DTOs separating API shapes from entities
- Service layer registered through dependency injection
- Validation with data annotations and `IValidatableObject`
- Books moved from the in-memory list to the database

### Level 3 and Level 4

Not started.

## Troubleshooting

- **`no such table` during `dotnet ef database update` and no `library.db` appears:** the connection string key in `appsettings.json` must match the name used in `Program.cs` (`Default`). If the key is missing, SQLite silently uses a temporary empty database.
- **`Unable to resolve service for type ...`:** the service is not registered in `Program.cs`, or the registration comes after `builder.Build()`.
- **`dotnet ef` command not found:** install the tool (see Prerequisites) and make sure `~/.dotnet/tools` is on your `PATH`.

## Screenshots

Swagger screenshots of every endpoint, including a 400 and a 404 response, are in the `docs/` folder.