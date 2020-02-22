using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class StorageConflictException : Exception
    {
        public StorageConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
