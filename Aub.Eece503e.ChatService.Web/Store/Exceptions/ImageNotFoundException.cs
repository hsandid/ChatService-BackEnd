using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
        public class ImageNotFoundException : Exception
        {
            public ImageNotFoundException(string message) : base(message)
            {
            }
        }
}
