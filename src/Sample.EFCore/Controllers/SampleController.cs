using System;
using Sample.EFCore.Data;
using ODatalizer.EFCore;
using Sample.EFCore.Entities;
using Microsoft.AspNet.OData.Routing;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ODatalizer;

namespace Sample.EFCore.Controllers
{
    public class SampleController : ODatalizerController<SampleDbContext>
    {
        public SampleController(IServiceProvider sp) : base(sp)
        {
        }

        /// <summary>
        /// Override fast implementation sample
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        [EnableQueryRef(PageSize = 100)]
        [ODataRoute("Products({id})/Categories/$ref", RouteName = "Sample")]
        public async Task<IActionResult> PostProductCategoriesRef(long id, [FromBody] Uri uri)
        {
            var keys = Request.GetKeysFromUri(uri);

            if (keys.Count() != 1)
                return BadRequest();

            var key = keys.First();

            DbContext.ProductCategoryRelations.Add(new ProductCategoryRelation
            {
                ProductId = id,
                CategoryId = (int)key["Id"]
            });

            await DbContext.SaveChangesAsync();

            return Ok();
        }
    }
}