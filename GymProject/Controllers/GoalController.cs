using System.Security.Claims;
using GymProject.Helpers;
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
    public class GoalController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public GoalController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Üye kendi hedefini ekleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddGoal")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> AddGoal(Goal model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                if (string.IsNullOrWhiteSpace(model.GoalName))
                {
                    return new ApiResponse("Error", "Hedef adı boş olamaz.", null);
                }

                if (model.TargetValue == null || model.TargetValue <= 0)
                {
                    return new ApiResponse("Error", "Hedef değer geçerli bir sayı olmalıdır.", null);
                }

                var goal = new Goal
                {
                    GoalId = NulidGenarator.Id(),
                    CustomerId = userId,
                    GoalType = model.GoalType ?? "General",
                    GoalName = model.GoalName,
                    TargetValue = model.TargetValue,
                    CurrentValue = model.CurrentValue ?? 0,
                    TargetDate = model.TargetDate.HasValue 
                        ? DateTime.SpecifyKind(model.TargetDate.Value, DateTimeKind.Unspecified) 
                        : null,
                    StartDate = model.StartDate.HasValue 
                        ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Unspecified) 
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    IsCompleted = false,
                    Notes = model.Notes
                };

                await _dbContext.Goals.AddAsync(goal);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hedef başarıyla eklendi.", goal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi hedeflerini görebilir
        /// </summary>
        [HttpGet]
        [Route("GetMyGoals")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyGoals()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var goals = await _dbContext.Goals
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();

                // İlerleme yüzdesini hesapla
                var goalsWithProgress = goals.Select(g =>
                {
                    decimal progressPercentage = 0;
                    if (g.TargetValue.HasValue && g.TargetValue > 0 && g.CurrentValue.HasValue)
                    {
                        progressPercentage = (g.CurrentValue.Value / g.TargetValue.Value) * 100;
                        if (progressPercentage > 100) progressPercentage = 100;
                    }

                    return new
                    {
                        g.GoalId,
                        g.CustomerId,
                        g.GoalType,
                        g.GoalName,
                        g.TargetValue,
                        g.CurrentValue,
                        g.TargetDate,
                        g.StartDate,
                        g.IsCompleted,
                        g.CompletedDate,
                        g.Notes,
                        g.TrainerId,
                        ProgressPercentage = Math.Round(progressPercentage, 2)
                    };
                }).ToList();

                return new ApiResponse("Success", "Hedefler başarıyla getirildi.", goalsWithProgress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer bir üyenin hedeflerini görebilir
        /// </summary>
        [HttpPost]
        [Route("GetCustomerGoals")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomerGoals([FromBody] CustomerRequest request)
        {
            try
            {
                var goals = await _dbContext.Goals
                    .Where(x => x.CustomerId == request.UserId)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();

                // İlerleme yüzdesini hesapla
                var goalsWithProgress = goals.Select(g =>
                {
                    decimal progressPercentage = 0;
                    if (g.TargetValue.HasValue && g.TargetValue > 0 && g.CurrentValue.HasValue)
                    {
                        progressPercentage = (g.CurrentValue.Value / g.TargetValue.Value) * 100;
                        if (progressPercentage > 100) progressPercentage = 100;
                    }

                    return new
                    {
                        g.GoalId,
                        g.CustomerId,
                        g.GoalType,
                        g.GoalName,
                        g.TargetValue,
                        g.CurrentValue,
                        g.TargetDate,
                        g.StartDate,
                        g.IsCompleted,
                        g.CompletedDate,
                        g.Notes,
                        g.TrainerId,
                        ProgressPercentage = Math.Round(progressPercentage, 2)
                    };
                }).ToList();

                return new ApiResponse("Success", "Hedefler başarıyla getirildi.", goalsWithProgress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi hedefini güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateGoal")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> UpdateGoal(Goal model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var goal = await _dbContext.Goals
                    .FirstOrDefaultAsync(x => x.GoalId == model.GoalId && x.CustomerId == userId);

                if (goal == null)
                {
                    return new ApiResponse("Error", "Hedef bulunamadı veya yetkiniz yok.", null);
                }

                goal.GoalType = model.GoalType ?? goal.GoalType;
                goal.GoalName = model.GoalName ?? goal.GoalName;
                goal.TargetValue = model.TargetValue ?? goal.TargetValue;
                goal.CurrentValue = model.CurrentValue ?? goal.CurrentValue;
                goal.TargetDate = model.TargetDate.HasValue 
                    ? DateTime.SpecifyKind(model.TargetDate.Value, DateTimeKind.Unspecified) 
                    : goal.TargetDate;
                goal.Notes = model.Notes ?? goal.Notes;

                // Eğer hedef değere ulaşıldıysa tamamlandı olarak işaretle
                if (goal.TargetValue.HasValue && goal.CurrentValue.HasValue && 
                    goal.CurrentValue >= goal.TargetValue && !goal.IsCompleted)
                {
                    goal.IsCompleted = true;
                    goal.CompletedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                }

                _dbContext.Goals.Update(goal);
                await _dbContext.SaveChangesAsync();

                // İlerleme yüzdesini hesapla
                decimal progressPercentage = 0;
                if (goal.TargetValue.HasValue && goal.TargetValue > 0 && goal.CurrentValue.HasValue)
                {
                    progressPercentage = (goal.CurrentValue.Value / goal.TargetValue.Value) * 100;
                    if (progressPercentage > 100) progressPercentage = 100;
                }

                var responseData = new
                {
                    goal,
                    ProgressPercentage = Math.Round(progressPercentage, 2)
                };

                return new ApiResponse("Success", "Hedef başarıyla güncellendi.", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi hedefini silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteGoal")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> DeleteGoal([FromBody] GoalRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var goal = await _dbContext.Goals
                    .FirstOrDefaultAsync(x => x.GoalId == request.GoalId && x.CustomerId == userId);

                if (goal == null)
                {
                    return new ApiResponse("Error", "Hedef bulunamadı veya yetkiniz yok.", null);
                }

                _dbContext.Goals.Remove(goal);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hedef başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer bir üyeye hedef belirleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddGoalForCustomer")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> AddGoalForCustomer(Goal model)
        {
            try
            {
                var trainerId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(model.CustomerId))
                {
                    return new ApiResponse("Error", "Müşteri ID boş olamaz.", null);
                }

                // Müşteri var mı kontrol et
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(x => x.UserId == model.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse("Error", "Müşteri bulunamadı.", null);
                }

                if (string.IsNullOrWhiteSpace(model.GoalName))
                {
                    return new ApiResponse("Error", "Hedef adı boş olamaz.", null);
                }

                if (model.TargetValue == null || model.TargetValue <= 0)
                {
                    return new ApiResponse("Error", "Hedef değer geçerli bir sayı olmalıdır.", null);
                }

                var goal = new Goal
                {
                    GoalId = NulidGenarator.Id(),
                    CustomerId = model.CustomerId,
                    GoalType = model.GoalType ?? "General",
                    GoalName = model.GoalName,
                    TargetValue = model.TargetValue,
                    CurrentValue = model.CurrentValue ?? 0,
                    TargetDate = model.TargetDate.HasValue 
                        ? DateTime.SpecifyKind(model.TargetDate.Value, DateTimeKind.Unspecified) 
                        : null,
                    StartDate = model.StartDate.HasValue 
                        ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Unspecified) 
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    IsCompleted = false,
                    Notes = model.Notes,
                    TrainerId = trainerId // Antrenör ID'si kaydedilir
                };

                await _dbContext.Goals.AddAsync(goal);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hedef başarıyla eklendi.", goal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tamamlanan hedefleri getirir
        /// </summary>
        [HttpGet]
        [Route("GetCompletedGoals")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetCompletedGoals()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var completedGoals = await _dbContext.Goals
                    .Where(x => x.CustomerId == userId && x.IsCompleted == true)
                    .OrderByDescending(x => x.CompletedDate)
                    .ToListAsync();

                return new ApiResponse("Success", "Tamamlanan hedefler başarıyla getirildi.", completedGoals);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Hedefi tamamlandı olarak işaretle
        /// </summary>
        [HttpPost]
        [Route("MarkGoalAsCompleted")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> MarkGoalAsCompleted([FromBody] GoalRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var goal = await _dbContext.Goals
                    .FirstOrDefaultAsync(x => x.GoalId == request.GoalId && x.CustomerId == userId);

                if (goal == null)
                {
                    return new ApiResponse("Error", "Hedef bulunamadı veya yetkiniz yok.", null);
                }

                goal.IsCompleted = true;
                goal.CompletedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                _dbContext.Goals.Update(goal);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Hedef tamamlandı olarak işaretlendi.", goal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

