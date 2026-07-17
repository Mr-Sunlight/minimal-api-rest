using API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Commands;

/// <summary>
/// Actualiza Name y RateToBase de una moneda existente.
/// </summary>
public record UpdateCurrencyCommand(int Id, string Name, decimal RateToBase);

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("id debe ser mayor a 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("nombre es requerido.")
            .MaximumLength(100).WithMessage("nombre no puede superar 100 caracteres.");

        RuleFor(x => x.RateToBase)
            .GreaterThan(0).WithMessage("tasa debe ser mayor a 0.");
    }
}

public class UpdateCurrencyCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, CurrencyResponse? Currency, string? Error, int StatusCode)> HandleAsync(
        UpdateCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (currency is null)
        {
            return (false, null, $"Currency with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }
        // Actualizacion de nombre y tasa
        currency.Name = command.Name.Trim();
        currency.RateToBase = command.RateToBase;
        await db.SaveChangesAsync(cancellationToken);

        var response = new CurrencyResponse(currency.Id, currency.Code, currency.Name, currency.RateToBase);
        return (true, response, null, StatusCodes.Status200OK);
    }
}
