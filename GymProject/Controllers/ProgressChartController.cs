using System.Security.Claims;
using GymProject.Models;
using GymProject.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgressChartController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public ProgressChartController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Üye için kilo grafiği verilerini getirir (Goal'dan Weight tipindeki hedefler)
        /// </summary>
        [HttpGet]
        [Route("GetWeightChart")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetWeightChart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var weightGoals = await _dbContext.Goals
                    .Where(x => x.CustomerId == userId && 
                                (x.GoalType == "Weight" || x.GoalType == "weight" || 
                                 (x.GoalName != null && x.GoalName.ToLower().Contains("kilo"))))
                    .OrderBy(x => x.StartDate)
                    .ToListAsync();

                var chartData = weightGoals.Select(g => new
                {
                    Date = g.StartDate?.ToString("yyyy-MM-dd"),
                    TargetValue = g.TargetValue,
                    CurrentValue = g.CurrentValue,
                    GoalName = g.GoalName,
                    IsCompleted = g.IsCompleted
                }).ToList();

                return new ApiResponse("Success", "Kilo grafik verileri başarıyla getirildi.", chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye için ölçü grafikleri verilerini getirir (Goal'dan Measurement tipindeki hedefler)
        /// </summary>
        [HttpGet]
        [Route("GetMeasurementChart")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMeasurementChart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var measurementGoals = await _dbContext.Goals
                    .Where(x => x.CustomerId == userId && 
                                (x.GoalType == "Measurement" || x.GoalType == "measurement" || 
                                 (x.GoalName != null && (x.GoalName.ToLower().Contains("ölçü") || x.GoalName.ToLower().Contains("cm")))))
                    .OrderBy(x => x.StartDate)
                    .ToListAsync();

                var chartData = measurementGoals.Select(g => new
                {
                    Date = g.StartDate?.ToString("yyyy-MM-dd"),
                    GoalName = g.GoalName,
                    TargetValue = g.TargetValue,
                    CurrentValue = g.CurrentValue,
                    ProgressPercentage = g.TargetValue.HasValue && g.TargetValue > 0 && g.CurrentValue.HasValue
                        ? Math.Round((g.CurrentValue.Value / g.TargetValue.Value) * 100, 2)
                        : 0,
                    IsCompleted = g.IsCompleted
                }).ToList();

                return new ApiResponse("Success", "Ölçü grafik verileri başarıyla getirildi.", chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye için antrenman sıklığı grafiği verilerini getirir (haftalık/aylık antrenman sayıları)
        /// </summary>
        [HttpPost]
        [Route("GetWorkoutFrequencyChart")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetWorkoutFrequencyChart([FromBody] WorkoutFrequencyRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var period = request.Period ?? "monthly"; // "weekly" veya "monthly"

                var workoutLogs = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId && x.WorkoutDate.HasValue)
                    .OrderBy(x => x.WorkoutDate)
                    .ToListAsync();

                var chartData = new List<object>();

                if (period == "weekly")
                {
                    // Haftalık gruplama
                    var weeklyGroups = workoutLogs
                        .GroupBy(x => GetWeekKey(x.WorkoutDate!.Value))
                        .Select(g => new
                        {
                            Period = g.Key,
                            Count = g.Count(),
                            TotalDuration = g.Sum(x => x.WorkoutDuration ?? 0),
                            Movements = g.Select(x => x.MovementName).Distinct().Count()
                        })
                        .OrderBy(x => x.Period)
                        .ToList();

                    chartData = weeklyGroups.Cast<object>().ToList();
                }
                else
                {
                    // Aylık gruplama
                    var monthlyGroups = workoutLogs
                        .GroupBy(x => new { Year = x.WorkoutDate!.Value.Year, Month = x.WorkoutDate.Value.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Count = g.Count(),
                            TotalDuration = g.Sum(x => x.WorkoutDuration ?? 0),
                            Movements = g.Select(x => x.MovementName).Distinct().Count()
                        })
                        .OrderBy(x => x.Period)
                        .ToList();

                    chartData = monthlyGroups.Cast<object>().ToList();
                }

                return new ApiResponse("Success", "Antrenman sıklığı grafik verileri başarıyla getirildi.", chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye için performans grafiği verilerini getirir (belirli bir hareket için ağırlık/set/rep ilerlemesi)
        /// </summary>
        [HttpPost]
        [Route("GetPerformanceChart")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetPerformanceChart([FromBody] PerformanceChartRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                IQueryable<WorkoutLog> query = _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId && x.WorkoutDate.HasValue);

                // Eğer MovementId belirtilmişse, sadece o hareket için
                if (!string.IsNullOrEmpty(request.MovementId))
                {
                    query = query.Where(x => x.MovementId == request.MovementId);
                }
                // Eğer MovementName belirtilmişse, sadece o hareket için
                else if (!string.IsNullOrEmpty(request.MovementName))
                {
                    query = query.Where(x => x.MovementName == request.MovementName);
                }

                var workoutLogs = await query
                    .OrderBy(x => x.WorkoutDate)
                    .ToListAsync();

                var chartData = workoutLogs.Select(w => new
                {
                    Date = w.WorkoutDate?.ToString("yyyy-MM-dd"),
                    MovementName = w.MovementName,
                    Weight = w.Weight,
                    SetCount = w.SetCount,
                    Reps = w.Reps,
                    TotalVolume = (w.Weight ?? 0) * (w.SetCount ?? 0) * (w.Reps ?? 0), // Toplam hacim
                    WorkoutDuration = w.WorkoutDuration
                }).ToList();

                return new ApiResponse("Success", "Performans grafik verileri başarıyla getirildi.", chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye için genel ilerleme özeti (dashboard için)
        /// </summary>
        [HttpGet]
        [Route("GetProgressSummary")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetProgressSummary()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // Hedefler
                var totalGoals = await _dbContext.Goals.CountAsync(x => x.CustomerId == userId);
                var completedGoals = await _dbContext.Goals.CountAsync(x => x.CustomerId == userId && x.IsCompleted);
                var activeGoals = totalGoals - completedGoals;

                // Antrenman istatistikleri
                var totalWorkouts = await _dbContext.WorkoutLogs.CountAsync(x => x.CustomerId == userId);
                var thisMonthWorkouts = await _dbContext.WorkoutLogs
                    .CountAsync(x => x.CustomerId == userId && 
                                     x.WorkoutDate.HasValue &&
                                     x.WorkoutDate.Value.Year == DateTime.Now.Year &&
                                     x.WorkoutDate.Value.Month == DateTime.Now.Month);

                var totalWorkoutDuration = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId && x.WorkoutDuration.HasValue)
                    .SumAsync(x => x.WorkoutDuration ?? 0);

                // En çok yapılan hareketler
                var topMovements = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId && !string.IsNullOrEmpty(x.MovementName))
                    .GroupBy(x => x.MovementName)
                    .Select(g => new
                    {
                        MovementName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                var summary = new
                {
                    Goals = new
                    {
                        Total = totalGoals,
                        Completed = completedGoals,
                        Active = activeGoals,
                        CompletionRate = totalGoals > 0 ? Math.Round((double)completedGoals / totalGoals * 100, 2) : 0
                    },
                    Workouts = new
                    {
                        Total = totalWorkouts,
                        ThisMonth = thisMonthWorkouts,
                        TotalDurationMinutes = totalWorkoutDuration,
                        TotalDurationHours = Math.Round(totalWorkoutDuration / 60.0, 2)
                    },
                    TopMovements = topMovements
                };

                // Üyelik bilgileri
                var registration = await _dbContext.CustomersRegistrations
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.CustomerRegistrationStartDate)
                    .FirstOrDefaultAsync();

                int remainingDays = 0;
                string membershipStatus = "Aktif";
                
                if (registration?.CustomerRegistrationFinishDate.HasValue == true)
                {
                    var finishDate = registration.CustomerRegistrationFinishDate.Value.Date;
                    var today = registration.CustomerRegistrationStartDate.Value.Date;
                    var daysDifference = (finishDate - today).Days;

                    if (daysDifference > 0)
                    {
                        remainingDays = daysDifference;
                        membershipStatus = "Aktif";
                    }
                    else if (daysDifference == 0)
                    {
                        remainingDays = 0;
                        membershipStatus = "Bugün Bitiyor";
                    }
                    else
                    {
                        remainingDays = 0;
                        membershipStatus = "Süresi Dolmuş";
                    }
                }

                var summaryWithMembership = new
                {
                    Goals = summary.Goals,
                    Workouts = summary.Workouts,
                    TopMovements = summary.TopMovements,
                    Membership = new
                    {
                        RemainingDays = remainingDays,
                        Status = membershipStatus,
                        FinishDate = registration?.CustomerRegistrationFinishDate?.ToString("yyyy-MM-dd"),
                        IsActive = remainingDays > 0 || membershipStatus == "Bugün Bitiyor"
                    }
                };

                return new ApiResponse("Success", "İlerleme özeti başarıyla getirildi.", summaryWithMembership);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin/Trainer için bir müşterinin ilerleme grafiklerini görüntüleme
        /// </summary>
        [HttpPost]
        [Route("GetCustomerProgressSummary")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomerProgressSummary([FromBody] CustomerRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return new ApiResponse("Error", "Müşteri ID'si boş olamaz.", null);
                }

                var customerExists = await _dbContext.Customers.AnyAsync(c => c.UserId == request.UserId);
                if (!customerExists)
                {
                    return new ApiResponse("Error", "Belirtilen müşteri bulunamadı.", null);
                }

                // Hedefler
                var totalGoals = await _dbContext.Goals.CountAsync(x => x.CustomerId == request.UserId);
                var completedGoals = await _dbContext.Goals.CountAsync(x => x.CustomerId == request.UserId && x.IsCompleted);
                var activeGoals = totalGoals - completedGoals;

                // Antrenman istatistikleri
                var totalWorkouts = await _dbContext.WorkoutLogs.CountAsync(x => x.CustomerId == request.UserId);
                var thisMonthWorkouts = await _dbContext.WorkoutLogs
                    .CountAsync(x => x.CustomerId == request.UserId && 
                                     x.WorkoutDate.HasValue &&
                                     x.WorkoutDate.Value.Year == DateTime.Now.Year &&
                                     x.WorkoutDate.Value.Month == DateTime.Now.Month);

                var totalWorkoutDuration = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == request.UserId && x.WorkoutDuration.HasValue)
                    .SumAsync(x => x.WorkoutDuration ?? 0);

                // En çok yapılan hareketler
                var topMovements = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == request.UserId && !string.IsNullOrEmpty(x.MovementName))
                    .GroupBy(x => x.MovementName)
                    .Select(g => new
                    {
                        MovementName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                var summary = new
                {
                    Goals = new
                    {
                        Total = totalGoals,
                        Completed = completedGoals,
                        Active = activeGoals,
                        CompletionRate = totalGoals > 0 ? Math.Round((double)completedGoals / totalGoals * 100, 2) : 0
                    },
                    Workouts = new
                    {
                        Total = totalWorkouts,
                        ThisMonth = thisMonthWorkouts,
                        TotalDurationMinutes = totalWorkoutDuration,
                        TotalDurationHours = Math.Round(totalWorkoutDuration / 60.0, 2)
                    },
                    TopMovements = topMovements
                };

                return new ApiResponse("Success", "Müşteri ilerleme özeti başarıyla getirildi.", summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        // Yardımcı metod: Hafta anahtarı oluştur
        private string GetWeekKey(DateTime date)
        {
            var startOfYear = new DateTime(date.Year, 1, 1);
            var daysSinceStart = (date - startOfYear).Days;
            var weekNumber = (daysSinceStart / 7) + 1;
            return $"{date.Year}-W{weekNumber:D2}";
        }
    }
}

