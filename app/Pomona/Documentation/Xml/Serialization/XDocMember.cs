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

using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocMember : XDocElement
    {
        public XDocMember(XElement node)
            : base(node)
        {
        }


        public XDocMember()
            : base(new XElement("member"))
        {
        }


        public string Name
        {
            get { return Node.Attribute("name").Value; }
            set { Node.SetAttributeValue("name", value); }
        }

        public XDocContentContainer Summary
        {
            get { return GetDocSection("summary"); }
            set { SetDocSection("summary", value); }
        }


        protected XElement GetOrAddLazyAttachedElement(string name)
        {
            var el = (XElement)Node;
            var childNode = el.Element(name);
            if (childNode == null)
            {
                childNode = new XElement(name);
                el.Add(childNode);
            }
            return childNode;
        }


        private XDocContentContainer GetDocSection(string name)
        {
            var el = Node.Element(name);
            return el != null ? new XDocContentContainer(el) : null;
        }


        private void SetDocSection(string name, XDocContentContainer content)
        {
            Node.Elements(name).Remove();
            if (content != null)
            {
                content.Node.Name = name;
                Node.Add(content.Node);
            }
        }
    }
}