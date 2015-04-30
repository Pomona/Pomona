#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using System;
using System.Reflection;

namespace Pomona.Common
{
    public struct UniqueMemberToken
    {
        private readonly int metadataToken;
        private readonly ModuleHandle moduleHandle;


        public UniqueMemberToken(int metadataToken, ModuleHandle moduleHandle)
        {
            this.metadataToken = metadataToken;
            this.moduleHandle = moduleHandle;
        }


        public int MetadataToken
        {
            get { return this.metadataToken; }
        }

        public ModuleHandle ModuleHandle
        {
            get { return this.moduleHandle; }
        }


        public override bool Equals(object obj)
        {
            if (obj is UniqueMemberToken)
                return ((UniqueMemberToken)obj) == this;

            return false;
        }


        public static UniqueMemberToken FromMemberInfo(MemberInfo member)
        {
            var module = member.DeclaringType != null ? member.DeclaringType.Module : member.Module;
            return new UniqueMemberToken(member.MetadataToken, module.ModuleHandle);
        }


        public override int GetHashCode()
        {
            return this.metadataToken.GetHashCode() ^ this.moduleHandle.GetHashCode();
        }


        public static bool operator ==(UniqueMemberToken a, UniqueMemberToken b)
        {
            return a.metadataToken == b.metadataToken && a.moduleHandle == b.moduleHandle;
        }


        public static bool operator !=(UniqueMemberToken a, UniqueMemberToken b)
        {
            return !(a == b);
        }
    }
}