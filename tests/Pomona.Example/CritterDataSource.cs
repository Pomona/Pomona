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
using System.Linq;

using Nancy.Validation;

using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.Queries;

namespace Pomona.Example
{
    public class CritterDataSource : IPomonaDataSource, IQueryExecutor
    {
        private readonly CritterRepository store;


        public CritterDataSource(CritterRepository store)
        {
            this.store = store;
        }


        public PomonaResponse ApplyAndExecute(IQueryable queryable, PomonaQuery query)
        {
            return this.store.ApplyAndExecute(queryable, query);
        }


        public object Patch<T>(T updatedObject) where T : class
        {
            return this.store.Patch(updatedObject);
        }


        public object Post<T>(T newObject) where T : class
        {
            if (typeof(T) == typeof(FailingThing))
                throw new Exception("Stupid exception from failing thing;");
            if (typeof(T) == typeof(HandledThing) || typeof(T) == typeof(HandledChild))
                throw new InvalidOperationException(
                    "HandledThing and HandledChild should not be handled by DataSource because they have custom handlers.");

            var newCritter = newObject as Critter;
            if (newCritter != null && newCritter.Name != null && newCritter.Name.Length > 50)
                throw new ModelValidationException("Critter can't have name longer than 50 characters.");

            return this.store.Post(newObject);
        }


        public IQueryable<T> Query<T>()
            where T : class
        {
            if (typeof(T)== typeof(HandledThing))
                throw new InvalidOperationException("Error: Should not call data source when querying HandledThing.");
            return this.store.Query<T>();
        }
    }
}