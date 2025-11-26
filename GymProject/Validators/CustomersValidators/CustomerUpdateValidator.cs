using FluentValidation;
using GymProject.Models;
using GymProject.Models.Requests;

namespace GymProject.Validators.CustomersValidators
{
    public class CustomerUpdateValidator : AbstractValidator<UpdateCustomerRequest>
    {
        public CustomerUpdateValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Id Alanı boş bırakılamaz")
                .MinimumLength(25).WithMessage("Id alanı en az 25 karakter olmalıdır.").MaximumLength(30)
                .WithMessage("Id alanı en fazla 30 karakter olmalıdır.");

            // Update işleminde alanlar optional olabilir, sadece gönderilen alanlar validate edilir
            When(x => !string.IsNullOrWhiteSpace(x.CustomerName), () =>
            {
                RuleFor(x => x.CustomerName)
                    .MaximumLength(50).WithMessage("Ad alanı en fazla 50 karakter olmalıdır.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.CustomerSurname), () =>
            {
                RuleFor(x => x.CustomerSurname)
                    .MaximumLength(50).WithMessage("Soyad alanı en fazla 50 karakter olmalıdır.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail), () =>
            {
                RuleFor(x => x.CustomerEmail)
                    .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.CustomerIdentityNumber), () =>
            {
                RuleFor(x => x.CustomerIdentityNumber)
                    .Matches("^[1-9]{1}[0-9]{9}[02468]{1}$").WithMessage("Geçerli bir TC No giriniz.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.CustomerPhoneNumber), () =>
            {
                RuleFor(x => x.CustomerPhoneNumber)
                    .Matches(@"^\+(?:[0-9] ?){6,14}[0-9]$").WithMessage("Geçerli bir telefon numarası giriniz.");
            });
        }
    }
}
