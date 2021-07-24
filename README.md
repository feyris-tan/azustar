# What's this?
This is a small library to inspect multi-volume GNU tar archives in C#

# How to compile?
1. Open up the solution file in Visual Studio.
2. Build the solution, done. No special requirements.

# How to use it?
1. Add a reference to this library to your project.
2. Create an instance of AzusTarFilesystem, passing the FileInfo-s or Stream-s of your volumes into the constructor.
3. Either iterate through the files, or use the RootDirectory property to get a DirectoryInfo-like experience.
