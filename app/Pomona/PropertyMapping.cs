#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona
{
#if false
    public class PropertyMapping : PropertySpec
    {
        #region PropertyAccessMode enum

        #endregion

        private readonly TransformedType declaringType;
        private readonly string name;
        private readonly TransformedType reflectedType;
        private readonly PropertyInfo propertyInfo;
        private readonly TypeSpec propertyType;
        private PropertyInfo normalizedPropertyInfo;


        private IDictionary<string, object> metadata;
        public IDictionary<string, object> Metadata
        {
            get { return this.metadata ?? (this.metadata = new Dictionary<string, object>()); }
        }

        public PropertyMapping(
            string name, TransformedType reflectedType, TransformedType declaringType, TypeSpec propertyType, PropertyInfo propertyInfo)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (reflectedType == null) throw new ArgumentNullException("reflectedType");
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            this.name = name;
            this.reflectedType = reflectedType;
            LowerCaseName = name.ToLower();
            JsonName = name.Substring(0, 1).ToLower() + name.Substring(1);
            UriName = NameUtils.ConvertCamelCaseToUri(name);
            this.declaringType = declaringType;
            this.propertyType = propertyType;
            this.propertyInfo = propertyInfo;
            ConstructorArgIndex = -1;
        }


        public HttpMethod AccessMode { get; set; }
        public HttpMethod ItemAccessMode { get; set; }

        public int ConstructorArgIndex { get; set; }

        public bool IsAttributesProperty { get; set; }

        public bool IsOneToManyCollection
        {
            get { return propertyType.IsCollection; }
        }

        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        private T GetMetadata<T>(string key, Func<T> initializer = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object result;
            if (!Metadata.TryGetValue(key, out result))
            {
                if (initializer != null)
                {
                    result = initializer();
                    Metadata[key] = result;
                }
                else
                {
                    result = default(T);
                }
            }
            return (T)result;
        }


        private void SetMetadata(string key, object value)
        {
            if (value == null)
                Metadata.Remove(key);
            else
                Metadata[key] = value;
        }

        public LambdaExpression Formula { get { return GetMetadata<LambdaExpression>("Formula"); } set { SetMetadata("Formula", value); } }

        public TypeMapper TypeMapper
        {
            get { return declaringType.TypeMapper; }
        }

        private PropertyInfo NormalizedPropertyInfo
        {
            get
            {
                if (propertyInfo == null)
                    return null;

                if (normalizedPropertyInfo == null)
                    normalizedPropertyInfo = propertyInfo.NormalizeReflectedType();

                return normalizedPropertyInfo;
            }
        }


        public string UriName { get; set; }

        public bool ExposedAsRepository { get; set; }
        public bool IsEtagProperty { get; set; }

        public bool AlwaysExpand { get; set; }
        public PropertyCreateMode CreateMode { get; set; }

        public TransformedType DeclaringType {get { return declaringType; }}

        TypeSpec PropertySpec.DeclaringType
        {
            get { return declaringType; }
        }

        public Func<object, object> Getter { get; set; }

        public bool IsPrimaryKey
        {
            get { return DeclaringType.PrimaryId == this; }
        }

        public bool IsWriteable
        {
            get { return this.AccessMode.HasFlag(HttpMethod.Patch) || this.AccessMode.HasFlag(HttpMethod.Post) || this.AccessMode.HasFlag(HttpMethod.Put); }
        }

        public bool IsReadable
        {
            get { return this.AccessMode.HasFlag(HttpMethod.Get); }
        }

        public bool IsSerialized
        {
            get { return IsReadable; }
        }

        public string JsonName { get; set; }
        public string LowerCaseName { get; private set; }

        public string Name
        {
            get { return name; }
        }

        public TypeSpec PropertyType
        {
            get { return propertyType; }
        }

        public Action<object, object> Setter { get; set; }

        public TransformedType ReflectedType
        {
            get { return reflectedType; }
        }

        public Expression CreateGetterExpression(Expression instance)
        {
            if (Formula == null)
                return Expression.MakeMemberAccess(instance, NormalizedPropertyInfo);

            // TODO: Make some assertions here..
            return FindAndReplaceVisitor.Replace(Formula.Body, Formula.Parameters[0], instance);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}::{2}", PropertyType, DeclaringType, Name);
        }
    }
#endif
}