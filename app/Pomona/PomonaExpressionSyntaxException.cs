#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona
{
    /// <summary>
    /// Thrown when Pomona discovers a syntax error in a query.
    /// </summary>
    [Serializable]
    public class PomonaExpressionSyntaxException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaExpressionFormatException" /> class.
        /// </summary>
        public PomonaExpressionSyntaxException()
        {
            // TODO: Isn't PomonaQuerySyntaxException a more accurate name for this class? [AUL]
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaExpressionFormatException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PomonaExpressionSyntaxException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaExpressionFormatException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public PomonaExpressionSyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PomonaExpressionSyntaxException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected PomonaExpressionSyntaxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
