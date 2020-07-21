using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODatalizer.EFCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Sample.EFCore.Controllers
{
    public class M2MController : ODataController
    {
        private const string RouteName = "Sample";
        private readonly Data.SampleDbContext _db;
        private readonly ILogger<M2MController> _logger;
        public M2MController(Data.SampleDbContext db, ILogger<M2MController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [EnableQuery]
        [ODataRoute("Products({ProductId})/Categories", RouteName = RouteName)]
        public IActionResult GetProductCategories(long ProductId)
        {
            return Ok(_db.ProductCategoryRelations.Where(r => r.ProductId == ProductId).Select(r => r.Category));
        }

        [ODataRoute("Products({ProductId})/Categories({CategoryId})", RouteName = RouteName)]
        public async Task<IActionResult> GetProductCategory(long ProductId, int CategoryId)
        {
            var entity = await _db.ProductCategoryRelations.Where(r => r.ProductId == ProductId && r.CategoryId == CategoryId).Select(r => r.Category).FirstOrDefaultAsync();

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [EnableQueryRef()]
        [ODataRoute("Products({ProductId})/Categories/$ref", RouteName = RouteName)]
        public IActionResult GetProductCategoryRefs(long ProductId)
        {
            return Ok(_db.ProductCategoryRelations.Where(r => r.ProductId == ProductId).Select(r => r.Category));
        }

        [ODataRoute("Products({ProductId})/Categories/$ref", RouteName = RouteName)]
        public async Task<IActionResult> PostProductCategoryRefs(long ProductId, [FromBody] Uri link)
        {
            var keys = Request.GetKeysFromUri(link);
            var key = keys.FirstOrDefault();

            if (key == null)
                return NotFound();

            var relation = await _db.ProductCategoryRelations.Where(r => r.ProductId == ProductId && r.CategoryId == (int)key["Id"]).FirstOrDefaultAsync();

            if (relation != null)
                return Conflict();

            _db.ProductCategoryRelations.Add(new Entities.ProductCategoryRelation { ProductId = ProductId, CategoryId = (int)key["Id"] });

            await _db.SaveChangesAsync();

            return Ok();
        }

        [ODataRoute("Products({ProductId})/Categories({CategoryId})/$ref", RouteName = RouteName)]
        public async Task<IActionResult> DeleteProductCategoryRef(long ProductId, int CategoryId)
        {
            var relation = await _db.ProductCategoryRelations.Where(r => r.ProductId == ProductId && r.CategoryId == CategoryId).FirstOrDefaultAsync();

            if (relation == null)
                return NotFound();

            _db.Remove(relation);

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
