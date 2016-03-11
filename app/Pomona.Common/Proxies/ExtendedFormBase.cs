#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.Serialization;

namespace Pomona.Common.Proxies
{
    public abstract class ExtendedFormBase<TWrappedResource> : ExtendedFormBase, IExtendedResourceProxy<TWrappedResource>
    {
    }

    public abstract class ExtendedFormBase : ExtendedResourceBase, IPostForm
    {
        bool IPomonaSerializable.PropertyIsSerialized(string propertyName)
        {
            var targetSerializable = WrappedResource as IPomonaSerializable;
            if (targetSerializable == null)
                return false;

            return targetSerializable.PropertyIsSerialized(propertyName);
        }
    }
}