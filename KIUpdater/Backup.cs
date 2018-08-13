using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class Backup
    {
        private InstallLocationFinder installFinder;
        private SavedGamesLocationFinder savedGamesFinder;
        private string backupPath;
        private List<string> excludeFiles;
        private string installPath;
        private string savedGamesPath;

        public Backup(InstallLocationFinder installFinder, SavedGamesLocationFinder savedGamesFinder, string backupPath)
        {
            this.installFinder = installFinder;
            this.savedGamesFinder = savedGamesFinder;
            this.backupPath = backupPath;
            excludeFiles = new List<string>
            {
                "KIUpdater.exe",
                "KIUpdater.exe.config",
                "KIUpdater.pdb",
                "RestSharp.dll",
                "Version.txt"
            };
        }

        public bool DeleteBackup()
        {
            try
            {
                if (!Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not delete backup files - " + ex.Message);
                return false;
            }
           
        }

        public bool BackupFiles()
        {
            Console.WriteLine("Backing up files prior to update...");
            installPath = installFinder.Find();
            savedGamesPath = savedGamesFinder.Find();

            if (installPath == null || savedGamesPath == null)
                return false;

            try
            {
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                CopyFolder(".", backupPath, excludeFiles);

                Directory.CreateDirectory(backupPath + "\\DCS_Backup_Hooks");
                string KIModFile = savedGamesPath + "\\Hooks\\KI_ServerGameGUI.lua";
                string BackupKIModFile = backupPath + "\\DCS_Backup_Hooks\\KI_ServerGameGUI.lua";
                if (File.Exists(KIModFile))
                    File.Copy(KIModFile, BackupKIModFile, true);

                Directory.CreateDirectory(backupPath + "\\DCS_Backup_Core");
                string MissionScriptingFile = installPath + "\\Scripts\\MissionScripting.lua";
                string BackupMissionScriptingFile = backupPath + "\\DCS_Backup_Core\\MissionScripting.lua";
                if (File.Exists(MissionScriptingFile))
                    File.Copy(MissionScriptingFile, BackupMissionScriptingFile, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error backing up files - " + ex.Message);
                return false;
            }

            return true;
        }

        public bool Restore()
        {
            Console.WriteLine("Restoring files to previous version...");
            if (installPath == null || savedGamesPath == null)
                return false;

            try
            {
                if (!Directory.Exists(backupPath))
                {
                    Console.WriteLine("Error restoring files - the backup directory was not found!");
                    return false;
                }

                string KIModFile = savedGamesPath + "\\Hooks\\KI_ServerGameGUI.lua";
                string BackupKIModFile = backupPath + "\\DCS_Backup_Hooks\\KI_ServerGameGUI.lua";
                if (File.Exists(BackupKIModFile))
                {
                    File.Copy(BackupKIModFile, KIModFile, true);
                    Directory.Delete(backupPath + "\\DCS_Backup_Hooks", true);
                }
                    

                string MissionScriptingFile = installPath + "\\Scripts\\MissionScripting.lua";
                string BackupMissionScriptingFile = backupPath + "\\DCS_Backup_Core\\MissionScripting.lua";
                if (File.Exists(BackupMissionScriptingFile))
                {
                    File.Copy(BackupMissionScriptingFile, MissionScriptingFile, true);
                    Directory.Delete(backupPath + "\\DCS_Backup_Core", true);
                }
                    



                CopyFolder(backupPath, ".", excludeFiles);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error restoring backup files - " + ex.Message);
                return false;
            }

            Console.WriteLine("Restoring files complete");
            return true;
        }

        private void CopyFolder(string sourceFolder, string destFolder, List<string> excludeFiles)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                bool isExcludedFile = false;
                string name = Path.GetFileName(file);
                foreach(string excludeFile in excludeFiles)
                    if (name.ToLower() == excludeFile.ToLower())
                        isExcludedFile = true;

                if (!isExcludedFile)
                {
                    string dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, true);
                }             
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                if (!name.Contains(backupPath))
                    CopyFolder(folder, dest, excludeFiles);
            }
        }
    }
}
