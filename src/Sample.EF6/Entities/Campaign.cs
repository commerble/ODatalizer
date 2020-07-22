using System;
using System.Collections.Generic;

namespace Sample.EF6.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public virtual ICollection<CampaignAction> Actions { get; set; } = new List<CampaignAction>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
