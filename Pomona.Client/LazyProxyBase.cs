// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

using System;

namespace Pomona.Client
{
    public class LazyProxyBase : IHasResourceUri
    {
        private ClientHelper client;

        private object target;
        private Type targetType;

        private string uri;

        public ClientHelper Client
        {
            get { return client; }
            internal set { client = value; }
        }

        public object Target
        {
            get { return target; }
            internal set { target = value; }
        }

        public Type TargetType
        {
            get { return targetType; }
            internal set { targetType = value; }
        }

        #region IHasResourceUri Members

        public string Uri
        {
            get { return uri; }
            internal set { uri = value; }
        }

        #endregion

        protected object OnPropertyGet(string propertyName)
        {
            if (target == null)
                target = client.GetUri(uri, targetType);

            // TODO: Optimize this, maybe OnPropertyGet could provide a lambda to return the prop value from an interface.
            return targetType.GetProperty(propertyName).GetValue(target, null);
        }


        protected void OnPropertySet(string propertyName, object value)
        {
            throw new NotImplementedException();
        }
    }
}