using System;

namespace OBSWebsocketDotNet
{
    /// <summary>
    /// Thrown if authentication fails
    /// </summary>
    public class AuthFailureException : Exception
    {
    }

    /// <summary>
    /// Thrown when a request receives no response from the server within <see cref="OBSWebsocket.WSTimeout"/>
    /// </summary>
    public class RequestTimeoutException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public RequestTimeoutException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown when the server responds with an error
    /// </summary>
    public class ErrorResponseException : Exception
    {
        /// <summary>
        /// Error Code of exception
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception Message</param>
        /// /// <param name="errorCode">Error Code</param>
        public ErrorResponseException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}