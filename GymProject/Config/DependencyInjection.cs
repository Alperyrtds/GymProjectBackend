using FluentValidation;
using GymProject.Models;
using GymProject.Models.Requests;
using GymProject.Validators.CustomersValidators;

namespace GymProject.Config
{
    public class DependencyInjection
    {
        public static void Configure(IServiceCollection services)
        {
            // Dependency Injection ile sınıfları kaydedin
            services.AddScoped<IValidator<AddCustomerRequest>, CustomerAddValidator>();
        }
    }
}
