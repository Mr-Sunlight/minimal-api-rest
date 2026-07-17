using API_REST_WITH_MINIMAL_API.Application.Currencies.Commands;
using API_REST_WITH_MINIMAL_API.Application.Currencies.Dtos;
using API_REST_WITH_MINIMAL_API.Application.Currencies.Queries;
using API_REST_WITH_MINIMAL_API.Application.CurrencyConversion;
using FluentValidation;

namespace API_REST_WITH_MINIMAL_API.Endpoints;

/// <summary>
/// Endpoints de monedas + conversión
/// Currencies NO se relaciona con Users/Addresses
/// </summary>
public static class CurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/currencies").WithTags("Currencies");

        // GET /currencies
        group.MapGet("/", async (
            GetCurrenciesQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var currencies = await handler.HandleAsync(new GetCurrenciesQuery(), cancellationToken);
            return Results.Ok(currencies);
        })
        .WithName("GetCurrencies")
        .WithSummary("Listar monedas");

        // GET /currencies/{id}
        group.MapGet("/{id:int}", async (
            int id,
            GetCurrencyByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var currency = await handler.HandleAsync(new GetCurrencyByIdQuery(id), cancellationToken);
            return currency is null
                ? Results.NotFound(new { error = $"Currency with id {id} was not found." })
                : Results.Ok(currency);
        })
        .WithName("GetCurrencyById")
        .WithSummary("Obtener moneda por Id");

        // POST /currencies
        group.MapPost("/", async (
            CreateCurrencyRequest request,
            IValidator<CreateCurrencyCommand> validator,
            CreateCurrencyCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCurrencyCommand(request.Code, request.Name, request.RateToBase);

            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Created($"/currencies/{result.Currency!.Id}", result.Currency);
        })
        .WithName("CreateCurrency")
        .WithSummary("Crear moneda");

        // PUT /currencies/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateCurrencyRequest request,
            IValidator<UpdateCurrencyCommand> validator,
            UpdateCurrencyCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCurrencyCommand(id, request.Name, request.RateToBase);

            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Currency);
        })
        .WithName("UpdateCurrency")
        .WithSummary("Modificar moneda (Name / RateToBase)");

        // DELETE /currencies/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            DeleteCurrencyCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteCurrencyCommand(id), cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.NoContent();
        })
        .WithName("DeleteCurrency")
        .WithSummary("Eliminar moneda");

        // POST /currency/convert 
        app.MapPost("/currency/convert", async (
            ConvertCurrencyRequest request,
            IValidator<ConvertCurrencyCommand> validator,
            ConvertCurrencyCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConvertCurrencyCommand(
                request.FromCurrencyCode,
                request.ToCurrencyCode,
                request.Amount);

            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(result.Result);
        })
        .WithTags("Currencies")
        .WithName("ConvertCurrency")
        .WithSummary("Convertir monto entre monedas (usa RateToBase)");

        return app;
    }
}
