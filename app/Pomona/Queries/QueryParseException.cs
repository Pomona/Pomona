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
using System.Runtime.Serialization;
using System.Text;
using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    public class QueryParseException : Exception
    {
        public QueryParseException()
        {
        }


        public QueryParseException(string message) : base(message)
        {
        }


        public QueryParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected QueryParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal static QueryParseException Create(ITree parserNode, string message, string parsedString,
                                                   Exception innerException)
        {
            if (parserNode != null && parsedString != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(message);
                var commonErrorNode = parserNode as CommonErrorNode;
                int line = parserNode.Line;
                int charPositionInLine = parserNode.CharPositionInLine;
                if (commonErrorNode != null)
                {
                    line = commonErrorNode.trappedException.Line;
                    charPositionInLine = commonErrorNode.trappedException.CharPositionInLine;
                    sb.AppendFormat("({0})\r\n", commonErrorNode.trappedException.Message);
                }

                sb.AppendFormat("Error on line {0} character {1} of query:\r\n", line,
                                charPositionInLine);
                sb.Append(' ', charPositionInLine);
                sb.AppendLine("|/");
                sb.AppendLine(parsedString);
                message = sb.ToString();
            }

            return new QueryParseException(message, innerException);
        }
    }
}