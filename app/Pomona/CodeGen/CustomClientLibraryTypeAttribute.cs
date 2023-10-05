#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.CodeGen
{
    public class CustomClientLibraryTypeAttribute : Attribute
    {
        public CustomClientLibraryTypeAttribute(Type type)
        {
            Type = type;
        }


        public Type Type { get; }
    }
}

