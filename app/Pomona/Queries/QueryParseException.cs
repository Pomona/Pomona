#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    [Serializable]
    public class QueryParseException : PomonaServerException
    {
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
            ErrorReason = errorReason;
            MemberName = memberName;
        }


        /// <summary>
        /// The reason for why query parsing failed
        /// </summary>
        public QueryParseErrorReason ErrorReason { get; }

        /// <summary>
        /// The name of the member causing the query parsing error.
        /// Will be null when not applicable.
        /// </summary>
        public string MemberName { get; }


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