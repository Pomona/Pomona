using System;

namespace Pomona.Client
{
    public class LazyProxyInterceptor : IProxyInterceptor
    {
        private readonly string uri;
        private readonly Type pocoType;
        private readonly ClientHelper client;
        private object target;

        public LazyProxyInterceptor(string uri, Type pocoType, ClientHelper client)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (pocoType == null) throw new ArgumentNullException("pocoType");
            if (client == null) throw new ArgumentNullException("client");

            this.uri = uri;
            this.pocoType = pocoType;
            this.client = client;
        }

        public object OnPropertyGet(string propertyName)
        {
            if (target == null)
            {
                target = client.GetUri(uri, pocoType);
            }

            // TODO: Optimize this, maybe OnPropertyGet could provide a lambda to return the prop value from an interface.
            return pocoType.GetProperty(propertyName).GetValue(target, null);
        }

        public void OnPropertySet(string propertyName, object value)
        {
            throw new NotImplementedException();
        }
    }
}