# Chat Service - Back-End API

## Description

Completed during the Spring 2019-2020 semester by Bassel Shurbaji and Hadi Sandid for the course _EECE503e - Web Services in the Cloud_ .

The project's objective was to learn more about back-end development and CD/CI systems, using the ASP.NET Core framework and the Azure DevOps platform.

## Features

Full API documentation can be found [here](https://github.com/hsandid/ChatService/blob/master/APIs_Documentation.md)

The following microservices are available :

- **Profile Microservice**

   - Uses Azure Table storage to store profile data
   - Model : Controller <=> Storage Layer
   - Logging available for each microservice event

- **Images Microservice**

   - Uses Azure Blob storage to store profile images
   - Model : Controller <=> Storage Layer
   - Logging available for each microservice event

- **Conversations Microservice**

   - Uses Azure DocumentDB to store conversations, and their related messages
   - DocumentDB allows support for pagination when fetching conversations or messages
   - Model : Controller <=> Service Layer <=> Storage Layer
   - Logging available for each microservice event

## Testing

Integration tests, Unit tests, and Deployment tests are available for each microservice.

## Requirements

For the Storage/Logging components to work properly, you must configure the following environment variables :

- **Azure Table Storage & Azure Blob Storage**

    - _"AzureStorageSettings:ConnectionString"_ should be set to the connection string associated with your Azure Table & Azure Blob Container Storage resources
    - _"AzureStorageSettings:ProfilesTableName"_ should be set to the name of the Azure Table you have created.
    - _"AzureStorageSettings:ImagesBlobContainerName"_ should be set to the name of the Azure Blob container you have created.

- **Azure DocumentDB Storage**

    - _"DocumentDbSettings:PrimaryKey"_ should be set to the connection string associated with your Azure DocumentDB resource
    - _"DocumentDbSettings:DatabaseName"_ should be set to the database name associated with your Azure DocumentDB resource
    - _"DocumentDbSettings:CollectionName"_ should be set to the collection name associated with your Azure DocumentDB resource
    - _"DocumentDbSettings:EndpointUrl"_ should be set to the endpoint URL associated with your Azure DocumentDB resource

- **Logging & Azure Insights**

    - _"ApplicationInsights:InstrumentationKey"_ should be set to the instrumentation key associated with your Azure Application Insights resource
