namespace API_REST_WITH_MINIMAL_API.Application.CurrencyConversion;

/// <summary>
/// Body de POST /currency/convert
/// </summary>
public record ConvertCurrencyRequest(
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal Amount);

/// <summary>
/// Respuesta de la conversión 
/// </summary>
public record ConvertCurrencyResponse(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount);
