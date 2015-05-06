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


        public XDocAssembly Assembly
        {
            get { return new XDocAssembly(GetOrAddElement("assembly")); }
        }

        public XDocMemberCollection Members
        {
            get { return new XDocMemberCollection(GetOrAddElement("members")); }
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
            return string.Format("{0}:{1}", memberTypePrefix, GetMemberPath(memberInfo));
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
    }
}