using System.Collections.Generic;

namespace Pomona
{
    public interface IPomonaDataSource
    {
        T GetById<T>(object id);
        ICollection<T> List<T>();
    }
}