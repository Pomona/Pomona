#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public class ResourceType : StructuredType
    {
        private static readonly MethodInfo convertToPathEncodedStringMethod =
            ReflectionHelper.GetMethodDefinition<ResourceType>(x => ConvertToPathEncodedString(null));

        private static readonly MethodInfo stringBuilderAppendFormatMethod =
            ReflectionHelper.GetMethodDefinition<StringBuilder>(
                x => x.AppendFormat((IFormatProvider)null, "", new object[] { }));

        private readonly Lazy<ResourceTypeDetails> resourceTypeDetails;
        private readonly Lazy<ResourceType> uriBaseType;
        private readonly Lazy<Action<object, StringBuilder>> uriGenerator;


        public ResourceType(IResourceTypeResolver typeResolver, Type type)
            : base(typeResolver, type)
        {
            this.uriBaseType = CreateLazy(() => typeResolver.LoadUriBaseType(this));
            this.resourceTypeDetails = CreateLazy(() => typeResolver.LoadResourceTypeDetails(this));
            this.uriGenerator = CreateLazy(() => BuildUriGenerator(this).Compile());
        }


        public ResourceProperty ChildToParentProperty => ResourceTypeDetails.ChildToParentProperty;

        public ResourceProperty ETagProperty => ResourceTypeDetails.ETagProperty;

        public bool IsChildResource => ResourceTypeDetails.ParentResourceType != null;

        public bool IsExposedAsRepository => ResourceTypeDetails.IsExposedAsRepository;

        public bool IsSingleton => ResourceTypeDetails.IsSingleton;

        public bool IsUriBaseType => UriBaseType == this;

        public IEnumerable<ResourceType> MergedTypes => SubTypes.OfType<ResourceType>();

        public ResourceType ParentResourceType => ResourceTypeDetails.ParentResourceType;

        public ResourceProperty ParentToChildProperty => ResourceTypeDetails.ParentToChildProperty;

        public string PluralName => ResourceTypeDetails.PluralName;

        public StructuredType PostReturnType => ResourceTypeDetails.PostReturnType;

        public new virtual IEnumerable<ResourceProperty> Properties => base.Properties.Cast<ResourceProperty>();

        public IEnumerable<Type> ResourceHandlers => ResourceTypeDetails.ResourceHandlers;

        public ResourceType UriBaseType => this.uriBaseType.Value;

        public string UrlRelativePath => ResourceTypeDetails.UrlRelativePath;

        protected ResourceTypeDetails ResourceTypeDetails => this.resourceTypeDetails.Value;


        public void AppendUri(object o, StringBuilder sb)
        {
            this.uriGenerator.Value(o, sb);
        }


        public string ToUri(object o)
        {
            var sb = new StringBuilder();
            AppendUri(o, sb);
            return sb.ToString();
        }


        protected internal virtual ResourceType OnLoadUriBaseType()
        {
            return this;
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new ResourceProperty(TypeResolver, property, this);
        }


        private static Expression<Action<object, StringBuilder>> BuildUriGenerator(ResourceType rt)
        {
            var parameterExpression = Expression.Parameter(typeof(object), "x");
            var objParam = Expression.Convert(parameterExpression, rt);
            var sbParam = Expression.Parameter(typeof(StringBuilder), "sb");
            var sbArgs = new List<Expression>();
            var formatStringBuilder = new StringBuilder();
            BuildUriGenerator(rt, sbArgs, objParam, formatStringBuilder);
            var sbArgsEncoded =
                sbArgs.Select(
                    GetUrlPathEncodedExpression);

            return Expression.Lambda<Action<object, StringBuilder>>(Expression.Call(sbParam,
                                                                                    stringBuilderAppendFormatMethod,
                                                                                    Expression.Constant(CultureInfo.InvariantCulture),
                                                                                    Expression.Constant(formatStringBuilder.ToString()),
                                                                                    Expression.NewArrayInit(typeof(object), sbArgsEncoded)),
                                                                    parameterExpression,
                                                                    sbParam);
        }


        private static void BuildUriGenerator(ResourceType rt,
                                              List<Expression> sbFormatArgs,
                                              Expression parentExpression,
                                              StringBuilder formatStringBuilder)
        {
            if (rt.PrimaryId == null)
                throw new InvalidOperationException($"{rt.Name} has no Id property or primary key mapping");

            var parentToChildProperty = rt.ParentToChildProperty;
            if (parentToChildProperty != null)
            {
                var childToParentProperty = rt.ChildToParentProperty;
                var nextParentExpr = childToParentProperty.CreateGetterExpression(parentExpression);
                BuildUriGenerator(rt.ParentResourceType, sbFormatArgs, nextParentExpr, formatStringBuilder);
                if (formatStringBuilder.Length > 0)
                    formatStringBuilder.Append('/');
                formatStringBuilder.AppendFormat("{0}", parentToChildProperty.UriName);
                if (parentToChildProperty.PropertyType.IsCollection)
                {
                    formatStringBuilder.AppendFormat("/{{{0}}}", sbFormatArgs.Count);
                    var sbArgsExpr = rt.PrimaryId.CreateGetterExpression(parentExpression);
                    sbFormatArgs.Add(sbArgsExpr);
                }
            }
            else
            {
                if (rt.IsSingleton)
                    formatStringBuilder.AppendFormat("{0}", rt.UrlRelativePath);
                else
                {
                    var sbArgsExpr = rt.PrimaryId.CreateGetterExpression(parentExpression);
                    formatStringBuilder.AppendFormat("{0}/{{{1}}}", rt.UrlRelativePath, sbFormatArgs.Count);
                    sbFormatArgs.Add(sbArgsExpr);
                }
            }
        }


        private static string ConvertToPathEncodedString(object o)
        {
            return HttpUtility.UrlPathSegmentEncode(Convert.ToString(o, CultureInfo.InvariantCulture));
        }


        private static Expression GetUrlPathEncodedExpression(Expression expr)
        {
            if (expr.Type == typeof(int))
                return Expression.Convert(expr, typeof(object));

            return Expression.Call(convertToPathEncodedStringMethod,
                                   Expression.Convert(expr, typeof(object)));
        }
    }
}