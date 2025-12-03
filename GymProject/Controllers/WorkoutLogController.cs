using System.Security.Claims;
using GymProject.Helpers;
using GymProject.Models;
using GymProject.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkoutLogController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public WorkoutLogController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Üye kendi antrenman oturumunu ekleyebilir (birden fazla hareket ile)
        /// </summary>
        [HttpPost]
        [Route("AddWorkoutSession")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> AddWorkoutSession([FromBody] AddWorkoutSessionRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                if (model.Movements == null || model.Movements.Count == 0)
                {
                    return new ApiResponse("Error", "En az bir hareket eklenmelidir.", null);
                }

                // Antrenman oturumu oluştur
                var workoutDate = model.WorkoutDate.HasValue
                    ? DateTime.SpecifyKind(model.WorkoutDate.Value, DateTimeKind.Unspecified)
                    : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                var workoutSession = new WorkoutSession
                {
                    WorkoutSessionId = NulidGenarator.Id(),
                    CustomerId = userId,
                    WorkoutDate = workoutDate,
                    TotalDuration = model.TotalDuration,
                    Notes = model.Notes
                };

                await _dbContext.WorkoutSessions.AddAsync(workoutSession);
                await _dbContext.SaveChangesAsync();

                // Her hareket için WorkoutLog oluştur
                var workoutLogs = new List<WorkoutLog>();
                foreach (var movementItem in model.Movements)
                {
                    // MovementId varsa Movement tablosundan bilgileri çek
                    string? movementName = movementItem.MovementName;
                    if (!string.IsNullOrEmpty(movementItem.MovementId))
                    {
                        var movement = await _dbContext.Movements
                            .FirstOrDefaultAsync(x => x.MovementId == movementItem.MovementId);
                        
                        if (movement != null)
                        {
                            movementName = movement.MovementName;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(movementName))
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse("Error", $"Hareket adı boş olamaz. Hareket: {movementItem.MovementId ?? "Bilinmeyen"}", null);
                    }

                    var workoutLog = new WorkoutLog
                    {
                        WorkoutLogId = NulidGenarator.Id(),
                        WorkoutSessionId = workoutSession.WorkoutSessionId,
                        CustomerId = userId, // Backward compatibility
                        MovementId = movementItem.MovementId,
                        MovementName = movementName,
                        Weight = movementItem.Weight,
                        SetCount = movementItem.SetCount,
                        Reps = movementItem.Reps,
                        Notes = movementItem.Notes,
                        WorkoutDate = workoutSession.WorkoutDate // Backward compatibility
                    };

                    workoutLogs.Add(workoutLog);
                }

                await _dbContext.WorkoutLogs.AddRangeAsync(workoutLogs);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // Response'da session ve logları döndür
                var response = new
                {
                    WorkoutSession = workoutSession,
                    Movements = workoutLogs.Select(log => new
                    {
                        log.WorkoutLogId,
                        log.MovementId,
                        log.MovementName,
                        log.Weight,
                        log.SetCount,
                        log.Reps,
                        log.Notes
                    }).ToList()
                };

                return new ApiResponse("Success", $"Antrenman oturumu başarıyla eklendi. {workoutLogs.Count} hareket kaydedildi.", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman kaydını ekleyebilir (Eski yöntem - backward compatibility)
        /// </summary>
        [HttpPost]
        [Route("AddWorkoutLog")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> AddWorkoutLog(WorkoutLog model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // MovementId varsa Movement tablosundan bilgileri çek
                string? movementName = model.MovementName;
                if (!string.IsNullOrEmpty(model.MovementId))
                {
                    var movement = await _dbContext.Movements
                        .FirstOrDefaultAsync(x => x.MovementId == model.MovementId);
                    
                    if (movement != null)
                    {
                        movementName = movement.MovementName;
                    }
                }

                if (string.IsNullOrWhiteSpace(movementName))
                {
                    return new ApiResponse("Error", "Hareket adı boş olamaz.", null);
                }

                // Eski yöntem için otomatik session oluştur
                var workoutDate = model.WorkoutDate.HasValue
                    ? DateTime.SpecifyKind(model.WorkoutDate.Value, DateTimeKind.Unspecified)
                    : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                var workoutSession = new WorkoutSession
                {
                    WorkoutSessionId = NulidGenarator.Id(),
                    CustomerId = userId,
                    WorkoutDate = workoutDate,
                    TotalDuration = model.WorkoutDuration,
                    Notes = "Tek hareket antrenmanı (eski yöntem)"
                };

                await _dbContext.WorkoutSessions.AddAsync(workoutSession);
                await _dbContext.SaveChangesAsync();

                var workoutLogDate = model.WorkoutDate.HasValue
                    ? DateTime.SpecifyKind(model.WorkoutDate.Value, DateTimeKind.Unspecified)
                    : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                var workoutLog = new WorkoutLog
                {
                    WorkoutLogId = NulidGenarator.Id(),
                    WorkoutSessionId = workoutSession.WorkoutSessionId,
                    CustomerId = userId,
                    MovementId = model.MovementId,
                    MovementName = movementName,
                    Weight = model.Weight,
                    SetCount = model.SetCount,
                    Reps = model.Reps,
                    WorkoutDate = workoutLogDate,
                    WorkoutDuration = model.WorkoutDuration,
                    Notes = model.Notes,
                    TrainerId = model.TrainerId
                };

                await _dbContext.WorkoutLogs.AddAsync(workoutLog);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenman kaydı başarıyla eklendi.", workoutLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman oturumlarını görebilir (session bazlı)
        /// </summary>
        [HttpGet]
        [Route("GetMyWorkoutSessions")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyWorkoutSessions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var sessions = await _dbContext.WorkoutSessions
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.WorkoutDate)
                    .ToListAsync();

                // Her session için hareketleri getir
                var sessionsWithMovements = sessions.Select(session =>
                {
                    var movements = _dbContext.WorkoutLogs
                        .Where(x => x.WorkoutSessionId == session.WorkoutSessionId)
                        .Select(log => new
                        {
                            log.WorkoutLogId,
                            log.MovementId,
                            log.MovementName,
                            log.Weight,
                            log.SetCount,
                            log.Reps,
                            log.Notes
                        })
                        .ToList();

                    return new
                    {
                        session.WorkoutSessionId,
                        session.WorkoutDate,
                        session.TotalDuration,
                        session.Notes,
                        MovementCount = movements.Count,
                        Movements = movements
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenman oturumları başarıyla getirildi.", sessionsWithMovements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman oturumlarını görebilir (session bazlı)
        /// </summary>
        [HttpPost]
        [Route("GetCustomersWorkoutSessions")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomersWorkoutSessions([FromBody] CustomerRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.UserId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var sessions = await _dbContext.WorkoutSessions
                    .Where(x => x.CustomerId == request.UserId)
                    .OrderByDescending(x => x.WorkoutDate)
                    .ToListAsync();

                // Her session için hareketleri getir
                var sessionsWithMovements = sessions.Select(session =>
                {
                    var movements = _dbContext.WorkoutLogs
                        .Where(x => x.WorkoutSessionId == session.WorkoutSessionId)
                        .Select(log => new
                        {
                            log.WorkoutLogId,
                            log.MovementId,
                            log.MovementName,
                            log.Weight,
                            log.SetCount,
                            log.Reps,
                            log.Notes
                        })
                        .ToList();

                    return new
                    {
                        session.WorkoutSessionId,
                        session.WorkoutDate,
                        session.TotalDuration,
                        session.Notes,
                        MovementCount = movements.Count,
                        Movements = movements
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenman oturumları başarıyla getirildi.", sessionsWithMovements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman kayıtlarını görebilir (Eski yöntem - backward compatibility)
        /// </summary>
        [HttpGet]
        [Route("GetMyWorkoutLogs")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyWorkoutLogs()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var workoutLogs = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.WorkoutDate)
                    .ThenByDescending(x => x.MovementName)
                    .ToListAsync();

                // Movement bilgilerini ekle
                var logsWithMovements = workoutLogs.Select(log =>
                {
                    Movement? movement = null;
                    if (!string.IsNullOrEmpty(log.MovementId))
                    {
                        movement = _dbContext.Movements.FirstOrDefault(m => m.MovementId == log.MovementId);
                    }
                    
                    return new
                    {
                        log.WorkoutLogId,
                        log.WorkoutSessionId,
                        log.CustomerId,
                        log.MovementId,
                        log.MovementName,
                        log.Weight,
                        log.SetCount,
                        log.Reps,
                        log.WorkoutDate,
                        log.WorkoutDuration,
                        log.Notes,
                        log.TrainerId,
                        Movement = movement
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenman kayıtları başarıyla getirildi.", logsWithMovements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Belirli bir antrenman oturumunun detayını getirir
        /// </summary>
        [HttpPost]
        [Route("GetWorkoutSessionDetail")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetWorkoutSessionDetail([FromBody] GetWorkoutSessionDetailRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var sessionId = request.WorkoutSessionId;

                var session = await _dbContext.WorkoutSessions
                    .FirstOrDefaultAsync(x => x.WorkoutSessionId == sessionId && x.CustomerId == userId);

                if (session == null)
                {
                    return new ApiResponse("Error", "Antrenman oturumu bulunamadı veya yetkiniz yok.", null);
                }

                // Session'a ait tüm hareketleri getir
                var movements = await _dbContext.WorkoutLogs
                    .Where(x => x.WorkoutSessionId == sessionId)
                    .Select(log => new
                    {
                        log.WorkoutLogId,
                        log.MovementId,
                        log.MovementName,
                        log.Weight,
                        log.SetCount,
                        log.Reps,
                        log.Notes,
                        Movement = _dbContext.Movements.FirstOrDefault(m => m.MovementId == log.MovementId)
                    })
                    .ToListAsync();

                var sessionDetail = new
                {
                    session.WorkoutSessionId,
                    session.WorkoutDate,
                    session.TotalDuration,
                    session.Notes,
                    MovementCount = movements.Count,
                    Movements = movements
                };

                return new ApiResponse("Success", "Antrenman oturumu detayı başarıyla getirildi.", sessionDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer bir üyenin antrenman kayıtlarını görebilir
        /// </summary>
        [HttpPost]
        [Route("GetCustomerWorkoutLogs")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomerWorkoutLogs([FromBody] CustomerRequest request)
        {
            try
            {
                var workoutLogs = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == request.UserId)
                    .OrderByDescending(x => x.WorkoutDate)
                    .ThenByDescending(x => x.MovementName)
                    .ToListAsync();

                // Movement bilgilerini ekle
                var logsWithMovements = workoutLogs.Select(log =>
                {
                    Movement? movement = null;
                    if (!string.IsNullOrEmpty(log.MovementId))
                    {
                        movement = _dbContext.Movements.FirstOrDefault(m => m.MovementId == log.MovementId);
                    }
                    
                    return new
                    {
                        log.WorkoutLogId,
                        log.CustomerId,
                        log.MovementId,
                        log.MovementName,
                        log.Weight,
                        log.SetCount,
                        log.Reps,
                        log.WorkoutDate,
                        log.WorkoutDuration,
                        log.Notes,
                        log.TrainerId,
                        Movement = movement
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenman kayıtları başarıyla getirildi.", logsWithMovements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye belirli bir hareket için ilerleme kayıtlarını görebilir
        /// </summary>
        [HttpPost]
        [Route("GetProgressByMovement")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetProgressByMovement([FromBody] WorkoutLogRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var workoutLogs = await _dbContext.WorkoutLogs
                    .Where(x => x.CustomerId == userId && x.MovementId == request.MovementId)
                    .OrderBy(x => x.WorkoutDate)
                    .ToListAsync();

                return new ApiResponse("Success", "İlerleme kayıtları başarıyla getirildi.", workoutLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman kaydını güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateWorkoutLog")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> UpdateWorkoutLog(WorkoutLog model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var workoutLog = await _dbContext.WorkoutLogs
                    .FirstOrDefaultAsync(x => x.WorkoutLogId == model.WorkoutLogId && x.CustomerId == userId);

                if (workoutLog == null)
                {
                    return new ApiResponse("Error", "Antrenman kaydı bulunamadı veya yetkiniz yok.", null);
                }

                // MovementId varsa Movement tablosundan bilgileri çek
                if (!string.IsNullOrEmpty(model.MovementId))
                {
                    var movement = await _dbContext.Movements
                        .FirstOrDefaultAsync(x => x.MovementId == model.MovementId);
                    
                    if (movement != null)
                    {
                        workoutLog.MovementName = movement.MovementName;
                        workoutLog.MovementId = model.MovementId;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(model.MovementName))
                {
                    workoutLog.MovementName = model.MovementName;
                }

                workoutLog.Weight = model.Weight;
                workoutLog.SetCount = model.SetCount;
                workoutLog.Reps = model.Reps;
                workoutLog.WorkoutDate = model.WorkoutDate.HasValue
                    ? DateTime.SpecifyKind(model.WorkoutDate.Value, DateTimeKind.Unspecified)
                    : workoutLog.WorkoutDate;
                workoutLog.WorkoutDuration = model.WorkoutDuration;
                workoutLog.Notes = model.Notes;

                _dbContext.WorkoutLogs.Update(workoutLog);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenman kaydı başarıyla güncellendi.", workoutLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi antrenman kaydını silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteWorkoutLog")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> DeleteWorkoutLog([FromBody] WorkoutLogRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var workoutLog = await _dbContext.WorkoutLogs
                    .FirstOrDefaultAsync(x => x.WorkoutLogId == request.WorkoutLogId && x.CustomerId == userId);

                if (workoutLog == null)
                {
                    return new ApiResponse("Error", "Antrenman kaydı bulunamadı veya yetkiniz yok.", null);
                }

                _dbContext.WorkoutLogs.Remove(workoutLog);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenman kaydı başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer bir üyeye antrenman kaydı ekleyebilir
        /// </summary>
        [HttpPost]
        [Route("AddWorkoutLogForCustomer")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> AddWorkoutLogForCustomer(WorkoutLog model)
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

                // MovementId varsa Movement tablosundan bilgileri çek
                string? movementName = model.MovementName;
                if (!string.IsNullOrEmpty(model.MovementId))
                {
                    var movement = await _dbContext.Movements
                        .FirstOrDefaultAsync(x => x.MovementId == model.MovementId);
                    
                    if (movement != null)
                    {
                        movementName = movement.MovementName;
                    }
                }

                if (string.IsNullOrWhiteSpace(movementName))
                {
                    return new ApiResponse("Error", "Hareket adı boş olamaz.", null);
                }

                var workoutLogDate = model.WorkoutDate.HasValue
                    ? DateTime.SpecifyKind(model.WorkoutDate.Value, DateTimeKind.Unspecified)
                    : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                var workoutLog = new WorkoutLog
                {
                    WorkoutLogId = NulidGenarator.Id(),
                    CustomerId = model.CustomerId,
                    MovementId = model.MovementId,
                    MovementName = movementName,
                    Weight = model.Weight,
                    SetCount = model.SetCount,
                    Reps = model.Reps,
                    WorkoutDate = workoutLogDate,
                    WorkoutDuration = model.WorkoutDuration, // Dakika cinsinden antrenman süresi
                    Notes = model.Notes,
                    TrainerId = trainerId // Antrenör ID'si kaydedilir
                };

                await _dbContext.WorkoutLogs.AddAsync(workoutLog);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Antrenman kaydı başarıyla eklendi.", workoutLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

 