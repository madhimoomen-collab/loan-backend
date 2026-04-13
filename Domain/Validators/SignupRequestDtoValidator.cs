using Domain.DTOs;
using FluentValidation;

namespace Domain.Validators;

public class SignupRequestDtoValidator : AbstractValidator<SignupRequestDto>
{
    public SignupRequestDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
