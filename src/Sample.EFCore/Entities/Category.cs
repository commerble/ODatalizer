using System.Collections.Generic;

namespace Sample.EFCore.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ProductCategoryRelation> ProductRelations { get; set; } = new List<ProductCategoryRelation>();
        public virtual ICollection<CampaignCategoryRelation> CampaignRelations { get; set; } = new List<CampaignCategoryRelation>();

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }

    }
}
