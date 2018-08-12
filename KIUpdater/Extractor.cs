using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    public class Extractor
    {
        public string DestinationPath { private set; get; }

        public Extractor(string path)
        {
            this.DestinationPath = path;
        }

        public bool Extract(string source)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(source, DestinationPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting file : " + ex.Message);
                return false;
            }
            
        }
    }
}
