using BuyItCommerce.Application.Orders.Contracts;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands;

public sealed class OrderItemInputValidator : AbstractValidator<OrderItemInput>
{
    public OrderItemInputValidator()
    {
        RuleFor(item => item.ProductId).NotEmpty();
        RuleFor(item => item.ProductName).NotEmpty();
        RuleFor(item => item.UnitPrice).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}
