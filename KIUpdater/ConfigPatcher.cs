using DiffMatchPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class ConfigPatcher
    {
        public ConfigPatcher()
        {

        }

        public string Patch(string sourceConfigPath, string destConfigPath)
        {
            string patchedFile = null;

            if (!(File.Exists(sourceConfigPath) && File.Exists(destConfigPath)))
            {
                Console.WriteLine("Config Patch error - could not find source or destination files");
                return patchedFile;
            }

            string sourceData = File.ReadAllText(sourceConfigPath);
            string destData = File.ReadAllText(destConfigPath);

            List<string> luaConfigs = new List<string>();
            int openBraces = 0;

            foreach (string l in File.ReadAllLines(destConfigPath))
            {
                if (l.Contains('{'))
                    ++openBraces;
                if (l.Contains('}'))
                    --openBraces;

                if (l.Contains('=') && openBraces == 0)
                {
                    string def = l.Substring(0, l.IndexOf('='));
                    def = def.Trim().Trim('\t').Trim('\r').Trim('\n');
                    luaConfigs.Add(def);
                }

            }

            diff_match_patch dmp = new diff_match_patch();
            List<Diff> diff = dmp.diff_main(sourceData, destData);
            dmp.diff_cleanupSemantic(diff);

            List<Patch> patches = dmp.patch_make(sourceData, diff);

            List<Patch> filteredPatches = new List<Patch>();

            foreach (Patch p in patches)
            {
                bool foundPatch = false;
                foreach (Diff d in p.diffs)
                {
                    if (d.operation == Operation.INSERT || d.operation == Operation.DELETE)
                    {
                        foreach (string f in luaConfigs)
                        {
                            if (d.text.IndexOf(f) >= 0)
                            {
                                filteredPatches.Add(p);
                                foundPatch = true;
                                break;
                            }
                        }
                    }

                    if (foundPatch)
                        break;
                }
            }

            Console.WriteLine("Applying the following patches: ");
            foreach (Patch p in filteredPatches)
                Console.WriteLine(p);

            object[] v = dmp.patch_apply(filteredPatches, sourceData);

            return Convert.ToString(v[0]);
        }
    }
}
