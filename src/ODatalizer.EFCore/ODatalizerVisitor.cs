using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODatalizer.EFCore.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ODatalizer.EFCore
{
    public class ODatalizerVisitor
    {
        private readonly static MethodInfo[] _queryableStaticsMethods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);
        private readonly static MethodInfo _where = _queryableStaticsMethods.First(m => m.Name == "Where" && m.GetParameters().Last().ParameterType.ToString() == "System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]]");
        private readonly static MethodInfo _firstOrDefault = _queryableStaticsMethods.First(m => m.Name == "FirstOrDefault" && m.GetParameters().Count() == 1);
        private readonly static MethodInfo _count = _queryableStaticsMethods.First(m => m.Name == "Count" && m.GetParameters().Count() == 1);
        private readonly static Type[] _emptyTypes = Array.Empty<Type>();
        private readonly DbContext _db;
        private readonly Type _dbType;
        private readonly IEnumerable<ITypeConverter> _typeConverters;

        public ODatalizerVisitor(DbContext db, IEnumerable<ITypeConverter> typeConverters)
        {
            _db = db;
            _dbType = db.GetType();
            _typeConverters = typeConverters ?? Enumerable.Empty<ITypeConverter>();
        }
        public bool NotFound { get; protected set; }
        public bool BadRequest { get; protected set; }
        public object Result { get; protected set; }
        public Type ResultType { get; protected set; }
        public Action<object> PropertySetter { get; protected set; }
        public int Index;

        public ODatalizerAuthorizationInfo AuthorizationInfo { get; } = new ODatalizerAuthorizationInfo();

        public async Task VisitAsync(ODataPath path)
        {
            NotFound = false;
            BadRequest = false;
            Result = null;
            ResultType = null;
            PropertySetter = null;
            Index = 0;
            foreach (var segment in path)
            {
                await (segment switch
                {
                    TypeSegment typeSegment => VisitAsync(typeSegment),
                    NavigationPropertySegment navigationPropertySegment => VisitAsync(navigationPropertySegment),
                    EntitySetSegment entitySetSegment => VisitAsync(entitySetSegment),
                    SingletonSegment singletonSegment => VisitAsync(singletonSegment),
                    KeySegment keySegment => VisitAsync(keySegment),
                    PropertySegment propertySegment => VisitAsync(propertySegment),
                    AnnotationSegment annotationSegment => VisitAsync(annotationSegment),
                    OperationImportSegment operationImportSegment => VisitAsync(operationImportSegment),
                    OperationSegment operationSegment => VisitAsync(operationSegment),
                    DynamicPathSegment dynamicPathSegment => VisitAsync(dynamicPathSegment),
                    CountSegment countSegment => VisitAsync(countSegment),
                    FilterSegment filterSegment => VisitAsync(filterSegment),
                    ReferenceSegment referenceSegment => VisitAsync(referenceSegment),
                    EachSegment eachSegment => VisitAsync(eachSegment),
                    NavigationPropertyLinkSegment navigationPropertyLinkSegment => VisitAsync(navigationPropertyLinkSegment),
                    ValueSegment valueSegment => VisitAsync(valueSegment),
                    BatchSegment batchSegment => VisitAsync(batchSegment),
                    BatchReferenceSegment batchReferenceSegment => VisitAsync(batchReferenceSegment),
                    MetadataSegment metadataSegment => VisitAsync(metadataSegment),
                    PathTemplateSegment pathTemplateSegment => VisitAsync(pathTemplateSegment),
                    _ => throw new NotSupportedException()
                });
                Index++;
            }
            
        }

        public Task VisitAsync(TypeSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(NavigationPropertySegment segment)
        {
            var obj = Result;
            var prop = obj.GetType().GetProperty(segment.NavigationProperty.Name);

            Result = prop.GetValue(obj);

            var multiplicity = segment.NavigationProperty.TargetMultiplicity();
            if (multiplicity == EdmMultiplicity.One || multiplicity == EdmMultiplicity.ZeroOrOne)
            {
                ResultType = prop.PropertyType;
                PropertySetter = o => prop.SetValue(obj, o);
            }
            else if (multiplicity == EdmMultiplicity.Many)
            {
                ResultType = prop.PropertyType.GetGenericArguments().First();
                PropertySetter = null;
            }

            AuthorizationInfo.BindProp(segment.NavigationProperty.Name);
            AuthorizationInfo.Add(ResultType.FullName);

            return Task.CompletedTask;
        }

        public Task VisitAsync(EntitySetSegment segment)
        {
            var entityType = _db.Model.FindEntityType(segment.EdmType.AsElementType().FullTypeName()).ClrType;
            var dbSet = _dbType.GetMethod("Set", _emptyTypes).MakeGenericMethod(entityType).Invoke(_db, null);

            if (dbSet == null)
            {
                NotFound = true;
                return Task.CompletedTask;
            }

            Result = dbSet;
            ResultType = entityType;
            AuthorizationInfo.Add(entityType.FullName);
            return Task.CompletedTask;
        }

        public Task VisitAsync(SingletonSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(KeySegment segment)
        {
            var queryable = (Result as IQueryable) ?? (Result as IEnumerable)?.AsQueryable();

            if (queryable == null)
            {
                BadRequest = true;
                return Task.CompletedTask;
            }

            Result = Find(queryable, segment.Keys);
            ResultType = queryable.ElementType;
            return Task.CompletedTask;
        }

        public Task VisitAsync(PropertySegment segment)
        {
            var obj = Result;
            var prop = obj.GetType().GetProperty(segment.Property.Name);
            Result = prop.GetValue(obj);
            ResultType = prop.PropertyType;
            PropertySetter = value => prop.SetValue(obj, value);
            return Task.CompletedTask;
        }

        public Task VisitAsync(AnnotationSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(OperationImportSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(OperationSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(DynamicPathSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(CountSegment segment)
        {
            // $count is handled in EnableQueryAttribute
            return Task.CompletedTask;
        }

        public Task VisitAsync(FilterSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(ReferenceSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(EachSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(NavigationPropertyLinkSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(ValueSegment segment)
        {
            return Task.CompletedTask;
        }

        public Task VisitAsync(BatchSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(BatchReferenceSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(MetadataSegment segment)
        {
            throw new NotSupportedException();
        }

        public Task VisitAsync(PathTemplateSegment segment)
        {
            throw new NotSupportedException();
        }

        private static readonly Type _dateTimeType = typeof(DateTime);
        private static readonly Type _dateTimeOffsetType = typeof(DateTimeOffset);
        private static readonly MethodInfo _compareDateTimeType = _dateTimeType.GetMethod(nameof(DateTime.Compare));
        private static readonly MethodInfo _compareDateTimeOffsetType = _dateTimeOffsetType.GetMethod(nameof(DateTimeOffset.Compare));
        public object Find(IQueryable queryable, IEnumerable<KeyValuePair<string, object>> keys)
        {
            var type = queryable.ElementType;
            var param = Expression.Parameter(type);
            var body = keys.Select(key => {
                var prop = type.GetProperty(key.Key);
                var value = As(key.Value, prop.PropertyType);
                return Expression.Equal(Expression.Property(param, prop), Expression.Constant(value));
            }).Aggregate((l, r) => Expression.And(l, r));
            var lambda = Expression.Lambda(body, param);
            var filter = _where.MakeGenericMethod(type).Invoke(null, new object[] { queryable, lambda });
            var first = _firstOrDefault.MakeGenericMethod(type).Invoke(null, new[] { filter });

            return first;
        }
        public object As(object src, Type dstType)
        {
            var srcType = src.GetType();
            if (dstType == srcType)
                return src;

            var converter = _typeConverters.FirstOrDefault(o => o.ModelType == dstType && o.CanConvertFrom(srcType));
            if (converter != null)
            {
                return converter.Convert(src);
            }

            throw new InvalidCastException();
        }
    }
}