using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Commands;

public record DeleteCurrencyCommand(int Id);

public class DeleteCurrencyCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, string? Error, int StatusCode)> HandleAsync(
        DeleteCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (currency is null)
        {
            return (false, $"Currency with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }

        db.Currencies.Remove(currency);
        await db.SaveChangesAsync(cancellationToken);

        return (true, null, StatusCodes.Status204NoContent);
    }
}
