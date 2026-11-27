# Library Management API

A REST API for a small community library, built with ASP.NET Core (.NET 8) as a fresher learning assignment. Librarians manage books and members, and members borrow and return books.

**Current status:** Levels 1, 2 and 3 complete. Level 4 (stretch) is not started.

## Tech stack

- .NET 8, ASP.NET Core Web API (controllers)
- Entity Framework Core 8 with SQLite
- Swagger / OpenAPI (Swashbuckle) for interactive documentation
- Built-in `ILogger` for structured logging

## Project structure

```
LibraryManagement/
├── global.json                     pins the .NET SDK version
├── README.md
├── LEARNING.md                     learning journal
├── docs/                           Swagger screenshots
└── LibraryManagement.Api/
    ├── Program.cs                  app startup, service and middleware registration
    ├── appsettings.json            configuration (connection string, logging)
    ├── Models/                     database entities (Book, Member, Loan)
    ├── Data/                       LibraryDbContext (EF Core)
    ├── Migrations/                 EF Core migrations
    ├── Dtos/                       request, response and query shapes
    ├── Services/                   business logic (interfaces + classes)
    ├── Exceptions/                 NotFoundException, BusinessRuleException
    ├── Middleware/                 global exception handling
    └── Controllers/                HTTP endpoints
```

## Architecture

```
Request -> Exception middleware -> Controller -> Service -> DbContext -> library.db (SQLite)
                                   (HTTP only)   (rules)     (EF Core)
```

- **Controllers** translate HTTP into method calls and return status codes. They contain no database code and almost no error handling.
- **Services** hold the business rules and map between entities and DTOs. They are registered with dependency injection (scoped lifetime) and used through interfaces (`IBookService`, `IMemberService`, `ILoanService`).
- **DTOs** separate what clients send (`BookRequest`, `MemberRequest`, `LoanRequest`, `BookQuery`) from what the API returns (`BookDto`, `MemberDto`, `LoanDto`, `PagedResult<T>`). Entities are never exposed directly, so clients cannot set server-controlled fields such as `Id`, `JoinedOn`, `BorrowedOn` or `DueOn`.
- **Validation** uses data annotations on the request DTOs. ASP.NET Core returns 400 automatically when they fail.
- **Exceptions for rule violations:** services throw `NotFoundException` (404) or `BusinessRuleException` (400). One global middleware, registered first in the pipeline, catches them and writes a consistent JSON error. Any other exception is logged with its stack trace and returned as a generic 500 without internal details.

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

Migrations are in `LibraryManagement.Api/Migrations` (`InitialCreate` for books and members, `AddLoans` for loans). Run these from `LibraryManagement.Api`.

```bash
dotnet ef migrations add <Name>    # after changing an entity or the DbContext
dotnet ef database update          # apply pending migrations to library.db
dotnet ef migrations list          # show applied and pending migrations
```

The database file `library.db` is created in `LibraryManagement.Api` and is not committed to git. To start from an empty database, delete `library.db*` and run `dotnet ef database update` again.

## API endpoints

### Books: `/api/books`

| Method | Route | Success | Errors |
| --- | --- | --- | --- |
| GET | `/api/books?author=&available=&page=&pageSize=` | 200 (paged) | 400 (invalid query) |
| GET | `/api/books/{id}` | 200 | 404 |
| POST | `/api/books` | 201 with `Location` header | 400 |
| PUT | `/api/books/{id}` | 204 | 400, 404 |
| DELETE | `/api/books/{id}` | 204 | 400 (has loan history), 404 |

**Filtering and paging (all parameters optional)**

| Parameter | Meaning | Default |
| --- | --- | --- |
| `author` | author contains this text, case-insensitive | no filter |
| `available` | `true`: copies left, `false`: none left | no filter |
| `page` | page number, at least 1 | 1 |
| `pageSize` | items per page, 1 to 100 | 10 |

Results are ordered by title, then id. The response is a wrapper object:

```json
{
  "items": [ { "id": 1, "title": "Clean Code", "author": "Robert Martin", "isbn": "9780132350884",
               "publishedYear": 2008, "totalCopies": 3, "availableCopies": 3 } ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 12,
  "totalPages": 2
}
```

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
| GET | `/api/members/{id}/loans` | 200 | 404 |
| POST | `/api/members` | 201 with `Location` header | 400 |
| PUT | `/api/members/{id}` | 204 | 400, 404 |
| DELETE | `/api/members/{id}` | 204 | 400 (has loan history), 404 |

Example request body for POST and PUT:

```json
{
  "fullName": "Ada Lovelace",
  "email": "ada@example.com",
  "isActive": true
}
```

`isActive` is optional and defaults to `true`. `joinedOn` is set by the server. `GET /api/members/{id}/loans` returns all of a member's loans (active and returned), newest first. An existing member with no loans gets `[]`, and an unknown member gets 404.

### Loans: `/api/loans`

