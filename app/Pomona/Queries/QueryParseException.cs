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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Antlr.Runtime.Tree;

using Nancy;

namespace Pomona.Queries
{
    [Serializable]
    public class QueryParseException : PomonaServerException
    {
        private readonly QueryParseErrorReason errorReason;
        private readonly string memberName;


        public QueryParseException()
        {
        }


        protected QueryParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        internal QueryParseException(string message,
                                     Exception innerException,
                                     QueryParseErrorReason errorReason,
                                     string memberName)
            : base(message, innerException, HttpStatusCode.BadRequest)
        {
            this.errorReason = errorReason;
            this.memberName = memberName;
        }


        /// <summary>
        /// The reason for why query parsing failed
        /// </summary>
        public QueryParseErrorReason ErrorReason
        {
            get { return this.errorReason; }
        }

        /// <summary>
        /// The name of the member causing the query parsing error.
        /// Will be null when not applicable.
        /// </summary>
        public string MemberName
        {
            get { return this.memberName; }
        }


        internal static QueryParseException Create(ITree parserNode,
                                                   string message,
                                                   string parsedString,
                                                   Exception innerException,
                                                   QueryParseErrorReason? errorReason = null,
                                                   string memberName = null)
        {
            if (parserNode != null && parsedString != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(message);
                var commonErrorNode = parserNode as CommonErrorNode;
                var line = parserNode.Line;
                var charPositionInLine = parserNode.CharPositionInLine;
                if (commonErrorNode != null)
                {
                    line = commonErrorNode.trappedException.Line;
                    charPositionInLine = commonErrorNode.trappedException.CharPositionInLine;
                    sb.AppendFormat("({0})\r\n", commonErrorNode.trappedException.Message);
                }

                sb.AppendFormat("Error on line {0} character {1} of query:\r\n",
                                line,
                                charPositionInLine);
                sb.Append(' ', charPositionInLine);
                sb.AppendLine("|/");
                sb.AppendLine(GetLineOfString(parsedString, line));
                message = sb.ToString();
            }

            return new QueryParseException(message,
                                           innerException,
                                           errorReason ?? QueryParseErrorReason.GenericError,
                                           memberName);
        }


        private static string GetLineOfString(string text, int linenumber)
        {
            return text.Replace("\r", "").Split('\n').Skip(linenumber - 1).FirstOrDefault() ??
                   "(WTF, unable to find line!!)";
        }
    }
}