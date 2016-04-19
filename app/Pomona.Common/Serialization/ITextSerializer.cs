#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.IO;

namespace Pomona.Common.Serialization
{
    public interface ITextSerializer
    {
        void Serialize(TextWriter textWriter, object o, SerializeOptions options = null);
    }
}