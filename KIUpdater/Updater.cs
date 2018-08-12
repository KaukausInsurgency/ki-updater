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

            {
                foreach (string p in Directory.GetFiles(extractPath + "\\ki\\DCSScripts", "*.lua", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(p);
                    string topFolder = Path.GetFileName(Path.GetDirectoryName(p));
                    string destPath = "";

                    if (topFolder.ToLower() != "dcsscripts")
                    {
                        if (!Directory.Exists(topFolder))
                            Directory.CreateDirectory(topFolder);

                        destPath = topFolder + "\\" + fileName;
                    }
                    else
                    {
                        destPath = fileName;
                    }
               
                    if (topFolder.ToLower() == "config" && File.Exists(destPath))
                    {
                        ConfigPatcher patcher = new ConfigPatcher();
                        string patchedConfig = patcher.Patch(destPath, p);
                        File.WriteAllText(destPath, patchedConfig);
                        Console.WriteLine("Patched: " + destPath);
                    }
                    else
                    {
                        File.Copy(p, destPath, true);
                        Console.WriteLine("Updated: " + destPath);
                    }

                }
            }

            return true;
        }
    }
}
