#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Threading;

namespace Pomona.Common.TypeSystem
{
    internal static class VirtualMemberMetadataTokenAllocator
    {
        private static int tokenCounter;


        static VirtualMemberMetadataTokenAllocator()
        {
            tokenCounter = (int)MetadataTokenType.Invalid;
        }


        internal static int AllocateToken()
        {
            return Interlocked.Increment(ref tokenCounter);
        }


        private enum MetadataTokenType
        {
            Module = 0,
            TypeRef = 16777216,
            TypeDef = 33554432,
            FieldDef = 67108864,
            MethodDef = 100663296,
            ParamDef = 134217728,
            InterfaceImpl = 150994944,
            MemberRef = 167772160,
            CustomAttribute = 201326592,
            Permission = 234881024,
            Signature = 285212672,
            Event = 335544320,
            Property = 385875968,
            ModuleRef = 436207616,
            TypeSpec = 452984832,
            Assembly = 536870912,
            AssemblyRef = 587202560,
            File = 637534208,
            ExportedType = 654311424,
            ManifestResource = 671088640,
            GenericPar = 704643072,
            MethodSpec = 721420288,
            String = 1879048192,
            Name = 1895825408,
            BaseType = 1912602624,
            Invalid = 2147483647,
            Mask = 0xff << 24
        }
    }
}
