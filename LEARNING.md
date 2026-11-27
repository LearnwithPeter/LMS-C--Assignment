# Learning Journal
## Day 0: Level 0 (C# basics and fundamentals)

**New concept:** I learned the basic code structure of C# , how to set it up , how to create Console app . I learned about classes , objects , access modifiers and LINQ . I brushed up OOPS concepts all four are clear but i always had difficulty with understanding Runtime Polyorphism (Method Overriding) i don't understant the virtual class thing and all.

**Hardest error:** I installed wrong version of .NET , suppose to install 8 but because of the Docs installed 10.It's not a error but i find difficulty in finding resources of this verion.

**Sources:** Claude (AI assistant) for step-by-step explanations of each line, and for fixing my errors.Used Microsoft Learn to learn C# basics , OOPs , LINQ and all .



## Day 1: Level 1 (basic CRUD with an in-memory list)

**New concept:** I learned how a request travels through an ASP.NET Core API. Swagger sends a GET, POST, PUT or DELETE, the route (`api/[controller]`) picks a controller method, and the method returns a status code like 200, 201, 204, 400 or 404. I also learned that ASP.NET makes a new controller object for every request, which is why my list of books had to be `static`. A normal field would be empty every time. `[ApiController]` also gave me automatic 400 errors when `[Required]` or `[Range]` rules fail, so I didn't have to write those checks myself.

**Hardest error:** `CS0106: The modifier 'public' is not valid for this item`. It confused me because the line it pointed at looked fine. The real problem was a missing closing `}` on the method above it, so my new method was sitting inside the old one. I fixed it by clicking on the opening brace to see where its matching brace was.

**Sources:** Claude (AI assistant) for step-by-step explanations of each line, and for fixing my errors. I ran every endpoint in Swagger myself to check the status codes. [Add any docs or videos you really opened, with links.]

**What I'd do differently:** I'd read compiler errors more slowly, and check the line above the one the error points to.

**Still unsure about:** What `UseHttpsRedirection()` and `UseAuthorization()` really do in `Program.cs` when I'm not using HTTPS or logins yet. I know where they sit in the pipeline, but I haven't used them for anything.

---

## Day 2: Level 2, part 1 (EF Core, SQLite, migrations)

**New concept:** Entity Framework Core lets me work with C# classes instead of writing SQL. The `DbContext` is my session with the database, and each `DbSet` is a table. A migration is like version control for the database structure: `migrations add` writes the change as code, and `database update` applies it to `library.db`. I also added unique indexes on ISBN and email, so the database refuses duplicates even if my code misses one.

**Hardest error:** `dotnet ef database update` failed with `SQLite Error 1: 'no such table: __EFMigrationsHistory'`, even though the log showed the tables being created. No `library.db` file appeared either. I tried resetting the database, checking my EF tool version, and using an absolute path, and none of it worked. What helped was running the migration's generated SQL directly in `sqlite3`, which worked fine, so I knew the migration itself was good. Then `dotnet ef migrations list` said the migration was still Pending even after the history row existed. That meant EF was looking at a different database. I finally printed my `appsettings.json` and saw the key was `DefaultConnection`, while `Program.cs` asked for `Default`. The connection string was null, and SQLite quietly used a temporary empty database. I renamed the key, and it worked.

**Sources:** Claude (AI assistant) walked me through the setup and helped me narrow the problem down step by step. 
**What I'd do differently:** When something fails strangely, I'd check configuration names first, because a wrong name doesn't give a clear error.

**Still unsure about:** Why exactly SQLite gives each connection a fresh empty database when the connection string is empty. I understand the result, but not the reason behind it.

---

## Day 3: Level 2, part 2 (DTOs, services, dependency injection, Members)

**New concept:** I learned why we have three kinds of classes: the entity (`Member`, a database row), a request class (`MemberRequest`, what a client may send) and a DTO (`MemberDto`, what the API returns). The server sets `Id` and `JoinedOn`, so a client can't set them. I also learned dependency injection: my controller asks for an `IMemberService` in its constructor and ASP.NET supplies it, because I registered it with `AddScoped`. I started to understand `async` and `await`: `await` lets the server use the thread for other requests while the database is busy.

**Hardest error:** `Unable to resolve service for type 'IBookService'`. I had written the service and the controller but the `AddScoped` line wasn't in `Program.cs` (or was after `builder.Build()`). I also made my own mistakes writing `MemberService` and `MembersController` by myself: I forgot to copy `IsActive` in both `CreateAsync` and `UpdateAsync`, wrote `db = _db` backwards in the constructor, missed a semicolon, left `ActionResult<BookDto>` in the members controller from copy-paste, and used `Controller` instead of `ControllerBase`. The compiler didn't catch the missing `IsActive`, so I had to compare my code with the request class.

**Sources:** Claude (AI assistant) for the Book version and explanations. I wrote `MemberService` and `MembersController` myself using the Book versions as a template, and Claude reviewed them and pointed out the bugs. [Add any docs you really opened, with links.]

**What I'd do differently:** After copying a pattern, I'd check each field of the request class against the mapping code, and search for leftover names from the old file.

**Still unsure about:** Why the DbContext should be scoped and not a singleton. I know it isn't safe to share between requests, but I can't fully explain what would go wrong. I'm also not fully sure how `async` works under the hood.

---

## Day 4: Level 3 (business rules, middleware, logging, paging)

**New concept:** The services now throw exceptions (`NotFoundException`, `BusinessRuleException`), and one global middleware catches them and turns them into a 404 or 400 with a clear message. It must be registered first so its `try/catch` wraps everything. For a borrow, I check the rules in order (exists, active, max 3 loans, copies available) and then save the new loan and the reduced copy count with one `SaveChangesAsync`, so EF does both in one transaction. I learned that `Include` is needed to load a loan's book and member, and that paging needs an `OrderBy`, or pages can repeat or skip rows.

**Hardest error:** I got `400: Cannot delete a member who has loan history` and thought something was broken. It was actually my own rule working, because loans use `DeleteBehavior.Restrict`. I also noticed ids kept going up after I deleted and re-created books. That's SQLite's `AUTOINCREMENT`, which never reuses ids, so old loan records can't point at a different book. Later, changing `GetAllAsync` to `GetPagedAsync` broke the build, because the interface still listed the old method and `BookService` no longer had it.

**Sources:** Claude (AI assistant) for the design, the code and the explanations. I tested every rule in Swagger myself, including each 400 and 404. 

**What I'd do differently:** I'd write down the test data and the expected results before testing, and change an interface and everything that uses it in one go.

**Still unsure about:** The race condition when two people borrow the last copy at the same instant. I know the check and the update aren't atomic, but I don't yet know how a concurrency token would fix it.