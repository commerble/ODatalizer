using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace ODatalizer.EF6.Builders
{
    public class EdmBuilder
    {
        public static IEdmModel Build<TDbContext>(TDbContext db) where TDbContext : DbContext
        {
            var context = (db as IObjectContextAdapter).ObjectContext;
            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);

            var sourceEndProp = typeof(AssociationSet).GetProperty("SourceEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceSetProp = typeof(AssociationSet).GetProperty("SourceSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var ass = container.AssociationSets.First();
            var sourceEnd = (AssociationEndMember)sourceEndProp.GetValue(ass);
            var sourceSet = (System.Data.Entity.Core.Metadata.Edm.EntitySet)sourceSetProp.GetValue(ass);
            var flag = sourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.One;
            //System.Data.Entity.Core.Metadata.Edm.AssociationEndMember end = ((dynamic)ass).SourceEnd;
            var builder = new ODataConventionModelBuilder();
            var dbSetType = typeof(DbSet<>);
            var dbSets = db.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == dbSetType).ToArray();
            var entityTypes = dbSets.Select(dbset => dbset.PropertyType.GetGenericArguments()[0]);

            // Add all Enums
            var clrEnumTypes = entityTypes.SelectMany(t => t.GetProperties()).Where(p => p.PropertyType.IsEnum).Select(p => p.PropertyType).Distinct();
            foreach (var type in clrEnumTypes)
            {
                builder.AddEnumType(type);
            }

            // Add Entities
            foreach (var entitySet in container.EntitySets)
            {
                var clrType = dbSets.First(dbset => dbset.Name == entitySet.Name).PropertyType.GetGenericArguments()[0];
                var type = builder.AddEntityType(clrType);

                foreach (var key in entitySet.ElementType.KeyMembers)
                {
                    type.HasKey(clrType.GetProperty(key.Name));
                }

                builder.AddEntitySet(entitySet.Name, type);
            }

            return builder.GetEdmModel();
        }
    }
}
