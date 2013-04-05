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
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    public static class PomonaClientLinqExtensions
    {
        public static IQueryable<T> Query<T>(this IPomonaClient client)
        {
            return new RestQuery<T>(new RestQueryProvider(client, typeof (T)));
        }

        public static IQueryable<T> Query<T>(this IPomonaClient client, Expression<Func<T, bool>> predicate)
        {
            return client.Query<T>().Where(predicate);
        }


        public static IQueryable<TResource> Query<TResource, TPostResponseResource>(
            this ClientRepository<TResource, TPostResponseResource> repository)
            where TResource : class, IClientResource
            where TPostResponseResource : IClientResource
        {
            return new RestQuery<TResource>(new RestQueryProvider(repository.Client, typeof (TResource), repository.Uri));
        }

        public static IQueryable<TResource> Query<TResource, TPostResponseResource>(
            this ClientRepository<TResource, TPostResponseResource> repository,
            Expression<Func<TResource, bool>> predicate)
            where TResource : class, IClientResource
            where TPostResponseResource : IClientResource
        {
            return repository.Query().Where(predicate);
        }
    }
}