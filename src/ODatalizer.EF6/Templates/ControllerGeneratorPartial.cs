using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace ODatalizer.EF6.Templates
{
    public partial class ControllerGenerator
    {
        public IEdmModel EdmModel { get; }
        public DbContext DbContext { get; }
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

                return "ODatalizer.EF6.Controllers." + name;
            }
        }
        public int? MaxNestNavigations { get; set; }

        IEnumerable<AssociationSet> AssociationSets { get; set; }

        public ControllerGenerator(ODatalizerEndpoint ep)
        {
            EdmModel = ep.EdmModel;
            DbContext = ep.DbContext;
            DbContextTypeName = ep.DbContext.GetType().FullName;
            MaxNestNavigations = ep.MaxNestNavigations;
            RouteName = ep.RouteName;
            _namespace = ep.Namespace;

            AssociationSets = GetM2MAssociationSets();
        }

        IEnumerable<AssociationSet> GetM2MAssociationSets()
        {
            var type = typeof(System.Data.Entity.Core.Metadata.Edm.AssociationSet);
            var sourceEndProp = type.GetProperty("SourceEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceSetProp = type.GetProperty("SourceSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetEndProp = type.GetProperty("TargetEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetSetProp = type.GetProperty("TargetSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var context = (DbContext as IObjectContextAdapter).ObjectContext;
            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);

            var associationSets = container.AssociationSets.Select(ass => new AssociationSet
            {
                SourceEnd = (AssociationEndMember)sourceEndProp.GetValue(ass),
                TargetEnd = (AssociationEndMember)targetEndProp.GetValue(ass),
                SourceSet = (EntitySet)sourceSetProp.GetValue(ass),
                TargetSet = (EntitySet)targetSetProp.GetValue(ass),
            });

            return associationSets.Where(ass => ass.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && ass.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many).ToList();
        }
        public static ControllerGenerator Create(ODatalizerEndpoint ep)
        {
            return new ControllerGenerator(ep);
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

        bool IsManyToMany(IEdmNavigationProperty nav)
        {
            var id = nav.DeclaringType.FullTypeName() + "@" + nav.Name;
            foreach (var ass in AssociationSets)
            {
                var sourceProp = (System.Reflection.PropertyInfo)ass.SourceEnd.MetadataProperties.FirstOrDefault(m => m.Name == "ClrPropertyInfo")?.Value;
                if (sourceProp != null) {
                    var clrType = (Type)ass.SourceSet.ElementType.MetadataProperties.FirstOrDefault(m => m.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType")?.Value;
                    var _id = clrType.FullName + "@" + sourceProp.Name;
                    if (id == _id)
                        return true;
                }

                var targetProp = (System.Reflection.PropertyInfo)ass.TargetEnd.MetadataProperties.FirstOrDefault(m => m.Name == "ClrPropertyInfo")?.Value;
                if (targetProp != null)
                {
                    var clrType = (Type)ass.TargetSet.ElementType.MetadataProperties.FirstOrDefault(m => m.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType")?.Value;
                    var _id = clrType.FullName + "@" + targetProp.Name;
                    if (id == _id)
                        return true;
                }
            }
            return false;
        }

        class NavigationInfo
        {
            public IEnumerable<IEdmNavigationProperty> Path;
        }

        class PathSegment
        {
            public int Suffix { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public IEnumerable<IEdmStructuralProperty> Keys { get; set; }
            public string KeysNameComma { get; set;  }
            public string KeysTypeNameComma { get; set; }
            public string KeysNameBraceComma { get; set; }
            public string KeysNameCondition { get; set; }
            public EdmMultiplicity Multiplicity { get; set; }
            public bool IsManyToMany { get; set; }
        }

        class AssociationSet
        {
            public AssociationEndMember SourceEnd { get; set; }
            public AssociationEndMember TargetEnd { get; set; }

            public EntitySet SourceSet { get; set; }
            public EntitySet TargetSet { get; set; }
        }
    }
}
