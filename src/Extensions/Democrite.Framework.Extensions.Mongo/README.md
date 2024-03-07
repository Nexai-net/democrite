Storage : Mongodb
=======

Configure [Mongodb](https://www.mongodb.com/) as storage systems. <br/>
This librairy use provider [Orleans.Providers.MongoDB](https://www.nuget.org/packages/Orleans.Providers.MongoDB).


## Cluster Membership

[Orleans](https://learn.microsoft.com/fr-fr/dotnet/orleans/) use a database as meeting point for the nodes and clients. <br />
All nodes state and configuration are stored and visible by any node of the cluster.

To store the cluster information you have multiple way:

### Membership : Manual configuration

```csharp

.UseMongoCluster(b => b.AddConnectionString("127.0.0.1:27017").AddOption((o, cfg) =>
                                     {
                                         o.DatabaseName = "democrite";
                                     })
```

This call create a database "democrite" on mongodb at address "127.0.0.1" on port 27017

### Membership : Use configuration file

```csharp

// Without argument the configuration will by extract from configuration files
.UseMongoCluster()
```

```json
{
  "Democrite": {

    "Cluster": {
      "ConnectionString": "mongodb://127.0.0.1:27017",
      "DatabaseName": "democrite"
    }
  }
}
```

### Membership : Use auto configuration


```csharp
// All cluster configurations will be extract from the config file
.SetupClusterFromConfig()
```

```json
{
  "Democrite": {

    // Register the mongo librairy as extension
    // This part is mandatory if not direct call is done by code
    // .net lazy load dependencies, to automatically setup the mongo source it's required that extensions are loaded
    "Extensions": [
      "Democrite.Framework.Extensions.Mongo"
    ],

    "Cluster": {
      "ConnectionString": "mongodb://127.0.0.1:27017",
      "DatabaseName": "democrite"

      // This key 'AutoConfig' specify the engine witch extension to use
      "AutoConfig" : "Mongo"
    }
  }
}
```