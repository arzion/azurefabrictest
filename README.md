#Prerequisites

* Install Visual Studio 2017 with the Azure development and ASP.NET and web development workloads.
* Install the Microsoft Azure Service Fabric SDK
* Run the following command to enable Visual Studio to deploy to the local Service Fabric cluster:

```
powershell Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force -Scope CurrentUser
```

What was done:

## AzureServiceFabric:

* Wirex.Engine.Api as the FabricStateful service that exposes REST fabric API to place an order
* Wirex.Fabric.IntegrationTests allows to test local fabric cluster through the reverse proxy (kinda smoke test just sends the request and expects OK status code)
* Wirex.Engine contains ITradingEngine interface with ReliableTradingEngine implementation that uses RelaibleDictionary as storage of Orders

## Single instance implementation:

* There are 2 more implementations that were done just for fun - fifo order closing algorithm and ordered closing algorithm that is also used in ReliableTradingEngine.
* ITradingEngine covered by integration test with simple test cases
