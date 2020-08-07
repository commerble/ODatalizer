﻿using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using ODatalizer.Batch;

namespace ODatalizer.EF6
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void MapODatalizer(this IEndpointRouteBuilder builder, params ODatalizerEndpoint[] endpoints)
        {
            builder.Select().Expand().Filter().OrderBy().MaxTop(100).Count().SkipToken();
            foreach (var ep in endpoints)
            {
                builder.MapODataRoute(ep.RouteName, ep.RoutePrefix, ep.EdmModel, new ODatalizerBatchHandler());
            }
        }
    }
}