using Domain.Commands;
using Domain.Models;
using FluentValidation;

namespace Domain.Validators;

public class AddLoanApplicationCommandValidator : AbstractValidator<AddGenericCommand<LoanApplication>>
{
    public AddLoanApplicationCommandValidator()
    {
        RuleFor(x => x.Entity.ApplicantId).NotEmpty();
        RuleFor(x => x.Entity.ApplicantName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Entity.RequestedAmount).GreaterThan(0);
        RuleFor(x => x.Entity.MonthlyIncome).GreaterThan(0);
        RuleFor(x => x.Entity.CreditScore).InclusiveBetween(300, 900);
        RuleFor(x => x.Entity.EmploymentYears).GreaterThanOrEqualTo(0);
    }
}
