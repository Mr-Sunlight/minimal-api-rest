using API_REST_WITH_MINIMAL_API.Application.Users.Dtos;
using API_REST_WITH_MINIMAL_API.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_REST_WITH_MINIMAL_API.Application.Users.Commands;

public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive);

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}

public class UpdateUserCommandHandler(AppDbContext db)
{
    public async Task<(bool Success, UserResponse? User, string? Error, int StatusCode)> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user is null)
        {
            return (false, null, $"User with id {command.Id} was not found.", StatusCodes.Status404NotFound);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        // Si cambia el email, verificamos que no lo use otro usuario
        var emailTaken = await db.Users.AnyAsync(
            u => u.Email == normalizedEmail && u.Id != command.Id,
            cancellationToken);

        if (emailTaken)
        {
            return (false, null, "A user with this email already exists.", StatusCodes.Status409Conflict);
        }

        user.Name = command.Name.Trim();
        user.Email = normalizedEmail;
        user.IsActive = command.IsActive;

        await db.SaveChangesAsync(cancellationToken); // UPDATE en SQLite

        var response = new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.CreatedAt);

        return (true, response, null, StatusCodes.Status200OK);
    }
}
