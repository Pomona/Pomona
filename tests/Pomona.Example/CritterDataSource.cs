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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

using Nancy;
using Nancy.Validation;

using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.Handlers;
using Pomona.Internals;

namespace Pomona.Example
{


    public class Root
    {
        private readonly CritterRepository critterRepository;


        public Root(CritterRepository critterRepository)
        {
            this.critterRepository = critterRepository;
        }

        public IQueryable<Critter> Critters { get { return critterRepository.List<Critter>().AsQueryable(); } }


        public IQueryable<Critter> Query(Root root)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Query<TEntity>(Root root)
        {
            throw new NotImplementedException();
        }

    }
    public class CritterDataSource : IPomonaDataSource
    {
        private readonly CritterRepository store;

        public CritterDataSource(CritterRepository store)
        {
            this.store = store;
        }

        public PomonaModule Module { get; set; }

        public T GetById<T>(object id) where T : class
        {
            return store.GetById<T>(id);
        }

        public IQueryable<T> Query<T>()
            where T : class
        {
            return store.Query<T>();
        }

        public object Post<T>(T newObject) where T : class
        {
            var newCritter = newObject as Critter;
            if (newCritter != null && newCritter.Name != null && newCritter.Name.Length > 50)
            {
                throw new ModelValidationException("Critter can't have name longer than 50 characters.");
            }

            return store.Post(newObject);
        }

        public object Patch<T>(T updatedObject) where T : class
        {
            return store.Patch(updatedObject);
        }

        [PomonaMethod("POST")]
        public object Capture(Critter critter, CaptureCommand captureCommand)
        {
            critter.IsCaptured = true;
            return critter;
        }
    }
}