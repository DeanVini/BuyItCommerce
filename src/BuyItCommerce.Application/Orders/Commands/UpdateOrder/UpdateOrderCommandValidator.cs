using BuyItCommerce.Application.Orders.Commands;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.BuyerId).NotEmpty();
        RuleFor(command => command.BuyerName).NotEmpty();
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).SetValidator(new OrderItemInputValidator());
    }
}
