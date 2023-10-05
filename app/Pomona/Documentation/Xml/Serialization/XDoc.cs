#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDoc : XDocElement
    {
        public XDoc()
            : base(CreateRoot())
        {
        }


        public XDoc(XElement node)
            : base(node)
        {
        }


        public XDocAssembly Assembly => new XDocAssembly(GetOrAddElement("assembly"));

        public XDocMemberCollection Members => new XDocMemberCollection(GetOrAddElement("members"));


        public XDocContentContainer GetSummary(MemberInfo memberInfo)
        {
            if (memberInfo.Module.Assembly.GetName().Name != Assembly.Name)
                return null;

            var memberKey = GetMemberKey(memberInfo);
            return
                Members.Where(x => string.Equals(x.Name, memberKey))
                       .Select(x => x.Summary)
                       .FirstOrDefault();
        }


        private static XElement CreateRoot()
        {
            var root = new XElement("doc");
            return root;
        }


        private static string GetMemberKey(MemberInfo memberInfo)
        {
            string memberTypePrefix = null;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    memberTypePrefix = "P";
                    break;
                case MemberTypes.TypeInfo:
                    memberTypePrefix = "T";
                    break;
                default:
                    throw new NotImplementedException("Do not support member type " + memberInfo.MemberType);
            }
            return $"{memberTypePrefix}:{GetMemberPath(memberInfo)}";
        }


        private static string GetMemberPath(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    return $"{GetMemberPath(memberInfo.DeclaringType)}.{memberInfo.Name}";
                case MemberTypes.TypeInfo:
                    return ((Type)memberInfo).FullName;
                default:
                    throw new NotImplementedException("Do not support member type " + memberInfo.MemberType);
            }
        }
    }
}
