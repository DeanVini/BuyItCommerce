using BuyItCommerce.Application.Orders.Commands;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.BuyerId).NotEmpty();
        RuleFor(command => command.BuyerName).NotEmpty();
        RuleFor(command => command.IdempotencyKey).NotEmpty();
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).SetValidator(new OrderItemInputValidator());
    }
}
