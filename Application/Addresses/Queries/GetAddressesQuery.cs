using API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Queries;

/// <summary>
/// QUERY = solo lectura.
/// UserId opcional: null = todas, con valor = solo las de ese usuario.
/// Ejemplo: GET /addresses?userId=1
/// </summary>
public record GetAddressesQuery(int? UserId);

public class GetAddressesQueryHandler(AppDbContext db)
{
    public async Task<List<AddressResponse>> HandleAsync(
        GetAddressesQuery query,
        CancellationToken cancellationToken = default)
    {
        // IQueryable: todavía no ejecuta SQL; armamos el filtro primero
        var addressesQuery = db.Addresses.AsNoTracking().AsQueryable();

        // Filtro por dueño (relación 1:N User → Addresses)
        if (query.UserId.HasValue)
        {
            addressesQuery = addressesQuery.Where(a => a.UserId == query.UserId.Value);
        }

        return await addressesQuery
            .OrderBy(a => a.Id)
            .Select(a => new AddressResponse(
                a.Id,
                a.UserId,
                a.Street,
                a.City,
                a.Country,
                a.ZipCode))
            .ToListAsync(cancellationToken);
    }
}
