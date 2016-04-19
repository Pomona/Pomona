#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

namespace Pomona
{
    public interface IPomonaDataSource
    {
        object Patch<T>(T updatedObject) where T : class;
        object Post<T>(T newObject) where T : class;
        IQueryable<T> Query<T>() where T : class;
    }
}