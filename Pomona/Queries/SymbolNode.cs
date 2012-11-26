#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright � 2012 Karsten Nikolai Strand
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

namespace Pomona.Queries
{
    internal class SymbolNode : NodeBase
    {
        private readonly string name;


        public SymbolNode(string name, IEnumerable<NodeBase> children) : base(NodeType.Symbol, children)
        {
            if (name != null)
                this.name = name;
        }


        protected SymbolNode(NodeType nodeType, string name, IEnumerable<NodeBase> children)
            : base(nodeType, children)
        {
            if (name != null)
                this.name = name;
        }


        public bool HasArguments
        {
            get { return Children.Count > 0; }
        }

        public string Name
        {
            get { return this.name; }
        }


        public override string ToString()
        {
            if (Children.Count == 0)
                return String.Format("{0} {1}", base.ToString(), this.name);
            else
            {
                return String.Format(
                    "{0} {1}({2})",
                    base.ToString(),
                    this.name,
                    string.Join(", ", Children.Select(x => x.ToString())));
            }
        }
    }
}