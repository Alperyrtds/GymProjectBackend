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
    public class CustomersProgramController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;

        public CustomersProgramController(AlperyurtdasGymProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Admin veya Trainer üyeye program yazabilir (birden fazla hareket ile)
        /// </summary>
        [HttpPost]
        [Route("AddProgram")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> AddProgram([FromBody] AddProgramRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var createdByUserId = User.FindFirstValue(ClaimTypes.Sid);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(createdByUserId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // Müşteri var mı kontrol et
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(x => x.UserId == model.CustomerId);

                if (customer == null)
                {
                    return new ApiResponse("Error", "Müşteri bulunamadı.", null);
                }

                // Yeni yöntem: Movements listesi varsa kullan
                if (model.Movements != null && model.Movements.Count > 0)
                {
                    // Oluşturan kişinin adını bul
                    string? createdByName = null;
                    if (userRole == "Admin")
                    {
                        var admin = await _dbContext.Administrators.FirstOrDefaultAsync(x => x.UserId == createdByUserId);
                        createdByName = admin != null ? $"{admin.AdministratorName} {admin.AdministratorSurname}" : "Admin";
                    }
                    else if (userRole == "Trainer")
                    {
                        var trainer = await _dbContext.Trainers.FirstOrDefaultAsync(x => x.UserId == createdByUserId);
                        createdByName = trainer != null ? $"{trainer.TrainerName} {trainer.TrainerSurname}" : "Trainer";
                    }

                    // Program oluştur
                    var programStartDate = model.ProgramStartDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramStartDate.Value, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    var programEndDate = model.ProgramEndDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramEndDate.Value, DateTimeKind.Unspecified)
                        : (DateTime?)null;
                    
                    var program = new CustomersProgram
                    {
                        CustomerProgramId = NulidGenarator.Id(),
                        CustomerId = model.CustomerId,
                        ProgramName = model.ProgramName,
                        CreatedByUserId = createdByUserId,
                        CreatedByName = createdByName,
                        ProgramStartDate = programStartDate,
                        ProgramEndDate = programEndDate,
                        LeftValidity = model.LeftValidity
                    };

                    await _dbContext.CustomersPrograms.AddAsync(program);
                    await _dbContext.SaveChangesAsync();

                    // Her hareket için ProgramMovement oluştur
                    var programMovements = new List<ProgramMovement>();
                    int order = 1;
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

                        var programMovement = new ProgramMovement
                        {
                            ProgramMovementId = NulidGenarator.Id(),
                            CustomerProgramId = program.CustomerProgramId,
                            MovementId = movementItem.MovementId,
                            MovementName = movementName,
                            SetCount = movementItem.SetCount,
                            Reps = movementItem.Reps,
                            Order = order++
                        };

                        programMovements.Add(programMovement);
                    }

                    await _dbContext.ProgramMovements.AddRangeAsync(programMovements);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Response'da program ve hareketleri döndür
                    var response = new
                    {
                        Program = program,
                        Movements = programMovements.Select(pm => new
                        {
                            pm.ProgramMovementId,
                            pm.MovementId,
                            pm.MovementName,
                            pm.SetCount,
                            pm.Reps,
                            pm.Order
                        }).ToList()
                    };

                    return new ApiResponse("Success", $"Program başarıyla eklendi. {programMovements.Count} hareket kaydedildi.", response);
                }
                else
                {
                    // Eski yöntem: Backward compatibility (tek hareket)
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

                    string? createdByName = null;
                    if (userRole == "Admin")
                    {
                        var admin = await _dbContext.Administrators.FirstOrDefaultAsync(x => x.UserId == createdByUserId);
                        createdByName = admin != null ? $"{admin.AdministratorName} {admin.AdministratorSurname}" : "Admin";
                    }
                    else if (userRole == "Trainer")
                    {
                        var trainer = await _dbContext.Trainers.FirstOrDefaultAsync(x => x.UserId == createdByUserId);
                        createdByName = trainer != null ? $"{trainer.TrainerName} {trainer.TrainerSurname}" : "Trainer";
                    }

                    // DateTime değerlerini düzelt
                    var programStartDate = model.ProgramStartDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramStartDate.Value, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    var programEndDate = model.ProgramEndDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramEndDate.Value, DateTimeKind.Unspecified)
                        : (DateTime?)null;

                    var program = new CustomersProgram
                    {
                        CustomerProgramId = NulidGenarator.Id(),
                        CustomerId = model.CustomerId,
                        MovementId = model.MovementId,
                        MovementName = movementName,
                        ProgramName = model.ProgramName,
                        CreatedByUserId = createdByUserId,
                        CreatedByName = createdByName,
                        SetCount = model.SetCount,
                        Reps = model.Reps,
                        ProgramStartDate = programStartDate,
                        ProgramEndDate = programEndDate,
                        LeftValidity = model.LeftValidity
                    };

                    await _dbContext.CustomersPrograms.AddAsync(program);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new ApiResponse("Success", "Program başarıyla eklendi (eski yöntem).", program);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi programını oluşturabilir (birden fazla hareket ile)
        /// </summary>
        [HttpPost]
        [Route("AddMyProgram")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> AddMyProgram([FromBody] AddProgramRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                // Yeni yöntem: Movements listesi varsa kullan
                if (model.Movements != null && model.Movements.Count > 0)
                {
                    // Müşteri bilgilerini al
                    var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
                    string? createdByName = customer != null ? $"{customer.CustomerName} {customer.CustomerSurname}" : "Üye";

                    // Program oluştur
                    var programStartDate = model.ProgramStartDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramStartDate.Value, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    var programEndDate = model.ProgramEndDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramEndDate.Value, DateTimeKind.Unspecified)
                        : (DateTime?)null;
                    
                    var program = new CustomersProgram
                    {
                        CustomerProgramId = NulidGenarator.Id(),
                        CustomerId = userId,
                        ProgramName = model.ProgramName,
                        CreatedByUserId = userId,
                        CreatedByName = createdByName,
                        ProgramStartDate = programStartDate,
                        ProgramEndDate = programEndDate,
                        LeftValidity = model.LeftValidity
                    };

                    await _dbContext.CustomersPrograms.AddAsync(program);
                    await _dbContext.SaveChangesAsync();

                    // Her hareket için ProgramMovement oluştur
                    var programMovements = new List<ProgramMovement>();
                    int order = 1;
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

                        var programMovement = new ProgramMovement
                        {
                            ProgramMovementId = NulidGenarator.Id(),
                            CustomerProgramId = program.CustomerProgramId,
                            MovementId = movementItem.MovementId,
                            MovementName = movementName,
                            SetCount = movementItem.SetCount,
                            Reps = movementItem.Reps,
                            Order = order++
                        };

                        programMovements.Add(programMovement);
                    }

                    await _dbContext.ProgramMovements.AddRangeAsync(programMovements);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Response'da program ve hareketleri döndür
                    var response = new
                    {
                        Program = program,
                        Movements = programMovements.Select(pm => new
                        {
                            pm.ProgramMovementId,
                            pm.MovementId,
                            pm.MovementName,
                            pm.SetCount,
                            pm.Reps,
                            pm.Order
                        }).ToList()
                    };

                    return new ApiResponse("Success", $"Program başarıyla eklendi. {programMovements.Count} hareket kaydedildi.", response);
                }
                else
                {
                    // Eski yöntem: Backward compatibility (tek hareket)
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

                    var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
                    string? createdByName = customer != null ? $"{customer.CustomerName} {customer.CustomerSurname}" : "Üye";

                    var programStartDate = model.ProgramStartDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramStartDate.Value, DateTimeKind.Unspecified)
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    var programEndDate = model.ProgramEndDate.HasValue
                        ? DateTime.SpecifyKind(model.ProgramEndDate.Value, DateTimeKind.Unspecified)
                        : (DateTime?)null;

                    var program = new CustomersProgram
                    {
                        CustomerProgramId = NulidGenarator.Id(),
                        CustomerId = userId,
                        MovementId = model.MovementId,
                        MovementName = movementName,
                        ProgramName = model.ProgramName,
                        CreatedByUserId = userId,
                        CreatedByName = createdByName,
                        SetCount = model.SetCount,
                        Reps = model.Reps,
                        ProgramStartDate = programStartDate,
                        ProgramEndDate = programEndDate,
                        LeftValidity = model.LeftValidity
                    };

                    await _dbContext.CustomersPrograms.AddAsync(program);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new ApiResponse("Success", "Program başarıyla eklendi (eski yöntem).", program);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Üye kendi programlarını görüntüleyebilir (tüm programlar)
        /// </summary>
        [HttpGet]
        [Route("GetMyProgram")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse> GetMyProgram()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var programs = await _dbContext.CustomersPrograms
                    .Where(x => x.CustomerId == userId)
                    .OrderByDescending(x => x.ProgramStartDate)
                    .ToListAsync();

                // Movement bilgilerini ekle
                var programsWithDetails = programs.Select(p =>
                {
                    
                    ProgramMovement? movement = null;
                    if (!string.IsNullOrEmpty(p.MovementId))
                    {
                        movement = _dbContext.ProgramMovements.FirstOrDefault(m => m.CustomerProgramId == p.CustomerProgramId);
                    }
                    
                    return new
                    {
                        p.CustomerProgramId,
                        p.CustomerId,
                        p.MovementId,
                        p.MovementName,
                        p.ProgramName,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.SetCount,
                        p.Reps,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.LeftValidity,
                        Movement = movement
                    };
                }).ToList();

                return new ApiResponse("Success", "Programlar başarıyla getirildi.", programsWithDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Program detayını getirir (Customer kendi programını, Admin/Trainer tüm programları görebilir)
        /// </summary>
        [HttpPost]
        [Route("GetProgramDetail")]
        [Authorize]
        public async Task<ApiResponse> GetProgramDetail([FromBody] GetProgramDetailRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Sid);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse("Error", "Kullanıcı bilgisi bulunamadı.", null);
                }

                var program = await _dbContext.CustomersPrograms
                    .FirstOrDefaultAsync(x => x.CustomerProgramId == request.ProgramId);

                if (program == null)
                {
                    return new ApiResponse("Error", "Program bulunamadı.", null);
                }

                // Customer sadece kendi programını görebilir
                if (userRole == "Customer" && program.CustomerId != userId)
                {
                    return new ApiResponse("Error", "Bu programı görüntüleme yetkiniz yok.", null);
                }

                // ProgramMovements tablosundan hareketleri getir
                var movementsDb = await _dbContext.ProgramMovements
                    .Where(pm => pm.CustomerProgramId == program.CustomerProgramId)
                    .OrderBy(pm => pm.Order)
                    .ToListAsync();

                object movements;
                
                if (movementsDb.Count > 0)
                {
                    movements = movementsDb.Select(pm => new
                    {
                        pm.ProgramMovementId,
                        pm.MovementId,
                        pm.MovementName,
                        pm.SetCount,
                        pm.Reps,
                        pm.Order,
                        Movement = _dbContext.Movements.FirstOrDefault(m => m.MovementId == pm.MovementId)
                    }).ToList();
                }
                else if (!string.IsNullOrEmpty(program.MovementId))
                {
                    // Eski yöntemle (backward compatibility)
                    Movement? movement = await _dbContext.Movements
                        .FirstOrDefaultAsync(m => m.MovementId == program.MovementId);
                    
                    movements = new List<object>
                    {
                        new
                        {
                            ProgramMovementId = (string?)null,
                            MovementId = program.MovementId,
                            MovementName = program.MovementName,
                            SetCount = program.SetCount,
                            Reps = program.Reps,
                            Order = 1,
                            Movement = movement
                        }
                    };
                }
                else
                {
                    movements = new List<object>();
                }

                // MovementCount hesapla
                int movementCount = 0;
                if (movements is List<object> objList)
                {
                    movementCount = objList.Count;
                }
                else if (movements is System.Collections.IEnumerable enumerable)
                {
                    movementCount = enumerable.Cast<object>().Count();
                }
                else if (movementsDb != null)
                {
                    movementCount = movementsDb.Count;
                }

                var programDetail = new
                {
                    program.CustomerProgramId,
                    program.CustomerId,
                    program.ProgramName,
                    program.CreatedByUserId,
                    program.CreatedByName,
                    program.ProgramStartDate,
                    program.ProgramEndDate,
                    program.LeftValidity,
                    MovementCount = movementCount,
                    Movements = movements
                };

                return new ApiResponse("Success", "Program detayı başarıyla getirildi.", programDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer bir müşterinin programını görüntüleyebilir
        /// </summary>
        [HttpPost]
        [Route("GetCustomerProgram")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetCustomerProgram([FromBody] CustomerProgramRequest request)
        {
            try
            {
                var programs = await _dbContext.CustomersPrograms
                    .Where(x => x.CustomerId == request.CustomerId)
                    .OrderByDescending(x => x.ProgramStartDate)
                    .ToListAsync();

                // Her program için hareketleri getir
                var programsWithDetails = programs.Select(p =>
                {
                    // ProgramMovements tablosundan hareketleri getir
                    var movementsDb = _dbContext.ProgramMovements
                        .Where(pm => pm.CustomerProgramId == p.CustomerProgramId)
                        .OrderBy(pm => pm.Order)
                        .ToList();

                    object movements;
                    
                    if (movementsDb.Count > 0)
                    {
                        movements = movementsDb.Select(pm => new
                        {
                            pm.ProgramMovementId,
                            pm.MovementId,
                            pm.MovementName,
                            pm.SetCount,
                            pm.Reps,
                            pm.Order
                        }).ToList();
                    }
                    else if (!string.IsNullOrEmpty(p.MovementId))
                    {
                        // Eski yöntemle (backward compatibility)
                        movements = new List<object>
                        {
                            new
                            {
                                ProgramMovementId = (string?)null,
                                MovementId = p.MovementId,
                                MovementName = p.MovementName,
                                SetCount = p.SetCount,
                                Reps = p.Reps,
                                Order = 1
                            }
                        };
                    }
                    else
                    {
                        movements = new List<object>();
                    }
                    
                // MovementCount hesapla
                int movementCount = 0;
                if (movements is List<object> objList)
                {
                    movementCount = objList.Count;
                }
                else if (movements is System.Collections.IEnumerable enumerable)
                {
                    movementCount = enumerable.Cast<object>().Count();
                }
                else if (movementsDb != null)
                {
                    movementCount = movementsDb.Count;
                }

                    return new
                    {
                        p.CustomerProgramId,
                        p.CustomerId,
                        p.ProgramName,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.LeftValidity,
                        MovementCount = movementCount,
                        Movements = movements
                    };
                }).ToList();

                return new ApiResponse("Success", "Programlar başarıyla getirildi.", programsWithDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer programı güncelleyebilir
        /// </summary>
        [HttpPost]
        [Route("UpdateProgram")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> UpdateProgram([FromBody] UpdateProgramRequest model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var program = await _dbContext.CustomersPrograms
                    .FirstOrDefaultAsync(x => x.CustomerProgramId == model.ProgramId);

                if (program == null)
                {
                    return new ApiResponse("Error", "Program bulunamadı.", null);
                }

                // Program bilgilerini güncelle
                program.ProgramName = model.ProgramName ?? program.ProgramName;
                program.ProgramStartDate = model.ProgramStartDate.HasValue
                    ? DateTime.SpecifyKind(model.ProgramStartDate.Value, DateTimeKind.Unspecified)
                    : program.ProgramStartDate;
                program.ProgramEndDate = model.ProgramEndDate.HasValue
                    ? DateTime.SpecifyKind(model.ProgramEndDate.Value, DateTimeKind.Unspecified)
                    : program.ProgramEndDate;
                program.LeftValidity = model.LeftValidity;

                // Eğer Movements listesi gönderilmişse, hareketleri güncelle
                if (model.Movements != null)
                {
                    // Mevcut hareketleri sil
                    var existingMovements = await _dbContext.ProgramMovements
                        .Where(pm => pm.CustomerProgramId == program.CustomerProgramId)
                        .ToListAsync();
                    
                    _dbContext.ProgramMovements.RemoveRange(existingMovements);
                    await _dbContext.SaveChangesAsync();

                    // Yeni hareketleri ekle
                    var programMovements = new List<ProgramMovement>();
                    int order = 1;
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

                        var programMovement = new ProgramMovement
                        {
                            ProgramMovementId = NulidGenarator.Id(),
                            CustomerProgramId = program.CustomerProgramId,
                            MovementId = movementItem.MovementId,
                            MovementName = movementName,
                            SetCount = movementItem.SetCount,
                            Reps = movementItem.Reps,
                            Order = order++
                        };

                        programMovements.Add(programMovement);
                    }

                    await _dbContext.ProgramMovements.AddRangeAsync(programMovements);
                }

                _dbContext.CustomersPrograms.Update(program);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse("Success", "Program başarıyla güncellendi.", program);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Admin veya Trainer programı silebilir
        /// </summary>
        [HttpPost]
        [Route("DeleteProgram")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> DeleteProgram([FromBody] DeleteProgramRequest request)
        {
            try
            {
                var program = await _dbContext.CustomersPrograms
                    .FirstOrDefaultAsync(x => x.CustomerProgramId == request.ProgramId);

                if (program == null)
                {
                    return new ApiResponse("Error", "Program bulunamadı.", null);
                }

                _dbContext.CustomersPrograms.Remove(program);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Program başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tüm antrenörlerin yazdığı programları getirir (Admin ve Trainer için)
        /// </summary>
        [HttpGet]
        [Route("GetTrainerPrograms")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetTrainerPrograms()
        {
            try
            {
                // Tüm antrenörlerin UserId'lerini al
                var trainerUserIds = await _dbContext.Trainers
                    .Select(t => t.UserId)
                    .ToListAsync();

                // Antrenörlerin yazdığı programları getir
                var programs = await _dbContext.CustomersPrograms
                    .Where(p => p.CreatedByUserId != null && trainerUserIds.Contains(p.CreatedByUserId))
                    .OrderByDescending(x => x.ProgramStartDate)
                    .ToListAsync();

                // Program detaylarını hazırla
                var programsWithDetails = programs.Select(p =>
                {
                    // Program'a ait hareketleri getir
                    var movements = _dbContext.ProgramMovements
                        .Where(pm => pm.CustomerProgramId == p.CustomerProgramId)
                        .Select(pm => new
                        {
                            pm.ProgramMovementId,
                            pm.MovementId,
                            pm.MovementName,
                            pm.SetCount,
                            pm.Reps
                        })
                        .ToList();

                    // Müşteri bilgilerini getir
                    var customer = _dbContext.Customers
                        .FirstOrDefault(c => c.UserId == p.CustomerId);

                    return new
                    {
                        p.CustomerProgramId,
                        p.CustomerId,
                        CustomerName = customer != null ? $"{customer.CustomerName} {customer.CustomerSurname}" : "Bilinmiyor",
                        CustomerEmail = customer?.CustomerEmail ?? "",
                        p.ProgramName,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.LeftValidity,
                        MovementCount = movements.Count,
                        Movements = movements
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenör programları başarıyla getirildi.", programsWithDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Belirli bir antrenörün yazdığı programları getirir (Admin ve Trainer için)
        /// </summary>
        [HttpPost]
        [Route("GetProgramsByTrainer")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetProgramsByTrainer([FromBody] CustomerRequest request)
        {
            try
            {
                // Antrenör var mı kontrol et
                var trainer = await _dbContext.Trainers
                    .FirstOrDefaultAsync(t => t.UserId == request.UserId);

                if (trainer == null)
                {
                    return new ApiResponse("Error", "Antrenör bulunamadı.", null);
                }

                // Bu antrenörün yazdığı programları getir
                var programs = await _dbContext.CustomersPrograms
                    .Where(p => p.CreatedByUserId == request.UserId)
                    .OrderByDescending(x => x.ProgramStartDate)
                    .ToListAsync();

                // Program detaylarını hazırla
                var programsWithDetails = programs.Select(p =>
                {
                    // Program'a ait hareketleri getir
                    var movements = _dbContext.ProgramMovements
                        .Where(pm => pm.CustomerProgramId == p.CustomerProgramId)
                        .Select(pm => new
                        {
                            pm.ProgramMovementId,
                            pm.MovementId,
                            pm.MovementName,
                            pm.SetCount,
                            pm.Reps
                        })
                        .ToList();

                    // Müşteri bilgilerini getir
                    var customer = _dbContext.Customers
                        .FirstOrDefault(c => c.UserId == p.CustomerId);

                    return new
                    {
                        p.CustomerProgramId,
                        p.CustomerId,
                        CustomerName = customer != null ? $"{customer.CustomerName} {customer.CustomerSurname}" : "Bilinmiyor",
                        CustomerEmail = customer?.CustomerEmail ?? "",
                        p.ProgramName,
                        p.CreatedByUserId,
                        p.CreatedByName,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.LeftValidity,
                        MovementCount = movements.Count,
                        Movements = movements
                    };
                }).ToList();

                return new ApiResponse("Success", "Antrenör programları başarıyla getirildi.", programsWithDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

    }
}

