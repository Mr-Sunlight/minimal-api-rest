using API_REST_WITH_MINIMAL_API.Application.Users.Dtos;
using API_REST_WITH_MINIMAL_API.Domain.Entities;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Commands;

/// <summary>
/// COMMAND = operación que CAMBIA el estado del sistema (crear, editar, borrar)
/// Este record solo transporta los datos necesarios para crear un usuario
/// </summary>
public record CreateUserCommand(string Name, string Email);

/// <summary>
/// Reglas de FluentValidation para crear usuario
/// Se ejecutan ANTES de tocar la base de datos
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido."); // Nombre requerido

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El Email es requerido.") // Correo requerido
            .EmailAddress().WithMessage("Formato de Email invalido."); // Formato del correo invalido
    }
}

/// <summary>
/// Handler = la logica que ejecuta el comando.
/// </summary>
public class CreateUserCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, UserResponse? User, string? Error, int StatusCode)> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Email unico: si ya existe, no creamos otro
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == command.Email, cancellationToken);

        if (emailExists)
        {
            return (false, null, "Ya existe un usuario con este correo.", StatusCodes.Status409Conflict);
        }

        // La prueba no pide password en el body, pero la entidad lo requiere 
        // Generamos un hash de una contraseña temporal por defecto 
        var user = new User
        {
            Name = command.Name.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Wooola123!"),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken); // aqui se hace INSERT en SQLite

        var response = new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.CreatedAt);

        return (true, response, null, StatusCodes.Status201Created);
    }
}
