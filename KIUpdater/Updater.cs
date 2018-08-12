using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class Updater
    {
        private InstallLocationFinder installFinder;
        private SavedGamesLocationFinder savedGamesFinder;
        private string extractPath;

        public Updater(InstallLocationFinder installFinder, SavedGamesLocationFinder savedGamesFinder, string extractPath)
        {
            this.installFinder = installFinder;
            this.savedGamesFinder = savedGamesFinder;
            this.extractPath = extractPath;
        }

        public bool Update()
        {
            string installPath = installFinder.Find();

            if (installPath == null)
                return false;

            string savedGamesPath = savedGamesFinder.Find();

            if (savedGamesPath == null)
                return false;

            Console.WriteLine("Updating Files");

            string HooksPath = savedGamesPath + "\\Hooks";

            if (!Directory.Exists(HooksPath))
                Directory.CreateDirectory(HooksPath);

            {
                string KIServerModNewPath = extractPath + "\\ki\\DCSMod\\Hooks\\KI_ServerGameGUI.lua";
                string DestPath = HooksPath + "\\KI_ServerGameGUI.lua";

                File.Copy(KIServerModNewPath, DestPath, true);
                Console.WriteLine("Updated: " + DestPath);
            }

            {
                Console.WriteLine("Unrestricting core files in DCS");
                string missionScriptingPath = extractPath + "\\ki\\DCSCore\\MissionScripting.lua";
                string DestPath = installPath + "\\Scripts\\MissionScripting.lua";

                File.Copy(missionScriptingPath, DestPath, true);
                Console.WriteLine("Updated: " + DestPath);
            }

            CopyFolder(extractPath + "\\ki", ".");

            return true;
        }

        private void CopyFolder(string sourceFolder, string destFolder)
        {  
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string topFolder = Path.GetFileName(destFolder);

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                if (topFolder.ToLower() == "config" && File.Exists(dest))
                {
                    ConfigPatcher patcher = new ConfigPatcher();
                    string patchedConfig = patcher.Patch(dest, file);
                    File.WriteAllText(dest, patchedConfig);
                    Console.WriteLine("Patched: " + dest);
                }
                else
                {
                    File.Copy(file, dest, true);
                    Console.WriteLine("Updated: " + dest);
                }
                
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
    }
}
