namespace API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;

/// <summary>
/// Body de POST /currencies.
/// Code ejemplo: "USD", "PYG". RateToBase = tasa respecto a la moneda base.
/// </summary>
public record CreateCurrencyRequest(
    string Code,
    string Name,
    decimal RateToBase);

/// <summary>
/// Body de PUT /currencies/{id}.
/// Permite actualizar nombre y tasa (el Code suele no cambiarse).
/// </summary>
public record UpdateCurrencyRequest(
    string Name,
    decimal RateToBase);

/// <summary>
/// Respuesta pública de una moneda.
/// </summary>
public record CurrencyResponse(
    int Id,
    string Code,
    string Name,
    decimal RateToBase);
