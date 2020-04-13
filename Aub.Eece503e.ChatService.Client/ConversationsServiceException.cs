using System;
using System.Net;

namespace Aub.Eece503e.ChatService.Client
{
    public class ConversationsServiceException : Exception
    {
        public ConversationsServiceException(string message, System.Net.HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
