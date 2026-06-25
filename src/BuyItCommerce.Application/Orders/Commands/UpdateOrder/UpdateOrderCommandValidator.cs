using BuyItCommerce.Application.Orders.Commands;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(command => command.OrderId)
            .NotEmpty().WithMessage("É obrigatório enviar o OrderId");
        RuleFor(command => command.BuyerId)
            .NotEmpty().WithMessage("É obrigatório enviar o BuyerId");
        RuleFor(command => command.BuyerName)
            .NotEmpty().WithMessage("É obrigatório enviar o BuyerName");
        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("O pedido deve conter ao menos um item");
        RuleForEach(command => command.Items).SetValidator(new OrderItemInputValidator());
    }
}
