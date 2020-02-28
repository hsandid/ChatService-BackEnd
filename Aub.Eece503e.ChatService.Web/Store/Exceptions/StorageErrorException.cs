using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class StorageErrorException : Exception
    {
        public StorageErrorException()
        {
        }

        public StorageErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
