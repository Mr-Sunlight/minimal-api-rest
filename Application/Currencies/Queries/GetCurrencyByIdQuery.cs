using API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Queries;

public record GetCurrencyByIdQuery(int Id);

public class GetCurrencyByIdQueryHandler(AppDbContext db)
{
    public async Task<CurrencyResponse?> HandleAsync(
        GetCurrencyByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return await db.Currencies
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
