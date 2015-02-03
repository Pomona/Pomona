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
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class ResourceTypeDetails
    {
        private readonly PropertyInfo childToParentPropertyInfo;
        private readonly IEnumerable<Type> resourceHandlers;

        private readonly bool isExposedAsRepository;
        private readonly PropertyInfo parentToChildPropertyInfo;
        private readonly Type postReturnType;
        private readonly ResourceType type;
        private readonly string urlRelativePath;


        public ResourceTypeDetails(ResourceType type,
            string urlRelativePath,
            bool isExposedAsRepository,
            Type postReturnType,
            PropertyInfo parentToChildPropertyInfo,
            PropertyInfo childToParentPropertyInfo,
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
            this.resourceHandlers = resourceHandlers;
        }


        public PropertyMapping ChildToParentProperty
        {
            get { return (PropertyMapping)this.type.GetPropertyByName(this.childToParentPropertyInfo.Name, false); }
        }

        public IEnumerable<Type> ResourceHandlers
        {
            get { return this.resourceHandlers; }
        }

        public bool IsExposedAsRepository
        {
            get { return this.isExposedAsRepository; }
        }

        public ResourceType ParentResourceType
        {
            get { return ParentToChildProperty != null ? (ResourceType)ParentToChildProperty.DeclaringType : null; }
        }

        public PropertyMapping ParentToChildProperty
        {
            get
            {
                return this.parentToChildPropertyInfo != null
                    ? (PropertyMapping)
                        this.type.TypeResolver.FromType(this.parentToChildPropertyInfo.DeclaringType).GetPropertyByName(
                            this.parentToChildPropertyInfo.Name,
                            false)
                    : null;
            }
        }

        public TransformedType PostReturnType
        {
            get { return (TransformedType)this.type.TypeResolver.FromType(this.postReturnType); }
        }

        public string UrlRelativePath
        {
            get { return this.urlRelativePath; }
        }
    }
}