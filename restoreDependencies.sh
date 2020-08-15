#!/bin/sh
dotnet restore ChatService.Client/ChatService.Client.csproj
dotnet restore ChatService.Datacontracts/ChatService.Datacontracts.csproj
dotnet restore ChatService.DeploymentTests/ChatService.DeploymentTests.csproj
dotnet restore ChatService.IntegrationTests/ChatService.IntegrationTests.csproj
dotnet restore ChatService.Web/ChatService.Web.csproj
dotnet restore ChatService.Tests/ChatService.Tests.csproj     
