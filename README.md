# Object Mapper in C#

This application demonstrates the different features of the object mapper in C#

Contributors: [Dave Bechberger](https://github.com/bechbd) adapted from [here](https://docs.datastax.com/en/developer/csharp-driver/3.12/features/components/mapper/)

## Objectives

* To demonstrate how to use the object mapper to insert data
* To demonstrate how to use the object mapper to retrieve data
* To demonstrate how to use the object mapper to update data
* To demonstrate how to use the object mapper to delete data
  
## Project Layout

* [Program.cs](Program.cs) - The main application file 
* [User.cs](User.cs) - The POCO object that maps to the Cassandra table

## How this Sample Works
This sample works by first making a connection to a Cassandra instance, this is defaulted to `localhost`.  Once the `Cluster` object has been built the mapping between the `User` object and the `users` table is created using this code:

```
MappingConfiguration.Global.Define(
    new Map<User>()
    .TableName("users")
    .PartitionKey(u => u.UserId)
    .Column(u => u.UserId, cm => cm.WithName("id")));
```
One thing to notice with this code is that we are mapping the `UserId` property in the object to the `id` column in the table.

Once we have made this can connected our session this sample contains four functions:

* InsertOperations - This contains the insert operations, including batch inserts, you can perform with the object mapper
* QueryOperations - This contains the query operations you can perform with the object mapper
* UpdateOperations - This contains the update operations you can perform with the object mapper
* DeleteOperations - This contains the delete operations, including batch inserts, you can perform with the object mapper

This sample shows the most common patterns used with the object mapper and is not an exhaustive example of the features and configuration options available within the driver.  For a complete listing of the features and configuration options please check the documentation [here](https://docs.datastax.com/en/developer/csharp-driver/3.12/features/components/mapper/).

## Setup and Running

### Prerequisites
* .NET Core 2.1
* A Cassandra cluster

**Note** This application defaults to connecting to a cluster on localhost. These parameters can be changed on line 29 of Program.cs.

### Running
To run this application use the following command:

`dotnet run`


This will produce output similar to the following:

```
Retrieved 11 users
Retrieved 1 users
Retrieved 1 users
Retrieved UserId: 15b6cca1-c5ed-4bdc-9b4e-625897173065, Name: User 0, Age: 0
Retrieved UserId: 15b6cca1-c5ed-4bdc-9b4e-625897173065, Name: User 0, Age: 0
Retrieved UserId: 0f0dfe3f-4f2e-4638-808b-1bdc5352a7eb, Name: User 3, Age: 3
Retrieved UserId: 0f0dfe3f-4f2e-4638-808b-1bdc5352a7eb, Name: User 3, Age: 3
Retrieved UserId: 15b6cca1-c5ed-4bdc-9b4e-625897173065, Name: Update POCO, Age: 0
Retrieved UserId: 15b6cca1-c5ed-4bdc-9b4e-625897173065, Name: Update CQL, Age: 0
Retrieved 0 users
```

