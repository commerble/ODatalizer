using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ODatalizer.EFCore.Templates
{
    public partial class ControllerGenerator
    {
        public IEdmModel EdmModel { get; }
        public DbContext DbContext { get; }
        public string DbContextTypeName { get; }
        public string RouteName { get; }
        public int PageSize { get; }
        public int MaxExpansionDepth { get; set; }
        public bool Authorize { get; }

        public string RouteNameValue => RouteName != null ? '"' + RouteName + '"' : "null";

        private readonly string _routePrefix;
        public string RoutePrefix => string.IsNullOrEmpty(_routePrefix) ? string.Empty : $"{_routePrefix}/";

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
        public int? MaxNestNavigations { get; set; }

        public ControllerGenerator(ODatalizerEndpoint ep)
        {
            EdmModel = ep.EdmModel;
            DbContext = ep.DbContext;
            DbContextTypeName = ep.DbContext.GetType().FullName;
            RouteName = ep.RouteName;
            PageSize = ep.PageSize;
            MaxExpansionDepth = ep.MaxExpansionDepth;
            Authorize = ep.Authorize;
            _namespace = ep.Namespace;
            _routePrefix = ep.RoutePrefix;
        }

        public static ControllerGenerator Create(ODatalizerEndpoint ep)
        {
            return new ControllerGenerator(ep);
        }

        private IDictionary<string, string> _typeMap = new Dictionary<string, string>
        {
            ["Edm.Binary"] = "byte[]",
            ["Edm.Boolean"] = "bool",
            ["Edm.Byte"] = "byte",
            ["Edm.SByte"] = "sbyte",
            ["Edm.Int16"] = "short",
            ["Edm.Int32"] = "int",
            ["Edm.Int64"] = "long",
            ["Edm.Single"] = "float",
            ["Edm.Double"] = "double",
            ["Edm.Decimal"] = "decimal",
            ["Edm.Date"] = "DateTimeOffset",
            ["Edm.DateTimeOffset"] = "DateTimeOffset",
            ["Edm.Time"] = "TimeSpan",
            ["Edm.Guid"] = "Guid",
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

        public string Type(string fullName, string propName)
        {
            var type = DbContext.GetType().Assembly.GetType(fullName, false)?.GetProperty(propName)?.PropertyType.Name;
            if (type == "Int32")
                return "int";
            if (type == "Int64")
                return "long";
            if (type != null)
                return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) 
            {
                type = asm.GetType(fullName, false)?.GetProperty(propName)?.PropertyType.Name;
                if (type != null) 
                    return type;
            }
            return null;
        }

        public bool IsSkipNavigation(IEdmNavigationPropertyBinding bind)
        {
            var skipNavigation = DbContext.Model.FindEntityType(bind.NavigationProperty.DeclaringType.FullTypeName()).FindDeclaredSkipNavigation(bind.NavigationProperty.Name);
            
            return skipNavigation != null;
        }
    }
}
