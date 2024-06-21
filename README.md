Work in progress Azure Function (c#) to identify any application stacks being used by Web Sites in a subscription that are using deprecated stacks.

Two projects:

ApplicationStack - POCO objects for holding Application Stacks information
StacksFunction - Azure Function project which retrieves list of Apps, Application Stack information and then checks each app.

**APIs/SDKs used:**

* Azure SDK for .NET - https://learn.microsoft.com/dotnet/azure/sdk/azure-sdk-for-dotnet
* Azure App Service Rest API - https://learn.microsoft.com/rest/api/appservice/provider/get-web-app-stacks?view=rest-appservice-2023-12-01&tabs=HTTP
