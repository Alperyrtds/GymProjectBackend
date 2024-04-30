using System.Transactions;
using GymProject.Helpers;
using GymProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymProject.Validators.CustomersValidators;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private AlperyurtdasGymProjectContext _dbContext;
        public CustomerController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("AddCustomer")]
        public async Task<ApiResponse> AddCustomer(Customer model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var validator = new CustomerAddValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var customer = new Customer()
                {
                    CustomerId = NulidGenarator.Id(),
                    CustomerEmail = model.CustomerEmail,
                    CustomerIdentityNumber = model.CustomerIdentityNumber,
                    CustomerName = model.CustomerName,
                    CustomerPhoneNumber = model.CustomerPhoneNumber,
                    CustomerSurname = model.CustomerSurname
                };

                await _dbContext.Customers.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                var user = new User()
                {
                    UserId = NulidGenarator.Id(),
                    CustomerId = customer.CustomerId,
                    CustomerBool = 1,
                    AdminastorId = null,
                    AdministratorBool = 0,
                    UserName = customer.CustomerEmail,
                    UserPassword = NulidGenarator.GenerateSHA512String(customer.CustomerEmail)
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                var customerRegistration = new CustomersRegistration()
                {
                   CustomerRegistrationId = NulidGenarator.Id(),
                    CustomerId = customer.CustomerId,
                    CustomerRegistrationStartDate = DateTime.Now,
                    CustomerRegistrationFinishDate = DateTime.Now.AddMonths(int.Parse(model.CustomerRegistryDateLong!)),
               
                };
                await _dbContext.CustomersRegistrations.AddAsync(customerRegistration);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", $"Başarıyla Eklendi", customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }
        public class CustomerRequest
        {
            public string CustomerId { get; set; }
        }

        [HttpPost]
        [Route("GetCustomerById")]

        public async Task<ApiResponse> GetCustomer([FromBody]CustomerRequest model)
        {
            try
            {
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

                return customer != null ? new ApiResponse("Success", $"Başarıyla Getirildi", customer) 
                    : new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }

        [HttpGet]
        [Route("GetAllCustomers")]

        public async Task<ApiResponse> GetAllCustomer()
        {
            try
            {
                var customerList = await _dbContext.Customers.ToListAsync();
                return new ApiResponse("Success", $"Başarıyla Getirildi", customerList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("UpdateCustomer")]

        public async Task<ApiResponse> UpdateCustomer(Customer model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var validator = new CustomerUpdateValidator();

                var result = await validator.ValidateAsync(model);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);
                }

                if (customer.CustomerEmail != model.CustomerEmail)
                {
                    var user = new User();
                    user.UserName = model.CustomerEmail;

                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                }

                customer.CustomerName = model.CustomerName;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerPhoneNumber = model.CustomerPhoneNumber;
                customer.CustomerSurname = model.CustomerSurname;
                customer.CustomerIdentityNumber = model.CustomerIdentityNumber;
                customer.CustomerRegistryDateLong = model.CustomerRegistryDateLong;
                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", $"Başarıyla Güncellendi", customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);

            }

        }

        [HttpPost]
        [Route("DeleteCustomer")]

        public async Task<ApiResponse> DeleteCustomer(CustomerRequest req)
        {
        
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var validator = new CustomerDeleteValidator();

                var result = await validator.ValidateAsync(req.CustomerId);

                if (!result.IsValid)
                {
                    return new ApiResponse("Error", $"Hata = Bir Hata Oluştu", result.Errors, null);
                }

                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == req.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse("Error", $"Hata = Müşteri Bulunamadı.", null);
                }

                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                //var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CustomerId == id);

                //_dbContext.Users.Remove(user);
                //await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", $"Başarıyla Silindi", customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }

        }
    }
}
