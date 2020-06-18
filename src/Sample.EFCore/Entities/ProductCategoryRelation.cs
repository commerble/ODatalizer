using System.Collections.Generic;

namespace Sample.EFCore.Entities
{
    public class ProductCategoryRelation
    {
        public long ProductId { get; set; }
        public int CategoryId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Category Category { get; set; }
    }
}
