namespace GymProject.Models;

public class PricingPlan
{
    public string PricingPlanId { get; set; } = null!;
    
    public string PlanName { get; set; } = null!; // "Aylık", "3 Aylık", "6 Aylık", "Yıllık"
    
    public int DurationInMonths { get; set; } // 1, 3, 6, 12
    
    public decimal Price { get; set; } // 899, 2399, 4499, 7999
    
    public bool IsPopular { get; set; } // "En Popüler" etiketi için
    
    public bool IsActive { get; set; } = true; // Aktif/pasif durumu
    
    public int DisplayOrder { get; set; } // Sıralama (1, 2, 3, 4)
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? UpdatedDate { get; set; }
    
    public string Language { get; set; } = "tr"; // "tr", "en", "de" vb.
    
    public string? OriginalLanguage { get; set; } // İlk eklenen planın dili (çeviri kaynağı)
    
    // Navigation property
    public virtual ICollection<PricingPlanFeature> Features { get; set; } = new List<PricingPlanFeature>();
}

