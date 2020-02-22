using Aub.Eece503e.ChatService.Datacontracts;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureTableProfileStore : IProfileStore
    {
        private const string _className = "EECE503E";// we need to think of another appropriate parition key!
        private readonly CloudTable _table;

        public AzureTableProfileStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(options.Value.ProfilesTableName);
        }

        private static ProfileTableEntity ToEntity(Profile profile)
        {
            return new ProfileTableEntity
            {
                PartitionKey = _className, //Inherited
                RowKey = profile.Username, //Inherited
                Firstname = profile.Firstname,
                Lastname = profile.Lastname,
            };
        }




        public Task AddProfile(Profile profile)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProfile(string username)
        {
            throw new NotImplementedException();
        }

        public Task<Profile> GetProfile()
        {
            throw new NotImplementedException();
        }

        public Task UpdateProfile(Profile profile)
        {
            throw new NotImplementedException();
        }
    }
}
