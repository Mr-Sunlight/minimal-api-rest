using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.CurrencyConversion;

/// <summary>
/// Convierte un monto entre dos monedas usando RateToBase.
/// No modifica la DB, conceptualmente es una Query, pero la prueba pide POST
/// </summary>
public record ConvertCurrencyCommand(
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal Amount);

public class ConvertCurrencyCommandValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyCommandValidator()
    {
        RuleFor(x => x.FromCurrencyCode)
            .NotEmpty().WithMessage("codigo de monera de origen es requerido.");

        RuleFor(x => x.ToCurrencyCode)
            .NotEmpty().WithMessage("codigo de moneda de destino es requerido.");

        // amount > 0 (regla que esta en la prueba)
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount debe ser mayor a 0.");
    }
}

public class ConvertCurrencyCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, ConvertCurrencyResponse? Result, string? Error, int StatusCode)> HandleAsync(
        ConvertCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        var fromCode = command.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = command.ToCurrencyCode.Trim().ToUpperInvariant();

        // Buscamos ambas monedas en Currencies
        var from = await db.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == fromCode, cancellationToken);

        var to = await db.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == toCode, cancellationToken);

        if (from is null)
        {
            return (false, null, $"Moneda '{fromCode}' no encontrada.", StatusCodes.Status404NotFound);
        }

        if (to is null)
        {
            return (false, null, $"Moneda '{toCode}' no encontrada.", StatusCodes.Status404NotFound);
        }

        // Fórmula de la prueba técnica:
        //   1.pasar el monto a moneda base:  montoBase = amount * from.RateToBase
        //   2.pasar de base a destino: converted  = montoBase / to.RateToBase
        //
        // Ejemplo (PYG base = 1, USD RateToBase = 7300):
        //   100 USD = 100 * 7300 = 730000 PYG = 730000 / 1 = 730000 PYG
        var amountInBase = command.Amount * from.RateToBase;
        var convertedAmount = amountInBase / to.RateToBase;

        var response = new ConvertCurrencyResponse(
            from.Code,
            to.Code,
            command.Amount,
            decimal.Round(convertedAmount, 2)); // redondeo con dos decimales

        return (true, response, null, StatusCodes.Status200OK);
    }
}
