using BuyItCommerce.Application.Orders.Contracts;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands;

public sealed class OrderItemInputValidator : AbstractValidator<OrderItemInput>
{
    public OrderItemInputValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty().WithMessage("É obrigatório enviar o ProductId");
        RuleFor(item => item.ProductName)
            .NotEmpty().WithMessage("É obrigatório enviar o ProductName");
        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero");
        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero");
    }
}
