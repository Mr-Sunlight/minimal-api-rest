namespace API_REST_WITH_MINIMAL_API.Application.Addresses.Dtos;

/// <summary>
/// Body de POST /addresses.
/// UserId es obligatorio porque Address pertenece a un User (relación 1:N).
/// ZipCode es opcional (nullable).
/// </summary>
public record CreateAddressRequest(
    int UserId,
    string Street,
    string City,
    string Country,
    string? ZipCode);

/// <summary>
/// Body de PUT /addresses/{id}.
/// No incluimos UserId: normalmente no se mueve una dirección de un user a otro.
/// Si hiciera falta, sería otra operación (reassign).
/// </summary>
public record UpdateAddressRequest(
    string Street,
    string City,
    string Country,
    string? ZipCode);

/// <summary>
/// Respuesta que devolvemos al cliente.
/// Incluye UserId para que se vea claramente a qué usuario pertenece.
/// </summary>
public record AddressResponse(
    int Id,
    int UserId,
    string Street,
    string City,
    string Country,
    string? ZipCode);
