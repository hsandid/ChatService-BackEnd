# Chat Service - Back-End API

## Description

Completed during the Spring 2019-2020 semester by Bassel Shurbaji and Hadi Sandid for the course _EECE503e - Web Services in the Cloud_ .

The project's objective was to learn more about back-end development and CD/CI systems, using the ASP.NET Core framework and the Azure DevOps platform.

## Features

Full API documentation can be found here :

Project offers three main controllers/services (?):

- Profile Microservice

   - Uses Azure Table storage to store profile data
   - Model : Controller <=> Storage Layer
   - Logging available for each microservice event
   
- Images Microservice

   - Uses Azure Blob storage to store images
   - Model : Controller <=> Storage Layer
   - Logging available for each microservice event
   
- Conversations Microservice

   - Uses Azure DocumentDB to store conversations, and their related messages
   - DocumentDB allows support for pagination when fetching conversations or messages
   - Model : Controller <=> Service Layer <=> Storage Layer
   - Logging available for each microservice event

## Testing

Integration tests, Unit tests, and Deployment tests are available for each microservice.

## Requirements

Azure components required for the project to run: 
Azure Table ( You must rename the x and y components in the _appsettings.json_ file accordingly) 
Azure Blob Storage ( You must rename the x and y components in the _appsettings.json_ file accordingly) 
Azure Insights ( You must rename the x and y components in the _appsettings.json_ file accordingly) 
Azure DocumentDB ( You must rename the x and y components in the _appsettings.json_ file accordingly)
