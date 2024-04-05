using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymProject.Helpers;
using GymProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockTracking.Model;

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
        public async Task<IActionResult> Login(LoginModel model)
        {
            var hashedPassword = NulidGenarator.GenerateSHA512String(model.Password);
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == model.Username && x.UserPassword == hashedPassword);
            if (user != null)
            {
                List<Claim> claims = new();
                if (user is { AdminastorId: not null, AdministratorBool: 1 })
                {
                    var admin = await _dbContext.Administrators.FirstOrDefaultAsync(x =>
                        x.AdministratorId == user.AdminastorId);
                    claims = new List<Claim>
                    {
                        new(ClaimTypes.Sid, user.AdminastorId),
                        new(ClaimTypes.Name, admin.AdministratorName),
                        new(ClaimTypes.Surname, admin.AdministratorSurname),
                        new(ClaimTypes.Role, "Admin"),

                    };
                }
                if (user is { CustomerId: not null, CustomerBool: 0 })
                {
                    var customer = await _dbContext.Customers.FirstOrDefaultAsync(x =>
                        x.CustomerId == user.CustomerId);
                    claims = new List<Claim>
                    {
                        new(ClaimTypes.Sid, user.CustomerId),
                        new(ClaimTypes.Name, customer.CustomerName),
                        new(ClaimTypes.Surname, customer.CustomerSurname),
                        new(ClaimTypes.MobilePhone, customer.CustomerPhoneNumber),
                        new(ClaimTypes.Anonymous, customer.CustomerIdentityNumber),
                        new(ClaimTypes.Email, customer.CustomerEmail),
                        new(ClaimTypes.Role, "Customer"),
                    };
                }

                var token = new JwtSecurityToken
                (
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(6000), //todo: parametreye taşı
                    notBefore: DateTime.UtcNow
                  

                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                user.Claims = claims.ToArray();
                user.Token = tokenString;
            }


            return user != null ? Ok(user) : NotFound();
        }
    }
}
