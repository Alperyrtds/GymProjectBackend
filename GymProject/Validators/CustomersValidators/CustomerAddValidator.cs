using FluentValidation;
using GymProject.Models;
using GymProject.Models.Requests;

namespace GymProject.Validators.CustomersValidators
{
    public class CustomerAddValidator : AbstractValidator<AddCustomerRequest>
    {
        public CustomerAddValidator()
        {

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

            RuleFor(x => x.CustomerRegistryDateLong)
                .NotEmpty().WithMessage("Üyelik süresi boş bırakılamaz.");
        }
    }
}
