#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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