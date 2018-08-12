using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class SavedGamesLocationFinder
    {
        string partialPath;

        public SavedGamesLocationFinder(string partialPath)
        {
            this.partialPath = partialPath;
        }

        public string Find()
        {
            Console.WriteLine("Searching for DCS Saved Games directory...");
            string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string FullPath = UserPath + partialPath;

            if (Directory.Exists(FullPath))
            {
                Console.WriteLine("Found DCS Saved Games directory");
                return FullPath;
            }
            else
            {
                Console.WriteLine("ERROR Could not find DCS Saved Games directory");
                return null;
            }
        }
    }
}
