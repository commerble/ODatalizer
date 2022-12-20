//using Microsoft.AspNetCore.Routing;
//using Microsoft.OData;
//using Microsoft.OData.Json;
//using ODatalizer.EFCore.Routing.Conventions;
//using System.Collections.Generic;

//namespace ODatalizer.EFCore
//{
//    public static class EndpointRouteBuilderExtensions
//    {
//        public static void MapODatalizer(this IEndpointRouteBuilder builder, params ODatalizerEndpoint[] endpoints)
//        {
//            builder.Select().Expand().Filter().OrderBy().MaxTop(ODatalizerEndpoint.DefaultPageSize).Count().SkipToken();
//            foreach (var ep in endpoints)
//            {
//                builder.MapODataRoute(ep.RouteName, ep.RoutePrefix, b => {
//                    b.AddService<IJsonWriterFactory>(ServiceLifetime.Singleton, sp => new DefaultJsonWriterFactory(ODataStringEscapeOption.EscapeOnlyControls));
//                    b.AddService(ServiceLifetime.Singleton, sp => ep.EdmModel);
//                    b.AddService<ODataBatchHandler>(ServiceLifetime.Singleton, sp => new ODatalizerBatchHandler());
//                    b.AddService<ODataDeserializerProvider>(ServiceLifetime.Singleton, sp => new DefaultODataDeserializerProvider(sp));
//                    b.AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp => {
//                        var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting(ep.RouteName, builder.ServiceProvider);
//                        if (string.IsNullOrEmpty(ep.ODatalizerController) == false)
//                        {
//                            conventions.Add(new ODatalizerDynamicConvention(ep.ODatalizerController.Replace("Controller", string.Empty)));
//                        }
//                        return conventions;
//                    });
//                });
//            }
//        }
//    }
//}
