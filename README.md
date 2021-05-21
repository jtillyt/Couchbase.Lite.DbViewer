# Couchbase Lite Database Viewer
<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/actions/workflows/dotnet.yml/badge.svg" />

<br>
<br>

# Project Goal
The single aim of this project is to simplify sharing, viewing and editing CouchbaseLite databases from any mobile device or desktop. 

Couchbase is not available on all platforms such as MacOS. This means that we can't open a Couchbase.Lite DB directly from a simulator. We can use the Hub as a proxy, though, and open the database on a platform that can open Couchbase.Lite databases.

<br>

# Project Roadmap
## Goals
- [x] Read Couchbase.Lite DBs
- [x] Provide Hub for receiving databases from remote systems
- [ ] Allow creation of new database
- [ ] Allow update of database records 
- [ ] Add Hub functionality to add/overrwrite database remotely
- [ ] Cache managment (Delete,Rename,Duplicate)
  
## Client Platforms
- [x] Windows
- [x] Android
- [x] iOS
- [ ] Linux
- [ ] Mac (Hopefully coming soon)

## Hub Platforms
- [x] Windows
- [x] Linux
- [x] Mac 
- [ ] Android
- [ ] iOS

<br>

# Components and Getting Started
## Hub
  The hub is the central point that apps can send their DBs to for other clients to share. They can also be used to scan the local machine for databases.  The hub is a super simple http server that serves the couchbase files.
  
  Platforms such as Linux and Mac are not able to open Couchbase.Lite databases at the time of this writing.  Using the hub to send the Couchbase databases to a platform that can is a common use case.
### [Set up Hub](https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/SettingUpHub.md)
<br>

## Client
  The clients do most of the real work. The databases are parsed using the CouchbaseLite SDK available for Windows, Android and iOS. 
### [Using the Client](https://github.com/jaytilly/Couchbase.Lite.DbViewer/blob/main/media/docs/UsingClient.md)


