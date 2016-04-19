#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

namespace Pomona.Common
{
    public struct UniqueMemberToken
    {
        public UniqueMemberToken(int metadataToken, ModuleHandle moduleHandle)
        {
            MetadataToken = metadataToken;
            ModuleHandle = moduleHandle;
        }


        public int MetadataToken { get; }

        public ModuleHandle ModuleHandle { get; }


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
            return MetadataToken.GetHashCode() ^ ModuleHandle.GetHashCode();
        }


        public static bool operator ==(UniqueMemberToken a, UniqueMemberToken b)
        {
            return a.MetadataToken == b.MetadataToken && a.ModuleHandle == b.ModuleHandle;
        }


        public static bool operator !=(UniqueMemberToken a, UniqueMemberToken b)
        {
            return !(a == b);
        }
    }
}