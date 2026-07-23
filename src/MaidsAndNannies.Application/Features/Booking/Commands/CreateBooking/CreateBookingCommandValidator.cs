using FluentValidation;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.WorkerId).GreaterThan(0);
        RuleFor(x => x.MonthlySalary).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}