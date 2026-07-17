using System.Text.RegularExpressions;
using API_REST_WITH_MINIMAL_API.Application.Users.Commands;
using API_REST_WITH_MINIMAL_API.Application.Users.Dtos;
using API_REST_WITH_MINIMAL_API.Application.Users.Queries;
using FluentValidation;

namespace API_REST_WITH_MINIMAL_API.Endpoints;

/// <summary>
/// Agrupa todos los endpoints de Users.
/// Así Program.cs queda limpio: solo llamamos MapUserEndpoints().
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        // POST /users  → crear
        group.MapPost("/", async (
            CreateUserRequest request,
            IValidator<CreateUserCommand> validator,
            CreateUserCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateUserCommand(request.Name, request.Email);

            // Validar con FluentValidation
            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            // Ejecutar el command
            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            // 201 Created mas Location header apuntando al nuevo recurso creado
            return Results.Created($"/users/{result.User!.Id}", result.User);
        })
        .WithName("CreateUser")
        .WithSummary("Crear un usuario");

        // GET /users?isActive=true|false  -- listar (filtro)
        group.MapGet("/", async (
            bool? isActive, // Filtro por si se requiere 
            GetUsersQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var users = await handler.HandleAsync(new GetUsersQuery(isActive), cancellationToken);
            return Results.Ok(users);
        })
        .WithName("GetUsers")
        .WithSummary("Listar usuarios (opcional: ?isActive=true|false)");

        // GET /users/{id}  -- obtener usuario 
        group.MapGet("/{id:int}", async (
            int id,
            GetUserByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var user = await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
            return user is null
                ? Results.NotFound(new { error = $"User with id {id} was not found." })
                : Results.Ok(user);
        })
        .WithName("GetUserById")
        .WithSummary("Obtener usuario por Id");

        // PUT /users/{id}  -- modificar
        group.MapPut("/{id:int}", async (
            int id,
            UpdateUserRequest request,
            IValidator<UpdateUserCommand> validator,
            UpdateUserCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserCommand(id, request.Name, request.Email, request.IsActive);

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

            return Results.Ok(result.User);
        })
        .WithName("UpdateUser")
        .WithSummary("Modificar usuario");

        // DELETE /users/{id}  -- eliminar FORMA FISICA
        group.MapDelete("/{id:int}", async (
            int id,
            DeleteUserCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteUserCommand(id), cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.NoContent(); // 204: borrado OK, sin body
        })
        .WithName("DeleteUser")
        .WithSummary("Eliminar usuario");

        // Soft Delete /users/{id}/soft  -- eliminar FORMA LOGICA
        group.MapDelete("/{id:int}/soft", async (
            int id,
            SoftDeleteUserCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new SoftDeleteUserCommand(id), cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }
            return Results.NoContent(); // 204: borrado OK, sin body
        })
        .WithName("SoftDeleteUser")
        .WithSummary("Eliminar usuario FORMA LOGICA");

        return app;
    }
}
