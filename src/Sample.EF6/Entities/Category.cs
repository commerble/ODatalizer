using System.Collections.Generic;

namespace Sample.EF6.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
