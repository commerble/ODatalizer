using System;
using System.Collections.Generic;

namespace Sample.EFCore.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public virtual ICollection<CampaignProductRelation> ProductRelations { get; set; } = new List<CampaignProductRelation>();
        public virtual ICollection<CampaignCategoryRelation> CategoryRelations { get; set; } = new List<CampaignCategoryRelation>();

        public virtual ICollection<CampaignAction> Actions { get; set; } = new List<CampaignAction>();

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

    }
}
