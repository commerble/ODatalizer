using Microsoft.OData.Edm;
using ODatalizer.EF6.Builders;
using System.Data.Entity;

namespace ODatalizer.EF6
{
    public class ODatalizerEndpoint
    {
        public string Namespace { get; }
        public string RouteName { get; }
        public string RoutePrefix { get; }
        public DbContext DbContext { get; }
        public IEdmModel EdmModel { get; }
        public int? MaxNestNavigations { get; }
        public ODatalizerEndpoint(DbContext db, string routeName = null, string routePrefix = null, int? maxNavigations = null, string @namespace = null)
        {
            DbContext = db;
            Namespace = @namespace;
            RouteName = routeName;
            RoutePrefix = routePrefix;
            MaxNestNavigations = maxNavigations;
            EdmModel = EdmBuilder.Build(db);
        }
    }
}
