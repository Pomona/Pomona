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
using System.Linq;

namespace Pomona.Common.Internals
{
    public static class ClientTypeResolverExtensions
    {
        /// <summary>
        /// This function gets the most subtyped server-known interface implemented by given type.
        /// 
        /// For example lets take a type hierarchy where we got entities IBase and IEntity, where IEntity
        /// inherits from IBase.
        /// 
        /// Then we got some other type like ICustomCrazyType that inherits from IEntity. When given
        /// this type this function will return IEntity type.
        /// </summary>
        /// <param name="client">Client responsible for resolving type. TODO: Might put this in its own interface.</param>
        /// <param name="sourceType">Type to find resource interface on.</param>
        /// <returns>Most subtyped server-known interface implemented by sourceType.</returns>
        public static Type GetMostInheritedResourceInterface(this IClientTypeResolver client, Type sourceType)
        {
            var mostSubtyped = client.GetMostInheritedResourceInterfaceInfo(sourceType);
            if (mostSubtyped == null)
            {
                throw new ArgumentException(
                    "sourceType does not implement any resource-type known to client.",
                    "sourceType");
            }
            return mostSubtyped.InterfaceType;
        }


        public static ResourceInfoAttribute GetResourceInfoForType(this IClientTypeResolver client, Type type)
        {
            ResourceInfoAttribute resourceInfoAttribute;
            if (!client.TryGetResourceInfoForType(type, out resourceInfoAttribute))
            {
                throw new InvalidOperationException(
                    "Unable to find ResourceInfoAttribute for type " + type.FullName);
            }
            return resourceInfoAttribute;
        }


        internal static ResourceInfoAttribute GetMostInheritedResourceInterfaceInfo(
            this IClientTypeResolver client,
            Type sourceType)
        {
            ResourceInfoAttribute sourceTypeResourceInfo;
            if (client.TryGetResourceInfoForType(sourceType, out sourceTypeResourceInfo))
                return sourceTypeResourceInfo;

            var allResourceInfos = sourceType.GetInterfaces().Select(
                x =>
                {
                    ResourceInfoAttribute resourceInfo;
                    if (!client.TryGetResourceInfoForType(x, out resourceInfo))
                        resourceInfo = null;
                    return resourceInfo;
                }).Where(x => x != null).ToList();

            var mostSubtyped = allResourceInfos
                .FirstOrDefault(
                    x =>
                        !allResourceInfos.Any(
                            y => x.InterfaceType != y.InterfaceType && x.InterfaceType.IsAssignableFrom(y.InterfaceType)));

            return mostSubtyped;
        }
    }
}