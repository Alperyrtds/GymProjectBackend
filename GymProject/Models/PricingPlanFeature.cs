namespace GymProject.Models;

public class PricingPlanFeature
{
    public string PricingPlanFeatureId { get; set; } = null!;
    
    public string PricingPlanId { get; set; } = null!; // Foreign key
    
    public string FeatureText { get; set; } = null!; // "Tüm ekipmanlara erişim", "Grup derslerine katılım" vb.
    
    public int DisplayOrder { get; set; } // Özelliklerin sıralaması
    
    // Navigation property
    public virtual PricingPlan? PricingPlan { get; set; }
}

