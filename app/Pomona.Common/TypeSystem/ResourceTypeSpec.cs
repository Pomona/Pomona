#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class ResourceType : TransformedType
    {
        private readonly Lazy<ResourceTypeDetails> resourceTypeDetails;
        private readonly Lazy<ResourceType> uriBaseType;


        public ResourceType(IExportedTypeResolver typeResolver, Type type)
            : base(typeResolver, type)
        {
            this.uriBaseType = CreateLazy(() => typeResolver.LoadUriBaseType(this));
            this.resourceTypeDetails = CreateLazy(() => typeResolver.LoadResourceTypeDetails(this));
        }


        public PropertyMapping ChildToParentProperty
        {
            get { return ResourceTypeDetails.ChildToParentProperty; }
        }

        public bool IsExposedAsRepository
        {
            get { return ResourceTypeDetails.IsExposedAsRepository; }
        }

        public bool IsRootResource
        {
            get { return ResourceTypeDetails.ParentResourceType == null; }
        }

        public bool IsUriBaseType
        {
            get { return UriBaseType == this; }
        }

        [PendingRemoval]
        public IEnumerable<ResourceType> MergedTypes
        {
            get { return SubTypes.OfType<ResourceType>(); }
        }

        public ResourceType ParentResourceType
        {
            get { return ResourceTypeDetails.ParentResourceType; }
        }

        public PropertyMapping ParentToChildProperty
        {
            get { return ResourceTypeDetails.ParentToChildProperty; }
        }

        public TransformedType PostReturnType
        {
            get { return ResourceTypeDetails.PostReturnType; }
        }

        public ResourceType UriBaseType
        {
            get { return this.uriBaseType.Value; }
        }

        public string UriRelativePath
        {
            get { return ResourceTypeDetails.UriRelativePath; }
        }

        protected ResourceTypeDetails ResourceTypeDetails
        {
            get { return this.resourceTypeDetails.Value; }
        }


        protected internal virtual ResourceType OnLoadUriBaseType()
        {
            return this;
        }
    }
}