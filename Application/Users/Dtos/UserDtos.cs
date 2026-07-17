namespace API_REST_WITH_MINIMAL_API.Application.Users.Dtos;

/// <summary>
/// Body que recibe POST /users
/// PasswordHash se genera internamente (en CreateUserCommandHandler)
/// </summary>
public record CreateUserRequest(string Name, string Email);

/// <summary>
/// Body que recibe PUT /users/{id}
/// Permite actualizar name, email e isActive
/// </summary>
public record UpdateUserRequest(string Name, string Email, bool IsActive);

/// <summary>
/// Lo que devolvemos al cliente
/// no devolvemos el password hash
/// </summary>
public record UserResponse(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt);
