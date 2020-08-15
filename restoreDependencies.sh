#!/bin/sh
dotnet restore ChatService.Client/ChatService.Client.csproj
dotnet restore ChatService.Datacontracts/ChatService.Datacontracts.csproj
dotnet restore ChatService.DeploymentTests/ChatService.DeploymentTests.csproj
dotnet restore ChatService.IntegrationTests/ChatService.IntegrationTests.csproj
dotnet restore ChatService.Web/ChatService.Web.csproj
dotnet restore ChatService.Tests/ChatService.Tests.csproj

  
dotnet build ChatService.Client/ChatService.Client.csproj --configuration Release --no-restore
dotnet build ChatService.Datacontracts/ChatService.Datacontracts.csproj --configuration Release --no-restore
dotnet build ChatService.DeploymentTests/ChatService.DeploymentTests.csproj --configuration Release --no-restore
dotnet build ChatService.IntegrationTests/ChatService.IntegrationTests.csproj --configuration Release --no-restore
dotnet build ChatService.Web/ChatService.Web.csproj --configuration Release --no-restore
dotnet build ChatService.Tests/ChatService.Tests.csproj --configuration Release --no-restore      