#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

namespace Pomona
{
    [Serializable]
    public class PomonaMappingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaMappingException"/> class.
        /// </summary>
        public PomonaMappingException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaMappingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public PomonaMappingException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pomona.PomonaMappingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public PomonaMappingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PomonaMappingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected PomonaMappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
