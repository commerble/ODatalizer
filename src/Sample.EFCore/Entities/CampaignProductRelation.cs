namespace Sample.EFCore.Entities
{
    public class CampaignProductRelation
    {
        public int CampaignId { get; set; }
        public long ProductId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Campaign Campaign { get; set; }
    }
}
