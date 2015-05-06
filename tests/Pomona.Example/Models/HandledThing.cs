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

namespace Pomona.Example.Models
{
    public class HandledThing : EntityBase, ISetEtaggedEntity
    {
        private readonly HashSet<HandledChild> children = new HashSet<HandledChild>();
        private string eTag = Guid.NewGuid().ToString();


        public HandledThing()
        {
            SingleChild = new HandledSingleChild(this) { Name = "The loner" };
        }


        public ISet<HandledChild> Children
        {
            get { return this.children; }
        }

        public string ETag
        {
            get { return this.eTag; }
        }

        public int FetchedCounter { get; set; }
        public string Foo { get; set; }
        public string Marker { get; set; }
        public int PatchCounter { get; set; }
        public int QueryCounter { get; set; }
        public HandledSingleChild SingleChild { get; set; }


        public void SetEtag(string newEtagValue)
        {
            this.eTag = newEtagValue;
        }
    }
}