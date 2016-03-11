#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    public class ErrorStatus
    {
        public ErrorStatus(string message, int errorCode, string member = null, string exception = null)
        {
            Message = message;
            ErrorCode = errorCode;
            Member = member;
            Exception = exception;
        }


        public int ErrorCode { get; }

        public string Exception { get; }

        public string Member { get; }

        public string Message { get; }
    }
}