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
 `{
 "Username",
 "FirstName",
 "LastName",
 "ProfilePictureId"
 }`

- GET api/profile/{username}

   - Request Body : `None`

   - Response Body :
 `{
 "Username",
 "FirstName",
 "LastName",
 "ProfilePictureId"
 }`

- PUT api/profile/{username}

   - Request Body :   
`{
"FirstName",
"LastName",
"ProfilePictureId"
}`

   - Response Body :
 `{
 "Username",
 "FirstName",
 "LastName",
 "ProfilePictureId"
 }`

- DELETE api/profile/{username}

   - Request Body : `None`

   - Response Body :
`{
"Username"
}`

## Images Microservice

- POST api/images

   - Request Body : form-data with key 'file' and value <Photo>

   - Response Body :
`{
    "ImageId"
}`
- GET api/images/{imageId}

   - Request Body : `None`

   - Response Body : form-data with value <Photo>

- DELETE api/images/{imageId}

   - Request Body : `None`

   - Response Body :
 `{
     "ImageId"
 }`

## Conversations Microservice

- POST api/conversations

   - Request Body :

  `{
    "Participants": [
        "Username1",
        "Username2"
    ],
    "FirstMessage": {
        "Id",
        "Text",
        "SenderUsername"
    }
}`

   - Response Body :
 `{
 	"Id"
 	"CreatedUnixTime"
 }`

- POST api/conversations/{conversationId}/messages

   - Request Body :
`{
"Id",
"Text",
"SenderUsername"
}`

   - Response Body :
`{      
  "Id"
  "Text"
  "SenderUsername"
  "UnixTime"
}`

- GET api/conversations?username={username}&continuationToken={continuationToken}&limit={limit}
&lastSeenConversationTime={lastSeenConversationTime}

   - Request Body : `None`

   - Response Body :
`{
 "Conversations" : [
 "Id"
 "LastModifiedUnixTime"
 "Recipient":[
 "Username",
  "FirstName",
  "LastName",
  "ProfilePictureId"
  ]
 ],
 "NextUri"
}`

- GET api/conversations/{conversationId}/messages? continuationToken={continuationToken}&limit={limit}
&lastSeenMessageTime={lastSeenMessageTime}

   - Request Body : `None`

   - Response Body :
`{
  Messages : [
  "Text"
  "SenderUsername"
  "UnixTime"
  ],
  "NextUri"
}`
