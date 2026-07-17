using API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Commands;

/// <summary>
/// Actualiza los datos de una dirección existente.
/// Id viene de la ruta PUT /addresses/{id}.
/// </summary>
public record UpdateAddressCommand(
    int Id,
    string Street,
    string City,
    string Country,
    string? ZipCode);

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id debe ser mayor a 0.");

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

public class UpdateAddressCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, AddressResponse? Address, string? Error, int StatusCode)> HandleAsync(
        UpdateAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (address is null)
        {
            return (false, null, $"Address with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }

        // Solo modificamos campos de la dirección.
        // UserId queda igual (sigue perteneciendo al mismo usuario).
        address.Street = command.Street.Trim();
        address.City = command.City.Trim();
        address.Country = command.Country.Trim();
        address.ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim();

        await db.SaveChangesAsync(cancellationToken); // UPDATE en SQLite

        var response = new AddressResponse(
            address.Id,
            address.UserId,
            address.Street,
            address.City,
            address.Country,
            address.ZipCode);

        return (true, response, null, StatusCodes.Status200OK);
    }
}
