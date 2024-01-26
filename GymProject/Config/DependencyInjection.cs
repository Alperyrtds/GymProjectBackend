using FluentValidation;
using System;
using GymProject.Models;
using GymProject.Validators;

namespace GymProject.Config
{
    public class DependencyInjection
    {
        public static void Configure(IServiceCollection services)
        {
            // Dependency Injection ile sınıfları kaydedin
            services.AddScoped<IValidator<Customer>, CustomerAddValidator>();
        }
    }
}
