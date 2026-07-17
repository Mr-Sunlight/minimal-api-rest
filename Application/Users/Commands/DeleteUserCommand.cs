using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Commands;

public record DeleteUserCommand(int Id);

public class DeleteUserCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, string? Error, int StatusCode)> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user is null)
        {
            return (false, $"User with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }

        // Borrado físico. Como Address tiene Cascade, también se borran sus direcciones.
        // Borrado Logico en SoftDeleteUserCommand.
        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return (true, null, StatusCodes.Status204NoContent);
    }
}