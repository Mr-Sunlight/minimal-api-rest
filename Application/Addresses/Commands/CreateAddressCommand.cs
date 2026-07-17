using API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;
using API_REST_WITH_MINIMAL_API.Domain.Entities;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Commands;

/// <summary>
/// COMMAND = cambia el sistema.
/// Acá creamos una Address ligada a un User existente (FK UserId).
/// </summary>
public record CreateAddressCommand(
    int UserId,
    string Street,
    string City,
    string Country,
    string? ZipCode);

/// <summary>
/// Validación ANTES de tocar la DB.
/// Street/City/Country son requeridos (como en AppDbContext).
/// ZipCode puede venir vacío/null 
/// </summary>
public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId debe ser mayor a 0.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street es requerido.")
            .MaximumLength(300).WithMessage("Street no puede superar 300 caracteres.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City es requerido.")
            .MaximumLength(150).WithMessage("City no puede superar 150 caracteres.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country es requerido.")
            .MaximumLength(150).WithMessage("Country no puede superar 150 caracteres.");

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("ZipCode no puede superar 20 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode));
    }
}

public class CreateAddressCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, AddressResponse? Address, string? Error, int StatusCode)> HandleAsync(
        CreateAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        // relación 1:N --- el User DEBE existir.
        // Si no, devolvemos 404 
        var userExists = await db.Users.AnyAsync(u => u.Id == command.UserId, cancellationToken);
        if (!userExists)
        {
            return (false, null, $"User with id {command.UserId} was not found.", StatusCodes.Status404NotFound);
        }

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Street.Trim(),
            City = command.City.Trim(),
            Country = command.Country.Trim(),
            // Si viene vacío o solo espacios, guardamos null (campo opcional)
            ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(cancellationToken); // INSERT en SQLite

        var response = new AddressResponse(
            address.Id,
            address.UserId,
            address.Street,
            address.City,
            address.Country,
            address.ZipCode);

        return (true, response, null, StatusCodes.Status201Created);
    }
}
