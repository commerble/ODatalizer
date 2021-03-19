using System;
using Sample.EFCore.Data;
using Sample.EFCore.Entities;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ODatalizer;
using ODatalizer.EFCore;


namespace Sample.EFCore.Controllers
{
    public class SampleController : ODatalizerController<SampleDbContext>
    {
        public SampleController(IServiceProvider sp) 
            : base(sp, authorize:sp.GetService<IOptions<TestSettings>>().Value.UseAuthorize)
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