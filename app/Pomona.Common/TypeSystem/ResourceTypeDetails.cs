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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class ResourceTypeDetails
    {
        private readonly PropertyInfo childToParentPropertyInfo;
        private readonly bool isExposedAsRepository;
        private readonly bool isSingleton;
        private readonly PropertyInfo parentToChildPropertyInfo;
        private readonly Type postReturnType;
        private readonly IEnumerable<Type> resourceHandlers;
        private readonly ResourceType type;
        private readonly string urlRelativePath;


        public ResourceTypeDetails(ResourceType type,
                                   string urlRelativePath,
                                   bool isExposedAsRepository,
                                   Type postReturnType,
                                   PropertyInfo parentToChildPropertyInfo,
                                   PropertyInfo childToParentPropertyInfo,
                                   bool isSingleton,
                                   IEnumerable<Type> resourceHandlers)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.type = type;
            this.urlRelativePath = urlRelativePath;
            this.isExposedAsRepository = isExposedAsRepository;
            this.postReturnType = postReturnType;
            this.parentToChildPropertyInfo = parentToChildPropertyInfo;
            this.childToParentPropertyInfo = childToParentPropertyInfo;
            this.isSingleton = isSingleton;
            this.resourceHandlers = resourceHandlers;
        }


        public ResourceProperty ChildToParentProperty
        {
            get { return (ResourceProperty)this.type.GetPropertyByName(this.childToParentPropertyInfo.Name, false); }
        }

        public ResourceProperty ETagProperty
        {
            get { return this.type.Properties.FirstOrDefault(x => x.IsEtagProperty); }
        }

        public bool IsExposedAsRepository
        {
            get { return this.isExposedAsRepository; }
        }

        public bool IsSingleton
        {
            get { return this.isSingleton; }
        }

        public ResourceType ParentResourceType
        {
            get { return ParentToChildProperty != null ? (ResourceType)ParentToChildProperty.DeclaringType : null; }
        }

        public ResourceProperty ParentToChildProperty
        {
            get
            {
                return this.parentToChildPropertyInfo != null
                    ? (ResourceProperty)
                        this.type.TypeResolver.FromType(this.parentToChildPropertyInfo.DeclaringType).GetPropertyByName(
                            this.parentToChildPropertyInfo.Name,
                            false)
                    : null;
            }
        }

        public StructuredType PostReturnType
        {
            get { return (StructuredType)this.type.TypeResolver.FromType(this.postReturnType); }
        }

        public IEnumerable<Type> ResourceHandlers
        {
            get { return this.resourceHandlers; }
        }

        public string UrlRelativePath
        {
            get { return this.urlRelativePath; }
        }
    }
}