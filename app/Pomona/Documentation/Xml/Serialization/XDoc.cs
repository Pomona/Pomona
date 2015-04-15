using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDoc : XDocElement
    {
        public XDoc() : base(CreateRoot())
        {
        }


        private static XElement CreateRoot()
        {
            var root = new XElement("doc");
            return root;
        }


        public XDoc(XElement node)
            : base(node)
        {
        }


        public XDocAssembly Assembly
        {
            get { return new XDocAssembly(GetOrAddElement("assembly")); }
        }

        public XDocMemberCollection Members
        {
            get { return new XDocMemberCollection(GetOrAddElement("members")); }
        }


        private static string GetMemberPath(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    return string.Format("{0}.{1}", GetMemberPath(memberInfo.DeclaringType), memberInfo.Name);
                case MemberTypes.TypeInfo:
                    return ((Type)memberInfo).FullName;
                default:
                    throw new NotImplementedException("Do not support member type " + memberInfo.MemberType);
            }
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
            return string.Format("{0}:{1}", memberTypePrefix, GetMemberPath(memberInfo));
        }

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
    }
}