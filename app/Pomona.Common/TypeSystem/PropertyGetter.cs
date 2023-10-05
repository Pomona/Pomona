#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public class PropertyGetter
    {
        private readonly Func<object, IContainer, object> del;


        public PropertyGetter(Func<object, IContainer, object> del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));
            this.del = del;
        }


        public object Invoke(object target, IContainer container)
        {
            container = container ?? new NoContainer();
            return this.del(target, container);
        }


        public static explicit operator Func<object, IContainer, object>(PropertyGetter propertyGetter)
        {
            if (propertyGetter == null)
                throw new ArgumentNullException(nameof(propertyGetter));
            return propertyGetter.del;
        }


        public static implicit operator PropertyGetter(Func<object, IContainer, object> del)
        {
            if (del == null)
                return null;
            return new PropertyGetter(del);
        }


        public static implicit operator PropertyGetter(Expression<Func<object, IContainer, object>> expression)
        {
            if (expression == null)
                return null;
            return new PropertyExpressionGetter(expression);
        }
    }
}

