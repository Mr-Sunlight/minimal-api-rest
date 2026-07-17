using API_REST_WITH_MINIMAL_API.Application.Users.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Queries;

/// <summary>
/// QUERY = operación que SOLO LEE datos (no modifica nada)
/// isActive es opcional: null = todos, true/false = filtrados
/// </summary>
public record GetUsersQuery(bool? IsActive);

public class GetUsersQueryHandler(AppDbContext db)
{
    public async Task<List<UserResponse>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        // Empezamos con un IQueryable: todavía no se ejecuta SQL
        var usersQuery = db.Users.AsNoTracking().AsQueryable();

        // Filtro opcional GET /users?isActive=true
        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
        }
        // Acá recién se ejecuta el SELECT en SQLite
        return await usersQuery
            .OrderBy(u => u.Id)
            .Select(u => new UserResponse(
                u.Id,
                u.Name,
                u.Email,
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
