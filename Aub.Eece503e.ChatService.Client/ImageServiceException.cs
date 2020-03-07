using System;
using System.Net;

namespace Aub.Eece503e.ChatService.Client
{
    public class ImageServiceException : Exception
    {
        public ImageServiceException(string message, System.Net.HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
