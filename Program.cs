using LibraryManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Services;



var builder = WebApplication.CreateBuilder(args);
/*creates the object that sets up your app. args are command-line arguments, and var lets the compiler work out the type. The builder holds configuration, logging, the web server settings and the service container.*/

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();//lets Swagger discover your endpoints.

builder.Services.AddSwaggerGen();//generates the OpenAPI document, which is a JSON description of your API.

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddScoped<IMemberService, MemberService>();

var app = builder.Build();//finishes the registration phase and creates the app.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();//serves the OpenAPI JSON file.
    app.UseSwaggerUI();//serves the interactive page at /swagger.
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();//connects your controllers' routes (like api/books) to the app. Without it, every URL returns 404.

app.Run();
