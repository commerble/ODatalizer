using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;

namespace ODatalizer.EFCore.Builders
{
    public class EdmBuilder
    {
        public static IEdmModel Build<TDbContext>(TDbContext db) where TDbContext : DbContext
            => Build(db, null);
        public static IEdmModel Build<TDbContext>(TDbContext db, Action<ODataConventionModelBuilder> onModelCreating) where TDbContext : DbContext
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


            if (onModelCreating != null)
            {
                onModelCreating(builder);
            }

            // ODataConventionModelBuilder make ForeignKey be Nullable=True even if the column type of RDB is NOT NULL.
            // Save original column Nullable settings and restore it on model creating.
            builder.OnModelCreating = builder =>
            {
                foreach (var st in builder.StructuralTypes)
                {
                    var et = st as EntityTypeConfiguration;
                    foreach (var nav in st.NavigationProperties)
                    {
                        foreach (var dp in nav.DependentProperties)
                        {
                            if (IsNotNull(et, dp))
                            {
                                var prop = Find(st, dp);
                                if (prop != null)
                                {
                                    prop.NullableProperty = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            return builder.GetEdmModel();
        }

        private static readonly Type _required = typeof(RequiredAttribute);
        private static bool IsNotNull(EntityTypeConfiguration? entityTypeConfiguration, PropertyInfo info)
        {
            if (entityTypeConfiguration?.Keys.Any(key => key.PropertyInfo == info) == true)
                return true;

            if (info.PropertyType.IsPrimitive)
                return true;

            if (info.CustomAttributes.Any(attr => attr.AttributeType == _required))
                return true;

            return false;
        }
        private static PrimitivePropertyConfiguration Find(StructuralTypeConfiguration st, PropertyInfo info)
        {
            return st.Properties.FirstOrDefault(p => p.PropertyInfo == info) as PrimitivePropertyConfiguration;
        }
    }
}
