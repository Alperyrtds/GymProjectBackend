using System.Security.Claims;
using GymProject.Helpers;
using GymProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public DashboardController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Admin ve Trainer için dashboard istatistiklerini getirir
        /// </summary>
        [HttpGet]
        [Route("GetDashboardStatistics")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetDashboardStatistics()
        {
            try
            {
                var today = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Unspecified);
                var firstDayOfMonth = DateTime.SpecifyKind(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTimeKind.Unspecified);

                // 1. Toplam Üye
                var totalCustomers = await _dbContext.Customers.CountAsync();

                // 2. Aktif Üye (Üyeliği aktif olanlar - finish date bugünden büyük veya eşit)
                var activeCustomers = await _dbContext.CustomersRegistrations
                    .Where(x => x.CustomerRegistrationFinishDate.HasValue &&
                                x.CustomerRegistrationFinishDate.Value.Date >= today)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();

                // 3. Yeni Üye (Bu Ay)
                var newCustomersThisMonth = await _dbContext.CustomersRegistrations
                    .Where(x => x.CustomerRegistrationStartDate.HasValue &&
                                x.CustomerRegistrationStartDate.Value.Date >= firstDayOfMonth)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();

                // 4. Toplam Admin
                var totalAdmins = await _dbContext.Administrators.CountAsync();

                // 5. Toplam Antrenör
                var totalTrainers = await _dbContext.Trainers.CountAsync();

                // 6. Toplam Hareket
                var totalMovements = await _dbContext.Movements.CountAsync();

                // 7. Son Kayıt Olan Üyeler (3 kişi)
                var lastRegisteredCustomers = await _dbContext.CustomersRegistrations
                    .OrderByDescending(x => x.CustomerRegistrationStartDate)
                    .Take(3)
                    .Join(_dbContext.Customers,
                        reg => reg.CustomerId,
                        cust => cust.UserId,
                        (reg, cust) => new
                        {
                            CustomerId = cust.UserId,
                            CustomerName = cust.CustomerName ?? "Bilinmiyor",
                            CustomerSurname = cust.CustomerSurname ?? "",
                            CustomerEmail = cust.CustomerEmail ?? "",
                            CustomerPhoneNumber = cust.CustomerPhoneNumber ?? "",
                            RegistrationStartDate = reg.CustomerRegistrationStartDate.Value.ToString("yyyy-MM-dd"),
                            RegistrationFinishDate = reg.CustomerRegistrationFinishDate.Value.ToString("yyyy-MM-dd")
                        })
                    .ToListAsync();

                var statistics = new
                {
                    TotalCustomers = totalCustomers,
                    ActiveCustomers = activeCustomers,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    TotalAdmins = totalAdmins,
                    TotalTrainers = totalTrainers,
                    TotalMovements = totalMovements,
                    LastRegisteredCustomers = lastRegisteredCustomers
                };

                return new ApiResponse("Success", "Dashboard istatistikleri başarıyla getirildi.", statistics);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

