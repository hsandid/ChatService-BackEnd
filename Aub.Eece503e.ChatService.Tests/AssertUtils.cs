using Microsoft.AspNetCore.Mvc;
using System.Net;
using Xunit;

namespace Aub.Eece503e.ChatService.Tests
{
    public static class AssertUtils
    {
        public static void HasStatusCode(HttpStatusCode statusCode, IActionResult actionResult)
        {
            Assert.True(actionResult is ObjectResult);
            ObjectResult objectResult = (ObjectResult)actionResult;

            Assert.Equal((int)statusCode, objectResult.StatusCode);
        }
    }
}


