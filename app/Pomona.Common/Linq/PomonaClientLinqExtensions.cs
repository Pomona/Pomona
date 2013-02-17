using System.Linq;

namespace Pomona.Common.Linq
{
    public static class PomonaClientLinqExtensions
    {
        public static IQueryable<T> Query<T>(this IPomonaClient client)
        {
            return new RestQuery<T>(new RestQueryProvider(client, typeof(T)));
        }


        public static IQueryable<TResource> Query<TResource, TPostResponseResource>(
            this ClientRepository<TResource, TPostResponseResource> repository)
            where TResource : class, IClientResource
            where TPostResponseResource : IClientResource
        {
            return new RestQuery<TResource>(new RestQueryProvider(repository.Client, typeof(TResource)));
        }
    }
}