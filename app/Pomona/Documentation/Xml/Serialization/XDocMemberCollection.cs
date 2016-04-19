#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocMemberCollection : XDocElement, ICollection<XDocMember>
    {
        public XDocMemberCollection(XElement node)
            : base(node)
        {
        }


        private IEnumerable<XDocMember> WrapAll()
        {
            return Node.Elements("member").Select(x => new XDocMember(x));
        }


        public void Add(XDocMember item)
        {
            Node.Add(item.Node);
        }


        public void Clear()
        {
            Node.RemoveNodes();
        }


        public bool Contains(XDocMember item)
        {
            if (item == null)
                return false;
            return item.Node.Equals(Node.Parent);
        }


        public void CopyTo(XDocMember[] array, int arrayIndex)
        {
            WrapAll().ToList().CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return Node.Elements("member").Count(); }
        }


        public IEnumerator<XDocMember> GetEnumerator()
        {
            return WrapAll().GetEnumerator();
        }


        public bool IsReadOnly
        {
            get { return false; }
        }


        public bool Remove(XDocMember item)
        {
            if (Contains(item))
            {
                item.Node.Remove();
                return true;
            }
            return false;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}