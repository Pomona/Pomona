#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

namespace Pomona.Queries
{
    internal class StringNode : NodeBase
    {
        private readonly string value;


        public StringNode(string unescapedString) : base(NodeType.StringLiteral, Enumerable.Empty<NodeBase>())
        {
            value = UnescapeText(unescapedString);
        }


        public string Value
        {
            get { return value; }
        }


        public override string ToString()
        {
            return String.Format("{0} '{1}'", base.ToString(), Value);
        }


        private string UnescapeText(string unescapedString)
        {
            if (unescapedString[0] != '\'' || unescapedString[unescapedString.Length - 1] != '\'')
                throw new InvalidOperationException("Don't know how to unescape string, expected quotes around");

            // TODO: Proper unescaping of strings
            return unescapedString.Substring(1, unescapedString.Length - 2).Replace("''", "'");
        }
    }
}