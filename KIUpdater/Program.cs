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
        private static string VERSION_FILE = "Version.txt";
        private static string DCS_SAVED_GAMES_PATH = "\\Saved Games\\DCS\\Scripts";
        private static string HTTP_DOWNLOAD_URL = "http://kaukasusinsurgency.com/downloads/ki.zip";

        static void Main(string[] args)
        {
            CleanUpTempFiles();
            string versionGUID;
            bool versionInfoExists = false;
            bool versionIsLatest = false;

            if (File.Exists(VERSION_FILE))
            {
                versionGUID = File.ReadAllText(VERSION_FILE);
                versionInfoExists = true;
                Console.WriteLine("Checking version...");
                // do http get call here
            }
            else
            {
                Console.WriteLine("No version information found");
            }

            if (!versionInfoExists || !versionIsLatest)
            {
                Console.WriteLine("Downloading latest release...");
                Downloader downloader = new Downloader(new Extractor(KI_EXTRACT_PATH),
                                                       new InstallLocationFinder("DCS World"),
                                                       new SavedGamesLocationFinder(DCS_SAVED_GAMES_PATH),
                                                       KI_DL_PATH);

                downloader.Download(HTTP_DOWNLOAD_URL);
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
