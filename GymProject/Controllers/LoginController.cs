using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymProject.Helpers;
using GymProject.Models;
using GymProject.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;
        private readonly IConfiguration _config;

        public LoginController(AlperyurtdasGymProjectContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _config = configuration;
        }
        [HttpPost]
        public async Task<ApiResponse> Login(LoginModel model)
        {
            var validator = new LoginValidator();

            var result = await validator.ValidateAsync(model);

            if (!result.IsValid)
            {
                return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
            }

            var hashedPassword = NulidGenarator.GenerateSHA512String(model.Password);
            
            // Önce tüm user tiplerinde arama yap
            BaseUser? user = null;
            List<Claim> claims = new();

            // Admin kontrolü (öncelikli)
            var admin = await _dbContext.Administrators
                .FirstOrDefaultAsync(x => x.UserName == model.Username && x.UserPassword == hashedPassword);
            
            if (admin != null)
            {
                user = admin;
                claims = new List<Claim>
                {
                    new(ClaimTypes.Sid, admin.UserId),
                    new(ClaimTypes.Name, admin.AdministratorName ?? ""),
                    new(ClaimTypes.Surname, admin.AdministratorSurname ?? ""),
                    new(ClaimTypes.Role, "Admin"),
                };
            }
            // Trainer kontrolü
            else
            {
                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(x => x.UserName == model.Username && x.UserPassword == hashedPassword);
                
                if (trainer != null)
                {
                    user = trainer;
                    claims = new List<Claim>
                    {
                        new(ClaimTypes.Sid, trainer.UserId),
                        new(ClaimTypes.Name, trainer.TrainerName ?? ""),
                        new(ClaimTypes.Surname, trainer.TrainerSurname ?? ""),
                        new(ClaimTypes.MobilePhone, trainer.TrainerPhoneNumber ?? ""),
                        new(ClaimTypes.Email, trainer.TrainerEmail ?? ""),
                        new(ClaimTypes.Role, "Trainer"),
                    };
                }
                // Customer kontrolü
                else
                {
                    var customer = await _dbContext.Customers
                        .FirstOrDefaultAsync(x => x.UserName == model.Username && x.UserPassword == hashedPassword);
                    
                    if (customer != null)
                    {
                        user = customer;
                        claims = new List<Claim>
                        {
                            new(ClaimTypes.Sid, customer.UserId),
                            new(ClaimTypes.Name, customer.CustomerName ?? ""),
                            new(ClaimTypes.Surname, customer.CustomerSurname ?? ""),
                            new(ClaimTypes.MobilePhone, customer.CustomerPhoneNumber ?? ""),
                            new(ClaimTypes.Anonymous, customer.CustomerIdentityNumber ?? ""),
                            new(ClaimTypes.Email, customer.CustomerEmail ?? ""),
                            new(ClaimTypes.Role, "Customer"),
                        };
                    }
                }
            }

            if (user == null || claims.Count == 0)
            {
                return new ApiResponse("Error", $"Kullanıcı Bulunamadı", null);
            }

            // Token oluştur
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(6000), //todo: parametreye taşı
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            user.Claims = claims.ToArray();
            user.Token = tokenString;

            // İlk giriş kontrolü - şifre değiştirilmemişse özel response döndür
            if (user is Customer customerUser && !customerUser.IsPasswordChanged)
            {
                var responseData = new
                {
                    user,
                    requiresPasswordChange = true,
                    message = "İlk giriş yaptınız. Lütfen şifrenizi değiştirin."
                };
                return new ApiResponse("Success", "Giriş yapıldı. Şifrenizi değiştirmeniz gerekiyor.", responseData);
            }
            else if (user is Administrator adminUser && !adminUser.IsPasswordChanged)
            {
                var responseData = new
                {
                    user,
                    requiresPasswordChange = true,
                    message = "İlk giriş yaptınız. Lütfen şifrenizi değiştirin."
                };
                return new ApiResponse("Success", "Giriş yapıldı. Şifrenizi değiştirmeniz gerekiyor.", responseData);
            }
            else if (user is Trainer trainerUser && !trainerUser.IsPasswordChanged)
            {
                var responseData = new
                {
                    user,
                    requiresPasswordChange = true,
                    message = "İlk giriş yaptınız. Lütfen şifrenizi değiştirin."
                };
                return new ApiResponse("Success", "Giriş yapıldı. Şifrenizi değiştirmeniz gerekiyor.", responseData);
            }

            return new ApiResponse("Success", $"Giriş Yapıldı.", user);
        }
    }
}
