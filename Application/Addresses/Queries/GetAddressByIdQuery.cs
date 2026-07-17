using API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Queries;

/// <summary>
/// Trae una sola Address por Id.
/// </summary>
public record GetAddressByIdQuery(int Id);

public class GetAddressByIdQueryHandler(AppDbContext db)
{
    public async Task<AddressResponse?> HandleAsync(
        GetAddressByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // AsNoTracking = solo lectura -- no trackea cambios en memoria
        return await db.Addresses
            .AsNoTracking()
            .Where(a => a.Id == query.Id)
            .Select(a => new AddressResponse(
                a.Id,
                a.UserId,
                a.Street,
                a.City,
                a.Country,
                a.ZipCode))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
