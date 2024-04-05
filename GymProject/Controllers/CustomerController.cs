using GymProject.Helpers;
using GymProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [Route("MusteriEkle")]
      
        public async Task<IActionResult> AddCustomer(Customer model)
        {
            try
            {
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

                return Ok(customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound(ex.Message);

            }

        }

        [HttpGet]
        [Route("MusteriGetir")]

        public async Task<IActionResult> GetCustomer(string customerId)
        {
            try
            {
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
                return customer != null ? Ok(customer) : NotFound("Müşteri Bulunamadı");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound(ex.Message);
            }

        }

        [HttpPost]
        [Route("TumMusterileriGetir")]

        public async Task<IActionResult> GetAllCustomer(string customerId)
        {
            try
            {
                return Ok(await _dbContext.Customers.ToListAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound(ex.Message);
            }
        }

        [HttpPut]
        [Route("MusteriGuncelle")]

        public async Task<IActionResult> UpdateCustomer(Customer model)
        {
            try
            {
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

                if (customer == null)
                {
                    return NotFound("Müşteri Bulunamadı");
                }

                customer.CustomerName = model.CustomerName;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerPhoneNumber = model.CustomerPhoneNumber;
                customer.CustomerSurname = model.CustomerSurname;
                customer.CustomerIdentityNumber = model.CustomerIdentityNumber;

                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                return Ok(customer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound(ex);

            }

        }

        [HttpDelete]
        [Route("MusteriSil")]

        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
                if (customer == null)
                    return NotFound("Müşteri Bulunamadı");

                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                return Ok("Başarıyla Silindi");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound(ex.Message);
            }

        }
    }
}
