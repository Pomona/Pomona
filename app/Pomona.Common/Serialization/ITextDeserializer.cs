#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.IO;

namespace Pomona.Common.Serialization
{
    public interface ITextDeserializer
    {
        object Deserialize(TextReader textReader, DeserializeOptions options = null);
    }
}

