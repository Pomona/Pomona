using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Pomona.Client
{
    public class UpdateProxyBase
    {
        private Dictionary<string, object> updateMap = new Dictionary<string, object>();

        protected object OnPropertyGet(string propertyName)
        {
            object value;
            if (!updateMap.TryGetValue(propertyName, out value))
                throw new InvalidOperationException("Update value for " + propertyName + " has not been set");

            return value;
        }

        protected void OnPropertySet(string propertyName, object value)
        {
            updateMap[propertyName] = value;
        }

        public JObject ToJson()
        {
            var jObject = new JObject();
            foreach (var kvp in updateMap)
            {
                var jsonName = kvp.Key.Substring(0, 1).ToLower() + kvp.Key.Substring(1);
                jObject.Add(jsonName, new JValue(kvp.Value));
            }

            return jObject;
        }
    }
}