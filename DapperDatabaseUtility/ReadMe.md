# DapperDatabase

## What is this?
DapperDatabaseUtility is a package containing a base query and command handler classes that contain generic synchronous and asynchronous sql database functionality.

## Functionality

### Command Handler
- Insert
- Update
- UpdateByProperty
- Delete
- DeleteWhere
- BulkInsert
- BulkUpdate
- BulkDelete

### Query Handler
- GetById
- GetAllItemsInTable
- Where
- FirstOrDefault

#### Working on
I am trying to work on a function that can easily join multiple tables with a where statement.

## How to use?
It is not that hard to hook up, but does require some work.

### Folder Architecture
It is best to create a seperate project in your solution containing all your database functionality.

![img_1_folderarch](https://user-images.githubusercontent.com/45364086/180491883-c9d03142-7f13-4dab-a9c5-926b74422886.PNG)

As you can see we have the project, the DataAccess folder, a folder for each database we are connecting to, and for each database we have a Dto (Table objects), query handler, and command handler. As you can imagine all queries go into the query handler and all commands (Update,Insert, Delete's) would go into the command handler.

### Dto Objects
The Dto Objects inside of a databases Dto folder should be exact replicas of the table it is representing.

![img_2_dtoObj](https://user-images.githubusercontent.com/45364086/180491938-6349d161-72a9-4659-8c18-910ce423266f.PNG)

The reason for this is because the attributes inside of the class are going to be used to fill out the generic queries and command statements, so they need to be correct.

The most important part of a Dto object is the Primary Key MUST BE THE FIRST PROPERTY IN THE DTO OBJECT CLASS!!

### Install NuGet Package
So now you have the project all set up, you can add the private feed and install the package.

![img_3_getToPackageManager](https://user-images.githubusercontent.com/45364086/180491965-46c3c71b-0b41-4c37-a649-26315de394c9.PNG)

Right click on the project that you want the package in, select "Manage NuGet Packages", then select the setting icon the the far right.
![img_4_addPackage](https://user-images.githubusercontent.com/45364086/180491989-f62f25f2-fdd5-4099-a399-1237a9de2d01.PNG)

Navigate to the NuGetPackageManager => Package Sources, and click on the add icon and enter DapperDatabaseUtilityPackage as the feed name and https://pkgs.dev.azure.com/PBNControls/_packaging/DapperDatabaseUtility%40Prerelease/nuget/v3/index.json as the source.

![img_5_selectFeed](https://user-images.githubusercontent.com/45364086/180492017-f1b5be54-1c0b-4b86-bd24-5f5d731893f4.PNG)

All thats left is selecting the new feed and then go to browse and select the DapperDatabaseUtility package.

### Sql Connection

![img_6_sqlConn](https://user-images.githubusercontent.com/45364086/180492063-4ed2d555-1bcb-4ed0-a5fa-ea323e0423a1.PNG)

The base command and query handlers require a class that contains the ability to distribute a sql connection. So whatever class you have that is being used to establish and distribute sql connections needs to extend this interface.

![img_7_sqlConnExample](https://user-images.githubusercontent.com/45364086/180492088-1c5e0444-522f-4c46-865e-e72f91e49f13.PNG)

This is the ISqlConnectionFactory used in the project as an example to show you how to extend the interface to it.

### Setting up the Command and Query Handler

![img_8_commandExample](https://user-images.githubusercontent.com/45364086/180492113-4912825d-0186-4374-ac92-c6699ad12177.PNG)

This is a example of how the command handler should look

### Whats inside the Base?
![img_9_insideTheBase](https://user-images.githubusercontent.com/45364086/180493722-5b1eeaaa-3dae-44f4-834b-ee879abd9e82.PNG)

Inside a bad command or query handler you have access to the Connection_String_Context you passed it, along with a logger, the connection factory, and a SqlUtilityService which has neat tools for creating sql commands and querys.
