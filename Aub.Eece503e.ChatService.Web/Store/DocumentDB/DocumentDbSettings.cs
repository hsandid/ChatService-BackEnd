using System;
namespace Aub.Eece503e.ChatService.Web.Store.DocumentDB
{
    public class DocumentDbSettings
    {
        public string EndpointUrl { get; set; }
        public string PrimaryKey { get; set; }
        public int MaxConnectionLimit { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
