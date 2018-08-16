using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KIUpdater
{
    class DCSMissionReader
    {

        public bool UpdateScriptLoadPath(string zipPath, string scriptLoadPath)
        {
            using (var file = File.OpenRead(zipPath))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name == "dictionary")
                    {
                        using (var stream = entry.Open())
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            while (!rdr.EndOfStream)
                            {
                                string line = rdr.ReadLine();
                                if (line.IndexOf("ki.lua", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                    line.IndexOf("loadfile", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    string newLine = ParseAndSet(line, scriptLoadPath);

                                }
                            }
                        }
                        break;
                    }
                    
                }
            }

            return true;
        }

        private string EscapeLoadFileString(string path)
        {
            return "loadfile(\\\"" + path.Replace("\\", "\\\\\\\\") + "\\\")";
        }

        private string EscapeParamString(string path)
        {
            return "))(\\\"" + path.Replace("\\", "\\\\\\\\") + "\\\\\\\\\\\")";
        }


        private string ParseAndSet(string line, string scriptLoadPath)
        {
            string newLine = line.Replace(Regex.Match(line, @"(loadfile).+?\)").Value, EscapeLoadFileString(scriptLoadPath + "\\KI.lua"));
            newLine = newLine.Replace(Regex.Match(newLine, @"\)\)\(.+?\)").Value, EscapeParamString(scriptLoadPath));
            return newLine;
        }
    }
}
