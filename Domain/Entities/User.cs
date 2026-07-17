namespace API_REST_WITH_MINIMAL_API.Domain.Entities;

public class User
{
    public int Id { get; set; } // id 
    public string Name { get; set; } = string.Empty; // Nombre
    public string Email { get; set; } = string.Empty; // Correo
    public bool IsActive { get; set; } = true; // Eliminacion logica: true = activo, false = inactivo
    public string PasswordHash { get; set; } = string.Empty; // Hash de la contraseña
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha de creacion

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
