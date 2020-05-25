# API Documentation

_All request/response bodies are in JSON format unless specified otherwise_

## Profiles Microservice

- POST api/profile

   - Request Body :
`{
"Username",
"FirstName",
"LastName",
"ProfilePictureId"
}`
  
   - Response Body : 

- GET api/profile/{username}

   - Request Body : `None`
  
   - Response Body :
   
- PUT api/profile/{username}

   - Request Body : 
`{
"FirstName",
"LastName",
"ProfilePictureId"
}`
  
   - Response Body :

- DELETE api/profile/{username}

   - Request Body : `None`
  
   - Response Body :

## Images Microservice

- POST api/images

   - Request Body : form-data with key 'file' and value <Photo>
  
   - Response Body :

- GET api/images/{imageId}

   - Request Body :
  
   - Response Body : form-data with value <Photo>

- DELETE api/images/{imageId}

   - Request Body : `None` 
  
   - Response Body : {}

## Conversations Microservice

- POST api/conversations

   - Request Body : 
  
   - Response Body :

- POST api/conversations/{conversationId}/messages

   - Request Body :
  
   - Response Body :

- GET api/conversations?username={username}&continuationToken={continuationToken}&limit={limit}
&lastSeenConversationTime={lastSeenConversationTime}

   - Request Body :
  
   - Response Body :
   
- GET api/conversations/{conversationId}/messages? continuationToken={continuationToken}&limit={limit}
&lastSeenMessageTime={lastSeenMessageTime}

   - Request Body :
  
   - Response Body :

