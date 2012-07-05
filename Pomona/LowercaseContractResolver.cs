using System.Text;

using Newtonsoft.Json.Serialization;

using Pomona.TestModel;

namespace Pomona
{
    public class LowercaseContractResolver : DefaultContractResolver
    {

        private class ConstantValueProvider : IValueProvider
        {
            public void SetValue(object target, object value)
            {
                throw new System.NotImplementedException();
            }


            public object GetValue(object target)
            {
                var entity = (EntityBase)target;
                return string.Format("http://localhost:2222/{0}/{1}", target.GetType().Name.ToLower(), entity.Id);
            }
        }
        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            //properties.Add(new JsonProperty()
            //{
            //    PropertyType = typeof(string),
            //    DeclaringType = type,
            //    Readable = true,
            //    PropertyName = "_url",
            //    ValueProvider = new ConstantValueProvider()
            //});
            return properties;
        }


        protected override string ResolvePropertyName(string propertyName)
        {

            return propertyName.Length > 0
                       ? propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1, propertyName.Length - 1)
                       : propertyName;

            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (var c in propertyName)
            {
                if (!char.IsLower(c) && !first)
                {
                    sb.Append('-');
                }

                sb.Append(char.ToLower(c));

                first = false;
            }

            return sb.ToString();
        }
    }
}