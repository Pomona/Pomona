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

using System.Collections.Generic;
using System.Linq;

namespace Pomona.Example.SimpleExtraSite
{
    internal class SimpleDataSource : IPomonaDataSource
    {
        private static readonly IList<SimpleExtraData> repository = new List<SimpleExtraData>()
        {
            new SimpleExtraData { Id = 0, TheString = "What" },
            new SimpleExtraData { Id = 1, TheString = "The" },
            new SimpleExtraData { Id = 2, TheString = "BLEEP" }
        };


        public object Patch<T>(T updatedObject) where T : class
        {
            var simpleData = updatedObject as SimpleExtraData;
            repository[simpleData.Id] = simpleData;
            return simpleData;
        }


        public object Post<T>(T newObject) where T : class
        {
            var simpleData = newObject as SimpleExtraData;
            simpleData.Id = repository.Count;
            repository.Add(simpleData);
            return simpleData;
        }


        public IQueryable<T> Query<T>() where T : class
        {
            return repository.Cast<T>().AsQueryable();
        }
    }
}