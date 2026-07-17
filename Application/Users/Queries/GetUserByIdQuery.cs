using API_REST_WITH_MINIMAL_API.Application.Users.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Queries;

public record GetUserByIdQuery(int Id);

public class GetUserByIdQueryHandler(AppDbContext db)
{
    public async Task<UserResponse?> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // AsNoTracking = solo lectura
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(u => new UserResponse(
                u.Id,
                u.Name,
                u.Email,
                u.IsActive,
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
