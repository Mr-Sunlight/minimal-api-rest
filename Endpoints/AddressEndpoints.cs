using API_REST_WITH_MINIMAL_API.Application.Addresses.Commands;
using API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;
using API_REST_WITH_MINIMAL_API.Application.Addresses.Queries;
using FluentValidation;

namespace API_REST_WITH_MINIMAL_API.Endpoints;

/// <summary>
/// Endpoints HTTP del recurso Addresses
/// Misma idea que UserEndpoints: Program.cs solo llama MapAddressEndpoints()
///
/// Relación 1:N:
///   User (1) ----&lt; Address (N)
/// Cada Address tiene un UserId (FK) apuntando a su dueño
/// </summary>
public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/addresses").WithTags("Addresses");

        // POST /addresses  → crear dirección para un user
        group.MapPost("/", async (
            CreateAddressRequest request,
            IValidator<CreateAddressCommand> validator,
            CreateAddressCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateAddressCommand(
                request.UserId,
                request.Street,
                request.City,
                request.Country,
                request.ZipCode);

            // 1) Validar formato/reglas
            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            // Ejecutar lógica (incluye check de que el User exista)
            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            // 201 + Location header hacia el recurso creado
            return Results.Created($"/addresses/{result.Address!.Id}", result.Address);
        })
        .WithName("CreateAddress")
        .WithSummary("Crear una dirección (requiere UserId existente)");

        // GET /addresses?userId=1  → listar (filtro opcional por usuario)
        group.MapGet("/", async (
            int? userId,
            GetAddressesQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var addresses = await handler.HandleAsync(new GetAddressesQuery(userId), cancellationToken);
            return Results.Ok(addresses);
        })
        .WithName("GetAddresses")
        .WithSummary("Listar direcciones (opcional: ?userId=1)");

        // GET /addresses/{id}  → una dirección
        group.MapGet("/{id:int}", async (
            int id,
            GetAddressByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var address = await handler.HandleAsync(new GetAddressByIdQuery(id), cancellationToken);
            return address is null
                ? Results.NotFound(new { error = $"Address with id {id} was not found." })
                : Results.Ok(address);
        })
        .WithName("GetAddressById")
        .WithSummary("Obtener dirección por Id");

        // PUT /addresses/{id}  → modificar
        group.MapPut("/{id:int}", async (
            int id,
            UpdateAddressRequest request,
            IValidator<UpdateAddressCommand> validator,
            UpdateAddressCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateAddressCommand(
                id,
                request.Street,
                request.City,
                request.Country,
                request.ZipCode);

            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Address);
        })
        .WithName("UpdateAddress")
        .WithSummary("Modificar una dirección");

        // DELETE /addresses/{id}  → borrar (físico)
        group.MapDelete("/{id:int}", async (
            int id,
            DeleteAddressCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteAddressCommand(id), cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.NoContent(); // 204 sin body
        })
        .WithName("DeleteAddress")
        .WithSummary("Eliminar una dirección");

        return app;
    }
}
