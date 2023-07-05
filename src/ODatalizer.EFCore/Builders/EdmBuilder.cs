using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Linq;

namespace ODatalizer.EFCore.Builders
{
    public class EdmBuilder
    {
        public static IEdmModel Build<TDbContext>(TDbContext db) where TDbContext : DbContext
        {
            var builder = new ODataConventionModelBuilder();

            var dbSetType = typeof(DbSet<>);
            var dbSets = db.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == dbSetType).ToArray();
            var entityTypes = db.Model.GetEntityTypes();

            // Add all Enums
            var clrEnumTypes = entityTypes.SelectMany(t => t.ClrType.GetProperties()).Where(p => p.PropertyType.IsEnum).Select(p => p.PropertyType).Distinct();
            foreach (var type in clrEnumTypes)
            {
                builder.AddEnumType(type);
            }

            // Add Entities
            foreach (var entityType in entityTypes)
            {
                var dbSet = dbSets.FirstOrDefault(p => p.PropertyType.GenericTypeArguments.Contains(entityType.ClrType));
                if (dbSet == null)
                    continue;

                var type = builder.AddEntityType(entityType.ClrType);

                foreach (var key in entityType.FindPrimaryKey().Properties)
                {
                    type.HasKey(key.PropertyInfo);
                }

                builder.AddEntitySet(dbSet.Name, type);
            }

            return builder.GetEdmModel();
        }
    }
}
