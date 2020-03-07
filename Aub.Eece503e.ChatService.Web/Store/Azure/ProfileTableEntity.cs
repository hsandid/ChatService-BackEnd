using Microsoft.WindowsAzure.Storage.Table;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class ProfileTableEntity : TableEntity
    {
        public ProfileTableEntity() 
        {
        }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfilePictureId { get; set; }
    }
}

