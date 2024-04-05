using FluentValidation;
using GymProject.Models;

namespace GymProject.Validators.CustomersValidators
{
    public class CustomerUpdateValidator : AbstractValidator<Customer>
    {
        public CustomerUpdateValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Id Alanı boş bırakılamaz")
                .MinimumLength(30).WithMessage("Id alanı en az 30 karakter olmalıdır.").MaximumLength(30)
                .WithMessage("Id alanı en fazla 30 karakter olmalıdır.");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Ad alanı en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.CustomerSurname)
                .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Soyad alanı en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("E-posta alanı boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.CustomerIdentityNumber)
                .NotEmpty().WithMessage("Geçerli bir TC No giriniz.").Matches("^[1-9]{1}[0-9]{9}[02468]{1}$");

            RuleFor(x => x.CustomerPhoneNumber)
                .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
                .Matches(@"^\+(?:[0-9] ?){6,14}[0-9]$").WithMessage("Geçerli bir telefon numarası giriniz.");
        }
    }
}
