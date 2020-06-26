using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer.EFCore.Templates
{
    public partial class ControllerGenerator
    {
        public IEdmModel EdmModel { get; }
        public string DbContextTypeName { get; }
        public string RouteName { get; }

        public string RouteNameValue => RouteName != null ? '"' + RouteName + '"' : "null";

        private readonly string _namespace = null;
        public string Namespace {
            get {
                if (_namespace != null)
                    return _namespace;

                var name = RouteName ?? "Default";
                var prefix = $"{name}.";
                if (DbContextTypeName.StartsWith(prefix) || EdmModel.SchemaElements.Any(schema => schema.Namespace.StartsWith(prefix)))
                    name += "2";

                return "ODatalizer.EFCore.Controllers." + name;
            }
        }

        public ControllerGenerator(IEdmModel edmModel, string dbContextTypeName, string routeName, string @namespace)
        {
            EdmModel = edmModel;
            DbContextTypeName = dbContextTypeName;
            RouteName = routeName;
            _namespace = @namespace;
        }

        public static ControllerGenerator Create(ODatalizerEndpoint ep)
        {
            return new ControllerGenerator(ep.EdmModel, ep.DbContext.GetType().FullName, ep.RouteName, ep.Namespace);
        }

        private IDictionary<string, string> _typeMap = new Dictionary<string, string>
        {
            ["Edm.Boolean"] = "bool",
            ["Edm.Byte"] = "byte",
            ["Edm.Int32"] = "int",
            ["Edm.Int64"] = "long",
            ["Edm.Single"] = "float",
            ["Edm.Double"] = "double",
            ["Edm.Date"] = "DateTimeOffset",
            ["Edm.DateTimeOffset"] = "DateTimeOffset",
            ["Edm.String"] = "string",
        };

        public string Type(IEdmTypeReference typeRef)
        {
            var edm = typeRef.FullName();
            if (_typeMap.ContainsKey(edm))
                return _typeMap[edm];

            if (edm.StartsWith("Edm.") == false)
                return edm;

            throw new NotImplementedException();
        }
    }
}
