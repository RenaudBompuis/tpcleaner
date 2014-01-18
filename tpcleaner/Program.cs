using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace tpcleaner
{
    class Program
    {
        private static string pathToThumbs;
        private static int countOfDeleted = 0;

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("You must call this utility with the full path to the tpdb8 database.");
                return;
            }

            var dbPath = Path.GetFullPath(args[0]);
            if (!File.Exists(dbPath)) {
                Console.WriteLine("Could not find the file '{0}'", dbPath);
                return;
            }
            pathToThumbs = dbPath + "_files";
            if (!Directory.Exists(pathToThumbs)) {
                Console.WriteLine("Could not find the Thumbs directory '{0}'", pathToThumbs);
                return;
            }
            // Record the amout of free space available on the drive before we start
            var drive = Path.GetPathRoot(dbPath);
            ulong freespaceBefore = 0;
            DriveFreeBytes(drive, out freespaceBefore);

            // Open the database
            var dbEngine = new DAO.DBEngine();
            var db = dbEngine.OpenDatabase(dbPath);
            var rs = db.OpenRecordset("SELECT idThumb FROM Thumbnail ORDER BY idThumb ASC", DAO.RecordsetTypeEnum.dbOpenForwardOnly);

            // Go through each Thumb ID and if there are gaps, 
            // attempt to delete the thumbnails that would be in these gaps.
            int prevID = 0;
            int curID;
            while (!rs.EOF) {
                curID = (int)rs.Fields[0].Value;
                for (int i = prevID; i < curID; i++)
                    DeleteThumb(i);
                rs.MoveNext();
                prevID = curID + 1;
            }
            rs.Close();
            rs = null;
            db.Close();
            db = null;
            dbEngine = null;

            // Write a summary report
            if (countOfDeleted > 0) {
                Console.WriteLine("Deleted a total of '{0}' thumbnails for database '{1}'", countOfDeleted, dbPath);
                // Check how much free space we have now
                ulong freespaceAfter = 0;
                if (DriveFreeBytes(drive, out freespaceAfter)) {
                    var freed = freespaceBefore - freespaceAfter;
                    Console.WriteLine("Total disk space saved: {0:N0} bytes.", freed);
                }
            } else {
                Console.WriteLine("No orphaned thumbnails were found, database is clean.");
            }
        }

        private static void DeleteThumb(int thumbId) {
            // Calculate the thumb directory based on its ID
            // See http://forums.cerious.com/forum/index.php?id=539
            int folderL1 = thumbId / (1627 * 1627);
            int folderL2 = (thumbId - (folderL1 * (1627 * 1627))) / 1627;
            int thumb    = thumbId % 1627;
            string path = string.Format(@"{0}\{1:x3}\{2:x3}\{3:x3}.tn", pathToThumbs, folderL1, folderL2, thumb);
            if (File.Exists(path)) {
                File.Delete(path);
                Console.WriteLine("Deleted ThumbID '{0}' in '{1}'", thumbId, path);
                countOfDeleted++;
            }
        }


        // Pinvoke for API function for Getting the Drive free space before and after we delete the thumbnails
        // Take from http://stackoverflow.com/a/13578940/3811
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
                                                     out ulong lpFreeBytesAvailable,
                                                     out ulong lpTotalNumberOfBytes,
                                                     out ulong lpTotalNumberOfFreeBytes);

        public static bool DriveFreeBytes(string folderName, out ulong freespace) {
            freespace = 0;
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentNullException("folderName");

            if (!folderName.EndsWith("\\")) 
                folderName += '\\';

            ulong free = 0, dummy1 = 0, dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, out free, out dummy1, out dummy2)) {
                freespace = free;
                return true;
            } else {
                return false;
            }
        }
    }
}
