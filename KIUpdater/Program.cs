using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KIUpdater
{
    class Program
    {
        private static string KI_EXTRACT_PATH = "ki-extract";
        private static string KI_DL_PATH = "ki.zip";
        private static string KI_BACKUP_PATH = "ki-backup";
        private static string VERSION_FILE = "Version.txt";
        private static string DCS_SAVED_GAMES_PATH = "\\Saved Games\\DCS\\Scripts";
        private static string HTTP_GET_URL = "http://localhost:50475";

        static void Main(string[] args)
        {
            CleanUpTempFiles();
            string versionGUID;
            bool versionInfoExists = false;
            bool versionIsLatest = false;

            Console.WriteLine("Communicating with server...");
            // do http get call here
            VersionRequest request = new VersionRequest(HTTP_GET_URL);

            if (request.Response == null)
            {
                Console.WriteLine("Update failed - could not communicate with server");
                Console.ReadKey();
                return;
            }
            else if (request.Response.GUID == "0")
            {
                Console.WriteLine("Update failed - bad version information was received");
                Console.ReadKey();
                return;
            }

            if (File.Exists(VERSION_FILE))
            {
                try
                {
                    versionGUID = File.ReadAllText(VERSION_FILE);
                    versionInfoExists = true;
                    Console.WriteLine("Checking version...");
                    if (versionGUID == request.Response.GUID)
                    {
                        versionIsLatest = true;
                        Console.WriteLine("You are on the latest version! Exiting updater.");
                    }
                    else
                    {
                        Console.WriteLine("New version " + request.Response.Version + " available");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error checking version - " + ex.Message);
                    versionInfoExists = false;
                }             
            }
            else
            {
                Console.WriteLine("No version information found");
            }

            if (!versionInfoExists || !versionIsLatest)
            {
                Backup backuper = new Backup(new InstallLocationFinder("DCS World"),
                                             new SavedGamesLocationFinder(DCS_SAVED_GAMES_PATH),
                                             KI_BACKUP_PATH);
                if (backuper.BackupFiles())
                {
                    Console.WriteLine("Backup completed");

                    Console.WriteLine("Downloading latest release...");
                    Downloader downloader = new Downloader(new Extractor(KI_EXTRACT_PATH),
                                                           new InstallLocationFinder("DCS World"),
                                                           new SavedGamesLocationFinder(DCS_SAVED_GAMES_PATH),
                                                           KI_DL_PATH);

                    if (downloader.Download(request.Response.DownloadURL))
                    {
                        try
                        {
                            File.WriteAllText(VERSION_FILE, request.Response.GUID);
                            Console.WriteLine("Successfully updated to version " + request.Response.Version);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error updating version - " + ex.Message);
                            backuper.Restore();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to update to version " + request.Response.Version);
                        backuper.Restore();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to backup files because of an error - aborting update");
                }

                backuper.DeleteBackup();
            }

            CleanUpTempFiles();

            Console.WriteLine("Done - Press Enter to close");
            Console.ReadLine();
        }

        private static void CleanUpTempFiles()
        {
            if (Directory.Exists(KI_EXTRACT_PATH))
                Directory.Delete(KI_EXTRACT_PATH, true);

            if (File.Exists(KI_DL_PATH))
                File.Delete(KI_DL_PATH);
        }
    }

    
}
