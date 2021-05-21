
## Client
  The clients do most of the real work. The databases are parsed using the CouchbaseLite SDK available for Windows, Android and iOS. 

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
