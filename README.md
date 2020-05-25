# Chat Service - Back-End API

## Description

Completed during the Spring 2019-2020 semester by Bassel Shurbaji and Hadi Sandid for the course _EECE503e - Web Services in the Cloud_ .

The project's objective was to learn more about back-end development and CD/CI systems, using the ASP.NET Core framework and the Azure DevOps platform.

## Features

Full API documentation can be found [here]()

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

    - *"AzureStorageSettings:ConnectionString"* should be set to the connection string associated with your Azure Table & Azure Blob Container Storage resources
    - *"AzureStorageSettings:ProfilesTableName"* should be set to the name of the Azure Table you have created.
    - *"AzureStorageSettings:ImagesBlobContainerName"* should be set to the name of the Azure Blob container you have created.
    
- **Azure DocumentDB Storage**

    - *DocumentDbSettings:PrimaryKey"* should be set to the connection string associated with your Azure DocumentDB resource
    - *"DocumentDbSettings:DatabaseName"* should be set to the database name associated with your Azure DocumentDB resource
     - *"DocumentDbSettings:CollectionName"* should be set to the collection name associated with your Azure DocumentDB resource
     - *"DocumentDbSettings:EndpointUrl"* should be set to the endpoint URL associated with your Azure DocumentDB resource
    
- **Logging & Azure Insights**

    - *"ApplicationInsights:InstrumentationKey"* should be set to the instrumentation key associated with your Azure Application Insights resource
