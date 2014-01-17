tpcleaner
=========

### Purpose

Clean thumbnail files that were not deleted by [ThumbPlus][1], an image database software from [Cerious Software Inc][2].

### The problem

ThumbPlus uses an Access database by default (with a renamed extension `.tpdb8` instead of `.mdb`) and will save thumbnails within the database.
  
However, to avoid reaching the 2GB size limit for an Access database, you can make ThumbPlus save the thumbnail files in a collection of sub-directories that reside in the same folder as the database.

Unfortunately, ThumbPlus version 9 SP1 and below have a bug that prevent the thumbnail files from being deleted when they should.  
This means that thumbnails will keep accumulating and the space they use will never be freed.

### The solution

`tpcleaner.exe` is a small utility that will delete thumbnails whose records were removed from the database.  

Just save `tpcleaner.exe` in the same folder as your database file, open a command prompt:

    C:\MyThumbPlusDbFolder\tpcleaner.exe "Thumbs.tpdb8"

The software will list the id and path of each deleted thumbnail if it finds any. 

### How it works

Thumbnails are stored in a coded directory structure that represents the unique ID of each thumbnail record in the database.  
For instance:

    Thumbs.tpdb8_files\000\02e\4fd.tn

A description of how the path is derived from the unique thumbnail record ID available on the forum.

`tpcleaner` just scans the `Thumbnails` table in the database and search for gaps in its ID and delete any thumbnail files whose unique ID fits in these gaps. 

  [1]:http://www.cerious.com/thumbnails.shtml
  [2]:http://www.cerious.com/