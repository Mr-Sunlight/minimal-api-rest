using API_REST_WITH_MINIMAL_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Infrastructure.Data;

/// <summary>
/// Datos iniciales para poder probar conversión sin crear monedas a mano
/// PYG = moneda base (RateToBase = 1)
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        // Solo si la tabla está vacía (no pisa datos que ya se crearon)
        if (await db.Currencies.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Currencies.AddRange(
            new Currency { Code = "PYG", Name = "Guaraní paraguayo", RateToBase = 1m },
            new Currency { Code = "USD", Name = "Dólar estadounidense", RateToBase = 7300m },
            new Currency { Code = "EUR", Name = "Euro", RateToBase = 7900m });

        await db.SaveChangesAsync(cancellationToken);
    }
}
