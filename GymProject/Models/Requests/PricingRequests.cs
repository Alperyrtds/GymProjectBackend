namespace GymProject.Models.Requests;

public class PricingPlanFeatureItem
{
    public string FeatureText { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

public class AddPricingPlanRequest
{
    public string PlanName { get; set; } = null!; // "Aylık", "3 Aylık", "6 Aylık", "Yıllık"
    public int DurationInMonths { get; set; } // 1, 3, 6, 12
    public decimal Price { get; set; } // 899, 2399, 4499, 7999
    public bool IsPopular { get; set; } // "En Popüler" etiketi için
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } // Sıralama (1, 2, 3, 4)
    public string OriginalLanguage { get; set; } = "tr"; // İlk request'in dili
    public List<string>? WantedLanguages { get; set; } // Hangi dillere çevrileceği ["en", "de", "fr"] vb.
    public List<PricingPlanFeatureItem> Features { get; set; } = new List<PricingPlanFeatureItem>();
}

public class UpdatePricingPlanRequest
{
    public string PricingPlanId { get; set; } = null!;
    public string? PlanName { get; set; }
    public int? DurationInMonths { get; set; }
    public decimal? Price { get; set; }
    public bool? IsPopular { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }
    public List<PricingPlanFeatureItem>? Features { get; set; } // Null ise güncellenmez, boş liste ise tüm özellikler silinir
}

public class DeletePricingPlanRequest
{
    public string PricingPlanId { get; set; } = null!;
}

public class GetPricingPlanRequest
{
    public string PricingPlanId { get; set; } = null!;
}

public class AddPricingPlanFeatureRequest
{
    public string PricingPlanId { get; set; } = null!;
    public string FeatureText { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

public class DeletePricingPlanFeatureRequest
{
    public string PricingPlanFeatureId { get; set; } = null!;
}

public class UpdatePricingPlanFeatureRequest
{
    public string PricingPlanFeatureId { get; set; } = null!;
    public string? FeatureText { get; set; }
    public int? DisplayOrder { get; set; }
}

