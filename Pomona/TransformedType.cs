using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Pomona.TestModel;

namespace Pomona
{
    /// <summary>
    /// Represents a type that is transformed
    /// </summary>
    public class TransformedType : IMappedType
    {
        private Type type;



        internal TransformedType(Type type, string name, ClassMappingFactory classMappingFactory)
        {
            if (classMappingFactory == null) throw new ArgumentNullException("classMappingFactory");
            this.type = type;
            this.name = name;
            this.classMappingFactory = classMappingFactory;
        }


        public void FillWithType(Type type)
        {
            foreach (var propInfo in type.GetProperties()
                .Where(x => x.GetGetMethod().IsPublic && x.GetIndexParameters().Count() == 0))
            {
                var propDef = new PropertyMapping(propInfo.Name,
                    classMappingFactory.GetClassMapping(propInfo.DeclaringType),
                    classMappingFactory.GetClassMapping(propInfo.PropertyType),
                    propInfo);

                var propInfoLocal = propInfo;

                // TODO: This is not the most optimized way to set property, small code gen needed.
                propDef.Getter = x => propInfoLocal.GetValue(x, null);
                propDef.Setter = (x, value) => propInfoLocal.SetValue(x, value, null);

                properties.Add(propDef);
            }

            BaseType = classMappingFactory.GetClassMapping(type.BaseType);

            // Find longest (most specific) public constructor
            var longestCtor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            if (longestCtor != null)
            {
                // TODO: match constructor arguments
                foreach (var ctorParam in longestCtor.GetParameters())
                {
                    var matchingProperty = this.properties.FirstOrDefault(x => x.JsonName.ToLower() == ctorParam.Name);
                    if (matchingProperty != null)
                    {
                        matchingProperty.CreateMode = PropertyMapping.PropertyCreateMode.Required;
                        matchingProperty.ConstructorArgIndex = ctorParam.Position;
                    }
                }
            }
            else
            {
                // No public constructor? Default this to 
            }
        }

        private readonly List<PropertyMapping> properties = new List<PropertyMapping>();

        public IList<PropertyMapping> Properties { get { return properties; } }


        private bool createAllowed;
        private bool updateAllowed;
        private string name;
        private readonly ClassMappingFactory classMappingFactory;


        public string Name
        {
            get { return name; }
        }

        public bool IsGenericType
        {
            get { return false; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public IList<IMappedType> GenericArguments
        {
            get { return new IMappedType[] {}; }
        }

        public IMappedType BaseType { get; set; }
    }
}