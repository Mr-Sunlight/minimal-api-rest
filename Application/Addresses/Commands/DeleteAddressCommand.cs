using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Commands;

/// <summary>
/// Borrado físico de una Address.
/// No hay soft delete acá: la entidad Address no tiene IsActive.
/// </summary>
public record DeleteAddressCommand(int Id);

public class DeleteAddressCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, string? Error, int StatusCode)> HandleAsync(
        DeleteAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (address is null)
        {
            return (false, $"Address with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(cancellationToken); // DELETE en SQLite

        return (true, null, StatusCodes.Status204NoContent);
    }
}
