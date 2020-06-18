namespace Sample.EFCore.Entities
{
    public class CampaignAction
    {
        public int CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string OptionValue { get; set; }

        public virtual Campaign Campaign { get; set; }
    }
}
