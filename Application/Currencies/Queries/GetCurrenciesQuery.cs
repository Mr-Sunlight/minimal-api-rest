using API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Queries;

/// <summary>
/// Lista todas las monedas. GET /currencies
/// </summary>
public record GetCurrenciesQuery;

public class GetCurrenciesQueryHandler(AppDbContext db)
{
    public async Task<List<CurrencyResponse>> HandleAsync(
        GetCurrenciesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await db.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase))
            .ToListAsync(cancellationToken);
    }
}
