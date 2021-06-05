# Setting up config
Open up the DbViewer.Hub.sln
Edit the **appsettings.json** so that it looks like the following:

Sample
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "AllowedHosts": "*",
  "CurrentDbScanner": "DBViewer.Hub.Services.LocalDbScanner",

  "IOSSimulatorDbScannerOptions": {
    "AppBundleId": "com.your.company.appname",
    "SimulatorId": "1AEB873E-C290-4419-9E8E-D2B66829DA4A",
    "DataRelativePath": "Documents/DataStore"
  },
  "LocalDbScannerOptions": {
    "LocalDbDirectory": "/users/{Username}/CouchbaseDbs"
  }
}

```
# Loading databases from a constant path

- Set the `CurrentDbScanner` to use the value "DBViewer.Hub.Services.LocalDbScanner"
- Set the `LocalDbDirectory` property to the root folder that contains CouchbbaseDb directories.
  - _The directory for `LocalDbDirectory` should look the following:_
  - Database1.cblite2
    - db.sqlite2
    - db.sqlite3-shm
    - db.sqlite-wal

  - Database2
    - db.sqlite2
    - db.sqlite3-shm
    - db.sqlite-wal

**Sample Simulator Directory**
/Users/{Username}/Library/Developer/CoreSimulator/Devices/{SimulatorId}/data/Containers/Data/Application/8800851B-6254-4A77-BEA8-7BB53D01F72A/Documents/DataStore

*_Note: The simulator application directories can change on every app restart so be mindful that you are in the current app folder_*

# Auto-finding databases from a simulator

This is the prefered way to use the Hub with simulators because of the way simulators change folders on each run. It is difficult to keep track of where the current running database is in MacOS.

- Set the `CurrentDbScanner` to use the value **DBViewer.Hub.Services.IOSSimulatorDbScanner**
- Set the `AppBundleId` found in **Info.plist** file under `CFBundleIdentifier`
- Set the `SimulatorId` to be the simulator that is running the application you are testing.
    XCode->Window->Devices and Simulators-> find the device and copy/paste the ID here.
- Set the `DataRelativePath` value as a relative path to the root of the Application Directory

This utilizes an XCode command to pull the current path.  The command is:
`xcrun simctl get_app_container {SimulatorId} {AppBundleId} data`.

To ensure that this tool is available, you'll need to make sure the command line tools are set in XCode.
- XCode->Preferences->Location: Set the command line tools any value. If none are available then you will need to download

Each time the databases are fetched, the latest application data will be found using the command above.