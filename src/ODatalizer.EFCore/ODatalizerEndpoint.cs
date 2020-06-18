using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODatalizer.EFCore.Builders;

namespace ODatalizer.EFCore
{
    public class ODatalizerEndpoint
    {
        public string Namespace { get; }
        public string RouteName { get; }
        public string RoutePrefix { get; }
        public DbContext DbContext { get; }
        public IEdmModel EdmModel { get; }
        public ODatalizerEndpoint(DbContext db, string routeName = null, string routePrefix = null, string @namespace = null)
        {
            DbContext = db;
            Namespace = @namespace;
            RouteName = routeName;
            RoutePrefix = routePrefix;
            EdmModel = EdmBuilder.Build(db);
        }
    }
}
