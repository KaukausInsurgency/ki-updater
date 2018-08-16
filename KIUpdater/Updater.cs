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

            string ScriptsPath = savedGamesPath + "\\Scripts";
            if (!TryCreateDirectory(ScriptsPath))
            {
                return false;
            }

            string HooksPath = savedGamesPath + "\\Scripts\\Hooks";

            if (!TryCreateDirectory(HooksPath))
            {
                return false;
            }

            string MissionsPath = savedGamesPath + "\\Missions\\Kaukasus Insurgency";

            if (!TryCreateDirectory(MissionsPath))
            {
                return false;
            }

            if (!TryCreateDirectory(MissionsPath + "\\Server") ||
                !TryCreateDirectory(MissionsPath + "\\GameEvents") ||
                !TryCreateDirectory(MissionsPath + "\\SlingloadEvents"))
            {
                return false;
            }

            {
                string KIServerModNewPath = extractPath + "\\ki\\DCSMod\\Hooks\\KI_ServerGameGUI.lua";
                string DestPath = HooksPath + "\\KI_ServerGameGUI.lua";

                if (!TryCopyFile(KIServerModNewPath, DestPath))
                {
                    Console.WriteLine("Could not update KI_ServerGameGUI.lua because of an error");
                    return false;
                }
                else
                {
                    Console.WriteLine("Updated: " + DestPath);
                }             
            }

            {
                string JsonModPath = extractPath + "\\ki\\DCSMod\\JSON.lua";
                string DestPath = savedGamesPath + "\\JSON.lua";

                if (!TryCopyFile(JsonModPath, DestPath))
                {
                    Console.WriteLine("Could not update JSON.lua because of an error");
                    return false;
                }
                else
                {
                    Console.WriteLine("Updated: " + DestPath);
                }
            }

            {
                Console.WriteLine("Unrestricting core files in DCS");
                string missionScriptingPath = extractPath + "\\ki\\DCSCore\\MissionScripting.lua";
                string DestPath = installPath + "\\Scripts\\MissionScripting.lua";

                if (!TryCopyFile(missionScriptingPath, DestPath))
                {
                    Console.WriteLine("Could not update KI_ServerGameGUI.lua because of an error");
                    return false;
                }
                else
                {
                    Console.WriteLine("Updated: " + DestPath);
                }
            }

            try
            {
                CopyFolder(extractPath + "\\ki", ".");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying updates - " + ex.Message);
                return false;
            }

            DCSMissionReader rdr = new DCSMissionReader();
            rdr.UpdateScriptLoadPath(extractPath + "\\ki\\DCSMissions\\KIAlpha.miz", Path.GetFullPath(".\\DCSScripts"));



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

        private bool TryCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating directory - " + ex.Message);
                    return false;
                }
            }
            else
                return true;          
        }

        private bool TryCopyFile(string source, string dest)
        {
            try
            {
                File.Copy(source, dest, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error copying file - " + ex.Message);
                return false;
            }
        }
    }
}
