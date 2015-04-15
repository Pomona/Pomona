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


        public IEnumerator<XDocMember> GetEnumerator()
        {
            return WrapAll().GetEnumerator();
        }


        private IEnumerable<XDocMember> WrapAll()
        {
            return Node.Elements("member").Select(x => new XDocMember(x));
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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


        public bool Remove(XDocMember item)
        {
            if (Contains(item))
            {
                item.Node.Remove();
                return true;
            }
            return false;
        }


        public int Count
        {
            get { return Node.Elements("member").Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}