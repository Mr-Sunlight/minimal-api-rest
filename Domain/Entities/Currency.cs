namespace API_REST_WITH_MINIMAL_API.Domain.Entities;

public class Currency
{
    public int Id { get; set; } // id 
    public string Code { get; set; } = string.Empty; // Codigo
    public string Name { get; set; } = string.Empty; // Nombre
    /// <summary>
    /// Tasa relativa a la moneda base (Ejemplo: PYG = 1).
    /// </summary>
    public decimal RateToBase { get; set; }
}
