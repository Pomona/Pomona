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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Nancy.TinyIoc;

namespace Pomona
{
    public class TinyIoCServiceLocator : IServiceLocator
    {
        private readonly TinyIoCContainer container;

        public TinyIoCServiceLocator(TinyIoCContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return container.Resolve(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return container.Resolve(serviceType, key);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.ResolveAll(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return (TService) container.Resolve(typeof (TService));
        }

        public TService GetInstance<TService>(string key)
        {
            return (TService) container.Resolve(typeof (TService), key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return GetAllInstances(typeof (TService)).Cast<TService>();
        }
    }
}