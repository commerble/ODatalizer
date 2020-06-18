namespace Sample.EFCore.Entities
{
    public class CampaignCategoryRelation
    {
        public int CampaignId { get; set; }
        public int CategoryId { get; set; }

        public virtual Campaign Campaign { get; set; }
        public virtual Category Category { get; set; }
    }
}
