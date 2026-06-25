using Asp.Versioning;
using Asp.Versioning.Builder;
using BuyItCommerce.Api.Contracts;
using BuyItCommerce.Application.Orders.Commands.CancelOrder;
using BuyItCommerce.Application.Orders.Commands.CreateOrder;
using BuyItCommerce.Application.Orders.Commands.ProcessOrder;
using BuyItCommerce.Application.Orders.Commands.ShipOrder;
using BuyItCommerce.Application.Orders.Commands.UpdateOrder;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Application.Orders.Queries.GetOrderById;
using BuyItCommerce.Application.Orders.Queries.ListOrders;
using BuyItCommerce.Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyItCommerce.Api.Endpoints;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var orders = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1))
            .WithTags("Orders");

        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Cria um novo pedido de forma idempotente.")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapGet("/", ListOrders)
            .WithName("ListOrders")
            .WithSummary("Lista pedidos com filtro por status e comprador.")
            .Produces<PagedResult<OrderListItemResponse>>();

        orders.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("Busca um pedido por identificador.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        orders.MapPut("/{id:guid}", UpdateOrder)
            .WithName("UpdateOrder")
            .WithSummary("Altera um pedido (somente quando iniciado).")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapPost("/{id:guid}/process", ProcessOrder)
            .WithName("ProcessOrder")
            .WithSummary("Processa um pedido iniciado.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .WithSummary("Envia um pedido processado.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        orders.MapPost("/{id:guid}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .WithSummary("Cancela um pedido iniciado ou processado.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Cabeçalho Idempotency-Key é obrigatório.");
        }

        var command = new CreateOrderCommand(request.BuyerId, request.BuyerName, request.Items, idempotencyKey);
        var response = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/v1/orders/{response.Id}", response);
    }

    private static async Task<IResult> ListOrders(
        ISender sender,
        CancellationToken cancellationToken,
        EOrderStatus? status = null,
        Guid? buyerId = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new ListOrdersQuery(status, buyerId, page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrderById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return response is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Pedido não encontrado.")
            : Results.Ok(response);
    }

    private static async Task<IResult> UpdateOrder(
        Guid id,
        UpdateOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderCommand(id, request.BuyerId, request.BuyerName, request.Items);
        var response = await sender.Send(command, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> ProcessOrder(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new ProcessOrderCommand(id), cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> ShipOrder(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new ShipOrderCommand(id), cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> CancelOrder(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new CancelOrderCommand(id), cancellationToken);
        return Results.Ok(response);
    }
}
