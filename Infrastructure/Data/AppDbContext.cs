using API_REST_WITH_MINIMAL_API.Domain.Entities; //Importamos las entidades 
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Infrastructure.Data; // Namespace de la clase

// Configuracioin de las tablas indicies y relaciones 

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>(); // Acceso a  usuarios
    public DbSet<Address> Addresses => Set<Address>(); // Acceso a direcciones
    public DbSet<Currency> Currencies => Set<Currency>(); // Acceso a moneda 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity => //Configuracion de la tabla usuarios
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique(); // Indice unico para el correo
            entity.Property(u => u.PasswordHash).IsRequired(); 
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Address>(entity => //Configuracion de la tabla direcciones
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Street).IsRequired().HasMaxLength(300);
            entity.Property(a => a.City).IsRequired().HasMaxLength(150);
            entity.Property(a => a.Country).IsRequired().HasMaxLength(150);
            entity.Property(a => a.ZipCode).HasMaxLength(20);

            entity.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina el usuario se borran tambien las direcciones. Metodo cascade
        });

        modelBuilder.Entity<Currency>(entity => //Configuracion de la tabla moneda
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Code).IsRequired().HasMaxLength(10); // Codigo de la moneda
            entity.HasIndex(c => c.Code).IsUnique(); // Indice unico para el codigo de la moneda
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.RateToBase).HasPrecision(18, 8); // Precision de la tasa de la moneda
        });
    }
}
