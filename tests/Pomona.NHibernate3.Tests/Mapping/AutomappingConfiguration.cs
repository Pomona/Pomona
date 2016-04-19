#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

using FluentNHibernate;
using FluentNHibernate.Automapping;

namespace Pomona.NHibernate3.Tests.Mapping
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            return type.Namespace == "Pomona.NHibernate3.Tests.Models" && type.IsPublic;
        }


        public override bool ShouldMap(Member member)
        {
            if (member.IsProperty && ((PropertyInfo)member.MemberInfo).GetSetMethod(true) == null)
                return false;
            return base.ShouldMap(member);
        }
    }
}