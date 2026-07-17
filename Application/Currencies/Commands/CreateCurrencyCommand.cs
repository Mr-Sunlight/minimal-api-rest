using API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;
using API_REST_WITH_MINIMAL_API.Domain.Entities;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Commands;

/// <summary>
/// Crea una moneda en la tabla Currencies, esto no tiene ninguna relacion con Users y Addresses
/// </summary>
public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase);

/// <summary>
/// Reglas de la prueba:
/// - code no vacío
/// - name no vacío
/// - rateToBase &gt; 0
/// (unicidad de Code se valida en el handler contra la DB)
/// </summary>
public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code es requerido.")
            .MaximumLength(10).WithMessage("Code no puede superar 10 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name es requerido.")
            .MaximumLength(100).WithMessage("Name no puede superar 100 caracteres.");

        RuleFor(x => x.RateToBase)
            .GreaterThan(0).WithMessage("RateToBase debe ser mayor a 0.");
    }
}

public class CreateCurrencyCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, CurrencyResponse? Currency, string? Error, int StatusCode)> HandleAsync(
        CreateCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Normalizacion de mayusculas: "usd" a "USD"
        var code = command.Code.Trim().ToUpperInvariant();

        // Code único (índice único en AppDbContext)
        var exists = await db.Currencies.AnyAsync(c => c.Code == code, cancellationToken);
        if (exists)
        {
            return (false, null, $"Ya existe una moneda con code '{code}'.", StatusCodes.Status409Conflict);
        }

        var currency = new Currency
        {
            Code = code,
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase
        };

        db.Currencies.Add(currency);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CurrencyResponse(currency.Id, currency.Code, currency.Name, currency.RateToBase);
        return (true, response, null, StatusCodes.Status201Created);
    }
}
