using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODatalizer.EFCore.Builders;
using System;

namespace ODatalizer.EFCore
{
    public class ODatalizerEndpoint
    {
        /// <summary>
        /// DefaultPageSize is const value 
        /// because EnableQueryAttribute of ODatalizerController need const value.
        /// You cannot change this value yet.
        /// </summary>
        public const int DefaultPageSize = 100;
        public const int DefaultMaxExpansionDepth = 5;

        public string Namespace { get; }
        public string RouteName { get; }
        public string RoutePrefix { get; }
        public DbContext DbContext { get; }
        public IEdmModel EdmModel { get; }
        public string ODatalizerController { get; set; }
        public int PageSize { get; set; } = DefaultPageSize;
        public int MaxExpansionDepth { get; set; } = DefaultMaxExpansionDepth;
        public bool Authorize { get; }
        public ODatalizerEndpoint(
            DbContext db, 
            string routeName = null, 
            string routePrefix = null, 
            string controller = null, 
            string @namespace = null, 
            bool authorize = false,
            Action<ODataConventionModelBuilder> onEdmCreating = null)
        {
            DbContext = db;
            Namespace = @namespace;
            ODatalizerController = controller;
            RouteName = routeName;
            RoutePrefix = routePrefix;
            Authorize = authorize;
            EdmModel = EdmBuilder.Build(db, onEdmCreating);
        }
    }

    public class ODatalizerEndpoint<TDbContext> : ODatalizerEndpoint where TDbContext : DbContext
    {
        public ODatalizerEndpoint(IServiceProvider sp, string routeName = null, string routePrefix = null, string controller = null, string @namespace = null, bool authorize = false) : base(sp.GetRequiredService<TDbContext>(), routeName, routePrefix, controller, @namespace, authorize)
        {
        }
    }

}
