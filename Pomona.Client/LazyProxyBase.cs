#region License

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

#endregion

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
            get { return this.client; }
            internal set { this.client = value; }
        }

        public object Target
        {
            get { return this.target; }
            internal set { this.target = value; }
        }

        public Type TargetType
        {
            get { return this.targetType; }
            internal set { this.targetType = value; }
        }

        #region IHasResourceUri Members

        public string Uri
        {
            get { return this.uri; }
            internal set { this.uri = value; }
        }

        #endregion

        protected object OnPropertyGet(string propertyName)
        {
            if (this.target == null)
                this.target = this.client.GetUri(this.uri, this.targetType);

            // TODO: Optimize this, maybe OnPropertyGet could provide a lambda to return the prop value from an interface.
            return this.targetType.GetProperty(propertyName).GetValue(this.target, null);
        }


        protected void OnPropertySet(string propertyName, object value)
        {
            throw new NotImplementedException();
        }
    }
}