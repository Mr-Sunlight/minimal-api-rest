namespace API_REST_WITH_MINIMAL_API.Domain.Entities;

public class Address
{
    public int Id { get; set; } // id
    public int UserId { get; set; } // id del usuario - VENDRA DE LA ENTIDAD USER
    public string Street { get; set; } = string.Empty; // Calle
    public string City { get; set; } = string.Empty; // Ciudad
    public string Country { get; set; } = string.Empty; // Pais
    public string? ZipCode { get; set; } // Codigo postal

    public User User { get; set; } = null!; // Usuario - Viene de su propia entidad
}
