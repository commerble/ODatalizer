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
        
        public ICollection<CampaignProductRelation> ProductRelations { get; set; }
        public ICollection<CampaignCategoryRelation> CategoryRelations { get; set; }

        public ICollection<CampaignAction> Actions { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<Category> Categories { get; set; }

    }
}
