using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class SharedPropertyInfo : IPropertyInfo
    {
        private readonly PropertyInfo propertyInfo;
        private readonly ITypeMapper typeMapper;


        internal SharedPropertyInfo(PropertyInfo propertyInfo, ITypeMapper typeMapper)
        {
            this.propertyInfo = propertyInfo;
            this.typeMapper = typeMapper;
        }

        #region Implementation of IPropertyInfo

        public bool AlwaysExpand
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyCreateMode CreateMode
        {
            get { throw new NotImplementedException(); }
        }

        public IMappedType DeclaringType
        {
            get { return typeMapper.GetClassMapping(propertyInfo.DeclaringType); }
        }

        public Func<object, object> Getter
        {
            get { return x => propertyInfo.GetValue(x, null); }
        }

        public Expression CreateGetterExpression(Expression instance)
        {
            throw new NotImplementedException();
        }


        public bool IsWriteable
        {
            get { throw new NotImplementedException(); }
        }

        public string JsonName
        {
            get { return Name.LowercaseFirstLetter(); }
        }

        public string LowerCaseName
        {
            get { return Name.ToLower(); }
        }

        public string Name
        {
            get { return propertyInfo.Name; }
        }

        public IMappedType PropertyType
        {
            get { return typeMapper.GetClassMapping(propertyInfo.PropertyType); }
        }

        public Action<object, object> Setter
        {
            get { return (o, v) => propertyInfo.SetValue(o, v, null); }
        }

        public bool IsPrimaryKey
        {
            get { return false; }
        }

        #endregion
    }
}