using BuyItCommerce.Application.Orders.Commands;
using FluentValidation;

namespace BuyItCommerce.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.BuyerId)
            .NotEmpty().WithMessage("É obrigatório enviar o BuyerId");
        RuleFor(command => command.BuyerName)
            .NotEmpty().WithMessage("É obrigatório enviar o BuyerName");
        RuleFor(command => command.IdempotencyKey)
            .NotEmpty().WithMessage("É obrigatório enviar o cabeçalho Idempotency-Key");
        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("O pedido deve conter ao menos um item");
        RuleForEach(command => command.Items).SetValidator(new OrderItemInputValidator());
    }
}
