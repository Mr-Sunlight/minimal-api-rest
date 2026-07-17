using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Commands;


public record SoftDeleteUserCommand(int Id);

public class SoftDeleteUserCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, string? Error, int StatusCode)> HandleAsync(
        SoftDeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id && u.IsActive, cancellationToken);
        if (user is null)
        {
            return (false, $"User with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }
        // Borrado logico. 
        // Borrado fisico en DeleteUserCommand.
        // En el borrado logico se mantiene todos los registros como direcciones asociadas al usuario 
        user.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return (true, null, StatusCodes.Status204NoContent);
    }
}