using System.Numerics;
using System.Transactions;
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
    public class PricingController : ControllerBase
    {
        private readonly AlperyurtdasGymProjectContext _dbContext;
        private readonly Services.TranslationService _translationService;

        public PricingController(
            AlperyurtdasGymProjectContext dbContext,
            Services.TranslationService translationService)
        {
            _dbContext = dbContext;
            _translationService = translationService;
        }

        /// <summary>
        /// Tüm aktif fiyat planlarını getir (Public - Frontend için)
        /// </summary>
        [HttpGet]
        [Route("GetActivePricingPlans")]
        [AllowAnonymous] // Public endpoint
        public async Task<ApiResponse> GetActivePricingPlans([FromQuery] string? language = "tr")
        {
            try
            {
                // Eğer dil belirtilmemişse varsayılan olarak "tr" kullan
                var targetLanguage = string.IsNullOrEmpty(language) ? "tr" : language.ToLower();

                var plans = await _dbContext.PricingPlans
                    .Where(p => p.IsActive && p.Language.ToLower() == targetLanguage)
                    .OrderBy(p => p.DisplayOrder)
                    .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                    .Select(p => new
                    {
                        p.PricingPlanId,
                        p.PlanName,
                        p.DurationInMonths,
                        p.Price,
                        p.IsPopular,
                        p.DisplayOrder,
                        p.Language,
                        Features = p.Features.Select(f => new
                        {
                            f.PricingPlanFeatureId,
                            f.FeatureText,
                            f.DisplayOrder
                        }).OrderBy(f => f.DisplayOrder).ToList()
                    })
                    .ToListAsync();

                // Eğer istenen dilde plan bulunamadıysa, orijinal dildeki planları getir
                if (plans.Count == 0 && targetLanguage != "tr")
                {
                    Console.WriteLine($"[PRICING] {targetLanguage} dilinde plan bulunamadı, orijinal dildeki planlar getiriliyor.");
                    plans = await _dbContext.PricingPlans
                        .Where(p => p.IsActive && p.OriginalLanguage == p.Language) // Sadece orijinal planları getir
                        .OrderBy(p => p.DisplayOrder)
                        .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                        .Select(p => new
                        {
                            p.PricingPlanId,
                            p.PlanName,
                            p.DurationInMonths,
                            p.Price,
                            p.IsPopular,
                            p.DisplayOrder,
                            p.Language,
                            Features = p.Features.Select(f => new
                            {
                                f.PricingPlanFeatureId,
                                f.FeatureText,
                                f.DisplayOrder
                            }).OrderBy(f => f.DisplayOrder).ToList()
                        })
                        .ToListAsync();
                }

                return new ApiResponse("Success", $"{plans.Count} aktif fiyat planı bulundu ({targetLanguage}).", plans);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActivePricingPlans hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tüm fiyat planlarını getir (Admin/Trainer için - aktif/pasif tümü)
        /// </summary>
        [HttpGet]
        [Route("GetAllPricingPlans")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetAllPricingPlans()
        {
            try
            {
                var plans = await _dbContext.PricingPlans
                    .OrderBy(p => p.DisplayOrder)
                    .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                    .Select(p => new
                    {
                        p.PricingPlanId,
                        p.PlanName,
                        p.DurationInMonths,
                        p.Price,
                        p.IsPopular,
                        p.IsActive,
                        p.DisplayOrder,
                        p.CreatedDate,
                        p.UpdatedDate,
                        Features = p.Features.Select(f => new
                        {
                            f.PricingPlanFeatureId,
                            f.FeatureText,
                            f.DisplayOrder
                        }).OrderBy(f => f.DisplayOrder).ToList()
                    })
                    .ToListAsync();

                return new ApiResponse("Success", $"{plans.Count} fiyat planı bulundu.", plans);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllPricingPlans hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Belirli bir fiyat planını getir
        /// </summary>
        [HttpPost]
        [Route("GetPricingPlan")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ApiResponse> GetPricingPlan([FromBody] GetPricingPlanRequest request)
        {
            try
            {
                var plan = await _dbContext.PricingPlans
                    .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                    .FirstOrDefaultAsync(p => p.PricingPlanId == request.PricingPlanId);

                if (plan == null)
                {
                    return new ApiResponse("Error", "Fiyat planı bulunamadı.", null);
                }

                var response = new
                {
                    plan.PricingPlanId,
                    plan.PlanName,
                    plan.DurationInMonths,
                    plan.Price,
                    plan.IsPopular,
                    plan.IsActive,
                    plan.DisplayOrder,
                    plan.CreatedDate,
                    plan.UpdatedDate,
                    Features = plan.Features.Select(f => new
                    {
                        f.PricingPlanFeatureId,
                        f.FeatureText,
                        f.DisplayOrder
                    }).OrderBy(f => f.DisplayOrder).ToList()
                };

                return new ApiResponse("Success", "Fiyat planı başarıyla getirildi.", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPricingPlan hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Yeni fiyat planı ekle (çoklu dil desteği ile)
        /// </summary>
        [HttpPost]
        [Route("AddPricingPlan")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> AddPricingPlan([FromBody] AddPricingPlanRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var originalLanguage = request.OriginalLanguage ?? "tr";
                var createdDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                var createdPlans = new List<object>();

                // Önce orijinal dili kaydet
                var originalPlanId = NulidGenarator.Id();
                var originalPlan = new PricingPlan
                {
                    PricingPlanId = originalPlanId,
                    PlanName = request.PlanName,
                    DurationInMonths = request.DurationInMonths,
                    Price = request.Price,
                    IsPopular = request.IsPopular,
                    IsActive = request.IsActive,
                    DisplayOrder = request.DisplayOrder,
                    Language = originalLanguage,
                    OriginalLanguage = originalLanguage, // İlk eklenen plan, kendisi orijinal
                    CreatedDate = createdDate
                };

                await _dbContext.PricingPlans.AddAsync(originalPlan);
                await _dbContext.SaveChangesAsync();

                // Orijinal planın özelliklerini ekle
                if (request.Features != null && request.Features.Count > 0)
                {
                    var originalFeatures = request.Features.Select((f, index) => new PricingPlanFeature
                    {
                        PricingPlanFeatureId = NulidGenarator.Id(),
                        PricingPlanId = originalPlanId,
                        FeatureText = f.FeatureText,
                        DisplayOrder = f.DisplayOrder > 0 ? f.DisplayOrder : index + 1
                    }).ToList();

                    await _dbContext.PricingPlanFeatures.AddRangeAsync(originalFeatures);
                    await _dbContext.SaveChangesAsync();
                }

                createdPlans.Add(new
                {
                    PricingPlanId = originalPlanId,
                    Language = originalLanguage,
                    PlanName = request.PlanName
                });

                // İstenen dillere çevir ve kaydet
                if (request.WantedLanguages != null && request.WantedLanguages.Count > 0)
                {
                    foreach (var targetLanguage in request.WantedLanguages)
                    {
                        // Aynı dilse atla
                        if (targetLanguage.ToLower() == originalLanguage.ToLower())
                            continue;

                        try
                        {
                            // Plan adını çevir
                            var translatedPlanName = await _translationService.TranslateTextAsync(
                                request.PlanName,
                                originalLanguage,
                                targetLanguage
                            );

                            // Yeni plan oluştur (çevrilmiş)
                            var translatedPlanId = NulidGenarator.Id();
                            var translatedPlan = new PricingPlan
                            {
                                PricingPlanId = translatedPlanId,
                                PlanName = translatedPlanName,
                                DurationInMonths = request.DurationInMonths,
                                Price = request.Price,
                                IsPopular = request.IsPopular,
                                IsActive = request.IsActive,
                                DisplayOrder = request.DisplayOrder,
                                Language = targetLanguage,
                                OriginalLanguage = originalLanguage, // Orijinal planın dilini işaretle
                                CreatedDate = createdDate
                            };

                            await _dbContext.PricingPlans.AddAsync(translatedPlan);
                            await _dbContext.SaveChangesAsync();

                            // Özellikleri çevir ve ekle
                            if (request.Features != null && request.Features.Count > 0)
                            {
                                var translatedFeatures = new List<PricingPlanFeature>();

                                foreach (var feature in request.Features)
                                {
                                    var translatedFeatureText = await _translationService.TranslateTextAsync(
                                        feature.FeatureText,
                                        originalLanguage,
                                        targetLanguage
                                    );

                                    translatedFeatures.Add(new PricingPlanFeature
                                    {
                                        PricingPlanFeatureId = NulidGenarator.Id(),
                                        PricingPlanId = translatedPlanId,
                                        FeatureText = translatedFeatureText,
                                        DisplayOrder = feature.DisplayOrder > 0 ? feature.DisplayOrder : translatedFeatures.Count + 1
                                    });
                                }

                                await _dbContext.PricingPlanFeatures.AddRangeAsync(translatedFeatures);
                                await _dbContext.SaveChangesAsync();
                            }

                            createdPlans.Add(new
                            {
                                PricingPlanId = translatedPlanId,
                                Language = targetLanguage,
                                PlanName = translatedPlanName
                            });

                            Console.WriteLine($"[PRICING] {targetLanguage} dilinde plan oluşturuldu: {translatedPlanName}");
                        }
                        catch (Exception langEx)
                        {
                            Console.WriteLine($"[PRICING ERROR] {targetLanguage} dilinde plan oluşturulurken hata: {langEx.Message}");
                            // Bir dilde hata olsa bile diğer dillere devam et
                        }
                    }
                }

                await transaction.CommitAsync();

                return new ApiResponse("Success",
                    $"{createdPlans.Count} dilde fiyat planı başarıyla eklendi.",
                    new
                    {
                        OriginalPlanId = originalPlanId,
                        OriginalLanguage = originalLanguage,
                        CreatedPlans = createdPlans,
                        FeaturesCount = request.Features?.Count ?? 0
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddPricingPlan hatası: {ex.Message}");
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Fiyat planını güncelle
        /// </summary>
        [HttpPost]
        [Route("UpdatePricingPlan")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> UpdatePricingPlan([FromBody] UpdatePricingPlanRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var plan = await _dbContext.PricingPlans
                    .Include(p => p.Features)
                    .FirstOrDefaultAsync(p => p.PricingPlanId == request.PricingPlanId);

                if (plan == null)
                {
                    return new ApiResponse("Error", "Fiyat planı bulunamadı.", null);
                }

                // Plan bilgilerini güncelle
                if (!string.IsNullOrEmpty(request.PlanName))
                    plan.PlanName = request.PlanName;
                if (request.DurationInMonths.HasValue)
                    plan.DurationInMonths = request.DurationInMonths.Value;
                if (request.Price.HasValue)
                    plan.Price = request.Price.Value;
                if (request.IsPopular.HasValue)
                    plan.IsPopular = request.IsPopular.Value;
                if (request.IsActive.HasValue)
                    plan.IsActive = request.IsActive.Value;
                if (request.DisplayOrder.HasValue)
                    plan.DisplayOrder = request.DisplayOrder.Value;

                plan.UpdatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                // Özellikleri güncelle (eğer Features listesi gönderilmişse)

                _dbContext.PricingPlans.Update(plan);
                await _dbContext.SaveChangesAsync();

                if (request.PlanName == "Aylık")
                {
                    var translatedPlan =
                        await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "Monthly");

                    if (translatedPlan != null)
                    {
                        translatedPlan.Price = request.Price.Value;
                        translatedPlan.IsPopular = request.IsPopular.Value;
                        translatedPlan.DisplayOrder = request.DisplayOrder.Value;

                    }

                    _dbContext.PricingPlans.Update(translatedPlan);
                    await _dbContext.SaveChangesAsync();
                }
                else if (request.PlanName == "3 Aylık")
                {
                    var translatedPlan =
                        await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "3 Months");

                    if (translatedPlan != null)
                    {
                        translatedPlan.Price = request.Price.Value;
                        translatedPlan.IsPopular = request.IsPopular.Value;
                        translatedPlan.DisplayOrder = request.DisplayOrder.Value;

                    }
                    _dbContext.PricingPlans.Update(translatedPlan);
                    await _dbContext.SaveChangesAsync();

                }
                else if (request.PlanName == "6 Aylık")
                {
                    var translatedPlan =
                        await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "6 Months");

                    if (translatedPlan != null)
                    {
                        translatedPlan.Price = request.Price.Value;
                        translatedPlan.IsPopular = request.IsPopular.Value;
                        translatedPlan.DisplayOrder = request.DisplayOrder.Value;

                    }
                    _dbContext.PricingPlans.Update(translatedPlan);
                    await _dbContext.SaveChangesAsync();
                }
                else if (request.PlanName == "Yıllık")
                {
                    var translatedPlan =
                        await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "Yearly");

                    if (translatedPlan != null)
                    {
                        translatedPlan.Price = request.Price.Value;
                        translatedPlan.IsPopular = request.IsPopular.Value;
                        translatedPlan.DisplayOrder = request.DisplayOrder.Value;

                    }
                    _dbContext.PricingPlans.Update(translatedPlan);
                    await _dbContext.SaveChangesAsync();
                }
                await transaction.CommitAsync();

                return new ApiResponse("Success", "Fiyat planı başarıyla güncellendi.", new
                {
                    plan.PricingPlanId,
                    plan.PlanName,
                    plan.Price
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdatePricingPlan hatası: {ex.Message}");
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Fiyat planını sil
        /// </summary>
        [HttpPost]
        [Route("DeletePricingPlan")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> DeletePricingPlan([FromBody] DeletePricingPlanRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var plan = await _dbContext.PricingPlans
                    .Include(p => p.Features)
                    .FirstOrDefaultAsync(p => p.PricingPlanId == request.PricingPlanId);

                if (plan == null)
                {
                    return new ApiResponse("Error", "Fiyat planı bulunamadı.", null);
                }

                // Özellikleri sil (Cascade delete zaten var ama manuel de silebiliriz)
                _dbContext.PricingPlanFeatures.RemoveRange(plan.Features);
                _dbContext.PricingPlans.Remove(plan);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse("Success", "Fiyat planı başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeletePricingPlan hatası: {ex.Message}");
                await transaction.RollbackAsync();
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Fiyat planına özellik ekle
        /// </summary>
        [HttpPost]
        [Route("AddPricingPlanFeature")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> AddPricingPlanFeature([FromBody] AddPricingPlanFeatureRequest request)
        {
            try
            {
                // Plan var mı kontrol et
                var plan = await _dbContext.PricingPlans
                    .FirstOrDefaultAsync(p => p.PricingPlanId == request.PricingPlanId);

                if (plan == null)
                {
                    return new ApiResponse("Error", "Fiyat planı bulunamadı.", null);
                }

                // Özellik ekle
                var feature = new PricingPlanFeature
                {
                    PricingPlanFeatureId = NulidGenarator.Id(),
                    PricingPlanId = request.PricingPlanId,
                    FeatureText = request.FeatureText,
                    DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1
                };

                await _dbContext.PricingPlanFeatures.AddAsync(feature);

                var translatedPlanFeature = await _translationService.TranslateTextAsync(
                    request.FeatureText,
                    "tr",
                    "en"
                );

                if (translatedPlanFeature != request.FeatureText)
                {
                    if (plan.PlanName == "Aylık")
                    {
                        var translatedPlan =
                            await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "Monthly");

                        if (translatedPlan != null)
                        {
                            var translatedFeature = new PricingPlanFeature
                            {
                                PricingPlanFeatureId = NulidGenarator.Id(),
                                PricingPlanId = translatedPlan.PricingPlanId,
                                FeatureText = translatedPlanFeature,
                                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1
                            };

                            await _dbContext.PricingPlanFeatures.AddAsync(translatedFeature);
                        }

                    }
                    else if (plan.PlanName == "3 Aylık")
                    {
                        var translatedPlan =
                            await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "3 Months");

                        if (translatedPlan != null)
                        {
                            var translatedFeature = new PricingPlanFeature
                            {
                                PricingPlanFeatureId = NulidGenarator.Id(),
                                PricingPlanId = translatedPlan.PricingPlanId,
                                FeatureText = translatedPlanFeature,
                                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1
                            };

                            await _dbContext.PricingPlanFeatures.AddAsync(translatedFeature);
                        }

                    }
                    else if (plan.PlanName == "6 Aylık")
                    {
                        var translatedPlan =
                            await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "6 Months");

                        if (translatedPlan != null)
                        {
                            var translatedFeature = new PricingPlanFeature
                            {
                                PricingPlanFeatureId = NulidGenarator.Id(),
                                PricingPlanId = translatedPlan.PricingPlanId,
                                FeatureText = translatedPlanFeature,
                                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1
                            };

                            await _dbContext.PricingPlanFeatures.AddAsync(translatedFeature);
                        }

                    }
                    else if (plan.PlanName == "Yıllık")
                    {
                        var translatedPlan =
                            await _dbContext.PricingPlans.FirstOrDefaultAsync(x => x.PlanName == "Yearly");

                        if (translatedPlan != null)
                        {
                            var translatedFeature = new PricingPlanFeature
                            {
                                PricingPlanFeatureId = NulidGenarator.Id(),
                                PricingPlanId = translatedPlan.PricingPlanId,
                                FeatureText = translatedPlanFeature,
                                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1
                            };

                            await _dbContext.PricingPlanFeatures.AddAsync(translatedFeature);
                        }

                    }
                }


                await _dbContext.SaveChangesAsync();


                return new ApiResponse("Success", "Özellik başarıyla eklendi.", new
                {
                    feature.PricingPlanFeatureId,
                    feature.PricingPlanId,
                    feature.FeatureText,
                    feature.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddPricingPlanFeature hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Fiyat planı özelliğini güncelle
        /// </summary>
        [HttpPost]
        [Route("UpdatePricingPlanFeature")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> UpdatePricingPlanFeature([FromBody] UpdatePricingPlanFeatureRequest request)
        {
            try
            {
                var feature = await _dbContext.PricingPlanFeatures
                    .FirstOrDefaultAsync(f => f.PricingPlanFeatureId == request.PricingPlanFeatureId);

                if (feature == null)
                {
                    return new ApiResponse("Error", "Özellik bulunamadı.", null);
                }

                var oldFeatureText = feature.FeatureText;
                // Özellik bilgilerini güncelle
                if (!string.IsNullOrEmpty(request.FeatureText))
                    feature.FeatureText = request.FeatureText;

                if (request.DisplayOrder.HasValue && request.DisplayOrder.Value > 0)
                    feature.DisplayOrder = request.DisplayOrder.Value;

                _dbContext.PricingPlanFeatures.Update(feature);

                var translatedOldPlanFeature = await _translationService.TranslateTextAsync(
                    oldFeatureText,
                  "tr",
                  "en"
                );

                var translatedPlanFeature = await _translationService.TranslateTextAsync(
                    feature.FeatureText,
                    "tr",
                    "en"
                );

                if (translatedPlanFeature != request.FeatureText)
                {
                    var translatedFeature =
                        await _dbContext.PricingPlanFeatures.FirstOrDefaultAsync(x => x.FeatureText == translatedOldPlanFeature);

                    if (translatedFeature != null)
                    {
                        translatedFeature.FeatureText = translatedPlanFeature;
                        translatedFeature.DisplayOrder = request.DisplayOrder.Value;

                        _dbContext.PricingPlanFeatures.Update(translatedFeature);
                    }

                }
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Özellik başarıyla güncellendi.", new
                {
                    feature.PricingPlanFeatureId,
                    feature.PricingPlanId,
                    feature.FeatureText,
                    feature.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdatePricingPlanFeature hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }

        /// <summary>
        /// Fiyat planı özelliğini sil
        /// </summary>
        [HttpPost]
        [Route("DeletePricingPlanFeature")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> DeletePricingPlanFeature([FromBody] DeletePricingPlanFeatureRequest request)
        {
            try
            {
                var listForToBeRemoved = new List<PricingPlanFeature>();

                var feature = await _dbContext.PricingPlanFeatures
                    .FirstOrDefaultAsync(f => f.PricingPlanFeatureId == request.PricingPlanFeatureId);

                if (feature == null)
                {
                    return new ApiResponse("Error", "Özellik bulunamadı.", null);
                }

                var translatedPlanFeature = await _translationService.TranslateTextAsync(
                    feature.FeatureText,
                    "tr",
                    "en"
                );

                if (translatedPlanFeature != feature.FeatureText)
                {
                    var translatedFeature =
                        await _dbContext.PricingPlanFeatures.FirstOrDefaultAsync(x =>
                            x.FeatureText == translatedPlanFeature);


                    listForToBeRemoved.Add(translatedFeature);

                }
                listForToBeRemoved.Add(feature);


                _dbContext.PricingPlanFeatures.RemoveRange(listForToBeRemoved);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse("Success", "Özellik başarıyla silindi.", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeletePricingPlanFeature hatası: {ex.Message}");
                return new ApiResponse("Error", $"Hata = {ex.Message}", null);
            }
        }
    }
}

