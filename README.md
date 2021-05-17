# Couchbase Lite Database Viewer
https://github.com/jaytilly/Couchbase.Lite.DbViewer/actions/workflows/dotnet.yml/badge.svg

## Project Goal
The single aim of this project is to simplify sharing, viewing and editing CouchbaseLite databases from any mobile device or desktop.

## Components
### Hub
  The hub is the central point that apps can send their DBs to for other clients to share. They can also be used to scan the local machine for databases.  The hub is a super simple http server that serves the couchbase files.
  Platforms such as Linux and Mac aren't able to open Couchbase databases.  Using the hub to send the Couchbase databases to a platform that can is a common use case. 

### Client
  The clients do most of the real work. The databases are parsed using the CouchbaseLite SDK available for Windows, Android and iOS. 

## Viewer clients platforms
- [x] Windows
- [x] Android
- [x] iOS
- [ ] Linux
- [ ] Mac (Hopefully coming soon)

## Hub platforms
- [x] Windows
- [x] Linux
- [x] Mac 
- [ ] Android
- [ ] iOS

## Future Plans
Editing and saving of documents that save back to original device when synced.

### Getting Started
#### Start Hub
Set up the hub making sure to take a look at the **'appsettings.json'** file.  By default it is setup to scan for the test database that is included with the hub.  You can leave this as is if you just want to test.

### Using Client
## Cache Screen (Starts Empty)
You will see this screen:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/CacheScreen_Empty.png" width="300">

## Downloading a Database
From here we will need to add databases by connecting to a **Hub**

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/HubsScreen_Empty.png" width="300">

If the hub is running on the same machine, you can just click **List** and it will make the connection and pull any available databases. Select the database(s) that you want to view and then click **Download**.

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/HubScreen_SelectedDb.png" width="300">

You should now have the sample database downloaded to the client and ready to view:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/CacheScreen_TravelSample.png" width="300">

## Viewing Database
Clicking on the database will open the Database Browser view:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/AllDocument_Screen.png" width="300">

You can group documents by adding the characters to split the document names by. In this case we'll use a '**_**'.

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/AllDocument_Grouped.png" width="300">

You can filter documents by typing in any text in the filter toolbar:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/DocumentFilter.png" width="300">

By clicking on Search in the upper right, you can perform a full text search of the database:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/SeachScreenWithResult.png" width="300">

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/SearchResultDocument.png" width="300">

## Viewing Document
Clicking on any document will open the JSON representation

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/DocumentView.png" width="300">

You can share in any number of ways depending on the device:

<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/ShareScreen.png" width="300">

