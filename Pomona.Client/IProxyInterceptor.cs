namespace Pomona.Client
{
    public interface IProxyInterceptor
    {
        object OnPropertyGet(string propertyName);
        void OnPropertySet(string propertyName, object value);
    }
}