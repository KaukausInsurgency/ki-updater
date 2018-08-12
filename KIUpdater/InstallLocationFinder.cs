using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    public class InstallLocationFinder
    {
        private const string UNINSTALL_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        private const string CURRENT_USER_PATH = "SOFTWARE\\Eagle Dynamics";
        private string wildcard;

        public InstallLocationFinder(string wildcard)
        {
            this.wildcard = wildcard;
        }

        public string Find()
        {
            string path = SearchUninstallRegistry(wildcard);

            if (path == null)
                path = SearchCurrentUserRegistry(wildcard);

            return path;
        }

        private string SearchUninstallRegistry(string wildcard)
        {
            string path = null;
            try
            {
                foreach (var item in Registry.LocalMachine.OpenSubKey(UNINSTALL_PATH).GetSubKeyNames())
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(UNINSTALL_PATH + "\\" + item);
                    object programName = key.GetValue("DisplayName");

                    if (programName != null && Convert.ToString(programName).IndexOf(wildcard, 0) >= 0)
                    {
                        Console.WriteLine("Found install for " + wildcard);
                        object programInstallPath = key.GetValue("InstallLocation");

                        if (programInstallPath != null)
                            path = Convert.ToString(programInstallPath);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching in registry: " + ex.Message);
            }

            return path;
        }

        private string SearchCurrentUserRegistry(string wildcard)
        {
            string path = null;
            try
            {
                foreach (var item in Registry.CurrentUser.OpenSubKey(CURRENT_USER_PATH).GetSubKeyNames())
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(CURRENT_USER_PATH + "\\" + item);
                    object programName = key.Name;

                    if (programName != null && Convert.ToString(programName).IndexOf(wildcard, 0) >= 0)
                    {
                        Console.WriteLine("Found install for " + wildcard);
                        object programInstallPath = key.GetValue("Path");

                        if (programInstallPath != null)
                        {
                            path = Convert.ToString(programInstallPath);

                            if (Directory.Exists(path))
                                return path;
                        }
                            
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching in registry: " + ex.Message);
            }

            return path;
        }
    }
}
