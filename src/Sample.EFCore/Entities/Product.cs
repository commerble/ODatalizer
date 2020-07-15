using System.Collections.Generic;

namespace Sample.EFCore.Entities
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int SalesPatternId { get; set; }
        public virtual SalesPattern SalesPattern { get; set; }
        public virtual SalesProduct SalesProduct { get; set; }
        public virtual ICollection<ProductCategoryRelation> CategoryRelations { get; set; } = new List<ProductCategoryRelation>();
        public virtual ICollection<CampaignProductRelation> CampaignRelations { get; set; } = new List<CampaignProductRelation>();

        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
