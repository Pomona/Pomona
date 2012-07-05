using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Pomona.TestModel;

namespace Pomona
{
    public class ClassMapping
    {
        private Type type;


        internal ClassMapping(Type type)
        {
            this.type = type;
        }


        public void FillWithType(Type type)
        {
            foreach (var propInfo in type.GetProperties()
                .Where(x => x.GetGetMethod().IsPublic && x.GetIndexParameters().Count() == 0))
            {
                var propDef = new PropertyMapping(propInfo.Name, propInfo);

                var propInfoLocal = propInfo;

                // TODO: This is not the most optimized way to set property, small code gen needed.
                propDef.Getter = x => propInfoLocal.GetValue(x, null);
                propDef.Setter = (x, value) => propInfoLocal.SetValue(x, value, null);

                properties.Add(propDef);
            }

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

        
    }
}