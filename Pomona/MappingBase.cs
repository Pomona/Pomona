using System;
using System.Linq.Expressions;

namespace Pomona
{
    public class MappingBase<T>
    {
        public void Hide(params Expression<Func<T, object>>[] properties)
        {
            
        }

        public void Rename(Expression<Func<T, object>> prop, string newName)
        {
            
        }
    }
}