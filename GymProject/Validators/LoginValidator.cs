using FluentValidation;
using System.Text.RegularExpressions;
using StockTracking.Model;

namespace GymProject.Validators
{

    public class LoginValidator : AbstractValidator<LoginModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Email boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
                .Must(BeValidEmail).WithMessage("Geçerli bir email formatı giriniz.");
        }

        private bool BeValidEmail(string email)
        {
            // Özel kontrolü regex ile yapabiliriz
            var regexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, regexPattern);
        }
    }

}
