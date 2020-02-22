using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureStorageSettings
    {
        public string ConnectionString { get; set; }
        public string ProfilesTableName { get; set; }
    }
}
