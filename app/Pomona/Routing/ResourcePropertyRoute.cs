#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Collections.Generic;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class ResourcePropertyRoute : Route, ILiteralRoute
    {
        private readonly StructuredProperty property;


        public ResourcePropertyRoute(StructuredProperty property, Route parent)
            : base(0, parent)
        {
            this.property = property;
        }


        public override HttpMethod AllowedMethods
        {
            get { return this.property.AccessMode; }
        }

        public override TypeSpec InputType
        {
            get { return this.property.DeclaringType; }
        }

        public string MatchValue
        {
            get { return this.property.UriName; }
        }

        public StructuredProperty Property
        {
            get { return this.property; }
        }

        public override TypeSpec ResultType
        {
            get { return this.property.PropertyType; }
        }


        protected override IEnumerable<Route> LoadChildren()
        {
            return this.property.GetRoutes(this);
        }


        protected override bool Match(string pathSegment)
        {
            return string.Equals(pathSegment, this.property.UriName, StringComparison.InvariantCultureIgnoreCase);
        }


        protected override string PathSegmentToString()
        {
            return this.property.UriName;
        }
    }
}