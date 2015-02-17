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
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class GetByIdRoute : Route
    {
        private readonly ComplexProperty idProperty;

        private readonly ResourceType resultItemType;
        private readonly HttpMethod allowedMethods;


        public GetByIdRoute(ResourceType resultItemType, Route parent, HttpMethod allowedMethods)
            : base(10, parent)
        {
            if (resultItemType == null)
                throw new ArgumentNullException("resultItemType");
            this.resultItemType = resultItemType;
            this.allowedMethods = allowedMethods;
            this.idProperty = this.resultItemType.PrimaryId;
            if (this.idProperty == null)
                throw new ArgumentException("Resource in collection needs to have a primary id.");
        }


        public override HttpMethod AllowedMethods
        {
            get { return allowedMethods; }
        }

        public ComplexProperty IdProperty
        {
            get { return this.idProperty; }
        }

        public override TypeSpec InputType
        {
            get { return this.resultItemType; }
        }

        public override TypeSpec ResultType
        {
            get { return this.resultItemType; }
        }


        protected override IEnumerable<Route> LoadChildren()
        {
            return this.resultItemType.GetRoutes(this);
        }


        protected override bool Match(string pathSegment)
        {
            object parsedId;
            return pathSegment.TryParse(this.idProperty.PropertyType, out parsedId);
        }


        protected override string PathSegmentToString()
        {
            return string.Format("{{{0}}}", this.idProperty.JsonName);
        }
    }
}