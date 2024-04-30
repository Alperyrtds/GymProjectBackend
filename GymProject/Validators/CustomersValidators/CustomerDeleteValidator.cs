using FluentValidation;

namespace GymProject.Validators.CustomersValidators
{
    public class CustomerDeleteValidator : AbstractValidator<string>
    {
        public CustomerDeleteValidator()
        {
            RuleFor(x => x).NotEmpty().WithMessage("Id Alanı boş bırakılamaz")
                .MinimumLength(25).WithMessage("Id alanı en az 25 karakter olmalıdır.").MaximumLength(30)
                .WithMessage("Id alanı en fazla 30 karakter olmalıdır.");

        }
    }
}
