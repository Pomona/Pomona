#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.TypeSystem
{
#if false
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : TypeSpec
    {
        private static readonly Type[] basicWireTypes =
            {
                typeof (int), typeof (double), typeof (float), typeof (string),
                typeof (bool), typeof (decimal), typeof (DateTime), typeof (Uri)
            };

        private static readonly Lazy<StringEnumConverter> stringEnumConverter = new Lazy<StringEnumConverter>();

        private readonly bool isCollection;
        private readonly bool isDictionary;

        private readonly Type mappedType;
        private readonly Type mappedTypeInstance;
        private readonly ITypeMapper typeMapper;

        private IList<PropertySpec> properties;

        public SharedType(Type mappedTypeInstance, ITypeMapper typeMapper)
        {
            if (mappedTypeInstance == null)
                throw new ArgumentNullException("mappedTypeInstance");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");

            this.mappedTypeInstance = mappedTypeInstance;
            mappedType = mappedTypeInstance.IsGenericType
                             ? mappedTypeInstance.GetGenericTypeDefinition()
                             : mappedTypeInstance;
            this.typeMapper = typeMapper;

            var dictMetadataToken = typeof (IDictionary<,>).UniqueToken();
            isDictionary = mappedTypeInstance.UniqueToken() == dictMetadataToken ||
                           mappedTypeInstance.GetInterfaces().Any(x => x.UniqueToken() == dictMetadataToken);

            if (!isDictionary)
            {
                if (mappedType != typeof (string))
                {
                    var collectionMetadataToken = typeof (IEnumerable<>).UniqueToken();
                    isCollection =
                        mappedTypeInstance.UniqueToken() == collectionMetadataToken ||
                        mappedTypeInstance.GetInterfaces().Any(x => x.UniqueToken() == collectionMetadataToken);
                }
            }

            if (isDictionary)
                SerializationMode = TypeSerializationMode.Dictionary;
            else if (isCollection)
                SerializationMode = TypeSerializationMode.Array;
            else
                SerializationMode = TypeSerializationMode.Value;

            GenericArguments = new List<TypeSpec>();

            if (mappedTypeInstance.IsEnum)
                JsonConverter = stringEnumConverter.Value;

            InitializeGenericArguments();
        }

        public ITypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        #region TypeSpec Members

        public TypeSpec BaseType
        {
            get { return typeMapper.GetClassMapping(mappedType.BaseType); }
        }

        public Type CustomClientLibraryType { get; set; }

        public TypeSpec DictionaryKeyType
        {
            get { return DictionaryType.GenericArguments[0]; }
        }

        public TypeSpec DictionaryType
        {
            get
            {
                if (!isDictionary)
                    throw new InvalidOperationException("Type does not implement IDictionary<,>");

                var dictMetadataToken = typeof (IDictionary<,>).UniqueToken();

                if (mappedTypeInstance.UniqueToken() == dictMetadataToken)
                    return this;

                return
                    typeMapper.GetClassMapping(
                        mappedTypeInstance.GetInterfaces().First(x => x.UniqueToken() == dictMetadataToken));
            }
        }

        public TypeSpec DictionaryValueType
        {
            get { return DictionaryType.GenericArguments[1]; }
        }

        public TypeSpec ElementType
        {
            get
            {
                if (!isCollection && !IsNullable)
                    throw new InvalidOperationException("Type is not a collection, so it doesn't have an element type.");

                if (MappedType.IsArray)
                    return typeMapper.GetClassMapping(MappedType.GetElementType());

                if (GenericArguments.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Does not know how to find out what element type this collection is of, it has no generic arguement!");
                }

                return GenericArguments[0];
            }
        }

        public IList<TypeSpec> GenericArguments { get; private set; }

        public bool IsAlwaysExpanded
        {
            get { return !isCollection; }
        }

        public bool IsBasicWireType
        {
            get { return basicWireTypes.Contains(mappedType); }
        }

        public bool IsCollection
        {
            get { return isCollection; }
        }

        public bool IsNullable
        {
            get { return Nullable.GetUnderlyingType(mappedTypeInstance) != null; }
        }

        public bool IsGenericType
        {
            get { return mappedType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public bool IsValueType
        {
            get { return mappedType.IsValueType; }
        }

        public string PluralName
        {
            get { return null; }
        }

        public JsonConverter JsonConverter { get; set; }

        public virtual string Name
        {
            get { return mappedType.Name; }
        }

        public bool MappedAsValueObject
        {
            get { return false; }
        }

        public virtual object Create(IDictionary<PropertySpec, object> args)
        {
            if (MappedTypeInstance.IsAnonymous())
            {
                var ctor = mappedTypeInstance.GetConstructors().Single();
                var ctorArgs = ctor.GetParameters().Select(x => args.First(y => y.Key.Name == x.Name).Value).ToArray();
                return ctor.Invoke(ctorArgs);
            }
            throw new NotImplementedException();
        }

        #endregion

        public bool HasUri
        {
            get { return false; }
        }

        public bool IsDictionary
        {
            get { return isDictionary; }
        }

        public Type MappedType
        {
            get { return mappedType; }
        }

        public Type MappedTypeInstance
        {
            get { return mappedTypeInstance; }
        }

        public PropertySpec PrimaryId
        {
            get
            {
                return
                    Properties.Cast<SharedPropertyInfo>().FirstOrDefault(x => x.PropertyInfo.IsDefined(typeof (ResourceIdPropertyAttribute), true));
            }
        }

        public IList<PropertySpec> Properties
        {
            get { return GetPropertiesLazy(); }
        }

        public TypeSerializationMode SerializationMode { get; set; }

        protected virtual IEnumerable<PropertySpec> OnGetProperties()
        {
            return mappedTypeInstance.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(
                x => new SharedPropertyInfo(x, typeMapper));
        }


        private IList<PropertySpec> GetPropertiesLazy()
        {
            if (properties == null)
                properties = OnGetProperties().ToList();
            return properties;
        }

        public override string ToString()
        {
            if (!IsGenericType)
            {
                return Name;
            }

            return string.Format("{0}<{1}>", Name, string.Join(",", GenericArguments));
        }

        private void InitializeGenericArguments()
        {
            if (mappedTypeInstance.IsGenericType)
            {
                foreach (var genericTypeArg in mappedTypeInstance.GetGenericArguments())
                {
                    if (genericTypeArg == mappedTypeInstance)
                    {
                        // Special case, self referencing generics
                        GenericArguments.Add(this);
                    }
                    else
                        GenericArguments.Add(typeMapper.GetClassMapping(genericTypeArg));
                }
            }
        }
    }
#endif
}