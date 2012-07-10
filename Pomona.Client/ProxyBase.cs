using System;

namespace Pomona.Client
{
    public class ProxyBase
    {
        private IProxyInterceptor proxyInterceptor;
        public IProxyInterceptor ProxyInterceptor
        {
            get { return proxyInterceptor; }
            set { proxyInterceptor = value; }
        }

        public int OnSomethingCast(object blah)
        {
            return (int) blah;
        }

        public Func<ProxyBase, IProxyInterceptor> MakeGetterFuncWrapper()
        {
            return x => x.ProxyInterceptor;
        }

        public ProxyBase CastIt()
        {
            return (ProxyBase)OnPropertyGet("promp");
        }

        protected object OnPropertyGet(string propertyName)
        {
            return ProxyInterceptor.OnPropertyGet(propertyName);
        }
        protected void OnPropertySet(string propertyName, object value)
        {
            ProxyInterceptor.OnPropertySet(propertyName, value);
        }
    }
}