#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

using Nancy;

namespace Pomona
{
    [Serializable]
    public class ResourceNotFoundException : PomonaServerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ApplicationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error. </param>
        public ResourceNotFoundException(string message)
            : base(message, null, HttpStatusCode.NotFound)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ApplicationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.NotFound)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

