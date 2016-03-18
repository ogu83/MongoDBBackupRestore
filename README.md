# MongoDBBackupRestore
A WPF User Interface tool for quickly take backups and restore for Mongo DB instances.

## Project Description

A WPF User Interface tool for quickly take backups and restore for Mongo DB instances. More details at https://mongobackup.codeplex.com

This product is created for making manual and periodic backups of MongoDB Instances in Windows Server. Because of that this product needs MongoDB Installed. http://www.mongodb.org/downloads

## New Look of the UI

Thanks to the vfabregat https://www.codeplex.com/site/users/view/vfabregat we have a great improved look and feel powered with Elysium https://elysium.codeplex.com/ for this project.

## Properties

Periodic Daily Backup: If enabled each day at 00:00 a backup of whole instance with all databases will be created.
Each backup could be compressed in a zip file.
Manual backup and restore.
Shows the backup files in a table sorted by Created Date, which enables to user restore and delete backup instances.
Comming Soon

This abilities will be implemented with further versions.
This version could not make backups or restore directly to the datapath (folder) without running mongod.exe like "mongodump --dbpath /data/db/ --out /data/backup/".
Select Database to backup or backup all data bases at that instance.
This version cannot work over SSL (not a problem for local instances).
Resources

For Mongo Connection Mongo C# Official Driver has been used. http://www.nuget.org/packages/mongocsharpdriver
For compression dotnetzip http://dotnetzip.codeplex.com/ has been used.
For more information about Mongo DB Restore & Backup visit
http://docs.mongodb.org/manual/tutorial/backup-with-mongodump/
http://docs.mongodb.org/manual/reference/program/mongodump/
http://docs.mongodb.org/manual/reference/program/mongorestore/
follow me on twitter: twitter.com/oguzkoroglu

my linked in profile : http://tr.linkedin.com/in/oguzkoroglu/

my personal web site : http://oguzkoroglu.net/