| Method | Route | Success | Errors |
| --- | --- | --- | --- |
| POST | `/api/loans` (borrow) | 201 with `Location` header | 400, 404 |
| POST | `/api/loans/{id}/return` | 200 with the updated loan | 400, 404 |
| GET | `/api/loans/{id}` | 200 | 404 |

Borrow request body:

```json
{ "bookId": 1, "memberId": 1 }
```

### Status codes used

| Code | Meaning |
| --- | --- |
| 200 | OK, body contains the data |
| 201 | Created, `Location` header points to the new resource |
| 204 | Success with no response body (PUT, DELETE) |
| 400 | Invalid input, duplicate ISBN or email, or a broken business rule |
| 404 | Resource not found |
| 500 | Unexpected error (generic message, details only in the log) |

### Error format

Business rule and not-found errors from the exception middleware look like this:

```json
{ "status": 400, "detail": "A member can hold at most 3 active loans." }
```

Validation failures (for example an empty title) use the standard ASP.NET Core validation problem format, listing each invalid field.

## Business rules

Enforced in `LoanService`, checked in this order:

1. The member and the book must exist (404 otherwise).
2. Inactive members cannot borrow.
3. A member can hold at most 3 active loans (an active loan has no `ReturnedOn`).
4. A book can be borrowed only if `AvailableCopies > 0`.
5. `DueOn` is set to 14 days after `BorrowedOn`.
6. Borrowing decreases `AvailableCopies` and returning increases it. The loan and the copy count are saved in one database transaction.
7. Returning a loan that was already returned is an error (400).
8. A book or member that has loan history cannot be deleted (400).

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

**Loan:** `BookId` and `MemberId` must be at least 1.

Uniqueness of ISBN and email is checked in code (returning 400 with a message) and enforced by a unique index in the database.

## Logging

Logging uses the built-in `ILogger` with named placeholders (structured logging). Expected problems such as not found or a broken rule are logged as warnings. Unexpected exceptions are logged as errors with the stack trace. Successful borrows and returns are logged as information. Output goes to the console, and levels are controlled in the `Logging` section of `appsettings.json`.

## Assumptions

- ISBN and email must be unique. A duplicate returns 400.
- Email uniqueness is case-sensitive (SQLite default), so `Ada@x.com` and `ada@x.com` count as different emails.
- `Id`, `JoinedOn`, `BorrowedOn`, `DueOn` and `ReturnedOn` are set by the server and cannot be sent by clients.
- All timestamps are stored in UTC.
- `PUT` replaces the whole record. Omitting `isActive` in a member update sets it to `true`.
- `IsActive` is set manually through the API. Automatic deactivation after a period of inactivity is not implemented.
- A book or member with any loan history (even returned loans) cannot be deleted, so loan records stay valid. There is no endpoint to delete loans.
- Ids are never reused after deletion (SQLite `AUTOINCREMENT`).
- The author filter matches text anywhere in the author name and ignores upper/lower case for English letters.
- `pageSize` is capped at 100 to protect the server.
- The `PublishedYear` range (1000 to 2100) is an arbitrary sanity limit.
- `GET /api/books` returns a paged wrapper object, which is a change from the plain list returned in Level 2.

## Known limitations

- **Concurrent borrowing:** if two requests borrow the last copy at the same instant, both could pass the availability check and `AvailableCopies` could go below zero. A fix would need a concurrency token or a stricter transaction.
- **Manual edits:** `PUT /api/books/{id}` can set `availableCopies` directly, and nothing reconciles it with the loans currently out.
- **Duplicate race:** two simultaneous requests with the same ISBN or email are stopped by the database unique index, but the resulting database exception is not mapped to a 400, so the client gets the generic 500.
- **Error format:** duplicate ISBN/email messages are returned as a plain text 400 body, while business rule errors from the middleware are JSON.
- No authentication or authorization.

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

### Level 3: business logic (completed)

- `Loan` entity with relationships and the `AddLoans` migration
- Borrow and return endpoints enforcing every business rule
- `GET /api/members/{id}/loans`
- Book filtering (author, availability) and paging
- Global exception handling middleware and structured logging

### Level 4: stretch

Not started.

## Troubleshooting

- **`no such table` during `dotnet ef database update` and no `library.db` appears:** the connection string key in `appsettings.json` must match the name used in `Program.cs` (`Default`). If the key is missing, SQLite silently uses a temporary empty database.
- **`Unable to resolve service for type ...`:** the service is not registered in `Program.cs`, or the registration comes after `builder.Build()`.
- **`dotnet ef` command not found:** install the tool (see Prerequisites) and make sure `~/.dotnet/tools` is on your `PATH`.
- **Cannot delete a book or member:** it has loan history. Loans must be removed directly in the database first (for test data only).

## Screenshots

Swagger screenshots of every endpoint, including 400 and 404 responses, are in the `docs/` folder.