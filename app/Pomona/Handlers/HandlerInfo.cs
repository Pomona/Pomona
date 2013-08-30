// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System.Reflection;
using Pomona.Common.TypeSystem;

namespace Pomona.Handlers
{
    public class HandlerInfo
    {
        private string httpMethod;

        public string HttpMethod
        {
            get { return httpMethod; }
        }

        public string UriName
        {
            get { return uriName; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

        public IMappedType TargetResourceType
        {
            get { return targetResourceType; }
        }

        public IMappedType FormType
        {
            get { return formType; }
        }

        private string uriName;
        private MethodInfo method;
        private IMappedType targetResourceType;
        private IMappedType formType;

        public HandlerInfo(string httpMethod, string uriName, MethodInfo method, IMappedType targetResourceType, IMappedType formType)
        {
            this.httpMethod = httpMethod;
            this.uriName = uriName;
            this.method = method;
            this.targetResourceType = targetResourceType;
            this.formType = formType;
        }
    }
}