﻿using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureTableProfileStore : IProfileStore
    {
        private const string _partitionKey = "Key1";// we need to think of another appropriate parition key!
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
                PartitionKey = _partitionKey, //Inherited
                RowKey = profile.Username, //Inherited
                Firstname = profile.Firstname,
                Lastname = profile.Lastname,
            };
        }

        private async Task<ProfileTableEntity> RetrieveProfileEntity(string username)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ProfileTableEntity>(partitionKey: _partitionKey, rowkey: username);
            TableResult tableResult = await _table.ExecuteAsync(retrieveOperation);
            var entity = (ProfileTableEntity)tableResult.Result;
            if (entity == null)
            {
                throw new ProfileNotFoundException($"Profile with username {username} was not found");
            }
            return entity;
        }
        private static Profile ToProfile(ProfileTableEntity entity)
        {
            return new Profile
            {
                Username = entity.RowKey,
                Firstname = entity.Firstname,
                Lastname= entity.Lastname
            };
        }
        public async Task AddProfile(Profile profile)
        {
            ProfileTableEntity entity = ToEntity(profile);
            var insertOperation = TableOperation.Insert(entity);
            try
            {
                await _table.ExecuteAsync(insertOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // conflict
                {
                    throw new ProfileAlreadyExistsException($"Profile {profile.Username} already exists");
                }
                throw new StorageErrorException("Could not write to Azure Table", e);
            }
        }

        public async Task DeleteProfile(string username)
        {
            TableEntity entity = await RetrieveProfileEntity(username);
            var deleteOperation = TableOperation.Delete(entity);
            try
            {
                await _table.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not delete profile in storage, username = {username}", e);
            }
        }

        public async Task<Profile> GetProfile(string username)
        {
            try
            {
                ProfileTableEntity entity = await RetrieveProfileEntity(username);
                return ToProfile(entity);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not read from Azure Table", e);
            }
        }

        public async Task UpdateProfile(Profile profile)
        {
            var entity = ToEntity(profile);
            TableOperation updateOperation = TableOperation.InsertOrReplace(entity);

            try
            {
                await _table.ExecuteAsync(updateOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 412) // precondition failed
                {
                    throw new StorageConflictException("Optimistic concurrency failed", e);
                }
                throw new StorageErrorException($"Could not update profile in storage, username = {profile.Username}", e);
            }
        }
    }
}
