using System.Reflection;
using API_REST_WITH_MINIMAL_API.Application.Addresses.Commands;
using API_REST_WITH_MINIMAL_API.Application.Addresses.Queries;
using API_REST_WITH_MINIMAL_API.Application.Currencies.Commands;
using API_REST_WITH_MINIMAL_API.Application.Currencies.Queries;
using API_REST_WITH_MINIMAL_API.Application.CurrencyConversion;
using API_REST_WITH_MINIMAL_API.Application.Users.Commands;
using API_REST_WITH_MINIMAL_API.Application.Users.Queries;
using API_REST_WITH_MINIMAL_API.Endpoints;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using API_REST_WITH_MINIMAL_API.Infrastructure.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation — busca automáticamente clases AbstractValidator<> en este proyecto
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Handlers CQRS de Users (inyección de dependencias)
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<SoftDeleteUserCommandHandler>();
builder.Services.AddScoped<GetUsersQueryHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();

// Handlers CQRS de Addresses — sin registrar → Swagger/DI fallan (Body inferred)
builder.Services.AddScoped<CreateAddressCommandHandler>();
builder.Services.AddScoped<UpdateAddressCommandHandler>();
builder.Services.AddScoped<DeleteAddressCommandHandler>();
builder.Services.AddScoped<GetAddressesQueryHandler>();
builder.Services.AddScoped<GetAddressByIdQueryHandler>();

// Handlers CQRS de Currencies + conversión
builder.Services.AddScoped<CreateCurrencyCommandHandler>();
builder.Services.AddScoped<UpdateCurrencyCommandHandler>();
builder.Services.AddScoped<DeleteCurrencyCommandHandler>();
builder.Services.AddScoped<GetCurrenciesQueryHandler>();
builder.Services.AddScoped<GetCurrencyByIdQueryHandler>();
builder.Services.AddScoped<ConvertCurrencyCommandHandler>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API REST with Minimal API",
        Version = "v1",
        Description = "CRUD Users/Addresses + Currency conversion. Requires X-API-KEY header."
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Example: test-api-key-12345",
        Name = "X-API-KEY",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Swashbuckle 10 requires a document delegate for security requirements
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

var app = builder.Build();

// Crea la DB SQLite (app.db) y las tablas si no existen.
// con migraciones EF, se puede usar: db.Database.Migrate();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Seed PYG/USD/EUR 
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Endpoints de Users (POST/GET/PUT/DELETE /users)
app.MapUserEndpoints();

// Endpoints de Addresses (POST/GET/PUT/DELETE /addresses)
app.MapAddressEndpoints();

// Endpoints de Currencies + POST /currency/convert
app.MapCurrencyEndpoints();

app.Run();
