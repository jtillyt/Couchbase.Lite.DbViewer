# Couchbase Lite Database Viewer
<img src="https://github.com/jaytilly/Couchbase.Lite.DbViewer/actions/workflows/dotnet.yml/badge.svg" />

<br>
<br>

# Project Goal
The single aim of this project is to simplify sharing, viewing and editing Couchbase.Lite databases from any mobile device or desktop. 

Couchbase.Lite is not available on all platforms such as MacOS. This means that we can't open a Couchbase.Lite DB directly from a simulator. We can use the Hub as a proxy, though, and open the database on a platform that can open Couchbase.Lite databases.

<br>

# Project Roadmap
## Goals
- [x] Read Couchbase.Lite DBs
- [x] Provide Hub for receiving databases from remote systems
- [x] Add/Remove Edit Database Scanners from client
- [ ] Cache managment (Delete,Rename,Duplicate)
- [ ] Allow update of database records 
- [ ] Save documents / allow adding updating among all cached DBs
- [ ] Allow creation of new database
- [ ] Add Hub functionality to add/overrwrite database remotely
  
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
  
  Platforms such as Linux and Mac are not able to open Couchbase.Lite databases at the time of this writing.  You can use the Hub to relay the files from a platform that does not support Couchbase.Lite to one that does.

  To find the Couchbase.Lite DB files, the Hub contains `Scanners` that search for the databases in different ways.  These are now setup client-side which will be covered in the client documentation.
<br>

## Client Viewer
  The client does the parsing of Couchbase.Lite files. Windows desktop (UWP), Android and iOS are currently being supported.  You can also connect to Hubs to relay files around between platforms and open Couchbase.Lite DBs located on platforms that aren't supported.

Setting up Hubs is also now all done on the client after connecting.

# Setting up Hub
Download the latest binary for the Hub here: [Releases](https://github.com/jaytilly/Couchbase.Lite.DbViewer/releases)


## Mac and Linux
After downloading and unzipping the folder, you will need to set permissions to run the `Hub` executable. Example (you may want to use less permissive options): 

``` chmod +x Hub ```

Then run the app with privilages:

```sudo .\Hub ```

You might also need to go through the process of allowing a binary to run that is from an unknown developer if you have Gatekeeper enabled on Mac.  

[Open Mac Gatekeeper Help](https://support.apple.com/en-us/HT202491)

<br>

# Using the Client
When starting the app, you will be met with a blank screen since we have not yet added any Couchbase.Lite database. We will first need to connect to a Hub, set it up if we haven't yet, and then we can start downloading and viewing databases.


## Connect to Hub
This step assumes that you have a Hub running.  You will need to note the IP address of the computer running the Hub, or, if it is the same machine, you can just leave as `127.0.0.1`.

<img src="docs/media/AddingHub.gif" width="300" />

You will only need to do this once after install.

<br>

## Downloading Database
The Hub comes with a sample DB that we are going to download here. No additional setup is required to try this. Simply go to the Hub and click `Rescan All`, select the `travel-sample` and then `Download`.  

<img src="docs/media/DownloadDatabase.gif" width="300" />

<br>
<br>

# Adding Scanners
The sample database uses a scanner that looks in the same directory as the Hub.  You can add additional databases by adding and configuring additional scanners.

## iOS DB Scanner
The iOS DB Scanner uses XCode to find the location of a particular simulator OR allows you to always scan the active `booted` simulator. You will just need to supply the bundle-id, simulatorid (or `booted`) and the relative path to the root app.

<img src="docs/media/SettingUpIosSimulator.gif" width="300" />

<br>
<br>

## LocalDirectory Scanner
This is a simple path scanner. You only need to provide the path to the location of the Couchbase.Lite root directory.

<img src="docs/media/SettingUpLocalDirectoryScanner.gif" width="300" />