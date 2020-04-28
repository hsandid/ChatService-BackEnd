using System;
using System.Linq;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aub.Eece503e.ChatService.Web.Store.DocumentDB
{
    public class DocumentDbConversationStore : IConversationStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);
        
        public DocumentDbConversationStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        //TO-DO
        //Complete functions associated to DocumentDb for Conversations
        //Create DocumentDbConversationEntity
        //Create ToConversation, ToEntity, ToConversationResponseEntry

        public async Task<PostConversationResponse> GetConversation(string conversationId)
        {
            return new PostConversationResponse();
        }

        public async Task AddConversation(PostConversationResponse conversation)
        {
        }
        public async Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenCOnversationTime)
        {
            return new ConversationList();
        }

        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }


    }
}
